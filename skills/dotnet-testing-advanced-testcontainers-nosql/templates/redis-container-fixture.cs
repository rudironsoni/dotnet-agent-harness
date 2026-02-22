using StackExchange.Redis;
using Testcontainers.Redis;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// Redis Container Fixture - Uses Collection Fixture pattern for container sharing
/// Supports Redis 7.x features, including all five data structure tests
/// </summary>
public class RedisContainerFixture : IAsyncLifetime
{
    private RedisContainer? _container;

    /// <summary>
    /// Redis connection multiplexer - for connection pool management
    /// </summary>
    public IConnectionMultiplexer Connection { get; private set; } = null!;

    /// <summary>
    /// Redis database instance - for executing commands
    /// </summary>
    public IDatabase Database { get; private set; } = null!;

    /// <summary>
    /// Redis connection string
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Starts Redis container when test collection begins
    /// </summary>
    public async Task InitializeAsync()
    {
        // Use Redis 7.2 for latest features
        _container = new RedisBuilder()
                     .WithImage("redis:7.2")
                     .WithPortBinding(6379, true)  // Auto-assign host port
                     .Build();

        await _container.StartAsync();

        // Establish Redis connection
        ConnectionString = _container.GetConnectionString();
        Connection = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
        Database = Connection.GetDatabase();
    }

    /// <summary>
    /// Releases resources when test collection ends
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Connection != null)
        {
            await Connection.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Clears database - Uses KeyDelete instead of FLUSHDB
    /// Some Redis container images don't enable admin mode by default, FLUSHDB will fail
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var server = Connection.GetServer(Connection.GetEndPoints().First());
        var keys = server.Keys(Database.Database);
        if (keys.Any())
        {
            await Database.KeyDeleteAsync(keys.ToArray());
        }
    }

    /// <summary>
    /// Gets Redis Server instance - for advanced operations like key scanning
    /// </summary>
    public IServer GetServer()
    {
        return Connection.GetServer(Connection.GetEndPoints().First());
    }

    /// <summary>
    /// Deletes all keys matching a pattern
    /// </summary>
    public async Task DeleteKeysByPatternAsync(string pattern)
    {
        var server = GetServer();
        var keys = server.Keys(Database.Database, pattern);
        if (keys.Any())
        {
            await Database.KeyDeleteAsync(keys.ToArray());
        }
    }
}

/// <summary>
/// Defines test collection using Redis Fixture
/// Test classes marked with this collection will share the same container instance
/// </summary>
[CollectionDefinition("Redis Collection")]
public class RedisCollectionFixture : ICollectionFixture<RedisContainerFixture>
{
    // This class doesn't need implementation, only for marking the collection
    // xUnit will automatically manage Fixture lifecycle
}
