
using System.IdentityModel.Tokens.Jwt;
using Common;
using Common.Clients;
using Common.Grpc;
using Common.Services;
using ComposeService.src.dto;
using Common.Dto;
using Protos;
using static Protos.ProfileService;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddAuth(builder.Configuration);

builder.Services.AddAndConfigureProfileServiceClient(builder.Configuration);
builder.Services.AddAndConfigureServersServiceClient(builder.Configuration);

builder.Services.AddSingleton<GrpcCallHandler>();
builder.Services.AddScoped<IProfileClient, ProfileGrpcClient>();
builder.Services.AddScoped<IServersClient, ServersGrpcClient>();

builder.Services.AddScoped<IResponseBuilderService, ResponseBuilderService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health");

app.MapGet("/home", async (IProfileClient client, IServersClient serversClient, IResponseBuilderService responseBuilder, HttpContext context) =>
{
    var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;

    var profileTask = client.GetUserProfileAsync(userId, CancellationToken.None);
    var serversTask = serversClient.GetServersAsync(userId, CancellationToken.None);

    await Task.WhenAll(profileTask, serversTask);

    var profileOutcome = (await profileTask).AsOutcome(true);
    var serversOutcome = (await serversTask).AsOutcome(false);



    ICallOutcome[] outcomes = [profileOutcome, serversOutcome];

    return responseBuilder.BuildResponse(HttpStatusCode.OK, outcomes,
        () =>
            {
                Common.Dto.GetUserProfileResponse pd = profileOutcome.Data!;
                Common.Dto.GetServersResponse? sd = serversOutcome.Data;
                var servers = sd?.Servers;
                if (servers is not null && pd.ServerPositions.Count > 0)
                {
                    var rank = pd.ServerPositions
                        .Select((id, i) => (id, i))
                        .ToDictionary(x => x.id, x => x.i);

                    servers = [.. servers.OrderBy(s => rank.TryGetValue(s.Id, out var i) ? i : int.MinValue)];
                }
                return new HomeResponse(
                    Servers: servers,
                    Friends: [],
                    UserProfile: pd
                );
            });
}).RequireAuthorization();





app.Run();

