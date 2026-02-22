using Flurl.Http;
using Microsoft.Extensions.Time.Testing;

namespace YourProject.Tests.Integration.Fixtures;

/// <summary>
/// Integration test collection definition - all tests share the same TestWebApplicationFactory
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    /// <summary>
    /// Collection name constant
    /// </summary>
    public const string Name = "Integration Tests";

    // This class requires no implementation
    // It's only used to define the Collection Fixture
}

/// <summary>
/// Integration test base class - uses Collection Fixture to share containers
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    /// <summary>
    /// WebApplicationFactory instance
    /// </summary>
    protected readonly TestWebApplicationFactory Factory;

    /// <summary>
    /// HTTP client
    /// </summary>
    protected readonly HttpClient HttpClient;

    /// <summary>
    /// Database manager
    /// </summary>
    protected readonly DatabaseManager DatabaseManager;

    /// <summary>
    /// Flurl HTTP client
    /// </summary>
    protected readonly IFlurlClient FlurlClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        DatabaseManager = new DatabaseManager(factory.PostgresContainer.GetConnectionString());

        // Configure Flurl client
        FlurlClient = new FlurlClient(HttpClient);
    }

    /// <summary>
    /// Executed before each test - initialize database schema
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        await DatabaseManager.InitializeDatabaseAsync();
        ResetTime();
    }

    /// <summary>
    /// Executed after each test - clean up database data
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
        FlurlClient.Dispose();
    }

    /// <summary>
    /// Reset time to test start time (2024-01-01 00:00:00 UTC)
    /// </summary>
    protected void ResetTime()
    {
        Factory.TimeProvider.SetUtcNow(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>
    /// Advance time
    /// </summary>
    /// <param name="timeSpan">Time span to advance</param>
    protected void AdvanceTime(TimeSpan timeSpan)
    {
        Factory.TimeProvider.Advance(timeSpan);
    }

    /// <summary>
    /// Set specific time
    /// </summary>
    /// <param name="time">Time to set</param>
    protected void SetTime(DateTimeOffset time)
    {
        Factory.TimeProvider.SetUtcNow(time);
    }

    /// <summary>
    /// Get current test time
    /// </summary>
    protected DateTimeOffset GetCurrentTime()
    {
        return Factory.TimeProvider.GetUtcNow();
    }
}
