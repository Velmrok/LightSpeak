using Aspire.Hosting;
using LightSpeak.AppHost.src;
using Microsoft.Extensions.Configuration;
using LightSpeak.AppHost.src.Constants;
var builder = DistributedApplication.CreateBuilder(args);

var parameters = builder.AddApplicationParameters();
var settings = builder.AddApplicationSettings();

//////////////////////////////////////////// DECLARATIONS ////////////////////////////////////////////
var redis = builder.AddRedis(ResourcesNames.Redis).WithLifetime(ContainerLifetime.Persistent);
var postgres = builder.AddPostgres(ResourcesNames.Postgres, parameters.PostgresUser, parameters.PostgresPassword).WithLifetime(ContainerLifetime.Persistent);
var gateway = builder.AddProject<Projects.Gateway>(ResourcesNames.Gateway);
var keycloak =builder.AddKeycloak(ResourcesNames.Keycloak, 8080, parameters.KcAdminUser, parameters.KcAdminPassword);
var profileDatabase = postgres.AddDatabase(ResourcesNames.ProfileDatabase, ResourcesNames.ProfileDatabase);
var profileService = builder.AddProject<Projects.ProfileService>(ResourcesNames.ProfileService);
var kcConfig = builder.AddContainer(ResourcesNames.KeycloakConfig , "adorsys/keycloak-config-cli", "6.5.1-26.1.0");
var composeService = builder.AddProject<Projects.ComposeService>(ResourcesNames.ComposeService);
var rabbitmq = builder.AddRabbitMQ(ResourcesNames.RabbitMQ,parameters.RabbitUser, parameters.RabbitPassword).WithLifetime(ContainerLifetime.Persistent);


AppResources resources= new()
{
    Keycloak = keycloak,
    KeycloakConfig = kcConfig,
    Redis = redis,
    PostgresServer = postgres,
    Gateway = gateway,
    ProfileDatabase = profileDatabase,
    ProfileService = profileService,
    ComposeService = composeService,
    RabbitMQ = rabbitmq
};

//////////////////////////////////////////// DYNAMIC PARAMETERS ////////////////////////////////////////////
    
parameters.GatewayUrl= settings.IsTesting
    ? ReferenceExpression.Create($"{gateway.GetEndpoint("http")}")
    : ReferenceExpression.Create($"{parameters.AppBaseUrl}");

parameters.ClientAuthority = ReferenceExpression.Create($"{parameters.GatewayUrl}/auth/realms/lightspeak");

//////////////////////////////////////////// CONFIGURATIONS ////////////////////////////////////////////
keycloak.ConfigureKeycloak(parameters, settings, resources);
profileService.ConfigureProfileSevice(parameters, settings,resources);
kcConfig.ConfigureKeycloakConfig(parameters, settings, resources);
gateway.ConfigureGateway(parameters, settings, resources);
if(settings.IsDev || settings.IsTesting) builder.AddAndConfigureDebugService(parameters, settings, resources);

composeService
    .WithReference(profileService)
    .WithEnvironment("AuthSettings__Authority", parameters.ClientAuthority)
    .WithEnvironment("AuthSettings__Audience", parameters.ClientAudience)
    .WithEnvironment("Grpc__ProfileService__Address", $"http://_grpc.{profileService.Resource.Name}");
rabbitmq.WithManagementPlugin();





gateway.WaitForCompletion(kcConfig);
builder.Build().Run();
