using Aspire.Hosting;
using Common.Dto;
using JasperFx.Events.Documents;
using LightSpeak.AppHost.src.Constants;
using ProfileService.src.database;
using ProfileService.src.services;

namespace LightSpeak.Tests.src;
[Collection("Aspire")]
public class ProfileServiceTest : TestBase
{
   
   
    public ProfileServiceTest(AppFixture fixture) : base(fixture)
    {
       
    }

    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ProfileDatabase, ct);
    }

    [Fact]
    public async Task CreatesProfile_Correctly_OnUserRegistrationEvent()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.ProfileService, ct);

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
            }, TimeSpan.FromSeconds(15) , ct);

    }
    [Fact]
    public async Task CreatesProfile_Correctly_WithDoubleEventSent()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.ProfileService, ct);

        var id = Guid.NewGuid().ToString();
        var email = id + "@test.com";
        var username = id + "user";
        await RabbitMqTestHelper.PublishRegisterEventAsync(Fixture.RabbitMqConnection, id, username, email,ct);
        await RabbitMqTestHelper.PublishRegisterEventAsync(Fixture.RabbitMqConnection, id, username, email,ct);

        await Eventually.Assert(
            async () =>
            {
                var dbContext = await CreateDbContext(ct);
                var profile = dbContext.Profiles.FirstOrDefault(p => p.Id == id);
                Assert.NotNull(profile);
                Assert.Equal(username, profile.Username);
                Assert.Equal(email, profile.Email);
            }, TimeSpan.FromSeconds(15) , ct);

    }
    [Fact]
    public async Task CreateProfile_CreatesRecordInDatabase()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.ProfileService, ct);
        await WaitForResourceRunningAsync(ResourcesNames.ProfileDatabase, ct);
        var id = Guid.NewGuid().ToString();
        var email = id + "@test.com";
        var username = id + "user";

        var profile = new Profile
        {
            Id = id,
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        
        var profileService = new ProfileApplicationService(await CreateDbContext(ct));
        var result = await profileService.CreateProfileAsync(profile, ct);
        Assert.True(result.IsSuccess);

        var dbContext = await CreateDbContext(ct);
        var createdProfile = dbContext.Profiles.FirstOrDefault(p => p.Id == id);
        Assert.NotNull(createdProfile);
        Assert.Equal(username, createdProfile.Username);
        Assert.Equal(email, createdProfile.Email);
    }
    [Fact]
    public async Task CreateProfile_ReturnsError_WhenProfileAlreadyExists()
    {
        var ct = CancellationToken.None;
        await WaitForResourceRunningAsync(ResourcesNames.ProfileService, ct);
        await WaitForResourceRunningAsync(ResourcesNames.ProfileDatabase, ct);
        var id = Guid.NewGuid().ToString();
        var email = id + "@test.com";
        var username = id + "user";

        var profile = new Profile
        {
            Id = id,
            Username = username,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        
        var profileService = new ProfileApplicationService(await CreateDbContext(ct));
        var result1 = await profileService.CreateProfileAsync(profile, ct);
        Assert.True(result1.IsSuccess);

        var result2 = await profileService.CreateProfileAsync(profile, ct);
        Assert.False(result2.IsSuccess);
    }
    
}