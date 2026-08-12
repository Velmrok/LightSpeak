using Common;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ProfileService.src.database;
using ProfileService.src.grpc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpoint =>
    {
        endpoint.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("profile-database")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ProfileGrpcService>();

app.MapGet("/health", () => Results.Ok("Profile Service is healthy!"));


app.Run();
