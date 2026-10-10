using System.Net.Http.Json;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;
using ServersService.src.permissions;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class CreateRoleTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    [Fact]
    public async Task CreateRole_ReturnsCreatedRole_WhenValidDataIsProvided_AndUserHasPermission()
    {
        
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        
        var request = new CreateRoleRequest
        (
            Name: "New Role",
            Priority: 1,
            Permissions: [Permission.ManageChannels]
        );
        var response = await client.PostAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var roleResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CreateRoleResponse>>(ct);
        Assert.NotNull(roleResponse);
        Assert.NotNull(roleResponse.Data);
        Assert.Equal(request.Name, roleResponse.Data.Name);
        Assert.Equal(request.Priority, roleResponse.Data.Priority);
        var dbContext = await CreateDbContext(ct);
        var roleInDb = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleResponse.Data.Id, ct);
        Assert.NotNull(roleInDb);
        Assert.Equal(request.Name, roleInDb.Name);
        Assert.Equal(request.Priority, roleInDb.Priority);
        Assert.Equal(request.Permissions.ToMask(), roleInDb.Permissions);
        var everyoneRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Everyone", ct);
        Assert.NotNull(everyoneRole);
        Assert.Equal(2, everyoneRole.Priority);
    }
    [Fact]
    public async Task CreateRole_Returns403Forbidden_WhenUserDoesNotHaveManageRolesPermission()
    {

        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        await LoginOnFreshTestUserAsync(client, ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        var dbContext = await CreateDbContext(ct);
        var ownerRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Owner", ct);
        Assert.NotNull(ownerRole);
        ownerRole.Permissions &= ~Permission.ManageRoles;
        await dbContext.SaveChangesAsync(ct);

        var request = new CreateRoleRequest
        (
            Name: "New Role",
            Priority: 1,
            Permissions: [Permission.ManageChannels]
        );
        var response = await client.PostAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    }
    [Fact]
    public async Task CreateRole_Returns403Forbidden_WhenUserCreatesRoleWithLowerPriority()
    {

        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client, ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        var dbContext = await CreateDbContext(ct);
        var everyoneRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Owner", ct);
        var member = await dbContext.Members.Include(m => m.Roles).FirstOrDefaultAsync(m => m.UserId == user.Id && m.ServerId == serverId, ct);
        Assert.NotNull(member);
        member.Roles.RemoveAll(r => r.Name == "Owner");
        Assert.NotNull(everyoneRole);
        everyoneRole.Permissions |= Permission.ManageRoles;
         await dbContext.SaveChangesAsync(ct);

        var request = new CreateRoleRequest
        (
            Name: "New Role",
            Priority: 0,
            Permissions: [Permission.ReadOnChannel]
        );
        var response = await client.PostAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    }
    [Fact]
    public async Task CreateRole_Returns403Forbidden_WhenUserCreatesRole_WithPermissionsHeDoesntOwn()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client, ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        var dbContext = await CreateDbContext(ct);
        var everyoneRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Owner", ct);
        var member = await dbContext.Members.Include(m => m.Roles).FirstOrDefaultAsync(m => m.UserId == user.Id && m.ServerId == serverId, ct);
        Assert.NotNull(member);
        member.Roles.RemoveAll(r => r.Name == "Owner");
        Assert.NotNull(everyoneRole);
        everyoneRole.Permissions |= Permission.ManageRoles;
         await dbContext.SaveChangesAsync(ct);
        var request = new CreateRoleRequest
        (
            Name: "New Role",
            Priority: 4,
            Permissions: [Permission.ManageChannels]
        );
        var response = await client.PostAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    }
    [Fact]
    public async Task CreateRole_Returns409Conflict_WhenUserCreatesRole_WithDuplicateName()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
      await LoginOnFreshTestUserAsync(client, ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;


        var request = new CreateRoleRequest
        (
            Name: "Everyone",
            Priority: 4,
            Permissions: [Permission.ManageChannels]
        );
        var response = await client.PostAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    }
}
