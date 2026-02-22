// =============================================================================
// TimeProvider Basic Usage Examples
// Demonstrates how to refactor time-dependent code into testable design
// =============================================================================

using System;

namespace TimeProviderExamples;

#region Problem: Traditional DateTime Cannot Be Tested

/// <summary>
/// ❌ Problem code: Uses DateTime.Now directly, cannot control time in tests
/// </summary>
public class LegacyOrderService
{
    public bool CanPlaceOrder()
    {
        // Uses static time - test results depend on execution time
        var now = DateTime.Now;
        var currentHour = now.Hour;

        // Business hours: 9 AM to 5 PM
        return currentHour >= 9 && currentHour < 17;
    }

    public string GetTimeBasedDiscount()
    {
        var today = DateTime.Today;

        if (today.DayOfWeek == DayOfWeek.Friday)
        {
            return "Friday Special: 10% Discount";
        }

        if (today.Month == 12 && today.Day == 25)
        {
            return "Christmas Special: 20% Discount";
        }

        return "No Discount";
    }
}

#endregion

#region Solution: Use TimeProvider Abstraction

/// <summary>
/// ✅ Testable code: Receive TimeProvider through dependency injection
/// </summary>
public class OrderService
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Constructor injection for TimeProvider
    /// Pass TimeProvider.System for production
    /// Pass FakeTimeProvider for testing
    /// </summary>
    public OrderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>
    /// Determines if order can be placed (within business hours)
    /// </summary>
    public bool CanPlaceOrder()
    {
        // Use injected TimeProvider to get time
        var now = _timeProvider.GetLocalNow();
        var currentHour = now.Hour;

        // Business hours: 9 AM to 5 PM
        return currentHour >= 9 && currentHour < 17;
    }

    /// <summary>
    /// Gets discount information based on date
    /// </summary>
    public string GetTimeBasedDiscount()
    {
        var today = _timeProvider.GetLocalNow().Date;

        if (today.DayOfWeek == DayOfWeek.Friday)
        {
            return "Friday Special: 10% Discount";
        }

        if (today.Month == 12 && today.Day == 25)
        {
            return "Christmas Special: 20% Discount";
        }

        return "No Discount";
    }
}

#endregion

#region TimeProvider Core API Reference

/// <summary>
/// TimeProvider Core API Reference
/// </summary>
public static class TimeProviderApiReference
{
    public static void ShowTimeProviderUsage()
    {
        // 1. System time provider (for production use)
        TimeProvider systemProvider = TimeProvider.System;

        // 2. Get UTC time
        DateTimeOffset utcNow = systemProvider.GetUtcNow();
        Console.WriteLine($"UTC Time: {utcNow}");

        // 3. Get local time
        DateTimeOffset localNow = systemProvider.GetLocalNow();
        Console.WriteLine($"Local Time: {localNow}");

        // 4. Get local time zone information
        TimeZoneInfo localTimeZone = systemProvider.LocalTimeZone;
        Console.WriteLine($"Local Time Zone: {localTimeZone.DisplayName}");

        // 5. High-precision timestamp (for performance measurement)
        long timestamp = systemProvider.GetTimestamp();
        Console.WriteLine($"Timestamp: {timestamp}");

        // 6. Calculate elapsed time
        long startTimestamp = systemProvider.GetTimestamp();
        // ... perform some operations ...
        long endTimestamp = systemProvider.GetTimestamp();
        TimeSpan elapsed = systemProvider.GetElapsedTime(startTimestamp, endTimestamp);
        Console.WriteLine($"Elapsed Time: {elapsed}");
    }
}

#endregion

#region Dependency Injection Setup

/// <summary>
/// Dependency Injection setup examples
/// </summary>
public static class DependencyInjectionSetup
{
    /*
    // Program.cs or Startup.cs

    // Production - use system time
    services.AddSingleton(TimeProvider.System);
    services.AddScoped<OrderService>();

    // Development (if specific time testing needed)
    if (builder.Environment.IsDevelopment())
    {
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 25, 10, 0, 0)); // Test Christmas
        services.AddSingleton<TimeProvider>(fakeTimeProvider);
    }
    */
}

#endregion

#region Real Business Logic Examples

/// <summary>
/// Schedule Service - demonstrates time-dependent scheduling logic
/// </summary>
public class ScheduleService
{
    private readonly TimeProvider _timeProvider;

    public ScheduleService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Determines if job should be executed
    /// </summary>
    public bool ShouldExecuteJob(JobSchedule schedule)
    {
        var now = _timeProvider.GetLocalNow();
        return schedule.NextExecutionTime <= now;
    }

    /// <summary>
    /// Calculates next execution time
    /// </summary>
    public DateTime CalculateNextExecution(JobSchedule schedule)
    {
        var now = _timeProvider.GetLocalNow();

        return schedule.CronExpression switch
        {
            "0 0 * * *" => now.Date.AddDays(1),           // Daily at midnight
            "0 0 * * 1" => GetNextMonday(now),             // Every Monday at midnight
            _ => now.DateTime.AddHours(1)                  // Default hourly
        };
    }

    private DateTime GetNextMonday(DateTimeOffset now)
    {
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        return now.Date.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
    }
}

public class JobSchedule
{
    public DateTime NextExecutionTime { get; set; }
    public string CronExpression { get; set; } = string.Empty;
}

/// <summary>
/// Trading Service - demonstrates time window logic
/// </summary>
public class TradingService
{
    private readonly TimeProvider _timeProvider;

    public TradingService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Determines if within trading hours
    /// Trading hours: 9:00-11:30, 13:00-15:00
    /// </summary>
    public bool IsInTradingHours()
    {
        var now = _timeProvider.GetLocalNow();
        var currentTime = now.TimeOfDay;

        return (currentTime >= TimeSpan.FromHours(9) && currentTime <= TimeSpan.FromHours(11.5)) ||
               (currentTime >= TimeSpan.FromHours(13) && currentTime <= TimeSpan.FromHours(15));
    }

    /// <summary>
    /// Gets market multiplier
    /// </summary>
    public decimal GetMarketMultiplier()
    {
        var now = _timeProvider.GetLocalNow();

        return now.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => 0m,      // No trading on weekends
            DayOfWeek.Friday when now.Hour >= 14 => 1.1m,       // Higher volatility Friday afternoon
            _ => 1.0m
        };
    }
}

/// <summary>
/// Global Time Service - demonstrates time zone handling
/// </summary>
public class GlobalTimeService
{
    private readonly TimeProvider _timeProvider;

    public GlobalTimeService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Gets current time in specified time zone
    /// </summary>
    public DateTimeOffset GetTimeInTimeZone(string timeZoneId)
    {
        var utcNow = _timeProvider.GetUtcNow();
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        return TimeZoneInfo.ConvertTime(utcNow, targetTimeZone);
    }

    /// <summary>
    /// Gets current time string
    /// </summary>
    public string GetCurrentTimeString()
    {
        return _timeProvider.GetLocalNow().ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Gets current time
    /// </summary>
    public DateTime GetCurrentTime()
    {
        return _timeProvider.GetLocalNow().DateTime;
    }
}

/// <summary>
/// Audit Logger Service - demonstrates UTC and local time conversion
/// </summary>
public class AuditLogger
{
    private readonly TimeProvider _timeProvider;

    public AuditLogger(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Logs activity (stores UTC time, displays local time)
    /// </summary>
    public AuditLog LogActivity(string activity)
    {
        var utcTimestamp = _timeProvider.GetUtcNow();
        var localTime = _timeProvider.GetLocalNow();

        return new AuditLog
        {
            Activity = activity,
            UtcTimestamp = utcTimestamp,
            LocalTimeDisplay = localTime.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}

public class AuditLog
{
    public string Activity { get; set; } = string.Empty;
    public DateTimeOffset UtcTimestamp { get; set; }
    public string LocalTimeDisplay { get; set; } = string.Empty;
}

#endregion
