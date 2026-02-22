using System.Data;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// Database manager - uses Aspire-provided connection strings
/// Responsible for database initialization, schema creation, and test data cleanup
/// </summary>
public class DatabaseManager
{
    private readonly Func<Task<string>> _getConnectionStringAsync;
    private Respawner? _respawner;

    public DatabaseManager(Func<Task<string>> getConnectionStringAsync)
    {
        _getConnectionStringAsync = getConnectionStringAsync;
    }

    /// <summary>
    /// Initialize database schema
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        var connectionString = await _getConnectionStringAsync();

        // First ensure database exists
        await EnsureDatabaseExistsAsync(connectionString);

        // Connect to database and ensure tables exist
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Ensure tables exist
        await EnsureTablesExistAsync(connection);

        // Initialize Respawner - key: specify PostgreSQL adapter
        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
                SchemasToInclude = new[] { "public" },
                DbAdapter = DbAdapter.Postgres  // Important: explicitly specify PostgreSQL adapter
            });
        }
    }

    /// <summary>
    /// Clean test data
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (_respawner == null) return;

        var connectionString = await _getConnectionStringAsync();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Get connection string
    /// </summary>
    public async Task<string> GetConnectionStringAsync()
    {
        return await _getConnectionStringAsync();
    }

    /// <summary>
    /// Ensure database exists - Aspire starts containers but doesn't auto-create databases
    /// </summary>
    private async Task EnsureDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        // Connect to postgres default database to check and create target database
        builder.Database = "postgres";
        var masterConnectionString = builder.ToString();

        await using var connection = new NpgsqlConnection(masterConnectionString);
        await WaitForDatabaseConnectionAsync(connection);

        // Check if database exists
        var checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
        await using var checkCommand = new NpgsqlCommand(checkDbQuery, connection);
        var dbExists = await checkCommand.ExecuteScalarAsync();

        if (dbExists == null)
        {
            // Create database
            var createDbQuery = $"CREATE DATABASE \"{databaseName}\"";
            await using var createCommand = new NpgsqlCommand(createDbQuery, connection);
            await createCommand.ExecuteNonQueryAsync();
            Console.WriteLine($"Created database: {databaseName}");
        }
    }

    /// <summary>
    /// Retry mechanism for waiting for database connection
    /// </summary>
    private async Task WaitForDatabaseConnectionAsync(NpgsqlConnection connection)
    {
        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await connection.OpenAsync();
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Database connection attempt {i + 1} failed: {ex.Message}");
                await Task.Delay(delayMs);

                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        await connection.OpenAsync();
    }

    /// <summary>
    /// Ensure required tables exist
    /// Can load from external SQL file or define directly
    /// </summary>
    private async Task EnsureTablesExistAsync(NpgsqlConnection connection)
    {
        var createProductTableSql = """
            CREATE TABLE IF NOT EXISTS products (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(100) NOT NULL,
                price DECIMAL(10,2) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            """;

        await using var command = new NpgsqlCommand(createProductTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
