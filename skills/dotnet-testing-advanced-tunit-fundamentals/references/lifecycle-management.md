# Test Lifecycle Management

## Constructor and Dispose Pattern

```csharp
public class BasicLifecycleTests : IDisposable
{
    private readonly Calculator _calculator;

    public BasicLifecycleTests()
    {
        _calculator = new Calculator();
    }

    [Test]
    public async Task Add_BasicTest()
    {
        await Assert.That(_calculator.Add(1, 2)).IsEqualTo(3);
    }

    public void Dispose()
    {
        // Cleanup resources
    }
}
```

## Before / After Attributes

TUnit provides more granular lifecycle control:

```csharp
public class LifecycleTests
{
    private static TestDatabase? _database;

    // Class level: executes once before all tests
    [Before(Class)]
    public static async Task ClassSetup()
    {
        _database = new TestDatabase();
        await _database.InitializeAsync();
    }

    // Test level: executes before each test
    [Before(Test)]
    public async Task TestSetup()
    {
        await _database!.ClearDataAsync();
    }

    [Test]
    public async Task TestUserCreation()
    {
        var userService = new UserService(_database!);
        var user = await userService.CreateUserAsync("test@example.com");
        await Assert.That(user.Id).IsNotEqualTo(Guid.Empty);
    }

    // Test level: executes after each test
    [After(Test)]
    public async Task TestTearDown()
    {
        // Log test results
    }

    // Class level: executes once after all tests
    [After(Class)]
    public static async Task ClassTearDown()
    {
        if (_database != null)
        {
            await _database.DisposeAsync();
        }
    }
}
```

## Lifecycle Attribute Types

| Attribute            | Type           | Description                          |
| -------------------- | -------------- | ------------------------------------ |
| `[Before(Test)]`     | Instance method | Before each test executes            |
| `[Before(Class)]`    | Static method  | Before first test in class executes  |
| `[Before(Assembly)]` | Static method  | Before first test in assembly executes |
| `[After(Test)]`      | Instance method | After each test executes             |
| `[After(Class)]`     | Static method  | After last test in class executes    |
| `[After(Assembly)]`  | Static method  | After last test in assembly executes |

## Execution Order

```text
1. Before(Class)
2. Constructor
3. Before(Test)
4. Test Method
5. After(Test)
6. Dispose
7. After(Class)
```
