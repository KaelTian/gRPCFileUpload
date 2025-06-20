using System.Security.Cryptography;

namespace FileTransferClient.Console
{
    public static class FileHelper
    {
        // 最佳缓冲区大小经过测试得出（根据硬件调整）
        private const int OptimalBufferSize = 1 * 1024 * 1024; // 1MB缓冲区
        public static string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        //public static async Task<string> CalculateFileHashAsync(string filePath)
        //{
        //    using (var md5 = MD5.Create())
        //    using (var stream = File.OpenRead(filePath))
        //    {
        //        var hash = await md5.ComputeHashAsync(stream);
        //        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        //    }
        //}
        public static async Task<string> CalculateFileHashAsync(string filePath, int bufferSize = 81920 /* 默认缓冲区大小 */, CancellationToken cancellationToken = default)
        {
            using (var md5 = MD5.Create())
            {
                // 使用带有异步选项和优化缓冲区大小的FileStream
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: bufferSize,
                    options: FileOptions.SequentialScan | FileOptions.Asynchronous))
                {
                    var hash = await md5.ComputeHashAsync(stream, cancellationToken)
                                       .ConfigureAwait(false);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        // 快速文件标识（前1MB哈希 + 文件大小）
        public static async Task<string> GetQuickFileIdentity(string filePath)
        {
            const int sampleSize = 1 * 1024 * 1024; // 1MB
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[Math.Min(sampleSize, fileSize)];
                int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);

                using (var sha = SHA256.Create())
                {
                    byte[] hashBytes = sha.ComputeHash(buffer);
                    return $"{fileSize}-{BitConverter.ToString(hashBytes).Replace("-", "")}";
                }
            }
        }

    }
}
