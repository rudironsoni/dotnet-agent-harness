# xUnit Test Project Structure Example

## Standard Project Structure

Below is the standard structure for a complete .NET solution with an xUnit test project:

```text
MyProject/
│
├── MyProject.sln                         # Solution file
│
├── src/                                  # Main source code directory
│   │
│   └── MyProject.Core/                   # Core business logic project
│       ├── MyProject.Core.csproj
│       │
│       ├── Models/                       # Data models
│       │   ├── User.cs
│       │   ├── Order.cs
│       │   └── Product.cs
│       │
│       ├── Services/                     # Business logic services
│       │   ├── IOrderService.cs
│       │   ├── OrderService.cs
│       │   ├── IUserService.cs
│       │   └── UserService.cs
│       │
│       ├── Repositories/                 # Data access layer
│       │   ├── IOrderRepository.cs
│       │   ├── OrderRepository.cs
│       │   ├── IUserRepository.cs
│       │   └── UserRepository.cs
│       │
│       └── Utilities/                    # Utility classes
│           ├── Calculator.cs
│           ├── StringHelper.cs
│           └── DateTimeHelper.cs
│
├── tests/                                # Test code directory
│   │
│   └── MyProject.Core.Tests/             # Core business logic test project
│       ├── MyProject.Core.Tests.csproj
│       │
│       ├── Models/                       # Tests corresponding to Models
│       │   ├── UserTests.cs
│       │   ├── OrderTests.cs
│       │   └── ProductTests.cs
│       │
│       ├── Services/                     # Tests corresponding to Services
│       │   ├── OrderServiceTests.cs
│       │   └── UserServiceTests.cs
│       │
│       ├── Repositories/                 # Tests corresponding to Repositories
│       │   ├── OrderRepositoryTests.cs
│       │   └── UserRepositoryTests.cs
│       │
│       ├── Utilities/                    # Tests corresponding to Utilities
│       │   ├── CalculatorTests.cs
│       │   ├── StringHelperTests.cs
│       │   └── DateTimeHelperTests.cs
│       │
│       └── Fixtures/                     # Shared test resources (Fixtures)
│           ├── DatabaseFixture.cs
│           └── TestDataFixture.cs
│
├── .gitignore
└── README.md
```

## Multi-Project Solution Structure

Recommended complete structure when the solution contains multiple projects:

```text
MyCompany.MyProduct/
│
├── MyCompany.MyProduct.sln
│
├── src/
│   │
│   ├── MyCompany.MyProduct.Core/         # Core business logic
│   │   └── MyCompany.MyProduct.Core.csproj
│   │
│   ├── MyCompany.MyProduct.Web/          # Web API project
│   │   └── MyCompany.MyProduct.Web.csproj
│   │
│   ├── MyCompany.MyProduct.Infrastructure/ # Infrastructure layer
│   │   └── MyCompany.MyProduct.Infrastructure.csproj
│   │
│   └── MyCompany.MyProduct.Shared/       # Shared code
│       └── MyCompany.MyProduct.Shared.csproj
│
├── tests/
│   │
│   ├── MyCompany.MyProduct.Core.Tests/   # Core business logic unit tests
│   │   └── MyCompany.MyProduct.Core.Tests.csproj
│   │
│   ├── MyCompany.MyProduct.Web.Tests/    # Web API unit tests
│   │   └── MyCompany.MyProduct.Web.Tests.csproj
│   │
│   ├── MyCompany.MyProduct.Infrastructure.Tests/ # Infrastructure unit tests
│   │   └── MyCompany.MyProduct.Infrastructure.Tests.csproj
│   │
│   └── MyCompany.MyProduct.Integration.Tests/    # Integration tests
│       └── MyCompany.MyProduct.Integration.Tests.csproj
│
├── docs/                                 # Documentation directory
│   ├── architecture.md
│   └── api-documentation.md
│
├── .editorconfig                         # Editor settings
├── .gitignore
├── Directory.Build.props                 # Shared MSBuild properties
├── Directory.Build.targets               # Shared MSBuild targets
└── README.md
```

## Test Class File Structure

### Single Test Class File Example

```csharp
// CalculatorTests.cs
using Xunit;
using MyProject.Core.Utilities;

namespace MyProject.Core.Tests.Utilities;

/// <summary>
/// Unit tests for the Calculator class
/// </summary>
public class CalculatorTests : IDisposable
{
    private readonly Calculator _calculator;

    // Constructor: Initialize before each test
    public CalculatorTests()
    {
        _calculator = new Calculator();
    }

    // Fact: Single test case
    [Fact]
    public void Add_TwoPositiveIntegers_ReturnsCorrectSum()
    {
        // Arrange
        int a = 5;
        int b = 3;

        // Act
        var result = _calculator.Add(a, b);

        // Assert
        Assert.Equal(8, result);
    }

    // Theory: Parameterized test cases
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void Add_MultipleInputs_ReturnsCorrectResult(int a, int b, int expected)
    {
        // Act
        var result = _calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // Dispose: Cleanup resources after each test
    public void Dispose()
    {
        // Cleanup logic (if needed)
    }
}
```

## Naming Conventions

### Test Project Naming

| Main Project           | Test Project                           | Description           |
| ---------------------- | -------------------------------------- | --------------------- |
| `MyProject.Core`       | `MyProject.Core.Tests`                 | Unit tests            |
| `MyProject.Web`        | `MyProject.Web.Tests`                  | Web layer unit tests  |
| `MyProject.Web`        | `MyProject.Web.Integration.Tests`      | Web layer integration tests |
| `MyProject`            | `MyProject.Acceptance.Tests`           | Acceptance tests      |
| `MyProject`            | `MyProject.Performance.Tests`          | Performance tests     |

### Test Class Naming

| Class Being Tested | Test Class               | File Name                     |
| ------------------ | ------------------------ | ----------------------------- |
| `Calculator`       | `CalculatorTests`        | `CalculatorTests.cs`          |
| `OrderService`     | `OrderServiceTests`      | `OrderServiceTests.cs`        |
| `UserRepository`   | `UserRepositoryTests`    | `UserRepositoryTests.cs`      |
| `StringHelper`     | `StringHelperTests`      | `StringHelperTests.cs`        |

## Directory Mapping Principle

The test project folder structure should mirror the main project:

```text
Main project:
src/MyProject.Core/Services/OrderService.cs

Test project:
tests/MyProject.Core.Tests/Services/OrderServiceTests.cs
```

This structure allows developers to quickly find the corresponding test file.

## Command Reference

### Using .NET CLI to Create This Structure

```powershell
# 1. Create solution
dotnet new sln -n MyProject

# 2. Create main project
dotnet new classlib -n MyProject.Core -o src/MyProject.Core

# 3. Create test project
dotnet new xunit -n MyProject.Core.Tests -o tests/MyProject.Core.Tests

# 4. Add to solution
dotnet sln add src/MyProject.Core/MyProject.Core.csproj
dotnet sln add tests/MyProject.Core.Tests/MyProject.Core.Tests.csproj

# 5. Create project reference
dotnet add tests/MyProject.Core.Tests/MyProject.Core.Tests.csproj reference src/MyProject.Core/MyProject.Core.csproj

# 6. Create subdirectories in main project
mkdir src/MyProject.Core/Models
mkdir src/MyProject.Core/Services
mkdir src/MyProject.Core/Repositories
mkdir src/MyProject.Core/Utilities

# 7. Create corresponding subdirectories in test project
mkdir tests/MyProject.Core.Tests/Models
mkdir tests/MyProject.Core.Tests/Services
mkdir tests/MyProject.Core.Tests/Repositories
mkdir tests/MyProject.Core.Tests/Utilities
mkdir tests/MyProject.Core.Tests/Fixtures
```

## Best Practice Recommendations

### Recommended Practices

1. **Separate test and main projects**: Use `src/` and `tests/` directories
2. **Naming consistency**: Test project name is `{MainProject}.Tests`
3. **Directory mirroring**: Test project folder structure corresponds to main project
4. **One class per test file**: `Calculator.cs` → `CalculatorTests.cs`
5. **Use Fixtures folder**: Store shared test resources

### Practices to Avoid

1. **Don't mix tests with production code**: Don't put tests in the main project
2. **Don't nest too deep**: Folder levels should not exceed 3-4 layers
3. **Don't include business logic in test projects**: Test projects only contain tests
4. **Main project should not reference test projects**: Only tests reference main project
5. **Don't package test projects**: Ensure `IsPackable=false`

## Example: Complete Small Project

```text
Calculator/
├── Calculator.sln
│
├── src/
│   └── Calculator.Core/
│       ├── Calculator.Core.csproj
│       ├── Calculator.cs
│       └── MathHelper.cs
│
└── tests/
    └── Calculator.Core.Tests/
        ├── Calculator.Core.Tests.csproj
        ├── CalculatorTests.cs
        └── MathHelperTests.cs
```

**Setup Commands:**

```powershell
dotnet new sln -n Calculator
dotnet new classlib -n Calculator.Core -o src/Calculator.Core
dotnet new xunit -n Calculator.Core.Tests -o tests/Calculator.Core.Tests
dotnet sln add src/Calculator.Core/Calculator.Core.csproj
dotnet sln add tests/Calculator.Core.Tests/Calculator.Core.Tests.csproj
dotnet add tests/Calculator.Core.Tests reference src/Calculator.Core
```

This structure is clean, clear, and suitable for small to medium-sized projects.
