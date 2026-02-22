# xUnit to TUnit Syntax Comparison Migration Guide

This document provides syntax comparison from xUnit to TUnit to help development teams quickly perform migration assessment and conversion.

---

## Core Differences Overview

| Feature           | xUnit                       | TUnit                          |
| ----------------- | --------------------------- | ------------------------------ |
| **Basic Test**    | `[Fact]`                    | `[Test]`                       |
| **Parameterized** | `[Theory]` + `[InlineData]` | `[Test]` + `[Arguments]`       |
| **Test Method**   | `void` or `Task`            | Must be `async Task`           |
| **Assertion**     | `Assert.Method()`           | `await Assert.That().Method()` |

---

## Test Attribute Comparison

### Basic Test

```csharp
// xUnit
[Fact]
public void TestMethod()
{
    Assert.True(true);
}

// TUnit
[Test]
public async Task TestMethod()
{
    await Assert.That(true).IsTrue();
}
```

### Parameterized Test

```csharp
// xUnit
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
public void Add_MultipleInputs(int a, int b, int expected)
{
    Assert.Equal(expected, _calculator.Add(a, b));
}

// TUnit
[Test]
[Arguments(1, 2, 3)]
[Arguments(-1, 1, 0)]
public async Task Add_MultipleInputs(int a, int b, int expected)
{
    await Assert.That(_calculator.Add(a, b)).IsEqualTo(expected);
}
```

---

## Assertion Syntax Comparison

### Equality Assertions

```csharp
// xUnit
Assert.Equal(expected, actual);
Assert.NotEqual(unexpected, actual);

// TUnit
await Assert.That(actual).IsEqualTo(expected);
await Assert.That(actual).IsNotEqualTo(unexpected);
```

### Boolean Assertions

```csharp
// xUnit
Assert.True(condition);
Assert.False(condition);

// TUnit
await Assert.That(condition).IsTrue();
await Assert.That(condition).IsFalse();
```

### Null Check

```csharp
// xUnit
Assert.Null(value);
Assert.NotNull(value);

// TUnit
await Assert.That(value).IsNull();
await Assert.That(value).IsNotNull();
```

### String Assertions

```csharp
// xUnit
Assert.Contains("substring", fullString);
Assert.StartsWith("prefix", fullString);
Assert.EndsWith("suffix", fullString);
Assert.Empty(emptyString);

// TUnit
await Assert.That(fullString).Contains("substring");
await Assert.That(fullString).StartsWith("prefix");
await Assert.That(fullString).EndsWith("suffix");
await Assert.That(emptyString).IsEmpty();
```

### Collection Assertions

```csharp
// xUnit
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);

// TUnit
await Assert.That(collection).Contains(item);
await Assert.That(collection).DoesNotContain(item);
await Assert.That(collection).IsEmpty();
await Assert.That(collection).IsNotEmpty();
await Assert.That(collection).HasCount(1);
```

### Numeric Comparison

```csharp
// xUnit
Assert.InRange(actual, low, high);
Assert.True(actual > compareValue);  // No direct API

// TUnit
await Assert.That(actual).IsBetween(low, high);
await Assert.That(actual).IsGreaterThan(compareValue);
await Assert.That(actual).IsLessThan(compareValue);
```

### Exception Assertions

```csharp
// xUnit
Assert.Throws<ArgumentException>(() => SomeMethod());
var ex = Assert.Throws<ArgumentException>(() => SomeMethod());
Assert.Equal("message", ex.Message);

// TUnit
await Assert.That(() => SomeMethod()).Throws<ArgumentException>();
await Assert.That(() => SomeMethod())
    .Throws<ArgumentException>()
    .WithMessage("message");
```

### Async Exception Assertions

```csharp
// xUnit
await Assert.ThrowsAsync<ArgumentException>(async () => await SomeAsyncMethod());

// TUnit
await Assert.That(async () => await SomeAsyncMethod())
    .Throws<ArgumentException>();
```

---

## Lifecycle Comparison

### Class Level Setup/Cleanup

```csharp
// xUnit
public class MyTests : IClassFixture<MyFixture>
{
    private readonly MyFixture _fixture;
    public MyTests(MyFixture fixture) => _fixture = fixture;
}

// TUnit
public class MyTests
{
    private static MyResource? _resource;

    [Before(Class)]
    public static async Task ClassSetup()
    {
        _resource = new MyResource();
        await _resource.InitializeAsync();
    }

    [After(Class)]
    public static async Task ClassTearDown()
    {
        await _resource?.DisposeAsync();
    }
}
```

### Test Level Setup/Cleanup

```csharp
// xUnit (using constructor and Dispose)
public class MyTests : IDisposable
{
    public MyTests() { /* Setup */ }
    public void Dispose() { /* Cleanup */ }
}

// TUnit (can use constructor/Dispose, or Before/After)
public class MyTests
{
    [Before(Test)]
    public async Task Setup() { /* Setup */ }

    [After(Test)]
    public async Task TearDown() { /* Cleanup */ }
}
```

---

## Parallel Control Comparison

### Disable Parallel

```csharp
// xUnit (using Collection)
[Collection("NonParallel")]
public class MyTests { }

// TUnit (using NotInParallel)
public class MyTests
{
    [Test]
    [NotInParallel("DatabaseTests")]
    public async Task DatabaseTest() { }
}
```

---

## Complete Migration Example

### xUnit Original Code

```csharp
using Xunit;

public class EmailValidatorTests
{
    private readonly EmailValidator _validator;

    public EmailValidatorTests()
    {
        _validator = new EmailValidator();
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_VariousInputs_ShouldReturnCorrectResult(string? email, bool expected)
    {
        var result = _validator.IsValidEmail(email);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetDomain_InvalidEmail_ShouldThrowArgumentException()
    {
        var invalidEmail = "invalid-email";
        Assert.Throws<ArgumentException>(() => _validator.GetDomain(invalidEmail));
    }

    [Fact]
    public void GetDomain_ValidEmail_ShouldReturnCorrectDomain()
    {
        var email = "user@example.com";
        var domain = _validator.GetDomain(email);
        Assert.Equal("example.com", domain);
        Assert.Contains(".", domain);
    }
}
```

### TUnit Converted

```csharp
using TUnit.Core;
using TUnit.Assertions;

public class EmailValidatorTests
{
    private readonly EmailValidator _validator;

    public EmailValidatorTests()
    {
        _validator = new EmailValidator();
    }

    [Test]
    [Arguments("test@example.com", true)]
    [Arguments("invalid-email", false)]
    [Arguments("", false)]
    [Arguments(null, false)]
    public async Task IsValidEmail_VariousInputs_ShouldReturnCorrectResult(string? email, bool expected)
    {
        var result = _validator.IsValidEmail(email);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task GetDomain_InvalidEmail_ShouldThrowArgumentException()
    {
        var invalidEmail = "invalid-email";
        await Assert.That(() => _validator.GetDomain(invalidEmail))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task GetDomain_ValidEmail_ShouldReturnCorrectDomain()
    {
        var email = "user@example.com";
        var domain = _validator.GetDomain(email);
        await Assert.That(domain)
            .IsEqualTo("example.com")
            .And.Contains(".");
    }
}
```

---

## Migration Checklist

- [ ] Remove `Microsoft.NET.Test.Sdk` package reference
- [ ] Install `TUnit` package
- [ ] Update GlobalUsings to include TUnit namespaces
- [ ] Change `[Fact]` to `[Test]`
- [ ] Change `[Theory]` to `[Test]`
- [ ] Change `[InlineData]` to `[Arguments]`
- [ ] Change test methods to `async Task`
- [ ] Add `await` before all assertions
- [ ] Convert assertion syntax to fluent style
- [ ] Confirm IDE version supports Microsoft.Testing.Platform
