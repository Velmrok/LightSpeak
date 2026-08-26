using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Aspire.Hosting;
using Debug;
using Microsoft.Extensions.Logging;

namespace LightSpeak.Tests.src;
[Collection("Aspire")]
public class AuthTest : TestBase
{
    public AuthTest(AppFixture fixture) : base(fixture)
    {
    }
    

    [Fact] 
    public async Task Me_Returns401_OnMissingSession()
    {
        var ct = await WaitForResourceRunningAsync("gateway");

        var client = Fixture.CreateGatewayClient();

        var resp = await client.GetAsync("/users/me",ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task Login_Returns200_OnValidCredentials()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();
  
        
        var resp = await _authClient.LoginAsync(client, DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns200_OnValidSession()
    {
        var ct = await WaitForResourceRunningAsync("gateway");

        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);

        var resp = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns401_AfterLogout()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();
        await _authClient.LoginAsync(client, DefaultTimeout, ct);
           
        var resp = await client.PostAsync("/logout", null, ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var resp2 = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp2.StatusCode);

    }
    [Fact]
    public async Task Token_IsValid_AfterLogin()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();

        await _authClient.LoginAsync(client, DefaultTimeout, ct);
        
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
    [Fact]
    public async Task Token_isCorrectlyAuthenticated_AfterLogin()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();

        await _authClient.LoginAsync(client, DefaultTimeout, ct);
        
        var resp = await client.GetAsync("/debug/auth-token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Auth_Returns401_OnInvalidToken()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var resp = await client.GetAsync("/debug/auth-token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task Auth_Returns401_OnMissingToken()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();

        var resp = await client.GetAsync("/debug/auth-token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // Test grpc auth handling via debug service
    [Fact]
    public async Task GrpcAuthCheck_Returns401_OnMissingToken()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();

        var resp = await client.GetAsync("/debug/grpc-auth-check", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task GrpcAuthCheck_Returns200_AfterLogin()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
        var client = Fixture.CreateGatewayClient();
        
        await _authClient.LoginAsync(client, DefaultTimeout, ct);

        var resp = await client.GetAsync("/debug/grpc-auth-check", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

}