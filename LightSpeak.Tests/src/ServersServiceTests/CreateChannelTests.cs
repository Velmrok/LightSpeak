using System.Net.Http.Json;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class CreateChannelTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    [Fact]
    public async Task CreatesChannel_Correctly_OnCreateChannelRequest_WhileBeingMemberOfAServer()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";

        var ct = CancellationToken.None;

        var client = CreateClient();

         await LoginOnFreshTestUserAsync(client,ct);
        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        

        var channelData = await CreateChannelAsync(client, serverId, channelName, ct);
        Assert.Equal(channelName, channelData.Name);
      
        var dbContext = await CreateDbContext(ct);

        var channelInDb = await dbContext.Channels.FirstOrDefaultAsync(c => c.Id == channelData.ChannelId, ct);
        Assert.NotNull(channelInDb);
        Assert.Equal(channelName, channelInDb.Name);
    }
    [Fact]
    public async Task CreateChannel_Returns404_NotFound_WhenServerDoesNotExist()
    {
        var name = "Test Channel";
        var ct = CancellationToken.None;
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var request = new CreateChannelRequest
        (
            Name: name
        );
        
        var json = await client.PostAsJsonAsync($"/servers/nonexistent-server-id/channels", request, ct);
        Assert.Equal(HttpStatusCode.NotFound, json.StatusCode);
    }
    [Fact]
    public async Task CreateChannel_Returns400_BadRequest_WhenNameIsInvalid()
    {
        var channelName = "";
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var request = new CreateChannelRequest
        (
            Name: channelName
        );
        var json = await client.PostAsJsonAsync($"/servers/{serverId}/channels", request, ct);
        Assert.Equal(HttpStatusCode.BadRequest, json.StatusCode);
    }
    [Fact]
    public async Task CreateChannel_Returns404_NotFound_WhenUserIsNotMemberOfServer()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        _authClient.ResetCookies();
        
        await LoginOnFreshTestUserAsync(client,ct);

        var request = new CreateChannelRequest
        (
            Name: channelName
        );
        
        var json = await client.PostAsJsonAsync($"/servers/{serverId}/channels", request, ct);
        Assert.Equal(HttpStatusCode.NotFound, json.StatusCode);
    }
}
