using Aspire.Hosting;
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
    protected async Task<CancellationToken> WaitForResourceRunningAsync(string resourceName)
    {
        var ct = CancellationToken.None;
        await App.ResourceNotifications
            .WaitForResourceAsync(resourceName, KnownResourceStates.Running)
            .WaitAsync(DefaultTimeout, ct);
        return ct;
    }

}