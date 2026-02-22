# TUnit + Testcontainers Infrastructure Orchestration

> This document is extracted from [SKILL.md](../SKILL.md), providing complete examples and details for TUnit and Testcontainers multi-service orchestration.

## Managing Containers with [Before(Assembly)] and [After(Assembly)]

```csharp
public static class GlobalTestInfrastructureSetup
{
    public static PostgreSqlContainer? PostgreSqlContainer { get; private set; }
    public static RedisContainer? RedisContainer { get; private set; }
    public static KafkaContainer? KafkaContainer { get; private set; }
    public static INetwork? Network { get; private set; }

    [Before(Assembly)]
    public static async Task SetupGlobalInfrastructure()
    {
        Console.WriteLine("=== Starting Global Test Infrastructure Setup ===");

        // Create network
        Network = new NetworkBuilder()
            .WithName("global-test-network")
            .Build();

        await Network.CreateAsync();

        // Create PostgreSQL container
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithNetwork(Network)
            .WithCleanUp(true)
            .Build();

        await PostgreSqlContainer.StartAsync();

        // Create Redis container
        RedisContainer = new RedisBuilder()
            .WithNetwork(Network)
            .WithCleanUp(true)
            .Build();

        await RedisContainer.StartAsync();

        // Create Kafka container
        KafkaContainer = new KafkaBuilder()
            .WithNetwork(Network)
            .WithCleanUp(true)
            .Build();

        await KafkaContainer.StartAsync();

        Console.WriteLine("=== Global Test Infrastructure Setup Complete ===");
    }

    [After(Assembly)]
    public static async Task TeardownGlobalInfrastructure()
    {
        Console.WriteLine("=== Starting Global Test Infrastructure Teardown ===");

        if (KafkaContainer != null)
            await KafkaContainer.DisposeAsync();

        if (RedisContainer != null)
            await RedisContainer.DisposeAsync();

        if (PostgreSqlContainer != null)
            await PostgreSqlContainer.DisposeAsync();

        if (Network != null)
            await Network.DeleteAsync();

        Console.WriteLine("=== Global Test Infrastructure Teardown Complete ===");
    }
}
```

## Using Global Containers for Testing

```csharp
public class ComplexInfrastructureTests
{
    [Test]
    [Property("Category", "Integration")]
    [Property("Infrastructure", "Complex")]
    [DisplayName("Multi-Service Collaboration: PostgreSQL + Redis + Kafka Complete Test")]
    public async Task CompleteWorkflow_MultiServiceCollaboration_ShouldExecuteCorrectly()
    {
        var dbConnectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.GetConnectionString();
        var redisConnectionString = GlobalTestInfrastructureSetup.RedisContainer!.GetConnectionString();
        var kafkaBootstrapServers = GlobalTestInfrastructureSetup.KafkaContainer!.GetBootstrapAddress();

        await Assert.That(dbConnectionString).IsNotNull();
        await Assert.That(dbConnectionString).Contains("test_db");

        await Assert.That(redisConnectionString).IsNotNull();
        await Assert.That(redisConnectionString).Contains("127.0.0.1");

        await Assert.That(kafkaBootstrapServers).IsNotNull();
        await Assert.That(kafkaBootstrapServers).Contains("127.0.0.1");
    }

    [Test]
    [Property("Category", "Database")]
    [DisplayName("PostgreSQL Database Connection Verification")]
    public async Task PostgreSqlDatabase_ConnectionVerification_ShouldSuccessfullyEstablishConnection()
    {
        var connectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.GetConnectionString();

        await Assert.That(connectionString).Contains("test_db");
        await Assert.That(connectionString).Contains("test_user");
    }
}
```

**Benefits of Assembly-Level Container Sharing:**

1. **Greatly Reduces Startup Time**: Containers only start once at Assembly beginning
2. **Significantly Lowers Resource Consumption**: Avoids recreating containers for each test class
3. **Improves Test Stability**: Reduces risk of container startup failures
4. **Maintains Test Isolation**: Tests can still independently clean data between runs
