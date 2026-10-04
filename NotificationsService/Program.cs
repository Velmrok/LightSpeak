using Common;
using Common.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();
builder.Services.AddSingleton<IResponseBuilderService, ResponseBuilderService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.Run();

