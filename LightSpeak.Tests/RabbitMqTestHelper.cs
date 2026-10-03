

using System.Text;
using System.Text.Json;
using Common.Constants;
using Common.Dto;
using RabbitMQ.Client;

namespace LightSpeak.Tests;

public class RabbitMqTestHelper
{
    public static async Task PublishEventAsync(IConnection connection, string routingKey, object message, CancellationToken ct)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
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
            Time: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            UserId: userId,
            Details: new KeycloakRegisterEventDetails(
                Username: username,
                Email: email
            )
        );
        await PublishEventAsync(connection, RoutingKeys.UserRegistered, message, ct);

    }
}