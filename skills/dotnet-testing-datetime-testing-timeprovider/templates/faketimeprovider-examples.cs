// =============================================================================
// FakeTimeProvider Testing Examples
// Demonstrates how to use FakeTimeProvider for time-controlled testing
// =============================================================================

using System;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace TimeProviderExamples.Tests;

#region FakeTimeProvider Extension Methods

/// <summary>
/// FakeTimeProvider extension methods for simplified time setting
/// </summary>
public static class FakeTimeProviderExtensions
{
    /// <summary>
    /// Sets FakeTimeProvider local time
    /// </summary>
    /// <param name="fakeTimeProvider">FakeTimeProvider instance</param>
    /// <param name="localDateTime">Local time to set</param>
    public static void SetLocalNow(this FakeTimeProvider fakeTimeProvider, DateTime localDateTime)
    {
        fakeTimeProvider.SetLocalTimeZone(TimeZoneInfo.Local);
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, TimeZoneInfo.Local);
        fakeTimeProvider.SetUtcNow(utcTime);
    }

    /// <summary>
    /// Sets FakeTimeProvider to specific date and hour
    /// </summary>
    public static void SetLocalNow(this FakeTimeProvider fakeTimeProvider, int year, int month, int day, int hour, int minute = 0, int second = 0)
    {
        var localDateTime = new DateTime(year, month, day, hour, minute, second);
        fakeTimeProvider.SetLocalNow(localDateTime);
    }
}

#endregion

#region Basic Time Control Tests

/// <summary>
/// OrderService unit tests - demonstrates basic FakeTimeProvider usage
/// </summary>
public class OrderServiceTests
{
    [Fact]
    public void CanPlaceOrder_DuringBusinessHours_ShouldReturnTrue()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // Set to 2 PM (within business hours)
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var result = sut.CanPlaceOrder();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanPlaceOrder_OutsideBusinessHours_ShouldReturnFalse()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // Set to 8 PM (outside business hours)
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 20, 0, 0));

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var result = sut.CanPlaceOrder();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetTimeBasedDiscount_Friday_ShouldReturnTenPercentDiscount()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // 2024/3/15 is Friday
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("Friday Special: 10% Discount");
    }

    [Fact]
    public void GetTimeBasedDiscount_Christmas_ShouldReturnTwentyPercentDiscount()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // Christmas
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 25, 10, 0, 0));

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("Christmas Special: 20% Discount");
    }

    [Fact]
    public void GetTimeBasedDiscount_RegularDate_ShouldReturnNoDiscount()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // 2024/3/11 is Monday (regular date)
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 11, 14, 0, 0));

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be("No Discount");
    }
}

#endregion

#region Parameterized Boundary Tests

/// <summary>
/// Parameterized tests - covering all boundary conditions
/// </summary>
public class OrderServiceBoundaryTests
{
    [Theory]
    [InlineData(8, false)]   // 8 AM - before business hours
    [InlineData(9, true)]    // 9 AM - business starts (boundary)
    [InlineData(12, true)]   // 12 PM - within business hours
    [InlineData(16, true)]   // 4 PM - within business hours
    [InlineData(17, false)]  // 5 PM - business ends (boundary)
    [InlineData(18, false)]  // 6 PM - after business hours
    [InlineData(0, false)]   // 12 AM - midnight
    [InlineData(23, false)]  // 11 PM - late night
    public void CanPlaceOrder_DifferentTimes_ShouldReturnCorrectResult(int hour, bool expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, hour, 0, 0));

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var result = sut.CanPlaceOrder();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2024-03-15", "Friday Special: 10% Discount")]  // Friday
    [InlineData("2024-03-11", "No Discount")]               // Monday
    [InlineData("2024-12-25", "Christmas Special: 20% Discount")]  // Christmas
    [InlineData("2024-03-16", "No Discount")]               // Saturday (not Friday)
    public void GetTimeBasedDiscount_DifferentDates_ShouldReturnCorrectDiscount(string dateStr, string expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var date = DateTime.Parse(dateStr);
        fakeTimeProvider.SetLocalNow(date.AddHours(12)); // 12 PM

        var sut = new OrderService(fakeTimeProvider);

        // Act
        var discount = sut.GetTimeBasedDiscount();

        // Assert
        discount.Should().Be(expected);
    }
}

#endregion

#region Time Freeze Tests

/// <summary>
/// Time freeze tests - verify multiple operations at same time point
/// </summary>
public class TimeFreezeTests
{
    [Fact]
    public void ProcessBatch_AtFixedTime_ShouldGenerateSameTimestamp()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var fixedTime = new DateTime(2024, 12, 25, 10, 30, 0);
        fakeTimeProvider.SetLocalNow(fixedTime);

        var processor = new BatchProcessor(fakeTimeProvider);

        // Act - execute multiple operations
        var result1 = processor.ProcessItem("Item1");
        var result2 = processor.ProcessItem("Item2");
        var result3 = processor.ProcessItem("Item3");

        // Assert - time is frozen, all timestamps should be same
        result1.Timestamp.Should().Be(fixedTime);
        result2.Timestamp.Should().Be(fixedTime);
        result3.Timestamp.Should().Be(fixedTime);
    }
}

public class BatchProcessor
{
    private readonly TimeProvider _timeProvider;

    public BatchProcessor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public ProcessResult ProcessItem(string item)
    {
        return new ProcessResult
        {
            Item = item,
            Timestamp = _timeProvider.GetLocalNow().DateTime
        };
    }
}

public class ProcessResult
{
    public string Item { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

#endregion

#region Time Advance Tests

/// <summary>
/// Time advance tests - using Advance() method
/// </summary>
public class TimeAdvanceTests
{
    [Fact]
    public void Cache_SetItemThenAdvanceTime_ShouldCorrectlyHandleExpiration()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var startTime = new DateTime(2024, 3, 15, 10, 0, 0);
        fakeTimeProvider.SetLocalNow(startTime);

        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(5));

        // Act & Assert - set cache item (time: 10:00)
        cache.Set("key1", "value1");
        cache.Get("key1").Should().Be("value1");

        // Advance time 3 minutes (time: 10:03), cache not expired (5 min limit)
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
        cache.Get("key1").Should().Be("value1"); // 3 < 5, still valid

        // Advance time another 3 minutes (time: 10:06), cache expired
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3)); // Total 6 min > 5 min limit
        cache.Get("key1").Should().BeNull(); // Expired, returns null
    }

    [Fact]
    public void Cache_BoundaryTest_ExactlyAtExpirationMoment()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));

        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(5));
        cache.Set("key", "value");

        // Act & Assert - still valid after 4 min 59 sec
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(59)));
        cache.Get("key").Should().Be("value");

        // Expires after another 2 seconds
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
        cache.Get("key").Should().BeNull();
    }

    [Fact]
    public void Token_UsingAdvanceTestsExpiration()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));

        var tokenService = new TokenService(fakeTimeProvider);
        var token = tokenService.GenerateToken("user123", TimeSpan.FromHours(1));

        // Act & Assert - should be valid immediately
        tokenService.ValidateToken(token).Should().BeTrue();

        // Still valid after 30 minutes
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(30));
        tokenService.ValidateToken(token).Should().BeTrue();

        // Expires after another 31 minutes (total 61 minutes)
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(31));
        tokenService.ValidateToken(token).Should().BeFalse();
    }
}

/// <summary>
/// Generic cache class
/// </summary>
public class TimedCache<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, CacheItem<T>> _cache = new();

    public TimeSpan DefaultExpiry { get; }

    public TimedCache(TimeProvider timeProvider, TimeSpan defaultExpiry)
    {
        _timeProvider = timeProvider;
        DefaultExpiry = defaultExpiry;
    }

    public void Set(string key, T value, TimeSpan? expiry = null)
    {
        var expiryTime = _timeProvider.GetUtcNow().Add(expiry ?? DefaultExpiry);
        _cache[key] = new CacheItem<T>(value, expiryTime);
    }

    public T? Get(string key)
    {
        if (!_cache.TryGetValue(key, out var item))
            return default;

        if (item.ExpiryTime <= _timeProvider.GetUtcNow())
        {
            _cache.Remove(key);
            return default;
        }

        return item.Value;
    }
}

public record CacheItem<T>(T Value, DateTimeOffset ExpiryTime);

/// <summary>
/// Token service example
/// </summary>
public class TokenService
{
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, TokenInfo> _tokens = new();

    public TokenService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public string GenerateToken(string userId, TimeSpan validity)
    {
        var token = Guid.NewGuid().ToString();
        var expiryTime = _timeProvider.GetUtcNow().Add(validity);

        _tokens[token] = new TokenInfo(userId, expiryTime);
        return token;
    }

    public bool ValidateToken(string token)
    {
        if (!_tokens.TryGetValue(token, out var info))
            return false;

        return info.ExpiryTime > _timeProvider.GetUtcNow();
    }
}

public record TokenInfo(string UserId, DateTimeOffset ExpiryTime);

#endregion

#region Time Rewind Tests

/// <summary>
/// Time rewind tests - historical data processing
/// </summary>
public class TimeRewindTests
{
    [Fact]
    public void HistoricalDataProcessor_RewindToPastTime_ShouldCorrectlyProcessHistoricalData()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // Go back to a day in 2020
        var historicalTime = new DateTime(2020, 1, 15, 9, 0, 0);
        fakeTimeProvider.SetLocalNow(historicalTime);

        var processor = new HistoricalDataProcessor(fakeTimeProvider);

        // Act
        var result = processor.ProcessDataForDate(historicalTime.Date);

        // Assert
        result.Should().NotBeNull();
        result.ProcessedAt.Should().Be(historicalTime);
    }
}

public class HistoricalDataProcessor
{
    private readonly TimeProvider _timeProvider;

    public HistoricalDataProcessor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public HistoricalResult ProcessDataForDate(DateTime date)
    {
        return new HistoricalResult
        {
            Date = date,
            ProcessedAt = _timeProvider.GetLocalNow().DateTime
        };
    }
}

public class HistoricalResult
{
    public DateTime Date { get; set; }
    public DateTime ProcessedAt { get; set; }
}

#endregion

#region Schedule and Trading Time Tests

/// <summary>
/// Schedule service tests
/// </summary>
public class ScheduleServiceTests
{
    [Theory]
    [InlineData("2024-03-15 14:30:00", "2024-03-15 14:00:00", true)]  // Execution time reached
    [InlineData("2024-03-15 13:30:00", "2024-03-15 14:00:00", false)] // Not yet execution time
    [InlineData("2024-03-15 14:00:00", "2024-03-15 14:00:00", true)]  // Exactly at execution time (boundary)
    public void ShouldExecuteJob_BasedOnTime_ShouldReturnCorrectResult(
        string currentTimeStr,
        string scheduledTimeStr,
        bool expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var currentTime = DateTime.Parse(currentTimeStr);
        var scheduledTime = DateTime.Parse(scheduledTimeStr);

        fakeTimeProvider.SetLocalNow(currentTime);

        var schedule = new JobSchedule { NextExecutionTime = scheduledTime };
        var sut = new ScheduleService(fakeTimeProvider);

        // Act
        var result = sut.ShouldExecuteJob(schedule);

        // Assert
        result.Should().Be(expected);
    }
}

/// <summary>
/// Trading service tests
/// </summary>
public class TradingServiceTests
{
    [Theory]
    [InlineData("09:30:00", true)]   // Morning trading hours
    [InlineData("11:15:00", true)]   // Before morning trading ends
    [InlineData("12:00:00", false)]  // Lunch break
    [InlineData("14:30:00", true)]   // Afternoon trading hours
    [InlineData("15:30:00", false)]  // After afternoon trading
    [InlineData("09:00:00", true)]   // Morning trading starts (boundary)
    [InlineData("15:00:00", true)]   // Afternoon trading ends (boundary)
    public void IsInTradingHours_DifferentTimes_ShouldReturnCorrectResult(string timeStr, bool expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var testTime = DateTime.Today.Add(TimeSpan.Parse(timeStr));
        fakeTimeProvider.SetLocalNow(testTime);

        var sut = new TradingService(fakeTimeProvider);

        // Act
        var result = sut.IsInTradingHours();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday, 0)]     // No trading on Saturday
    [InlineData(DayOfWeek.Sunday, 0)]       // No trading on Sunday
    [InlineData(DayOfWeek.Monday, 1.0)]     // Normal on Monday
    [InlineData(DayOfWeek.Friday, 1.1)]     // Higher volatility Friday afternoon (hour >= 14)
    public void GetMarketMultiplier_DifferentDays_ShouldReturnCorrectMultiplier(DayOfWeek dayOfWeek, decimal expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        // Find date for corresponding weekday, set to 3 PM
        var date = GetNextWeekday(new DateTime(2024, 3, 1), dayOfWeek);
        fakeTimeProvider.SetLocalNow(date.AddHours(15)); // 3 PM

        var sut = new TradingService(fakeTimeProvider);

        // Act
        var result = sut.GetMarketMultiplier();

        // Assert
        result.Should().Be(expected);
    }

    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd == 0 && start.DayOfWeek != day ? 7 : daysToAdd);
    }
}

#endregion

#region Time Zone Tests

/// <summary>
/// Global time service tests - time zone handling
/// </summary>
public class GlobalTimeServiceTests
{
    [Theory]
    [InlineData("UTC", "2024-03-15 10:00:00")]
    [InlineData("Tokyo Standard Time", "2024-03-15 19:00:00")]
    [InlineData("Eastern Standard Time", "2024-03-15 06:00:00")]
    public void GetTimeInTimeZone_DifferentTimeZones_ShouldReturnCorrectTime(string timeZoneId, string expectedTimeStr)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var baseUtcTime = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        fakeTimeProvider.SetUtcNow(baseUtcTime);

        var sut = new GlobalTimeService(fakeTimeProvider);
        var expectedTime = DateTime.Parse(expectedTimeStr);

        // Act
        var result = sut.GetTimeInTimeZone(timeZoneId);

        // Assert
        result.DateTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(1));
    }
}

#endregion

#region Test Isolation Strategy

/// <summary>
/// Demonstrates proper test isolation strategy
/// </summary>
public class TimeServiceTestsWithIsolation : IDisposable
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly GlobalTimeService _sut;

    public TimeServiceTestsWithIsolation()
    {
        // Each test instance has its own independent FakeTimeProvider
        _fakeTimeProvider = new FakeTimeProvider();
        _sut = new GlobalTimeService(_fakeTimeProvider);
    }

    public void Dispose()
    {
        // FakeTimeProvider implements IDisposable
        _fakeTimeProvider?.Dispose();
    }

    [Fact]
    public void Test1_SetToJanuaryFirst2024()
    {
        _fakeTimeProvider.SetLocalNow(new DateTime(2024, 1, 1, 12, 0, 0));

        var result = _sut.GetCurrentTimeString();

        result.Should().Contain("2024-01-01");
    }

    [Fact]
    public void Test2_SetToDecemberThirtyFirst2024()
    {
        _fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 31, 12, 0, 0));

        var result = _sut.GetCurrentTimeString();

        result.Should().Contain("2024-12-31");
    }
}

#endregion
