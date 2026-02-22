# Private Method Testing Techniques

> This document is extracted from SKILL.md and provides complete technical details and code examples for private method testing.

## Decision Tree: Should You Test Private Methods

```text
Start
  |
Can it be refactored into a separate class?
  |-- Yes -> Refactor and test new class
  |-- No
      |
      Is the private method more than 10 lines?
        |-- No -> Test through public methods
        |-- Yes
            |
            Does it contain complex algorithm/security logic?
              |-- No -> Reconsider design
              |-- Yes -> Consider using reflection testing
```

## When to Consider Testing Private Methods

**Necessary Conditions (must be met simultaneously):**

1. **High Complexity**: Complex logic over 10 lines
2. **Business Critical**: Contains important business rules or algorithms
3. **Difficult to Test Indirectly**: Cannot be fully verified through public methods
4. **High Refactoring Cost**: Cannot be refactored into a separate class in the short term

**Typical Scenarios:**

- Complex mathematical operations or algorithms
- Encryption, decryption and other security-related logic
- Performance-critical internal implementations
- Safety net before legacy system refactoring

## Testing Private Methods Using Reflection

When you determine that private method testing is needed, use reflection techniques:

### Testing Private Instance Methods

```csharp
[Theory]
[InlineData(1000, PaymentMethod.CreditCard, 30)]
[InlineData(1000, PaymentMethod.DebitCard, 10)]
public void TestPrivateInstanceMethod_UsingReflection(
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
```

### Testing Static Private Methods

```csharp
[Theory]
[InlineData("2024-03-15", true)]  // Friday
[InlineData("2024-03-16", false)] // Saturday
public void TestPrivateStaticMethod_UsingReflection(string dateString, bool expected)
{
    // Arrange
    var date = DateTime.Parse(dateString);
    var methodInfo = typeof(DateHelper).GetMethod(
        "IsBusinessDay",
        BindingFlags.NonPublic | BindingFlags.Static
    );

    // Act
    var actual = (bool)methodInfo.Invoke(null, new object[] { date });

    // Assert
    actual.Should().Be(expected);
}
```

### Reflection Testing Helper Class

Create helper methods to simplify reflection operations:

```csharp
public static class ReflectionTestHelper
{
    /// <summary>
    /// Call private instance method
    /// </summary>
    public static object InvokePrivateMethod(
        object instance, 
        string methodName, 
        params object[] parameters)
    {
        var methodInfo = instance.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"Private method not found: {methodName}");

        return methodInfo.Invoke(instance, parameters);
    }

    /// <summary>
    /// Call static private method
    /// </summary>
    public static object InvokePrivateStaticMethod(
        Type type,
        string methodName,
        params object[] parameters)
    {
        var methodInfo = type.GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Static
        );

        if (methodInfo == null)
            throw new InvalidOperationException($"Static private method not found: {methodName}");

        return methodInfo.Invoke(null, parameters);
    }
}

// Usage example
[Fact]
public void TestWithHelper_MoreConciseReflectionTesting()
{
    // Arrange
    var processor = new PaymentProcessor();

    // Act
    var actual = (decimal)ReflectionTestHelper.InvokePrivateMethod(
        processor, 
        "CalculateFee", 
        1000m, 
        PaymentMethod.CreditCard
    );

    // Assert
    actual.Should().Be(30m);
}
```

## Reflection Testing Considerations

**Risks:**

- Warning: Fragile tests: Method name changes will cause tests to fail
- Warning: Refactoring resistance: Increases difficulty of refactoring
- Warning: Maintenance cost: Requires additional maintenance of reflection code
- Warning: Performance impact: Reflection is slower than direct calls

**Best Practices:**

- Use helper methods to encapsulate reflection logic
- Clearly indicate the use of reflection in test names
- Regularly review whether it can be refactored into a better design
- Consider using constants to store method names
