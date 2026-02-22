# ORM Advanced Feature Testing

> This document is extracted from SKILL.md and contains advanced testing examples for EF Core and Dapper.

## EF Core Advanced Feature Testing

### Include/ThenInclude Multi-Level Relationship Queries

```csharp
[Fact]
public async Task GetProductWithCategoryAndTagsAsync_LoadCompleteRelatedData_ShouldLoadCorrectly()
{
    // Arrange
    await CreateProductWithCategoryAndTagsAsync();

    // Act
    var product = await _repository.GetProductWithCategoryAndTagsAsync(1);

    // Assert
    product.Should().NotBeNull();
    product!.Category.Should().NotBeNull();
    product.ProductTags.Should().NotBeEmpty();
}
```

### AsSplitQuery Avoiding Cartesian Product

```csharp
[Fact]
public async Task GetProductsByCategoryWithSplitQueryAsync_UsingSplitQuery_ShouldAvoidCartesianProduct()
{
    // Arrange
    await CreateMultipleProductsWithTagsAsync();

    // Act
    var products = await _repository.GetProductsByCategoryWithSplitQueryAsync(1);

    // Assert
    products.Should().NotBeEmpty();
    products.All(p => p.ProductTags.Any()).Should().BeTrue();
}
```

> **Cartesian Product Problem**: When a query JOINs multiple one-to-many relationships, it produces one row for each possible combination. `AsSplitQuery()` breaks the query into multiple independent SQL queries, combining results in memory to avoid this problem.

### N+1 Query Problem Verification

```csharp
[Fact]
public async Task N1QueryProblemVerification_CompareRepositoryMethods_ShouldDemonstrateEfficiencyDifference()
{
    // Arrange
    await CreateCategoriesWithProductsAsync();

    // Act 1: Test problematic method
    var categoriesWithProblem = await _repository.GetCategoriesWithN1ProblemAsync();

    // Act 2: Test optimized method
    var categoriesOptimized = await _repository.GetCategoriesWithProductsOptimizedAsync();

    // Assert
    categoriesOptimized.All(c => c.Products.Any()).Should().BeTrue();
}
```

### AsNoTracking Read-Only Query Optimization

```csharp
[Fact]
public async Task GetProductsWithNoTrackingAsync_ReadOnlyQuery_ShouldNotTrackEntities()
{
    // Arrange
    await CreateMultipleProductsAsync();

    // Act
    var products = await _repository.GetProductsWithNoTrackingAsync(500m);

    // Assert
    products.Should().NotBeEmpty();
    var trackedEntities = _dbContext.ChangeTracker.Entries<Product>().Count();
    trackedEntities.Should().Be(0, "AsNoTracking query should not track entities");
}
```

## Dapper Advanced Feature Testing

### Basic CRUD Testing

```csharp
[Collection(nameof(SqlServerCollectionFixture))]
public class DapperCrudTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IProductRepository _productRepository;

    public DapperCrudTests()
    {
        var connectionString = SqlServerContainerFixture.ConnectionString;
        _connection = new SqlConnection(connectionString);
        _connection.Open();

        _productRepository = new DapperProductRepository(connectionString);
        EnsureTablesExist();
    }

    public void Dispose()
    {
        _connection.Execute("DELETE FROM Products");
        _connection.Execute("DELETE FROM Categories");
        _connection.Close();
        _connection.Dispose();
    }
}
```

### QueryMultiple One-to-Many Relationship Handling

```csharp
public async Task<Product?> GetProductWithTagsAsync(int productId)
{
    const string sql = @"
        SELECT * FROM Products WHERE Id = @ProductId;
        SELECT t.* FROM Tags t
        INNER JOIN ProductTags pt ON t.Id = pt.TagId
        WHERE pt.ProductId = @ProductId;";

    using var multi = await _connection.QueryMultipleAsync(sql, new { ProductId = productId });
    var product = await multi.ReadSingleOrDefaultAsync<Product>();
    if (product != null)
    {
        product.Tags = (await multi.ReadAsync<Tag>()).ToList();
    }
    return product;
}
```

### DynamicParameters Dynamic Queries

```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(
    int? categoryId = null,
    decimal? minPrice = null,
    bool? isActive = null)
{
    var sql = new StringBuilder("SELECT * FROM Products WHERE 1=1");
    var parameters = new DynamicParameters();

    if (categoryId.HasValue)
    {
        sql.Append(" AND CategoryId = @CategoryId");
        parameters.Add("CategoryId", categoryId.Value);
    }

    if (minPrice.HasValue)
    {
        sql.Append(" AND Price >= @MinPrice");
        parameters.Add("MinPrice", minPrice.Value);
    }

    if (isActive.HasValue)
    {
        sql.Append(" AND IsActive = @IsActive");
        parameters.Add("IsActive", isActive.Value);
    }

    return await _connection.QueryAsync<Product>(sql.ToString(), parameters);
}
```
