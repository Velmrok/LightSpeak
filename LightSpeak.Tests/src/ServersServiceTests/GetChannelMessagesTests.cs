

using System.Net.Http.Json;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class GetChannelMessagesTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    [Fact]
    public async Task GetsChannelMessages_Correctly_OnGetChannelMessagesRequest_WhileBeingMember()
    {
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);
        var server = await CreateServerAsync(client, "Test Server", ct);
        var channel = await CreateChannelAsync(client, server.ServerId, "Test Channel", ct);
        await PostMessageAsync(client, channel.ChannelId, "Hello, world!", ct);

        var response = await client.GetAsync($"api/servers/channels/{channel.ChannelId}/messages?page=1&limit=10", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resp = await ReadFromJson<GetChannelMessagesResponse>(response, ct);
        var data = resp.Data;
        Assert.NotNull(data);
        Assert.Single(data.Messages);
        var message = data.Messages.First();
        Assert.Equal("Hello, world!", message.Content);
        Assert.Equal(user.Id, message.SenderId);
    }
}