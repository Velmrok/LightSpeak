using System.Security.Claims;
using Gateway.src.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("reverseproxy.json", optional: false, reloadOnChange: true);

builder.Services.AddStackExchangeRedisCache(o =>
    o.Configuration = builder.Configuration.GetConnectionString("Redis"));


builder.Services.AddServices(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
    
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("users/me", (ClaimsPrincipal claim) =>
{
    return claim.Claims.ToDictionary(c => c.Type, c => c.Value);
}).RequireAuthorization();

app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();


app.Run();
