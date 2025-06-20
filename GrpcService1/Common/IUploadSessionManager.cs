namespace GrpcService1.Common
{
    public interface IUploadSessionManager
    {
        string CreateSession(string fileName,long fileSize, string fileHash);
        UploadSession? GetSession(string sessionId);
        UploadSession? FindSession(string fileName, string fileHash);
        void UpdateSessionProgress(string sessionId, long uploadedBytes);
        long GetUploadedBytes(string sessionId);
        void CompleteSession(string sessionId);
        void AbortSession(string sessionId);
        void CleanupExpiredSession(TimeSpan expirationTime);
    }
}
