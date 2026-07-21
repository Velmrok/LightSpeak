using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var isTesting = builder.Configuration.GetValue<bool>("Testing");
var sufix = isTesting ? "-test" : "";


var kcAdminUser = builder.AddParameter("kc-admin-user");
var kcAdminPassword = builder.AddParameter("kc-admin-password"/*, secret: true*/);
var kcGatewaySecret = builder.AddParameter("kc-gateway-secret"/*, secret: true*/);
var kcAdminSecret = builder.AddParameter("kc-admin-client-secret"/*, secret: true*/);
var appBaseUrl = builder.AddParameter("app-base-url");


var redis = builder.AddRedis("redis" + sufix,6379).WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder.AddKeycloak("keycloak" + sufix,8080,kcAdminUser,kcAdminPassword)
    .WithHttpEndpoint(name: "keycloak",port:8081, targetPort: 8080) 
    .WithImageTag("26.1.0")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("KC_HTTP_ENABLED", "true");

var gateway = builder.AddProject<Projects.Gateway>("gateway" + sufix)
    .WithReference(redis)
    .WithReference(keycloak)
    .WithEnvironment("ASPNETCORE_URLS", appBaseUrl)
    .WithEnvironment("AppBaseUrl", appBaseUrl)
    .WithEnvironment("OpenIDConnectSettings__Authority",$"{keycloak.GetEndpoint("keycloak")}/realms/lightspeak")
    .WithEnvironment("OpenIDConnectSettings__ClientSecret", kcGatewaySecret)
    .WithEnvironment("OpenIDConnectSettings__ClientId", "light-speak-gateway")
    .WithEnvironment("Services__keycloak__http", keycloak.GetEndpoint("keycloak"))
    .WaitFor(keycloak)
    .WaitFor(redis);

var kcConfig = builder.AddContainer("keycloak-config" + sufix , "adorsys/keycloak-config-cli", "6.5.1-26.1.0")
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
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(
           name: "http",
           targetPort: 8079,
           port: 8079)
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
