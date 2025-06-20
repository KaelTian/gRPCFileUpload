using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Compression;

namespace FileTransferClient.Winform
{
    public static class GrpcClientFactory
    {
        public static TClient? CreateClient<TClient>(string serviceName) where TClient : ClientBase<TClient>
        {
            var settings = ConfigurationHelper.GetGrpcServiceSettings(serviceName);
            var globalSettings = ConfigurationHelper.GetGrpcGlobalSettings();

            var methodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = settings!.MaxRetryAttempts,
                    InitialBackoff = TimeSpan.FromMilliseconds(settings.RetryDelayMilliseconds),
                    MaxBackoff = TimeSpan.FromMilliseconds(settings.RetryDelayMilliseconds * 2),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };

            var channelOptions = new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler
                {
                    KeepAlivePingDelay = TimeSpan.FromSeconds(globalSettings!.KeepAlivePingDelay),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(globalSettings.KeepAlivePingTimeout),
                    EnableMultipleHttp2Connections = true,
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                },
                MaxReceiveMessageSize = globalSettings.MaxReceiveMessageSize,
                MaxSendMessageSize = globalSettings.MaxSendMessageSize,
                CompressionProviders = settings.EnableCompression
                    ? new List<ICompressionProvider> { new GzipCompressionProvider() }
                    : null,
                ServiceConfig = new ServiceConfig { MethodConfigs = { methodConfig } }
            };

            var channel = GrpcChannel.ForAddress(settings.ServerUrl, channelOptions);

            return (TClient?)Activator.CreateInstance(typeof(TClient), channel);
        }
    }
}
