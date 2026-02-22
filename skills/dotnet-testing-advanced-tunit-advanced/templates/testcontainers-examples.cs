// TUnit + Testcontainers Infrastructure Orchestration Examples

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

// Requires installing these packages:
// dotnet add package Testcontainers.PostgreSql
// dotnet add package Testcontainers.Redis
// dotnet add package Testcontainers.Kafka

/*
 * The following examples use Testcontainers.NET for container management
 * For actual use, uncomment and install relevant packages
 */

namespace TUnit.Advanced.Testcontainers.Examples;

#region Global Test Infrastructure Setup

/// <summary>
/// Global test infrastructure setup
/// Uses [Before(Assembly)] and [After(Assembly)] to manage container lifecycle
///
/// This is best practice: All tests share the same infrastructure
/// </summary>
public static class GlobalTestInfrastructureSetup
{
    // Simulated container properties (in real projects, use Testcontainers types)
    public static MockPostgreSqlContainer? PostgreSqlContainer { get; private set; }
    public static MockRedisContainer? RedisContainer { get; private set; }
    public static MockKafkaContainer? KafkaContainer { get; private set; }
    public static string? NetworkName { get; private set; }

    /// <summary>
    /// Assembly level setup
    /// Executes once before entire test assembly begins
    /// </summary>
    [Before(Assembly)]
    public static async Task SetupGlobalInfrastructure()
    {
        Console.WriteLine("=== Starting global test infrastructure setup ===");

        // Create network
        NetworkName = "global-test-network";
        Console.WriteLine($"Test network created: {NetworkName}");

        // Create PostgreSQL container
        PostgreSqlContainer = new MockPostgreSqlContainer
        {
            ConnectionString = "Host=localhost;Database=test_db;Username=test_user;Password=test_password"
        };
        await PostgreSqlContainer.StartAsync();
        Console.WriteLine($"PostgreSQL container started: {PostgreSqlContainer.ConnectionString}");

        // Create Redis container
        RedisContainer = new MockRedisContainer
        {
            ConnectionString = "127.0.0.1:6379"
        };
        await RedisContainer.StartAsync();
        Console.WriteLine($"Redis container started: {RedisContainer.ConnectionString}");

        // Create Kafka container
        KafkaContainer = new MockKafkaContainer
        {
            BootstrapAddress = "127.0.0.1:9092"
        };
        await KafkaContainer.StartAsync();
        Console.WriteLine($"Kafka container started: {KafkaContainer.BootstrapAddress}");

        Console.WriteLine("=== Global test infrastructure setup complete ===");
    }

    /// <summary>
    /// Assembly level cleanup
    /// Executes once after entire test assembly ends
    /// </summary>
    [After(Assembly)]
    public static async Task TeardownGlobalInfrastructure()
    {
        Console.WriteLine("=== Starting global test infrastructure cleanup ===");

        if (KafkaContainer != null)
        {
            await KafkaContainer.DisposeAsync();
            Console.WriteLine("Kafka container stopped");
        }

        if (RedisContainer != null)
        {
            await RedisContainer.DisposeAsync();
            Console.WriteLine("Redis container stopped");
        }

        if (PostgreSqlContainer != null)
        {
            await PostgreSqlContainer.DisposeAsync();
            Console.WriteLine("PostgreSQL container stopped");
        }

        Console.WriteLine("=== Global test infrastructure cleanup complete ===");
    }
}

#endregion

#region Mock Container Classes (for demonstration)

/// <summary>
/// Mock PostgreSQL container (in real projects, use Testcontainers.PostgreSql.PostgreSqlContainer)
/// </summary>
public class MockPostgreSqlContainer : IAsyncDisposable
{
    public string ConnectionString { get; set; } = string.Empty;
    public string State { get; private set; } = "Created";

    public Task StartAsync()
    {
        State = "Running";
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        State = "Stopped";
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Mock Redis container (in real projects, use Testcontainers.Redis.RedisContainer)
/// </summary>
public class MockRedisContainer : IAsyncDisposable
{
    public string ConnectionString { get; set; } = string.Empty;
    public string State { get; private set; } = "Created";

    public Task StartAsync()
    {
        State = "Running";
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        State = "Stopped";
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Mock Kafka container (in real projects, use Testcontainers.Kafka.KafkaContainer)
/// </summary>
public class MockKafkaContainer : IAsyncDisposable
{
    public string BootstrapAddress { get; set; } = string.Empty;
    public string State { get; private set; } = "Created";

    public Task StartAsync()
    {
        State = "Running";
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        State = "Stopped";
        return ValueTask.CompletedTask;
    }
}

#endregion

#region Infrastructure Verification Tests

/// <summary>
/// Infrastructure verification tests
/// Ensures test environment container services are working properly
/// </summary>
public class ComplexInfrastructureTests
{
    /// <summary>
    /// Multi-service collaboration test
    /// Verifies all containers started and are connectable
    /// </summary>
    [Test]
    [Property("Category", "Integration")]
    [Property("Infrastructure", "Complex")]
    [DisplayName("Multi-service collaboration: PostgreSQL + Redis + Kafka complete test")]
    public async Task CompleteWorkflow_MultiServiceCollaboration_ShouldExecuteCorrectly()
    {
        // Arrange & Act
        var dbConnectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.ConnectionString;
        var redisConnectionString = GlobalTestInfrastructureSetup.RedisContainer!.ConnectionString;
        var kafkaBootstrapServers = GlobalTestInfrastructureSetup.KafkaContainer!.BootstrapAddress;

        // Assert
        await Assert.That(dbConnectionString).IsNotNull();
        await Assert.That(dbConnectionString).Contains("test_db");
        await Assert.That(dbConnectionString).Contains("test_user");

        await Assert.That(redisConnectionString).IsNotNull();
        await Assert.That(redisConnectionString).Contains("127.0.0.1");

        await Assert.That(kafkaBootstrapServers).IsNotNull();
        await Assert.That(kafkaBootstrapServers).Contains("127.0.0.1");

        // Output verification info
        Console.WriteLine("=== Multi-service collaboration test ===");
        Console.WriteLine($"PostgreSQL: {dbConnectionString}");
        Console.WriteLine($"Redis: {redisConnectionString}");
        Console.WriteLine($"Kafka: {kafkaBootstrapServers}");
        Console.WriteLine("=====================");
    }

    /// <summary>
    /// PostgreSQL connection verification
    /// </summary>
    [Test]
    [Property("Category", "Database")]
    [DisplayName("PostgreSQL database connection verification")]
    public async Task PostgreSqlDatabase_ConnectionVerification_ShouldSuccessfullyEstablishConnection()
    {
        // Arrange
        var connectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.ConnectionString;

        // Act & Assert
        await Assert.That(connectionString).Contains("test_db");
        await Assert.That(connectionString).Contains("test_user");
        await Assert.That(connectionString).Contains("test_password");

        Console.WriteLine($"Database connection verified: {connectionString}");
    }

    /// <summary>
    /// Redis service verification
    /// </summary>
    [Test]
    [Property("Category", "Cache")]
    [DisplayName("Redis cache service verification")]
    public async Task RedisCache_CacheService_ShouldStartCorrectly()
    {
        // Arrange
        var connectionString = GlobalTestInfrastructureSetup.RedisContainer!.ConnectionString;

        // Act & Assert
        await Assert.That(connectionString).IsNotNull();
        await Assert.That(connectionString).Contains("127.0.0.1");

        Console.WriteLine($"Redis connection verified: {connectionString}");
    }

    /// <summary>
    /// Kafka service verification
    /// </summary>
    [Test]
    [Property("Category", "MessageQueue")]
    [DisplayName("Kafka message queue service verification")]
    public async Task KafkaMessageQueue_MessageQueue_ShouldStartCorrectly()
    {
        // Arrange
        var bootstrapServers = GlobalTestInfrastructureSetup.KafkaContainer!.BootstrapAddress;

        // Act & Assert
        await Assert.That(bootstrapServers).IsNotNull();
        await Assert.That(bootstrapServers).Contains("127.0.0.1");

        Console.WriteLine($"Kafka connection verified: {bootstrapServers}");
    }
}

#endregion

#region Advanced Dependency Tests

/// <summary>
/// Advanced dependency management tests
/// Shows how to use container-provided services in tests
/// </summary>
public class AdvancedDependencyTests
{
    /// <summary>
    /// Network infrastructure verification
    /// </summary>
    [Test]
    [Property("Category", "Network")]
    [DisplayName("Network infrastructure verification")]
    public async Task NetworkInfrastructure_NetworkSetup_ShouldBeCorrectlyCreated()
    {
        // Arrange & Act
        var networkName = GlobalTestInfrastructureSetup.NetworkName;

        // Assert
        await Assert.That(networkName).IsEqualTo("global-test-network");

        Console.WriteLine($"Test network verified: {networkName}");
    }

    /// <summary>
    /// Container status verification
    /// </summary>
    [Test]
    [Property("Category", "Infrastructure")]
    [DisplayName("All container running status verification")]
    public async Task AllContainers_RunningStatus_ShouldBeRunning()
    {
        // Assert
        await Assert.That(GlobalTestInfrastructureSetup.PostgreSqlContainer!.State).IsEqualTo("Running");
        await Assert.That(GlobalTestInfrastructureSetup.RedisContainer!.State).IsEqualTo("Running");
        await Assert.That(GlobalTestInfrastructureSetup.KafkaContainer!.State).IsEqualTo("Running");

        Console.WriteLine("All containers are running");
    }
}

#endregion

#region Test Infrastructure Manager

/// <summary>
/// Test infrastructure manager
/// Provides unified container management and configuration generation
/// </summary>
public class TestInfrastructureManager
{
    /// <summary>
    /// Gets complete application configuration
    /// </summary>
    private Dictionary<string, string> GetTestConfiguration()
    {
        return new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = GlobalTestInfrastructureSetup.PostgreSqlContainer!.ConnectionString,
            ["ConnectionStrings:Redis"] = GlobalTestInfrastructureSetup.RedisContainer!.ConnectionString,
            ["Kafka:BootstrapServers"] = GlobalTestInfrastructureSetup.KafkaContainer!.BootstrapAddress,
            ["Environment"] = "Testing"
        };
    }

    /// <summary>
    /// Configuration generation verification
    /// </summary>
    [Test]
    [Property("Category", "Infrastructure")]
    [DisplayName("Infrastructure Manager: Configuration generation verification")]
    public async Task InfrastructureManager_ConfigurationGeneration_ShouldProvideCompleteConfiguration()
    {
        // Act
        var configuration = GetTestConfiguration();

        // Assert
        await Assert.That(configuration).IsNotNull();
        await Assert.That(configuration.ContainsKey("ConnectionStrings:DefaultConnection")).IsTrue();
        await Assert.That(configuration.ContainsKey("ConnectionStrings:Redis")).IsTrue();
        await Assert.That(configuration.ContainsKey("Kafka:BootstrapServers")).IsTrue();
        await Assert.That(configuration.ContainsKey("Environment")).IsTrue();

        await Assert.That(configuration["Environment"]).IsEqualTo("Testing");
        await Assert.That(configuration["ConnectionStrings:DefaultConnection"]).Contains("test_db");
        await Assert.That(configuration["ConnectionStrings:Redis"]).Contains("127.0.0.1");

        // Output configuration info
        Console.WriteLine("Generated test configuration:");
        foreach (var kvp in configuration)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
    }
}

#endregion

#region Real Testcontainers Example (Commented)

/*
 * Real Testcontainers usage example
 * Requires installing these packages:
 *
 * dotnet add package Testcontainers.PostgreSql
 * dotnet add package Testcontainers.Redis
 * dotnet add package Testcontainers.Kafka
 *
 * using Testcontainers.PostgreSql;
 * using Testcontainers.Redis;
 * using Testcontainers.Kafka;
 * using DotNet.Testcontainers.Builders;
 * using DotNet.Testcontainers.Containers;
 * using DotNet.Testcontainers.Networks;
 *
 * public static class RealGlobalTestInfrastructureSetup
 * {
 *     public static PostgreSqlContainer? PostgreSqlContainer { get; private set; }
 *     public static RedisContainer? RedisContainer { get; private set; }
 *     public static KafkaContainer? KafkaContainer { get; private set; }
 *     public static INetwork? Network { get; private set; }
 *
 *     [Before(Assembly)]
 *     public static async Task SetupGlobalInfrastructure()
 *     {
 *         // Create network
 *         Network = new NetworkBuilder()
 *             .WithName("global-test-network")
 *             .Build();
 *
 *         await Network.CreateAsync();
 *
 *         // Create PostgreSQL container
 *         PostgreSqlContainer = new PostgreSqlBuilder()
 *             .WithDatabase("test_db")
 *             .WithUsername("test_user")
 *             .WithPassword("test_password")
 *             .WithNetwork(Network)
 *             .WithCleanUp(true)
 *             .Build();
 *
 *         await PostgreSqlContainer.StartAsync();
 *
 *         // Create Redis container
 *         RedisContainer = new RedisBuilder()
 *             .WithNetwork(Network)
 *             .WithCleanUp(true)
 *             .Build();
 *
 *         await RedisContainer.StartAsync();
 *
 *         // Create Kafka container
 *         KafkaContainer = new KafkaBuilder()
 *             .WithNetwork(Network)
 *             .WithCleanUp(true)
 *             .Build();
 *
 *         await KafkaContainer.StartAsync();
 *     }
 *
 *     [After(Assembly)]
 *     public static async Task TeardownGlobalInfrastructure()
 *     {
 *         if (KafkaContainer != null)
 *             await KafkaContainer.DisposeAsync();
 *
 *         if (RedisContainer != null)
 *             await RedisContainer.DisposeAsync();
 *
 *         if (PostgreSqlContainer != null)
 *             await PostgreSqlContainer.DisposeAsync();
 *
 *         if (Network != null)
 *             await Network.DeleteAsync();
 *     }
 * }
 */

#endregion

#region Performance Optimization Notes

/*
 * Assembly-level container sharing performance advantages:
 *
 * 1. Significantly reduces startup time
 *    - Containers only start once at Assembly beginning
 *    - Avoids repeated container creation per test class
 *
 * 2. Significantly reduces resource consumption
 *    - Reduces Docker container count
 *    - Lowers memory and CPU usage
 *
 * 3. Improves test stability
 *    - Reduces container startup failure risk
 *    - Container state remains consistent between tests
 *
 * 4. Maintains test isolation
 *    - Tests can still independently clean data
 *    - Container state won't interfere with each other
 *
 * Best practices:
 * - Use [Before(Assembly)] to start all shared containers
 * - Use [After(Assembly)] to clean all containers
 * - Use static properties to access containers in tests
 * - Consider using Testcontainers WithCleanUp(true) to ensure cleanup after tests
 */

#endregion
