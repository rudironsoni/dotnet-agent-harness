namespace MyApp.Tests;

/// <summary>
/// TUnit assertion system complete examples
/// Demonstrates various assertion type usage
/// </summary>
public class AssertionExamplesTests
{
    #region Basic Equality Assertions

    [Test]
    public async Task BasicEqualityAssertionExamples()
    {
        // Numeric equality check
        var expected = 42;
        var actual = 40 + 2;
        await Assert.That(actual).IsEqualTo(expected);
        await Assert.That(actual).IsNotEqualTo(43);

        // Object equality check
        var obj1 = new object();
        var obj2 = obj1;
        await Assert.That(obj2).IsEqualTo(obj1);
    }

    [Test]
    public async Task NullValueCheckExamples()
    {
        string? nullValue = null;
        string notNullValue = "test";

        await Assert.That(nullValue).IsNull();
        await Assert.That(notNullValue).IsNotNull();
    }

    #endregion

    #region Boolean Assertions

    [Test]
    public async Task BooleanAssertionExamples()
    {
        // Basic boolean check
        var condition1 = 1 + 1 == 2;
        var condition2 = 1 + 1 == 3;
        await Assert.That(condition1).IsTrue();
        await Assert.That(condition2).IsFalse();

        // Conditional boolean check
        var number = 10;
        await Assert.That(number > 5).IsTrue();
        await Assert.That(number < 5).IsFalse();
    }

    #endregion

    #region Numeric Comparison Assertions

    [Test]
    public async Task NumericComparisonAssertionExamples()
    {
        var actualValue = 5 + 5;  // 10
        var compareValue = 3 + 2; // 5
        var equalValue = 4 + 6;   // 10

        // Basic numeric comparison
        await Assert.That(actualValue).IsGreaterThan(compareValue);
        await Assert.That(actualValue).IsGreaterThanOrEqualTo(equalValue);
        await Assert.That(compareValue).IsLessThan(actualValue);
        await Assert.That(compareValue).IsLessThanOrEqualTo(compareValue);

        // Numeric range check
        await Assert.That(actualValue).IsBetween(5, 15);
    }

    [Test]
    [Arguments(3.14159, 3.14, 0.01)]
    [Arguments(1.0001, 1.0, 0.001)]
    [Arguments(99.999, 100.0, 0.01)]
    public async Task FloatingPointPrecisionControl(double actual, double expected, double tolerance)
    {
        // Floating point comparison with tolerance
        await Assert.That(actual)
            .IsEqualTo(expected)
            .Within(tolerance);
    }

    #endregion

    #region String Assertions

    [Test]
    public async Task StringAssertionExamples()
    {
        var email = "user@example.com";
        var emptyString = "";

        // Contains check
        await Assert.That(email).Contains("@");
        await Assert.That(email).Contains("example");
        await Assert.That(email).DoesNotContain(" ");

        // Start/End check
        await Assert.That(email).StartsWith("user");
        await Assert.That(email).EndsWith(".com");

        // Empty string check
        await Assert.That(emptyString).IsEmpty();
        await Assert.That(email).IsNotEmpty();

        // String length check
        await Assert.That(email.Length).IsGreaterThan(5);
        await Assert.That(email.Length).IsEqualTo(16);
    }

    #endregion

    #region Collection Assertions

    [Test]
    public async Task CollectionAssertionExamples()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5 };
        var emptyList = new List<string>();

        // Collection count check
        await Assert.That(numbers).HasCount(5);
        await Assert.That(emptyList).IsEmpty();
        await Assert.That(numbers).IsNotEmpty();

        // Element contains check
        await Assert.That(numbers).Contains(3);
        await Assert.That(numbers).DoesNotContain(10);

        // Collection position check
        await Assert.That(numbers.First()).IsEqualTo(1);
        await Assert.That(numbers.Last()).IsEqualTo(5);
        await Assert.That(numbers[2]).IsEqualTo(3);

        // Collection all check
        await Assert.That(numbers.All(x => x > 0)).IsTrue();
        await Assert.That(numbers.Any(x => x > 3)).IsTrue();
    }

    #endregion

    #region Exception Assertions

    [Test]
    public async Task ExceptionAssertionExamples()
    {
        var calculator = new Calculator();

        // Check specific exception type
        await Assert.That(() => calculator.Divide(10, 0))
            .Throws<DivideByZeroException>();

        // Check exception message
        await Assert.That(() => calculator.Divide(10, 0))
            .Throws<DivideByZeroException>()
            .WithMessage("Divisor cannot be zero");

        // Check no exception thrown
        await Assert.That(() => calculator.Add(1, 2))
            .DoesNotThrow();
    }

    #endregion

    #region And Condition Combination

    [Test]
    public async Task AndConditionCombinationExamples()
    {
        var number = 10;

        // Combine multiple conditions - all must pass
        await Assert.That(number)
            .IsGreaterThan(5)
            .And.IsLessThan(15)
            .And.IsEqualTo(10);

        var email = "test@example.com";
        await Assert.That(email)
            .Contains("@")
            .And.EndsWith(".com")
            .And.StartsWith("test");
    }

    #endregion

    #region Or Condition Combination

    [Test]
    public async Task OrConditionCombinationExamples()
    {
        var number = 15;

        // Any condition passing will pass the test
        await Assert.That(number)
            .IsEqualTo(10)
            .Or.IsEqualTo(15)
            .Or.IsEqualTo(20);

        var text = "Hello World";
        await Assert.That(text)
            .StartsWith("Hi")
            .Or.StartsWith("Hello")
            .Or.StartsWith("Hey");
    }

    [Test]
    public async Task OrConditionPracticalExamples()
    {
        var email = "admin@company.com";

        // Check if admin or test account
        await Assert.That(email)
            .StartsWith("admin@")
            .Or.StartsWith("test@")
            .Or.Contains("@localhost");

        var httpStatusCode = 200;

        // Check if successful HTTP status code
        await Assert.That(httpStatusCode)
            .IsEqualTo(200)  // OK
            .Or.IsEqualTo(201)  // Created
            .Or.IsEqualTo(204); // No Content
    }

    #endregion
}
