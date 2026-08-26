using Aspire.Hosting;
using Common.Dto;
using JasperFx.Events.Documents;
using ProfileService.src.database;

namespace LightSpeak.Tests.src;
[Collection("Aspire")]
public class ProfileServiceTest : TestBase
{
    
   
    public ProfileServiceTest(AppFixture fixture) : base(fixture)
    {
    }

    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>("profile-database", ct);
    }

    [Fact]
    public async Task CreatesProfile_Correctly_OnUserRegistrationEvent()
    {
        var ct = await WaitForResourceRunningAsync("gateway");
      

        var id = Guid.NewGuid().ToString();
        var email = id + "@test.com";
        var username = id + "user";
        await RabbitMqTestHelper.PublishRegisterEventAsync(Fixture.RabbitMqConnection, id, username, email,ct);

        await Eventually.Assert(
            async () =>
            {
                var dbContext = await CreateDbContext(ct);
                var profile = dbContext.Profiles.FirstOrDefault(p => p.Id == id);
                Assert.NotNull(profile);
                Assert.Equal(username, profile.Username);
                Assert.Equal(email, profile.Email);
            }, TimeSpan.FromSeconds(60) , ct);

    }
    
}