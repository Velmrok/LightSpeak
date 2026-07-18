using System.Text.RegularExpressions;
using Aspire.Hosting;

namespace LightSpeak.Tests;

public partial class AppFixture : IAsyncLifetime
{
    public DistributedApplication App = null!;

    public async Task InitializeAsync()
    {
        CancellationToken ct = CancellationToken.None;
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LightSpeak_AppHost>();
        App = await builder.BuildAsync();
        await App.StartAsync();

        await App.ResourceNotifications.WaitForResourceAsync(
            "gateway", KnownResourceStates.Running).WaitAsync(TimeSpan.FromMinutes(1), ct);
    }
    static async Task LoginAsync(HttpClient browser, string user, string pass)
    {
        var loginPage = await browser.GetAsync("/login");
        var html = await loginPage.Content.ReadAsStringAsync();

        var action = WebUtility.HtmlDecode(ActionRegex().Match(html).Groups[1].Value);

        var afterLogin = await browser.PostAsync(action, new FormUrlEncodedContent(
            new Dictionary<string, string> { ["username"] = user, ["password"] = pass }));

        afterLogin.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync() => await App.DisposeAsync();

    [GeneratedRegex("action=\"([^\"]+)\"")]
    private static partial Regex ActionRegex();
}