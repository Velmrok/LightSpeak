using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;
using Gateway.src;
using Gateway.src.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("reverseproxy.json", optional: false, reloadOnChange: true);

builder.Services.AddStackExchangeRedisCache(o =>
    o.Configuration = builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddOpenIdConnectAccessTokenManagement();
builder.Services.AddAuthorization();
builder.Services.AddReverseProxy(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
    
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/users/me", (ClaimsPrincipal user) => new UserDto(
    Id:    user.FindFirstValue(JwtRegisteredClaimNames.Sub)!,
    Name:  user.Identity!.Name,
    Email: user.FindFirstValue(JwtRegisteredClaimNames.Email),
    Roles: user.FindAll("roles").Select(c => c.Value).ToArray()
)).RequireAuthorization();

app.MapPost("/logout", async ctx =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
    
}).RequireAuthorization();

app.MapGet("/login", async (HttpContext context) =>
    Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }) );

app.UseStaticFiles();   
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.UseAuthentication();
app.UseAuthorization();


app.Run();
