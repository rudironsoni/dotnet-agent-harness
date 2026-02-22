using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// MongoDB Container Fixture - Uses Collection Fixture pattern for container sharing
/// Saves 80%+ of test execution time
/// </summary>
public class MongoDbContainerFixture : IAsyncLifetime
{
    private MongoDbContainer? _container;

    /// <summary>
    /// MongoDB database instance - for test operations
    /// </summary>
    public IMongoDatabase Database { get; private set; } = null!;

    /// <summary>
    /// MongoDB connection string
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Test database name
    /// </summary>
    public string DatabaseName { get; } = "testdb";

    /// <summary>
    /// Starts MongoDB container when test collection begins
    /// </summary>
    public async Task InitializeAsync()
    {
        // Use MongoDB 7.0 for complete feature support
        _container = new MongoDbBuilder()
                     .WithImage("mongo:7.0")
                     .WithPortBinding(27017, true)  // Auto-assign host port
                     .Build();

        await _container.StartAsync();

        // Get connection string and establish database connection
        ConnectionString = _container.GetConnectionString();
        var client = new MongoClient(ConnectionString);
        Database = client.GetDatabase(DatabaseName);
    }

    /// <summary>
    /// Releases container resources when test collection ends
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Clears all collections in the database - for test isolation
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var collections = await Database.ListCollectionNamesAsync();
        await collections.ForEachAsync(async collectionName =>
        {
            await Database.DropCollectionAsync(collectionName);
        });
    }

    /// <summary>
    /// Gets the specified collection
    /// </summary>
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return Database.GetCollection<T>(collectionName);
    }
}

/// <summary>
/// Defines test collection using MongoDB Fixture
/// Test classes marked with this collection will share the same container instance
/// </summary>
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbContainerFixture>
{
    // This class doesn't need implementation, only for marking the collection
    // xUnit will automatically manage Fixture lifecycle
}
