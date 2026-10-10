using System.Net.Http.Json;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class GetRolesTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    [Fact]
    public async Task GetRoles_Returns200OK_WhenUserIsMember()
    {
        
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user =await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roles = await dbContext.Roles.Where(r => r.ServerId == serverId).ToListAsync(ct);

        var response = await client.GetAsync($"/servers/{serverId}/roles", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rolesResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GetRolesResponse>>(ct);
        Assert.NotNull(rolesResponse);
        Assert.NotNull(rolesResponse.Data);
        Assert.NotNull(rolesResponse.Data.Roles);
        Assert.Equal(roles.Count, rolesResponse.Data.Roles.Count);
    }
    [Fact]
    public async Task GetRoles_Returns404NotFound_WhenUserIsNotMember()
    {
        
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        _authClient.ResetCookies();
        await LoginOnFreshTestUserAsync(client,ct);

        var response = await client.GetAsync($"/servers/{serverId}/roles", ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task GetRoles_Returns404Unauthorized_WhenUserIsNotLoggedIn()
    {
        
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        _authClient.ResetCookies();

        var response = await client.GetAsync($"/servers/{serverId}/roles", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task GetRoles_ReturnsEmptyList_WhenServerHasNoRoles()
    {
        
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user =await LoginOnFreshTestUserAsync(client,ct);

        var serverData = await CreateServerAsync(client, serverName, ct);
        var serverId = serverData.ServerId;

        var dbContext = await CreateDbContext(ct);
        var roles = await dbContext.Roles.Where(r => r.ServerId == serverId).ToListAsync(ct);
        foreach (var role in roles)
        {
            dbContext.Roles.Remove(role);
        }
        await dbContext.SaveChangesAsync(ct);

        var response = await client.GetAsync($"/servers/{serverId}/roles", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rolesResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GetRolesResponse>>(ct);
        Assert.NotNull(rolesResponse);
        Assert.NotNull(rolesResponse.Data);
        Assert.NotNull(rolesResponse.Data.Roles);
        Assert.Empty(rolesResponse.Data.Roles);
    }
}
