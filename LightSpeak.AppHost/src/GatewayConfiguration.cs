
namespace LightSpeak.AppHost.src;

public static class GatewayConfiguration
{
    public static void ConfigureGateway
    (this IResourceBuilder<ProjectResource> gateway, AppParameters p, AppSettings s,AppResources a)
    {
      
        gateway
            .WithReference(a.Redis)
            .WithReference(a.Keycloak)
            .WithReference(a.ProfileService)
            .WithReference(a.ComposeService)
            .WithReference(a.RabbitMQ)
            .WithReference(a.ServersService)
            .WithEnvironment("AppBaseUrl", p.GatewayUrl)
            .WithEnvironment("OpenIDConnectSettings__Authority", p.ClientAuthority)
            .WithEnvironment("OpenIDConnectSettings__ClientSecret", p.KcGatewaySecret)
            .WithEnvironment("OpenIDConnectSettings__ClientId", "light-speak-gateway")
            .WithEnvironment("Services__keycloak__http", a.Keycloak.GetEndpoint("keycloak"))
            .WithEnvironment("Production", "false")
            .WaitFor(a.Keycloak)
            .WaitFor(a.Redis)
            .WaitFor(a.RabbitMQ);
        if(!s.IsTesting) gateway.WithEnvironment("ASPNETCORE_URLS", p.GatewayUrl);
    }
}