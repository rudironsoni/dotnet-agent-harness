// =============================================================================
// ASP.NET Core Integration Testing - Test Base Class Template
// =============================================================================
// Purpose: Provide shared functionality for integration tests, including database operations and HttpClient management
// Usage: Have test classes inherit from this base class
// =============================================================================

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace YourProject.IntegrationTests;

/// <summary>
/// Integration test base class
/// Provides shared test setup, database operations, and HttpClient management
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    // ========================================
    // Database helper methods
    // ========================================

    /// <summary>
    /// Add test shipper data
    /// </summary>
    /// <param name="companyName">Company name</param>
    /// <param name="phone">Phone number</param>
    /// <returns>The added shipper ID</returns>
    protected async Task<int> SeedShipperAsync(string companyName, string phone = "02-12345678")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var shipper = new Shipper
        {
            CompanyName = companyName,
            Phone = phone,
            CreatedAt = DateTime.UtcNow
        };

        context.Shippers.Add(shipper);
        await context.SaveChangesAsync();

        return shipper.ShipperId;
    }

    /// <summary>
    /// Batch add multiple test shippers
    /// </summary>
    /// <param name="shippers">List of shipper data</param>
    /// <returns>List of added shipper IDs</returns>
    protected async Task<List<int>> SeedShippersAsync(
        params (string CompanyName, string Phone)[] shippers)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entities = shippers.Select(s => new Shipper
        {
            CompanyName = s.CompanyName,
            Phone = s.Phone,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        context.Shippers.AddRange(entities);
        await context.SaveChangesAsync();

        return entities.Select(e => e.ShipperId).ToList();
    }

    /// <summary>
    /// Clean up all shipper data from the database
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Shippers.RemoveRange(context.Shippers);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Get the count of shippers in the database
    /// </summary>
    protected async Task<int> GetShipperCountAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Shippers.CountAsync();
    }

    // ========================================
    // HTTP request helper methods
    // ========================================

    /// <summary>
    /// Send GET request and get result
    /// </summary>
    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>
    /// Send POST request and get result
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// Send PUT request and get result
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PutAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// Send DELETE request
    /// </summary>
    protected async Task DeleteAsync(string url)
    {
        var response = await Client.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }

    // ========================================
    // Resource cleanup
    // ========================================

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Client?.Dispose();
            Factory?.Dispose();
        }
    }
}

// =============================================================================
// Usage example
// =============================================================================

/// <summary>
/// Shippers controller integration test example
/// </summary>
public class ShippersControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetShipper_WhenShipperExists_ShouldReturnSuccessResult()
    {
        // Arrange
        await CleanupDatabaseAsync();
        var shipperId = await SeedShipperAsync("SF Express", "02-2345-6789");

        // Act
        var response = await Client.GetAsync($"/api/shippers/{shipperId}");

        // Assert
        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                {
                    result.Status.Should().Be("Success");
                    result.Data!.ShipperId.Should().Be(shipperId);
                    result.Data.CompanyName.Should().Be("SF Express");
                });
    }

    [Fact]
    public async Task CreateShipper_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        await CleanupDatabaseAsync();
        var createParameter = new ShipperCreateParameter
        {
            CompanyName = "Black Cat Home Delivery",
            Phone = "02-1234-5678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/shippers", createParameter);

        // Assert
        response.Should().Be201Created()
                .And
                .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                {
                    result.Status.Should().Be("Success");
                    result.Data!.ShipperId.Should().BeGreaterThan(0);
                    result.Data.CompanyName.Should().Be("Black Cat Home Delivery");
                });
    }

    [Fact]
    public async Task GetAllShippers_ShouldReturnAllShippers()
    {
        // Arrange
        await CleanupDatabaseAsync();
        await SeedShippersAsync(
            ("Company A", "02-1111-1111"),
            ("Company B", "02-2222-2222"),
            ("Company C", "02-3333-3333")
        );

        // Act
        var response = await Client.GetAsync("/api/shippers");

        // Assert
        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<List<ShipperOutputModel>>>(result =>
                {
                    result.Data!.Count.Should().Be(3);
                    result.Data.Should().Contain(s => s.CompanyName == "Company A");
                    result.Data.Should().Contain(s => s.CompanyName == "Company B");
                    result.Data.Should().Contain(s => s.CompanyName == "Company C");
                });
    }
}
