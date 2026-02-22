using System;
using System.Reflection;
using Xunit;
using AwesomeAssertions;

/// <summary>
/// Reflection testing examples
/// Demonstrates how to use reflection techniques to test private methods
/// </summary>
///
// ========================================
// Class under test: Contains private methods
// ========================================

namespace MyProject.Core;

public class PaymentProcessor
{
    /// <summary>
    /// Public method: Process payment
    /// </summary>
    public PaymentResult ProcessPayment(PaymentRequest request)
    {
        if (!ValidateRequest(request))
            return PaymentResult.InvalidRequest();

        var fee = CalculateFee(request.Amount, request.Method);
        var total = request.Amount + fee;

        return PaymentResult.Success(total);
    }

    /// <summary>
    /// Private method: Validate request
    /// </summary>
    private bool ValidateRequest(PaymentRequest request)
    {
        return request is { Amount: > 0 };
    }

    /// <summary>
    /// Private method: Calculate fee
    /// This is complex business logic we want to test directly
    /// </summary>
    private decimal CalculateFee(decimal amount, PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CreditCard => amount * 0.03m,
            PaymentMethod.DebitCard => Math.Max(10, amount * 0.01m),
            PaymentMethod.BankTransfer => Math.Max(10, amount * 0.005m),
            _ => 0
        };
    }

    /// <summary>
    /// Static private method: Check if business day
    /// </summary>
    private static bool IsBusinessDay(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday &&
               date.DayOfWeek != DayOfWeek.Sunday;
    }
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public decimal TotalAmount { get; set; }
    public string ErrorMessage { get; set; }

    public static PaymentResult Success(decimal total) =>
        new() { Success = true, TotalAmount = total };

    public static PaymentResult InvalidRequest() =>
        new() { Success = false, ErrorMessage = "Invalid request" };
}


// ========================================
// Testing Examples
// ========================================

namespace MyProject.Tests;

/// <summary>
/// PaymentProcessor test class
/// </summary>
public class PaymentProcessorReflectionTests
{
    // ========================================
    // Method 1: Directly use reflection to test private instance methods
    // ========================================

    [Theory]
    [InlineData(1000, PaymentMethod.CreditCard, 30)]    // 1000 * 0.03 = 30
    [InlineData(1000, PaymentMethod.DebitCard, 10)]     // 1000 * 0.01 = 10 (minimum 10)
    [InlineData(1000, PaymentMethod.BankTransfer, 10)]  // 1000 * 0.005 = 5 (minimum 10)
    [InlineData(100, PaymentMethod.BankTransfer, 10)]   // 100 * 0.005 = 0.5 (minimum 10)
    [InlineData(5000, PaymentMethod.BankTransfer, 25)]  // 5000 * 0.005 = 25
    public void CalculateFee_UsingReflectionDirectly_ShouldCalculateCorrectFee(
        decimal amount, PaymentMethod method, decimal expected)
    {
        // Arrange
        var processor = new PaymentProcessor();
        var methodInfo = typeof(PaymentProcessor).GetMethod(
            "CalculateFee",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        // Act
        var actual = (decimal)methodInfo.Invoke(processor, new object[] { amount, method });

        // Assert
        actual.Should().Be(expected);
    }

    // ========================================
    // Method 2: Test static private methods
    // ========================================

    [Theory]
    [InlineData("2024-03-15", true)]  // Friday
    [InlineData("2024-03-16", false)] // Saturday
    [InlineData("2024-03-17", false)] // Sunday
    [InlineData("2024-03-18", true)]  // Monday
    public void IsBusinessDay_UsingReflectionDirectly_ShouldCorrectlyIdentifyBusinessDays(string dateString, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateString);
        var methodInfo = typeof(PaymentProcessor).GetMethod(
            "IsBusinessDay",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Act
        var actual = (bool)methodInfo.Invoke(null, new object[] { date });

        // Assert
        actual.Should().Be(expected);
    }
}


// ========================================
// Reflection Testing Helper Class
// ========================================

namespace MyProject.Tests.Helpers;

/// <summary>
/// Reflection testing helper class
/// Encapsulates common reflection operations to simplify test code
/// </summary>
public static class ReflectionTestHelper
{
    /// <summary>
    /// Call private instance method
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="methodName">Method name</param>
    /// <param name="parameters">Method parameters</param>
    /// <returns>Method return value</returns>
    public static object InvokePrivateMethod(
        object instance,
        string methodName,
        params object[] parameters)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var methodInfo = instance.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"Private method not found: {methodName}");

        return methodInfo.Invoke(instance, parameters);
    }

    /// <summary>
    /// Call private instance method (generic version)
    /// </summary>
    public static T InvokePrivateMethod<T>(
        object instance,
        string methodName,
        params object[] parameters)
    {
        var result = InvokePrivateMethod(instance, methodName, parameters);
        return (T)result;
    }

    /// <summary>
    /// Call static private method
    /// </summary>
    /// <param name="type">Class type</param>
    /// <param name="methodName">Method name</param>
    /// <param name="parameters">Method parameters</param>
    /// <returns>Method return value</returns>
    public static object InvokePrivateStaticMethod(
        Type type,
        string methodName,
        params object[] parameters)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var methodInfo = type.GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"Static private method not found: {methodName}");

        return methodInfo.Invoke(null, parameters);
    }

    /// <summary>
    /// Call static private method (generic version)
    /// </summary>
    public static T InvokePrivateStaticMethod<T>(
        Type type,
        string methodName,
        params object[] parameters)
    {
        var result = InvokePrivateStaticMethod(type, methodName, parameters);
        return (T)result;
    }

    /// <summary>
    /// Get private field value
    /// </summary>
    public static object GetPrivateField(object instance, string fieldName)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var fieldInfo = instance.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (fieldInfo == null)
            throw new InvalidOperationException($"Private field not found: {fieldName}");

        return fieldInfo.GetValue(instance);
    }

    /// <summary>
    /// Set private field value
    /// </summary>
    public static void SetPrivateField(object instance, string fieldName, object value)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var fieldInfo = instance.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (fieldInfo == null)
            throw new InvalidOperationException($"Private field not found: {fieldName}");

        fieldInfo.SetValue(instance, value);
    }
}


// ========================================
// Testing Examples Using Helper Class
// ========================================

/// <summary>
/// Testing examples using ReflectionTestHelper
/// </summary>
public class PaymentProcessorWithHelperTests
{
    [Fact]
    public void CalculateFee_UsingHelperMethod_ShouldCalculateCorrectFee()
    {
        // Arrange
        var processor = new PaymentProcessor();

        // Act
        var actual = ReflectionTestHelper.InvokePrivateMethod<decimal>(
            processor,
            "CalculateFee",
            1000m,
            PaymentMethod.CreditCard
        );

        // Assert
        actual.Should().Be(30m);
    }

    [Fact]
    public void IsBusinessDay_UsingHelperMethod_ShouldCorrectlyIdentifyBusinessDays()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15); // Friday

        // Act
        var actual = ReflectionTestHelper.InvokePrivateStaticMethod<bool>(
            typeof(PaymentProcessor),
            "IsBusinessDay",
            date
        );

        // Assert
        actual.Should().BeTrue();
    }
}


// ========================================
// Reflection Testing Notes and Best Practices
// ========================================

/*
Notes:

1. Performance Considerations
   - Reflection is about 10-100x slower than direct calls
   - Not suitable for performance tests
   - Consider caching MethodInfo for large numbers of tests

2. Type Safety
   - Method names are strings, not checked at compile time
   - Easy to miss test updates during refactoring
   - Consider using constants to store method names

3. Maintainability
   - Tests are fragile, renaming methods will break them
   - Increases refactoring resistance
   - Regularly review if design can be improved

Best Practices:

1. Use helper methods
   - Encapsulate reflection logic
   - Provide generic versions
   - Standardize error handling

2. Clear test naming
   - Indicate use of reflection in test names
   - Example: TestMethod_UsingReflection_ExpectedResult

3. Use constants
   private const string CalculateFeeMethod = "CalculateFee";

   [Fact]
   public void Test()
   {
       ReflectionTestHelper.InvokePrivateMethod(
           instance,
           CalculateFeeMethod,
           parameters
       );
   }

4. Regular review
   - Review reflection tests every Sprint
   - Evaluate if design can be refactored to be better
   - Consider if worth continuing to maintain

When to avoid reflection testing:

❌ Simple private methods (< 10 lines)
❌ Logic easily testable through public methods
❌ Implementation details that change frequently
❌ Pure delegation calls

When to consider reflection testing:

✅ Complex algorithm validation (> 10 lines)
✅ Security-related logic
✅ Safety net before legacy system refactoring
✅ Complex logic that cannot be refactored short-term

Remember: Reflection testing should be the last resort, prefer improving design first.
*/
