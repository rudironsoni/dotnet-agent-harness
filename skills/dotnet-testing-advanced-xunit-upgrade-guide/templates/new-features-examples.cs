// =============================================================================
// xUnit 3.x New Features Usage Examples
// =============================================================================

using System.Globalization;
using System.Runtime.InteropServices;
using Xunit;

namespace XunitUpgradeGuide.Examples;

// =============================================================================
// 1. [Test] Attribute - Unified Test Marker
// =============================================================================

public class TestAttributeExamples
{
    // New [Test] attribute, equivalent to [Fact]
    [Test]
    public void UsingTestAttributeTest()
    {
        Assert.True(true);
    }

    // [Fact] still available
    [Fact]
    public void UsingFactAttributeTest()
    {
        Assert.True(true);
    }

    // [Theory] for parameterized tests
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    public void ParameterizedTest(int a, int b, int expected)
    {
        Assert.Equal(expected, a + b);
    }
}

// =============================================================================
// 2. Explicit Tests
// =============================================================================

public class ExplicitTestExamples
{
    // Explicit test: not run by default, only when explicitly requested
    [Fact(Explicit = true)]
    public void ExpensiveIntegrationTest()
    {
        // This test only runs when explicitly selected
        // Suitable for: performance tests, long-running tests, tests requiring special environment
        Thread.Sleep(1000); // Simulate time-consuming operation
        Assert.True(true);
    }

    [Fact(Explicit = true)]
    public void SpecialEnvironmentTest()
    {
        // e.g., requires specific database, external services, etc.
        Assert.True(true);
    }
}

// =============================================================================
// 3. Dynamic Skip Tests
// =============================================================================

public class DynamicSkipExamples
{
    // Use Assert.Skip (imperative)
    [Fact]
    public void SkipTestBasedOnFeatureFlag()
    {
        var featureEnabled = GetFeatureFlag("NEW_CALCULATION_ENGINE");

        if (!featureEnabled)
        {
            Assert.Skip("New calculation engine feature not enabled");
        }

        // Test new feature...
        Assert.True(true);
    }

    // Use SkipUnless (declarative)
    [Fact(SkipUnless = nameof(IsLinuxEnvironment),
          Skip = "This test only runs on Linux")]
    public void LinuxOnlyTest()
    {
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
    }

    // Use SkipWhen (declarative)
    [Fact(SkipWhen = nameof(IsDebugBuild),
          Skip = "This test is skipped in Debug build")]
    public void ReleaseBuildOnlyTest()
    {
        Assert.True(true);
    }

    // Static properties
    public static bool IsLinuxEnvironment
        => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsDebugBuild
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

    private bool GetFeatureFlag(string flagName)
        => bool.TryParse(Environment.GetEnvironmentVariable($"FEATURE_{flagName}"),
                         out var result) && result;
}

// =============================================================================
// 4. Matrix Theory Data
// =============================================================================

public class MatrixTheoryDataExamples
{
    // Matrix combination: generates all possible combinations
    public static TheoryData<int, string> MatrixData =>
        new MatrixTheoryData<int, string>(
            [1, 2, 3],                      // 3 numbers
            ["Hello", "World", "Test"]       // 3 strings
        );
        // Generates 3x3=9 test cases:
        // (1, "Hello"), (1, "World"), (1, "Test")
        // (2, "Hello"), (2, "World"), (2, "Test")
        // (3, "Hello"), (3, "World"), (3, "Test")

    [Theory]
    [MemberData(nameof(MatrixData))]
    public void MatrixTestExample(int number, string text)
    {
        Assert.True(number > 0);
        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }

    // More complex matrix combination
    public static TheoryData<string, int, bool> ComplexMatrixData =>
        new MatrixTheoryData<string, int, bool>(
            ["Admin", "User", "Guest"],     // Roles
            [1, 5, 10],                      // Permission levels
            [true, false]                    // Enabled status
        );
        // Generates 3x3x2=18 test cases

    [Theory]
    [MemberData(nameof(ComplexMatrixData))]
    public void ComplexMatrixTest(string role, int level, bool enabled)
    {
        Assert.NotNull(role);
        Assert.InRange(level, 1, 10);
    }
}

// =============================================================================
// 5. Assembly Fixtures (Assembly-level Setup)
// =============================================================================

/// <summary>
/// Assembly Fixture: Resources shared across entire test assembly
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Initialize before all tests run
        ConnectionString = "Server=localhost;Database=TestDb;";
        await Task.CompletedTask;

        Console.WriteLine("Assembly Fixture initialized");
    }

    public async Task DisposeAsync()
    {
        // Cleanup after all tests run
        await Task.CompletedTask;

        Console.WriteLine("Assembly Fixture cleaned up");
    }
}

// Register Assembly Fixture (usually in AssemblyInfo.cs or project root)
// [assembly: AssemblyFixture(typeof(TestDatabaseFixture))]

// Using Assembly Fixture in tests
public class AssemblyFixtureExamples
{
    private readonly TestDatabaseFixture _dbFixture;

    public AssemblyFixtureExamples(TestDatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public void UsingAssemblyFixtureTest()
    {
        Assert.NotNull(_dbFixture.ConnectionString);
    }
}

// =============================================================================
// 6. Test Pipeline Startup (Pre-test Execution)
// =============================================================================

/// <summary>
/// Test Pipeline Startup: Global initialization before any test runs
/// </summary>
public class CustomTestPipelineStartup : ITestPipelineStartup
{
    public async ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        // Global initialization logic
        diagnosticMessageSink.OnMessage(
            new DiagnosticMessage("Initializing test environment..."));

        // e.g., set environment variables, initialize shared resources
        Environment.SetEnvironmentVariable("TEST_MODE", "true");

        await Task.CompletedTask;
    }
}

// Register Test Pipeline Startup (usually in AssemblyInfo.cs)
// [assembly: TestPipelineStartup(typeof(CustomTestPipelineStartup))]

// =============================================================================
// 7. Culture Settings (Multilingual Tests)
// =============================================================================

public class CultureTestExamples
{
    [Fact]
    public void CurrencyFormatTestUsingEnglishCulture()
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var result = 123.45m.ToString("C");
            Assert.Equal("$123.45", result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void DateFormatTestUsingTraditionalChineseCulture()
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var testDate = new DateTime(2024, 12, 31);

        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-TW");
            var result = testDate.ToString("yyyy/MM/dd");
            Assert.Equal("2024/12/31", result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("en-US", "$123.45")]
    [InlineData("zh-TW", "NT$123.45")]
    [InlineData("ja-JP", "￥123")]
    public void MultiCultureCurrencyFormatTest(string cultureName, string expected)
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            var result = cultureName == "ja-JP"
                ? 123m.ToString("C")
                : 123.45m.ToString("C");
            Assert.Equal(expected, result);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}

// =============================================================================
// 8. Improved Test Diagnostics
// =============================================================================

public class DiagnosticsExamples
{
    private readonly ITestOutputHelper _output;

    public DiagnosticsExamples(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DetailedDiagnosticsTest()
    {
        // xUnit 3.x automatically provides more detailed test execution info
        _output.WriteLine("Test started");
        _output.WriteLine($"Execution time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        var startTime = DateTime.Now;

        // Execute test logic
        var result = PerformCalculation(5, 3);

        var duration = DateTime.Now - startTime;
        _output.WriteLine($"Execution time: {duration.TotalMilliseconds:F2} ms");
        _output.WriteLine($"Calculation result: {result}");

        Assert.Equal(8, result);
    }

    private int PerformCalculation(int a, int b) => a + b;
}
