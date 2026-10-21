using System.Net.Http.Json;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using Microsoft.EntityFrameworkCore;
using Common.Dto.Events;
using System.Text.Json;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class PostDmMessageTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    private async Task<PostMessageResponse> PostDmMessageAsync(HttpClient client, string targetUserId, string content, CancellationToken ct)
    {
        var request = new PostMessageRequest
        (
            Content: content
        );

        var json = await client.PostAsJsonAsync($"/api/servers/channels/dm/{targetUserId}/messages", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        var response = await ReadFromJson<PostMessageResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        return data;
    }

    [Fact]
    public async Task PostsDmMessage_Correctly_OnPostMessageRequest_ToExistingUser()
    {
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user1 = await LoginOnFreshTestUserAsync(client,ct);
        var user2 = await CreateUniqueTestUserAsync(ct);

        var request = new PostMessageRequest
        (
            Content: messageContent
        );

        var json = await client.PostAsJsonAsync($"/api/servers/channels/dm/{user2.Id}/messages", request, ct);
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
    public async Task PostDmMessage_SendsEvent_Correctly_OnSuccessfulPost()
    {
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user1 = await LoginOnFreshTestUserAsync(client,ct);
        var user2 = await CreateUniqueTestUserAsync(ct);

        await using var channel = await Fixture.RabbitMqConnection.CreateChannelAsync(cancellationToken: ct);
        var queue = await channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true);
        await channel.QueueBindAsync(queue.QueueName, "amq.topic", MessageCreatedEvent.RoutingKey);

        var response = await PostDmMessageAsync(client, user2.Id, messageContent, ct);

        await Eventually.Assert(
            async () =>
            {
                var result = await channel.BasicGetAsync(queue.QueueName, autoAck: true);
                Assert.NotNull(result);
                var evt = JsonSerializer.Deserialize<MessageCreatedEvent>(result.Body.Span);
                Assert.NotNull(evt);
                Assert.Equal(messageContent, evt.Content);
                Assert.Equal(response.channelId, evt.ChannelId);
                Assert.Contains(user2.Id,evt.RecipientUserIds);
                Assert.Contains(user1.Id,evt.RecipientUserIds);
                Assert.Equal(2,evt.RecipientUserIds.Count);
            }, TimeSpan.FromSeconds(15), ct);
    }
    [Fact]
    public async Task PostDmMessage_Returns404NotFound_OnPostMessageRequest_ToNonExistingUser()
    {
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = CreateClient();
        await LoginOnFreshTestUserAsync(client,ct);
        var nonExistingUserId = Guid.NewGuid().ToString();

        var request = new PostMessageRequest
        (
            Content: messageContent
        );

        var json = await client.PostAsJsonAsync($"/api/servers/channels/dm/{nonExistingUserId}/messages", request, ct);
        Assert.Equal(HttpStatusCode.NotFound, json.StatusCode);

    }
}
