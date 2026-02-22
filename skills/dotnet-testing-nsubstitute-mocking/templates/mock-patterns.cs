// NSubstitute Mock/Stub/Spy Pattern Examples
// Demonstrates the five Test Double types and common testing patterns

using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NSubstituteMockingExamples;

// ==================== Test Data Models ====================

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public OrderStatus Status { get; set; }
}

public enum CustomerType
{
    Regular,
    Premium,
    VIP
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

public enum PaymentResult
{
    Success,
    Failed,
    Pending
}

// ==================== Dependency Interface Definitions ====================

public interface IUserRepository
{
    User? GetById(int id);
    Task<User?> GetByIdAsync(int id);
    void Save(User user);
    Task SaveAsync(User user);
    void Delete(int id);
    IEnumerable<User> GetAll();
}

public interface IEmailService
{
    void SendEmail(string to, string subject, string body, ILogger logger);
    void SendWelcomeEmail(string email, string name);
    void SendConfirmation(string email);
    bool SendNotification(string email, string message);
}

public interface ICustomerService
{
    CustomerType GetCustomerType(int customerId);
}

public interface IPaymentGateway
{
    PaymentResult ProcessPayment(decimal amount);
}

public interface IOrderRepository
{
    Order? GetById(int id);
    void Save(Order order);
}

// ==================== Business Logic Classes ====================

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService? _emailService;
    private readonly ILogger<UserService>? _logger;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public UserService(IUserRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public User? GetUser(int id)
    {
        return _repository.GetById(id);
    }

    public async Task<User?> GetUserAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public void CreateUser(User user)
    {
        _repository.Save(user);
        _logger?.LogInformation("User created: {Name}", user.Name);
    }

    public void RegisterUser(string email, string name)
    {
        _emailService?.SendWelcomeEmail(email, name);
    }

    public async Task SaveUserAsync(User user)
    {
        await _repository.SaveAsync(user);
    }
}

public class OrderService
{
    private readonly IOrderRepository? _repository;
    private readonly IEmailService? _emailService;
    private readonly ILogger<OrderService>? _logger;

    public OrderService() { }

    public OrderService(IOrderRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public OrderService(IOrderRepository repository, IEmailService emailService, ILogger<OrderService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }

    public OrderResult ProcessOrder(Order order, ILogger dummyLogger)
    {
        // Order processing logic
        return new OrderResult { Success = true };
    }

    public OrderResult ProcessOrder(int orderId)
    {
        var order = _repository?.GetById(orderId);
        if (order == null)
            return new OrderResult { Success = false };

        order.Status = OrderStatus.Completed;
        return new OrderResult { Success = true };
    }
}

public class OrderResult
{
    public bool Success { get; set; }
}

public class PricingService
{
    private readonly ICustomerService _customerService;

    public PricingService(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public decimal CalculateDiscount(int customerId, decimal amount)
    {
        var customerType = _customerService.GetCustomerType(customerId);

        return customerType switch
        {
            CustomerType.Premium => amount * 0.2m,
            CustomerType.VIP => amount * 0.3m,
            _ => 0
        };
    }
}

public class PaymentService
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentGateway paymentGateway, ILogger<PaymentService> logger)
    {
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public void ProcessPayment(decimal amount)
    {
        var result = _paymentGateway.ProcessPayment(amount);
        _logger.LogInformation("Payment processed: {Amount} - Result: {Result}", amount, result);
    }
}

// ==================== Test Classes ====================

public class TestDoublePatternTests
{
    // ==================== Pattern 1: Dummy - Placeholder Object ====================

    [Fact]
    public void Pattern1_Dummy_OnlyToSatisfyParameterRequirementsNotUsed()
    {
        // Arrange - Dummy: Just to satisfy parameter requirements
        var dummyLogger = Substitute.For<ILogger>();
        var order = new Order { Id = 1, ProductName = "Product A" };
        var service = new OrderService();

        // Act
        var result = service.ProcessOrder(order, dummyLogger);

        // Assert
        Assert.True(result.Success);
        // Don't care if dummyLogger was called
    }

    // ==================== Test Double Type Examples ====================

    [Fact]
    public void Pattern1_Dummy_OnlyUsedToSatisfyMethodSignature()
    {
        // Arrange
        var dummyLogger = Substitute.For<ILogger>();
        var order = new Order { Id = 1, ProductName = "Product A" };
        var service = new OrderService();

        // Act
        var result = service.ProcessOrder(order, dummyLogger);

        // Assert
        Assert.True(result.Success);
        // Don't care if dummyLogger was called
    }
}

// ==================== Test Classes ====================

public class NSubstituteMockPatternsTests
{
    // ==================== Pattern 1: Dummy - Placeholder Object ====================

    [Fact]
    public void Dummy_ProcessOrder_DoesNotUseLogger_ShouldSuccessfullyProcessOrder()
    {
        // Arrange - Dummy: Just to satisfy parameter requirements
        var dummyLogger = Substitute.For<ILogger>();
        var service = new OrderService();
        var order = new Order { Id = 1, ProductName = "Test" };

        // Act
        var result = service.ProcessOrder(order, dummyLogger);

        // Assert
        Assert.True(result.Success);
        // Don't care if logger was called
    }

    // ==================== Stub Test Patterns ====================

    [Fact]
    public void Stub_GetUser_ValidUserId_ShouldReturnUserData()
    {
        // Arrange - Stub: Predefined return values
        var stubRepository = Substitute.For<IUserRepository>();
        stubRepository.GetById(123).Returns(new User
        {
            Id = 123,
            Name = "John Doe",
            Email = "john@example.com"
        });

        var service = new UserService(stubRepository);

        // Act
        var actual = service.GetUser(123);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("John Doe", actual.Name);
        Assert.Equal("john@example.com", actual.Email);
        // Don't care how many times GetById was called
    }

    [Fact]
    public void Stub_GetUser_AnyId_ShouldReturnDefaultUser()
    {
        // Arrange - Use Arg.Any to match any parameter
        var stubRepository = Substitute.For<IUserRepository>();
        stubRepository.GetById(Arg.Any<int>()).Returns(new User
        {
            Id = 999,
            Name = "Default User"
        });

        var service = new UserService(stubRepository);

        // Act
        var result1 = service.GetUser(1);
        var result2 = service.GetUser(100);
        var result3 = service.GetUser(999);

        // Assert
        Assert.Equal("Default User", result1?.Name);
        Assert.Equal("Default User", result2?.Name);
        Assert.Equal("Default User", result3?.Name);
    }

    [Fact]
    public void Stub_CalculateDiscount_PremiumMember_ShouldReturn20PercentDiscount()
    {
        // Arrange - Stub: Only care about return value
        var stubCustomerService = Substitute.For<ICustomerService>();
        stubCustomerService.GetCustomerType(123).Returns(CustomerType.Premium);

        var service = new PricingService(stubCustomerService);

        // Act
        var discount = service.CalculateDiscount(123, 1000);

        // Assert - Only verify result state
        Assert.Equal(200, discount); // 20% of 1000
    }

    [Fact]
    public void Stub_CalculateDiscount_VIPMember_ShouldReturn30PercentDiscount()
    {
        // Arrange
        var stubCustomerService = Substitute.For<ICustomerService>();
        stubCustomerService.GetCustomerType(456).Returns(CustomerType.VIP);

        var service = new PricingService(stubCustomerService);

        // Act
        var discount = service.CalculateDiscount(456, 1000);

        // Assert
        Assert.Equal(300, discount); // 30% of 1000
    }

    // ==================== Fake Test Patterns ====================

    [Fact]
    public void Fake_CreateAndGetUser_ShouldCorrectlyStoreAndQuery()
    {
        // Arrange - Fake: Simplified implementation with real logic
        var fakeRepository = new FakeUserRepository();
        var service = new UserService(fakeRepository);

        var newUser = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

        // Act
        fakeRepository.Save(newUser);
        var actual = service.GetUser(1);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("John Doe", actual.Name);
        Assert.Equal("john@example.com", actual.Email);
    }

    [Fact]
    public void Fake_DeleteUser_ShouldRemoveUser()
    {
        // Arrange
        var fakeRepository = new FakeUserRepository();
        var service = new UserService(fakeRepository);

        fakeRepository.Save(new User { Id = 1, Name = "John" });

        // Act
        fakeRepository.Delete(1);
        var actual = service.GetUser(1);

        // Assert
        Assert.Null(actual);
    }

    // ==================== Spy Test Patterns ====================

    [Fact]
    public void Spy_CreateUser_ShouldLogUserCreationInfo()
    {
        // Arrange
        var spyLogger = Substitute.For<ILogger<UserService>>();
        var repository = Substitute.For<IUserRepository>();
        var service = new UserService(repository, spyLogger);

        var newUser = new User { Id = 1, Name = "John Doe" };

        // Act
        service.CreateUser(newUser);

        // Assert - Spy: Verify call records
        spyLogger.Received(1).LogInformation("User created: {Name}", "John Doe");
    }

    // ==================== Mock Test Patterns ====================

    [Fact]
    public void Mock_RegisterUser_ShouldSendWelcomeEmail()
    {
        // Arrange
        var mockEmailService = Substitute.For<IEmailService>();
        var repository = Substitute.For<IUserRepository>();
        var service = new UserService(repository, mockEmailService);

        // Act
        service.RegisterUser("john@example.com", "John Doe");

        // Assert - Mock: Verify specific interaction behavior
        mockEmailService.Received(1).SendWelcomeEmail("john@example.com", "John Doe");
    }

    [Fact]
    public void Mock_RegisterUser_ShouldOnlySendEmailOnce()
    {
        // Arrange
        var mockEmailService = Substitute.For<IEmailService>();
        var repository = Substitute.For<IUserRepository>();
        var service = new UserService(repository, mockEmailService);

        // Act
        service.RegisterUser("john@example.com", "John");

        // Assert - Verify call count
        mockEmailService.Received(1).SendWelcomeEmail(Arg.Any<string>(), Arg.Any<string>());
    }

    // ==================== Async Test Patterns ====================

    [Fact]
    public async Task Async_GetUserAsync_UserExists_ShouldReturnUserData()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.GetByIdAsync(123).Returns(Task.FromResult<User?>(
            new User { Id = 123, Name = "John Doe" }));

        var service = new UserService(repository);

        // Act
        var result = await service.GetUserAsync(123);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        await repository.Received(1).GetByIdAsync(123);
    }

    [Fact]
    public async Task Async_SaveUserAsync_DatabaseError_ShouldThrowException()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.SaveAsync(Arg.Any<User>())
                  .Throws(new InvalidOperationException("Database connection failed"));

        var service = new UserService(repository);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.SaveUserAsync(new User { Name = "John" }));
    }

    // ==================== Returns Sequence Test ====================

    [Fact]
    public void Returns_Sequence_GetAll_ShouldReturnDifferentUserCollectionsSequentially()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();

        // Set sequence return values
        repository.GetAll().Returns(
            new[] { new User { Id = 1, Name = "User1" } },
            new[] { new User { Id = 1, Name = "User1" }, new User { Id = 2, Name = "User2" } },
            new[] { new User { Id = 1, Name = "User1" }, new User { Id = 2, Name = "User2" }, new User { Id = 3, Name = "User3" } }
        );

        // Act
        var result1 = repository.GetAll();
        var result2 = repository.GetAll();
        var result3 = repository.GetAll();

        // Assert
        Assert.Single(result1);
        Assert.Equal(2, result2.Count());
        Assert.Equal(3, result3.Count());
    }

    // ==================== Exception Handling Test ====================

    [Fact]
    public void Throws_GetUser_DatabaseConnectionFailed_ShouldThrowException()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        repository.GetById(Arg.Any<int>())
                  .Throws(new InvalidOperationException("Database connection failed"));

        var service = new UserService(repository);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.GetUser(123));
    }

    // ==================== Conditional Returns Test ====================

    [Fact]
    public void Returns_Conditional_GetById_ShouldReturnDifferentUsersBasedOnId()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();

        // Conditional return: Even ID returns Premium user, odd returns Regular user
        repository.GetById(Arg.Any<int>()).Returns(x =>
        {
            var id = (int)x[0];
            return new User
            {
                Id = id,
                Name = $"User{id}",
                CustomerType = id % 2 == 0 ? CustomerType.Premium : CustomerType.Regular
            };
        });

        var service = new UserService(repository);

        // Act
        var user1 = service.GetUser(1);
        var user2 = service.GetUser(2);
        var user3 = service.GetUser(3);

        // Assert
        Assert.Equal(CustomerType.Regular, user1?.CustomerType);
        Assert.Equal(CustomerType.Premium, user2?.CustomerType);
        Assert.Equal(CustomerType.Regular, user3?.CustomerType);
    }

    // ==================== Did Not Receive Test ====================

    [Fact]
    public void DidNotReceive_ProcessOrder_OrderProcessingFailed_ShouldNotSendEmail()
    {
        // Arrange
        var mockEmailService = Substitute.For<IEmailService>();
        var repository = Substitute.For<IOrderRepository>();
        repository.GetById(Arg.Any<int>()).Returns((Order?)null); // Order does not exist

        var service = new OrderService(repository, mockEmailService);

        // Act
        var result = service.ProcessOrder(999);

        // Assert
        Assert.False(result.Success);
        mockEmailService.DidNotReceive().SendConfirmation(Arg.Any<string>());
    }
}

// ==================== Fake Implementation Example ====================

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();

    public User? GetById(int id)
    {
        _users.TryGetValue(id, out var user);
        return user;
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return Task.FromResult(GetById(id));
    }

    public void Save(User user)
    {
        _users[user.Id] = user;
    }

    public Task SaveAsync(User user)
    {
        Save(user);
        return Task.CompletedTask;
    }

    public void Delete(int id)
    {
        _users.Remove(id);
    }

    public IEnumerable<User> GetAll()
    {
        return _users.Values;
    }
}
