// Async Validator Examples
using FluentValidation;
using FluentValidation.TestHelper;
using NSubstitute;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluentValidationAsyncExample;

// External service interface
public interface IUserService
{
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailRegisteredAsync(string email);
}

// Test data model
public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Async validator implementation
public class UserRegistrationAsyncValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly IUserService _userService;

    public UserRegistrationAsyncValidator(IUserService userService)
    {
        _userService = userService;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be null or empty")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters");

        RuleFor(x => x.Username)
            .MustAsync(async (username, cancellation) =>
                await _userService.IsUsernameAvailableAsync(username))
            .WithMessage("Username is already taken")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be null or empty")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) =>
                !await _userService.IsEmailRegisteredAsync(email))
            .WithMessage("This email is already registered")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

// Async validator tests
public class UserRegistrationAsyncValidatorTests
{
    private readonly IUserService _mockUserService;
    private readonly UserRegistrationAsyncValidator _validator;

    public UserRegistrationAsyncValidatorTests()
    {
        _mockUserService = Substitute.For<IUserService>();
        _validator = new UserRegistrationAsyncValidator(_mockUserService);
    }

    #region Username Availability Tests

    [Fact]
    public async Task ValidateAsync_UsernameAvailable_ShouldPassValidation()
    {
        var request = new UserRegistrationRequest
        {
            Username = "newuser123",
            Email = "new@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("newuser123").Returns(Task.FromResult(true));
        _mockUserService.IsEmailRegisteredAsync("new@example.com").Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);

        await _mockUserService.Received(1).IsUsernameAvailableAsync("newuser123");
        await _mockUserService.Received(1).IsEmailRegisteredAsync("new@example.com");
    }

    [Fact]
    public async Task ValidateAsync_UsernameAlreadyTaken_ShouldFailValidation()
    {
        var request = new UserRegistrationRequest
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("existinguser").Returns(Task.FromResult(false));
        _mockUserService.IsEmailRegisteredAsync("new@example.com").Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is already taken");

        await _mockUserService.Received(1).IsUsernameAvailableAsync("existinguser");
    }

    [Fact]
    public async Task ValidateAsync_EmptyUsername_ShouldSkipAsyncValidation()
    {
        var request = new UserRegistrationRequest
        {
            Username = "",
            Email = "test@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
        _mockUserService.IsEmailRegisteredAsync("test@example.com").Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username cannot be null or empty");

        await _mockUserService.DidNotReceive().IsUsernameAvailableAsync(Arg.Any<string>());
    }

    #endregion

    #region Email Availability Tests

    [Fact]
    public async Task ValidateAsync_EmailNotRegistered_ShouldPassValidation()
    {
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "available@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("testuser").Returns(Task.FromResult(true));
        _mockUserService.IsEmailRegisteredAsync("available@example.com").Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        await _mockUserService.Received(1).IsEmailRegisteredAsync("available@example.com");
    }

    [Fact]
    public async Task ValidateAsync_EmailAlreadyRegistered_ShouldFailValidation()
    {
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("testuser").Returns(Task.FromResult(true));
        _mockUserService.IsEmailRegisteredAsync("existing@example.com").Returns(Task.FromResult(true));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("This email is already registered");

        await _mockUserService.Received(1).IsEmailRegisteredAsync("existing@example.com");
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task ValidateAsync_ServiceThrowsException_ShouldPropagate()
    {
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("testuser")
            .Throws(new TimeoutException("Database connection timeout"));

        await Assert.ThrowsAsync<TimeoutException>(async () =>
            await _validator.TestValidateAsync(request));
    }

    [Fact]
    public async Task ValidateAsync_ServiceUnavailable_ShouldHandleCorrectly()
    {
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("testuser")
            .Returns(Task.FromException<bool>(new InvalidOperationException("Service temporarily unavailable")));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _validator.TestValidateAsync(request));

        await _mockUserService.Received(1).IsUsernameAvailableAsync("testuser");
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldPassCorrectly()
    {
        var request = new UserRegistrationRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        var cts = new CancellationTokenSource();

        _mockUserService.IsUsernameAvailableAsync("testuser").Returns(Task.FromResult(true));
        _mockUserService.IsEmailRegisteredAsync("test@example.com").Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(request, strategy =>
        {
            strategy.IncludeAllRuleSets();
        }, cts.Token);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ValidateAsync_UsernameAndEmailBothTaken_ShouldShowBothErrors()
    {
        var request = new UserRegistrationRequest
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        _mockUserService.IsUsernameAvailableAsync("existinguser").Returns(Task.FromResult(false));
        _mockUserService.IsEmailRegisteredAsync("existing@example.com").Returns(Task.FromResult(true));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is already taken");

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("This email is already registered");

        await _mockUserService.Received(1).IsUsernameAvailableAsync("existinguser");
        await _mockUserService.Received(1).IsEmailRegisteredAsync("existing@example.com");
    }

    #endregion
}

// Advanced example: Conditional async validation
public class OrderValidator : AbstractValidator<OrderRequest>
{
    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;

    public OrderValidator(IInventoryService inventoryService, IPaymentService paymentService)
    {
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID cannot be empty");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x)
            .MustAsync(async (order, cancellation) =>
                await _inventoryService.IsStockAvailableAsync(order.ProductId, order.Quantity))
            .WithMessage("Insufficient stock")
            .When(x => !string.IsNullOrEmpty(x.ProductId) && x.Quantity > 0);

        RuleFor(x => x.PaymentMethod)
            .MustAsync(async (order, paymentMethod, cancellation) =>
                await _paymentService.IsPaymentMethodValidAsync(paymentMethod, order.Amount))
            .WithMessage("This payment method is not valid for this order amount")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));
    }
}

public class OrderRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public interface IInventoryService
{
    Task<bool> IsStockAvailableAsync(string productId, int quantity);
}

public interface IPaymentService
{
    Task<bool> IsPaymentMethodValidAsync(string paymentMethod, decimal amount);
}

// Conditional async validation tests
public class OrderValidatorTests
{
    private readonly IInventoryService _mockInventoryService;
    private readonly IPaymentService _mockPaymentService;
    private readonly OrderValidator _validator;

    public OrderValidatorTests()
    {
        _mockInventoryService = Substitute.For<IInventoryService>();
        _mockPaymentService = Substitute.For<IPaymentService>();
        _validator = new OrderValidator(_mockInventoryService, _mockPaymentService);
    }

    [Fact]
    public async Task ValidateAsync_StockAvailableAndPaymentValid_ShouldPassValidation()
    {
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 5,
            PaymentMethod = "CreditCard",
            Amount = 1000m
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 5).Returns(Task.FromResult(true));
        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 1000m).Returns(Task.FromResult(true));

        var result = await _validator.TestValidateAsync(order);

        result.ShouldNotHaveAnyValidationErrors();

        await _mockInventoryService.Received(1).IsStockAvailableAsync("PROD001", 5);
        await _mockPaymentService.Received(1).IsPaymentMethodValidAsync("CreditCard", 1000m);
    }

    [Fact]
    public async Task ValidateAsync_InsufficientStock_ShouldFailValidation()
    {
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 100,
            PaymentMethod = "CreditCard",
            Amount = 10000m
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 100).Returns(Task.FromResult(false));
        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 10000m).Returns(Task.FromResult(true));

        var result = await _validator.TestValidateAsync(order);

        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("Insufficient stock");
    }

    [Fact]
    public async Task ValidateAsync_EmptyProductId_ShouldSkipStockCheck()
    {
        var order = new OrderRequest
        {
            ProductId = "",
            Quantity = 5,
            PaymentMethod = "CreditCard",
            Amount = 1000m
        };

        _mockInventoryService.IsStockAvailableAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(Task.FromResult(true));
        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 1000m).Returns(Task.FromResult(true));

        var result = await _validator.TestValidateAsync(order);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        await _mockInventoryService.DidNotReceive().IsStockAvailableAsync(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ValidateAsync_PaymentMethodInvalid_ShouldFailValidation()
    {
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 1,
            PaymentMethod = "Cash",
            Amount = 100000m
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 1).Returns(Task.FromResult(true));
        _mockPaymentService.IsPaymentMethodValidAsync("Cash", 100000m).Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(order);

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod)
              .WithErrorMessage("This payment method is not valid for this order amount");
    }

    [Fact]
    public async Task ValidateAsync_MultipleAsyncRules_ParallelExecution()
    {
        var order = new OrderRequest
        {
            ProductId = "PROD001",
            Quantity = 5,
            PaymentMethod = "CreditCard",
            Amount = 1000m
        };

        _mockInventoryService.IsStockAvailableAsync("PROD001", 5)
            .Returns(async _ => { await Task.Delay(100); return true; });

        _mockPaymentService.IsPaymentMethodValidAsync("CreditCard", 1000m)
            .Returns(async _ => { await Task.Delay(100); return true; });

        var startTime = DateTime.UtcNow;
        var result = await _validator.TestValidateAsync(order);
        var elapsed = DateTime.UtcNow - startTime;

        result.ShouldNotHaveAnyValidationErrors();

        Assert.True(elapsed.TotalMilliseconds < 300,
            $"Validation took {elapsed.TotalMilliseconds}ms, may not be running in parallel");
    }
}
