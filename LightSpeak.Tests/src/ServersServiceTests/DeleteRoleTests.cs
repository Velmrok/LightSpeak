using System.Net.Http.Json;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;
using ServersService.src.permissions;
using ServersService.src.models;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class DeleteRoleTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    private async Task<Role> AddTestRole(AppDbContext dbContext, string serverId, string roleName, int priority, Permission permissions, CancellationToken ct)
    {
      
        var testRole = new Role
        {
            ServerId = serverId,
            Name = roleName,
            Priority = priority,
            Permissions = permissions
        };
        await dbContext.Roles.AddAsync(testRole, ct);
        return testRole;
    }
    
    [Fact]
    public async Task DeleteRole_ReturnsNoContent_WhenValidDataIsProvided_AndUserHasPermission()
    {
        
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;
        var dbContext = await CreateDbContext(ct);
        var testRole = await AddTestRole(dbContext,serverId, "Test Role", 1, Permission.ManageChannels, ct);
        await dbContext.SaveChangesAsync(ct);

        var response = await client.DeleteAsync($"/servers/{serverId}/roles/{testRole.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync(ct);
        Assert.Empty(body);
        var roleInDb = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == testRole.Id, ct);
        Assert.Null(roleInDb);
    }
    [Fact]
    public async Task DeleteRole_Returns403Forbidden_WhenUserDoesNotHaveManageRolesPermission()
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
         var testRole = await AddTestRole(dbContext, serverId, "Test Role", 1, Permission.ManageChannels, ct);
        await dbContext.SaveChangesAsync(ct);

       
        var response = await client.DeleteAsync($"/servers/{serverId}/roles/{testRole.Id}", ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    }
    [Fact]
    public async Task DeleteRole_Returns403Forbidden_WhenUserDeletesRoleWithLowerPriority()
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
        everyoneRole.Priority = 2;

         var testRole = await AddTestRole(dbContext, serverId, "Test Role", 1, Permission.ManageChannels, ct);
        await dbContext.SaveChangesAsync(ct);

        var response = await client.DeleteAsync($"/servers/{serverId}/roles/{testRole.Id}", ct);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    }
}
