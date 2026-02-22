# TUnit.Assertions Assertion System

TUnit adopts a Fluent assertion design, all assertions are asynchronous:

## Basic Equality Assertions

```csharp
[Test]
public async Task BasicEqualityAssertionExample()
{
    var expected = 42;
    var actual = 40 + 2;
    
    await Assert.That(actual).IsEqualTo(expected);
    await Assert.That(actual).IsNotEqualTo(43);
    
    // Null checks
    string? nullValue = null;
    await Assert.That(nullValue).IsNull();
    await Assert.That("test").IsNotNull();
}
```

## Boolean Assertions

```csharp
[Test]
public async Task BooleanAssertionExample()
{
    var condition = 1 + 1 == 2;
    
    await Assert.That(condition).IsTrue();
    await Assert.That(1 + 1 == 3).IsFalse();
    
    var number = 10;
    await Assert.That(number > 5).IsTrue();
}
```

## Numeric Comparison Assertions

```csharp
[Test]
public async Task NumericComparisonAssertionExample()
{
    var actual = 10;
    
    await Assert.That(actual).IsGreaterThan(5);
    await Assert.That(actual).IsGreaterThanOrEqualTo(10);
    await Assert.That(actual).IsLessThan(15);
    await Assert.That(actual).IsBetween(5, 15);
}

[Test]
[Arguments(3.14159, 3.14, 0.01)]
public async Task FloatingPointPrecisionControl(double actual, double expected, double tolerance)
{
    await Assert.That(actual)
        .IsEqualTo(expected)
        .Within(tolerance);
}
```

## String Assertions

```csharp
[Test]
public async Task StringAssertionExample()
{
    var email = "user@example.com";
    
    await Assert.That(email).Contains("@");
    await Assert.That(email).StartsWith("user");
    await Assert.That(email).EndsWith(".com");
    await Assert.That(email).DoesNotContain(" ");
    await Assert.That("").IsEmpty();
    await Assert.That(email).IsNotEmpty();
}
```

## Collection Assertions

```csharp
[Test]
public async Task CollectionAssertionExample()
{
    var numbers = new List<int> { 1, 2, 3, 4, 5 };
    
    await Assert.That(numbers).HasCount(5);
    await Assert.That(numbers).IsNotEmpty();
    await Assert.That(numbers).Contains(3);
    await Assert.That(numbers).DoesNotContain(10);
    await Assert.That(numbers.First()).IsEqualTo(1);
    await Assert.That(numbers.Last()).IsEqualTo(5);
}
```

## Exception Assertions

```csharp
[Test]
public async Task ExceptionAssertionExample()
{
    var calculator = new Calculator();
    
    // Check for specific exception type
    await Assert.That(() => calculator.Divide(10, 0))
        .Throws<DivideByZeroException>();
    
    // Check exception message
    await Assert.That(() => calculator.Divide(10, 0))
        .Throws<DivideByZeroException>()
        .WithMessage("Divisor cannot be zero");
    
    // Check no exception is thrown
    await Assert.That(() => calculator.Add(1, 2))
        .DoesNotThrow();
}
```

## And / Or Condition Combination

```csharp
[Test]
public async Task ConditionCombinationExample()
{
    var number = 10;
    
    // And: All conditions must be true
    await Assert.That(number)
        .IsGreaterThan(5)
        .And.IsLessThan(15)
        .And.IsEqualTo(10);
    
    // Or: Any condition can be true
    await Assert.That(number)
        .IsEqualTo(5)
        .Or.IsEqualTo(10)
        .Or.IsEqualTo(15);
}
```
