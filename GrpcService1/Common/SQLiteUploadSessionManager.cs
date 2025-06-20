
using Microsoft.Data.Sqlite;

namespace GrpcService1.Common
{
    public class SQLiteUploadSessionManager : IUploadSessionManager
    {
        private readonly string _connectionString;
        private readonly string _tempStoragePath;

        public SQLiteUploadSessionManager(string dbPath, string tempStoragePath)
        {
            _connectionString = $"Data Source={dbPath}";
            if (!Directory.Exists(tempStoragePath))
            {
                Directory.CreateDirectory(tempStoragePath);
            }
            _tempStoragePath = tempStoragePath;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UploadSessions (
                    SessionId TEXT PRIMARY KEY,
                    FileName TEXT NOT NULL,
                    FileSize INTEGER NOT NULL,
                    FileHash TEXT NOT NULL,
                    UploadedBytes INTEGER DEFAULT 0,
                    TempFilePath TEXT,
                    CreatedAt TEXT NOT NULL,
                    CompletedAt TEXT,
                    IsCompleted  INTEGER NOT NULL DEFAULT 0
                );";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库初始化失败: {ex.Message}");
                throw;
            }
        }


        public void AbortSession(string sessionId)
        {
            var session = GetSession(sessionId);
            if (session != null)
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM UploadSessions WHERE SessionId = @SessionId";
                command.Parameters.AddWithValue("@SessionId", sessionId);
                command.ExecuteNonQuery();
                // Optionally, delete the temporary file
                if (File.Exists(session.TempFilePath))
                {
                    File.Delete(session.TempFilePath);
                }
            }
        }
        public void CleanupExpiredSession(TimeSpan expirationTime)
        {
            var cutoffTime = DateTime.UtcNow - expirationTime;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // First get all exoired sessions
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = @"
                SELECT SessionId, TempFilePath 
                FROM UploadSessions 
                WHERE CreatedAt < @CutoffTime AND (CompletedAt IS NULL OR CompletedAt < @CutoffTime);";
            selectCommand.Parameters.AddWithValue("@CutoffTime", cutoffTime.ToString("o"));

            var sessionsToDelete = new List<(string SessionId, string TempFilePath)>();

            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var sessionId = reader.GetString(0);
                    var tempFilePath = reader.GetString(1);
                    sessionsToDelete.Add((sessionId, tempFilePath));
                }
            }

            // Delete expired sessions and their temporary files
            foreach (var (sessionId, tempFilePath) in sessionsToDelete)
            {
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM UploadSessions WHERE SessionId = @SessionId";
                deleteCommand.Parameters.AddWithValue("@SessionId", sessionId);
                deleteCommand.ExecuteNonQuery();
                // Optionally, delete the temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        public void CompleteSession(string sessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE UploadSessions 
                SET CompletedAt = @CompletedAt,
                IsCompleted = @IsCompleted
                WHERE SessionId = @SessionId;";

            command.Parameters.AddWithValue("@SessionId", sessionId);
            command.Parameters.AddWithValue("@CompletedAt", DateTime.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("@IsCompleted", 1);

            command.ExecuteNonQuery();
        }

        public string CreateSession(string fileName, long fileSize, string fileHash)
        {
            var sessionId = Guid.NewGuid().ToString();
            var tempFilePath = Path.Combine(_tempStoragePath, $"temp_{sessionId}_{Path.GetFileName(fileName)}");
            var createdAt = DateTime.UtcNow;

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO UploadSessions 
                (SessionId, FileName, FileSize, FileHash, TempFilePath, CreatedAt,IsCompleted)
                VALUES 
                (@SessionId, @FileName, @FileSize, @FileHash, @TempFilePath, @CreatedAt,@IsCompleted);";
            command.Parameters.AddWithValue("@SessionId", sessionId);
            command.Parameters.AddWithValue("@FileName", fileName);
            command.Parameters.AddWithValue("@FileSize", fileSize);
            command.Parameters.AddWithValue("@FileHash", fileHash);
            command.Parameters.AddWithValue("@TempFilePath", tempFilePath);
            command.Parameters.AddWithValue("@CreatedAt", createdAt.ToString("o"));
            command.Parameters.AddWithValue("@IsCompleted", 0);
            command.ExecuteNonQuery();
            return sessionId;
        }

        public UploadSession? FindSession(string fileName, string fileHash)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM UploadSessions 
                WHERE FileName = @FileName AND FileHash = @FileHash
                ORDER BY CreatedAt DESC
                LIMIT 1;";

            command.Parameters.AddWithValue("@FileName", fileName);
            command.Parameters.AddWithValue("@FileHash", fileHash);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UploadSession
                {
                    SessionId = reader.GetString(0),
                    FileName = reader.GetString(1),
                    FileSize = reader.GetInt64(2),
                    FileHash = reader.GetString(3),
                    UploadedBytes = reader.GetInt64(4),
                    TempFilePath = reader.GetString(5),
                    CreateAt = DateTime.Parse(reader.GetString(6)),
                    CompletedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                    IsCompleted = reader.GetBoolean(8)
                };
            }
            return null;
        }

        public UploadSession? GetSession(string sessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM UploadSessions WHERE SessionId = @SessionId";
            command.Parameters.AddWithValue("@SessionId", sessionId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UploadSession
                {
                    SessionId = reader.GetString(0),
                    FileName = reader.GetString(1),
                    FileSize = reader.GetInt64(2),
                    FileHash = reader.GetString(3),
                    UploadedBytes = reader.GetInt64(4),
                    TempFilePath = reader.GetString(5),
                    CreateAt = DateTime.Parse(reader.GetString(6)),
                    CompletedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                    IsCompleted = reader.GetBoolean(8)
                };
            }
            return null; // or throw an exception if session not found
        }

        public long GetUploadedBytes(string sessionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UploadedBytes FROM UploadSessions WHERE SessionId = @SessionId";
            command.Parameters.AddWithValue("@SessionId", sessionId);

            var result = command.ExecuteScalar();
            if (result != null && result is long uploadedBytes)
            {
                return uploadedBytes;
            }
            return 0;
        }

        public void UpdateSessionProgress(string sessionId, long uploadedBytes)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE UploadSessions 
                SET UploadedBytes = @UploadedBytes
                WHERE SessionId = @SessionId;";
            command.Parameters.AddWithValue("@SessionId", sessionId);
            command.Parameters.AddWithValue("@UploadedBytes", uploadedBytes);
            command.ExecuteNonQuery();
        }
    }
}
