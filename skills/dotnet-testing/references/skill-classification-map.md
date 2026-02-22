# Skill Classification Map

> This document is extracted from `SKILL.md`, providing complete classification descriptions for 19 fundamental skills.

---

### 1. Testing Fundamentals (3 skills) - Essential Basics

| Skill Name | Core Value | Beginner Friendly | When to Use |
|-----------|-----------|-------------------|-------------|
| `dotnet-testing-unit-test-fundamentals` | Understand FIRST principles, 3A Pattern, testing pyramid and other basic concepts | ⭐⭐⭐ | Starting point for all tests, establish correct testing mindset |
| `dotnet-testing-test-naming-conventions` | Learn three-part naming method, Chinese naming suggestions | ⭐⭐⭐ | Improve test readability and maintainability |
| `dotnet-testing-xunit-project-setup` | Establish standard xUnit test project structure | ⭐⭐⭐ | When creating new test projects |

**Recommended Learning Order**: fundamentals → naming-conventions → xunit-project-setup

**Core Takeaways**:
- **FIRST Principles**: Fast, Independent, Repeatable, Self-Validating, Timely
- **3A Pattern**: Arrange-Act-Assert structured testing
- **Naming Convention**: `[MethodUnderTest]_[TestScenario]_[ExpectedBehavior]`
- **Project Structure**: tests/ directory, .csproj configuration, NuGet package management

---

### 2. Test Data Generation (5 skills) - Improve Efficiency

| Skill Name | Features | Advantages | When to Use |
|-----------|----------|-----------|-------------|
| `dotnet-testing-autofixture-basics` | Auto-generate anonymous test data | Reduce boilerplate code, quickly create test objects | Need large amounts of test data, test data content is not important |
| `dotnet-testing-autofixture-customization` | Customize AutoFixture behavior | Control data generation rules, match business logic | Need test data with specific rules |
| `dotnet-testing-bogus-fake-data` | Generate realistic fake data (names, addresses, emails, etc.) | Data looks real, easy to debug | Need realistic test data, demo purposes |
| `dotnet-testing-test-data-builder-pattern` | Use Builder Pattern to create test data | Semantic clarity, express test intent | Need high readability, complex object construction |
| `dotnet-testing-autofixture-bogus-integration` | Combine AutoFixture and Bogus | Complementary advantages of both | Need both automation and realistic data |

**Selection Guide**:

```
Need large amounts of data + don't care about realism?
  → autofixture-basics

Need data that looks realistic?
  → bogus-fake-data

Need high readability and semantic clarity?
  → test-data-builder-pattern

Need flexible control over generation rules?
  → autofixture-basics + autofixture-customization

Need automation + realism?
  → autofixture-basics + autofixture-bogus-integration
```

**Learning Path**:
- Beginner: autofixture-basics or bogus-fake-data
- Advanced: autofixture-customization + test-data-builder-pattern
- Integration: autofixture-bogus-integration

---

### 3. Test Doubles (2 skills) - Handle Dependencies

| Skill Name | Purpose | Coverage | When to Use |
|-----------|---------|----------|-------------|
| `dotnet-testing-nsubstitute-mocking` | NSubstitute Mock framework | Mock, Stub, Spy, verify calls | Have external dependencies to mock |
| `dotnet-testing-autofixture-nsubstitute-integration` | AutoFixture + NSubstitute integration | Auto-create Mock objects | Using AutoFixture with many dependencies |

**Core Concepts**:
- **Mock**: Simulate objects and verify interactions
- **Stub**: Provide default responses
- **Spy**: Record calls and verify

**Recommended Learning Order**:
1. Learn nsubstitute-mocking first (understand Mock basics)
2. If already using AutoFixture, then learn integration (improve efficiency)

**Common Uses**:
- Mock database access layer (Repository)
- Mock external API calls
- Mock third-party services
- Verify methods are called correctly

---

### 4. Assertion Verification (3 skills) - Write Clear Tests

| Skill Name | Features | Improvement Level | When to Use |
|-----------|----------|-------------------|-------------|
| `dotnet-testing-awesome-assertions-guide` | FluentAssertions fluent assertions | ⭐⭐⭐ High | All tests (strongly recommended) |
| `dotnet-testing-complex-object-comparison` | Deep object comparison techniques | ⭐⭐⭐ High | DTO, Entity, complex object validation |
| `dotnet-testing-fluentvalidation-testing` | Test FluentValidation validators | ⭐⭐ Medium | Projects using FluentValidation |

**FluentAssertions Example Comparison**:

```csharp
// Traditional assertions
Assert.Equal(expected.Name, actual.Name);
Assert.Equal(expected.Age, actual.Age);
Assert.True(actual.IsActive);

// FluentAssertions (more readable)
actual.Should().BeEquivalentTo(expected, options => options
    .Including(x => x.Name)
    .Including(x => x.Age));
actual.IsActive.Should().BeTrue();
```

**Learning Path**:
1. awesome-assertions-guide (essential for all projects)
2. complex-object-comparison (handle complex comparisons)
3. fluentvalidation-testing (specific needs)

---

### 5. Special Scenarios (3 skills) - Solve Tricky Problems

| Skill Name | Problem Solved | Practical Value | Learning Difficulty |
|-----------|----------------|-----------------|---------------------|
| `dotnet-testing-datetime-testing-timeprovider` | Time-related testing (using .NET 8+ TimeProvider) | ⭐⭐⭐ High | Medium |
| `dotnet-testing-filesystem-testing-abstractions` | File system abstraction testing | ⭐⭐⭐ High | Medium |
| `dotnet-testing-private-internal-testing` | Test private/internal members | ⭐⭐ Medium | Simple |

**Time Testing Common Problem**:
```csharp
// Problem: hard to test
public bool IsExpired()
{
    return DateTime.Now > ExpiryDate;  // DateTime.Now cannot be controlled
}

// Solution: use TimeProvider
public bool IsExpired(TimeProvider timeProvider)
{
    return timeProvider.GetUtcNow() > ExpiryDate;  // Can inject fake time in tests
}
```

**File System Testing Common Problem**:
```csharp
// Problem: hard to test
public void SaveToFile(string content)
{
    File.WriteAllText("data.txt", content);  // Real file operation
}

// Solution: use IFileSystem abstraction
public void SaveToFile(string content, IFileSystem fileSystem)
{
    fileSystem.File.WriteAllText("data.txt", content);  // Can use in-memory file system in tests
}
```

**When to Use These Skills**:
- **TimeProvider**: Scheduling systems, validity checks, time calculations
- **FileSystem**: File uploads, report generation, configuration file read/write
- **Private Testing**: Refactoring legacy code (but consider refactoring design first)

---

### 6. Test Metrics (1 skill) - Quality Monitoring

| Skill Name | Purpose | Tools | When to Use |
|-----------|---------|-------|-------------|
| `dotnet-testing-code-coverage-analysis` | Code coverage analysis and reporting | Coverlet, ReportGenerator | Evaluate test completeness, CI/CD integration |

**Coverage**:
- Use Coverlet to collect coverage data
- Generate HTML reports
- Set coverage thresholds
- CI/CD integration

**Important Reminder**:
- High coverage ≠ high quality tests
- Goal is meaningful tests, not pursuing 100% coverage
- Coverage is a reference metric, not an absolute standard

---

### 7. Framework Integration (2 skills) - Advanced Integration

| Skill Name | Integration Target | Value | When to Use |
|-----------|-------------------|-------|-------------|
| `dotnet-testing-autodata-xunit-integration` | AutoFixture + xUnit Theory | Simplify parameterized tests | Using xUnit Theory and need test data |
| `dotnet-testing-test-output-logging` | ITestOutputHelper + ILogger | Test output and debugging | Need to view test execution process, debug complex tests |

**AutoData Example**:

```csharp
// Traditional approach
[Theory]
[InlineData("user1", "pass1")]
[InlineData("user2", "pass2")]
public void Login_ValidCredentials_Success(string username, string password)
{
    // ...
}

// Using AutoData
[Theory, AutoData]
public void Login_ValidCredentials_Success(string username, string password)
{
    // username and password are auto-generated
}
```

**Test Output Example**:

```csharp
public class MyTests
{
    private readonly ITestOutputHelper _output;

    public MyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Test()
    {
        _output.WriteLine("Debug information");  // Displayed in test output
    }
}
```
