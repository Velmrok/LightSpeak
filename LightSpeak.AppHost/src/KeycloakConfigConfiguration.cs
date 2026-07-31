
namespace LightSpeak.AppHost.src;

public static class KeycloakConfigConfiguration
{
    public static void ConfigureKeycloakConfig
    (this IResourceBuilder<ContainerResource> kcConfig, AppParameters p, AppSettings s, AppResources a)
    {
        kcConfig
            .WithBindMount("../keycloak", "/config", isReadOnly: true)
            .WithEnvironment("KEYCLOAK_URL", ReferenceExpression.Create($"{a.Keycloak.GetEndpoint("keycloak")}/auth"))
            .WithEnvironment("KEYCLOAK_USER", p.KcAdminUser)
            .WithEnvironment("KEYCLOAK_PASSWORD", p.KcAdminPassword)
            .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_ENABLED", "true")
            .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_TIMEOUT", "120s")
            .WithEnvironment("IMPORT_FILES_LOCATIONS", "/config/*.yaml")
            .WithEnvironment("IMPORT_VARSUBSTITUTION_ENABLED", "true")
            .WithEnvironment("KC_GATEWAY_SECRET", p.KcGatewaySecret)
            .WithEnvironment("KC_ADMIN_CLIENT_SECRET", p.KcAdminSecret)
            .WithEnvironment("CLIENT_AUDIENCE", p.ClientAudience)
            .WithEnvironment("KC_TESTUSER_ENABLED", s.IsTesting ? "true" : "false")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithHttpEndpoint(
                name: "http",
                targetPort: 8079,
                port: 8079)
            .WaitFor(a.Keycloak);
        if (s.IsTesting)
        {
            kcConfig.WithEnvironment("APP_BASE_URL", "*");
        }
        else
        {
            kcConfig.WithEnvironment("APP_BASE_URL", p.AppBaseUrl);
        }
            
    }
}