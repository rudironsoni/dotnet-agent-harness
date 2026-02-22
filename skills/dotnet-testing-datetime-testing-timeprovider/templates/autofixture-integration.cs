// =============================================================================
// AutoFixture and TimeProvider Integration Examples
// Demonstrates how to combine AutoFixture for automated time testing
// =============================================================================

using System;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace TimeProviderExamples.Tests;

#region FakeTimeProvider Customization

/// <summary>
/// AutoFixture Customization for FakeTimeProvider
/// Lets AutoFixture know how to create FakeTimeProvider
/// </summary>
public class FakeTimeProviderCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Register FakeTimeProvider creation method
        fixture.Register(() => new FakeTimeProvider());
    }
}

#endregion

#region AutoDataWithCustomization Attribute

/// <summary>
/// Custom AutoData attribute integrating NSubstitute and FakeTimeProvider
/// </summary>
public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new FakeTimeProviderCustomization());
    }
}

/// <summary>
/// Attribute combining InlineData and AutoFixture
/// Used for parameterized tests with auto-generated objects
/// </summary>
public class InlineAutoDataWithCustomizationAttribute : InlineAutoDataAttribute
{
    public InlineAutoDataWithCustomizationAttribute(params object[] values)
        : base(new AutoDataWithCustomizationAttribute(), values)
    {
    }
}

#endregion

#region Traditional vs AutoFixture Approach Comparison

/// <summary>
/// Traditional test approach - manually create all objects
/// </summary>
public class OrderServiceTraditionalTests
{
    [Fact]
    public void CanPlaceOrder_DuringBusinessHours_TraditionalApproach()
    {
        // Arrange - need to manually create all objects
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));

        var orderService = new OrderService(fakeTimeProvider);

        // Act
        var result = orderService.CanPlaceOrder();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTimeBasedDiscount_Friday_TraditionalApproach()
    {
        // Arrange - repeat these settings every time
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0)); // Friday

        var orderService = new OrderService(fakeTimeProvider);

        // Act
        var discount = orderService.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("Friday Special: 10% Discount");
    }
}

/// <summary>
/// AutoFixture test approach - automated object creation
/// </summary>
public class OrderServiceAutoFixtureTests
{
    /// <summary>
    /// Simplify tests with AutoFixture
    /// [Frozen(Matching.DirectBaseType)] is the key!
    /// </summary>
    [Theory]
    [AutoDataWithCustomization]
    public void GetTimeBasedDiscount_Monday_ShouldReturnNoDiscount(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut) // sut = System Under Test, auto-created by AutoFixture
    {
        // Arrange - only set time relevant to test
        var mondayTime = new DateTime(2024, 3, 11, 14, 0, 0); // 2024/3/11 is Monday
        fakeTimeProvider.SetLocalNow(mondayTime);

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("No Discount");
    }

    [Theory]
    [AutoDataWithCustomization]
    public void GetTimeBasedDiscount_Friday_ShouldReturnTenPercentDiscount(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange - set to Friday
        var fridayTime = new DateTime(2024, 3, 15, 14, 0, 0); // 2024/3/15 is Friday
        fakeTimeProvider.SetLocalNow(fridayTime);

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("Friday Special: 10% Discount");
    }

    [Theory]
    [AutoDataWithCustomization]
    public void GetTimeBasedDiscount_Christmas_ShouldReturnTwentyPercentDiscount(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 25, 10, 0, 0));

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("Christmas Special: 20% Discount");
    }

    [Theory]
    [AutoDataWithCustomization]
    public void CanPlaceOrder_BusinessHoursBoundaryTest_9AM(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange - 9:00 AM (business hours start)
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 9, 0, 0));

        // Act & Assert
        sut.CanPlaceOrder().Should().BeTrue();
    }

    [Theory]
    [AutoDataWithCustomization]
    public void CanPlaceOrder_BusinessHoursBoundaryTest_5PM(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut)
    {
        // Arrange - 5:00 PM (business hours end)
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 17, 0, 0));

        // Act & Assert
        sut.CanPlaceOrder().Should().BeFalse();
    }
}

#endregion

#region Matching.DirectBaseType Detailed Explanation

/*
 * ============================================================
 * Importance of Matching.DirectBaseType
 * ============================================================
 *
 * Problem:
 * OrderService constructor needs TimeProvider (abstract class)
 * But we want to use FakeTimeProvider (derived class) in tests
 *
 * Without Matching.DirectBaseType:
 *
 *     [Theory]
 *     [AutoDataWithCustomization]
 *     public void Test([Frozen] FakeTimeProvider provider, OrderService sut)
 *     {
 *         // ❌ Fails! AutoFixture only knows FakeTimeProvider
 *         // When creating OrderService, it creates a separate TimeProvider
 *         // Causing provider and sut to use different time sources
 *     }
 *
 * Solution using Matching.DirectBaseType:
 *
 *     [Theory]
 *     [AutoDataWithCustomization]
 *     public void Test([Frozen(Matching.DirectBaseType)] FakeTimeProvider provider, OrderService sut)
 *     {
 *         // ✅ Works! AutoFixture registers FakeTimeProvider as TimeProvider
 *         // When creating OrderService, uses the same FakeTimeProvider instance
 *     }
 *
 * Workflow:
 * 1. AutoFixture sees need to create OrderService
 * 2. Discovers OrderService constructor needs TimeProvider parameter
 * 3. Checks for [Frozen] marked instances to satisfy this need
 * 4. Finds [Frozen(Matching.DirectBaseType)] FakeTimeProvider
 * 5. Confirms TimeProvider is FakeTimeProvider's direct base type
 * 6. Injects FakeTimeProvider instance into OrderService constructor
 */

#endregion

#region Cache Tests with AutoFixture Integration

/// <summary>
/// Cache tests combined with AutoFixture
/// AutoFixture generates test data (key, value)
/// </summary>
public class TimedCacheAutoFixtureTests
{
    [Theory]
    [AutoDataWithCustomization]
    public void TimedCache_UsingAutoFixtureTestExpiration_ShouldHandleCorrectly(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        string key,    // AutoFixture auto-generates
        string value)  // AutoFixture auto-generates
    {
        // Arrange
        var startTime = new DateTime(2024, 3, 15, 10, 0, 0);
        fakeTimeProvider.SetLocalNow(startTime);

        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(30));

        // Act & Assert - set and get immediately
        cache.Set(key, value);
        cache.Get(key).Should().Be(value);

        // Act & Assert - should expire after advancing time
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(31));
        cache.Get(key).Should().BeNull();
    }

    [Theory]
    [AutoDataWithCustomization]
    public void TimedCache_MultipleItemsTest(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        string key1, string value1,
        string key2, string value2,
        string key3, string value3)
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));
        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(10));

        // Act - set multiple items
        cache.Set(key1, value1);
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
        cache.Set(key2, value2);
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
        cache.Set(key3, value3);

        // Assert - at this point key1 is 6 min old, key2 is 3 min, key3 just set
        // Advance another 5 minutes (total 11 minutes)
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        cache.Get(key1).Should().BeNull();   // 11 min > 10 min, expired
        cache.Get(key2).Should().Be(value2); // 8 min < 10 min, still valid
        cache.Get(key3).Should().Be(value3); // 5 min < 10 min, still valid
    }
}

#endregion

#region InlineAutoData Combined with Parameterized Tests

/// <summary>
/// Using InlineAutoDataWithCustomization with parameterized tests
/// Some parameters from InlineData, others from AutoFixture
/// </summary>
public class OrderServiceInlineAutoDataTests
{
    [Theory]
    [InlineAutoDataWithCustomization(8, false)]   // 8 AM - before business hours
    [InlineAutoDataWithCustomization(9, true)]    // 9 AM - business starts
    [InlineAutoDataWithCustomization(12, true)]   // 12 PM - within business hours
    [InlineAutoDataWithCustomization(16, true)]   // 4 PM - within business hours
    [InlineAutoDataWithCustomization(17, false)]  // 5 PM - business ends
    [InlineAutoDataWithCustomization(18, false)]  // 6 PM - after business hours
    public void CanPlaceOrder_DifferentTimes_AutoFixtureVersion(
        int hour,      // From InlineData
        bool expected, // From InlineData
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        OrderService sut) // Auto-created by AutoFixture
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, hour, 0, 0));

        // Act
        var result = sut.CanPlaceOrder();

        // Assert
        result.Should().Be(expected);
    }
}

#endregion

#region Schedule Service AutoFixture Tests

/// <summary>
/// Schedule service tests using AutoFixture
/// </summary>
public class ScheduleServiceAutoFixtureTests
{
    [Theory]
    [AutoDataWithCustomization]
    public void ShouldExecuteJob_ExecutionTimeReached_ShouldReturnTrue(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ScheduleService sut)
    {
        // Arrange
        var now = new DateTime(2024, 3, 15, 14, 30, 0);
        fakeTimeProvider.SetLocalNow(now);

        var schedule = new JobSchedule
        {
            NextExecutionTime = now.AddMinutes(-30), // Should have executed 30 min ago
            CronExpression = "0 0 * * *"
        };

        // Act
        var result = sut.ShouldExecuteJob(schedule);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [AutoDataWithCustomization]
    public void ShouldExecuteJob_NotYetExecutionTime_ShouldReturnFalse(
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ScheduleService sut)
    {
        // Arrange
        var now = new DateTime(2024, 3, 15, 14, 30, 0);
        fakeTimeProvider.SetLocalNow(now);

        var schedule = new JobSchedule
        {
            NextExecutionTime = now.AddMinutes(30), // Executes in 30 min
            CronExpression = "0 0 * * *"
        };

        // Act
        var result = sut.ShouldExecuteJob(schedule);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineAutoDataWithCustomization("2024-03-15 14:30:00", "2024-03-15 14:00:00", true)]
    [InlineAutoDataWithCustomization("2024-03-15 13:30:00", "2024-03-15 14:00:00", false)]
    [InlineAutoDataWithCustomization("2024-03-15 14:00:00", "2024-03-15 14:00:00", true)]
    public void ShouldExecuteJob_ParameterizedTest(
        string currentTimeStr,
        string scheduledTimeStr,
        bool expected,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ScheduleService sut)
    {
        // Arrange
        fakeTimeProvider.SetLocalNow(DateTime.Parse(currentTimeStr));

        var schedule = new JobSchedule
        {
            NextExecutionTime = DateTime.Parse(scheduledTimeStr)
        };

        // Act
        var result = sut.ShouldExecuteJob(schedule);

        // Assert
        result.Should().Be(expected);
    }
}

#endregion

#region AutoFixture Benefits Summary

/*
 * ============================================================
 * Benefits of AutoFixture with TimeProvider Integration
 * ============================================================
 *
 * 1. Reduces Boilerplate Code
 *    - No need to manually new FakeTimeProvider()
 *    - No need to manually new OrderService(fakeTimeProvider)
 *    - AutoFixture handles dependency injection automatically
 *
 * 2. Improves Test Coverage
 *    - Can easily generate multiple test cases
 *    - AutoFixture generates test data (e.g., key, value)
 *
 * 3. Maintains Test Isolation
 *    - Each test has its own independent FakeTimeProvider instance
 *    - No interference between tests
 *
 * 4. Enhances Readability
 *    - Tests focus more on business logic verification
 *    - Rather than object creation details
 *
 * 5. Improves Maintainability
 *    - When constructor parameters change, AutoFixture adapts automatically
 *    - Reduces scope of test code modifications
 *
 * ============================================================
 * When to Use AutoFixture?
 * ============================================================
 *
 * Recommended when:
 * - Test class has multiple similar test methods
 * - Class under test has complex constructor parameters
 * - Need many different test data combinations
 * - Want to reduce test code duplication
 *
 * Consider traditional approach when:
 * - Test cases are simple, only a few
 * - Need complete control over object creation process
 * - Team unfamiliar with AutoFixture, learning curve concerns
 */

#endregion
