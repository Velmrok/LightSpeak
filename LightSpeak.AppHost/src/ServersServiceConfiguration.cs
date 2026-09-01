namespace LightSpeak.AppHost.src;

public static class ServersServiceConfiguration
{
    public static void ConfigureServersService
    (this IResourceBuilder<ProjectResource> serversService, AppParameters p, AppSettings s,AppResources a)
    {
        serversService
            // .WithHttpEndpoint(name: "http")
            // .WithHttpEndpoint(name: "grpc")
            .WithReference(a.RabbitMQ)
            .WithReference(a.ServersDatabase)
            .WithEnvironment("AuthSettings__Authority", p.ClientAuthority)
            .WithEnvironment("AuthSettings__Audience", p.ClientAudience)
            .WaitFor(a.PostgresServer)
            .WaitFor(a.ServersDatabase);
    }
}