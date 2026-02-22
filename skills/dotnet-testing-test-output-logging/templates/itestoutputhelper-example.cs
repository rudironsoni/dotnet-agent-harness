using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// ITestOutputHelper usage examples
/// Demonstrates how to correctly use ITestOutputHelper for diagnostic output in xUnit tests
/// </summary>
public class ITestOutputHelperExample
{
    private readonly ITestOutputHelper _output;

    // Correct injection method: through constructor injection
    public ITestOutputHelperExample(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Fact]
    public void BasicOutputExample_DemonstratesBasicOutput()
    {
        // Arrange
        var productName = "Laptop";
        var price = 30000;

        // Act
        _output.WriteLine("=== Test Start ===");
        _output.WriteLine($"Product Name: {productName}");
        _output.WriteLine($"Price: NT${price:N0}");

        var discountedPrice = price * 0.9m;
        _output.WriteLine($"Discounted Price: NT${discountedPrice:N0}");

        // Assert
        _output.WriteLine("=== Test Complete ===");
        Assert.True(discountedPrice < price);
    }

    [Fact]
    public void StructuredOutputExample_DemonstratesStructuredOutput()
    {
        // Arrange
        LogSection("Test Setup");
        var customer = new { Name = "John Doe", Level = "VIP" };
        LogKeyValue("Customer Name", customer.Name);
        LogKeyValue("Membership Level", customer.Level);

        // Act
        LogSection("Execute Test");
        var discount = CalculateDiscount(customer.Level);
        LogKeyValue("Calculated Discount", $"{discount}%");

        // Assert
        LogSection("Verify Result");
        Assert.Equal(10, discount);
        _output.WriteLine("Discount calculation correct");
    }

    [Fact]
    public async Task PerformanceTestExample_DemonstratesPerformanceTestOutput()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        _output.WriteLine("=== Performance Test Start ===");
        _output.WriteLine($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

        // Act - Stage 1
        await Task.Delay(100); // Simulate data loading
        var loadTime = stopwatch.Elapsed;
        _output.WriteLine($"Data Load Complete: {loadTime.TotalMilliseconds:F2} ms");

        // Act - Stage 2
        await Task.Delay(50); // Simulate data processing
        var processTime = stopwatch.Elapsed;
        _output.WriteLine($"Data Processing Complete: {processTime.TotalMilliseconds:F2} ms");

        stopwatch.Stop();

        // Assert & Report
        _output.WriteLine("\n=== Performance Report ===");
        _output.WriteLine($"Total Execution Time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
        _output.WriteLine($"Meets Performance Requirements (< 200ms): {stopwatch.Elapsed.TotalMilliseconds < 200}");

        Assert.True(stopwatch.Elapsed.TotalMilliseconds < 200);
    }

    // Helper methods: structured output
    private void LogSection(string title)
    {
        _output.WriteLine($"\n=== {title} ===");
    }

    private void LogKeyValue(string key, object value)
    {
        _output.WriteLine($"{key}: {value}");
    }

    private int CalculateDiscount(string customerLevel)
    {
        return customerLevel == "VIP" ? 10 : 0;
    }
}

// ❌ Wrong example: don't do this
public class WrongITestOutputHelperUsage
{
    // Error 1: using static field
    private static ITestOutputHelper _staticOutput; // ❌ won't work

    public WrongITestOutputHelperUsage(ITestOutputHelper output)
    {
        _staticOutput = output;
    }

    // Error 2: trying to use in static method
    public static void StaticHelper()
    {
        // _staticOutput.WriteLine("This will fail"); // ❌ won't work
    }

    // Error 3: trying to use in Dispose
    public void Dispose()
    {
        // _staticOutput?.WriteLine("Test cleanup"); // ❌ may fail
    }
}
