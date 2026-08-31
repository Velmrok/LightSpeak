using Common;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();




app.MapHealthChecks("/health");



app.Run();

