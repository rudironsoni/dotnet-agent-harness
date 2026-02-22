// TUnit Lifecycle Management and Dependency Injection Examples

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace TUnit.Advanced.Lifecycle.Examples;

#region Domain Models and Interfaces

public enum CustomerLevel
{
    RegularMember = 0,
    VipMember = 1,
    PlatinumMember = 2,
    DiamondMember = 3
}

public class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public CustomerLevel CustomerLevel { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal TotalAmount => SubTotal;
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public interface IOrderRepository
{
    Task<bool> SaveOrderAsync(Order order);
}

public interface IDiscountCalculator
{
    Task<decimal> CalculateDiscountAsync(Order order, string discountCode);
}

public interface IShippingCalculator
{
    decimal CalculateShippingFee(Order order);
}

public interface ILogger<T>
{
    void LogInformation(string message);
}

#endregion

#region Mock Implementations

public class MockOrderRepository : IOrderRepository
{
    public Task<bool> SaveOrderAsync(Order order)
    {
        order.OrderId = Guid.NewGuid().ToString();
        return Task.FromResult(true);
    }
}

public class MockDiscountCalculator : IDiscountCalculator
{
    public Task<decimal> CalculateDiscountAsync(Order order, string discountCode)
    {
        var baseDiscount = order.CustomerLevel == CustomerLevel.VipMember ?
            order.TotalAmount * 0.1m : 0m;
        return Task.FromResult(baseDiscount);
    }
}

public class MockShippingCalculator : IShippingCalculator
{
    public decimal CalculateShippingFee(Order order)
    {
        if (order.CustomerLevel == CustomerLevel.DiamondMember) return 0m;
        if (order.SubTotal >= 1000m) return 0m;
        return 80m;
    }
}

public class MockLogger<T> : ILogger<T>
{
    public void LogInformation(string message)
    {
        Console.WriteLine($"[{typeof(T).Name}] {message}");
    }
}

#endregion

#region Order Service

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IDiscountCalculator _discountCalculator;
    private readonly IShippingCalculator _shippingCalculator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IDiscountCalculator discountCalculator,
        IShippingCalculator shippingCalculator,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _discountCalculator = discountCalculator;
        _shippingCalculator = shippingCalculator;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(string customerId, CustomerLevel level, List<OrderItem> items)
    {
        var order = new Order
        {
            CustomerId = customerId,
            CustomerLevel = level,
            Items = items
        };

        await _repository.SaveOrderAsync(order);
        _logger.LogInformation($"Order created: {order.OrderId}");
        return order;
    }
}

#endregion

#region Lifecycle Examples

/// <summary>
/// TUnit lifecycle complete example
/// Demonstrates Before/After attribute execution order
/// </summary>
public class LifecycleCompleteExample
{
    private readonly StringBuilder _logBuilder;
    private static readonly List<string> ClassLog = [];

    public LifecycleCompleteExample()
    {
        Console.WriteLine("1. Constructor executed - Test instance created");
        _logBuilder = new StringBuilder();
        _logBuilder.AppendLine("Constructor executed");
    }

    [Before(Class)]
    public static async Task BeforeClass()
    {
        Console.WriteLine("2. BeforeClass executed - Class level initialization");
        ClassLog.Add("BeforeClass executed");
        await Task.Delay(10); // Simulate async initialization
    }

    [Before(Test)]
    public async Task BeforeTest()
    {
        Console.WriteLine("3. BeforeTest executed - Test pre-setup");
        _logBuilder.AppendLine("BeforeTest executed");
        await Task.Delay(5); // Simulate async setup
    }

    [Test]
    public async Task FirstTest_ShouldExecuteLifecycleMethodsInCorrectOrder()
    {
        Console.WriteLine($"4. FirstTest executed - Verifying lifecycle order [{DateTime.Now:HH:mm:ss.fff}]");
        _logBuilder.AppendLine("FirstTest executed");

        var log = _logBuilder.ToString();
        await Assert.That(log).Contains("Constructor executed");
        await Assert.That(log).Contains("BeforeTest executed");
        await Assert.That(ClassLog).Contains("BeforeClass executed");
    }

    [Test]
    public async Task SecondTest_ShouldHaveIndependentInstance()
    {
        Console.WriteLine($"4. SecondTest executed - Verifying instance independence [{DateTime.Now:HH:mm:ss.fff}]");
        _logBuilder.AppendLine("SecondTest executed");

        // Each test has new instance, so constructor runs again
        var log = _logBuilder.ToString();
        await Assert.That(log).Contains("Constructor executed");
        await Assert.That(log).Contains("BeforeTest executed");
    }

    [After(Test)]
    public async Task AfterTest()
    {
        Console.WriteLine("5. AfterTest executed - Post test cleanup");
        _logBuilder.AppendLine("AfterTest executed");
        await Task.Delay(5); // Simulate async cleanup
    }

    [After(Class)]
    public static async Task AfterClass()
    {
        Console.WriteLine("6. AfterClass executed - Class level cleanup");
        ClassLog.Add("AfterClass executed");
        await Task.Delay(10); // Simulate async cleanup
    }
}

/// <summary>
/// IDisposable support example
/// Demonstrates proper test resource cleanup
/// </summary>
public class DisposableLifecycleExample : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly List<string> _tempFiles = [];

    public DisposableLifecycleExample()
    {
        _httpClient = new HttpClient();
        Console.WriteLine("Resources created: HttpClient");
    }

    [Test]
    public async Task TestWithResources_ShouldManageResourcesCorrectly()
    {
        // Simulate creating temp file
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        await Assert.That(_httpClient).IsNotNull();
        await Assert.That(File.Exists(tempFile)).IsTrue();
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("DisposeAsync executed - Releasing all resources");

        _httpClient.Dispose();

        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        await Task.CompletedTask;
    }
}

#endregion

#region Dependency Injection

/// <summary>
/// TUnit dependency injection data source attribute
/// Based on Microsoft.Extensions.DependencyInjection implementation
/// </summary>
public class MicrosoftDependencyInjectionDataSourceAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly IServiceProvider ServiceProvider = CreateSharedServiceProvider();

    public override IServiceScope CreateScope(DataGeneratorMetadata dataGeneratorMetadata)
    {
        return ServiceProvider.CreateScope();
    }

    public override object? Create(IServiceScope scope, Type type)
    {
        return scope.ServiceProvider.GetService(type);
    }

    private static IServiceProvider CreateSharedServiceProvider()
    {
        return new ServiceCollection()
            .AddSingleton<IOrderRepository, MockOrderRepository>()
            .AddSingleton<IDiscountCalculator, MockDiscountCalculator>()
            .AddSingleton<IShippingCalculator, MockShippingCalculator>()
            .AddSingleton<ILogger<OrderService>, MockLogger<OrderService>>()
            .AddTransient<OrderService>()
            .BuildServiceProvider();
    }
}

/// <summary>
/// Testing using TUnit dependency injection
/// Demonstrates automatic service injection through constructor
/// </summary>
[MicrosoftDependencyInjectionDataSource]
public class DependencyInjectionTests(OrderService orderService)
{
    [Test]
    public async Task CreateOrder_UsingTUnitDependencyInjection_ShouldWorkCorrectly()
    {
        // Arrange - dependencies auto-injected via TUnit DI
        var items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "Test Product", UnitPrice = 100m, Quantity = 2 }
        };

        // Act
        var order = await orderService.CreateOrderAsync("CUST001", CustomerLevel.VipMember, items);

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo("CUST001");
        await Assert.That(order.CustomerLevel).IsEqualTo(CustomerLevel.VipMember);
        await Assert.That(order.Items).HasCount().EqualTo(1);
    }

    [Test]
    public async Task TUnitDependencyInjection_VerifyAutoInjection_ServiceShouldBeCorrectType()
    {
        // Assert - Verify TUnit correctly injected OrderService instance
        await Assert.That(orderService).IsNotNull();
        await Assert.That(orderService.GetType().Name).IsEqualTo("OrderService");
    }
}

/// <summary>
/// Manual dependency creation comparison example
/// Shows difference between traditional approach and TUnit DI
/// </summary>
public class ManualDependencyTests
{
    [Test]
    public async Task CreateOrder_ManualDependencyCreation_TraditionalApproachComparison()
    {
        // Arrange - Manually create test dependencies (traditional approach)
        var mockRepository = new MockOrderRepository();
        var mockDiscountCalculator = new MockDiscountCalculator();
        var mockShippingCalculator = new MockShippingCalculator();
        var mockLogger = new MockLogger<OrderService>();

        var orderService = new OrderService(
            mockRepository,
            mockDiscountCalculator,
            mockShippingCalculator,
            mockLogger);

        var items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "Test Product", UnitPrice = 100m, Quantity = 2 }
        };

        // Act
        var order = await orderService.CreateOrderAsync("CUST001", CustomerLevel.VipMember, items);

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo("CUST001");
        await Assert.That(order.CustomerLevel).IsEqualTo(CustomerLevel.VipMember);
        await Assert.That(order.Items).HasCount().EqualTo(1);
    }
}

#endregion

#region Properties Examples

/// <summary>
/// Properties attribute marking and test filtering examples
/// </summary>
public class PropertiesExamples
{
    /// <summary>
    /// Establish consistent property naming convention
    /// </summary>
    public static class TestProperties
    {
        // Test categories
        public const string CATEGORY_UNIT = "Unit";
        public const string CATEGORY_INTEGRATION = "Integration";
        public const string CATEGORY_E2E = "E2E";

        // Priorities
        public const string PRIORITY_CRITICAL = "Critical";
        public const string PRIORITY_HIGH = "High";
        public const string PRIORITY_MEDIUM = "Medium";
        public const string PRIORITY_LOW = "Low";

        // Environments
        public const string ENV_DEVELOPMENT = "Development";
        public const string ENV_STAGING = "Staging";
        public const string ENV_PRODUCTION = "Production";
    }

    [Test]
    [Property("Category", "Database")]
    [Property("Priority", "High")]
    public async Task DatabaseTest_HighPriority_ShouldFilterByProperty()
    {
        await Assert.That(true).IsTrue();
    }

    [Test]
    [Property("Category", "Unit")]
    [Property("Priority", "Medium")]
    public async Task UnitTest_MediumPriority_BasicValidation()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }

    [Test]
    [Property("Category", "Integration")]
    [Property("Priority", "Low")]
    [Property("Environment", "Development")]
    public async Task IntegrationTest_LowPriority_DevelopmentEnvironmentOnly()
    {
        await Assert.That("Hello World").Contains("World");
    }

    // Use constants to ensure consistency
    [Test]
    [Property("Category", "Unit")]
    [Property("Priority", "High")]
    public async Task ExampleTest_UsingConstants_EnsureConsistency()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }
}

#endregion

#region Test Filtering Commands

/*
 * TUnit Test Filter Execution Command Examples
 *
 * TUnit uses dotnet run instead of dotnet test:
 *
 * # Run only unit tests
 * dotnet run --treenode-filter "/*/*/*/*[Category=Unit]"
 *
 * # Run only high priority tests
 * dotnet run --treenode-filter "/*/*/*/*[Priority=High]"
 *
 * # Combined: Run high priority unit tests
 * dotnet run --treenode-filter "/*/*/*/*[(Category=Unit)&(Priority=High)]"
 *
 * # Run smoke test suite
 * dotnet run --treenode-filter "/*/*/*/*[Suite=Smoke]"
 *
 * # Run specific feature tests
 * dotnet run --treenode-filter "/*/*/*/*[Feature=OrderProcessing]"
 *
 * # Complex combination: Run high priority unit tests OR smoke tests
 * dotnet run --treenode-filter "/*/*/*/*[((Category=Unit)&(Priority=High))|(Suite=Smoke)]"
 *
 * Filter syntax notes:
 * 1. Path pattern /*/*/*/* is fixed format, representing Assembly/Namespace/Class/Method levels
 * 2. Property names are case-sensitive
 * 3. Values are case-sensitive
 * 4. Parentheses: Combined conditions must be properly enclosed
 * 5. Quotes: Entire filter string must be enclosed in quotes
 */

#endregion
