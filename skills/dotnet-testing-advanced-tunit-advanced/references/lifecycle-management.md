# Test Lifecycle Management

> This document is extracted from [SKILL.md](../SKILL.md), providing complete examples and details for TUnit test lifecycles.

## Lifecycle Method Overview

| Lifecycle Method    | Execution Timing                              | Use Case                              |
| :------------------ | :-------------------------------------------- | :------------------------------------ |
| `[Before(Class)]`   | Before first test in class                    | Expensive resource initialization (e.g., database connections) |
| `Constructor`       | Before each test                              | Basic test instance setup             |
| `[Before(Test)]`    | Before each test method execution             | Test-specific setup operations        |
| `Test Method`       | Actual test execution                         | Test logic itself                     |
| `[After(Test)]`     | After each test method execution              | Test-specific cleanup operations      |
| `Dispose`           | When test instance is destroyed               | Release test instance resources       |
| `[After(Class)]`    | After last test in class completes            | Cleanup shared resources              |

## Before/After Attribute Family

```csharp
// Before attributes
[Before(Test)]           // Instance method - executed before each test
[Before(Class)]          // Static method - executed once before first test in class
[Before(Assembly)]       // Static method - executed once before first test in assembly
[Before(TestSession)]    // Static method - executed once before test session starts

// After attributes
[After(Test)]           // Instance method - executed after each test
[After(Class)]          // Static method - executed once after last test in class
[After(Assembly)]       // Static method - executed once after last test in assembly
[After(TestSession)]    // Static method - executed once after test session ends

// Global hooks
[BeforeEvery(Test)]     // Static method - executed before every test (global)
[AfterEvery(Test)]      // Static method - executed after every test (global)
```

## Practical Example

```csharp
public class LifecycleTests
{
    private readonly StringBuilder _logBuilder;
    private static readonly List<string> ClassLog = [];

    public LifecycleTests()
    {
        Console.WriteLine("1. Constructor executed - Test instance created");
        _logBuilder = new StringBuilder();
    }

    [Before(Class)]
    public static async Task BeforeClass()
    {
        Console.WriteLine("2. BeforeClass executed - Class-level initialization");
        ClassLog.Add("BeforeClass executed");
        await Task.Delay(10);
    }

    [Before(Test)]
    public async Task BeforeTest()
    {
        Console.WriteLine("3. BeforeTest executed - Test pre-setup");
        _logBuilder.AppendLine("BeforeTest executed");
        await Task.Delay(5);
    }

    [Test]
    public async Task TestMethod_ShouldExecuteLifecycleMethodsInCorrectOrder()
    {
        Console.WriteLine("4. TestMethod executed");
        await Assert.That(ClassLog).Contains("BeforeClass executed");
    }

    [After(Test)]
    public async Task AfterTest()
    {
        Console.WriteLine("5. AfterTest executed - Test cleanup");
        await Task.Delay(5);
    }

    [After(Class)]
    public static async Task AfterClass()
    {
        Console.WriteLine("6. AfterClass executed - Class-level cleanup");
        await Task.Delay(10);
    }
}
```

**Important Observations:**

1. **Constructor Priority**: Always executes before all TUnit lifecycle attributes
2. **BeforeClass Executes Once**: Executed once before all tests begin
3. **Test Execution is Parallel**: Multiple test methods may execute simultaneously
4. **AfterClass Executes Once**: Executed once after all tests complete
