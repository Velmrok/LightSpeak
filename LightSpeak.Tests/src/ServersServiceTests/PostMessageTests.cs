using System.Net.Http.Json;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using Microsoft.EntityFrameworkCore;
using Common.Dto.Events;
using System.Text.Json;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class PostMessageTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    private async Task<PostMessageResponse> PostMessageAsync(HttpClient client, string channelId, string content, CancellationToken ct)
    {
        var request = new PostMessageRequest
        (
            Content: content
        );

        var json = await client.PostAsJsonAsync($"servers/channels/{channelId}/messages", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        var response = await ReadFromJson<PostMessageResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        return data;
    }

    [Fact]
    public async Task PostsMessage_Correctly_OnPostMessageRequest_WhileBeingMemberOfAServer()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var channelData = await CreateChannelAsync(client, serverId, channelName, ct);
        var channelId = channelData.ChannelId;

        var request = new PostMessageRequest
        (
            Content: messageContent
        );

        var json = await client.PostAsJsonAsync($"servers/channels/{channelId}/messages", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);

        var response = await ReadFromJson<PostMessageResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.Equal(messageContent, data.Content);
    
        var dbContext = await CreateDbContext(ct);
        var messageInDb = await dbContext.ChatMessages.FirstOrDefaultAsync(m => m.Id == data.MessageId, ct);
        Assert.NotNull(messageInDb);
        Assert.Equal(messageContent, messageInDb.Content);
    }
    [Fact]
    public async Task PostMessage_SendsEvent_Correctly_OnSuccessfulPost()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var channelData = await CreateChannelAsync(client, serverId, channelName, ct);
        var channelId = channelData.ChannelId;

        await using var channel = await Fixture.RabbitMqConnection.CreateChannelAsync(cancellationToken: ct);
        var queue = await channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true);
        await channel.QueueBindAsync(queue.QueueName, "amq.topic", MessageCreatedEvent.RoutingKey);

        await PostMessageAsync(client, channelId, messageContent, ct);

        await Eventually.Assert(
            async () =>
            {
                var result = await channel.BasicGetAsync(queue.QueueName, autoAck: true);
                Assert.NotNull(result);
                var evt = JsonSerializer.Deserialize<MessageCreatedEvent>(result.Body.Span);
                Assert.NotNull(evt);
                Assert.Equal(messageContent, evt.Content);
                Assert.Equal(channelId, evt.ChannelId);
            }, TimeSpan.FromSeconds(15), ct);
    }
}
