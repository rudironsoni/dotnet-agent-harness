// =============================================================================
// xUnit 2.x -> 3.x Code Migration Examples
// =============================================================================

using System.Runtime.InteropServices;
using Xunit;

namespace XunitUpgradeGuide.Examples;

// =============================================================================
// 1. async void -> async Task Fix
// =============================================================================

/// <summary>
/// Before: async void test (not supported in xUnit 3.x)
/// </summary>
public class AsyncVoidTests_Before
{
    // This will fail in xUnit 3.x
    // [Fact]
    // public async void TestAsyncMethod()
    // {
    //     var result = await SomeAsyncOperation();
    //     Assert.True(result);
    // }
}

/// <summary>
/// After: async Task test (correct syntax)
/// </summary>
public class AsyncVoidTests_After
{
    // Correct xUnit 3.x syntax
    [Fact]
    public async Task TestAsyncMethod()
    {
        var result = await SomeAsyncOperation();
        Assert.True(result);
    }

    private Task<bool> SomeAsyncOperation() => Task.FromResult(true);
}

// =============================================================================
// 2. IAsyncLifetime + IDisposable Fix
// =============================================================================

/// <summary>
/// Before: Implementing both IAsyncLifetime and IDisposable
/// </summary>
public class AsyncLifetimeTests_Before // : IAsyncLifetime, IDisposable
{
    // In xUnit 2.x, both Dispose and DisposeAsync are called
    // In xUnit 3.x, only DisposeAsync is called

    // public async Task InitializeAsync() { /* Initialize */ }
    // public async Task DisposeAsync() { /* Async cleanup */ }
    // public void Dispose() { /* Sync cleanup - not called in 3.x */ }
}

/// <summary>
/// After: Only use IAsyncLifetime
/// </summary>
public class AsyncLifetimeTests_After : IAsyncLifetime
{
    private IDisposable? _resource;

    public async Task InitializeAsync()
    {
        // Initialize resources
        _resource = await CreateResourceAsync();
    }

    public async Task DisposeAsync()
    {
        // Move all cleanup logic here
        _resource?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public void Test1()
    {
        Assert.NotNull(_resource);
    }

    private Task<IDisposable> CreateResourceAsync()
        => Task.FromResult<IDisposable>(new MemoryStream());
}

// =============================================================================
// 3. SkippableFact -> Assert.Skip Fix
// =============================================================================

/// <summary>
/// Before: Using SkippableFact (removed in xUnit 3.x)
/// </summary>
public class SkippableTests_Before
{
    // Removed in xUnit 3.x
    // [SkippableFact]
    // public void SkippableTest()
    // {
    //     Skip.If(!IsWindowsEnvironment, "Only run on Windows");
    //     // Test logic
    // }
}

/// <summary>
/// After: Using Assert.Skip (imperative)
/// </summary>
public class SkippableTests_After_Imperative
{
    [Fact]
    public void ConditionallySkippedTest()
    {
        // Use Assert.Skip (imperative)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Skip("This test only runs on Windows");
        }

        // Test logic
        Assert.True(true);
    }

    [Fact]
    public void EnvironmentVariableSkippedTest()
    {
        var enableTests = Environment.GetEnvironmentVariable("ENABLE_INTEGRATION_TESTS");

        if (string.IsNullOrEmpty(enableTests) || enableTests.ToLower() != "true")
        {
            Assert.Skip("Integration tests disabled. Set ENABLE_INTEGRATION_TESTS=true to run");
        }

        // Integration test logic
        Assert.True(true);
    }
}

/// <summary>
/// After: Using SkipUnless attribute (declarative)
/// </summary>
public class SkippableTests_After_Declarative
{
    // Use SkipUnless attribute (declarative)
    // Note: Must also set Skip property to provide skip message
    [Fact(SkipUnless = nameof(IsWindowsEnvironment),
          Skip = "This test only runs on Windows")]
    public void WindowsOnlyTest()
    {
        // Test logic
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    }

    // SkipWhen: skip when condition is true
    [Fact(SkipWhen = nameof(IsCIEnvironment),
          Skip = "This test is skipped in CI environment")]
    public void NotInCIEnvironmentTest()
    {
        // Test logic
        Assert.True(true);
    }

    // Static properties for SkipUnless/SkipWhen
    public static bool IsWindowsEnvironment
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsCIEnvironment
        => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
}

// =============================================================================
// 4. Custom DataAttribute Fix
// =============================================================================

/// <summary>
/// Before: xUnit 2.x DataAttribute implementation
/// </summary>
// public class CustomDataAttribute_Before : DataAttribute
// {
//     // xUnit 2.x synchronous method
//     public override IEnumerable<object[]> GetData(MethodInfo testMethod)
//     {
//         yield return new object[] { 1, "test1" };
//         yield return new object[] { 2, "test2" };
//     }
// }

/// <summary>
/// After: xUnit 3.x DataAttribute implementation
/// </summary>
public class CustomDataAttribute_After : DataAttribute
{
    // xUnit 3.x async method
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        // Supports async data loading
        var data = await LoadDataAsync();

        return data.Select(item => new TheoryDataRow(item.Id, item.Name))
                   .ToList();
    }

    private Task<List<(int Id, string Name)>> LoadDataAsync()
    {
        var data = new List<(int, string)>
        {
            (1, "test1"),
            (2, "test2"),
            (3, "test3")
        };
        return Task.FromResult(data);
    }
}

// Using custom attribute
public class CustomDataAttributeTests
{
    [Theory]
    [CustomData_After]
    public void UsingCustomDataAttributeTest(int id, string name)
    {
        Assert.True(id > 0);
        Assert.NotNullOrEmpty(name);
    }
}

// =============================================================================
// 5. ITestOutputHelper Migration
// =============================================================================

/// <summary>
/// ITestOutputHelper is still available in xUnit 3.x
/// But namespace has changed
/// </summary>
public class TestOutputTests
{
    private readonly ITestOutputHelper _output;

    public TestOutputTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OutputTestInformation()
    {
        // ITestOutputHelper usage unchanged
        _output.WriteLine("Test started");

        var result = 1 + 1;
        _output.WriteLine($"Calculation result: {result}");

        Assert.Equal(2, result);
    }
}
