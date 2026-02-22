# Guided Conversation Examples

> This document is extracted from `SKILL.md`, demonstrating how AI interacts with users and recommends the correct skill sets.

The following examples show how AI interacts with you to help you choose the right skills.

---

### Example 1: Getting Started

**You**: I want to learn .NET testing

**AI**: I'd be happy to help you learn .NET testing! To provide the most suitable guidance, please tell me:

1. **Your testing experience**:
   - [ ] Never written tests before
   - [ ] Written some basic tests
   - [ ] Have some experience, want to learn more

2. **Your project situation**:
   - [ ] New project, starting from scratch
   - [ ] Existing project, want to add tests
   - [ ] Improve existing test quality

Please tell me your situation, and I'll recommend the appropriate learning path.

**You**: Never written tests before, it's a new project

**AI**: Great! I'll plan a step-by-step learning path for you. I recommend starting with these three skills:

**Step 1: Understand Testing Fundamentals (1-2 days)**
Use `dotnet-testing-unit-test-fundamentals` skill
Learning focus: FIRST principles, 3A Pattern, basic test structure

**Step 2: Learn Naming Conventions (half day)**
Use `dotnet-testing-test-naming-conventions` skill
Learning focus: Three-part naming method, improving readability

**Step 3: Set Up Test Project (1 day)**
Use `dotnet-testing-xunit-project-setup` skill
Learning focus: Project structure, package configuration

Would you like me to help you get started with these skills?

---

### Example 2: Handling Dependencies

**You**: I need to write tests for UserService, but it depends on a database and external API

**AI**: Understood! Your situation requires mocking external dependencies. I recommend the following skill combination:

**Required Skills**:
1. `dotnet-testing-nsubstitute-mocking` - Mock database and API dependencies
   Purpose: Create Mock objects for IUserRepository and IApiClient

**Recommended Additions**:
2. `dotnet-testing-autofixture-basics` - Auto-generate test data
   Purpose: Quickly generate User test objects

3. `dotnet-testing-awesome-assertions-guide` - Clear assertions
   Purpose: Make tests more readable

**Implementation Suggestions**:
1. First use NSubstitute to create Mocks for dependencies
2. Use AutoFixture to generate User test data
3. Write tests following the 3A Pattern
4. Use FluentAssertions to verify results

Would you like me to help you create tests using these skills?

---

### Example 3: Specific Problem

**You**: My code uses DateTime.Now, how do I test it?

**AI**: This is a classic time-related testing problem! I recommend using the `dotnet-testing-datetime-testing-timeprovider` skill.

**Problem Analysis**:
`DateTime.Now` is different each execution, making repeatable tests impossible (violates Repeatable in FIRST principles).

**Solution**:
1. Refactor code to inject `TimeProvider`
2. Use `FakeTimeProvider` in tests to control time
3. This allows testing behavior at specific time points

**Example**:
```csharp
// Before refactoring (hard to test)
public bool IsExpired()
{
    return DateTime.Now > ExpiryDate;
}

// After refactoring (testable)
public bool IsExpired(TimeProvider timeProvider)
{
    return timeProvider.GetUtcNow() > ExpiryDate;
}
```

Would you like me to use the `dotnet-testing-datetime-testing-timeprovider` skill to help you refactor and create tests?

---

### Example 4: Improving Existing Tests

**You**: My tests are hard to read and difficult to maintain

**AI**: Test readability issues are common! I recommend improving from these three directions:

**1. Naming Conventions**
Use `dotnet-testing-test-naming-conventions` skill
- Adopt three-part naming: `[Method]_[Scenario]_[Expected]`
- Make test names self-documenting

**2. Fluent Assertions**
Use `dotnet-testing-awesome-assertions-guide` skill
- Use FluentAssertions instead of traditional assertions
- Greatly improve readability

**3. Clear Test Data**
Use `dotnet-testing-test-data-builder-pattern` skill
- Use Builder Pattern to create test data
- Make test intent clearer

**Quick Example**:
```csharp
// Before improvement
[Fact]
public void Test1()
{
    var u = new User { Name = "test", Age = 20 };
    var r = svc.Validate(u);
    Assert.True(r);
}

// After improvement
[Fact]
public void Validate_AdultUser_ShouldReturnTrue()
{
    // Arrange
    var user = new UserBuilder()
        .WithName("test")
        .WithAge(20)
        .Build();

    // Act
    var result = _service.Validate(user);

    // Assert
    result.Should().BeTrue();
}
```

Would you like me to help you improve specific tests? Please paste your test code.
