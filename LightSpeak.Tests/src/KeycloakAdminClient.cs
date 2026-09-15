using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LightSpeak.Tests.src;
public class KeycloakAdminClient
{
    private readonly HttpClient _http;
    private readonly string _realm;
    private readonly string _kcAdminSecret;

    public KeycloakAdminClient(HttpClient http, string realm, string kcAdminSecret)
    {
        _http = http;
        _realm = realm;
        _kcAdminSecret = kcAdminSecret;
    }

    public async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
     

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "backend-admin",
            ["client_secret"] = _kcAdminSecret
        };

        var resp = await _http.PostAsync(
            $"auth/realms/{_realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(form), ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("access_token").GetString()!;
    }

    public async Task<string> CreateUserAsync(string username, string email, string password, CancellationToken ct)
    {
        var token = await GetAdminTokenAsync(ct);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            username,
            email,
            enabled = true,
            emailVerified = true,
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            }
        };

        var resp = await _http.PostAsJsonAsync($"auth/admin/realms/{_realm}/users", payload, ct);
        
        resp.EnsureSuccessStatusCode();
        
        
        var location = resp.Headers.Location!.ToString();
        var userId = location.Split('/').Last();
        return userId;
    }
}