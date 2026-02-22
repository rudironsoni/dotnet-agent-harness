namespace MyApp.Tests;

/// <summary>
/// TUnit basic test examples
/// Demonstrates [Test] attribute, async assertions, parameterized tests
/// </summary>
public class CalculatorTests
{
    private readonly Calculator _calculator;

    public CalculatorTests()
    {
        _calculator = new Calculator();
    }

    #region Basic Tests

    /// <summary>
    /// Basic [Test] test method
    /// Note: TUnit test methods must be async Task
    /// </summary>
    [Test]
    public async Task Add_Input1And2_ShouldReturn3()
    {
        // Arrange
        int a = 1;
        int b = 2;
        int expected = 3;

        // Act
        var result = _calculator.Add(a, b);

        // Assert - using fluent assertions, must include await
        await Assert.That(result).IsEqualTo(expected);
    }

    /// <summary>
    /// Exception test example
    /// </summary>
    [Test]
    public async Task Divide_Input0AsDivisor_ShouldThrowDivideByZeroException()
    {
        // Arrange
        int dividend = 10;
        int divisor = 0;

        // Act & Assert
        await Assert.That(() => _calculator.Divide(dividend, divisor))
            .Throws<DivideByZeroException>();
    }

    /// <summary>
    /// Exception message validation
    /// </summary>
    [Test]
    public async Task Divide_Input0AsDivisor_ShouldContainCorrectErrorMessage()
    {
        // Arrange
        int dividend = 10;
        int divisor = 0;

        // Act & Assert
        await Assert.That(() => _calculator.Divide(dividend, divisor))
            .Throws<DivideByZeroException>()
            .WithMessage("Divisor cannot be zero");
    }

    #endregion

    #region Parameterized Tests

    /// <summary>
    /// Parameterized test example
    /// TUnit uses [Arguments] instead of xUnit's [InlineData]
    /// </summary>
    [Test]
    [Arguments(1, 2, 3)]
    [Arguments(-1, 1, 0)]
    [Arguments(0, 0, 0)]
    [Arguments(100, -50, 50)]
    [Arguments(int.MaxValue, 0, int.MaxValue)]
    public async Task Add_MultipleInputs_ShouldReturnCorrectResult(int a, int b, int expected)
    {
        // Act
        var result = _calculator.Add(a, b);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    /// <summary>
    /// Boolean parameterized test
    /// </summary>
    [Test]
    [Arguments(1, true)]
    [Arguments(-1, false)]
    [Arguments(0, false)]
    [Arguments(100, true)]
    [Arguments(-999, false)]
    public async Task IsPositive_VariousNumbers_ShouldReturnCorrectResult(int number, bool expected)
    {
        // Act
        var result = _calculator.IsPositive(number);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }

    /// <summary>
    /// Floating point comparison (with tolerance)
    /// </summary>
    [Test]
    [Arguments(3.14159, 3.14, 0.01)]
    [Arguments(1.0001, 1.0, 0.001)]
    [Arguments(99.999, 100.0, 0.01)]
    public async Task FloatingPointComparison_ShouldAllowTolerance(double actual, double expected, double tolerance)
    {
        await Assert.That(actual)
            .IsEqualTo(expected)
            .Within(tolerance);
    }

    #endregion
}

/// <summary>
/// Calculator class to be tested
/// </summary>
public class Calculator
{
    public int Add(int a, int b) => a + b;

    public double Divide(int dividend, int divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("Divisor cannot be zero");
        }
        return (double)dividend / divisor;
    }

    public bool IsPositive(int number) => number > 0;
}
