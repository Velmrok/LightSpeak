using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using Microsoft.EntityFrameworkCore;
using Common.Dto.Events;

namespace LightSpeak.Tests.src.ServersServiceTests;

public class UserChangedDataHandlerTests : TestBase
{
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    
    [Fact]
    public async Task UserChangedDataHandler_UpdatesUserDataInDatabase_Correctly_OnUserChangedDataEvent()
    {
 
        var serverName = "Test Server";
        var ct = CancellationToken.None;
        var client = CreateClient();
        var user = await LoginOnFreshTestUserAsync(client,ct);

        await CreateServerAsync(client, serverName, ct); // Create a server to ensure the userSnapshot exists in the database
        await Eventually.Assert(
            async () =>
            {
                var dbContext = await CreateDbContext(ct);
                var userSnapshot = await dbContext.UserSnapshots.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id, ct);
                Assert.NotNull(userSnapshot);
            }, TimeSpan.FromSeconds(15), ct);

        var evt = new UserDataChangedEvent(
            UserId: user.Id,
            Username: "UpdatedUsername",
            AvatarUrl: "https://example.com/avatar.jpg"
        );
    
        await RabbitMqTestHelper.PublishEventAsync(Fixture.RabbitMqConnection, UserDataChangedEvent.RoutingKey, evt, ct);

        await Eventually.Assert(
            async () =>
            {
                var dbContext = await CreateDbContext(ct);
                var userSnapshot = await dbContext.UserSnapshots.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id, ct);
                Assert.NotNull(userSnapshot);
                Assert.Equal(evt.Username, userSnapshot.Name);
                Assert.Equal(evt.AvatarUrl, userSnapshot.AvatarUrl);
            }, TimeSpan.FromSeconds(15), ct);
    }
}
