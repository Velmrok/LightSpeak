using System.Text;
using System.Text.Json;
using Common.Dto.Events;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Common.Services;
public class RabbitMqClient(IConfiguration config) : IAsyncDisposable, IEventPublisher
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;
     public async Task PublishEventAsync<TEvent>(TEvent message , CancellationToken ct=default) where TEvent : IEvent
    {
        await using var channel = await CreateChannelAsync(ct);
        await channel.BasicPublishAsync(
            exchange: "amq.topic",
            routingKey: TEvent.RoutingKey,
            body: JsonSerializer.SerializeToUtf8Bytes(message),
            cancellationToken: ct);
    }
    private async Task<IChannel> CreateChannelAsync(CancellationToken ct)
    {
        var connection = await GetConnectionAsync(ct);
        return await connection.CreateChannelAsync(cancellationToken: ct);
    }

    private async ValueTask<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is not null) return _connection;

        await _lock.WaitAsync(ct);
        try
        {
            if (_connection is not null) return _connection;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(config.GetConnectionString("rabbitmq")!)
            };
            var connection = await factory.CreateConnectionAsync(ct);
            _connection = connection;
            return _connection;
        }
        finally { _lock.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        _lock.Dispose();
    }

   
}