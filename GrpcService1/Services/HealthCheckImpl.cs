using Grpc.Core;
using GrpcService1.Protos;

namespace GrpcService1.Services
{
    public class HealthCheckImpl : HealthCheck.HealthCheckBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse
            {
                Message = "pong",
                ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
    }
}
