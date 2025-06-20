namespace GrpcService1.Common
{
    public class UploadSession
    {
        public string? SessionId { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? FileHash { get; set; }
        public long UploadedBytes { get; set; }
        public string? TempFilePath { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
