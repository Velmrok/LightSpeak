using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var isTesting = builder.Configuration.GetValue<bool>("Testing");
var isDev = builder.Configuration.GetValue<bool>("DevMode");


var kcAdminUser = builder.AddParameter("kc-admin-user");
var kcAdminPassword = builder.AddParameter("kc-admin-password"/*, secret: true*/);
var kcGatewaySecret = builder.AddParameter("kc-gateway-secret"/*, secret: true*/);
var kcAdminSecret = builder.AddParameter("kc-admin-client-secret"/*, secret: true*/);
var appBaseUrl = builder.AddParameter("app-base-url");
var clientAudience = builder.AddParameter("client-audience");



var redis = builder.AddRedis("redis").WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder.AddKeycloak("keycloak", 8080, kcAdminUser, kcAdminPassword)
    .WithHttpEndpoint(name: "keycloak", port: 8081, targetPort: 8080)
    .WithImageTag("26.1.0")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("KC_HEALTH_ENABLED", "true")
    .WithEnvironment("KC_HTTP_RELATIVE_PATH", "/auth")
    .WithEnvironment("KC_HTTP_MANAGEMENT_RELATIVE_PATH", "/")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", isTesting ? "false" : "true")
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded");

if(!isTesting) keycloak.WithEnvironment("KC_HOSTNAME", ReferenceExpression.Create($"{appBaseUrl}/auth"));

var gateway = builder.AddProject<Projects.Gateway>("gateway");
    
var gatewayUrl = isTesting
    ? ReferenceExpression.Create($"{gateway.GetEndpoint("http")}")
    : ReferenceExpression.Create($"{appBaseUrl}");
if(!isTesting) gateway.WithEnvironment("ASPNETCORE_URLS", gatewayUrl);

var clientAuthority = ReferenceExpression.Create($"{gatewayUrl}/auth/realms/lightspeak");

gateway
    .WithReference(redis)
    .WithReference(keycloak)
    .WithEnvironment("AppBaseUrl", gatewayUrl)
    .WithEnvironment("OpenIDConnectSettings__Authority",clientAuthority)
    .WithEnvironment("OpenIDConnectSettings__ClientSecret", kcGatewaySecret)
    .WithEnvironment("OpenIDConnectSettings__ClientId", "light-speak-gateway")
    .WithEnvironment("Services__keycloak__http", keycloak.GetEndpoint("keycloak"))
    .WithEnvironment("Production", "false")
    .WaitFor(keycloak)
    .WaitFor(redis);


var kcConfig = builder.AddContainer("keycloak-config" , "adorsys/keycloak-config-cli", "6.5.1-26.1.0")
    .WithBindMount("../keycloak", "/config", isReadOnly: true)
    .WithEnvironment("KEYCLOAK_URL", ReferenceExpression.Create($"{keycloak.GetEndpoint("keycloak")}/auth"))
    .WithEnvironment("KEYCLOAK_USER", kcAdminUser)
    .WithEnvironment("KEYCLOAK_PASSWORD", kcAdminPassword)
    .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_ENABLED", "true")
    .WithEnvironment("KEYCLOAK_AVAILABILITYCHECK_TIMEOUT", "120s")
    .WithEnvironment("IMPORT_FILES_LOCATIONS", "/config/*.yaml")
    .WithEnvironment("IMPORT_VARSUBSTITUTION_ENABLED", "true")
    .WithEnvironment("KC_GATEWAY_SECRET", kcGatewaySecret)
    .WithEnvironment("KC_ADMIN_CLIENT_SECRET", kcAdminSecret)
    .WithEnvironment("CLIENT_AUDIENCE", clientAudience)
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

if(isDev || isTesting){
    var debugService = builder.AddProject<Projects.Debug>("debug")
        .WithEnvironment("AuthSettings__Authority", clientAuthority)
        .WithEnvironment("AuthSettings__Audience", clientAudience);
    gateway.WithReference(debugService);
    gateway.WithEnvironment("ReverseProxy__Routes__debug-route__ClusterId", "debug");
    gateway.WithEnvironment("ReverseProxy__Routes__debug-route__AuthorizationPolicy", "anonymous");
    gateway.WithEnvironment("ReverseProxy__Routes__debug-route__Match__Path", "/debug/{**catch-all}");
    gateway.WithEnvironment("ReverseProxy__Routes__debug-route__Transforms__0__PathRemovePrefix","/debug");
    gateway.WithEnvironment("ReverseProxy__Clusters__debug__Destinations__d1__Address", debugService.GetEndpoint("http"));
}

gateway.WaitForCompletion(kcConfig);
builder.Build().Run();
