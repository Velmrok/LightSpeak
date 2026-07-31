using Common;
using Microsoft.EntityFrameworkCore;
using ProfileService.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("profile-database")));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("Profile Service is healthy!"));


app.Run();
