using Common;
using Microsoft.EntityFrameworkCore;
using ServersService.src;
using ServersService.src.endpoints;
using Common.Grpc;
using ServersService.src.services;
using Common.Clients;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using ServersService.src.permissions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("servers-database")));
builder.AddAndConfigureWolverine();

builder.Services.AddAndConfigureProfileServiceClient(builder.Configuration);
    builder.Services.AddSingleton<GrpcCallHandler>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqClient>();

builder.Services.AddSingleton<IResponseBuilderService, ResponseBuilderService>();
builder.Services.AddScoped<IServersApplicationService, ServersApplicationService>();
builder.Services.AddScoped<IProfileClient, ProfileGrpcClient>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>(); 
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthResultHandler>();
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

if (app.Configuration.GetValue<bool>("IsTesting"))
{
    app.MapGet("/test-unreachable-role-permission", () => Results.Ok()).RequirePermission(Permission.UnreachableRolePermission);
}
   

app.MapServersEndpoints();
app.MapHealthChecks("/health");



app.Run();

public record ServerResponse(string Id, string Name, List<MemberResponse> Members, List<ChannelResponse> Channels);
public record MemberResponse(string UserId, string Username);
public record ChannelResponse(string Id, string Name);