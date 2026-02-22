using Xunit;

namespace MyProject.Tests;

/// <summary>
/// Basic unit test template - Following FIRST principles and 3A Pattern
/// </summary>
public class BasicTestTemplate
{
    // -------------------------------------------------------------------------
    // [Fact] Single test case template
    // -------------------------------------------------------------------------

    [Fact]
    public void MethodName_ScenarioDescription_ExpectedBehavior()
    {
        // Arrange - Prepare test data and dependencies
        // var sut = new SystemUnderTest();
        // const int input = 1;
        // const int expected = 2;

        // Act - Execute the method under test
        // var result = sut.Method(input);

        // Assert - Verify the result matches expectations
        // Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // Happy path test template
    // -------------------------------------------------------------------------

    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();
        const int a = 1;
        const int b = 2;
        const int expected = 3;

        // Act
        var result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // Boundary condition test template
    // -------------------------------------------------------------------------

    [Fact]
    public void Add_InputZero_ReturnsOtherNumber()
    {
        // Arrange
        var calculator = new Calculator();
        const int a = 0;
        const int b = 5;
        const int expected = 5;

        // Act
        var result = calculator.Add(a, b);

        // Assert
        Assert.Equal(expected, result);
    }

    // -------------------------------------------------------------------------
    // Invalid input test template
    // -------------------------------------------------------------------------

    [Fact]
    public void IsValid_InputNull_ReturnsFalse()
    {
        // Arrange
        var validator = new Validator();
        string? input = null;

        // Act
        var result = validator.IsValid(input);

        // Assert
        Assert.False(result);
    }

    // -------------------------------------------------------------------------
    // Exception test template
    // -------------------------------------------------------------------------

    [Fact]
    public void Divide_DivisorIsZero_ThrowsDivideByZeroException()
    {
        // Arrange
        var calculator = new Calculator();
        const decimal dividend = 10m;
        const decimal divisor = 0m;

        // Act & Assert
        var exception = Assert.Throws<DivideByZeroException>(
            () => calculator.Divide(dividend, divisor)
        );

        Assert.Equal("Divisor cannot be zero", exception.Message);
    }

    // -------------------------------------------------------------------------
    // Example classes (for template reference only)
    // -------------------------------------------------------------------------

    private class Calculator
    {
        public int Add(int a, int b) => a + b;

        public decimal Divide(decimal dividend, decimal divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException("Divisor cannot be zero");
            return dividend / divisor;
        }
    }

    private class Validator
    {
        public bool IsValid(string? input) => !string.IsNullOrWhiteSpace(input);
    }
}
