# Test Double Five Types

> This document is extracted from [SKILL.md](../SKILL.md) and contains complete Test Double classification descriptions and code examples.

Based on Gerard Meszaros's definition in "xUnit Test Patterns":

## 1. Dummy - Placeholder Object

Only used to satisfy method signatures, not actually used.

```csharp
public interface IEmailService
{
    void SendEmail(string to, string subject, string body, ILogger logger);
}

[Fact]
public void ProcessOrder_NotUsingLogger_ShouldProcessOrderSuccessfully()
{
    // Dummy: Just to satisfy parameter requirement
    var dummyLogger = Substitute.For<ILogger>();
    
    var service = new OrderService();
    var result = service.ProcessOrder(order, dummyLogger);
    
    result.Success.Should().BeTrue();
    // Don't care if logger was called
}
```

## 2. Stub - Predefined Return Values

Provide predefined return values for testing specific scenarios.

```csharp
[Fact]
public void GetUser_ValidUserId_ShouldReturnUserData()
{
    // Arrange - Stub: Predefined return value
    var stubRepository = Substitute.For<IUserRepository>();
    stubRepository.GetById(123).Returns(new User { Id = 123, Name = "John" });
    
    var service = new UserService(stubRepository);
    
    // Act
    var actual = service.GetUser(123);
    
    // Assert
    actual.Name.Should().Be("John");
    // Don't care how many times GetById was called
}
```

## 3. Fake - Simplified Implementation

Simplified implementation with actual functionality, typically used for integration tests.

```csharp
public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    
    public User GetById(int id) => _users.TryGetValue(id, out var user) ? user : null;
    public void Save(User user) => _users[user.Id] = user;
    public void Delete(int id) => _users.Remove(id);
}

[Fact]
public void CreateUser_CreateUser_ShouldSaveAndBeQueryable()
{
    // Fake: Simplified implementation with real logic
    var fakeRepository = new FakeUserRepository();
    var service = new UserService(fakeRepository);
    
    service.CreateUser(new User { Id = 1, Name = "John" });
    var actual = service.GetUser(1);
    
    actual.Name.Should().Be("John");
}
```

## 4. Spy - Record Calls

Record how it was called, can verify afterward.

```csharp
[Fact]
public void CreateUser_CreateUser_ShouldRecordCreationInfo()
{
    // Arrange
    var spyLogger = Substitute.For<ILogger<UserService>>();
    var repository = Substitute.For<IUserRepository>();
    var service = new UserService(repository, spyLogger);
    
    // Act
    service.CreateUser(new User { Name = "John" });
    
    // Assert - Spy: Verify call records
    spyLogger.Received(1).LogInformation("User created: {Name}", "John");
}
```

## 5. Mock - Behavior Verification

Preset expected interaction behavior, test fails if expectations are not met.

```csharp
[Fact]
public void RegisterUser_RegisterUser_ShouldSendWelcomeEmail()
{
    // Arrange
    var mockEmailService = Substitute.For<IEmailService>();
    var repository = Substitute.For<IUserRepository>();
    var service = new UserService(repository, mockEmailService);
    
    // Act
    service.RegisterUser("john@example.com", "John");
    
    // Assert - Mock: Verify specific interaction behavior
    mockEmailService.Received(1).SendWelcomeEmail("john@example.com", "John");
}
```
