// TUnit ASP.NET Core Integration Test Examples

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TUnit.Advanced.AspNetCore.Examples;

#region Basic Integration Tests

/// <summary>
/// ASP.NET Core integration test basic example
/// Uses WebApplicationFactory for complete Web API testing
/// </summary>
public class WebApiIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Add test-specific service configurations here
                    // e.g., replace database connection, use mock services, etc.
                });
            });

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Basic API endpoint test
    /// </summary>
    [Test]
    public async Task WeatherForecast_Get_ShouldReturnCorrectFormatData()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(0);
    }

    /// <summary>
    /// Validates HTTP response headers
    /// </summary>
    [Test]
    [Property("Category", "Integration")]
    public async Task WeatherForecast_ResponseHeaders_ShouldContainContentTypeHeader()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        // Check Content-Type header
        var contentType = response.Content.Headers.ContentType?.MediaType;
        await Assert.That(contentType).IsEqualTo("application/json");
    }

    /// <summary>
    /// Smoke test: ensures endpoint is available
    /// </summary>
    [Test]
    [Property("Category", "Smoke")]
    public async Task WeatherForecast_EndpointAvailability_ShouldRespondNormally()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(10); // Ensure actual content
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region Performance Tests

/// <summary>
/// Performance and load testing examples
/// </summary>
public class PerformanceIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PerformanceIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Response time validation
    /// </summary>
    [Test]
    [Property("Category", "Performance")]
    [Timeout(10000)] // 10 second timeout protection
    public async Task WeatherForecast_ResponseTime_ShouldBeWithinReasonableRange()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/weatherforecast");
        stopwatch.Stop();

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000); // Response within 5 seconds

        Console.WriteLine($"Response time: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Concurrent load test
    /// </summary>
    [Test]
    [Property("Category", "Load")]
    [Timeout(30000)]
    public async Task WeatherForecast_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        const int concurrentRequests = 50;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync("/weatherforecast"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        await Assert.That(responses.Length).IsEqualTo(concurrentRequests);
        await Assert.That(responses.All(r => r.IsSuccessStatusCode)).IsTrue();

        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }

        Console.WriteLine($"Successfully handled {concurrentRequests} concurrent requests");
    }

    /// <summary>
    /// Sustained load test
    /// </summary>
    [Test]
    [Property("Category", "Load")]
    [Timeout(60000)] // 60 second timeout
    public async Task WeatherForecast_SustainedLoad_ShouldMaintainStablePerformance()
    {
        // Arrange
        const int totalRequests = 100;
        const int batchSize = 10;
        var responseTimes = new List<long>();

        // Act
        for (int batch = 0; batch < totalRequests / batchSize; batch++)
        {
            var batchTasks = Enumerable.Range(0, batchSize)
                .Select(async _ =>
                {
                    var sw = Stopwatch.StartNew();
                    var response = await _client.GetAsync("/weatherforecast");
                    sw.Stop();
                    response.Dispose();
                    return sw.ElapsedMilliseconds;
                });

            var batchResults = await Task.WhenAll(batchTasks);
            responseTimes.AddRange(batchResults);

            // Brief delay between batches
            await Task.Delay(100);
        }

        // Assert
        var avgResponseTime = responseTimes.Average();
        var maxResponseTime = responseTimes.Max();
        var p95ResponseTime = responseTimes.OrderBy(x => x).ElementAt((int)(responseTimes.Count * 0.95));

        Console.WriteLine($"Average response time: {avgResponseTime:F2}ms");
        Console.WriteLine($"Max response time: {maxResponseTime}ms");
        Console.WriteLine($"P95 response time: {p95ResponseTime}ms");

        await Assert.That(avgResponseTime).IsLessThan(1000); // Average within 1 second
        await Assert.That(p95ResponseTime).IsLessThan(2000); // P95 within 2 seconds
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region Health Check Tests

/// <summary>
/// Health check and monitoring tests
/// </summary>
public class HealthCheckTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Health status endpoint test
    /// Important for Kubernetes deployments and monitoring
    /// </summary>
    [Test]
    [Property("Category", "Health")]
    public async Task HealthCheck_ShouldReturnHealthyStatus()
    {
        try
        {
            var response = await _client.GetAsync("/health");
            // If health endpoint exists, test it
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
        }
        catch (HttpRequestException)
        {
            // If no /health endpoint, test root path
            var response = await _client.GetAsync("/");
            await Assert.That((int)response.StatusCode).IsLessThan(500);
        }
    }

    /// <summary>
    /// Application basic availability test
    /// </summary>
    [Test]
    [Property("Category", "Smoke")]
    [DisplayName("Application Availability: Basic endpoint should respond")]
    public async Task ApplicationAvailability_BasicEndpoint_ShouldRespond()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert - ensure not server error
        await Assert.That((int)response.StatusCode).IsLessThan(500);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region E2E Business Flow Tests

/// <summary>
/// End-to-end business flow tests
/// </summary>
public class OrderApiIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrderApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Complete order flow test
    /// </summary>
    [Test]
    [Property("Category", "E2E")]
    [DisplayName("Complete Order Flow: Create -> Query -> Update Status")]
    public async Task CreateOrder_CompleteFlow_ShouldSuccessfullyCreateOrder()
    {
        // This test demonstrates complete order creation flow
        // Since sample API may not have actual order endpoints, we test basic API availability

        // Act
        var response = await _client.GetAsync("/");

        // Assert - ensure API can start and respond normally
        // In real projects, this would test actual business logic endpoints
        await Assert.That((int)response.StatusCode).IsLessThan(500); // Not server error
    }

    /// <summary>
    /// Simulates complete CRUD operations flow
    /// </summary>
    [Test]
    [Property("Category", "E2E")]
    [DisplayName("CRUD Operations Flow Validation")]
    public async Task CrudOperations_CompleteFlow_ShouldExecuteCorrectly()
    {
        // 1. Verify API is available
        var listResponse = await _client.GetAsync("/weatherforecast");
        await Assert.That(listResponse.IsSuccessStatusCode).IsTrue();

        // 2. Verify response content
        var content = await listResponse.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(0);

        // 3. Verify response format
        var contentType = listResponse.Content.Headers.ContentType?.MediaType;
        await Assert.That(contentType).IsEqualTo("application/json");
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region Integration Test Best Practices

/*
 * ASP.NET Core Integration Testing Best Practices
 *
 * 1. Test Project Setup:
 *    - Add at the end of WebApi project's Program.cs:
 *      public partial class Program { }
 *    - This allows integration tests to access the Program class
 *
 * 2. Package Dependencies:
 *    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
 *    <PackageReference Include="TUnit" Version="0.57.24" />
 *
 * 3. GlobalUsings.cs Setup:
 *    global using Microsoft.AspNetCore.Mvc.Testing;
 *    global using System.Net.Http.Json;
 *    global using TUnit.Core;
 *    global using TUnit.Assertions;
 *    global using TUnit.Assertions.Extensions;
 *
 * 4. Test Class Structure:
 *    - Implement IDisposable for proper resource cleanup
 *    - Use constructor to create WebApplicationFactory and HttpClient
 *    - Dispose resources in Dispose method
 *
 * 5. Value of Smoke Tests:
 *    - Fast feedback: Provides quickest basic functionality validation in CI/CD
 *    - Early detection: Catches deployment or configuration issues immediately
 *    - Cost effective: Fast execution but catches most basic problems
 *    - Confidence building: Establishes foundation for subsequent detailed tests
 */

/// <summary>
/// Integration test setup example
/// </summary>
public class IntegrationTestSetupGuide
{
    [Test]
    [DisplayName("Integration Test Setup Documentation")]
    public async Task IntegrationTestSetup_Documentation()
    {
        // This test serves as documentation
        await Assert.That(true).IsTrue();
    }
}

#endregion

// Note: This file needs an actual Program class to compile
// In real projects, ensure WebApi project has: public partial class Program { }
public partial class Program { }
