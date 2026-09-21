
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

builder.Services.AddSingleton<GrpcCallHandler>();
builder.Services.AddScoped<IProfileClient, ProfileGrpcClient>();
builder.Services.AddScoped<IResponseBuilderService, ResponseBuilderService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health");

app.MapGet("/home", async (IProfileClient client, IResponseBuilderService responseBuilder, HttpContext context) =>
{
    var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;

    var profileTask = client.GetUserProfileAsync(userId, CancellationToken.None);
    
    var results = await Task.WhenAll([profileTask]);

    var outcomes = results.Select(r => r.AsOutcome(true)).ToList();
   
    return responseBuilder.BuildResponse(HttpStatusCode.OK, outcomes,
        () =>
            {
                var profileData = results[0].Data!;
                return new HomeResponse(
                    UserId: profileData.UserId,
                    Username: profileData.Username,
                    Email: profileData.Email);
            });
}).RequireAuthorization();





app.Run();

