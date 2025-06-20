using GrpcService1.Common;
using GrpcService1.Services;

var builder = WebApplication.CreateBuilder(args);

// 或者直接检查Kestrel配置
var kestrelConfig = builder.Configuration.GetSection("Kestrel");
Console.WriteLine($"Kestrel Url: {kestrelConfig["Endpoints:Http:Url"]}");

// Add services to the container.
builder.Services.AddGrpc(o =>
{
    o.MaxSendMessageSize = 100 * 1024 * 1024; // 100 MB
    o.MaxReceiveMessageSize = 100 * 1024 * 1024; // 100 MB
});

var sessionManager = new SQLiteUploadSessionManager(
   //dbPath: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "UploadSessions.db"),
   dbPath: SafePathHelper.GetSqliteFilePath("UploadSessions.db"),
   tempStoragePath: SafePathHelper.GetUploadFolder("TempUploadFiles")
);

builder.Services.AddSingleton<IUploadSessionManager>(sessionManager);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FileTransferService>();
app.MapGrpcService<HealthCheckImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
