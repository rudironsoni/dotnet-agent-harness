// MSSQL Collection Fixture Template
// Used for multiple test classes sharing the same SQL Server container
// Significantly improves test execution efficiency (reduces execution time by approximately 67%)

using Testcontainers.MsSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace YourNamespace.Tests.Fixtures;

// ===== Step 1: Create Container Fixture =====

/// <summary>
/// SQL Server container Collection Fixture
/// Used to share the same container instance across multiple test classes
/// </summary>
/// <remarks>
/// Performance improvement:
/// - Traditional approach: each test class starts a container, if 3 classes = 30 seconds
/// - Collection Fixture: all classes share the same container = 10 seconds
/// - Execution time reduced by approximately 67%
/// </remarks>
public class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public SqlServerContainerFixture()
    {
        _container = new MsSqlBuilder()
            // Use latest SQL Server 2022 image
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            // Set strong password (SQL Server password requirements: uppercase, lowercase, numbers, special characters)
            .WithPassword("Test123456!")
            // Automatically clean up container after test completion
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// Static connection string, accessible by all test classes
    /// </summary>
    public static string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Initialize container
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // Wait for container to fully start (SQL Server requires more time)
        await Task.Delay(2000);

        Console.WriteLine($"SQL Server container started, connection string: {ConnectionString}");
    }

    /// <summary>
    /// Clean up container
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        Console.WriteLine("SQL Server container cleaned up");
    }
}

// ===== Step 2: Define Collection Definition =====

/// <summary>
/// Define test collection so multiple test classes can share the same SqlServerContainerFixture
/// </summary>
/// <remarks>
/// Usage: Add [Collection(nameof(SqlServerCollectionFixture))] to test class
/// </remarks>
[CollectionDefinition(nameof(SqlServerCollectionFixture))]
public class SqlServerCollectionFixture : ICollectionFixture<SqlServerContainerFixture>
{
    // This class is only used to define the Collection, no implementation needed
}

// ===== Step 3: Test Base Class (Optional but recommended) =====

/// <summary>
/// EF Core test base class, provides shared setup and cleanup logic
/// </summary>
public abstract class EfCoreTestBase : IDisposable
{
    protected readonly ECommerceDbContext DbContext;
    protected readonly ITestOutputHelper TestOutputHelper;

    protected EfCoreTestBase(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        var connectionString = SqlServerContainerFixture.ConnectionString;

        TestOutputHelper.WriteLine($"Using connection string: {connectionString}");

        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(connectionString)
            // Enable sensitive data logging (development/test environment only)
            .EnableSensitiveDataLogging()
            // Output SQL logs to test results
            .LogTo(testOutputHelper.WriteLine, LogLevel.Information)
            .Options;

        DbContext = new ECommerceDbContext(options);
        DbContext.Database.EnsureCreated();
        
        // Ensure tables exist (using SQL script externalization strategy)
        EnsureTablesExist();
    }

    /// <summary>
    /// Ensure tables exist, using external SQL scripts to create
    /// </summary>
    protected virtual void EnsureTablesExist()
    {
        var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
        if (!Directory.Exists(scriptDirectory)) return;

        // Execute table creation scripts in dependency order
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
                DbContext.Database.ExecuteSqlRaw(script);
            }
        }
    }

    /// <summary>
    /// Clean up test data, execute DELETE in foreign key constraint order
    /// </summary>
    public virtual void Dispose()
    {
        // Clean up data in foreign key constraint order to ensure test isolation
        DbContext.Database.ExecuteSqlRaw("DELETE FROM ProductTags");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM OrderItems");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Products");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Categories");
        DbContext.Database.ExecuteSqlRaw("DELETE FROM Tags");
        DbContext.Dispose();
    }
}

// ===== Step 4: Actual Test Class Examples =====

/// <summary>
/// EF Core CRUD operation tests
/// </summary>
[Collection(nameof(SqlServerCollectionFixture))]
public class EfCoreCrudTests : EfCoreTestBase
{
    private readonly IProductRepository _productRepository;

    public EfCoreCrudTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _productRepository = new EfCoreProductRepository(DbContext);
    }

    [Fact]
    public async Task AddAsync_WithValidProduct_ShouldPersistToDatabase()
    {
        // Arrange
        await SeedCategoryAsync();
        var category = await DbContext.Categories.FirstAsync();
        var product = new Product
        {
            Name = "Test Product",
            Price = 1500,
            Stock = 25,
            CategoryId = category.Id,
            SKU = "TEST-001",
            IsActive = true
        };

        // Act
        await _productRepository.AddAsync(product);

        // Assert
        product.Id.Should().BeGreaterThan(0);
        var saved = await DbContext.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        await SeedCategoryAsync();
        var category = await DbContext.Categories.FirstAsync();
        var product = new Product
        {
            Name = "Query Test Product",
            Price = 999,
            CategoryId = category.Id,
            SKU = "GET-001",
            IsActive = true
        };
        await _productRepository.AddAsync(product);

        // Act
        var result = await _productRepository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Query Test Product");
    }

    private async Task SeedCategoryAsync()
    {
        if (!await DbContext.Categories.AnyAsync())
        {
            DbContext.Categories.Add(new Category
            {
                Name = "Electronics",
                Description = "Various electronic devices",
                IsActive = true
            });
            await DbContext.SaveChangesAsync();
        }
    }
}

/// <summary>
/// EF Core advanced feature tests
/// </summary>
[Collection(nameof(SqlServerCollectionFixture))]
public class EfCoreAdvancedTests : EfCoreTestBase
{
    private readonly IProductByEFCoreRepository _advancedRepository;

    public EfCoreAdvancedTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _advancedRepository = new EfCoreProductRepository(DbContext);
    }

    [Fact]
    public async Task GetProductWithCategoryAndTagsAsync_ShouldLoadRelatedData()
    {
        // Arrange
        await CreateProductWithCategoryAndTagsAsync();

        // Act
        var product = await _advancedRepository.GetProductWithCategoryAndTagsAsync(1);

        // Assert
        product.Should().NotBeNull();
        product!.Category.Should().NotBeNull();
        product.ProductTags.Should().NotBeEmpty();
        
        TestOutputHelper.WriteLine($"Product: {product.Name}, Category: {product.Category.Name}");
    }

    [Fact]
    public async Task GetProductsWithNoTrackingAsync_ShouldNotTrackEntities()
    {
        // Arrange
        await CreateMultipleProductsAsync();
        var minPrice = 500m;

        // Act
        var products = await _advancedRepository.GetProductsWithNoTrackingAsync(minPrice);

        // Assert
        products.Should().NotBeEmpty();
        
        // Verify these entities are not tracked by ChangeTracker
        var trackedEntities = DbContext.ChangeTracker.Entries<Product>().Count();
        trackedEntities.Should().Be(0, "AsNoTracking queries should not track entities");
    }

    private async Task CreateProductWithCategoryAndTagsAsync()
    {
        // Implement test data creation logic
    }

    private async Task CreateMultipleProductsAsync()
    {
        // Implement test data creation logic
    }
}

// ===== Replace these classes with your actual implementations =====

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}

public class Tag { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class ProductTag { public int Id { get; set; } public int ProductId { get; set; } public int TagId { get; set; } }
public class Order { public int Id { get; set; } }
public class OrderItem { public int Id { get; set; } }

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

public interface IProductByEFCoreRepository
{
    Task<Product?> GetProductWithCategoryAndTagsAsync(int productId);
    Task<IEnumerable<Product>> GetProductsWithNoTrackingAsync(decimal minPrice);
}

public class EfCoreProductRepository : IProductRepository, IProductByEFCoreRepository
{
    private readonly ECommerceDbContext _context;
    public EfCoreProductRepository(ECommerceDbContext context) => _context = context;
    
    public async Task<IEnumerable<Product>> GetAllAsync() => await _context.Products.ToListAsync();
    public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FindAsync(id);
    public async Task AddAsync(Product product) { _context.Products.Add(product); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(Product product) { _context.Products.Update(product); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var entity = await GetByIdAsync(id); if (entity != null) { _context.Products.Remove(entity); await _context.SaveChangesAsync(); } }
    
    public async Task<Product?> GetProductWithCategoryAndTagsAsync(int productId) =>
        await _context.Products.Include(p => p.Category).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == productId);
    
    public async Task<IEnumerable<Product>> GetProductsWithNoTrackingAsync(decimal minPrice) =>
        await _context.Products.AsNoTracking().Where(p => p.Price >= minPrice).ToListAsync();
}
