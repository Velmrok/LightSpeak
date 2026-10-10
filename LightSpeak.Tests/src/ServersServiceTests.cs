using System.Net.Http.Json;
using Aspire.Hosting;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;
using Common.Dto.Events;
using System.Text.Json;
using ServersService.src.permissions;

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
    public async Task CreatesServer_Correctly_OnCreateServerRequest_WhileBeingLoggedIn()
    {
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);
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
        Assert.Equal( user.Username,member.User.Name);
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
         await LoginOnFreshTestUserAsync(client,ct);
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
        var client = Fixture.CreateGatewayClient();
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
        var client = Fixture.CreateGatewayClient();
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
        var client = Fixture.CreateGatewayClient();
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
    [Fact]
    public async Task PostsMessage_Correctly_OnPostMessageRequest_WhileBeingMemberOfAServer()
    {
        var channelName = "Test Channel";
        var serverName = "Test Server";
        var messageContent = "Hello, world!";
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
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
        var client = Fixture.CreateGatewayClient();
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
    [Fact]
    public async Task UserChangedDataHandler_UpdatesUserDataInDatabase_Correctly_OnUserChangedDataEvent()
    {
 
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);

        await CreateServerAsync(client, serverName, ct); // Create a server to ensure the userSnapshot exists in the database
        await Eventually.Assert(
            async () =>
            {
                var dbContext = await CreateDbContext(ct);
                var userSnapshot = await dbContext.UserSnapshots.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id, ct);
                Assert.NotNull(userSnapshot);
            }, TimeSpan.FromSeconds(15), ct);

        var evt = new UserDataChangedEvent(
            UserId: user.Id,
            Username: "UpdatedUsername",
            AvatarUrl: "https://example.com/avatar.jpg"
        );
    
        await RabbitMqTestHelper.PublishEventAsync(Fixture.RabbitMqConnection, UserDataChangedEvent.RoutingKey, evt, ct);

        await Eventually.Assert(
            async () =>
            {
                var dbContext = await CreateDbContext(ct);
                var userSnapshot = await dbContext.UserSnapshots.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id, ct);
                Assert.NotNull(userSnapshot);
                Assert.Equal(evt.Username, userSnapshot.Name);
                Assert.Equal(evt.AvatarUrl, userSnapshot.AvatarUrl);
            }, TimeSpan.FromSeconds(15), ct);
    }
    [Fact]
    public async Task TestUnreachableRolePermission_Returns403_WhenLoggedIn()
    {

        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var json = await client.GetAsync("/servers/test-unreachable-role-permission", ct);
        Assert.Equal(HttpStatusCode.Forbidden, json.StatusCode);
    }
    [Fact]
    public async Task TestUnreachableRolePermission_Returns401_WhenNotLoggedIn()
    {

        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();

        var json = await client.GetAsync("/servers/test-unreachable-role-permission", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, json.StatusCode);
    }
    [Fact]
    public async Task PatchRole_Correctly_OnPatchRoleRequest_WhileHavingManageRolesPermission()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = Fixture.CreateGatewayClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roleToEdit = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Everyone", ct);
        Assert.NotNull(roleToEdit);

        var request = new PatchRoleRequest
        (
            RoleId: roleToEdit.Id,
            NewName: null,
            AddedPermissions: [Permission.ManageChannels],
            RemovedPermissions: [Permission.ReadOnChannel]
        );

        var json = await client.PatchAsJsonAsync($"/servers/{serverId}/roles", request, ct);
       // Assert.Equal(HttpStatusCode.OK, json.StatusCode);

        var response = await ReadFromJson<PatchRoleResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data); 
        Assert.Equal(roleToEdit.Name, data.Name);
        Assert.Contains(Permission.ManageChannels, data.Permissions);
        Assert.DoesNotContain(Permission.ReadOnChannel, data.Permissions);
    }
}
