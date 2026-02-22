# Common Task Mapping Table

> This document is extracted from `SKILL.md`, providing skill combination recommendations and implementation steps for 7 common testing tasks.

---

### Task 1: Create Test Project from Scratch

**Scenario**: Brand new project, need to establish complete testing infrastructure

**Recommended Skill Combination**:
1. `dotnet-testing-xunit-project-setup` - Create project structure
2. `dotnet-testing-test-naming-conventions` - Set naming conventions
3. `dotnet-testing-unit-test-fundamentals` - Write first test

**Implementation Steps**:
1. Use xunit-project-setup to create test project
2. Configure .csproj file and NuGet packages
3. Learn naming conventions, establish team standards
4. Write first test following 3A Pattern

**Prompt Example**:
```
Please use dotnet-testing-xunit-project-setup skill to create test structure for my project,
project name is MyProject, then use dotnet-testing-unit-test-fundamentals skill
to create first test for Calculator class.
```

---

### Task 2: Write Tests for Service Class with Dependencies

**Scenario**: UserService depends on IUserRepository and IEmailService

**Recommended Skill Combination**:
1. `dotnet-testing-unit-test-fundamentals` - Test structure
2. `dotnet-testing-nsubstitute-mocking` - Mock dependencies
3. `dotnet-testing-autofixture-basics` - Generate test data
4. `dotnet-testing-awesome-assertions-guide` - Clear assertions

**Implementation Steps**:
1. Use NSubstitute to create Mocks for Repository and EmailService
2. Use AutoFixture to generate User test data
3. Write tests following 3A Pattern
4. Use FluentAssertions to verify results

**Prompt Example**:
```
Please use dotnet-testing-nsubstitute-mocking and dotnet-testing-autofixture-basics skills
to create tests for UserService. UserService constructor needs IUserRepository and IEmailService.
Please test CreateUser method.
```

**Expected Code Structure**:
```csharp
[Fact]
public void CreateUser_ValidUser_ShouldSaveAndSendEmail()
{
    // Arrange - Use NSubstitute to create Mock
    var repository = Substitute.For<IUserRepository>();
    var emailService = Substitute.For<IEmailService>();

    // Use AutoFixture to generate test data
    var fixture = new Fixture();
    var user = fixture.Create<User>();

    var sut = new UserService(repository, emailService);

    // Act
    sut.CreateUser(user);

    // Assert - Use FluentAssertions
    repository.Received(1).Save(Arg.Is<User>(u => u.Email == user.Email));
    emailService.Received(1).SendWelcomeEmail(user.Email);
}
```

---

### Task 3: Test Code with Time Logic

**Scenario**: OrderService determines if order is expired

**Recommended Skill Combination**:
1. `dotnet-testing-datetime-testing-timeprovider` - TimeProvider abstraction
2. `dotnet-testing-unit-test-fundamentals` - Test fundamentals
3. `dotnet-testing-nsubstitute-mocking` - Mock TimeProvider

**Implementation Steps**:
1. Refactor code to inject TimeProvider
2. Use FakeTimeProvider in tests
3. Control time for testing

**Prompt Example**:
```
Please use dotnet-testing-datetime-testing-timeprovider skill to help refactor OrderService,
so it can be tested for time-related logic. Orders are considered expired after 30 days.
```

**Expected Code Structure**:
```csharp
// Before refactoring
public class OrderService
{
    public bool IsExpired(Order order)
    {
        return DateTime.Now > order.CreatedDate.AddDays(30);  // Cannot test
    }
}

// After refactoring
public class OrderService
{
    private readonly TimeProvider _timeProvider;

    public OrderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsExpired(Order order)
    {
        return _timeProvider.GetUtcNow() > order.CreatedDate.AddDays(30);
    }
}

// Test
[Fact]
public void IsExpired_Order31DaysOld_ShouldReturnTrue()
{
    // Arrange
    var fakeTime = new FakeTimeProvider();
    fakeTime.SetUtcNow(new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero));

    var order = new Order
    {
        CreatedDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
    };

    var sut = new OrderService(fakeTime);

    // Act
    var result = sut.IsExpired(order);

    // Assert
    result.Should().BeTrue();
}
```

---

### Task 4: Improve Test Readability

**Scenario**: Existing tests are hard to understand, difficult to maintain

**Recommended Skill Combination**:
1. `dotnet-testing-test-naming-conventions` - Naming conventions
2. `dotnet-testing-awesome-assertions-guide` - Fluent assertions
3. `dotnet-testing-test-data-builder-pattern` - Clear test data

**Implementation Steps**:
1. Rename test methods following three-part naming
2. Rewrite assertions using FluentAssertions
3. Use Builder Pattern to create test data

**Prompt Example**:
```
Please use dotnet-testing-awesome-assertions-guide and dotnet-testing-test-naming-conventions skills
to review and improve readability of these tests:
[paste test code]
```

---

### Task 5: Generate Large Amounts of Test Data

**Scenario**: Performance testing needs 1000 Customer records

**Recommended Skill Combination**:
1. `dotnet-testing-autofixture-basics` - Auto-generate
2. `dotnet-testing-bogus-fake-data` - Realistic data (optional)
3. `dotnet-testing-autofixture-bogus-integration` - Combine both advantages (optional)

**Implementation Steps**:
1. Use AutoFixture.CreateMany<T>() to generate large amounts of data
2. (Optional) Use Bogus to make data more realistic
3. (Optional) Integrate both for automation + realism

**Prompt Example**:
```
Please use dotnet-testing-autofixture-basics skill to generate 1000 Customer records for performance testing,
each record needs Name, Email, PhoneNumber fields.
```

---

### Task 6: Test FluentValidation Validators

**Scenario**: CreateUserValidator has multiple validation rules

**Recommended Skill**:
- `dotnet-testing-fluentvalidation-testing`

**Implementation Steps**:
1. Use FluentValidation.TestHelper
2. Test each validation rule
3. Test combination scenarios

**Prompt Example**:
```
Please use dotnet-testing-fluentvalidation-testing skill to create tests for CreateUserValidator.
Validator rules include: Name required, Email format validation, Age must be over 18.
```

---

### Task 7: Test File Upload Functionality

**Scenario**: FileUploadService needs to store uploaded files

**Recommended Skill**:
- `dotnet-testing-filesystem-testing-abstractions`

**Implementation Steps**:
1. Refactor code to use IFileSystem
2. Use MockFileSystem in tests
3. Verify file operations

**Prompt Example**:
```
Please use dotnet-testing-filesystem-testing-abstractions skill to help test FileUploadService.
This service stores uploaded files to uploads/ directory.
```
