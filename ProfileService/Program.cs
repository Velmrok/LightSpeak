using Common;
using JasperFx.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ProfileService.src.database;
using ProfileService.src.grpc;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpoint =>
    {
        endpoint.Protocols = HttpProtocols.Http2;
    });
});
builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(builder.Configuration.GetConnectionString("rabbitmq")!)
        .AutoProvision()
        .BindExchange("amq.topic", ex =>
        {
            ex.ExchangeType = ExchangeType.Topic;
        })
        .ToQueue("profile-service.keycloak.register", "KK.EVENT.CLIENT.*.SUCCESS.*.REGISTER");

    opts.ListenToRabbitQueue("profile-service.keycloak.register");

    opts.OnException<Exception>()
        .RetryWithCooldown(1.Seconds(), 5.Seconds(), 15.Seconds())
        .Then.MoveToErrorQueue();
});

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


app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ProfileGrpcService>();

app.MapGet("/health", () => Results.Ok("Profile Service is healthy!"));


app.Run();
