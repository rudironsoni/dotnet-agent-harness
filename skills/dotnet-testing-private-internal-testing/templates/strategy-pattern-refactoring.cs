using System;
using Xunit;
using AwesomeAssertions;
using NSubstitute;

/// <summary>
/// Strategy pattern refactoring example
/// Demonstrates how to improve testability through strategy pattern, avoiding private method testing
/// </summary>
///
// ========================================
// Before Refactoring: Hard to test design
// ========================================

namespace MyProject.BeforeRefactoring;

/// <summary>
/// Before refactoring: Pricing service with complex private methods
/// </summary>
public class PricingService
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;

        // Complex private methods, hard to test independently
        var discount = CalculateDiscount(customer, product);
        var tax = CalculateTax(product, customer.Location);

        return basePrice - discount + tax;
    }

    /// <summary>
    /// Private method: Calculate discount (20 lines of complex logic)
    /// </summary>
    private decimal CalculateDiscount(Customer customer, Product product)
    {
        decimal discount = 0;

        // VIP discount
        if (customer.IsVIP)
            discount += product.BasePrice * 0.1m;

        // Bulk purchase discount
        if (customer.PurchaseHistory > 10000)
            discount += product.BasePrice * 0.05m;

        // Seasonal discount
        if (DateTime.Now.Month == 12)
            discount += product.BasePrice * 0.05m;

        // Product category discount
        if (product.Category == "Electronics")
            discount += product.BasePrice * 0.03m;

        return Math.Min(discount, product.BasePrice * 0.3m); // Max 30% discount
    }

    /// <summary>
    /// Private method: Calculate tax (15 lines of complex logic)
    /// </summary>
    private decimal CalculateTax(Product product, Location location)
    {
        var taxRate = 0.05m; // Base tax rate

        // Adjust by region
        if (location.Country == "US")
        {
            if (location.City == "New York")
                taxRate = 0.08m;
            else
                taxRate = 0.06m;
        }

        // Adjust by product type
        if (product.Category == "Food")
            taxRate = 0m; // Food is tax exempt

        return product.BasePrice * taxRate;
    }
}


// ========================================
// After Refactoring: Using Strategy Pattern
// ========================================

namespace MyProject.AfterRefactoring;

// ========================================
// 1. Define Strategy Interfaces
// ========================================

/// <summary>
/// Discount calculation strategy interface
/// </summary>
public interface IDiscountStrategy
{
    decimal Calculate(Customer customer, Product product);
}

/// <summary>
/// Tax calculation strategy interface
/// </summary>
public interface ITaxStrategy
{
    decimal Calculate(Product product, Location location);
}


// ========================================
// 2. Implement Concrete Strategies
// ========================================

/// <summary>
/// Standard discount strategy
/// </summary>
public class StandardDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Customer customer, Product product)
    {
        decimal discount = 0;

        // VIP discount
        if (customer.IsVIP)
            discount += product.BasePrice * 0.1m;

        // Bulk purchase discount
        if (customer.PurchaseHistory > 10000)
            discount += product.BasePrice * 0.05m;

        return Math.Min(discount, product.BasePrice * 0.3m);
    }
}

/// <summary>
/// Seasonal discount strategy
/// </summary>
public class SeasonalDiscountStrategy : IDiscountStrategy
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SeasonalDiscountStrategy(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public decimal Calculate(Customer customer, Product product)
    {
        var baseStrategy = new StandardDiscountStrategy();
        var baseDiscount = baseStrategy.Calculate(customer, product);

        // December extra discount
        if (_dateTimeProvider.Now.Month == 12)
            baseDiscount += product.BasePrice * 0.05m;

        return Math.Min(baseDiscount, product.BasePrice * 0.3m);
    }
}

/// <summary>
/// US Tax strategy
/// </summary>
public class USTaxStrategy : ITaxStrategy
{
    public decimal Calculate(Product product, Location location)
    {
        // Food is tax exempt
        if (product.Category == "Food")
            return 0;

        // Determine tax rate by city
        var taxRate = location.City == "New York" ? 0.08m : 0.06m;

        return product.BasePrice * taxRate;
    }
}


// ========================================
// 3. Improved Pricing Service
// ========================================

/// <summary>
/// Refactored pricing service
/// Uses strategy pattern, dependency injection, easy to test
/// </summary>
public class PricingService
{
    private readonly IDiscountStrategy _discountStrategy;
    private readonly ITaxStrategy _taxStrategy;

    public PricingService(
        IDiscountStrategy discountStrategy,
        ITaxStrategy taxStrategy)
    {
        _discountStrategy = discountStrategy;
        _taxStrategy = taxStrategy;
    }

    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = _discountStrategy.Calculate(customer, product);
        var tax = _taxStrategy.Calculate(product, customer.Location);

        return basePrice - discount + tax;
    }
}


// ========================================
// Testing Examples
// ========================================

namespace MyProject.Tests;

/// <summary>
/// Strategy independent tests: Discount strategy
/// </summary>
public class StandardDiscountStrategyTests
{
    [Fact]
    public void Calculate_VIPCustomer_ShouldGive10PercentDiscount()
    {
        // Arrange
        var strategy = new StandardDiscountStrategy();
        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(100m); // 1000 * 0.1 = 100
    }

    [Fact]
    public void Calculate_BulkPurchaseCustomer_ShouldGive5PercentDiscount()
    {
        // Arrange
        var strategy = new StandardDiscountStrategy();
        var customer = new Customer { IsVIP = false, PurchaseHistory = 15000 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(50m); // 1000 * 0.05 = 50
    }

    [Fact]
    public void Calculate_VIPAndBulkPurchase_ShouldGiveCumulativeDiscountButNotExceed30Percent()
    {
        // Arrange
        var strategy = new StandardDiscountStrategy();
        var customer = new Customer { IsVIP = true, PurchaseHistory = 15000 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(150m); // (10% + 5%) = 15%
    }
}

/// <summary>
/// Seasonal discount strategy tests
/// </summary>
public class SeasonalDiscountStrategyTests
{
    [Fact]
    public void Calculate_December_ShouldGiveExtra5PercentDiscount()
    {
        // Arrange
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.Now.Returns(new DateTime(2024, 12, 15));

        var strategy = new SeasonalDiscountStrategy(dateTimeProvider);
        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(150m); // VIP 10% + Seasonal 5% = 15%
    }

    [Fact]
    public void Calculate_NotDecember_ShouldNotGiveExtraDiscount()
    {
        // Arrange
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.Now.Returns(new DateTime(2024, 6, 15));

        var strategy = new SeasonalDiscountStrategy(dateTimeProvider);
        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m };

        // Act
        var discount = strategy.Calculate(customer, product);

        // Assert
        discount.Should().Be(100m); // Only VIP 10%
    }
}

/// <summary>
/// Tax strategy tests
/// </summary>
public class USTaxStrategyTests
{
    [Fact]
    public void Calculate_NewYorkCity_ShouldCalculate8PercentTax()
    {
        // Arrange
        var strategy = new USTaxStrategy();
        var product = new Product { BasePrice = 1000m, Category = "Electronics" };
        var location = new Location { Country = "US", City = "New York" };

        // Act
        var tax = strategy.Calculate(product, location);

        // Assert
        tax.Should().Be(80m); // 1000 * 0.08 = 80
    }

    [Fact]
    public void Calculate_NonNewYorkCity_ShouldCalculate6PercentTax()
    {
        // Arrange
        var strategy = new USTaxStrategy();
        var product = new Product { BasePrice = 1000m, Category = "Electronics" };
        var location = new Location { Country = "US", City = "Los Angeles" };

        // Act
        var tax = strategy.Calculate(product, location);

        // Assert
        tax.Should().Be(60m); // 1000 * 0.06 = 60
    }

    [Fact]
    public void Calculate_FoodCategory_ShouldBeTaxExempt()
    {
        // Arrange
        var strategy = new USTaxStrategy();
        var product = new Product { BasePrice = 1000m, Category = "Food" };
        var location = new Location { Country = "US", City = "New York" };

        // Act
        var tax = strategy.Calculate(product, location);

        // Assert
        tax.Should().Be(0m);
    }
}

/// <summary>
/// Pricing service integration tests
/// </summary>
public class PricingServiceTests
{
    [Fact]
    public void CalculatePrice_IntegrationTest_ShouldCalculateFinalPriceCorrectly()
    {
        // Arrange
        var discountStrategy = new StandardDiscountStrategy();
        var taxStrategy = new USTaxStrategy();
        var service = new PricingService(discountStrategy, taxStrategy);

        var customer = new Customer { IsVIP = true, PurchaseHistory = 0 };
        var product = new Product { BasePrice = 1000m, Category = "Electronics" };
        var location = new Location { Country = "US", City = "New York" };
        customer.Location = location;

        // Act
        var finalPrice = service.CalculatePrice(product, customer);

        // Assert
        // 1000 (base price) - 100 (VIP 10% discount) + 80 (8% tax) = 980
        finalPrice.Should().Be(980m);
    }

    [Fact]
    public void CalculatePrice_UsingMock_CanTestLogicIndependently()
    {
        // Arrange
        var discountStrategy = Substitute.For<IDiscountStrategy>();
        var taxStrategy = Substitute.For<ITaxStrategy>();

        discountStrategy.Calculate(Arg.Any<Customer>(), Arg.Any<Product>())
            .Returns(100m);
        taxStrategy.Calculate(Arg.Any<Product>(), Arg.Any<Location>())
            .Returns(50m);

        var service = new PricingService(discountStrategy, taxStrategy);
        var customer = new Customer();
        var product = new Product { BasePrice = 1000m };

        // Act
        var finalPrice = service.CalculatePrice(product, customer);

        // Assert
        finalPrice.Should().Be(950m); // 1000 - 100 + 50

        // Verify strategies were called correctly
        discountStrategy.Received(1).Calculate(customer, product);
        taxStrategy.Received(1).Calculate(product, customer.Location);
    }
}


// ========================================
// Supporting Class Definitions
// ========================================

public class Product
{
    public decimal BasePrice { get; set; }
    public string Category { get; set; }
}

public class Customer
{
    public bool IsVIP { get; set; }
    public decimal PurchaseHistory { get; set; }
    public Location Location { get; set; }
}

public class Location
{
    public string Country { get; set; }
    public string City { get; set; }
}

public interface IDateTimeProvider
{
    DateTime Now { get; }
}


// ========================================
// Before and After Comparison
// ========================================

/*
Problems before refactoring:

1. Difficult to test
   ❌ Private methods cannot be tested directly
   ❌ Need to use reflection, increasing maintenance cost
   ❌ Tests are fragile, easy to fail during refactoring

2. Design issues
   ❌ Single class has multiple responsibilities
   ❌ Violates Open/Closed Principle (adding discount types requires modifying class)
   ❌ Hard to extend new strategies

3. Readability issues
   ❌ Complex logic hidden in private methods
   ❌ Hard to understand overall design intent

Advantages after refactoring:

1. Test friendly
   ✅ Each strategy can be tested independently
   ✅ No need to use reflection
   ✅ Tests are stable, not easy to fail during refactoring

2. Good design
   ✅ Follows Single Responsibility Principle
   ✅ Follows Open/Closed Principle
   ✅ Follows Dependency Inversion Principle
   ✅ Easy to extend new strategies

3. High readability
   ✅ Clear intent (discount strategy, tax strategy)
   ✅ Easy to understand and maintain
   ✅ Clear responsibilities

4. High flexibility
   ✅ Can dynamically switch strategies
   ✅ Can combine different strategies
   ✅ Easy to test different combinations

Refactoring steps:

1. Identify complex private methods
2. Define strategy interfaces
3. Extract private method logic into strategy implementations
4. Modify original class to use dependency injection
5. Write independent strategy tests
6. Write integration tests to verify combinations

When to consider strategy pattern refactoring:

✅ Private methods exceed 10 lines
✅ Contain important business rules
✅ Have multiple variants or algorithms
✅ Need to frequently extend new implementations
✅ Test coverage is insufficient

Remember: Good design naturally has good testability.
Rather than worrying about how to test private methods, improve design to make testing simple.
*/
