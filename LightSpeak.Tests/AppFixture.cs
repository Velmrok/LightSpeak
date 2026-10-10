using System.Text.RegularExpressions;
using Aspire.Hosting;
using HtmlAgilityPack;
using LightSpeak.AppHost.src.Constants;
using LightSpeak.Tests.src;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using RabbitMQ.Client;

namespace LightSpeak.Tests;

public partial class AppFixture : IAsyncLifetime
{
    public IConnection RabbitMqConnection { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;
    public DistributedApplication App = null!;
    public HttpClient CreateGatewayClient(CookieContainer? cookies)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies ?? new CookieContainer(),
            UseCookies = true,
            AllowAutoRedirect = true
        };

        return new HttpClient(handler)
        {
            BaseAddress = App.GetEndpoint(ResourcesNames.Gateway, "http")
        };
    }
    public async Task<TContext> CreateDbContextAsync<TContext>(string connectionStringName, CancellationToken ct = default) where TContext : DbContext
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
        Configuration = builder.Configuration;
        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("LightSpeak.AppHost.Resources", LogLevel.Warning);
            logging.AddFilter("Aspire.Hosting", LogLevel.Warning);
            logging.AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.None);
            logging.AddFilter("HealthChecks", LogLevel.None);
            logging.AddFilter("LightSpeak.AppHost.Resources.postgres", LogLevel.None);

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
        
    }
    private async Task AssertTestUserHasBeenCreatedAsync(string id, string username, CancellationToken ct)
    {
        await Eventually.Assert(async () =>
        {
            await using var dbContext = await CreateDbContextAsync<ProfileService.src.database.AppDbContext>(ResourcesNames.ProfileDatabase, ct);
            var profile = await dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);
            Assert.NotNull(profile);
            Assert.Equal(username, profile.Username);
        }, TimeSpan.FromSeconds(30), ct);
    }

    public async Task<string> CreateTestUserAsync(string name, string email, string password,  CancellationToken ct = default)
    {
        var kcAdminSecret = Configuration["Parameters:kc-admin-secret"]!;
        var keycloakHttpClient = App.CreateHttpClient(ResourcesNames.Keycloak, "http");
        var keycloakClient = new KeycloakAdminClient(keycloakHttpClient, "lightspeak", kcAdminSecret);

        var id = await keycloakClient.CreateUserAsync(name, email, password, ct);
        await AssertTestUserHasBeenCreatedAsync(id, name, ct);
        return id;


    }
    public async Task DisposeAsync()
    {
        if(App is not null)
            await App.DisposeAsync();
        if(RabbitMqConnection is not null)
            await RabbitMqConnection.DisposeAsync();
    }
}