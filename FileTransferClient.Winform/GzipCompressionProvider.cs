using Grpc.Net.Compression;
using System.IO.Compression;

namespace FileTransferClient.Winform
{
    public class GzipCompressionProvider : ICompressionProvider
    {
        public string EncodingName => "gzip";  // 重要，必须是标准的 "gzip"

        public Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)
        {
            return new GZipStream(stream, compressionLevel ?? CompressionLevel.Fastest, leaveOpen: true);
        }

        public Stream CreateDecompressionStream(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}
