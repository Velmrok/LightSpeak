using System.Text.RegularExpressions;
using Aspire.Hosting;
using HtmlAgilityPack;
using LightSpeak.Tests.src;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace LightSpeak.Tests;

[CollectionDefinition("Aspire")]
public class AspireCollectionDefinition : ICollectionFixture<AppFixture>{}

public partial class AppFixture : IAsyncLifetime
{
    public IConnection RabbitMqConnection { get; private set; } = null!;
    public DistributedApplication App = null!;
    public HttpClient CreateGatewayClient() => App.CreateHttpClient("gateway", "http");
    public async Task<TContext> CreateDbContextAsync<TContext>(string connectionStringName,CancellationToken ct = default) where TContext : DbContext
    {
        var connectionString = await App.GetConnectionStringAsync(connectionStringName, ct);
        var options = new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString)
            .Options;
        return (TContext)Activator.CreateInstance(typeof(TContext), options)!;
    }
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

        var connectionString = await App.GetConnectionStringAsync("rabbitmq");
        var factory = new ConnectionFactory { Uri = new Uri(connectionString!) };
        

        await App.ResourceNotifications.WaitForResourceAsync(
            "gateway", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);

        await App.ResourceNotifications.WaitForResourceAsync(
            "keycloak", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);

        RabbitMqConnection = await factory.CreateConnectionAsync();
    }


    public async Task DisposeAsync()
    {
        if(App is not null)
            await App.DisposeAsync();
        if(RabbitMqConnection is not null)
            await RabbitMqConnection.DisposeAsync();
    }
}