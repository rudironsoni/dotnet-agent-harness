using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// ILogger testing strategy examples
/// Demonstrates how to verify ILogger recording behavior in tests
/// </summary>
public class ILoggerTestingExample
{
    // ===== Method 1: Use AbstractLogger to simplify testing =====

    [Fact]
    public void WithAbstractLogger_PaymentFailed_ShouldLogError()
    {
        // Arrange
        var logger = Substitute.For<AbstractLogger<PaymentService>>();
        var paymentGateway = Substitute.For<IPaymentGateway>();
        paymentGateway.ProcessPayment(Arg.Any<decimal>()).Returns(new PaymentResult
        {
            Success = false,
            ErrorMessage = "Insufficient balance"
        });

        var service = new PaymentService(logger, paymentGateway);

        // Act
        service.ProcessPayment(1000);

        // Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<Exception>(),
            Arg.Is<string>(msg => msg.Contains("Payment failed") && msg.Contains("Insufficient balance"))
        );
    }

    // ===== Method 2: Directly intercept Log<TState> method (complex but complete) =====

    [Fact]
    public void WithStandardILogger_PaymentSuccess_ShouldLogInfo()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PaymentService>>();
        var paymentGateway = Substitute.For<IPaymentGateway>();
        paymentGateway.ProcessPayment(Arg.Any<decimal>()).Returns(new PaymentResult
        {
            Success = true
        });

        var service = new PaymentService(logger, paymentGateway);

        // Act
        service.ProcessPayment(1000);

        // Assert - intercept underlying Log<TState> method
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains("Payment successful")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }

    // ===== Method 3: Wrong example - directly mocking extension methods will fail =====

    [Fact]
    public void WrongApproach_ThisWillFail()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PaymentService>>();
        var service = new PaymentService(logger, null);

        // Act
        // service.ProcessPayment(1000);

        // Assert - ❌ this will fail because LogError is an extension method
        // logger.Received().LogError(Arg.Any<string>()); // ❌ won't work
    }
}

// ===== AbstractLogger abstraction implementation =====

/// <summary>
/// Simplified ILogger abstraction for easier testing
/// </summary>
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

    // Simplified abstract method, easier to test
    public abstract void Log(LogLevel logLevel, Exception ex, string information);
}

// ===== Service class under test =====

public class PaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentService(ILogger<PaymentService> logger, IPaymentGateway paymentGateway)
    {
        _logger = logger;
        _paymentGateway = paymentGateway;
    }

    public void ProcessPayment(decimal amount)
    {
        _logger.LogInformation($"Starting payment processing, amount: ${amount}");

        var result = _paymentGateway.ProcessPayment(amount);

        if (result.Success)
        {
            _logger.LogInformation($"Payment successful, amount: ${amount}");
        }
        else
        {
            _logger.LogError($"Payment failed: {result.ErrorMessage}");
        }
    }
}

// ===== Dependency interface definitions =====

public interface IPaymentGateway
{
    PaymentResult ProcessPayment(decimal amount);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
