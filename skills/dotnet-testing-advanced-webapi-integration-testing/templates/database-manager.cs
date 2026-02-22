using Npgsql;
using Respawn;

namespace YourProject.Tests.Integration.Fixtures;

/// <summary>
/// Database manager - responsible for database initialization and cleanup
/// </summary>
public class DatabaseManager
{
    private readonly string _connectionString;
    private Respawner? _respawner;
    private bool _isInitialized;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Initialize database schema
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Ensure tables exist
        await EnsureTablesExistAsync(connection);

        // Initialize Respawner
        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" },
                TablesToIgnore = new Respawn.Graph.Table[]
                {
                    // Can ignore tables that don't need cleanup, for example:
                    // "schema_migrations"
                }
            });
        }

        _isInitialized = true;
    }

    /// <summary>
    /// Clean database data (preserve structure)
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (_respawner == null)
        {
            throw new InvalidOperationException("Respawner not initialized, please call InitializeDatabaseAsync first");
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Ensure tables exist, using external SQL scripts to create
    /// </summary>
    private async Task EnsureTablesExistAsync(NpgsqlConnection connection)
    {
        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
        if (!Directory.Exists(scriptDirectory))
        {
            throw new DirectoryNotFoundException($"SQL scripts directory does not exist: {scriptDirectory}");
        }

        // Execute table creation scripts in dependency order
        var orderedScripts = new[]
        {
            "Tables/CreateProductsTable.sql"
            // Add more tables as needed
        };

        foreach (var scriptPath in orderedScripts)
        {
            var fullPath = Path.Combine(scriptDirectory, scriptPath);
            if (File.Exists(fullPath))
            {
                var script = await File.ReadAllTextAsync(fullPath);
                await using var command = new NpgsqlCommand(script, connection);
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                throw new FileNotFoundException($"SQL script file does not exist: {fullPath}");
            }
        }
    }

    /// <summary>
    /// Execute custom SQL script
    /// </summary>
    /// <param name="sql">SQL script</param>
    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute query and return results
    /// </summary>
    public async Task<T?> QuerySingleAsync<T>(string sql, Func<NpgsqlDataReader, T> mapper)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return mapper(reader);
        }

        return default;
    }

    /// <summary>
    /// Add test product data
    /// </summary>
    public async Task<Guid> SeedProductAsync(string name, decimal price)
    {
        var id = Guid.NewGuid();
        var sql = $@"
            INSERT INTO products (id, name, price, created_at, updated_at)
            VALUES ('{id}', '{name}', {price}, NOW(), NOW())";

        await ExecuteAsync(sql);
        return id;
    }

    /// <summary>
    /// Batch add test product data
    /// </summary>
    public async Task SeedProductsAsync(int count)
    {
        var tasks = Enumerable.Range(1, count)
            .Select(i => SeedProductAsync($"Product {i:D2}", i * 10.0m));
        await Task.WhenAll(tasks);
    }
}
