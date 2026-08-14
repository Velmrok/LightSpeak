
using Common;
using Common.Grpc;
using ComposeService.src.dto;
using Protos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddAuth(builder.Configuration);

//builder.Services.AddTransient<JwtInterceptor>();

builder.Services.AddGrpcClient<ProfileService.ProfileServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:ProfileService:Address"]!);
}).AddServiceDiscovery()
.ConfigureGrpcCredentials();


builder.Services.AddSingleton<GrpcCallHandler>();

builder.Services.AddCommonServices(builder.Configuration);
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


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
                    Name: profileData.Name,
                    Email: profileData.Email);
            });
});




app.Run();

