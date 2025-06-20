namespace FileTransferClient.Winform.Configs
{
    public class ApplicationSettings
    {
        public string Title { get; set; } = "gRPC 客户端应用程序";
        public string Theme { get; set; } = "Light";
    }

    public class GrpcServiceSettings
    {
        public string ServerUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMilliseconds { get; set; } = 1000;
        public bool EnableCompression { get; set; } = true;
    }

    public class GrpcGlobalSettings
    {
        public int KeepAlivePingDelay { get; set; } = 60;
        public int KeepAlivePingTimeout { get; set; } = 5;
        public int? MaxReceiveMessageSize { get; set; } //= 4 * 1024 * 1024; // 4MB
        public int? MaxSendMessageSize { get; set; } //= 4 * 1024 * 1024; // 4MB
    }

    public class GrpcSettings
    {
        public Dictionary<string, GrpcServiceSettings> Services { get; set; } = new();
        public GrpcGlobalSettings GlobalSettings { get; set; } = new();
    }

    public class AppConfig
    {
        public ApplicationSettings Application { get; set; } = new();
        public GrpcSettings GrpcSettings { get; set; } = new();
    }
}
