using System.Net.Http.Json;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class GetServerDetailsTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    [Fact]
    public async Task GetsServerDetails_Correctly_OnGetServerDetailsRequest_WhileBeingMember()
    {
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);
        var server = await CreateServerAsync(client, "Test Server", ct);

        var response = await client.GetAsync($"api/servers/{server.ServerId}", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resp = await ReadFromJson<GetServerDetailsResponse>(response, ct);
        var data = resp.Data;
        Assert.NotNull(data);
        Assert.Equal(server.ServerId, data.ServerId);
        Assert.Equal(server.Name, data.Name);
    }
    [Fact]
    public async Task GetsServerDetails_ReturnsChannelsSummaries_Correctly_OnGetServerDetailsRequest_WhileBeingMember()
    {
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);
        var server = await CreateServerAsync(client, "Test Server", ct);
        await CreateChannelAsync(client, server.ServerId, "Test Channel", ct);
        await CreateChannelAsync(client, server.ServerId, "Test Channel 2", ct);

        var response = await client.GetAsync($"api/servers/{server.ServerId}", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resp = await ReadFromJson<GetServerDetailsResponse>(response, ct);
        var data = resp.Data;
        Assert.NotNull(data);
        Assert.Equal(server.ServerId, data.ServerId);
        Assert.Equal(server.Name, data.Name);
        Assert.Equal(2, data.Channels.Count);
        Assert.Contains(data.Channels, c => c.Name == "Test Channel");
        Assert.Contains(data.Channels, c => c.Name == "Test Channel 2");
    }
    [Fact]
    public async Task GetsServerDetails_ReturnsMembersSummaries_Correctly_OnGetServerDetailsRequest_WhileBeingMember()
    {
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);
        var server = await CreateServerAsync(client, "Test Server", ct);
       

        var response = await client.GetAsync($"api/servers/{server.ServerId}", ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resp = await ReadFromJson<GetServerDetailsResponse>(response, ct);
        var data = resp.Data;
        Assert.NotNull(data);
        Assert.Equal(server.ServerId, data.ServerId);
        Assert.Equal(server.Name, data.Name);
        Assert.Single(data.Members);
        var member = data.Members.First();
        Assert.Equal(user.Id, member.Id);
    }
}