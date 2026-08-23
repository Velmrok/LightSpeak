

using System.Text;
using System.Text.Json;
using Common.Constants;
using Common.Dto;
using RabbitMQ.Client;

namespace LightSpeak.Tests;

public class RabbitMqTestHelper
{
    private static async Task PublishEventAsync(IChannel channel, string routingKey, object message, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        await channel.BasicPublishAsync(
            exchange: "amq.topic",
            routingKey: routingKey,
            body: body,
            cancellationToken: ct);
    }
    public static async Task PublishRegisterEventAsync(IConnection connection,string userId, string username, string email,CancellationToken ct)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        var message = new KeycloakRegisterEvent(
            Time: int.TryParse(DateTime.UtcNow.ToString(), out var time) ? time : 0,
            UserId: userId,
            Details: new KeycloakRegisterEventDetails(
                Username: username,
                Email: email
            )
        );
        await PublishEventAsync(channel, RoutingKeys.UserRegistered, message, ct);

    }
}