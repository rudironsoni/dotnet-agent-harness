using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// Aspire application test fixture
/// Uses .NET Aspire Testing framework for distributed application testing
/// </summary>
public class AspireAppFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _httpClient;

    /// <summary>
    /// Application instance
    /// </summary>
    public DistributedApplication App => _app ?? throw new InvalidOperationException("Application not initialized");

    /// <summary>
    /// HTTP client
    /// </summary>
    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("HTTP client not initialized");

    /// <summary>
    /// Initialize Aspire test application
    /// </summary>
    public async Task InitializeAsync()
    {
        // Create Aspire Testing host - using architecture defined by AppHost
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyApp_AppHost>();

        // Build and start application
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Ensure all services are fully ready
        await WaitForServicesReadyAsync();

        // Create HTTP client for API calls
        _httpClient = _app.CreateHttpClient("myapp-api", "http");
    }

    /// <summary>
    /// Wait for all services to be fully ready
    /// </summary>
    private async Task WaitForServicesReadyAsync()
    {
        await WaitForPostgreSqlReadyAsync();
        await WaitForRedisReadyAsync();
    }

    /// <summary>
    /// Wait for PostgreSQL service to be ready
    /// </summary>
    private async Task WaitForPostgreSqlReadyAsync()
    {
        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var connectionString = await GetConnectionStringAsync();
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                builder.Database = "postgres"; // Use default database to test connection

                await using var connection = new Npgsql.NpgsqlConnection(builder.ToString());
                await connection.OpenAsync();
                await connection.CloseAsync();
                Console.WriteLine("PostgreSQL service is ready");
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Waiting for PostgreSQL, attempt {i + 1}/{maxRetries}: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException("PostgreSQL service failed to become ready within expected time");
    }

    /// <summary>
    /// Wait for Redis service to be ready
    /// </summary>
    private async Task WaitForRedisReadyAsync()
    {
        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var connectionString = await GetRedisConnectionStringAsync();
                await using var connection = StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
                var database = connection.GetDatabase();
                await database.PingAsync();
                await connection.DisposeAsync();
                Console.WriteLine("Redis service is ready");
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Waiting for Redis, attempt {i + 1}/{maxRetries}: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException("Redis service failed to become ready within expected time");
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    /// <summary>
    /// Get PostgreSQL connection string
    /// </summary>
    public async Task<string> GetConnectionStringAsync()
    {
        return await _app!.GetConnectionStringAsync("productdb")
            ?? throw new InvalidOperationException("Unable to get PostgreSQL connection string");
    }

    /// <summary>
    /// Get Redis connection string
    /// </summary>
    public async Task<string> GetRedisConnectionStringAsync()
    {
        return await _app!.GetConnectionStringAsync("redis")
            ?? throw new InvalidOperationException("Unable to get Redis connection string");
    }
}
