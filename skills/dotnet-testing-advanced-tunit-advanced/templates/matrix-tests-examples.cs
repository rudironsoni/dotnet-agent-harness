// TUnit Matrix Tests Combination Test Examples

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

namespace TUnit.Advanced.Matrix.Examples;

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
    public CustomerLevel CustomerLevel { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public decimal SubTotal => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class OrderItem
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

#endregion

#region Matrix Tests Examples

/// <summary>
/// Matrix Tests basic usage example
/// Automatically generates all parameter combination test cases
/// </summary>
public class MatrixTestsBasicExamples
{
    /// <summary>
    /// Basic Matrix test
    /// Generates 4 x 4 = 16 test cases
    ///
    /// Important notes:
    /// - Use [MatrixDataSource] attribute to mark test method
    /// - Due to C# attribute limitations, enum must be represented by numeric values
    /// - TUnit automatically converts numeric values to corresponding enum values
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task CalculateShipping_CustomerLevelAndAmountCombination_ShouldFollowShippingRules(
        [Matrix(0, 1, 2, 3)] CustomerLevel customerLevel, // 0=Regular, 1=VIP, 2=Platinum, 3=Diamond
        [Matrix(100, 500, 1000, 2000)] decimal orderAmount)
    {
        // Arrange
        var order = new Order
        {
            CustomerLevel = customerLevel,
            Items = [new OrderItem { UnitPrice = orderAmount, Quantity = 1 }]
        };

        // Act
        var shippingFee = CalculateShippingFee(order);
        var isFreeShipping = IsEligibleForFreeShipping(order);

        // Assert - Verify shipping logic consistency
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

            case CustomerLevel.VipMember or CustomerLevel.PlatinumMember:
                if (orderAmount < 1000m)
                {
                    await Assert.That(shippingFee).IsEqualTo(40m); // VIP+ half price shipping
                }
                break;

            case CustomerLevel.RegularMember:
                if (orderAmount < 1000m)
                {
                    await Assert.That(shippingFee).IsEqualTo(80m); // Regular member standard shipping
                }
                break;
        }
    }

    /// <summary>
    /// Discount logic Matrix test
    /// Focus on core combinations - total 2 x 4 = 8 tests
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task TestDiscountLogic_MemberAndAmountCombination_ShouldCalculateCorrectly(
        [Matrix(true, false)] bool isMember,
        [Matrix(0, 1, 100, 1000)] int amount)
    {
        // Arrange
        var discount = CalculateMemberDiscount(isMember, amount);

        // Assert
        if (!isMember)
        {
            await Assert.That(discount).IsEqualTo(0m);
        }
        else if (amount >= 1000)
        {
            await Assert.That(discount).IsGreaterThan(0m);
        }
    }

    /// <summary>
    /// Boolean combination Matrix test
    /// Generates 2 x 2 = 4 tests
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task ValidateInput_BooleanCombination_ShouldValidateCorrectly(
        [Matrix(true, false)] bool hasEmail,
        [Matrix(true, false)] bool hasPhone)
    {
        // Arrange
        var isValid = hasEmail || hasPhone; // At least one contact method required

        // Assert
        if (hasEmail || hasPhone)
        {
            await Assert.That(isValid).IsTrue();
        }
        else
        {
            await Assert.That(isValid).IsFalse();
        }
    }

    /// <summary>
    /// String and numeric combination Matrix test
    /// Generates 3 x 4 = 12 tests
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task ProcessPayment_PaymentMethodAndAmountCombination_ShouldHandleCorrectly(
        [Matrix("CreditCard", "DebitCard", "BankTransfer")] string paymentMethod,
        [Matrix(100, 500, 1000, 5000)] decimal amount)
    {
        // Arrange
        var fee = CalculatePaymentFee(paymentMethod, amount);

        // Assert
        await Assert.That(fee).IsGreaterThanOrEqualTo(0);

        // Specific rule validation
        if (paymentMethod == "BankTransfer" && amount >= 1000)
        {
            await Assert.That(fee).IsEqualTo(0); // Bank transfer free above threshold
        }
    }

    #region Helper Methods

    private static decimal CalculateShippingFee(Order order)
    {
        // Diamond members always get free shipping
        if (order.CustomerLevel == CustomerLevel.DiamondMember)
            return 0m;

        // Free shipping threshold
        if (order.SubTotal >= 1000m)
            return 0m;

        // VIP and Platinum members get half price shipping
        if (order.CustomerLevel == CustomerLevel.VipMember ||
            order.CustomerLevel == CustomerLevel.PlatinumMember)
            return 40m;

        // Regular member standard shipping
        return 80m;
    }

    private static bool IsEligibleForFreeShipping(Order order)
    {
        return order.CustomerLevel == CustomerLevel.DiamondMember || order.SubTotal >= 1000m;
    }

    private static decimal CalculateMemberDiscount(bool isMember, int amount)
    {
        if (!isMember) return 0m;
        if (amount >= 1000) return amount * 0.1m;
        if (amount >= 100) return amount * 0.05m;
        return 0m;
    }

    private static decimal CalculatePaymentFee(string paymentMethod, decimal amount)
    {
        return paymentMethod switch
        {
            "CreditCard" => amount * 0.03m,
            "DebitCard" => amount * 0.01m,
            "BankTransfer" when amount >= 1000 => 0m,
            "BankTransfer" => 30m,
            _ => 0m
        };
    }

    #endregion
}

#endregion

#region Matrix Tests Best Practices

/// <summary>
/// Matrix Tests best practices examples
/// Shows how to effectively use Matrix Tests to avoid common pitfalls
/// </summary>
public class MatrixTestsBestPractices
{
    /// <summary>
    /// Good practice: Focus on core combinations
    /// Total 2 x 4 = 8 tests, each with clear meaning
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task GoodPractice_FocusOnCoreCombinations_AvoidTooManyTests(
        [Matrix(true, false)] bool isMember,
        [Matrix(0, 1, 100, 1000)] int amount) // Key amount thresholds
    {
        var discount = isMember && amount >= 100 ? amount * 0.05m : 0;
        await Assert.That(discount).IsGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Matrix Tests suitable scenario:
    /// Business rule cross validation
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task BusinessRuleValidation_OrderLimitValidation(
        [Matrix(0, 1, 2, 3)] CustomerLevel level, // Customer level
        [Matrix(1, 5, 10, 50)] int itemCount)     // Item count
    {
        // Different levels have different item count limits
        var maxItems = level switch
        {
            CustomerLevel.DiamondMember => 100,
            CustomerLevel.PlatinumMember => 50,
            CustomerLevel.VipMember => 30,
            _ => 20
        };

        var isValid = itemCount <= maxItems;

        await Assert.That(isValid).IsTrue();
    }

    /// <summary>
    /// Matrix Tests suitable scenario:
    /// API parameter validation
    /// </summary>
    [Test]
    [MatrixDataSource]
    public async Task ApiParameterValidation_PageParameterValidation(
        [Matrix(1, 10, 50, 100)] int pageSize,
        [Matrix(1, 2, 10, 100)] int pageNumber)
    {
        // Validate pagination parameter validity
        var isValidPageSize = pageSize >= 1 && pageSize <= 100;
        var isValidPageNumber = pageNumber >= 1;

        await Assert.That(isValidPageSize).IsTrue();
        await Assert.That(isValidPageNumber).IsTrue();
    }
}

#endregion

#region Matrix Tests Warnings

/// <summary>
/// Matrix Tests warnings and notes
/// Shows anti-patterns to avoid
/// </summary>
public class MatrixTestsWarnings
{
    /*
     * Avoid: Too many combinations cause exponential growth
     *
     * The following example generates 5 x 4 x 3 x 6 = 360 test cases!
     * This makes test execution time too long
     *
     * [Test]
     * [MatrixDataSource]
     * public async Task TooManyParameters_TooManyCombinations_ShouldBeAvoided(
     *     [Matrix(1, 2, 3, 4, 5)] int quantity,
     *     [Matrix(0, 1, 2, 3)] CustomerLevel level,
     *     [Matrix(true, false, null)] bool? expedited,
     *     [Matrix("Standard", "Express", "Overnight", "International", "Pickup", "Digital")] string method)
     * {
     *     // 360 test cases make test execution time too long!
     * }
     */

    /*
     * C# enum constant limitation
     *
     * In C#, enum values cannot be used directly as attribute constants
     *
     * This will cause compile error:
     * [Test]
     * [MatrixDataSource]
     * public async Task TestMethod(
     *     [Matrix(CustomerLevel.RegularMember, CustomerLevel.VipMember)] CustomerLevel level)
     * {
     * }
     *
     * Correct approach: Use numeric values to represent enum
     * [Test]
     * [MatrixDataSource]
     * public async Task TestMethod(
     *     [Matrix(0, 1, 2, 3)] CustomerLevel level) // 0=Regular, 1=VIP, 2=Platinum, 3=Diamond
     * {
     * }
     */

    /// <summary>
    /// Practical recommendations:
    /// 1. Limit parameter combinations, avoid exceeding 50-100 cases
    /// 2. Consider using [Arguments] to specify important combinations
    /// 3. Use Theory tests to supplement edge cases
    /// 4. Matrix Tests are not suitable for long-running integration tests
    /// </summary>
    [Test]
    public async Task MatrixTestsGuidelines_BestPracticeTips()
    {
        // This is just a documentation test
        await Assert.That(true).IsTrue();
    }
}

#endregion
