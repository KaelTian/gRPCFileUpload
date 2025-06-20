namespace GrpcService1.Common
{
    public static class SafePathHelper
    {
        // ① Base directory resolution ────────────────────────────────────────────
        private static string GetBasePath()
        {
            // Priority 1: env var or app‑setting override
            var overridePath = Environment.GetEnvironmentVariable("MYAPP_DATA_DIR");
            if (!string.IsNullOrWhiteSpace(overridePath))
                return EnsureFolder(overridePath);

            // Priority 2: decide by execution context
            bool isService = !Environment.UserInteractive;   // Rough but works
            var folder = isService
                ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)  // ProgramData
                : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);  // LocalAppData
            return EnsureFolder(Path.Combine(folder, "MyGrpcUploader"));
        }

        // ② Public API ────────────────────────────────────────────────────────────
        /// <summary>Returns the full path to the SQLite file that stores run logs.</summary>
        public static string GetSqliteFilePath(string dbFileName = "myfile.db")
        {
            string path = Path.Combine(GetBasePath(), dbFileName);
            Console.WriteLine($"SQLite file path: {path}");
            return path;
        }

        /// <summary>Returns (and creates) the folder used to store server‑uploaded files.</summary>
        public static string GetUploadFolder(string subFolderName = "Uploads")
            => EnsureFolder(Path.Combine(GetBasePath(), subFolderName));

        // ③ Utility ──────────────────────────────────────────────────────────────
        private static string EnsureFolder(string path)
        {
            Directory.CreateDirectory(path);  // idempotent; creates if missing
            return path;
        }
    }
}
