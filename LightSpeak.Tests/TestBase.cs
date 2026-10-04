using System.Net.Http.Json;
using Aspire.Hosting;
using Common.Dto;
using LightSpeak.Tests.src;
using ServersService.src.dto;

namespace LightSpeak.Tests;

public class TestBase
{
    protected readonly AppFixture Fixture;
    protected static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    protected DistributedApplication App => Fixture.App;
    protected AuthClient _authClient => new();

    public TestBase(AppFixture fixture)
    {
        Fixture = fixture;
        _authClient.ResetCookies();
    }
    protected async Task<TestUser> LoginOnFreshTestUserAsync(HttpClient browser, CancellationToken ct = default)
    {
        var authClient = new AuthClient();
        var testUser = await CreateUniqueTestUserAsync(ct);
        await authClient.LoginAsync(browser, testUser.Username, testUser.Password, ct);
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