using System.Text.RegularExpressions;
using Aspire.Hosting;
using HtmlAgilityPack;
using LightSpeak.Tests.src;
using Microsoft.Extensions.Logging;

namespace LightSpeak.Tests;

public partial class AppFixture : IAsyncLifetime
{
    public DistributedApplication App = null!;
    public HttpClient CreateGatewayClient() => App.CreateHttpClient("gateway", "http");
    public async Task InitializeAsync()
    {
        CancellationToken ct = CancellationToken.None;
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LightSpeak_AppHost>(
            ["IsTesting=true"], ct);
        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("Aspire.Hosting", LogLevel.Warning);
            logging.AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.None);
            logging.AddFilter("HealthChecks", LogLevel.None);

        });
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                CookieContainer = AuthClient._cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true
            });
        });
        App = await builder.BuildAsync();
        await App.StartAsync();



        await App.ResourceNotifications.WaitForResourceAsync(
            "gateway", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);

        await App.ResourceNotifications.WaitForResourceAsync(
            "keycloak", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);
    }
    

    public async Task DisposeAsync() => await App.DisposeAsync();
}