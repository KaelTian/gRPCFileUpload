using Grpc.Core;
using GrpcService1.Common;
using GrpcService1.Protos;

namespace GrpcService1.Services
{
    public class FileTransferService : FileTransfer.FileTransferBase
    {
        private readonly IUploadSessionManager _sessionManager;
        private readonly string _storagePath;

        public FileTransferService(IUploadSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
            _storagePath = SafePathHelper.GetUploadFolder();
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public override Task<InitUploadResponse> InitUpload(InitUploadRequest request, ServerCallContext context)
        {
            var sessionId = _sessionManager.CreateSession(request.FileName, request.FileSize, request.FileHash);
            var uploadedBytes = _sessionManager.GetUploadedBytes(sessionId);

            return Task.FromResult(new InitUploadResponse
            {
                SessionId = sessionId,
                UploadedBytes = uploadedBytes
            });
        }

        public override async Task<UploadChunkResponse> UploadChunk(IAsyncStreamReader<UploadChunkRequest> requestStream, ServerCallContext context)
        {
            UploadChunkRequest? currentRequest = null;
            var response = new UploadChunkResponse();

            while (await requestStream.MoveNext())
            {
                currentRequest = requestStream.Current;
                if (currentRequest == null)
                {
                    continue;
                }
                var session = _sessionManager.GetSession(currentRequest.SessionId);
                if (session == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Session not found"));
                }

                using var fileStream = new FileStream(session.TempFilePath!, FileMode.OpenOrCreate, FileAccess.Write);
                fileStream.Seek(currentRequest.Offset, SeekOrigin.Begin);
                await fileStream.WriteAsync(currentRequest.ChunkData.ToByteArray(), 0, currentRequest.ChunkData.Length);

                _sessionManager.UpdateSessionProgress(currentRequest.SessionId, currentRequest.Offset + currentRequest.ChunkData.Length);
                response.UploadedBytes = currentRequest.Offset + currentRequest.ChunkData.Length;

            }

            return response;
        }

        public override async Task<CompleteUploadResponse> CompleteUpload(CompleteUploadRequest request, ServerCallContext context)
        {
            var session = _sessionManager.GetSession(request.SessionId);
            if (session == null)
            {
                return await Task.FromResult(new CompleteUploadResponse
                {
                    Success = false,
                    Message = "Session not found"
                });
            }
            // Verify file hash if needed
            if (!string.IsNullOrWhiteSpace(request.FileHash) &&
                !session.IsCompleted &&
                !string.IsNullOrWhiteSpace(session.TempFilePath))
            {
                var actualHash = await FileHelper.GetQuickFileIdentity(session.TempFilePath!);
                if (actualHash != request.FileHash)
                {
                    return await Task.FromResult(new CompleteUploadResponse
                    {
                        Success = false,
                        Message = "File hash mismatch"
                    });
                }
                else if (!File.Exists(session.TempFilePath))
                {
                    return await Task.FromResult(new CompleteUploadResponse
                    {
                        Success = false,
                        Message = $"File not exist on: {session.TempFilePath}"
                    });
                }
            }
            else if (!session.IsCompleted && string.IsNullOrWhiteSpace(session.TempFilePath))
            {
                return await Task.FromResult(new CompleteUploadResponse
                {
                    Success = false,
                    Message = "Temporary file path is not set"
                });
            }
            else if (session.IsCompleted)
            {
                return await Task.FromResult(new CompleteUploadResponse
                {
                    Success = true,
                    Message = "File has been uploaded"
                });
            }
            // Move the file to the final storage location
            var finalFilePath = Path.Combine(_storagePath, session.FileName!);
            File.Move(session.TempFilePath!, finalFilePath, true);
            // Complete session
            _sessionManager.CompleteSession(request.SessionId);

            return await Task.FromResult(new CompleteUploadResponse
            {
                Success = true,
                Message = "File uploaded successfully"
            });
        }

        public override async Task<CheckUploadStatusResponse> CheckUploadStatus(CheckUploadStatusRequest request, ServerCallContext context)
        {
            var session = _sessionManager.FindSession(request.FileName, request.FileHash);

            return await Task.FromResult(new CheckUploadStatusResponse
            {
                Exists = session != null,
                SessionId = session?.SessionId ?? string.Empty,
                UploadedBytes = session?.UploadedBytes ?? 0
            });
        }
    }
}
