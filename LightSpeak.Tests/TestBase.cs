using System.Net.Http.Json;
using Aspire.Hosting;
using Common.Dto;
using LightSpeak.Tests.src;
using ServersService.src.dto;

namespace LightSpeak.Tests;

public class TestBase : IAsyncLifetime
{
    protected AppFixture? Fixture;
    protected static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    protected DistributedApplication App => Fixture?.App ?? throw new InvalidOperationException("Fixture is not initialized");
    protected readonly CookieContainer Cookies = new();
    protected readonly AuthClient _authClient;

    public TestBase()
    {
       _authClient = new AuthClient(Cookies);
    }
    public async Task InitializeAsync() => Fixture = await SharedApp.GetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    protected HttpClient CreateClient() => Fixture.CreateGatewayClient(Cookies);
    protected async Task<TestUser> LoginOnFreshTestUserAsync(HttpClient browser, CancellationToken ct = default)
    {
       
        var testUser = await CreateUniqueTestUserAsync(ct);
        await _authClient.LoginAsync(browser, testUser.Username, testUser.Password, ct);
        return testUser;
    }
    protected async Task<TestUser> CreateUniqueTestUserAsync(CancellationToken ct = default)
    {
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        var password = "Password123!";
        var id = await Fixture.CreateTestUserAsync(username, email, password, ct);

        return new TestUser(id, username, email, password);
    }
    protected async Task<CreateServerResponse> CreateServerAsync(HttpClient client, string name, CancellationToken ct)
    {
        var request = new CreateServerRequest
        (
            Name: name
        );

        var json = await client.PostAsJsonAsync("/servers", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        var response = await ReadFromJson<CreateServerResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        return data;
    }
    protected async Task<CreateChannelResponse> CreateChannelAsync(HttpClient client, string serverId, string name, CancellationToken ct)
    {
        var request = new CreateChannelRequest
        (
            Name: name
        );

        var json = await client.PostAsJsonAsync($"/servers/{serverId}/channels", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        var response = await ReadFromJson<CreateChannelResponse>(json, ct);
        var data = response.Data;
        Assert.NotNull(data);
        Assert.NotNull(response.Errors);
        Assert.Empty(response.Errors);
        return data;
    }
    protected async Task WaitForResourceRunningAsync(string resourceName, CancellationToken ct)
    {
        await App.ResourceNotifications
            .WaitForResourceAsync(resourceName, KnownResourceStates.Running)
            .WaitAsync(DefaultTimeout, ct);

    }
    protected async Task<ApiResponse<T>> ReadFromJson<T>(HttpResponseMessage json, CancellationToken ct = default)
    {
        var response = await json.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
        Assert.NotNull(response);
        return response;
    }


}

public record TestUser(string Id, string Username, string Email, string Password) { }