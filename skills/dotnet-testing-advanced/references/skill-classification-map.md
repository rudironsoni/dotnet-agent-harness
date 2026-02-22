# Skill Classification Map

### 1. Integration Testing (4 Skills) - Web API Testing

| Skill Name | Test Scope | Database | Use Case |
|-----------|-----------|----------|----------|
| `dotnet-testing-advanced-aspnet-integration-testing` | WebApplicationFactory, TestServer, HTTP responses | In-memory database | Basic API integration testing |
| `dotnet-testing-advanced-webapi-integration-testing` | Complete CRUD, error handling, business processes | In-memory/Real | Real API project testing |
| `dotnet-testing-advanced-testcontainers-database` | SQL Server, PostgreSQL, MySQL | Real (containerized) | Need real database behavior |
| `dotnet-testing-advanced-testcontainers-nosql` | MongoDB, Redis, Elasticsearch | Real (containerized) | NoSQL database testing |

#### Skill Detailed Description

**aspnet-integration-testing**

**Core Value**:
- Learn WebApplicationFactory basics
- Understand integration testing concepts
- Test HTTP endpoints without starting real server

**Suitable For**:
- Basic API endpoint testing
- Routing validation
- Middleware testing
- Don't need real database

**Learning Difficulty**: ⭐⭐ Medium

**Prerequisite Skills**:
- `dotnet-testing-unit-test-fundamentals` (required)
- `dotnet-testing-awesome-assertions-guide` (recommended)

---

**webapi-integration-testing**

**Core Value**:
- Complete API testing workflow
- Test data management strategies
- Error handling validation
- Real business scenario testing

**Suitable For**:
- Production project API testing
- Complete CRUD workflow
- Complex business logic validation
- Need test data preparation and cleanup

**Learning Difficulty**: ⭐⭐⭐ Medium-High

**Prerequisite Skills**:
- `dotnet-testing-advanced-aspnet-integration-testing` (required)
- `dotnet-testing-nsubstitute-mocking` (recommended)

---

**testcontainers-database**

**Core Value**:
- Test with real databases
- Automated container management
- Test database-specific features
- Isolated test environments

**Suitable For**:
- EF Core testing
- Dapper testing
- Stored Procedures testing
- Database migration testing

**Learning Difficulty**: ⭐⭐⭐ Medium-High

**Prerequisite Skills**:
- `dotnet-testing-unit-test-fundamentals` (required)
- Docker basics (required)

**Technical Requirements**:
- Docker Desktop installed
- WSL2 (Windows environment)

---

**testcontainers-nosql**

**Core Value**:
- Test NoSQL database operations
- Containerized NoSQL environments
- Real database behavior validation

**Suitable For**:
- MongoDB document operations
- Redis cache logic
- Elasticsearch search functionality

**Learning Difficulty**: ⭐⭐⭐ Medium-High

**Prerequisite Skills**:
- `dotnet-testing-advanced-testcontainers-database` (recommended)
- NoSQL database basics

---

### 2. Microservices Testing (1 Skill) - Distributed Systems

| Skill Name | Test Scope | Architecture | Use Case |
|-----------|-----------|--------------|----------|
| `dotnet-testing-advanced-aspire-testing` | .NET Aspire distributed applications | Microservices | Cloud-native, microservices architecture |

#### Skill Detailed Description

**aspire-testing**

**Core Value**:
- Test .NET Aspire projects
- Distributed application integration testing
- Service dependency management
- End-to-end process validation

**Suitable For**:
- .NET Aspire microservices projects
- Multi-service collaboration testing
- Service discovery testing
- Distributed tracing validation

**Learning Difficulty**: ⭐⭐⭐⭐ High

**Prerequisite Skills**:
- `dotnet-testing-advanced-aspnet-integration-testing` (required)
- `dotnet-testing-advanced-testcontainers-database` (recommended)
- .NET Aspire basics (required)

**Technical Requirements**:
- .NET 8+
- .NET Aspire Workload
- Docker Desktop

**Coverage**:
- DistributedApplication testing
- Service-to-service communication testing
- Dependency service management
- Test container orchestration

---

### 3. Framework Migration (3 Skills) - Test Framework Upgrade

| Skill Name | Migration Path | Difficulty | Use Case |
|-----------|---------------|------------|----------|
| `dotnet-testing-advanced-xunit-upgrade-guide` | xUnit 2.x → 3.x | ⭐⭐ Medium | Upgrade existing xUnit projects |
| `dotnet-testing-advanced-tunit-fundamentals` | xUnit → TUnit (fundamentals) | ⭐⭐ Medium | Evaluate or migrate to TUnit |
| `dotnet-testing-advanced-tunit-advanced` | TUnit advanced features | ⭐⭐⭐ Medium-High | Deeply use TUnit |

#### Skill Detailed Description

**xunit-upgrade-guide**

**Core Value**:
- Understand xUnit 3.x new features
- Handle upgrade issues
- Migration step guidance

**Suitable For**:
- Projects using xUnit 2.x
- Want to upgrade to latest version
- Understand version differences

**Learning Difficulty**: ⭐⭐ Medium

**Coverage**:
- Breaking changes list
- Package upgrade steps
- Compatibility issue handling
- Upgrade checklist

**When to Upgrade**:
- Need xUnit 3.x new features
- .NET 9+ projects
- Resolve known issues

---

**tunit-fundamentals**

**Core Value**:
- Understand TUnit next-generation testing framework
- Learn TUnit core concepts
- Evaluate migration feasibility

**Suitable For**:
- Consider migrating from xUnit
- New project choosing testing framework
- Understand modern testing frameworks

**Learning Difficulty**: ⭐⭐ Medium

**Coverage**:
- TUnit vs xUnit comparison
- Basic test writing
- Attributes and assertions
- Migration recommendations

**TUnit Advantages**:
- Better performance
- Native dependency injection support
- More flexible test organization
- Modernized API design

---

**tunit-advanced**

**Core Value**:
- Deeply use TUnit advanced features
- Parallel execution control
- Dependency injection integration
- Data-driven testing

**Suitable For**:
- Already using TUnit basics
- Need advanced features
- Large test projects

**Learning Difficulty**: ⭐⭐⭐ Medium-High

**Prerequisite Skills**:
- `dotnet-testing-advanced-tunit-fundamentals` (required)

**Coverage**:
- Advanced data-driven testing
- Dependency injection container
- Test execution control
- Custom test framework behavior
