
using HtmlAgilityPack;

namespace LightSpeak.Tests.src;

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
    private void DisableSecureCookies()
    {
        foreach (Cookie cookie in _cookieContainer.GetAllCookies())
        {
            cookie.Secure = false;

        }
    }
    private async Task<HttpResponseMessage> GetLoginPageAsync(HttpClient browser,TimeSpan? timeout,CancellationToken ct)
    {
        var response = await browser
            .GetAsync("/login", ct)
            .WaitAsync(timeout ?? DefaultTimeout, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            throw new InvalidOperationException(
                $"GET /login returned {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. Body: {body}");
        }

        return response;
    }
    public async Task<HttpResponseMessage> LoginAsync
    (HttpClient browser, TimeSpan? timeout = null, CancellationToken ct = default, string user = testUserName, string pass = testUserPassword)
    {
        var loginPage = await GetLoginPageAsync(browser, timeout, ct);

        DisableSecureCookies();

        var html = await loginPage.Content.ReadAsStringAsync(ct);
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