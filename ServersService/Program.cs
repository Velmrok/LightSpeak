using Common;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("Servers Service is healthy!"));





app.Run();

