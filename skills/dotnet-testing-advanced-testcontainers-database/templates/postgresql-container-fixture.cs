// PostgreSQL Container Fixture Template
// Used for single test class PostgreSQL container configuration
// Suitable for test scenarios that don't require cross-class container sharing

using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Tests.Fixtures;

/// <summary>
/// PostgreSQL container Fixture, implements IAsyncLifetime for async lifecycle management
/// </summary>
/// <remarks>
/// Use cases:
/// - Single test class needs PostgreSQL container
/// - No need to share container across test classes
/// - Tests require isolated database environment
/// </remarks>
public class PostgreSqlContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private YourDbContext _dbContext = null!;

    public PostgreSqlContainerTests()
    {
        _postgres = new PostgreSqlBuilder()
            // Use Alpine version to reduce container size and startup time
            .WithImage("postgres:15-alpine")
            // Set database name
            .WithDatabase("testdb")
            // Set username
            .WithUsername("testuser")
            // Set password
            .WithPassword("testpass")
            // Use random port to avoid conflicts (true = auto-assign)
            .WithPortBinding(5432, true)
            // Automatically clean up container after test completion
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// Initialize container and database context
    /// </summary>
    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        await _postgres.StartAsync();

        // Get connection string and create DbContext
        var options = new DbContextOptionsBuilder<YourDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            // Enable sensitive data logging (development/test environment only)
            .EnableSensitiveDataLogging()
            .Options;

        _dbContext = new YourDbContext(options);
        
        // Ensure database is created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Clean up container and database context
    /// </summary>
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    // ===== Test method examples =====

    [Fact]
    public async Task CreateEntity_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        var entity = new YourEntity
        {
            Name = "Test Entity",
            Description = "Created in PostgreSQL container"
        };

        // Act
        _dbContext.YourEntities.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.YourEntities.FindAsync(entity.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task QueryEntity_WithValidId_ShouldReturnCorrectData()
    {
        // Arrange
        var entity = new YourEntity { Name = "Query Test" };
        _dbContext.YourEntities.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _dbContext.YourEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Query Test");
    }

    [Fact]
    public void GetConnectionString_AfterContainerStarted_ShouldReturnValidConnectionString()
    {
        // Act
        var connectionString = _postgres.GetConnectionString();
        var mappedPort = _postgres.GetMappedPublicPort(5432);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain($"Port={mappedPort}");
        connectionString.Should().Contain("Database=testdb");
        connectionString.Should().Contain("Username=testuser");
    }
}

// ===== Advanced configuration examples =====

/// <summary>
/// PostgreSQL container configuration with Wait Strategy
/// </summary>
public class PostgreSqlWithWaitStrategyTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public PostgreSqlWithWaitStrategyTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            // Use Wait Strategy to ensure container is fully ready
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432)
                .UntilMessageIsLogged("database system is ready to accept connections"))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}

/// <summary>
/// PostgreSQL container configuration with resource limits
// Suitable for CI/CD environments or resource-constrained development machines
/// </summary>
public class PostgreSqlWithResourceLimitsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public PostgreSqlWithResourceLimitsTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            // Use tmpfs mount to improve performance
            .WithTmpfsMount("/var/lib/postgresql/data")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}

// ===== Replace these classes with your actual implementations =====

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    public DbSet<YourEntity> YourEntities { get; set; }
}

public class YourEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
