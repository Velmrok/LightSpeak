using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Aspire.Hosting;
using Debug;
using LightSpeak.AppHost.src.Constants;
using Microsoft.Extensions.Logging;

namespace LightSpeak.Tests.src;

public class AuthTest : TestBase
{


    [Fact] 
    public async Task Me_Returns401_OnMissingSession()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);

        var client = CreateClient();

        var resp = await client.GetAsync("/users/me",ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task Login_Returns200_OnValidCredentials()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();
        var user = await CreateUniqueTestUserAsync(ct);
        
        var resp = await _authClient.LoginAsync(client, user.Username, user.Password, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns200_OnValidSession()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);

        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);

        var resp = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Me_Returns401_AfterLogout()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();
         await LoginOnFreshTestUserAsync(client,ct);
           
        var resp = await client.PostAsync("/logout", null, ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var resp2 = await client.GetAsync("/users/me", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp2.StatusCode);

    }
    [Fact]
    public async Task Token_IsValid_AfterLogin()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();

         await LoginOnFreshTestUserAsync(client,ct);
        
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
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();

         await LoginOnFreshTestUserAsync(client,ct);
        
        var resp = await client.GetAsync("/debug/auth-token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Auth_Returns401_OnInvalidToken()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

        var resp = await client.GetAsync("/debug/auth-token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task Auth_Returns401_OnMissingToken()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();

        var resp = await client.GetAsync("/debug/auth-token", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // Test grpc auth handling via debug service
    [Fact]
    public async Task GrpcAuthCheck_Returns401_OnMissingToken()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();

        var resp = await client.GetAsync("/debug/grpc-auth-check", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
    [Fact]
    public async Task GrpcAuthCheck_Returns200_AfterLogin()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        var client = CreateClient();
        
         await LoginOnFreshTestUserAsync(client,ct);

        var resp = await client.GetAsync("/debug/grpc-auth-check", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

}