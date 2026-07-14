using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("users/me", (ClaimsPrincipal claim) =>
{
    return claim.Claims.ToDictionary(c => c.Type, c => c.Value);
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
