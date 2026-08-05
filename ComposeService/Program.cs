
using Common;
using Protos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddAuth(builder.Configuration);

builder.Services.AddTransient<JwtInterceptor>();

builder.Services.AddGrpcClient<ProfileService.ProfileServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:ProfileService:Address"]!);
}).AddServiceDiscovery()
.AddInterceptor<JwtInterceptor>();

builder.Services.AddCommonServices(builder.Configuration);
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/home", async (ProfileService.ProfileServiceClient client) =>
{
    var request = new GetProfileRequest { UserId = "123" };
    var response = await client.GetProfileAsync(request);
    return Results.Ok(response);
});




app.Run();

