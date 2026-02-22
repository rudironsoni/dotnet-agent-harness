# Quick Decision Tree

### What Kind of Advanced Testing Do I Need?

#### Scenario 1: Testing ASP.NET Core Web API

**Option A - Basic API Testing**
→ `dotnet-testing-advanced-aspnet-integration-testing`

**Suitable For**:
- Simple API endpoint testing
- Don't need real database (use in-memory database)
- Test routing, model binding, HTTP responses

**Coverage**:
- WebApplicationFactory usage
- TestServer configuration
- HTTP response validation
- In-memory database configuration

---

**Option B - Complete WebAPI Process Testing**
→ `dotnet-testing-advanced-webapi-integration-testing`

**Suitable For**:
- Complete CRUD API testing
- Need to test complete business processes
- Need test data preparation and cleanup

**Coverage**:
- Complete GET, POST, PUT, DELETE testing
- Error handling validation
- Test base class patterns
- Data preparation strategies

---

#### Scenario 2: Testing Requires Real Database

**Option A - Relational Database (SQL Server, PostgreSQL, MySQL)**
→ `dotnet-testing-advanced-testcontainers-database`

**Suitable For**:
- Entity Framework Core testing
- Dapper testing
- Real database behavior validation
- Need to test database-specific features (stored procedures, triggers, etc.)

**Supported Databases**:
- SQL Server
- PostgreSQL
- MySQL
- MariaDB

---

**Option B - NoSQL Database (MongoDB, Redis, Elasticsearch)**
→ `dotnet-testing-advanced-testcontainers-nosql`

**Suitable For**:
- MongoDB document operation testing
- Redis cache testing
- Elasticsearch search testing
- NoSQL-specific feature testing

**Supported Databases**:
- MongoDB
- Redis
- Elasticsearch

---

#### Scenario 3: Testing Microservices Architecture

→ `dotnet-testing-advanced-aspire-testing`

**Suitable For**:
- .NET Aspire projects
- Distributed application testing
- Service-to-service communication testing
- Microservices integration testing

**Coverage**:
- DistributedApplication testing
- Service dependency management
- Cross-service testing
- Test container orchestration

---

#### Scenario 4: Upgrade or Migrate Test Framework

**Option A - xUnit Upgrade (2.x → 3.x)**
→ `dotnet-testing-advanced-xunit-upgrade-guide`

**Suitable For**:
- Existing projects using xUnit 2.x
- Want to upgrade to xUnit 3.x
- Understand version differences

**Coverage**:
- Breaking changes explanation
- Upgrade step guidance
- Compatibility issue handling
- Best practices

---

**Option B - Migrate to TUnit (Fundamentals)**
→ `dotnet-testing-advanced-tunit-fundamentals`

**Suitable For**:
- Evaluate whether to migrate to TUnit
- Understand TUnit basics
- Learn differences between TUnit and xUnit

**Coverage**:
- TUnit core concepts
- Comparison with xUnit
- Migration steps
- Basic usage

---

**Option C - TUnit Advanced Features**
→ `dotnet-testing-advanced-tunit-advanced`

**Suitable For**:
- Already using TUnit basics
- Want to deeply use TUnit features
- Need parallel execution, dependency injection and other advanced features

**Coverage**:
- Data-driven testing
- Dependency injection
- Parallel execution control
- Advanced features
