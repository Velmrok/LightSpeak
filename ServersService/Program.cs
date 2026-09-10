using Common;
using Microsoft.EntityFrameworkCore;
using ServersService.src;
using ServersService.src.endpoints;
using Common.Grpc;
using ServersService.src.services;
using Common.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("servers-database")));

builder.Services.AddAndConfigureProfileServiceClient(builder.Configuration);
    builder.Services.AddSingleton<GrpcCallHandler>();
    
builder.Services.AddScoped<IServersApplicationService, ServersApplicationService>();
builder.Services.AddScoped<IProfileClient, ProfileGrpcClient>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("", async (AppDbContext db) =>
{
    var servers = await db.Servers
        .Select(s => new ServerResponse(
            s.Id,
            s.Name,
            s.Members.Select(m => new MemberResponse(m.UserId, m.User.Name)).ToList(),
            s.Channels.Select(c => new ChannelResponse(c.Id, c.Name)).ToList()
        ))
        .ToListAsync();

    return Results.Ok(new { servers });
});



app.MapServersEndpoints();
app.MapHealthChecks("/health");



app.Run();

public record ServerResponse(string Id, string Name, List<MemberResponse> Members, List<ChannelResponse> Channels);
public record MemberResponse(string UserId, string Username);
public record ChannelResponse(string Id, string Name);