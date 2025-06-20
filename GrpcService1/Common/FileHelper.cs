using System.Security.Cryptography;

namespace GrpcService1.Common
{
    public class FileHelper
    {
        // 1MB的样本大小
        private const int sampleSize = 1 * 1024 * 1024;
        // 快速文件标识（前1MB哈希 + 文件大小）
        public static async Task<string> GetQuickFileIdentity(string filePath)
        {
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
