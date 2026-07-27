using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace LightSpeak.Tests;

public class AuthTest(AppFixture fx) : IClassFixture<AppFixture>
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    private DistributedApplication App => fx.App;

    private async Task<CancellationToken> WaitForResourceRunningAsync(string resourceName)
    {
        var ct = CancellationToken.None;
        await App.ResourceNotifications
            .WaitForResourceAsync(resourceName, KnownResourceStates.Running)
            .WaitAsync(DefaultTimeout, ct);
        return ct;
    }

    [Fact] 
    public async Task Me_Returns401_OnMissingSession()
    {
        var ct = await WaitForResourceRunningAsync("gateway");

        var client = fx.CreateGatewayClient();

        var resp = await client.GetAsync("/users/me",ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task Login_Returns200_OnValidCredentials()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = fx.CreateGatewayClient();
  
        
        var resp = await AppFixture.LoginAsync(client, AppFixture.testUserName, AppFixture.testUserPassword).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns200_OnValidSession()
    {
        var ct = await WaitForResourceRunningAsync("gateway");

        var client = fx.CreateGatewayClient();
        await AppFixture.LoginAsync(client, AppFixture.testUserName, AppFixture.testUserPassword, DefaultTimeout, ct).WaitAsync(DefaultTimeout, ct);

        var resp = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns401_AfterLogout()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = fx.CreateGatewayClient();
        await AppFixture.LoginAsync(client, AppFixture.testUserName, AppFixture.testUserPassword, DefaultTimeout, ct)
            .WaitAsync(DefaultTimeout, ct);
        var resp = await client.PostAsync("/logout", null, ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var resp2 = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp2.StatusCode);

    }

}