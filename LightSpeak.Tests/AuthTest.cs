using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Aspire.Hosting;
using Debug;
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
  
        
        var resp = await AppFixture.LoginAsync(client, DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns200_OnValidSession()
    {
        var ct = await WaitForResourceRunningAsync("gateway");

        var client = fx.CreateGatewayClient();
        await AppFixture.LoginAsync(client, DefaultTimeout, ct);

        var resp = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns401_AfterLogout()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = fx.CreateGatewayClient();
        await AppFixture.LoginAsync(client, DefaultTimeout, ct);
           
        var resp = await client.PostAsync("/logout", null, ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var resp2 = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp2.StatusCode);

    }
    [Fact]
    public async Task Token_IsValid_AfterLogin()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = fx.CreateGatewayClient();

        await AppFixture.LoginAsync(client, DefaultTimeout, ct);
        
        var resp = await client.GetAsync("/debug/token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var tokenResponse = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);

    
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.Jwt);
        Assert.NotEmpty(tokenResponse.Jwt);
        var handler = new JwtSecurityTokenHandler();
        var token = tokenResponse.Jwt;
        Assert.True(handler.CanReadToken(token));

    }

}