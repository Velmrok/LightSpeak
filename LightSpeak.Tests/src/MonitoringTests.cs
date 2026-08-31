
using LightSpeak.AppHost.src.Constants;

namespace LightSpeak.Tests.src;

[Collection("Aspire")]
public class MonitoringTests : TestBase
{
    public MonitoringTests(AppFixture fixture) : base(fixture)
    {
    }
    [Fact]
    public async Task ComposeService_IsHealthy()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        await WaitForResourceRunningAsync(ResourcesNames.ComposeService, ct);

        var client = Fixture.CreateGatewayClient();
        var resp = await client.GetAsync("compose/health", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    [Fact]
    public async Task Gateway_IsHealthy()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);

        var client = Fixture.CreateGatewayClient();
        var resp = await client.GetAsync("health", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
     [Fact]
    public async Task ProfileService_IsHealthy()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.Gateway, ct);
        await WaitForResourceRunningAsync(ResourcesNames.ProfileService, ct);

        var client = Fixture.CreateGatewayClient();
        var resp = await client.GetAsync("profile/health", ct).WaitAsync(DefaultTimeout, ct);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    
    
}