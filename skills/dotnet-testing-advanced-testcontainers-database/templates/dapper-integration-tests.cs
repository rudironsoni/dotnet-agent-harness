// Dapper Integration Test Template
// Used for containerized testing of Dapper data access layer
// Includes advanced features like QueryMultiple, DynamicParameters, stored procedure calls

using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using Xunit.Abstractions;

namespace YourNamespace.Tests.Dapper;

// ===== Dapper CRUD Test Class =====

/// <summary>
/// Dapper Repository CRUD operation tests
/// </summary>
[Collection(nameof(SqlServerCollectionFixture))]
public class DapperCrudTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IProductRepository _productRepository;
    private readonly ITestOutputHelper _testOutputHelper;

    public DapperCrudTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var connectionString = SqlServerContainerFixture.ConnectionString;
        
        // Create database connection
        _connection = new SqlConnection(connectionString);
        _connection.Open();

        // Create Repository instance
        _productRepository = new DapperProductRepository(connectionString);

        // Ensure tables exist
        EnsureTablesExist();
        
        // Create test categories
        SeedCategories();
    }

    public void Dispose()
    {
        // Clean up data in foreign key constraint order
        _connection.Execute("DELETE FROM ProductTags");
        _connection.Execute("DELETE FROM OrderItems");
        _connection.Execute("DELETE FROM Orders");
        _connection.Execute("DELETE FROM Products");
        _connection.Execute("DELETE FROM Categories");
        _connection.Execute("DELETE FROM Tags");
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Ensure tables exist using SQL script externalization strategy
    /// </summary>
    private void EnsureTablesExist()
    {
        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
        if (!Directory.Exists(scriptDirectory)) return;

        var orderedScripts = new[]
        {
            "Tables/CreateCategoriesTable.sql",
            "Tables/CreateTagsTable.sql",
            "Tables/CreateProductsTable.sql",
            "Tables/CreateOrdersTable.sql",
            "Tables/CreateOrderItemsTable.sql",
            "Tables/CreateProductTagsTable.sql"
        };

        foreach (var scriptPath in orderedScripts)
        {
            var fullPath = Path.Combine(scriptDirectory, scriptPath);
            if (File.Exists(fullPath))
            {
                var script = File.ReadAllText(fullPath);
                _connection.Execute(script);
            }
        }
    }

    private void SeedCategories()
    {
        var count = _connection.QuerySingle<int>("SELECT COUNT(*) FROM Categories");
        if (count == 0)
        {
            _connection.Execute(@"
                INSERT INTO Categories (Name, Description, IsActive) 
                VALUES ('Electronics', 'Various electronic devices', 1), ('Books', 'Various books', 1)");
        }
    }

    // ===== CRUD test methods =====

    [Fact]
    public async Task AddAsync_WithValidProduct_ShouldPersistToDatabase()
    {
        // Arrange
        var categoryId = await _connection.QuerySingleAsync<int>(
            "SELECT TOP 1 Id FROM Categories WHERE IsActive = 1");
        
        var product = new Product
        {
            Name = "Dapper Test Product",
            Description = "Dapper test use",
            Price = 1500,
            Stock = 25,
            CategoryId = categoryId,
            SKU = "DAPPER-001",
            IsActive = true
        };

        // Act
        await _productRepository.AddAsync(product);

        // Assert
        product.Id.Should().BeGreaterThan(0);
        
        var saved = await _connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", new { product.Id });
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Dapper Test Product");
        
        _testOutputHelper.WriteLine($"Successfully created product, ID: {product.Id}");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var categoryId = await _connection.QuerySingleAsync<int>(
            "SELECT TOP 1 Id FROM Categories WHERE IsActive = 1");
        
        var productId = await _connection.QuerySingleAsync<int>(@"
            INSERT INTO Products (Name, Price, Stock, CategoryId, SKU, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES ('Query Test Product', 999, 10, @CategoryId, 'GET-001', 1, GETUTCDATE())",
            new { CategoryId = categoryId });

        // Act
        var result = await _productRepository.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Query Test Product");
    }

    [Fact]
    public async Task UpdateAsync_WithValidProduct_ShouldUpdateDatabase()
    {
        // Arrange
        var categoryId = await _connection.QuerySingleAsync<int>(
            "SELECT TOP 1 Id FROM Categories WHERE IsActive = 1");
        
        var productId = await _connection.QuerySingleAsync<int>(@"
            INSERT INTO Products (Name, Price, Stock, CategoryId, SKU, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES ('Update Test Product', 500, 10, @CategoryId, 'UPDATE-001', 1, GETUTCDATE())",
            new { CategoryId = categoryId });

        var product = await _productRepository.GetByIdAsync(productId);
        product!.Price = 800;

        // Act
        await _productRepository.UpdateAsync(product);

        // Assert
        var updated = await _productRepository.GetByIdAsync(productId);
        updated!.Price.Should().Be(800);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveFromDatabase()
    {
        // Arrange
        var categoryId = await _connection.QuerySingleAsync<int>(
            "SELECT TOP 1 Id FROM Categories WHERE IsActive = 1");
        
        var productId = await _connection.QuerySingleAsync<int>(@"
            INSERT INTO Products (Name, Price, Stock, CategoryId, SKU, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES ('Delete Test Product', 300, 5, @CategoryId, 'DELETE-001', 1, GETUTCDATE())",
            new { CategoryId = categoryId });

        // Act
        await _productRepository.DeleteAsync(productId);

        // Assert
        var deleted = await _productRepository.GetByIdAsync(productId);
        deleted.Should().BeNull();
    }
}

// ===== Dapper Advanced Features Test Class =====

/// <summary>
/// Dapper advanced features tests: QueryMultiple, DynamicParameters, stored procedures
/// </summary>
[Collection(nameof(SqlServerCollectionFixture))]
public class DapperAdvancedTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IProductByDapperRepository _advancedRepository;
    private readonly ITestOutputHelper _testOutputHelper;

    public DapperAdvancedTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var connectionString = SqlServerContainerFixture.ConnectionString;
        
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        
        _advancedRepository = new DapperProductRepository(connectionString);
        
        EnsureDatabaseObjectsExist();
    }

    private void EnsureDatabaseObjectsExist()
    {
        // Create tables and stored procedures
        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
        if (!Directory.Exists(scriptDirectory)) return;

        // Load table scripts
        var tableScripts = new[]
        {
            "Tables/CreateCategoriesTable.sql",
            "Tables/CreateTagsTable.sql",
            "Tables/CreateProductsTable.sql",
            "Tables/CreateProductTagsTable.sql"
        };

        foreach (var scriptPath in tableScripts)
        {
            var fullPath = Path.Combine(scriptDirectory, scriptPath);
            if (File.Exists(fullPath))
            {
                var script = File.ReadAllText(fullPath);
                _connection.Execute(script);
            }
        }

        // Load stored procedure scripts
        var spPath = Path.Combine(scriptDirectory, "StoredProcedures/GetProductSalesReport.sql");
        if (File.Exists(spPath))
        {
            var script = File.ReadAllText(spPath);
            _connection.Execute(script);
        }
    }

    public void Dispose()
    {
        _connection.Execute("DELETE FROM ProductTags");
        _connection.Execute("DELETE FROM Products");
        _connection.Execute("DELETE FROM Categories");
        _connection.Execute("DELETE FROM Tags");
        _connection.Close();
        _connection.Dispose();
    }

    // ===== QueryMultiple Tests =====

    [Fact]
    public async Task GetProductWithTagsAsync_UsingQueryMultiple_ShouldLoadRelatedData()
    {
        // Arrange
        await SeedProductWithTagsAsync();
        var productId = await _connection.QuerySingleAsync<int>(
            "SELECT TOP 1 Id FROM Products");

        // Act
        var product = await _advancedRepository.GetProductWithTagsAsync(productId);

        // Assert
        product.Should().NotBeNull();
        product!.Tags.Should().NotBeEmpty();
        
        _testOutputHelper.WriteLine($"Product: {product.Name}, Tag count: {product.Tags.Count}");
    }

    // ===== DynamicParameters Tests =====

    [Fact]
    public async Task SearchProductsAsync_WithDynamicParameters_ShouldFilterCorrectly()
    {
        // Arrange
        await SeedMultipleProductsAsync();

        // Act - Test different parameter combinations
        var allProducts = await _advancedRepository.SearchProductsAsync();
        var byCategory = await _advancedRepository.SearchProductsAsync(categoryId: 1);
        var byMinPrice = await _advancedRepository.SearchProductsAsync(minPrice: 500m);
        var activeOnly = await _advancedRepository.SearchProductsAsync(isActive: true);

        // Assert
        allProducts.Should().NotBeEmpty();
        byCategory.All(p => p.CategoryId == 1).Should().BeTrue();
        byMinPrice.All(p => p.Price >= 500m).Should().BeTrue();
        activeOnly.All(p => p.IsActive).Should().BeTrue();
        
        _testOutputHelper.WriteLine($"Total: {allProducts.Count()}, Category 1: {byCategory.Count()}");
    }

    // ===== Stored Procedure Tests =====

    [Fact]
    public async Task GetProductSalesReportAsync_CallingStoredProcedure_ShouldReturnReport()
    {
        // Arrange
        await SeedProductsWithSalesDataAsync();

        // Act
        var report = await _advancedRepository.GetProductSalesReportAsync(minPrice: 100m);

        // Assert
        report.Should().NotBeEmpty();
        
        foreach (var item in report)
        {
            _testOutputHelper.WriteLine(
                $"Product: {item.ProductName}, Total Sales: {item.TotalSales:C}");
        }
    }

    private async Task SeedProductWithTagsAsync()
    {
        // Implement test data creation logic
        await _connection.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Categories) 
            INSERT INTO Categories (Name, IsActive) VALUES ('Test Category', 1)");
        
        await _connection.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Tags) 
            INSERT INTO Tags (Name) VALUES ('Tag1'), ('Tag2')");
        
        await _connection.ExecuteAsync(@"
            IF NOT EXISTS (SELECT 1 FROM Products)
            BEGIN
                DECLARE @CategoryId INT = (SELECT TOP 1 Id FROM Categories)
                INSERT INTO Products (Name, Price, Stock, CategoryId, SKU, IsActive, CreatedAt)
                VALUES ('Test Product', 1000, 10, @CategoryId, 'TEST-001', 1, GETUTCDATE())
                
                DECLARE @ProductId INT = SCOPE_IDENTITY()
                INSERT INTO ProductTags (ProductId, TagId)
                SELECT @ProductId, Id FROM Tags
            END");
    }

    private async Task SeedMultipleProductsAsync()
    {
        // Implement test data creation logic
    }

    private async Task SeedProductsWithSalesDataAsync()
    {
        // Implement sales data creation logic
    }
}

// ===== Dapper Repository Implementation Example =====

public class DapperProductRepository : IProductRepository, IProductByDapperRepository
{
    private readonly string _connectionString;

    public DapperProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    // ===== IProductRepository Implementation =====

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<Product>("SELECT * FROM Products");
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", new { Id = id });
    }

    public async Task AddAsync(Product product)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO Products (Name, Description, Price, Stock, CategoryId, SKU, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @SKU, @IsActive, GETUTCDATE())";
        
        product.Id = await connection.QuerySingleAsync<int>(sql, product);
    }

    public async Task UpdateAsync(Product product)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE Products 
            SET Name = @Name, Description = @Description, Price = @Price, 
                Stock = @Stock, CategoryId = @CategoryId, IsActive = @IsActive,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, product);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { Id = id });
    }

    // ===== IProductByDapperRepository Implementation =====

    /// <summary>
    /// Use QueryMultiple to load product and its tags
    /// </summary>
    public async Task<Product?> GetProductWithTagsAsync(int productId)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT * FROM Products WHERE Id = @ProductId;
            SELECT t.* FROM Tags t
            INNER JOIN ProductTags pt ON t.Id = pt.TagId
            WHERE pt.ProductId = @ProductId;";

        using var multi = await connection.QueryMultipleAsync(sql, new { ProductId = productId });
        
        var product = await multi.ReadSingleOrDefaultAsync<Product>();
        if (product != null)
        {
            product.Tags = (await multi.ReadAsync<Tag>()).ToList();
        }
        return product;
    }

    /// <summary>
    /// Use DynamicParameters to build dynamic queries
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(
        int? categoryId = null,
        decimal? minPrice = null,
        bool? isActive = null)
    {
        using var connection = CreateConnection();
        
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

        return await connection.QueryAsync<Product>(sql.ToString(), parameters);
    }

    /// <summary>
    /// Call stored procedure to get sales report
    /// </summary>
    public async Task<IEnumerable<ProductSalesReport>> GetProductSalesReportAsync(decimal minPrice)
    {
        using var connection = CreateConnection();
        
        return await connection.QueryAsync<ProductSalesReport>(
            "GetProductSalesReport",
            new { MinPrice = minPrice },
            commandType: CommandType.StoredProcedure);
    }
}

// ===== Interface and Model Definitions =====

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

public interface IProductByDapperRepository
{
    Task<Product?> GetProductWithTagsAsync(int productId);
    Task<IEnumerable<Product>> SearchProductsAsync(int? categoryId = null, decimal? minPrice = null, bool? isActive = null);
    Task<IEnumerable<ProductSalesReport>> GetProductSalesReportAsync(decimal minPrice);
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Tag> Tags { get; set; } = new();
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductSalesReport
{
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TotalQuantity { get; set; }
}

// Assume SqlServerCollectionFixture is defined in another file
// public class SqlServerContainerFixture : IAsyncLifetime { ... }
// [CollectionDefinition(nameof(SqlServerCollectionFixture))]
// public class SqlServerCollectionFixture : ICollectionFixture<SqlServerContainerFixture> { }
