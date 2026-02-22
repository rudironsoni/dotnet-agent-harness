// =============================================================================
// Test Naming Convention Examples
// Standard format: MethodName_ScenarioDescription_ExpectedResult (Three-part naming)
// =============================================================================

using Xunit;

namespace TestNamingConventions.Examples;

// =============================================================================
// Test class naming: {ClassUnderTest}Tests
// =============================================================================

/// <summary>
/// Calculator functionality tests - Demonstrates three-part naming for basic operations
/// </summary>
public class CalculatorTests
{
    // Happy path test
    [Fact]
    public void Add_Input1And2_Returns3()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(1, 2);

        // Assert
        Assert.Equal(3, result);
    }

    // Boundary condition test
    [Fact]
    public void Add_Input0And0_Returns0()
    {
        var calculator = new Calculator();
        var result = calculator.Add(0, 0);
        Assert.Equal(0, result);
    }

    // Negative number test
    [Fact]
    public void Add_InputNegativeAndPositive_ReturnsCorrectResult()
    {
        var calculator = new Calculator();
        var result = calculator.Add(-1, 3);
        Assert.Equal(2, result);
    }
}

/// <summary>
/// Email validation tests - Demonstrates naming conventions for validation logic
/// </summary>
public class EmailValidatorTests
{
    // Valid input test
    [Fact]
    public void IsValidEmail_InputValidEmail_ReturnsTrue()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail("user@example.com");
        Assert.True(result);
    }

    // Invalid input - null
    [Fact]
    public void IsValidEmail_InputNullValue_ReturnsFalse()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail(null!);
        Assert.False(result);
    }

    // Invalid input - empty string
    [Fact]
    public void IsValidEmail_InputEmptyString_ReturnsFalse()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail("");
        Assert.False(result);
    }

    // Invalid format
    [Fact]
    public void IsValidEmail_InputInvalidEmailFormat_ReturnsFalse()
    {
        var validator = new EmailValidator();
        var result = validator.IsValidEmail("not-an-email");
        Assert.False(result);
    }
}

/// <summary>
/// Order processing tests - Demonstrates naming conventions for business logic and exceptions
/// </summary>
public class OrderServiceTests
{
    // Normal processing flow
    [Fact]
    public void ProcessOrder_InputValidOrder_ReturnsProcessedOrder()
    {
        // Arrange & Act & Assert
    }

    // Exception scenario
    [Fact]
    public void ProcessOrder_InputNull_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
    }

    // Calculation logic
    [Fact]
    public void Calculate_Input100And10PercentDiscount_Returns90()
    {
        // Arrange & Act & Assert
    }

    // State change
    [Fact]
    public void Cancel_CompletedOrder_ThrowsInvalidOperationException()
    {
        // Arrange & Act & Assert
    }
}

// =============================================================================
// Bad naming examples (DO NOT imitate)
// =============================================================================

// public void TestAdd() { }              // Lacks scenario and expected result
// public void Test1() { }                // Meaningless name
// public void EmailTest() { }            // Lacks three-part structure
// public void OrderTest() { }            // Cannot determine test purpose

// =============================================================================
// Common scenario vocabulary reference
// =============================================================================
// Happy path: Valid, Correct, Normal, Success
// Boundary conditions: MinValue, MaxValue, EmptyCollection, Zero
// Error scenarios: Null, EmptyString, InvalidFormat, OutOfRange
// State changes: FromXToY, InitialState, Completed, Cancelled
// Expected results: ShouldReturn, ShouldThrow, ShouldContain, ShouldBeEmpty, ShouldBeEqual
