# Learning Path Recommendations

### Integration Testing Introduction (1 Week)

**Goal**: Master integration testing fundamentals, able to create tests for Web API

#### Day 1-2: Basic Integration Testing

**Skill**: `dotnet-testing-advanced-aspnet-integration-testing`

**Learning Focus**:
- WebApplicationFactory concepts
- TestServer usage
- HTTP response validation
- In-memory database configuration

**Hands-on Practice**:
- Create integration tests for simple API
- Test GET, POST endpoints

---

#### Day 3-4: Complete WebAPI Testing

**Skill**: `dotnet-testing-advanced-webapi-integration-testing`

**Learning Focus**:
- Complete CRUD testing
- Test base class design
- Data preparation and cleanup
- Error handling validation

**Hands-on Practice**:
- Create tests for complete Controller
- Implement test data management

---

#### Day 5-6: Containerized Database Testing

**Skill**: `dotnet-testing-advanced-testcontainers-database`

**Learning Focus**:
- Testcontainers concepts
- SQL Server container configuration
- Database migration execution
- Test isolation

**Hands-on Practice**:
- Create tests for Repository
- Use real database

---

#### Day 7: NoSQL Database Testing

**Skill**: `dotnet-testing-advanced-testcontainers-nosql`

**Learning Focus**:
- MongoDB container configuration
- Redis container configuration
- NoSQL-specific test patterns

**Hands-on Practice**:
- Test MongoDB Repository
- Test Redis Cache Service

---

### Microservices Testing Specialization (3-5 Days)

**Prerequisite**: Complete Integration Testing Introduction

**Skill**: `dotnet-testing-advanced-aspire-testing`

**Learning Focus**:
- .NET Aspire architecture understanding
- DistributedApplication testing
- Service dependency management
- Distributed testing strategies

**Hands-on Practice**:
- Test Aspire microservices project
- Verify service-to-service communication

---

### Framework Migration Path (As Needed)

#### xUnit Upgrade (1-2 Days)

**Skill**: `dotnet-testing-advanced-xunit-upgrade-guide`

**Learning Focus**:
- xUnit 3.x new features
- Breaking change handling
- Upgrade steps
- Compatibility testing

**Implementation Steps**:
1. Understand version differences
2. Update packages
3. Handle compilation errors
4. Verify test execution

---

#### TUnit Migration (2-5 Days)

**Fundamentals (2-3 Days)**
Skill: `dotnet-testing-advanced-tunit-fundamentals`

**Learning Focus**:
- TUnit core concepts
- Comparison with xUnit
- Basic test writing
- Migration recommendations

---

**Advanced (2-3 Days)**
Skill: `dotnet-testing-advanced-tunit-advanced`

**Learning Focus**:
- Data-driven testing
- Dependency injection
- Parallel execution control
- Advanced features
