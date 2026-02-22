# Common Task Mapping Table

### Task 1: Test ASP.NET Core Web API (Basic)

**Scenario**: Create integration tests for simple ProductsController

**Recommended Skill**:
- `dotnet-testing-advanced-aspnet-integration-testing`

**Applicable Conditions**:
- Simple CRUD API
- Don't need real database
- Test basic HTTP endpoints

**Implementation Steps**:
1. Create CustomWebApplicationFactory
2. Configure in-memory database
3. Write GET, POST tests
4. Use FluentAssertions.Web to validate responses

**Prompt Example**:
```
Please use dotnet-testing-advanced-aspnet-integration-testing skill
to create integration tests for my ProductsController. Controller has GetAll and GetById endpoints.
```

**Expected Code Structure**:
```csharp
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.Should().Be200Ok();
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeEmpty();
    }
}
```

---

### Task 2: Test ASP.NET Core Web API (Complete Process)

**Scenario**: Create complete CRUD tests for ShippersController

**Recommended Skill**:
- `dotnet-testing-advanced-webapi-integration-testing`

**Applicable Conditions**:
- Complete CRUD API
- Need to test error handling
- Need test data preparation and cleanup

**Implementation Steps**:
1. Create test base class (BaseIntegrationTest)
2. Implement IAsyncLifetime for data preparation/cleanup
3. Test all CRUD endpoints
4. Validate error handling (404, 400, 409, etc.)

**Prompt Example**:
```
Please use dotnet-testing-advanced-webapi-integration-testing skill
to create complete CRUD tests for my ShippersController. Need to test:
- GET /api/shippers (get all)
- GET /api/shippers/{id} (get single)
- POST /api/shippers (create)
- PUT /api/shippers/{id} (update)
- DELETE /api/shippers/{id} (delete)
And validate error scenarios (like resource not found).
```

**Expected Test Coverage**:
- ✅ GET successfully returns data
- ✅ GET non-existent ID returns 404
- ✅ POST create successful
- ✅ POST invalid data returns 400
- ✅ PUT update successful
- ✅ PUT non-existent ID returns 404
- ✅ DELETE delete successful
- ✅ DELETE non-existent ID returns 404

---

### Task 3: Test Code Requiring Real Database (SQL)

**Scenario**: Test OrderRepository (using SQL Server)

**Recommended Skill**:
- `dotnet-testing-advanced-testcontainers-database`

**Applicable Conditions**:
- Using EF Core or Dapper
- Need to test real database behavior
- Need to test database-specific features

**Implementation Steps**:
1. Configure Testcontainers.MsSql
2. Execute database migrations
3. Test Repository methods
4. Clean up data after each test

**Prompt Example**:
```
Please use dotnet-testing-advanced-testcontainers-database skill
to create tests for my OrderRepository. Repository uses EF Core connecting to SQL Server.
Need to test GetById, Create, Update, Delete methods.
```

**Expected Code Structure**:
```csharp
public class OrderRepositoryTests : IAsyncLifetime
{
    private MsSqlContainer _container;
    private OrderDbContext _context;
    private OrderRepository _sut;

    public async Task InitializeAsync()
    {
        // Start SQL Server container
        _container = new MsSqlBuilder().Build();
        await _container.StartAsync();

        // Create DbContext
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _context = new OrderDbContext(options);
        await _context.Database.MigrateAsync();

        _sut = new OrderRepository(_context);
    }

    [Fact]
    public async Task GetById_ExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = new Order { /* ... */ };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetById(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

---

### Task 4: Test NoSQL Database (MongoDB, Redis)

**Scenario**: Test CacheService (using Redis)

**Recommended Skill**:
- `dotnet-testing-advanced-testcontainers-nosql`

**Applicable Conditions**:
- Using MongoDB, Redis, Elasticsearch
- Need to test NoSQL-specific features

**Implementation Steps**:
1. Configure Testcontainers.Redis
2. Test cache logic
3. Verify expiration time
4. Test cache invalidation

**Prompt Example**:
```
Please use dotnet-testing-advanced-testcontainers-nosql skill
to create tests for my CacheService. Service uses Redis for caching.
Need to test Set, Get, Remove and expiration time.
```

**Expected Test Coverage**:
- ✅ Set successfully stores data
- ✅ Get successfully retrieves data
- ✅ Get non-existent key returns null
- ✅ Expiration time works correctly
- ✅ Remove successfully removes data

---

### Task 5: Test Microservices Architecture (.NET Aspire)

**Scenario**: Test .NET Aspire microservices project

**Recommended Skill**:
- `dotnet-testing-advanced-aspire-testing`

**Applicable Conditions**:
- Using .NET Aspire
- Multi-service collaboration
- Distributed applications

**Implementation Steps**:
1. Create DistributedApplication tests
2. Configure service dependencies
3. Test service-to-service communication
4. Validate complete process

**Prompt Example**:
```
Please use dotnet-testing-advanced-aspire-testing skill
to create tests for my .NET Aspire project. Project includes API Service and Worker Service.
Need to test collaboration between two services.
```

---

### Task 6: Upgrade xUnit to 3.x

**Scenario**: Existing project uses xUnit 2.9.x, want to upgrade to 3.x

**Recommended Skill**:
- `dotnet-testing-advanced-xunit-upgrade-guide`

**Implementation Steps**:
1. Understand breaking changes
2. Update package versions
3. Handle compatibility issues
4. Verify test execution

**Prompt Example**:
```
Please use dotnet-testing-advanced-xunit-upgrade-guide skill
to help me upgrade xUnit in my project to 3.x version. Currently using 2.9.2.
```

---

### Task 7: Evaluate Whether to Migrate to TUnit

**Scenario**: Consider migrating from xUnit to TUnit

**Recommended Skills**:
1. `dotnet-testing-advanced-tunit-fundamentals` (understand basics)
2. `dotnet-testing-advanced-tunit-advanced` (evaluate advanced features)

**Implementation Steps**:
1. Understand differences between TUnit and xUnit
2. Evaluate migration cost
3. Experimental migration of one test file
4. Decide whether to fully migrate

**Prompt Example**:
```
Please use dotnet-testing-advanced-tunit-fundamentals skill
to evaluate whether project should migrate from xUnit to TUnit. Project currently has 500+ tests.
```
