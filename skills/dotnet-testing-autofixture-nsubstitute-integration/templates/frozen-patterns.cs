// =============================================================================
// Frozen Pattern Usage Templates
// Demonstrates various usage patterns of the [Frozen] attribute in AutoFixture + NSubstitute integration
// =============================================================================

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MyProject.Tests.FrozenPatterns;

#region Basic Frozen Patterns

// =============================================================================
// Basic Frozen Patterns
// =============================================================================

/// <summary>
/// Demonstrates basic usage of the Frozen attribute
/// </summary>
public class BasicFrozenPatternTests
{
    /// <summary>
    /// Basic pattern: Freeze dependency and set behavior
    /// </summary>
    /// <remarks>
    /// [Frozen] ensures repository and sut use the same instance
    /// </remarks>
    [Theory]
    [AutoNSubstituteData]
    public async Task BasicFrozenPattern(
        [Frozen] IOrderRepository repository,  // Freeze this dependency
        OrderService sut)                       // SUT will use the same repository
    {
        // Arrange
        var orderId = 1;
        var expectedOrder = new Order { Id = orderId, Status = "Active" };
        
        // Set behavior on frozen instance
        repository.GetByIdAsync(orderId).Returns(expectedOrder);

        // Act
        var result = await sut.GetOrderAsync(orderId);

        // Assert
        result.Should().BeEquivalentTo(expectedOrder);
        
        // Verify call (since it's the same instance, verification will succeed)
        await repository.Received(1).GetByIdAsync(orderId);
    }

    /// <summary>
    /// Wrong pattern: Frozen parameter after SUT
    /// </summary>
    /// <remarks>
    /// When Frozen parameter is after SUT, SUT uses a different repository instance
    /// This demonstrates an incorrect pattern
    /// </remarks>
    // [Theory]
    // [AutoNSubstituteData]
    // public async Task WrongFrozenOrder(
    //     OrderService sut,                       // SUT created first
    //     [Frozen] IOrderRepository repository)   // Too late to freeze, this is a different instance
    // {
    //     // This repository is not the one used inside sut
    //     repository.GetByIdAsync(Arg.Any<int>()).Returns(new Order());
    //     
    //     // sut uses a different repository instance
    //     // So this setup won't work!
    // }

    /// <summary>
    /// Parameter order importance
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task CorrectParameterOrder(
        [Frozen] IOrderRepository repository,   // 1. Freeze dependency first
        [Frozen] INotificationService notifier, // 2. Can freeze multiple
        [Frozen] ILogger<OrderService> logger,  // 3. Including ILogger
        OrderService sut)                        // 4. SUT last
    {
        // Arrange
        repository.GetByIdAsync(Arg.Any<int>()).Returns(new Order { Id = 1 });
        notifier.SendAsync(Arg.Any<string>()).Returns(true);

        // Act
        var result = await sut.GetOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
    }
}

#endregion

#region Multiple Frozen Dependencies

// =============================================================================
// Multiple Frozen Dependencies
// =============================================================================

/// <summary>
/// Demonstrates patterns for freezing multiple dependencies
/// </summary>
public class MultipleFrozenDependenciesTests
{
    /// <summary>
    /// Freeze multiple dependencies and set behaviors separately
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task FreezeMultipleDependencies(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] ICustomerRepository customerRepository,
        [Frozen] IInventoryService inventoryService,
        OrderProcessingService sut)
    {
        // Arrange
        var order = new Order { Id = 1, CustomerId = 100, ProductId = 200 };
        var customer = new Customer { Id = 100, Name = "Test Customer" };
        
        orderRepository.GetByIdAsync(1).Returns(order);
        customerRepository.GetByIdAsync(100).Returns(customer);
        inventoryService.CheckAvailabilityAsync(200).Returns(true);

        // Act
        var result = await sut.ProcessOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be("Test Customer");
    }

    /// <summary>
    /// Partial freeze: Only freeze dependencies that need behavior setup
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task PartialFreeze(
        [Frozen] IOrderRepository repository,  // Only freeze this
        OrderService sut)                       // Other dependencies auto-generated but not frozen
    {
        // Arrange
        repository.GetByIdAsync(1).Returns(new Order { Id = 1 });

        // Act
        var result = await sut.GetOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
        
        // Only verify dependency with set behavior
        await repository.Received(1).GetByIdAsync(1);
    }
}

#endregion

#region Frozen with Auto-Generated Data

// =============================================================================
// Frozen with Auto-Generated Data
// =============================================================================

/// <summary>
/// Demonstrates combining Frozen dependencies with auto-generated data
/// </summary>
public class FrozenWithAutoGeneratedDataTests
{
    /// <summary>
    /// Auto-generate test data and use frozen dependency
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task FrozenWithAutoGeneratedModel(
        [Frozen] IOrderRepository repository,
        OrderService sut,
        Order order)  // AutoFixture auto-generates
    {
        // Arrange
        // Use auto-generated order object
        repository.GetByIdAsync(order.Id).Returns(order);

        // Act
        var result = await sut.GetOrderAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
    }

    /// <summary>
    /// Auto-generate collection data
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task FrozenWithAutoGeneratedCollection(
        [Frozen] IOrderRepository repository,
        OrderService sut,
        List<Order> orders)  // Auto-generates 3 Orders
    {
        // Arrange
        repository.GetAllAsync().Returns(orders);

        // Act
        var result = await sut.GetAllOrdersAsync();

        // Assert
        result.Should().HaveCount(3);  // AutoFixture default generates 3
    }

    /// <summary>
    /// Using CollectionSize to control collection size
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task FrozenWithCollectionSize(
        [Frozen] IOrderRepository repository,
        OrderService sut,
        [CollectionSize(10)] IEnumerable<Order> orders)
    {
        // Arrange
        repository.GetAllAsync().Returns(orders);

        // Act
        var result = await sut.GetAllOrdersAsync();

        // Assert
        result.Should().HaveCount(10);
    }
}

#endregion

#region Frozen with IFixture

// =============================================================================
// Frozen with IFixture
// =============================================================================

/// <summary>
/// Demonstrates combining Frozen with IFixture
/// </summary>
public class FrozenWithIFixtureTests
{
    /// <summary>
    /// Use IFixture for precise test data control
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task FrozenWithIFixtureControl(
        IFixture fixture,
        [Frozen] IOrderRepository repository,
        OrderService sut)
    {
        // Arrange
        // Use fixture for precise data control
        var order = fixture.Build<Order>()
            .With(o => o.Id, 999)
            .With(o => o.Status, "Pending")
            .With(o => o.TotalAmount, 1500.00m)
            .Create();

        repository.GetByIdAsync(999).Returns(order);

        // Act
        var result = await sut.GetOrderAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(999);
        result.Status.Should().Be("Pending");
        result.TotalAmount.Should().Be(1500.00m);
    }

    /// <summary>
    /// Use IFixture to create multiple related objects
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task FrozenWithRelatedObjects(
        IFixture fixture,
        [Frozen] IOrderRepository orderRepository,
        [Frozen] ICustomerRepository customerRepository,
        OrderProcessingService sut)
    {
        // Arrange
        var customerId = 100;
        
        var customer = fixture.Build<Customer>()
            .With(c => c.Id, customerId)
            .With(c => c.Name, "VIP Customer")
            .Create();

        var orders = fixture.Build<Order>()
            .With(o => o.CustomerId, customerId)
            .CreateMany(5);

        customerRepository.GetByIdAsync(customerId).Returns(customer);
        orderRepository.GetByCustomerIdAsync(customerId).Returns(orders);

        // Act
        var result = await sut.GetCustomerOrdersAsync(customerId);

        // Assert
        result.Should().HaveCount(5);
        result.All(o => o.CustomerId == customerId).Should().BeTrue();
    }
}

#endregion

#region Frozen Verification Patterns

// =============================================================================
// Frozen Verification Patterns
// =============================================================================

/// <summary>
/// Demonstrates using Frozen for behavior verification
/// </summary>
public class FrozenVerificationPatternsTests
{
    /// <summary>
    /// Verify method was called
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task VerifyMethodCalled(
        [Frozen] INotificationService notifier,
        [Frozen] IOrderRepository repository,
        OrderService sut,
        Order order)
    {
        // Arrange
        repository.GetByIdAsync(order.Id).Returns(order);
        notifier.SendAsync(Arg.Any<string>()).Returns(true);

        // Act
        await sut.ProcessAndNotifyAsync(order.Id);

        // Assert
        await notifier.Received(1).SendAsync(Arg.Any<string>());
    }

    /// <summary>
    /// Verify method was not called
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task VerifyMethodNotCalled(
        [Frozen] INotificationService notifier,
        [Frozen] IOrderRepository repository,
        OrderService sut)
    {
        // Arrange
        repository.GetByIdAsync(Arg.Any<int>()).Returns((Order?)null);

        // Act
        await sut.ProcessAndNotifyAsync(999);

        // Assert
        // When order doesn't exist, notification should not be sent
        await notifier.DidNotReceive().SendAsync(Arg.Any<string>());
    }

    /// <summary>
    /// Verify passed arguments
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task VerifyArgumentPassed(
        [Frozen] IOrderRepository repository,
        OrderService sut,
        Order order)
    {
        // Arrange
        repository.SaveAsync(Arg.Any<Order>()).Returns(true);

        // Act
        await sut.UpdateOrderStatusAsync(order, "Completed");

        // Assert
        await repository.Received(1).SaveAsync(Arg.Is<Order>(o =>
            o.Id == order.Id &&
            o.Status == "Completed"));
    }

    /// <summary>
    /// Capture and verify arguments
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task CaptureAndVerifyArgument(
        [Frozen] INotificationService notifier,
        [Frozen] IOrderRepository repository,
        OrderService sut,
        Order order)
    {
        // Arrange
        string? capturedMessage = null;
        repository.GetByIdAsync(order.Id).Returns(order);
        notifier.SendAsync(Arg.Do<string>(msg => capturedMessage = msg))
                .Returns(true);

        // Act
        await sut.ProcessAndNotifyAsync(order.Id);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage.Should().Contain(order.Id.ToString());
    }
}

#endregion

#region Advanced Frozen Patterns

// =============================================================================
// Advanced Frozen Patterns
// =============================================================================

/// <summary>
/// Advanced Frozen usage patterns
/// </summary>
public class AdvancedFrozenPatternsTests
{
    /// <summary>
    /// Set different return values for different inputs
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task DifferentReturnsForDifferentInputs(
        [Frozen] IOrderRepository repository,
        OrderService sut)
    {
        // Arrange
        var activeOrder = new Order { Id = 1, Status = "Active" };
        var completedOrder = new Order { Id = 2, Status = "Completed" };

        repository.GetByIdAsync(1).Returns(activeOrder);
        repository.GetByIdAsync(2).Returns(completedOrder);

        // Act
        var result1 = await sut.GetOrderAsync(1);
        var result2 = await sut.GetOrderAsync(2);

        // Assert
        result1.Status.Should().Be("Active");
        result2.Status.Should().Be("Completed");
    }

    /// <summary>
    /// Set sequential return values
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task SequentialReturns(
        [Frozen] IOrderRepository repository,
        OrderService sut)
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = 1, Status = "Pending" },
            new Order { Id = 2, Status = "Processing" },
            new Order { Id = 3, Status = "Completed" }
        };

        repository.GetNextOrderAsync()
            .Returns(orders[0], orders[1], orders[2]);

        // Act
        var result1 = await sut.GetNextOrderAsync();
        var result2 = await sut.GetNextOrderAsync();
        var result3 = await sut.GetNextOrderAsync();

        // Assert
        result1.Status.Should().Be("Pending");
        result2.Status.Should().Be("Processing");
        result3.Status.Should().Be("Completed");
    }

    /// <summary>
    /// Simulate exceptions
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task SimulateException(
        [Frozen] IOrderRepository repository,
        OrderService sut)
    {
        // Arrange
        repository.GetByIdAsync(Arg.Any<int>())
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetOrderAsync(1));

        exception.Message.Should().Contain("Database connection failed");
    }

    /// <summary>
    /// Conditional return values
    /// </summary>
    [Theory]
    [AutoNSubstituteData]
    public async Task ConditionalReturns(
        [Frozen] IOrderRepository repository,
        OrderService sut)
    {
        // Arrange
        repository.GetByIdAsync(Arg.Is<int>(id => id > 0))
            .Returns(callInfo => new Order 
            { 
                Id = callInfo.Arg<int>(), 
                Status = "Found" 
            });

        repository.GetByIdAsync(Arg.Is<int>(id => id <= 0))
            .Returns((Order?)null);

        // Act
        var positiveResult = await sut.GetOrderAsync(5);
        var negativeResult = await sut.GetOrderAsync(-1);

        // Assert
        positiveResult.Should().NotBeNull();
        positiveResult!.Id.Should().Be(5);
        negativeResult.Should().BeNull();
    }
}

#endregion

#region Example Interfaces and Classes

// =============================================================================
// Example Interfaces and Classes (for tests above)
// =============================================================================

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<Order?> GetNextOrderAsync();
    Task<bool> SaveAsync(Order order);
}

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
}

public interface INotificationService
{
    Task<bool> SendAsync(string message);
}

public interface IInventoryService
{
    Task<bool> CheckAvailabilityAsync(int productId);
}

public interface ILogger<T>
{
    void LogInformation(string message);
    void LogError(Exception ex, string message);
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class OrderResult
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly INotificationService? _notifier;

    public OrderService(
        IOrderRepository repository,
        INotificationService? notifier = null)
    {
        _repository = repository;
        _notifier = notifier;
    }

    public async Task<Order?> GetOrderAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Order?> GetNextOrderAsync()
    {
        return await _repository.GetNextOrderAsync();
    }

    public async Task ProcessAndNotifyAsync(int orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order != null && _notifier != null)
        {
            await _notifier.SendAsync($"Order {orderId} processed");
        }
    }

    public async Task UpdateOrderStatusAsync(Order order, string status)
    {
        order.Status = status;
        await _repository.SaveAsync(order);
    }
}

public class OrderProcessingService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IInventoryService _inventoryService;

    public OrderProcessingService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IInventoryService inventoryService)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _inventoryService = inventoryService;
    }

    public async Task<OrderResult?> ProcessOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer == null) return null;

        var available = await _inventoryService.CheckAvailabilityAsync(order.ProductId);
        if (!available) return null;

        return new OrderResult
        {
            OrderId = order.Id,
            CustomerName = customer.Name
        };
    }

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId)
    {
        return await _orderRepository.GetByCustomerIdAsync(customerId);
    }
}

// AutoNSubstituteData attribute (simplified version for this file's examples)
public class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute() : base(() => 
        new Fixture().Customize(new AutoNSubstituteCustomization()))
    {
    }
}

// CollectionSize attribute (from autofixture-customization skill)
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class CollectionSizeAttribute : CustomizeAttribute
{
    private readonly int _size;

    public CollectionSizeAttribute(int size)
    {
        _size = size;
    }

    public override ICustomization GetCustomization(System.Reflection.ParameterInfo parameter)
    {
        return new CollectionSizeCustomization(parameter.ParameterType, _size);
    }

    private class CollectionSizeCustomization : ICustomization
    {
        private readonly Type _targetType;
        private readonly int _size;

        public CollectionSizeCustomization(Type targetType, int size)
        {
            _targetType = targetType;
            _size = size;
        }

        public void Customize(IFixture fixture)
        {
            fixture.RepeatCount = _size;
        }
    }
}

#endregion
