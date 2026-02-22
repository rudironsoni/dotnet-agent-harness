namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// Integration test base class - using Aspire Testing framework
/// All integration test classes should inherit from this base class
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly AspireAppFixture Fixture;
    protected readonly HttpClient HttpClient;
    protected readonly DatabaseManager DatabaseManager;

    protected IntegrationTestBase(AspireAppFixture fixture)
    {
        Fixture = fixture;
        HttpClient = fixture.HttpClient;
        DatabaseManager = new DatabaseManager(() => fixture.GetConnectionStringAsync());
    }

    /// <summary>
    /// Initialize before each test
    /// Ensure database schema exists
    /// </summary>
    public async Task InitializeAsync()
    {
        await DatabaseManager.InitializeDatabaseAsync();
    }

    /// <summary>
    /// Cleanup after each test
    /// Use Respawn to clean test data, maintaining test isolation
    /// </summary>
    public async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
    }

    #region Time Control Helper Methods (using FakeTimeProvider)

    // If tests need time control, add FakeTimeProvider methods here
    // e.g.:
    // protected void AdvanceTime(TimeSpan timeSpan) { ... }
    // protected void SetTime(DateTimeOffset dateTime) { ... }

    #endregion
}
