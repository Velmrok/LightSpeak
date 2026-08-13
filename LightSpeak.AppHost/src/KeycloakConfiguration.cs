
namespace LightSpeak.AppHost.src;

public static class KeycloakConfiguration
{
    public static void ConfigureKeycloak
    (this IResourceBuilder<KeycloakResource> keycloak, AppParameters p, AppSettings s, AppResources a)
    {
      
        keycloak
            .WithHttpEndpoint(name: "keycloak", port: 8081, targetPort: 8080)
            .WithDockerfile("../keycloak")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithEnvironment("KC_HTTP_RELATIVE_PATH", "/auth")
            .WithEnvironment("KC_HTTP_MANAGEMENT_RELATIVE_PATH", "/")
            .WithEnvironment("KC_HTTP_ENABLED", "true")
            .WithEnvironment("KC_HOSTNAME_STRICT", s.IsTesting ? "false" : "true")
            .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
            .WithEnvironment("KK_TO_RMQ_URL", a.RabbitMQ.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
            .WithEnvironment("KK_TO_RMQ_PORT", a.RabbitMQ.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
            .WithEnvironment("KK_TO_RMQ_USERNAME", p.RabbitUser)
            .WithEnvironment("KK_TO_RMQ_PASSWORD", p.RabbitPassword)
            .WithEnvironment("KK_TO_RMQ_VHOST", "/"); //KK.EVENT.CLIENT.lightspeak.SUCCESS.light-speak-gateway.REGISTER
        if (!s.IsTesting) keycloak.WithEnvironment("KC_HOSTNAME", ReferenceExpression.Create($"{p.AppBaseUrl}/auth"));
    }
}