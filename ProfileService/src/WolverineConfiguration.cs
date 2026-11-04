using Common.Constants;
using Common.Dto;
using Common.Dto.Events;
using JasperFx.Core;
using ProfileService.src.database;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

namespace ProfileService.src;

public static class WolverineConfiguration
{
    public static void AddAndConfigureWolverine(this WebApplicationBuilder builder)
    {
        builder.UseWolverine(opts =>
        {
            opts.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();
            var rabbit = opts.UseRabbitMq(builder.Configuration.GetConnectionString("rabbitmq")!)
                .AutoProvision()
                .BindExchange("amq.topic", ex =>
                {
                    ex.ExchangeType = ExchangeType.Topic;
                });
            rabbit.ToQueue("profile-service.keycloak.register", RoutingKeys.UserRegistered);
            rabbit.ToQueue("profile-service.keycloak.admin.create", RoutingKeys.UserCreatedByAdmin);
            rabbit.ToQueue("profile-service.servers.joined", UserJoinedServerEvent.RoutingKey);


            opts.ApplicationAssembly = typeof(RegisterEventHandler).Assembly;

            opts.ListenToRabbitQueue("profile-service.keycloak.register")
            .DefaultIncomingMessage<KeycloakRegisterEvent>();

            opts.ListenToRabbitQueue("profile-service.servers.joined")
            .DefaultIncomingMessage<UserJoinedServerEvent>();

            opts.ListenToRabbitQueue("profile-service.keycloak.admin.create")
            .DefaultIncomingMessage<KeycloakAdminCreateEvent>();

            opts.OnException<Exception>()
                .RetryWithCooldown(1.Seconds(), 5.Seconds(), 15.Seconds())
                .Then.MoveToErrorQueue();
            opts.UseSystemTextJsonForSerialization(o =>
            {
                o.PropertyNameCaseInsensitive = true;
            });
        });
    }
}