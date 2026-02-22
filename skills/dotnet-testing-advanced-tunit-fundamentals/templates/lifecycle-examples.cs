namespace MyApp.Tests;

/// <summary>
/// TUnit test lifecycle management examples
/// Shows Before/After attributes and constructor/Dispose patterns
/// </summary>

#region Basic Lifecycle: Constructor and Dispose

/// <summary>
/// Basic lifecycle management using constructor and Dispose
/// Fully compatible with xUnit
/// </summary>
public class BasicLifecycleTests : IDisposable
{
    private readonly Calculator _calculator;

    // Constructor called before each test
    public BasicLifecycleTests()
    {
        _calculator = new Calculator();
        Console.WriteLine("Constructor: Creating Calculator instance");
    }

    [Test]
    public async Task Add_BasicTest()
    {
        Console.WriteLine("Executing test: Add_BasicTest");
        await Assert.That(_calculator.Add(1, 2)).IsEqualTo(3);
    }

    [Test]
    public async Task Multiply_BasicTest()
    {
        Console.WriteLine("Executing test: Multiply_BasicTest");
        await Assert.That(_calculator.Multiply(3, 4)).IsEqualTo(12);
    }

    // Dispose called after each test
    public void Dispose()
    {
        Console.WriteLine("Dispose: Cleaning up resources");
        // Perform necessary cleanup
    }
}

#endregion

#region Advanced Lifecycle: Before/After Attributes

/// <summary>
/// Advanced lifecycle management using Before and After attributes
/// </summary>
public class DatabaseLifecycleTests
{
    private static TestDatabase? _database;

    // Class level: executed once before all tests (static method)
    [Before(Class)]
    public static async Task ClassSetup()
    {
        _database = new TestDatabase();
        await _database.InitializeAsync();
        Console.WriteLine("Database initialization complete");
    }

    // Test level: executed before each test (instance method)
    [Before(Test)]
    public async Task TestSetup()
    {
        Console.WriteLine("Test preparation: Clearing database state");
        await _database!.ClearDataAsync();
    }

    [Test]
    public async Task TestUserCreation()
    {
        // Arrange
        var userService = new UserService(_database!);

        // Act
        var user = await userService.CreateUserAsync("test@example.com");

        // Assert
        await Assert.That(user.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(user.Email).IsEqualTo("test@example.com");
    }

    [Test]
    public async Task TestUserQuery()
    {
        // Arrange
        var userService = new UserService(_database!);
        await userService.CreateUserAsync("query@example.com");

        // Act
        var user = await userService.GetUserByEmailAsync("query@example.com");

        // Assert
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Email).IsEqualTo("query@example.com");
    }

    // Test level: executed after each test
    [After(Test)]
    public async Task TestTearDown()
    {
        Console.WriteLine("Test cleanup: Logging test results");
        await Task.CompletedTask;
    }

    // Class level: executed once after all tests
    [After(Class)]
    public static async Task ClassTearDown()
    {
        if (_database != null)
        {
            await _database.DisposeAsync();
            Console.WriteLine("Database connection closed");
        }
    }
}

#endregion

#region Lifecycle Execution Order Demo

/// <summary>
/// Demonstrates complete lifecycle execution order
/// Execution order: Before(Class) -> Constructor -> Before(Test) -> Test -> After(Test) -> Dispose -> After(Class)
/// </summary>
public class LifecycleOrderDemoTests : IDisposable
{
    public LifecycleOrderDemoTests()
    {
        Console.WriteLine("2. Constructor executed");
    }

    [Before(Class)]
    public static void ClassSetup()
    {
        Console.WriteLine("1. Before(Class) executed");
    }

    [Before(Test)]
    public async Task TestSetup()
    {
        Console.WriteLine("3. Before(Test) executed");
        await Task.CompletedTask;
    }

    [Test]
    public async Task DemoTest()
    {
        Console.WriteLine("4. Test method executed");
        await Assert.That(true).IsTrue();
    }

    [After(Test)]
    public async Task TestTearDown()
    {
        Console.WriteLine("5. After(Test) executed");
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Console.WriteLine("6. Dispose executed");
    }

    [After(Class)]
    public static void ClassTearDown()
    {
        Console.WriteLine("7. After(Class) executed");
    }
}

#endregion

#region Async Lifecycle

/// <summary>
/// Demonstrates async lifecycle methods
/// </summary>
public class AsyncLifecycleTests
{
    private HttpClient? _httpClient;

    [Before(Test)]
    public async Task SetupAsync()
    {
        // Async setup
        _httpClient = new HttpClient();
        await Task.Delay(100); // Simulate async initialization
    }

    [Test]
    public async Task HttpRequestTest()
    {
        // Use configured HttpClient
        var response = await _httpClient!.GetAsync("https://httpbin.org/status/200");
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        // Async cleanup
        if (_httpClient != null)
        {
            _httpClient.Dispose();
        }
        await Task.CompletedTask;
    }
}

#endregion

#region Helper Classes

public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Multiply(int a, int b) => a * b;
}

public class TestDatabase : IAsyncDisposable
{
    public Task InitializeAsync() => Task.CompletedTask;
    public Task ClearDataAsync() => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class UserService
{
    private readonly TestDatabase _database;

    public UserService(TestDatabase database)
    {
        _database = database;
    }

    public Task<User> CreateUserAsync(string email)
    {
        return Task.FromResult(new User
        {
            Id = Guid.NewGuid(),
            Email = email
        });
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return Task.FromResult<User?>(new User
        {
            Id = Guid.NewGuid(),
            Email = email
        });
    }
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

#endregion
