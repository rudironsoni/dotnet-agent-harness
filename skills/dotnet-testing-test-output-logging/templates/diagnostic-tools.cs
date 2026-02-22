using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// XUnitLogger and CompositeLogger diagnostic tools examples
/// Demonstrates how to perform both behavior verification and test output diagnostics
/// </summary>
public class DiagnosticToolsExample
{
    private readonly ITestOutputHelper _output;

    public DiagnosticToolsExample(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WithCompositeLogger_SimultaneousVerificationAndOutput()
    {
        // Arrange - combine Mock Logger with XUnit Logger
        var mockLogger = Substitute.For<AbstractLogger<OrderService>>();
        var xunitLogger = new XUnitLogger<OrderService>(_output);
        var compositeLogger = new CompositeLogger<OrderService>(mockLogger, xunitLogger);

        var service = new OrderService(compositeLogger);

        // Act
        service.ProcessOrder("ORD001", 1500);

        // Assert - can verify Mock Logger behavior
        mockLogger.Received().Log(
            LogLevel.Information,
            Arg.Any<Exception>(),
            Arg.Is<string>(msg => msg.Contains("Starting order processing"))
        );

        // Meanwhile, test output shows actual log messages for diagnostics
    }
}

// ===== XUnitLogger implementation =====

/// <summary>
/// Routes ILogger output to xUnit test output
/// </summary>
public class XUnitLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _categoryName;

    public XUnitLogger(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _categoryName = typeof(T).Name;
    }

    public IDisposable BeginScope<TState>(TState state) => new NoOpDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
        {
            return;
        }

        var message = formatter(state, exception);

        // Formatted output: [Time] [Level] [Category] Message
        _testOutputHelper.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}"
        );

        if (exception != null)
        {
            _testOutputHelper.WriteLine($"Exception: {exception}");
        }
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

// ===== CompositeLogger implementation =====

/// <summary>
/// Combines multiple Loggers, sends logs to all internal loggers
/// </summary>
public class CompositeLogger<T> : ILogger<T>
{
    private readonly ILogger<T>[] _loggers;

    public CompositeLogger(params ILogger<T>[] loggers)
    {
        _loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        var disposables = _loggers.Select(logger => logger.BeginScope(state)).ToArray();
        return new CompositeDisposable(disposables);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _loggers.Any(logger => logger.IsEnabled(logLevel));
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        foreach (var logger in _loggers)
        {
            if (logger.IsEnabled(logLevel))
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}

/// <summary>
/// Combines multiple IDisposable objects
/// </summary>
public class CompositeDisposable : IDisposable
{
    private readonly IDisposable[] _disposables;

    public CompositeDisposable(params IDisposable[] disposables)
    {
        _disposables = disposables;
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }
    }
}

// ===== TestLogger implementation (for collecting logs) =====

/// <summary>
/// Test Logger that collects all logs for verification
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    private readonly ConcurrentBag<LogEntry> _logs = new ConcurrentBag<LogEntry>();

    public IReadOnlyCollection<LogEntry> Logs => _logs.ToList();

    public IDisposable BeginScope<TState>(TState state) => new NoOpDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var message = formatter?.Invoke(state, exception) ?? state?.ToString();

        _logs.Add(new LogEntry
        {
            LogLevel = logLevel,
            Message = message,
            Exception = exception,
            Timestamp = DateTime.Now
        });
    }

    public bool HasLog(LogLevel level, string messageContains)
    {
        return _logs.Any(log =>
            log.LogLevel == level &&
            log.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

/// <summary>
/// Log entry
/// </summary>
public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
    public DateTime Timestamp { get; set; }
}

// ===== Service class under test =====

public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(string orderId, decimal amount)
    {
        _logger.LogInformation($"Starting order processing {orderId}, amount: ${amount}");

        // Simulate processing logic
        if (amount > 0)
        {
            _logger.LogInformation($"Order {orderId} processing complete");
        }
        else
        {
            _logger.LogError($"Order {orderId} amount invalid");
        }
    }
}

// ===== AbstractLogger (copied from previous example) =====

public abstract class AbstractLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        Log(logLevel, exception, state?.ToString() ?? string.Empty);
    }

    public abstract void Log(LogLevel logLevel, Exception ex, string information);
}
