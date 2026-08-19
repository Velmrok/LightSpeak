
namespace LightSpeak.AppHost.src;

public static class DebugServiceConfiguration
{
    public static void AddAndConfigureDebugService
    (this IDistributedApplicationBuilder builder, AppParameters p, AppSettings s, AppResources a)
    {
        var debugService = builder
            .AddProject<Projects.Debug>("debug")
            .WithReference(a.ProfileService)
            .WithEnvironment("AuthSettings__Authority", p.ClientAuthority)
            .WithEnvironment("AuthSettings__Audience", p.ClientAudience)
            .WithEnvironment("Grpc__ProfileService__Address", $"http://{a.ProfileService.Resource.Name}");
        a.Gateway.WithReference(debugService);
        a.Gateway.WithEnvironment("ReverseProxy__Routes__debug-route__ClusterId", "debug");
        a.Gateway.WithEnvironment("ReverseProxy__Routes__debug-route__AuthorizationPolicy", "anonymous");
        a.Gateway.WithEnvironment("ReverseProxy__Routes__debug-route__Match__Path", "/debug/{**catch-all}");
        a.Gateway.WithEnvironment("ReverseProxy__Routes__debug-route__Transforms__0__PathRemovePrefix", "/debug");
        a.Gateway.WithEnvironment("ReverseProxy__Clusters__debug__Destinations__d1__Address", debugService.GetEndpoint("http"));
    }
}