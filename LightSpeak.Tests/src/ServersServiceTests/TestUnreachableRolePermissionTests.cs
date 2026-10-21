namespace LightSpeak.Tests.src.ServersServiceTests;

public class TestUnreachableRolePermissionTests : TestBase
{
    [Fact]
    public async Task TestUnreachableRolePermission_Returns403_WhenLoggedIn()
    {

        var ct = CancellationToken.None;
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var json = await client.GetAsync($"api/servers/test-unreachable-role-permission", ct);
        Assert.Equal(HttpStatusCode.Forbidden, json.StatusCode);
    }
    [Fact]
    public async Task TestUnreachableRolePermission_Returns401_WhenNotLoggedIn()
    {

        var ct = CancellationToken.None;
        var client = CreateClient();

        var json = await client.GetAsync($"api/servers/test-unreachable-role-permission", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, json.StatusCode);
    }
}
