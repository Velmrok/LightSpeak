using Testcontainers.PostgreSql;
using Respawn;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using ServersService.src;
namespace LightSpeak.UnitTests.src;

[CollectionDefinition("db")]
public class DbCollection : ICollectionFixture<PostgresFixture>;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =new PostgreSqlBuilder("postgres:18.3").Build();

    private Respawner _respawner = default!;
    public string ConnectionString => _container.GetConnectionString();

    public AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDb();
        await db.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetAsync() 
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await _respawner.ResetAsync(conn);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}