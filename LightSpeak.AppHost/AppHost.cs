using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var isTesting = builder.Configuration.GetValue<bool>("Testing");
var lifetime = isTesting ? ContainerLifetime.Session : ContainerLifetime.Persistent;


var kcAdminUser = builder.AddParameter("kc-admin-user");
var kcAdminPassword = builder.AddParameter("kc-admin-password"/*, secret: true*/);
var kcGatewaySecret = builder.AddParameter("kc-gateway-secret"/*, secret: true*/);
var kcAdminSecret = builder.AddParameter("kc-admin-client-secret"/*, secret: true*/);
var appBaseUrl = builder.AddParameter("app-base-url");


var redis = builder.AddRedis("redis").WithLifetime(lifetime);

var keycloak = builder.AddKeycloak("keycloak",null,kcAdminUser,kcAdminPassword)
    .WithHttpEndpoint(name: "keycloak", targetPort: 8080) 
    .WithImageTag("26.1.0")
    .WithLifetime(lifetime)
    .WithEnvironment("KC_HTTP_ENABLED", "true");

var gateway = builder.AddProject<Projects.Gateway>("gateway")
    .WithReference(redis)
    .WithReference(keycloak)
    .WithEnvironment("ASPNETCORE_URLS", appBaseUrl)
    .WithEnvironment("AppBaseUrl", appBaseUrl)
    .WithEnvironment("OpenIDConnectSettings__Authority",$"{keycloak.GetEndpoint("keycloak")}/realms/lightspeak")
    .WithEnvironment("OpenIDConnectSettings__ClientSecret", kcGatewaySecret)
    .WithEnvironment("OpenIDConnectSettings__ClientId", "light-speak-gateway")
    .WaitFor(keycloak)
    .WaitFor(redis);

var kcConfig = builder.AddContainer("keycloak-config", "adorsys/keycloak-config-cli", "6.5.1-26.1.0")
    .WithBindMount("../keycloak", "/config", isReadOnly: true)
    .WithEnvironment("KEYCLOAK_URL", keycloak.GetEndpoint("keycloak"))
    .WithEnvironment("KEYCLOAK_USER", kcAdminUser)
    .WithEnvironment("KEYCLOAK_PASSWORD", kcAdminPassword)
    .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_ENABLED", "true")
    .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_TIMEOUT", "120s")
    .WithEnvironment("IMPORT_FILES_LOCATIONS", "/config/*.yaml")
    .WithEnvironment("IMPORT_VARSUBSTITUTION_ENABLED", "true")
    .WithEnvironment("KC_GATEWAY_SECRET", kcGatewaySecret)
    .WithEnvironment("KC_ADMIN_CLIENT_SECRET", kcAdminSecret)
    .WithEnvironment("KC_TESTUSER_ENABLED", isTesting ? "true" : "false")
    .WaitFor(keycloak);
if (isTesting)
{
    kcConfig.WithEnvironment("APP_BASE_URL", "*");
}
else
{
    kcConfig.WithEnvironment("APP_BASE_URL", appBaseUrl);
}
gateway.WaitForCompletion(kcConfig);

builder.Build().Run();
