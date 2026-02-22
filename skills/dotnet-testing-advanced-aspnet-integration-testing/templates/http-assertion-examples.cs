// =============================================================================
// ASP.NET Core Integration Testing - HTTP Response Assertion Examples
// =============================================================================
// Purpose: Demonstrates how to use AwesomeAssertions.Web for HTTP response verification
// Package requirement: AwesomeAssertions.Web (used with AwesomeAssertions)
// =============================================================================

using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Web;
using Microsoft.AspNetCore.Mvc;

namespace YourProject.IntegrationTests.Examples;

/// <summary>
/// HTTP response assertion examples
/// Demonstrates various assertion methods from AwesomeAssertions.Web
/// </summary>
public class HttpAssertionExamples : IntegrationTestBase
{
    // ========================================
    // HTTP status code assertions
    // ========================================

    [Fact]
    public async Task StatusCodeAssertion_200OK()
    {
        var response = await Client.GetAsync("/api/shippers");
        
        // Verify HTTP 200 OK
        response.Should().Be200Ok();
    }

    [Fact]
    public async Task StatusCodeAssertion_201Created()
    {
        var request = new ShipperCreateParameter
        {
            CompanyName = "New Company",
            Phone = "02-1234-5678"
        };
        
        var response = await Client.PostAsJsonAsync("/api/shippers", request);
        
        // Verify HTTP 201 Created
        response.Should().Be201Created();
    }

    [Fact]
    public async Task StatusCodeAssertion_204NoContent()
    {
        var shipperId = await SeedShipperAsync("Company to Delete");
        
        var response = await Client.DeleteAsync($"/api/shippers/{shipperId}");
        
        // Verify HTTP 204 No Content
        response.Should().Be204NoContent();
    }

    [Fact]
    public async Task StatusCodeAssertion_400BadRequest()
    {
        var invalidRequest = new ShipperCreateParameter
        {
            CompanyName = "", // Empty value, should fail validation
            Phone = ""
        };
        
        var response = await Client.PostAsJsonAsync("/api/shippers", invalidRequest);
        
        // Verify HTTP 400 Bad Request
        response.Should().Be400BadRequest();
    }

    [Fact]
    public async Task StatusCodeAssertion_404NotFound()
    {
        var response = await Client.GetAsync("/api/shippers/99999");
        
        // Verify HTTP 404 Not Found
        response.Should().Be404NotFound();
    }

    // ========================================
    // Satisfy<T> strongly-typed validation
    // ========================================

    [Fact]
    public async Task Satisfy_ValidateSuccessResponseContent()
    {
        await CleanupDatabaseAsync();
        var shipperId = await SeedShipperAsync("Test Company", "02-9876-5432");

        var response = await Client.GetAsync($"/api/shippers/{shipperId}");

        // Use Satisfy<T> for strongly-typed validation
        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                {
                    // Verify overall structure
                    result.Status.Should().Be("Success");
                    result.Data.Should().NotBeNull();
                    
                    // Verify data content
                    result.Data!.ShipperId.Should().Be(shipperId);
                    result.Data.CompanyName.Should().Be("Test Company");
                    result.Data.Phone.Should().Be("02-9876-5432");
                });
    }

    [Fact]
    public async Task Satisfy_ValidateCollectionResponse()
    {
        await CleanupDatabaseAsync();
        await SeedShipperAsync("Company A", "02-1111-1111");
        await SeedShipperAsync("Company B", "02-2222-2222");

        var response = await Client.GetAsync("/api/shippers");

        response.Should().Be200Ok()
                .And
                .Satisfy<SuccessResultOutputModel<List<ShipperOutputModel>>>(result =>
                {
                    result.Data!.Count.Should().Be(2);
                    result.Data.Should().Contain(s => s.CompanyName == "Company A");
                    result.Data.Should().Contain(s => s.CompanyName == "Company B");
                    
                    // Verify order (if sorting is required)
                    result.Data.Should().BeInAscendingOrder(s => s.CompanyName);
                });
    }

    [Fact]
    public async Task Satisfy_ValidateErrorResponseDetails()
    {
        var invalidRequest = new ShipperCreateParameter
        {
            CompanyName = "",
            Phone = "02-1234-5678"
        };

        var response = await Client.PostAsJsonAsync("/api/shippers", invalidRequest);

        response.Should().Be400BadRequest()
                .And
                .Satisfy<ValidationProblemDetails>(problem =>
                {
                    problem.Status.Should().Be(400);
                    problem.Title.Should().Contain("validation");
                    problem.Errors.Should().ContainKey("CompanyName");
                });
    }

    // ========================================
    // BeAs anonymous type validation
    // ========================================

    [Fact]
    public async Task BeAs_ValidateUsingAnonymousType()
    {
        await CleanupDatabaseAsync();
        var shipperId = await SeedShipperAsync("Anonymous Validation Company", "02-5555-5555");

        var response = await Client.GetAsync($"/api/shippers/{shipperId}");

        // Use anonymous type for quick validation
        response.Should().Be200Ok()
                .And
                .BeAs(new
                {
                    Status = "Success",
                    Data = new
                    {
                        CompanyName = "Anonymous Validation Company",
                        Phone = "02-5555-5555"
                    }
                });
    }

    // ========================================
    // Combined assertions
    // ========================================

    [Fact]
    public async Task CombinedAssertions_FullCRUDFlowValidation()
    {
        await CleanupDatabaseAsync();

        // Create
        var createRequest = new ShipperCreateParameter
        {
            CompanyName = "CRUD Test Company",
            Phone = "02-1234-5678"
        };
        
        var createResponse = await Client.PostAsJsonAsync("/api/shippers", createRequest);
        createResponse.Should().Be201Created();
        
        var created = await createResponse.Content
            .ReadFromJsonAsync<SuccessResultOutputModel<ShipperOutputModel>>();
        var shipperId = created!.Data!.ShipperId;

        // Read
        var readResponse = await Client.GetAsync($"/api/shippers/{shipperId}");
        readResponse.Should().Be200Ok()
                   .And
                   .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                   {
                       result.Data!.CompanyName.Should().Be("CRUD Test Company");
                   });

        // Update
        var updateRequest = new ShipperCreateParameter
        {
            CompanyName = "Updated Company Name",
            Phone = "02-8765-4321"
        };
        
        var updateResponse = await Client.PutAsJsonAsync($"/api/shippers/{shipperId}", updateRequest);
        updateResponse.Should().Be200Ok()
                     .And
                     .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
                     {
                         result.Data!.CompanyName.Should().Be("Updated Company Name");
                         result.Data.Phone.Should().Be("02-8765-4321");
                     });

        // Delete
        var deleteResponse = await Client.DeleteAsync($"/api/shippers/{shipperId}");
        deleteResponse.Should().Be204NoContent();

        // Verify deletion
        var verifyResponse = await Client.GetAsync($"/api/shippers/{shipperId}");
        verifyResponse.Should().Be404NotFound();
    }
}

// =============================================================================
// DTO model examples (adjust according to project requirements)
// =============================================================================

/// <summary>
/// Success response model
/// </summary>
public class SuccessResultOutputModel<T>
{
    public string Status { get; set; } = "Success";
    public T? Data { get; set; }
}

/// <summary>
/// Shipper output model
/// </summary>
public class ShipperOutputModel
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Shipper creation parameter
/// </summary>
public class ShipperCreateParameter
{
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
