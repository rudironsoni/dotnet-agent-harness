// TUnit Data Source Examples - MethodDataSource and ClassDataSource

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Text.Json;

namespace TUnit.Advanced.DataSource.Examples;

#region Domain Models

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
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount => SubTotal - DiscountAmount + ShippingFee;
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderValidationScenario
{
    public string Name { get; set; } = "";
    public Order Order { get; set; } = new();
    public bool ExpectedValid { get; set; }
    public string? ExpectedErrorKeyword { get; set; }
}

#endregion

#region MethodDataSource Examples

/// <summary>
/// MethodDataSource basic usage example
/// Demonstrates using method as data source for parameterized tests
/// </summary>
public class MethodDataSourceBasicTests
{
    /// <summary>
    /// Uses MethodDataSource for parameterized testing
    /// Supports complex object type data passing
    /// </summary>
    [Test]
    [MethodDataSource(nameof(GetOrderTestData))]
    public async Task CreateOrder_VariousScenarios_ShouldHandleCorrectly(
        string customerId,
        CustomerLevel level,
        List<OrderItem> items,
        decimal expectedTotal)
    {
        // Arrange
        var order = new Order
        {
            CustomerId = customerId,
            CustomerLevel = level,
            Items = items
        };

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo(customerId);
        await Assert.That(order.CustomerLevel).IsEqualTo(level);
        await Assert.That(order.SubTotal).IsEqualTo(expectedTotal);
    }

    /// <summary>
    /// Data provider method - uses yield return to generate test data
    /// </summary>
    public static IEnumerable<object[]> GetOrderTestData()
    {
        // Regular member order
        yield return new object[]
        {
            "CUST001",
            CustomerLevel.RegularMember,
            new List<OrderItem>
            {
                new() { ProductId = "PROD001", ProductName = "Product A", UnitPrice = 100m, Quantity = 2 }
            },
            200m
        };

        // VIP member order
        yield return new object[]
        {
            "CUST002",
            CustomerLevel.VipMember,
            new List<OrderItem>
            {
                new() { ProductId = "PROD002", ProductName = "Product B", UnitPrice = 500m, Quantity = 1 }
            },
            500m
        };

        // Multi-item order
        yield return new object[]
        {
            "CUST003",
            CustomerLevel.PlatinumMember,
            new List<OrderItem>
            {
                new() { ProductId = "PROD001", ProductName = "Product A", UnitPrice = 100m, Quantity = 1 },
                new() { ProductId = "PROD002", ProductName = "Product B", UnitPrice = 200m, Quantity = 2 }
            },
            500m
        };
    }
}

/// <summary>
/// Load test data from JSON file example
/// </summary>
public class MethodDataSourceFromFileTests
{
    /// <summary>
/// Scenario reading test data from JSON file
    /// </summary>
    [Test]
    [MethodDataSource(nameof(GetDiscountTestDataFromFile))]
    public async Task CalculateDiscount_LoadFromFile_ShouldApplyCorrectDiscount(
        string scenario,
        decimal originalAmount,
        CustomerLevel level,
        string discountCode,
        decimal expectedDiscount)
    {
        // Arrange
        // Uses mock discount calculation logic
        decimal discount = CalculateMockDiscount(originalAmount, level, discountCode);

        // Assert
        await Assert.That(discount).IsEqualTo(expectedDiscount);
    }

    /// <summary>
    /// Load test data from JSON file
    /// Requires creating TestData/discount-scenarios.json in project
    /// </summary>
    public static IEnumerable<object[]> GetDiscountTestDataFromFile()
    {
        // Simulated JSON data (in real projects, load from file)
        var scenarios = new List<DiscountScenario>
        {
            new() { Scenario = "Regular member no discount code", Amount = 1000, Level = 0, Code = "", Expected = 0 },
            new() { Scenario = "VIP member with VIP code", Amount = 1000, Level = 1, Code = "VIP50", Expected = 50 },
            new() { Scenario = "Platinum member with SAVE20 code", Amount = 1000, Level = 2, Code = "SAVE20", Expected = 250 }
        };

        foreach (var s in scenarios)
        {
            yield return new object[] { s.Scenario, s.Amount, (CustomerLevel)s.Level, s.Code, s.Expected };
        }
    }

    private static decimal CalculateMockDiscount(decimal amount, CustomerLevel level, string code)
    {
        return level switch
        {
            CustomerLevel.RegularMember => 0,
            CustomerLevel.VipMember when code == "VIP50" => 50,
            CustomerLevel.PlatinumMember when code == "SAVE20" => 250,
            _ => 0
        };
    }

    /// <summary>
    /// JSON data structure
    /// </summary>
    private class DiscountScenario
    {
        public string Scenario { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Level { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Expected { get; set; }
    }
}

/// <summary>
/// TestDataHelper - Unified test data loading management
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Generic method to load test data from JSON file
    /// </summary>
    public static IEnumerable<object[]> LoadFromJson<T>(string fileName, Func<T, object[]> converter)
    {
        var filePath = Path.Combine("TestData", fileName);

        if (!File.Exists(filePath))
        {
            yield break;
        }

        var jsonData = File.ReadAllText(filePath);
        var items = JsonSerializer.Deserialize<T[]>(jsonData);

        if (items == null) yield break;

        foreach (var item in items)
        {
            yield return converter(item);
        }
    }
}

#endregion

#region ClassDataSource Examples

/// <summary>
/// ClassDataSource basic usage example
/// Uses class as data provider, suitable for sharing data and reusable test scenarios
/// </summary>
public class ClassDataSourceTests
{
    /// <summary>
    /// Uses ClassDataSource for order validation testing
    /// </summary>
    [Test]
    [ClassDataSource<OrderValidationTestData>]
    public async Task ValidateOrder_VariousValidationScenarios_ShouldReturnCorrectResult(OrderValidationScenario scenario)
    {
        // Arrange
        var isValid = ValidateOrder(scenario.Order);

        // Assert
        await Assert.That(isValid).IsEqualTo(scenario.ExpectedValid);
    }

    private static bool ValidateOrder(Order order)
    {
        if (string.IsNullOrEmpty(order.CustomerId))
            return false;
        if (order.Items.Count == 0)
            return false;
        return true;
    }
}

/// <summary>
/// Order validation test data provider class
/// Implements IEnumerable<T> interface
/// </summary>
public class OrderValidationTestData : IEnumerable<OrderValidationScenario>
{
    public IEnumerator<OrderValidationScenario> GetEnumerator()
    {
        // Valid order
        yield return new OrderValidationScenario
        {
            Name = "Valid regular order",
            Order = CreateValidOrder(),
            ExpectedValid = true,
            ExpectedErrorKeyword = null
        };

        // Empty customer ID
        yield return new OrderValidationScenario
        {
            Name = "Empty customer ID",
            Order = CreateOrderWithEmptyCustomerId(),
            ExpectedValid = false,
            ExpectedErrorKeyword = "CustomerId"
        };

        // Empty item list
        yield return new OrderValidationScenario
        {
            Name = "No items",
            Order = CreateOrderWithNoItems(),
            ExpectedValid = false,
            ExpectedErrorKeyword = "Items"
        };
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private static Order CreateValidOrder() => new()
    {
        CustomerId = "CUST001",
        CustomerLevel = CustomerLevel.RegularMember,
        Items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "Test Product", UnitPrice = 100m, Quantity = 1 }
        }
    };

    private static Order CreateOrderWithEmptyCustomerId() => new()
    {
        CustomerId = "",
        CustomerLevel = CustomerLevel.RegularMember,
        Items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "Test Product", UnitPrice = 100m, Quantity = 1 }
        }
    };

    private static Order CreateOrderWithNoItems() => new()
    {
        CustomerId = "CUST001",
        CustomerLevel = CustomerLevel.RegularMember,
        Items = new List<OrderItem>()
    };
}

#endregion

#region AutoFixture Integration Example

/*
 * AutoFixture Integration Example (requires AutoFixture package)
 *
 * Install package: dotnet add package AutoFixture
 *
 * public class AutoFixtureOrderTestData : IEnumerable<Order>
 * {
 *     private readonly Fixture _fixture;
 *
 *     public AutoFixtureOrderTestData()
 *     {
 *         _fixture = new Fixture();
 *
 *         // Customize Order generation rules
 *         _fixture.Customize<Order>(composer => composer
 *             .With(o => o.CustomerId, () => $"CUST{_fixture.Create<int>() % 1000:D3}")
 *             .With(o => o.CustomerLevel, () => _fixture.Create<CustomerLevel>())
 *             .With(o => o.Items, () => _fixture.CreateMany<OrderItem>(Random.Shared.Next(1, 5)).ToList()));
 *
 *         // Customize OrderItem generation rules
 *         _fixture.Customize<OrderItem>(composer => composer
 *             .With(oi => oi.ProductId, () => $"PROD{_fixture.Create<int>() % 1000:D3}")
 *             .With(oi => oi.ProductName, () => $"Test Product{_fixture.Create<int>() % 100}")
 *             .With(oi => oi.UnitPrice, () => Math.Round(_fixture.Create<decimal>() % 1000 + 1, 2))
 *             .With(oi => oi.Quantity, () => _fixture.Create<int>() % 10 + 1));
 *     }
 *
 *     public IEnumerator<Order> GetEnumerator()
 *     {
 *         for (int i = 0; i < 5; i++)
 *         {
 *             yield return _fixture.Create<Order>();
 *         }
 *     }
 *
 *     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
 * }
 *
 * // Usage
 * [Test]
 * [ClassDataSource(typeof(AutoFixtureOrderTestData))]
 * public async Task ProcessOrder_AutoGeneratedTestData_ShouldCalculateOrderAmountCorrectly(Order order)
 * {
 *     await Assert.That(order).IsNotNull();
 *     await Assert.That(order.CustomerId).IsNotEmpty();
 *     await Assert.That(order.Items).IsNotEmpty();
 * }
 */

#endregion
