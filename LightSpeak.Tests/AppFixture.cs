using System.Text.RegularExpressions;
using Aspire.Hosting;
using HtmlAgilityPack;
using LightSpeak.AppHost.src.Constants;
using LightSpeak.Tests.src;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
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
                CookieContainer = AuthClient.CookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true
            });
        });

        var kcAdminSecret = builder.Configuration["Parameters:kc-admin-secret"];
        ArgumentException.ThrowIfNullOrEmpty(kcAdminSecret, "kc-admin-secret parameter is not set in appsettings.json");

        App = await builder.BuildAsync();
        await App.StartAsync();

        var connectionString = await App.GetConnectionStringAsync(ResourcesNames.RabbitMQ, ct);
        var factory = new ConnectionFactory { Uri = new Uri(connectionString!) };
        

        await App.ResourceNotifications.WaitForResourceAsync(
            "gateway", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);

        await App.ResourceNotifications.WaitForResourceAsync(
            "keycloak", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);

        await App.ResourceNotifications.WaitForResourceAsync(
            ResourcesNames.Postgres, KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);

        RabbitMqConnection = await factory.CreateConnectionAsync();
        
        var authClient = new AuthClient();
        var testUserId = await authClient.CreateTestUserAsync(App.CreateHttpClient(ResourcesNames.Keycloak, "http"),kcAdminSecret, ct);
        await AssertTestUserHasBeenCreatedAsync(testUserId, ct);
    }

    // BAD not OCP code, no idea for now how to handle it better
    private async Task AssertTestUserHasBeenCreatedAsync(string id, CancellationToken ct)
    {
        await Eventually.Assert(async () =>
        {
            var dbContext = await CreateDbContextAsync<ProfileService.src.database.AppDbContext>(ResourcesNames.ProfileDatabase, ct);
            var profile = await dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);
            Assert.NotNull(profile);
            Assert.Equal(AuthClient.testUserName, profile.Username);
        }, TimeSpan.FromSeconds(15), ct);
    }


    public async Task DisposeAsync()
    {
        if(App is not null)
            await App.DisposeAsync();
        if(RabbitMqConnection is not null)
            await RabbitMqConnection.DisposeAsync();
    }
}