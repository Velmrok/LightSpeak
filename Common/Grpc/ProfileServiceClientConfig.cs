using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Protos;
namespace Common.Grpc;

public static class ProfileServiceClientConfig
{
    public static void AddAndConfigureProfileServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpcClient<ProfileService.ProfileServiceClient>(o =>
{
    o.Address = new Uri(configuration["Grpc:ProfileService:Address"]!);
}).AddServiceDiscovery()
.ConfigureGrpcCredentials();
    }
}