using Common;
using Common.Constants;
using Common.Dto;
using JasperFx.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ProfileService.src;
using ProfileService.src.database;
using ProfileService.src.grpc;
using ProfileService.src.services;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpoint =>
    {
        endpoint.Protocols = HttpProtocols.Http1AndHttp2;
    });
});
builder.Services.AddHealthChecks();

builder.UseWolverine(opts =>
{
    opts.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();
    opts.UseRabbitMq(builder.Configuration.GetConnectionString("rabbitmq")!)
        .AutoProvision()
        .BindExchange("amq.topic", ex =>
        {
            ex.ExchangeType = ExchangeType.Topic;
        })
        .ToQueue("profile-service.keycloak.register", RoutingKeys.UserRegistered);
    opts.ApplicationAssembly = typeof(RegisterEventHandler).Assembly;
    opts.ListenToRabbitQueue("profile-service.keycloak.register")
    .DefaultIncomingMessage<KeycloakRegisterEvent>();;

    opts.OnException<Exception>()
        .RetryWithCooldown(1.Seconds(), 5.Seconds(), 15.Seconds())
        .Then.MoveToErrorQueue();
    opts.UseSystemTextJsonForSerialization(o =>
{
    o.PropertyNameCaseInsensitive = true;
});
});

builder.Services.AddScoped<IProfileApplicationService, ProfileApplicationService>();

builder.Services.AddGrpc();
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("profile-database")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapHealthChecks("/health");


app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ProfileGrpcService>();




app.Run();
