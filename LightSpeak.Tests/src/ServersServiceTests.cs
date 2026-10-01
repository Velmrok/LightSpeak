using System.Net.Http.Json;
using Aspire.Hosting;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src;
[Collection("Aspire")]
public class ServersServiceTest : TestBase
{
     public ServersServiceTest(AppFixture fixture) : base(fixture)
    {
       
    }
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    private async Task<CreateServerResponse> CreateServerAsync(HttpClient client, string name, CancellationToken ct)
    {
        var request = new CreateServerRequest
        (
            Name: name
        );

        var json = await client.PostAsJsonAsync("/servers", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        var response = await ReadFromJson<CreateServerResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        return data;
    }
    private async Task<CreateChannelResponse> CreateChannelAsync(HttpClient client, string serverId, string name, CancellationToken ct)
    {
        var request = new CreateChannelRequest
        (
            Name: name
        );

        var json = await client.PostAsJsonAsync($"/servers/{serverId}/channels", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        var response = await ReadFromJson<CreateChannelResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        return data;
    }
    
    [Fact]
    public async Task CreatesServer_Correctly_OnCreateServerRequest_WhileBeingLoggedIn()
    {
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);
        var name = "Test Server";

        var data = await CreateServerAsync(client, name, ct);
        Assert.Equal(name, data.Name);
      
        var dbContext = await CreateDbContext(ct);
        var serverInDb = await dbContext.Servers.FirstOrDefaultAsync(s => s.Id == data.ServerId, ct);
        Assert.NotNull(serverInDb);
        Assert.Equal(name, serverInDb.Name);

        var members = await dbContext.Members
            .Where(m => m.ServerId == serverInDb.Id)
            .Include(m => m.User)
            .ToListAsync(ct);
        Assert.Single(members);
        var member = members.First();
        Assert.Equal( AuthClient.testUserName,member.User.Name);
    }
    [Fact]
    public async Task CreateServer_Returns401_Unauthorized_WhenNotLoggedIn()
    {
        var ct = CancellationToken.None;

        var client = Fixture.CreateGatewayClient();

        var request = new CreateServerRequest
        (
            Name: "Test Server"
        );

        var json = await client.PostAsJsonAsync("/servers", request, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, json.StatusCode);
    }
    [Fact]
    public async Task CreateServer_Returns400_BadRequest_WhenNameIsInvalid()
    {
        var ct = CancellationToken.None;

        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);
        var request = new CreateServerRequest
        (
            Name: ""
        );

        var json = await client.PostAsJsonAsync("/servers", request, ct);
        Assert.Equal(HttpStatusCode.BadRequest, json.StatusCode);
    }
    [Fact]
    public async Task CreatesChannel_Correctly_OnCreateChannelRequest_WhileBeingMemberOfAServer()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";

        var ct = CancellationToken.None;

        var client = Fixture.CreateGatewayClient();

        await _authClient.LoginAsync(client, DefaultTimeout, ct);
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
        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);

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
        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);

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
        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        _authClient.ResetCookies();
        await _authClient.LoginAsync(client, DefaultTimeout, ct, AuthClient.testUserName2, AuthClient.testUserPassword2);

        var request = new CreateChannelRequest
        (
            Name: channelName
        );
        
        var json = await client.PostAsJsonAsync($"/servers/{serverId}/channels", request, ct);
        Assert.Equal(HttpStatusCode.NotFound, json.StatusCode);
    }
    [Fact]
    public async Task PostsMessage_Correctly_OnPostMessageRequest_WhileBeingMemberOfAServer()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);

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
}
