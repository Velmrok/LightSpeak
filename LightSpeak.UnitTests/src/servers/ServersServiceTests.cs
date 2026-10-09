using ServersService.src.permissions;
using ServersService.src.services;

namespace LightSpeak.UnitTests.src.servers;

[Collection("db")]
public class ServersServiceTests(PostgresFixture fx) : IAsyncLifetime
{
    public Task InitializeAsync() => fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    [Fact]
    public async Task PermissionFiltering_FiltersCorrectly_WithoutOverwrites()
    {
        var builder = new ServerBuilder().AddMember("user1");
        var channel = builder.AddChannel();

        await using (var arrange = fx.CreateDb())
            await builder.SaveAsync(arrange);

        await using var db = fx.CreateDb();
        var result = await new PermissionService(db)
            .FilterChannelMembersWithPermissionAsync(channel.Id, Permission.ReadOnChannel, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("user1", result.Data);
    }
    [Fact]
    public async Task PermissionFiltering_FiltersCorrectly_WithOverwrites()
    {
        var builder = new ServerBuilder()
            .EveryonePermissions(Permission.None)
            .AddMember("user1")
            .AddMember("user2");
        var channel = builder.AddChannel()
                    .AddMemberOverwrite("user1", Permission.ReadOnChannel, Permission.None);
    
        await using (var arrange = fx.CreateDb())
            await builder.SaveAsync(arrange);

        await using var db = fx.CreateDb();
        var result = await new PermissionService(db)
            .FilterChannelMembersWithPermissionAsync(channel.Id, Permission.ReadOnChannel, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("user1", result.Data);
        Assert.DoesNotContain("user2", result.Data);
    }
}