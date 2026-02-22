# Data-Driven Testing Advanced Techniques

> This document is extracted from [SKILL.md](../SKILL.md), providing complete examples and details for TUnit data-driven testing.

## Data Source Comparison

| Data Source Method    | Use Case              | Advantages       | Notes                       |
| :-------------------- | :-------------------- | :--------------- | :-------------------------- |
| **Arguments**         | Simple fixed data     | Concise syntax   | Data volume should not be too large |
| **MethodDataSource**  | Dynamic data, complex objects | Maximum flexibility | Requires additional method definition |
| **ClassDataSource**   | Shared data, dependency injection | High reusability | Class lifecycle management  |
| **Matrix Tests**      | Combination testing   | High coverage    | Can easily generate too many tests |

## MethodDataSource: Method as Data Source

The most flexible data provision method, suitable for dynamically generating or loading data from external sources:

```csharp
[Test]
[MethodDataSource(nameof(GetOrderTestData))]
public async Task CreateOrder_VariousScenarios_ShouldHandleCorrectly(
    string customerId, 
    CustomerLevel level, 
    List<OrderItem> items, 
    decimal expectedTotal)
{
    // Arrange
    var orderService = new OrderService(_repository, _discountCalculator, _shippingCalculator, _logger);

    // Act
    var order = await orderService.CreateOrderAsync(customerId, level, items);

    // Assert
    await Assert.That(order).IsNotNull();
    await Assert.That(order.CustomerId).IsEqualTo(customerId);
    await Assert.That(order.TotalAmount).IsEqualTo(expectedTotal);
}

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
        CustomerLevel.VIPMember,
        new List<OrderItem>
        {
            new() { ProductId = "PROD002", ProductName = "Product B", UnitPrice = 500m, Quantity = 1 }
        },
        500m
    };
}
```

**Loading Test Data from File:**

```csharp
[Test]
[MethodDataSource(nameof(GetDiscountTestDataFromFile))]
public async Task CalculateDiscount_LoadedFromFile_ShouldApplyCorrectDiscount(
    string scenario, 
    decimal originalAmount, 
    CustomerLevel level, 
    string discountCode, 
    decimal expectedDiscount)
{
    var calculator = new DiscountCalculator(new MockDiscountRepository(), new MockLogger<DiscountCalculator>());
    var order = new Order
    {
        CustomerLevel = level,
        Items = [new OrderItem { UnitPrice = originalAmount, Quantity = 1 }]
    };

    var discount = await calculator.CalculateDiscountAsync(order, discountCode);

    await Assert.That(discount).IsEqualTo(expectedDiscount);
}

public static IEnumerable<object[]> GetDiscountTestDataFromFile()
{
    var filePath = Path.Combine("TestData", "discount-scenarios.json");
    var jsonData = File.ReadAllText(filePath);
    var scenarios = JsonSerializer.Deserialize<List<DiscountScenario>>(jsonData);
    if (scenarios == null) yield break;
    
    foreach (var s in scenarios)
    {
        yield return new object[] { s.Scenario, s.Amount, (CustomerLevel)s.Level, s.Code, s.Expected };
    }
}
```

## ClassDataSource: Class as Data Provider

Used when test data needs to be shared across multiple test classes:

```csharp
[Test]
[ClassDataSource<OrderValidationTestData>]
public async Task ValidateOrder_VariousValidationScenarios_ShouldReturnCorrectResult(OrderValidationScenario scenario)
{
    var validator = new OrderValidator(_discountRepository, _logger);
    var result = await validator.ValidateAsync(scenario.Order);

    await Assert.That(result.IsValid).IsEqualTo(scenario.ExpectedValid);
    if (!scenario.ExpectedValid)
    {
        await Assert.That(result.ErrorMessage).Contains(scenario.ExpectedErrorKeyword);
    }
}

public class OrderValidationTestData : IEnumerable<OrderValidationScenario>
{
    public IEnumerator<OrderValidationScenario> GetEnumerator()
    {
        yield return new OrderValidationScenario
        {
            Name = "Valid Regular Order",
            Order = CreateValidOrder(),
            ExpectedValid = true,
            ExpectedErrorKeyword = null
        };

        yield return new OrderValidationScenario
        {
            Name = "Empty Customer ID",
            Order = CreateOrderWithEmptyCustomerId(),
            ExpectedValid = false,
            ExpectedErrorKeyword = "Customer ID"
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
}
```

**AutoFixture Integration:**

```csharp
public class AutoFixtureOrderTestData : IEnumerable<Order>
{
    private readonly Fixture _fixture;

    public AutoFixtureOrderTestData()
    {
        _fixture = new Fixture();
        
        _fixture.Customize<Order>(composer => composer
            .With(o => o.CustomerId, () => $"CUST{_fixture.Create<int>() % 1000:D3}")
            .With(o => o.CustomerLevel, () => _fixture.Create<CustomerLevel>())
            .With(o => o.Items, () => _fixture.CreateMany<OrderItem>(Random.Shared.Next(1, 5)).ToList()));

        _fixture.Customize<OrderItem>(composer => composer
            .With(oi => oi.ProductId, () => $"PROD{_fixture.Create<int>() % 1000:D3}")
            .With(oi => oi.ProductName, () => $"Test Product {_fixture.Create<int>() % 100}")
            .With(oi => oi.UnitPrice, () => Math.Round(_fixture.Create<decimal>() % 1000 + 1, 2))
            .With(oi => oi.Quantity, () => _fixture.Create<int>() % 10 + 1));
    }

    public IEnumerator<Order> GetEnumerator()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return _fixture.Create<Order>();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

## Matrix Tests: Combination Testing

Automatically generates all parameter combination test cases:

```csharp
[Test]
[MatrixDataSource]
public async Task CalculateShipping_CustomerLevelAndAmountCombinations_ShouldFollowShippingRules(
    [Matrix(0, 1, 2, 3)] CustomerLevel customerLevel, // 0=Regular Member, 1=VIP Member, 2=Platinum Member, 3=Diamond Member
    [Matrix(100, 500, 1000, 2000)] decimal orderAmount)
{
    // Arrange
    var calculator = new ShippingCalculator();
    var order = new Order
    {
        CustomerLevel = customerLevel,
        Items = [new OrderItem { UnitPrice = orderAmount, Quantity = 1 }]
    };

    // Act
    var shippingFee = calculator.CalculateShippingFee(order);
    var isFreeShipping = calculator.IsEligibleForFreeShipping(order);

    // Assert
    if (isFreeShipping)
    {
        await Assert.That(shippingFee).IsEqualTo(0m);
    }
    else
    {
        await Assert.That(shippingFee).IsGreaterThan(0m);
    }

    // Verify specific rules
    switch (customerLevel)
    {
        case CustomerLevel.DiamondMember:
            await Assert.That(shippingFee).IsEqualTo(0m); // Diamond members always get free shipping
            break;
        case CustomerLevel.VIPMember or CustomerLevel.PlatinumMember:
            if (orderAmount < 1000m)
                await Assert.That(shippingFee).IsEqualTo(40m); // VIP+ half shipping fee
            break;
        case CustomerLevel.RegularMember:
            if (orderAmount < 1000m)
                await Assert.That(shippingFee).IsEqualTo(80m); // Regular members standard shipping fee
            break;
    }
}
```

**Matrix Tests Notes:**

- Use `[MatrixDataSource]` attribute to mark test methods
- Due to C# attribute limitations, enums must be represented as numeric values
- Limit parameter combinations to avoid exceeding 50-100 cases
- This will generate 4 x 4 = 16 test cases
