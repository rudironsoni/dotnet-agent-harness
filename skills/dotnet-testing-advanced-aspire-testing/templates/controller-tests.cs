using AwesomeAssertions;
using Flurl;

namespace MyApp.Tests.Integration.Controllers;

/// <summary>
/// ProductsController integration tests - using Aspire Testing
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(AspireAppFixture fixture) : base(fixture)
    {
    }

    #region GET /products Tests

    [Fact]
    public async Task GetProducts_WhenNoProducts_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        // Database should be empty (cleaned before each test)

        // Act
        var response = await HttpClient.GetAsync("/products");

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<PagedResult<ProductResponse>>(result =>
                {
                    result.Total.Should().Be(0);
                    result.PageSize.Should().Be(10);
                    result.Page.Should().Be(1);
                    result.Items.Should().BeEmpty();
                });
    }

    [Fact]
    public async Task GetProducts_UsingPaginationParameters_ShouldReturnCorrectPagedResult()
    {
        // Arrange
        await TestHelpers.SeedProductsAsync(DatabaseManager, 15);

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
                });
    }

    [Fact]
    public async Task GetProducts_UsingSearchParameter_ShouldReturnMatchingProducts()
    {
        // Arrange
        await TestHelpers.SeedProductsAsync(DatabaseManager, 5);
        await TestHelpers.SeedSpecificProductAsync(DatabaseManager, "Special Product", 199.99m);

        // Act - Use Flurl to build QueryString
        var url = "/products"
                  .SetQueryParam("keyword", "Special");

        var response = await HttpClient.GetAsync(url);

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<PagedResult<ProductResponse>>(result =>
                {
                    result.Total.Should().Be(1);
                    result.Items.Should().HaveCount(1);
                    result.Items.First().Name.Should().Be("Special Product");
                    result.Items.First().Price.Should().Be(199.99m);
                });
    }

    #endregion

    #region POST /products Tests

    [Fact]
    public async Task CreateProduct_ValidRequest_ShouldReturn201WithProductInfo()
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
                    product.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
                });
    }

    [Fact]
    public async Task CreateProduct_InvalidRequest_ShouldReturn400WithValidationErrors()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "", // Name cannot be empty
            Price = -100m // Price cannot be negative
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", request);

        // Assert
        response.Should().Be400BadRequest()
                .And.Satisfy<ValidationProblemDetails>(problem =>
                {
                    problem.Errors.Should().ContainKey("Name");
                    problem.Errors.Should().ContainKey("Price");
                });
    }

    #endregion

    #region GET /products/{id} Tests

    [Fact]
    public async Task GetProductById_ExistingProduct_ShouldReturn200WithProductInfo()
    {
        // Arrange
        var productId = await TestHelpers.SeedSpecificProductAsync(
            DatabaseManager, "Test Product", 199.99m);

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
    public async Task GetProductById_NonExistingProduct_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/products/{nonExistentId}");

        // Assert
        response.Should().Be404NotFound();
    }

    #endregion

    #region PUT /products/{id} Tests

    [Fact]
    public async Task UpdateProduct_ExistingProduct_ShouldReturn200WithUpdatedProduct()
    {
        // Arrange
        var productId = await TestHelpers.SeedSpecificProductAsync(
            DatabaseManager, "Original Product", 100m);

        var updateRequest = new ProductUpdateRequest
        {
            Name = "Updated Product",
            Price = 150m
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/products/{productId}", updateRequest);

        // Assert
        response.Should().Be200Ok()
                .And.Satisfy<ProductResponse>(product =>
                {
                    product.Id.Should().Be(productId);
                    product.Name.Should().Be("Updated Product");
                    product.Price.Should().Be(150m);
                    product.UpdatedAt.Should().BeAfter(product.CreatedAt);
                });
    }

    #endregion

    #region DELETE /products/{id} Tests

    [Fact]
    public async Task DeleteProduct_ExistingProduct_ShouldReturn204()
    {
        // Arrange
        var productId = await TestHelpers.SeedSpecificProductAsync(
            DatabaseManager, "Product To Delete", 99.99m);

        // Act
        var response = await HttpClient.DeleteAsync($"/products/{productId}");

        // Assert
        response.Should().Be204NoContent();

        // Verify product was deleted
        var getResponse = await HttpClient.GetAsync($"/products/{productId}");
        getResponse.Should().Be404NotFound();
    }

    #endregion
}

#region Test DTO Classes

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

public class ValidationProblemDetails
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

#endregion
