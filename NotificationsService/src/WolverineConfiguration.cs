

using Common.Dto.Events;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.RabbitMQ;

namespace NotificationsService.src;

public static class WolverineConfiguration
{
    public static void AddAndConfigureWolverine(this WebApplicationBuilder builder)
    {
        builder.UseWolverine(opts =>
        {
            opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
            var rabbit = opts.UseRabbitMq(builder.Configuration.GetConnectionString("rabbitmq")!)
                .AutoProvision()
                .BindExchange("amq.topic", ex =>
                {
                    ex.ExchangeType = ExchangeType.Topic;
                });
            rabbit.ToQueue("servers-service.message.created", MessageCreatedEvent.RoutingKey);

            opts.ApplicationAssembly = typeof(MessageCreatedEventHandler).Assembly;

            opts.ListenToRabbitQueue("servers-service.message.created")
            .DefaultIncomingMessage<MessageCreatedEvent>();

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