# Core Assertions Syntax Complete Reference

> This document is extracted from [SKILL.md](../SKILL.md) and provides complete code examples for all types of AwesomeAssertions assertions.

---

## 1. Object Assertions

### Basic Checks

```csharp
[Fact]
public void Object_BasicAssertions_ShouldWork()
{
    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    // Null checks
    user.Should().NotBeNull();
    
    // Type checks
    user.Should().BeOfType<User>();
    user.Should().BeAssignableTo<IUser>();
    
    // Equality checks
    var anotherUser = new User { Id = 1, Name = "John", Email = "john@example.com" };
    user.Should().BeEquivalentTo(anotherUser);
}
```

### Property Validation

```csharp
[Fact]
public void Object_PropertyValidation_ShouldWork()
{
    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    // Single property validation
    user.Id.Should().Be(1);
    user.Name.Should().Be("John");
    user.Email.Should().Contain("@");
    
    // Multiple property validation
    user.Should().BeEquivalentTo(new 
    { 
        Id = 1, 
        Name = "John" 
    });
}
```

---

## 2. String Assertions

### Content Validation

```csharp
[Fact]
public void String_ContentValidation_ShouldWork()
{
    var text = "Hello World";
    
    // Basic checks
    text.Should().NotBeNullOrEmpty();
    text.Should().NotBeNullOrWhiteSpace();
    
    // Content checks
    text.Should().Contain("Hello");
    text.Should().StartWith("Hello");
    text.Should().EndWith("World");
    
    // Exact match
    text.Should().Be("Hello World");
    text.Should().BeEquivalentTo("hello world"); // Ignore case
}
```

### Pattern Matching

```csharp
[Fact]
public void String_PatternMatching_ShouldWork()
{
    var email = "user@example.com";
    
    // Regular expression matching
    email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    
    // Length validation
    email.Should().HaveLength(16);
    email.Should().HaveLengthGreaterThan(10);
    email.Should().HaveLengthLessThanOrEqualTo(50);
}
```

---

## 3. Numeric Assertions

### Range and Comparison

```csharp
[Fact]
public void Numeric_RangeChecks_ShouldWork()
{
    var value = 10;
    
    // Comparison operations
    value.Should().BeGreaterThan(5);
    value.Should().BeLessThan(15);
    value.Should().BeGreaterThanOrEqualTo(10);
    value.Should().BeLessThanOrEqualTo(10);
    
    // Range checks
    value.Should().BeInRange(5, 15);
    value.Should().BeOneOf(8, 9, 10, 11);
}
```

### Floating Point Handling

```csharp
[Fact]
public void Numeric_FloatingPointPrecision_ShouldWork()
{
    var pi = 3.14159;
    
    // Precision comparison
    pi.Should().BeApproximately(3.14, 0.01);
    
    // Special value checks
    double.NaN.Should().Be(double.NaN);
    double.PositiveInfinity.Should().BePositiveInfinity();
    
    // Sign checks
    pi.Should().BePositive();
    (-5.5).Should().BeNegative();
}
```

---

## 4. Collection Assertions

### Basic Checks

```csharp
[Fact]
public void Collection_BasicValidation_ShouldWork()
{
    var numbers = new[] { 1, 2, 3, 4, 5 };
    
    // Count checks
    numbers.Should().NotBeEmpty();
    numbers.Should().HaveCount(5);
    numbers.Should().HaveCountGreaterThan(3);
    
    // Content checks
    numbers.Should().Contain(3);
    numbers.Should().ContainSingle(x => x == 3);
    numbers.Should().NotContain(0);
    
    // Complete comparison
    numbers.Should().Equal(1, 2, 3, 4, 5);
    numbers.Should().BeEquivalentTo(new[] { 5, 4, 3, 2, 1 }); // Ignore order
}
```

### Order and Uniqueness

```csharp
[Fact]
public void Collection_OrderValidation_ShouldWork()
{
    var numbers = new[] { 1, 2, 3, 4, 5 };
    
    // Order checks
    numbers.Should().BeInAscendingOrder();
    numbers.Should().BeInDescendingOrder();
    
    // Uniqueness checks
    numbers.Should().OnlyHaveUniqueItems();
    
    // Subset checks
    numbers.Should().BeSubsetOf(new[] { 1, 2, 3, 4, 5, 6, 7 });
    numbers.Should().Contain(x => x > 3);
}
```

### Complex Object Collections

```csharp
[Fact]
public void Collection_ComplexObjects_ShouldWork()
{
    var users = new[]
    {
        new User { Id = 1, Name = "John", Age = 30 },
        new User { Id = 2, Name = "Jane", Age = 25 },
        new User { Id = 3, Name = "Bob", Age = 35 }
    };
    
    // Conditional filtering
    users.Should().Contain(u => u.Name == "John");
    users.Should().OnlyContain(u => u.Age >= 18);
    
    // All satisfy
    users.Should().AllSatisfy(u => 
    {
        u.Id.Should().BeGreaterThan(0);
        u.Name.Should().NotBeNullOrEmpty();
    });
    
    // LINQ integration
    users.Where(u => u.Age > 30).Should().HaveCount(1);
}
```

---

## 5. Exception Assertions

### Basic Exception Handling

```csharp
[Fact]
public void Exception_BasicValidation_ShouldWork()
{
    var service = new UserService();
    
    // Expect exception to be thrown
    Action act = () => service.GetUser(-1);
    
    act.Should().Throw<ArgumentException>()
       .WithMessage("*User ID*")
       .And.ParamName.Should().Be("userId");
}
```

### Should Not Throw Exception

```csharp
[Fact]
public void Exception_ShouldNotThrow_ShouldWork()
{
    var calculator = new Calculator();
    
    // Should not throw any exception
    Action act = () => calculator.Add(1, 2);
    act.Should().NotThrow();
    
    // Should not throw specific exception
    act.Should().NotThrow<DivideByZeroException>();
}
```

### Nested Exceptions

```csharp
[Fact]
public void Exception_NestedExceptions_ShouldWork()
{
    var service = new DatabaseService();
    
    Action act = () => service.Connect("invalid");
    
    act.Should().Throw<DatabaseConnectionException>()
       .WithInnerException<ArgumentException>()
       .WithMessage("*connection string*");
}
```

---

## 6. Async Assertions

### Task Completion Validation

```csharp
[Fact]
public async Task Async_TaskCompletion_ShouldWork()
{
    var service = new UserService();
    
    // Wait for task completion
    var task = service.GetUserAsync(1);
    await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    
    // Validate result
    task.Result.Should().NotBeNull();
    task.Result.Id.Should().Be(1);
}
```

### Async Exceptions

```csharp
[Fact]
public async Task Async_ExceptionHandling_ShouldWork()
{
    var service = new ApiService();
    
    Func<Task> act = async () => await service.CallInvalidEndpointAsync();
    
    await act.Should().ThrowAsync<HttpRequestException>()
             .WithMessage("*404*");
}
```
