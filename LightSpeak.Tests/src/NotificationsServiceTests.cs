using System.Net.Http.Json;
using Common.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using NotificationsService.src.dto;

namespace LightSpeak.Tests.src;

[Collection("Aspire")]
public class NotificationsServiceTests : TestBase
{
    public NotificationsServiceTests(AppFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task MemberReceivesMessageCreated_OnPostMessage()
    {
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
        await LoginOnFreshTestUserAsync(client,ct);

        var server = await CreateServerAsync(client, "Test Server", ct);
        var serverId = server.ServerId;
        
        var channel = await CreateChannelAsync(client, server.ServerId, "Test Channel", ct);
        var channelId = channel.ChannelId;

        var received = new TaskCompletionSource<MessageCreatedClientDto>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(client.BaseAddress!, "/hubs/app"), o =>
            {
                o.Cookies = AuthClient.CookieContainer;   
            })
            .Build();

        connection.On<MessageCreatedClientDto>("MessageCreated", msg => received.TrySetResult(msg));

        await connection.StartAsync(ct);

        var message = new PostMessageRequest
        (
            Content: "hello"
        );

        var response = await client.PostAsJsonAsync($"/servers/channels/{channelId}/messages", message, ct);
        response.EnsureSuccessStatusCode();
        
        var msg = await received.Task.WaitAsync(TimeSpan.FromSeconds(15), ct);
        Assert.Equal("hello", msg.Content);
        Assert.Equal(channelId, msg.ChannelId);
    }
}