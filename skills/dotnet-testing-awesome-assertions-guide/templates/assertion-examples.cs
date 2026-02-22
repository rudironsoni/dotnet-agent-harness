using AwesomeAssertions;
using Xunit;

namespace YourProject.Tests.Examples;

/// <summary>
/// AwesomeAssertions common assertion examples collection
/// Covers objects, strings, numbers, collections, exceptions, async, and various scenarios
/// </summary>
public class AssertionExamples
{
    #region Object Assertion Examples

    [Fact]
    public void ObjectAssertions_BasicValidation()
    {
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Null check
        user.Should().NotBeNull();

        // Type check
        user.Should().BeOfType<User>();
        user.Should().BeAssignableTo<IUser>();

        // Equality check
        var anotherUser = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com"
        };
        user.Should().BeEquivalentTo(anotherUser);
    }

    [Fact]
    public void ObjectAssertions_PropertyValidation()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10
        };

        // Single property validation
        product.Id.Should().BeGreaterThan(0);
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BePositive();

        // Multiple property anonymous object comparison
        product.Should().BeEquivalentTo(new
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m
        });
    }

    #endregion

    #region String Assertion Examples

    [Fact]
    public void StringAssertions_ContentValidation()
    {
        var message = "Hello World";

        // Basic checks
        message.Should().NotBeNullOrEmpty();
        message.Should().NotBeNullOrWhiteSpace();

        // Content checks
        message.Should().Contain("Hello");
        message.Should().StartWith("Hello");
        message.Should().EndWith("World");
        message.Should().ContainEquivalentOf("WORLD"); // Case-insensitive

        // Length checks
        message.Should().HaveLength(11);
        message.Should().HaveLengthGreaterThan(5);
    }

    [Fact]
    public void StringAssertions_PatternMatching()
    {
        var email = "user@example.com";

        // Regex matching
        email.Should().MatchRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");

        // Contains keywords
        email.Should().Contain("@").And.Contain(".");

        // Email format validation example
        email.Should().NotBeNullOrEmpty()
             .And.Contain("@")
             .And.MatchRegex(@"@[\w-]+\.");
    }

    #endregion

    #region Numeric Assertion Examples

    [Fact]
    public void NumericAssertions_RangeValidation()
    {
        var age = 25;

        // Comparison operations
        age.Should().BeGreaterThan(18);
        age.Should().BeLessThan(65);
        age.Should().BeInRange(18, 65);

        // Specific value checks
        age.Should().BeOneOf(25, 30, 35);
        age.Should().BePositive();
    }

    [Fact]
    public void NumericAssertions_FloatingPointHandling()
    {
        var pi = 3.14159;

        // Precision comparison (Important! Avoid floating-point precision issues)
        pi.Should().BeApproximately(3.14, 0.01);

        // Special value checks
        double.NaN.Should().Be(double.NaN);
        double.PositiveInfinity.Should().BePositiveInfinity();
        double.NegativeInfinity.Should().BeNegativeInfinity();
    }

    #endregion

    #region Collection Assertion Examples

    [Fact]
    public void CollectionAssertions_BasicValidation()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Count checks
        numbers.Should().NotBeEmpty();
        numbers.Should().HaveCount(5);
        numbers.Should().HaveCountGreaterThan(3);

        // Content checks
        numbers.Should().Contain(3);
        numbers.Should().ContainSingle(x => x == 3);
        numbers.Should().NotContain(0);

        // Complete comparison
        numbers.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void CollectionAssertions_ComplexObjects()
    {
        var users = new[]
        {
            new User { Id = 1, Name = "John", Age = 30 },
            new User { Id = 2, Name = "Jane", Age = 25 },
            new User { Id = 3, Name = "Bob", Age = 35 }
        };

        // Conditional filtering
        users.Should().Contain(u => u.Name == "John");
        users.Should().OnlyContain(u => u.Age >= 18);

        // All satisfy condition
        users.Should().AllSatisfy(u =>
        {
            u.Id.Should().BeGreaterThan(0);
            u.Name.Should().NotBeNullOrEmpty();
            u.Age.Should().BePositive();
        });

        // Order check
        var ages = users.Select(u => u.Age).ToArray();
        ages.Should().BeInAscendingOrder();
    }

    #endregion

    #region Exception Assertion Examples

    [Fact]
    public void ExceptionAssertions_BasicValidation()
    {
        var calculator = new Calculator();

        // Expected exception
        Action act = () => calculator.Divide(10, 0);

        act.Should().Throw<DivideByZeroException>()
           .WithMessage("*cannot divide by zero*");
    }

    [Fact]
    public void ExceptionAssertions_ParameterValidation()
    {
        var userService = new UserService();

        // Parameter exception validation
        Action act = () => userService.GetUser(-1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*User ID must be positive*")
           .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void ExceptionAssertions_ShouldNotThrow()
    {
        var calculator = new Calculator();

        // Should not throw any exception
        Action act = () => calculator.Add(1, 2);
        act.Should().NotThrow();
    }

    #endregion

    #region Async Assertion Examples

    [Fact]
    public async Task AsyncAssertions_TaskCompletion()
    {
        var service = new AsyncService();

        // Wait for task completion and validate
        var result = await service.GetDataAsync();

        result.Should().NotBeNull();
        result.Should().BeOfType<DataResult>();
    }

    [Fact]
    public async Task AsyncAssertions_ExecutionTime()
    {
        var service = new AsyncService();

        // Validate execution time
        Func<Task> act = () => service.GetDataAsync();

        await act.Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AsyncAssertions_ExceptionHandling()
    {
        var service = new AsyncService();

        // Async exception validation
        Func<Task> act = async () => await service.GetInvalidDataAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*data not found*");
    }

    #endregion

    #region Complex Object Comparison Examples

    [Fact]
    public void ComplexObjectComparison_DeepComparison()
    {
        var order = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            Items = new[]
            {
                new OrderItem { ProductId = 1, Quantity = 2, Price = 10.5m },
                new OrderItem { ProductId = 2, Quantity = 1, Price = 25.0m }
            },
            TotalAmount = 46.0m
        };

        var expected = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            Items = new[]
            {
                new OrderItem { ProductId = 1, Quantity = 2, Price = 10.5m },
                new OrderItem { ProductId = 2, Quantity = 1, Price = 25.0m }
            },
            TotalAmount = 46.0m
        };

        // Deep object comparison
        order.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ComplexObjectComparison_ExcludingFields()
    {
        var user = new User
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var expected = new User
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            CreatedAt = DateTime.Now.AddDays(-1),  // Different time
            UpdatedAt = DateTime.Now.AddHours(-2)   // Different time
        };

        // Exclude timestamp fields
        user.Should().BeEquivalentTo(expected, options => options
            .Excluding(u => u.CreatedAt)
            .Excluding(u => u.UpdatedAt)
        );
    }

    [Fact]
    public void ComplexObjectComparison_PartialProperties()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10,
            CreatedAt = DateTime.Now
        };

        // Compare only specific properties
        product.Should().BeEquivalentTo(new
        {
            Name = "Laptop",
            Price = 999.99m
        });
    }

    #endregion

    #region AssertionScope Examples

    [Fact]
    public void AssertionScope_CollectMultipleFailures()
    {
        var user = new User
        {
            Id = 0,  // Error: should be > 0
            Name = "",  // Error: should not be empty
            Email = "invalid-email"  // Error: incorrect format
        };

        // Use AssertionScope to collect all failed assertions
        using (new AssertionScope())
        {
            user.Id.Should().BeGreaterThan(0, "User ID must be positive");
            user.Name.Should().NotBeNullOrEmpty("User name is required");
            user.Email.Should().MatchRegex(@"@.*\.", "Email format is invalid");
        }
        // All failures will be displayed at once, instead of stopping at the first failure
    }

    #endregion
}

#region Test Model Classes

public interface IUser
{
    int Id { get; set; }
    string Name { get; set; }
}

public class User : IUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderItem[] Items { get; set; } = Array.Empty<OrderItem>();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class DataResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region Test Service Classes

public class Calculator
{
    public int Add(int a, int b) => a + b;

    public int Divide(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException("Cannot divide by zero");
        return a / b;
    }
}

public class UserService
{
    public User GetUser(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be positive", nameof(userId));

        return new User { Id = userId, Name = "Test User" };
    }
}

public class AsyncService
{
    public async Task<DataResult> GetDataAsync()
    {
        await Task.Delay(100);
        return new DataResult { IsSuccess = true, Message = "Success" };
    }

    public async Task<DataResult> GetInvalidDataAsync()
    {
        await Task.Delay(100);
        throw new InvalidOperationException("Data not found");
    }
}

#endregion
