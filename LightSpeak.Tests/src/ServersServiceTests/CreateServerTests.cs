using System.Net.Http.Json;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class CreateServerTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    [Fact]
    public async Task CreatesServer_Correctly_OnCreateServerRequest_WhileBeingLoggedIn()
    {
        var ct = CancellationToken.None;
        var client = CreateClient();
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

        var client = CreateClient();

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

        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);
        var request = new CreateServerRequest
        (
            Name: ""
        );

        var json = await client.PostAsJsonAsync("/servers", request, ct);
        Assert.Equal(HttpStatusCode.BadRequest, json.StatusCode);
    }
}
