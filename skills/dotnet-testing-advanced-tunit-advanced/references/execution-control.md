# Execution Control and Test Quality

> This document is extracted from [SKILL.md](../SKILL.md), providing complete examples and details for Retry, Timeout, and DisplayName.

## Retry Mechanism: Smart Retry Strategy

```csharp
[Test]
[Retry(3)] // Retry up to 3 times if failed
[Property("Category", "Flaky")]
public async Task NetworkCall_PotentiallyUnstable_UseRetryMechanism()
{
    var random = new Random();
    var success = random.Next(1, 4) == 1; // Approximately 33% success rate

    if (!success)
    {
        throw new HttpRequestException("Simulated network error");
    }

    await Assert.That(success).IsTrue();
}
```

**When to Use Retry:**

1. External service calls: API requests, database connections may temporarily fail due to network issues
2. File system operations: In CI/CD environments, file locking may cause temporary failures
3. Concurrent test contention: Race conditions when multiple tests access shared resources

**When NOT to Use Retry:**

1. Logic errors: Code bugs will not succeed no matter how many times you retry
2. Expected exceptions: Tests that are meant to verify exception scenarios
3. Performance tests: Retries affect the accuracy of performance measurements

## Timeout Control: Long-Running Test Management

```csharp
[Test]
[Timeout(5000)] // 5 second timeout
[Property("Category", "Performance")]
public async Task LongRunningOperation_ShouldCompleteWithinTimeLimit()
{
    await Task.Delay(1000); // 1 second operation, should be within 5 second limit
    await Assert.That(true).IsTrue();
}

[Test]
[Timeout(1000)] // Ensure it does not exceed 1 second
[Property("Category", "Performance")]
[Property("Baseline", "true")]
public async Task SearchFunction_PerformanceBaseline_ShouldMeetSLARequirements()
{
    var stopwatch = Stopwatch.StartNew();
    
    var searchResults = await PerformSearch("test query");
    
    stopwatch.Stop();
    
    await Assert.That(searchResults).IsNotNull();
    await Assert.That(searchResults.Count()).IsGreaterThan(0);
    await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(500);
}
```

## DisplayName: Custom Test Names

```csharp
[Test]
[DisplayName("Custom Test Name: Verify User Registration Flow")]
public async Task UserRegistration_CustomDisplayName_TestNameMoreReadable()
{
    await Assert.That("user@example.com").Contains("@");
}

// Dynamic display names for parameterized tests
[Test]
[Arguments("valid@email.com", true)]
[Arguments("invalid-email", false)]
[Arguments("", false)]
[DisplayName("Email Validation: {0} should be {1}")]
public async Task EmailValidation_ParameterizedDisplayName(string email, bool expectedValid)
{
    var isValid = !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
    await Assert.That(isValid).IsEqualTo(expectedValid);
}

// Business scenario-driven display names
[Test]
[Arguments(CustomerLevel.RegularMember, 1000, 0)]
[Arguments(CustomerLevel.VIPMember, 1000, 50)]
[Arguments(CustomerLevel.PlatinumMember, 1000, 100)]
[DisplayName("Member level {0} purchases ${1} should receive ${2} discount")]
public async Task MemberDiscount_CalculateDiscountBasedOnMemberLevel(
    CustomerLevel level, decimal amount, decimal expectedDiscount)
{
    var calculator = new DiscountCalculator();
    var discount = await calculator.CalculateDiscountAsync(amount, level);
    await Assert.That(discount).IsEqualTo(expectedDiscount);
}
```
