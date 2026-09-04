
using Common;
using Common.Grpc;
using ComposeService.src.dto;
using Protos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddAuth(builder.Configuration);

builder.Services.AddAndConfigureProfileServiceClient(builder.Configuration);

builder.Services.AddSingleton<GrpcCallHandler>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health");
app.MapGet("/home", async (ProfileService.ProfileServiceClient client, GrpcCallHandler grpc) =>
{
    var tasks = new[]
    {
        grpc.SafeCall("profile", true, (deadline) =>
            client.GetProfileAsync(new GetProfileRequest
            {
                UserId = "123"
            }, deadline:deadline).ResponseAsync)
    };
    var results = await Task.WhenAll(tasks);
    return grpc.BuildResponse(results,
        () =>
            {
                var profileData = results[0].Data!;

                return new HomeResponse(
                    UserId: profileData.UserId,
                    Username: profileData.Username,
                    Email: profileData.Email);
            });
});





app.Run();

