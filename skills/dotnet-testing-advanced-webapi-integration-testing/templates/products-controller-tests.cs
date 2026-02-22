using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using YourProject.Application.DTOs;
using YourProject.Tests.Integration.Fixtures;

namespace YourProject.Tests.Integration.Controllers;

/// <summary>
/// ProductsController integration tests
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Create Product Tests

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "New Product",
            Price = 299.99m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", request);

        // Assert
        response.Should().Be201Created()
            .And.Satisfy<ProductResponse>(product =>
            {
                product.Id.Should().NotBeEmpty();
                product.Name.Should().Be("New Product");
                product.Price.Should().Be(299.99m);
                product.CreatedAt.Should().Be(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
                product.UpdatedAt.Should().Be(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
            });
    }

    [Fact]
    public async Task CreateProduct_WhenProductNameIsEmpty_ShouldReturn400BadRequest()
    {
        // Arrange
        var invalidRequest = new ProductCreateRequest
        {
            Name = "",
            Price = 100.00m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

        // Assert
        response.Should().Be400BadRequest()
            .And.Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
                problem.Title.Should().Be("One or more validation errors occurred.");
                problem.Status.Should().Be(400);
                problem.Errors.Should().ContainKey("Name");
                problem.Errors["Name"].Should().Contain("Product name cannot be empty");
            });
    }

    [Fact]
    public async Task CreateProduct_WhenPriceIsLessThanZero_ShouldReturn400BadRequest()
    {
        // Arrange
        var invalidRequest = new ProductCreateRequest
        {
            Name = "Test Product",
            Price = -10.00m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

        // Assert
        response.Should().Be400BadRequest()
            .And.Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Errors.Should().ContainKey("Price");
                problem.Errors["Price"].Should().Contain("Product price must be greater than 0");
            });
    }

    [Fact]
    public async Task CreateProduct_WhenMultipleFieldsAreInvalid_ShouldReturnAllValidationErrors()
    {
        // Arrange
        var invalidRequest = new ProductCreateRequest
        {
            Name = "",
            Price = -10.00m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", invalidRequest);

        // Assert
        response.Should().Be400BadRequest()
            .And.Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Errors.Should().ContainKey("Name");
                problem.Errors.Should().ContainKey("Price");
            });
    }

    #endregion

    #region Query Product Tests

    [Fact]
    public async Task GetById_WhenProductExists_ShouldReturnProductData()
    {
        // Arrange
        var productId = await DatabaseManager.SeedProductAsync("Test Product", 199.99m);

        // Act
        var response = await HttpClient.GetAsync($"/products/{productId}");

        // Assert
        response.Should().Be200Ok()
            .And.Satisfy<ProductResponse>(product =>
            {
                product.Id.Should().Be(productId);
                product.Name.Should().Be("Test Product");
                product.Price.Should().Be(199.99m);
            });
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ShouldReturn404WithProblemDetails()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/products/{nonExistentId}");

        // Assert
        response.Should().Be404NotFound();

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

        problemDetails.GetProperty("type").GetString().Should().Be("https://httpstatuses.com/404");
        problemDetails.GetProperty("title").GetString().Should().Be("Product does not exist");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain($"Could not find product with ID {nonExistentId}");
    }

    #endregion

    #region Pagination Query Tests

    [Fact]
    public async Task GetProducts_WithPaginationParameters_ShouldReturnCorrectPaginationResults()
    {
        // Arrange
        await DatabaseManager.SeedProductsAsync(15);

        // Act - Use Flurl to build QueryString
        var url = "/products"
            .SetQueryParam("pageSize", 5)
            .SetQueryParam("page", 2);

        var response = await HttpClient.GetAsync(url);

        // Assert
        response.Should().Be200Ok()
            .And.Satisfy<PagedResult<ProductResponse>>(result =>
            {
                result.Total.Should().Be(15);
                result.PageSize.Should().Be(5);
                result.Page.Should().Be(2);
                result.PageCount.Should().Be(3);
                result.Items.Should().HaveCount(5);
                result.Items.Should().AllSatisfy(product =>
                {
                    product.Id.Should().NotBeEmpty();
                    product.Name.Should().NotBeNullOrEmpty();
                    product.Price.Should().BeGreaterThan(0);
                });
            });
    }

    [Fact]
    public async Task GetProducts_WithSearchParameters_ShouldReturnMatchingProducts()
    {
        // Arrange
        await DatabaseManager.SeedProductsAsync(5);
        await DatabaseManager.SeedProductAsync("Special Product", 199.99m);

        // Act - Use Flurl to build complex query
        var url = "/products"
            .SetQueryParam("keyword", "Special")
            .SetQueryParam("pageSize", 10);

        var response = await HttpClient.GetAsync(url);

        // Assert
        response.Should().Be200Ok()
            .And.Satisfy<PagedResult<ProductResponse>>(result =>
            {
                result.Total.Should().Be(1);
                result.Items.Should().HaveCount(1);

                var product = result.Items.First();
                product.Name.Should().Be("Special Product");
                product.Price.Should().Be(199.99m);
            });
    }

    #endregion

    #region Update Product Tests

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var productId = await DatabaseManager.SeedProductAsync("Original Product", 100.00m);
        var updateRequest = new ProductUpdateRequest
        {
            Name = "Updated Product",
            Price = 299.99m
        };

        // Advance time by 1 hour
        AdvanceTime(TimeSpan.FromHours(1));

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/products/{productId}", updateRequest);

        // Assert
        response.Should().Be200Ok()
            .And.Satisfy<ProductResponse>(product =>
            {
                product.Name.Should().Be("Updated Product");
                product.Price.Should().Be(299.99m);
            });
    }

    #endregion

    #region Delete Product Tests

    [Fact]
    public async Task DeleteProduct_WhenProductExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        var productId = await DatabaseManager.SeedProductAsync("Product to Delete", 100.00m);

        // Act
        var response = await HttpClient.DeleteAsync($"/products/{productId}");

        // Assert
        response.Should().Be204NoContent();

        // Verify product has been deleted
        var getResponse = await HttpClient.GetAsync($"/products/{productId}");
        getResponse.Should().Be404NotFound();
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/products/{nonExistentId}");

        // Assert
        response.Should().Be404NotFound();
    }

    #endregion
}

#region DTO Classes (Examples)

public class ProductCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class PagedResult<T>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}

#endregion
