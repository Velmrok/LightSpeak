
using HtmlAgilityPack;

namespace LightSpeak.Tests;

public class AuthClient
{
    public const string testUserName = "testuser";
    public const string testUserPassword = "testuser";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    public static CookieContainer _cookieContainer { get; } = new();

    public void ResetCookies()
    {
        foreach (Cookie cookie in _cookieContainer.GetAllCookies())
        {
            cookie.Expired = true;
        }
    }
    public async Task<HttpResponseMessage> LoginAsync
    (HttpClient browser, TimeSpan? timeout = null, CancellationToken ct = default, string user = testUserName, string pass = testUserPassword)
    {
        var loginPage = await browser.GetAsync("/login", ct).WaitAsync(timeout ?? DefaultTimeout, ct);

        foreach (Cookie cookie in _cookieContainer.GetAllCookies())
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
            ?? [];

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
}