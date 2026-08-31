using Common;
using Common.Grpc;
using Debug;
using Grpc.Core;
using Protos;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuth(builder.Configuration);

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddGrpcClient<ProfileService.ProfileServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["Grpc:ProfileService:Address"]!);
}).AddServiceDiscovery()
.ConfigureGrpcCredentials();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/auth-token", (HttpRequest request) =>
{
    return Results.Ok(new TokenResponse(

        Jwt: request.Headers.Authorization.ToString().Replace("Bearer ", "")
    ));
}).RequireAuthorization();
app.MapGet("/token", (HttpRequest request) =>
{
    return Results.Ok(new TokenResponse(

        Jwt: request.Headers.Authorization.ToString().Replace("Bearer ", "")
    ));
});

app.MapGet("/grpc-auth-check", async (ProfileService.ProfileServiceClient client) =>
{
    try
    {
        var response = await client.GetAuthCheckAsync(new Google.Protobuf.WellKnownTypes.Empty());
        return Results.Ok(response);
    }
    catch (RpcException ex)
    {
        return ex.StatusCode.ToRestResponse();
    }
}).RequireAuthorization();


app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();



