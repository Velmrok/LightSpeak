using Aspire.Hosting;
using LightSpeak.AppHost.src;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var parameters = builder.AddApplicationParameters();
var settings = builder.AddApplicationSettings();

//////////////////////////////////////////// DECLARATIONS ////////////////////////////////////////////
var redis = builder.AddRedis("redis").WithLifetime(ContainerLifetime.Persistent);
var postgres = builder.AddPostgres("postgres").WithLifetime(ContainerLifetime.Persistent);
var gateway = builder.AddProject<Projects.Gateway>("gateway");
var keycloak = builder.AddKeycloak("keycloak", 8080, parameters.KcAdminUser, parameters.KcAdminPassword);
var profileDatabase = postgres.AddDatabase("profile-database","profile-database");
var profileService = builder.AddProject<Projects.ProfileService>("profile-service");
var kcConfig = builder.AddContainer("keycloak-config" , "adorsys/keycloak-config-cli", "6.5.1-26.1.0");
var composeService = builder.AddProject<Projects.ComposeService>("compose-service");

AppResources resources= new()
{
    Keycloak = keycloak,
    KeycloakConfig = kcConfig,
    Redis = redis,
    PostgresServer = postgres,
    Gateway = gateway,
    ProfileDatabase = profileDatabase,
    ProfileService = profileService,
    ComposeService = composeService
};

//////////////////////////////////////////// DYNAMIC PARAMETERS ////////////////////////////////////////////
    
parameters.GatewayUrl= settings.IsTesting
    ? ReferenceExpression.Create($"{gateway.GetEndpoint("http")}")
    : ReferenceExpression.Create($"{parameters.AppBaseUrl}");

parameters.ClientAuthority = ReferenceExpression.Create($"{parameters.GatewayUrl}/auth/realms/lightspeak");

//////////////////////////////////////////// CONFIGURATIONS ////////////////////////////////////////////
keycloak.ConfigureKeycloak(parameters, settings);
profileService.ConfigureProfileSevice(parameters, settings,resources);
kcConfig.ConfigureKeycloakConfig(parameters, settings, resources);
gateway.ConfigureGateway(parameters, settings, resources);
if(settings.IsDev || settings.IsTesting) builder.AddAndConfigureDebugService(parameters, settings, resources);

composeService
    .WithReference(resources.ProfileService)
    .WithEnvironment("AuthSettings__Authority", parameters.ClientAuthority)
    .WithEnvironment("AuthSettings__Audience", parameters.ClientAudience);
   //.WithEnvironment("Grpc__ProfileService__Address",profileService.GetEndpoint("http"));




gateway.WaitForCompletion(kcConfig);
builder.Build().Run();
