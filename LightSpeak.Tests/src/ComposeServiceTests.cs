using ComposeService.src.dto;

namespace LightSpeak.Tests.src;

public class ComposeServiceTests : TestBase
{
    [Fact]
    public async Task GetHome_Returns200Ok_OnFreshAccount()
    {
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);

        var response = await client.GetAsync("api/home", ct);
        response.EnsureSuccessStatusCode();

        var json = await ReadFromJson<HomeResponse>(response);

        Assert.NotNull(json);
        Assert.NotNull(json.Data);

        Assert.NotNull(json.Data.UserProfile);
        Assert.Equal(user.Id, json.Data.UserProfile.UserId);
        
        Assert.NotNull(json.Data.Servers);
        Assert.Empty(json.Data.Servers);

        Assert.NotNull(json.Data.Friends);
    }
}