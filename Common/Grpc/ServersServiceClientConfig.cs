using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Protos;
namespace Common.Grpc;

public static class ServersServiceClientConfig
{
    public static void AddAndConfigureServersServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<ServersService.ServersServiceClient>(o =>
{
    o.Address = new Uri("http://_grpc.servers-service");
}).AddServiceDiscovery()
.ConfigureGrpcCredentials();
    }
}