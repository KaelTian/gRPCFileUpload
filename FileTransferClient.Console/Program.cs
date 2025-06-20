using FileTransferClient.Console;
using Grpc.Net.Client;
using GrpcService1.Protos;

GrpcChannel _channel;
FileTransfer.FileTransferClient _client;
CancellationTokenSource? _cancellationTokenSource;

try
{
    _channel = GrpcChannel.ForAddress("http://localhost:5173");
    _client = new FileTransfer.FileTransferClient(_channel);

    _cancellationTokenSource = new CancellationTokenSource();
    string filePath = @"D:\softwares\draw.io-24.5.3-windows-installer.exe"; // 替换为实际的文件路径
    await UploadFileWithResume(filePath, _cancellationTokenSource.Token);
    Console.WriteLine("文件成功上传");
}
catch (Exception ex)
{
    Console.WriteLine($"gRPC客户端执行失败: {ex}");
}
finally
{
    Console.WriteLine("按任意键退出...");
    Console.Read();
}

async Task UploadFileWithResume(string filePath, CancellationToken cancellationToken)
{
    var fileInfo = new FileInfo(filePath);
    var fileHash = await FileHelper.GetQuickFileIdentity(filePath);
    const int chunkSize = 10 * 1024 * 1024; // 1MB chunk size

    // 检查服务器上是否已有上传记录
    var statusResponse = await _client.CheckUploadStatusAsync(new CheckUploadStatusRequest
    {
        FileName = fileInfo.Name,
        FileHash = fileHash
    });

    string sessionId;
    long uploadedBytes = 0;

    if (statusResponse.Exists && statusResponse.UploadedBytes > 0)
    {
        // 恢复已有上传
        sessionId = statusResponse.SessionId;
        uploadedBytes = statusResponse.UploadedBytes;
    }
    else
    {
        // 开始新上传
        var initResponse = await _client.InitUploadAsync(new InitUploadRequest
        {
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            FileHash = fileHash
        });
        sessionId = initResponse.SessionId;
        uploadedBytes = initResponse.UploadedBytes;
    }

    // 更新UI显示初始进度
    UpdateProgress(uploadedBytes, fileInfo.Length);

    // 打开文件准备读取
    using (var fileStream = File.OpenRead(filePath))
    {
        fileStream.Seek(uploadedBytes, SeekOrigin.Begin);

        var call = _client.UploadChunk();
        var remainingBytes = fileInfo.Length - uploadedBytes;

        while (remainingBytes > 0 && !cancellationToken.IsCancellationRequested)
        {
            var currentChunkSize = (int)Math.Min(chunkSize, remainingBytes);
            var buffer = new byte[currentChunkSize];
            var bytesRead = await fileStream.ReadAsync(buffer, 0, currentChunkSize, cancellationToken);

            if (bytesRead == 0) break;

            await call.RequestStream.WriteAsync(new UploadChunkRequest
            {
                SessionId = sessionId,
                ChunkData = Google.Protobuf.ByteString.CopyFrom(buffer),
                Offset = uploadedBytes
            });

            uploadedBytes += bytesRead;
            remainingBytes -= bytesRead;

            // 更新UI进度
            UpdateProgress(uploadedBytes, fileInfo.Length);
        }

        await call.RequestStream.CompleteAsync();
        var response = await call.ResponseAsync;
    }

    if (!cancellationToken.IsCancellationRequested)
    {
        // 完成上传
        var completeResponse = await _client.CompleteUploadAsync(new CompleteUploadRequest
        {
            SessionId = sessionId,
            FileHash = fileHash
        });

        if (!completeResponse.Success)
        {
            throw new Exception(completeResponse.Message);
        }
    }
}

void UpdateProgress(long uploadedBytes, long totalBytes)
{


    var progressPercentage = (int)((double)uploadedBytes / totalBytes * 100);
    Console.WriteLine($"当前进度: {progressPercentage}%");
}

//string FormatFileSize(long bytes)
//{
//    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
//    int order = 0;
//    double len = bytes;

//    while (len >= 1024 && order < sizes.Length - 1)
//    {
//        order++;
//        len /= 1024;
//    }

//    return $"{len:0.##} {sizes[order]}";
//}