
using Protos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddGrpcClient<ProfileService.ProfileServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:ProfileService:Address"]!);
}).AddServiceDiscovery();

var app = builder.Build();

app.MapGet("/home", async (ProfileService.ProfileServiceClient client) =>
{
    var request = new GetProfileRequest { UserId = "123" };
    var response = await client.GetProfileAsync(request);
    return Results.Ok(response);
});



app.Run();

