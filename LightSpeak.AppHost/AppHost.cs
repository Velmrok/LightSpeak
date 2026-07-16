using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var kcAdminUser = builder.AddParameter("kc-admin-user", secret: true);
var kcAdminPassword = builder.AddParameter("kc-admin-password", secret: true);
var kcGatewaySecret = builder.AddParameter("kc-gateway-secret", secret: true);
var appBaseUrl = builder.AddParameter("app-base-url");


var redis = builder.AddRedis("redis").WithLifetime(ContainerLifetime.Persistent);
var keycloak = builder.AddKeycloak("keycloak",8080,kcAdminUser,kcAdminPassword).WithLifetime(ContainerLifetime.Persistent);

var gateway = builder.AddProject<Projects.Gateway>("gateway")
    .WithReference(redis)
    .WithReference(keycloak)
    .WithEnvironment("OpenIDConnectSettings__Authority",$"{keycloak.GetEndpoint("http")}/realms/lightspeak")
    .WithEnvironment("OpenIDConnectSettings__ClientSecret", kcGatewaySecret)
    .WaitFor(keycloak)
    .WaitFor(redis);

var kcConfig = builder.AddContainer("keycloak-config", "adorsys/keycloak-config-cli", "6.4.0-26.6.0")
    .WithBindMount("../keycloak", "/config", isReadOnly: true)
    .WithEnvironment("KEYCLOAK_URL", keycloak.GetEndpoint("http"))
    .WithEnvironment("KEYCLOAK_USER", kcAdminUser)
    .WithEnvironment("KEYCLOAK_PASSWORD", kcAdminPassword)
    .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_ENABLED", "true")
    .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_TIMEOUT", "120s")
    .WithEnvironment("IMPORT_FILES_LOCATIONS", "/config/*.yaml")
    .WithEnvironment("IMPORT_VARSUBSTITUTION_ENABLED", "true")
    .WithEnvironment("APP_BASE_URL", appBaseUrl)
    .WithEnvironment("KC_GATEWAY_SECRET", kcGatewaySecret)
    .WaitFor(keycloak);

builder.Build().Run();
