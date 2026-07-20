using System.Text.RegularExpressions;
using Aspire.Hosting;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace LightSpeak.Tests;

public partial class AppFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    public DistributedApplication App = null!;
    public HttpClient CreateGatewayClient() => App.CreateHttpClient("gateway", "http");

    private readonly static CookieContainer _cookieContainer = new();

    public async Task InitializeAsync()
    {
        CancellationToken ct = CancellationToken.None;
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LightSpeak_AppHost>(
            ["Testing=true"], ct);
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
                CookieContainer = _cookieContainer,
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
    public static async Task<HttpResponseMessage> LoginAsync
    (HttpClient browser, string user, string pass, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        var loginPage = await browser.GetAsync("/login", ct).WaitAsync(timeout ?? DefaultTimeout, ct);

        foreach(Cookie cookie in _cookieContainer.GetAllCookies())
        {
            cookie.Secure = false;
        }
        var html = await loginPage.Content.ReadAsStringAsync(ct);

        if (!loginPage.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"GET /login returned {(int)loginPage.StatusCode} {loginPage.ReasonPhrase}. Body: {html}");
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var form = doc.DocumentNode.SelectSingleNode("//form") ?? throw new InvalidOperationException($"Login page did not contain a form. Body: {html}");

        var actionUrl = form.GetAttributeValue("action", "");
        if (string.IsNullOrWhiteSpace(actionUrl))
        {
            throw new InvalidOperationException($"Login form did not contain an action attribute. Body: {html}");
        }

        actionUrl = HtmlEntity.DeEntitize(actionUrl);

        var postUri = new Uri(browser.BaseAddress!, actionUrl);

        var fields = form.SelectNodes(".//input[@name]")?
            .ToDictionary(
                node => node.GetAttributeValue("name", ""),
                node => node.GetAttributeValue("value", ""))
            ?? new Dictionary<string, string>();

        fields["username"] = user;
        fields["password"] = pass;

        
        var afterLogin = await browser.PostAsync(postUri, new FormUrlEncodedContent(fields), ct).WaitAsync(timeout ?? DefaultTimeout, ct);

        if (!afterLogin.IsSuccessStatusCode)
        {
            var responseBody = await afterLogin.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"POST {postUri} returned {(int)afterLogin.StatusCode} {afterLogin.ReasonPhrase}. Body: {responseBody}");
        }

        return afterLogin;
    }

    public async Task DisposeAsync() => await App.DisposeAsync();
}