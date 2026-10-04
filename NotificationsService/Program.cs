using Common;
using Common.Services;
using Microsoft.AspNetCore.SignalR;
using NotificationsService.src;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddServiceDiscovery()
    .AddConfigurationServiceEndpointProvider();
builder.Services.AddSingleton<IResponseBuilderService, ResponseBuilderService>();
builder.Services.AddSingleton<IUserIdProvider, SubUserIdProvider>();
builder.Services.AddSignalR();
builder.AddAndConfigureWolverine();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapHub<AppHub>("hubs/app");
app.Run();

