
namespace LightSpeak.AppHost.src;

public static class ProfileServiceConfiguration
{
    public static void ConfigureProfileSevice
    (this IResourceBuilder<ProjectResource> profileService, AppParameters p, AppSettings s,AppResources a)
    {
        profileService
            .WithHttpEndpoint(name: "http")
            .WithHttpEndpoint(name: "grpc")
            .WithReference(a.RabbitMQ)
            .WithReference(a.ProfileDatabase)
            .WithEnvironment("AuthSettings__Authority", p.ClientAuthority)
            .WithEnvironment("AuthSettings__Audience", p.ClientAudience)
            .WaitFor(a.PostgresServer)
            .WaitFor(a.ProfileDatabase);
    }
}