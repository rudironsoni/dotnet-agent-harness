// TUnit Execution Control Examples - Retry, Timeout, DisplayName

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Diagnostics;

namespace TUnit.Advanced.ExecutionControl.Examples;

#region Domain Models

public enum CustomerLevel
{
    RegularMember = 0,
    VipMember = 1,
    PlatinumMember = 2,
    DiamondMember = 3
}

#endregion

#region Retry Mechanism

/// <summary>
/// Retry mechanism examples
/// Used for tests that may fail occasionally due to external factors
/// </summary>
public class RetryMechanismExamples
{
    /// <summary>
    /// Basic Retry usage
    /// If failed, retry up to 3 times
    /// </summary>
    [Test]
    [Retry(3)]
    [Property("Category", "Flaky")]
    public async Task NetworkCall_PotentiallyUnstable_UseRetryMechanism()
    {
        // Simulate potentially failing network call
        var random = new Random();
        var success = random.Next(1, 4) == 1; // ~33% success rate

        if (!success)
        {
            throw new HttpRequestException("Simulated network error");
        }

        await Assert.That(success).IsTrue();
    }

    /// <summary>
    /// Retry best practices for external API calls
    /// </summary>
    [Test]
    [Retry(3)]
    [Property("Category", "ExternalDependency")]
    public async Task CallExternalApi_WhenNetworkIssuesRetry_ShouldEventuallySucceed()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            // Actual external API call
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");

            await Assert.That(response.IsSuccessStatusCode).IsTrue();

            var content = await response.Content.ReadAsStringAsync();
            await Assert.That(content).IsNotNull();
        }
        catch (TaskCanceledException)
        {
            // Timeout is also a transient error, can retry
            throw new HttpRequestException("Request timeout, will retry");
        }
    }

    /// <summary>
    /// When NOT to use Retry: tests expected to fail
    /// </summary>
    [Test]
    // Do NOT use Retry for tests expected to fail
    public async Task Divide_DivideByZero_ShouldThrowException()
    {
        await Assert.That(() => { var _ = 10 / int.Parse("0"); }).Throws<DivideByZeroException>();
    }
}

/// <summary>
/// Retry usage scenarios guide
/// </summary>
public class RetryUsageGuide
{
    /*
     * When to use Retry:
     *
     * 1. External service calls
     *    - API requests may temporarily fail due to network issues
     *    - Database connections may temporarily disconnect
     *
     * 2. File system operations
     *    - File locks may cause temporary failures in CI/CD
     *
     * 3. Concurrent test competition
     *    - Race conditions when multiple tests access shared resources
     *
     * When NOT to use Retry:
     *
     * 1. Logic errors
     *    - Code bugs won't be fixed by retrying
     *
     * 2. Expected exceptions
     *    - Tests that verify exception behavior
     *
     * 3. Performance tests
     *    - Retry affects performance measurement accuracy
     */

    [Test]
    public async Task RetryGuidelines_BestPracticeDocumentation()
    {
        await Assert.That(true).IsTrue();
    }
}

#endregion

#region Timeout Control

/// <summary>
/// Timeout control examples
/// Ensures tests complete within reasonable time
/// </summary>
public class TimeoutControlExamples
{
    /// <summary>
    /// Basic Timeout usage
    /// 5 second timeout protection
    /// </summary>
    [Test]
    [Timeout(5000)]
    [Property("Category", "Performance")]
    public async Task LongRunningOperation_ShouldCompleteWithinTimeLimit()
    {
        // Simulate potentially slow operation
        await Task.Delay(1000); // 1 second operation, within 5 second limit

        await Assert.That(true).IsTrue();
    }

    /// <summary>
    /// Longer Timeout setting
    /// Suitable for more complex operations
    /// </summary>
    [Test]
    [Timeout(30000)] // 30 second timeout
    [Property("Category", "Integration")]
    public async Task DatabaseMigration_BulkDataProcessing_ShouldCompleteWithinReasonableTime()
    {
        // Simulate database migration or bulk data processing
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(ProcessDataBatch(i));
        }

        await Task.WhenAll(tasks);
        await Assert.That(tasks.All(t => t.IsCompletedSuccessfully)).IsTrue();
    }

    private static async Task ProcessDataBatch(int batchNumber)
    {
        // Simulate batch processing
        await Task.Delay(50); // 50ms per batch
    }

    /// <summary>
    /// Performance baseline test
    /// Combines Timeout with performance measurement
    /// </summary>
    [Test]
    [Timeout(1000)] // Ensure doesn't exceed 1 second
    [Property("Category", "Performance")]
    [Property("Baseline", "true")]
    public async Task SearchFunction_PerformanceBaseline_ShouldMeetSLARequirements()
    {
        var stopwatch = Stopwatch.StartNew();

        // Simulate search functionality
        var searchResults = await PerformSearch("test query");

        stopwatch.Stop();

        // Functional validation
        await Assert.That(searchResults).IsNotNull();
        await Assert.That(searchResults.Count()).IsGreaterThan(0);

        // Performance validation: 99% of queries should complete within 500ms
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(500);
    }

    private static async Task<IEnumerable<string>> PerformSearch(string query)
    {
        // Simulate search logic
        await Task.Delay(100);
        return new[] { "result1", "result2", "result3" };
    }
}

#endregion

#region DisplayName

/// <summary>
/// DisplayName custom test name examples
/// Improves test report readability
/// </summary>
public class DisplayNameExamples
{
    /// <summary>
    /// Basic DisplayName usage
    /// </summary>
    [Test]
    [DisplayName("Custom test name: Verify user registration flow")]
    public async Task UserRegistration_CustomDisplayName_TestNameMoreReadable()
    {
        // Use custom display name to make test reports easier to understand
        await Assert.That("user@example.com").Contains("@");
    }

    /// <summary>
    /// Dynamic display name for parameterized tests
    /// DisplayName automatically replaces parameter values
    /// </summary>
    [Test]
    [Arguments("valid@email.com", true)]
    [Arguments("invalid-email", false)]
    [Arguments("", false)]
    [Arguments("test@domain.co.uk", true)]
    [Arguments("user.name+tag@example.com", true)]
    [DisplayName("Email validation: {0} should be {1}")]
    public async Task EmailValidation_ParameterizedDisplayName(string email, bool expectedValid)
    {
        // Display name automatically replaces parameters
        // Generated name like: "Email validation: valid@email.com should be True"
        var isValid = !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");

        await Assert.That(isValid).IsEqualTo(expectedValid);
    }

    /// <summary>
    /// Business scenario-driven display name
    /// Uses business language instead of technical terms
    /// </summary>
    [Test]
    [Arguments(CustomerLevel.RegularMember, 1000, 0)]
    [Arguments(CustomerLevel.VipMember, 1000, 50)]
    [Arguments(CustomerLevel.PlatinumMember, 1000, 100)]
    [Arguments(CustomerLevel.DiamondMember, 1000, 200)]
    [DisplayName("Customer level {0} purchases ${1} should get ${2} discount")]
    public async Task MemberDiscount_ByCustomerLevel_ShouldCalculateCorrectDiscount(
        CustomerLevel level, decimal amount, decimal expectedDiscount)
    {
        // Test report reads like business requirements
        var discount = CalculateDiscount(amount, level);

        await Assert.That(discount).IsEqualTo(expectedDiscount);
    }

    private static decimal CalculateDiscount(decimal amount, CustomerLevel level)
    {
        return level switch
        {
            CustomerLevel.DiamondMember => amount * 0.2m,
            CustomerLevel.PlatinumMember => amount * 0.1m,
            CustomerLevel.VipMember => amount * 0.05m,
            _ => 0m
        };
    }

    /// <summary>
    /// Order status transition business language display name
    /// </summary>
    [Test]
    [Arguments("Created", "Paid", true)]
    [Arguments("Paid", "Shipped", true)]
    [Arguments("Shipped", "Delivered", true)]
    [Arguments("Cancelled", "Shipped", false)]
    [DisplayName("Order status transition from {0} to {1} should be {2}")]
    public async Task OrderStatusTransition_StatusTransitionValidation(
        string fromStatus, string toStatus, bool expectedValid)
    {
        var isValid = IsValidTransition(fromStatus, toStatus);
        await Assert.That(isValid).IsEqualTo(expectedValid);
    }

    private static bool IsValidTransition(string from, string to)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            ["Created"] = new[] { "Paid", "Cancelled" },
            ["Paid"] = new[] { "Shipped", "Refunded" },
            ["Shipped"] = new[] { "Delivered", "Returned" },
            ["Delivered"] = new[] { "Completed", "Returned" }
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}

#endregion

#region Combined Examples

/// <summary>
/// Combined usage examples
/// Uses Retry, Timeout, DisplayName simultaneously
/// </summary>
public class CombinedExecutionControlExamples
{
    /// <summary>
    /// Complete execution control combination
    /// </summary>
    [Test]
    [Retry(2)]
    [Timeout(5000)]
    [Property("Category", "Integration")]
    [DisplayName("External API Integration Test: Health Check")]
    public async Task ExternalApiHealthCheck_CompleteExecutionControl()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(3);

        try
        {
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Exception thrown during retry
            throw new HttpRequestException($"API health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Performance test execution control combination
    /// </summary>
    [Test]
    [Timeout(10000)]
    [Property("Category", "Performance")]
    [Property("Priority", "High")]
    [DisplayName("Performance Baseline: Batch processing should complete within 10 seconds")]
    public async Task PerformanceBenchmark_BatchProcessingPerformance()
    {
        var stopwatch = Stopwatch.StartNew();

        // Simulate batch processing
        var tasks = Enumerable.Range(0, 50)
            .Select(async i =>
            {
                await Task.Delay(Random.Shared.Next(10, 50));
                return i;
            });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        await Assert.That(results.Length).IsEqualTo(50);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000);

        Console.WriteLine($"Batch processing completed in {stopwatch.ElapsedMilliseconds}ms");
    }
}

#endregion
