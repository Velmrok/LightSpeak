namespace LightSpeak.AppHost.src;

public sealed class AppResources
{
    public IResourceBuilder<KeycloakResource> Keycloak { get; init; }
    public IResourceBuilder<ContainerResource> KeycloakConfig { get; init; }
    public IResourceBuilder<RedisResource> Redis { get; init; }
    public IResourceBuilder<PostgresServerResource> PostgresServer { get; init; }
    public IResourceBuilder<ProjectResource> ProfileService { get; init; }
    public IResourceBuilder<ProjectResource> Gateway { get; init; }
    public IResourceBuilder<PostgresDatabaseResource> ProfileDatabase { get; init; }
    public IResourceBuilder<ProjectResource> ComposeService { get; init; }

}