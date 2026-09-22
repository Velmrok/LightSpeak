using System.Net.Http.Json;
using Aspire.Hosting;
using Common.Dto;
using LightSpeak.Tests.src;

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
    protected async Task WaitForResourceRunningAsync(string resourceName, CancellationToken ct)
    {
        await App.ResourceNotifications
            .WaitForResourceAsync(resourceName, KnownResourceStates.Running)
            .WaitAsync(DefaultTimeout, ct);
    
    }
    protected async Task<ApiResponse<T>> ReadFromJson<T>(HttpResponseMessage json, CancellationToken ct=default)
    {
        var response = await json.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
        Assert.NotNull(response);
        return response;
    }

}