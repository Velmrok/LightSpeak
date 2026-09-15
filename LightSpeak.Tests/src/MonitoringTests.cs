
using LightSpeak.AppHost.src.Constants;

namespace LightSpeak.Tests.src;

[Collection("Aspire")]
public class MonitoringTests : TestBase
{
    public MonitoringTests(AppFixture fixture) : base(fixture)
    {
    }
    private async Task AssertResourceIsHealthyAsync(string resourceName, string healthEndpoint, CancellationToken ct)
    {
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        await WaitForResourceRunningAsync(resourceName, ct);

        var client = Fixture.CreateGatewayClient();
        var resp = await client.GetAsync(healthEndpoint, ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task ComposeService_IsHealthy()
    {
        var ct = CancellationToken.None;
        await AssertResourceIsHealthyAsync(ResourcesNames.ComposeService, "compose/health", ct);
    }
    [Fact]
    public async Task Gateway_IsHealthy()
    {
        var ct = CancellationToken.None;
        await AssertResourceIsHealthyAsync(ResourcesNames.Gateway, "health", ct);
    }
     [Fact]
    public async Task ProfileService_IsHealthy()
    {
        var ct = CancellationToken.None;
        await AssertResourceIsHealthyAsync(ResourcesNames.ProfileService, "profile/health", ct);
    }
    [Fact]
    public async Task ServersService_IsHealthy()
    {
        var ct = CancellationToken.None;
        await AssertResourceIsHealthyAsync(ResourcesNames.ServersService, "servers/health", ct);
    }

    
    
}