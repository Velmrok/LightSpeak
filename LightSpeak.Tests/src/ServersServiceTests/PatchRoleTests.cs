using System.Net.Http.Json;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;
using ServersService.src.permissions;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class PatchRoleTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    [Fact]
    public async Task PatchesRole_Correctly_OnPatchRoleRequest_WhileHavingManageRolesPermission()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
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
        Assert.Equal(HttpStatusCode.OK, json.StatusCode);

        var response = await ReadFromJson<PatchRoleResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data); 
        Assert.Equal(roleToEdit.Name, data.Name);
        Assert.Contains(Permission.ManageChannels, data.Permissions);
        Assert.DoesNotContain(Permission.ReadOnChannel, data.Permissions);
    }
    [Fact]
    public async Task PatchRole_Returns403Forbidden_WhenUserLacksManageRolesPermission()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user =await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roles = await dbContext.Roles.Where(r => r.ServerId == serverId).ToListAsync(ct);
        var roleToEdit = roles.FirstOrDefault(r => r.Name == "Everyone");
        var ownerRole = roles.FirstOrDefault(r => r.Name == "Owner");
        Assert.NotNull(roleToEdit);
        Assert.NotNull(ownerRole);
        ownerRole.Permissions &= ~Permission.ManageRoles;
        await dbContext.SaveChangesAsync(ct);
        Assert.NotNull(roleToEdit);

        var request = new PatchRoleRequest
        (
            RoleId: roleToEdit.Id,
            NewName: null,
            AddedPermissions: [Permission.ManageChannels],
            RemovedPermissions: [Permission.ReadOnChannel]
        );

        var json = await client.PatchAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, json.StatusCode);
    }
    [Fact]
    public async Task PatchRole_Returns403Forbidden_WhenTryingToEditEveryoneRoleName()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user =await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roleToEdit = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Everyone", ct);
        Assert.NotNull(roleToEdit);

        var request = new PatchRoleRequest
        (
            RoleId: roleToEdit.Id,
            NewName: "not_null",
            AddedPermissions: [Permission.ManageChannels],
            RemovedPermissions: [Permission.ReadOnChannel]
        );

        var json = await client.PatchAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, json.StatusCode);
    }
    [Fact]
    public async Task PatchRole_Returns403Forbidden_WhenUserEditRoleWithTheSamePriorityAsHisOwn()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user =await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roleToEdit = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Owner", ct);
        Assert.NotNull(roleToEdit);

        var request = new PatchRoleRequest
        (
            RoleId: roleToEdit.Id,
            NewName: null,
            AddedPermissions: [Permission.ManageChannels],
            RemovedPermissions: [Permission.ReadOnChannel]
        );

        var json = await client.PatchAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, json.StatusCode);
    }
    [Fact]
    public async Task PatchRole_Returns403Forbidden_WhenUserEditRoleWithLowerPriorityAsHisOwn()
    {
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user =await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roleToEdit = await dbContext.Roles.FirstOrDefaultAsync(r => r.ServerId == serverId && r.Name == "Owner", ct);
        Assert.NotNull(roleToEdit);

        var userMember = await dbContext.Members.Include(m => m.Roles).FirstOrDefaultAsync(m => m.UserId == user.Id && m.ServerId == serverId);
        Assert.NotNull(userMember);
        userMember.Roles.RemoveAll(r => r.Name == "Owner");
        userMember.Roles.FirstOrDefault(r => r.Name == "Everyone")!.Permissions |= Permission.ManageRoles;
        
        await dbContext.SaveChangesAsync(ct);
        var request = new PatchRoleRequest
        (
            RoleId: roleToEdit.Id,
            NewName: null,
            AddedPermissions: [Permission.ManageChannels],
            RemovedPermissions: [Permission.ReadOnChannel]
        );

        var json = await client.PatchAsJsonAsync($"/servers/{serverId}/roles", request, ct);
        Assert.Equal(HttpStatusCode.Forbidden, json.StatusCode);
    }
}
