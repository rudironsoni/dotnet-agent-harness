// AutoFixture Custom ISpecimenBuilder Examples
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Xunit;

namespace AutoFixtureCustomization.Templates;

// Test model classes
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
}

public class Order
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipDate { get; set; }
    public int Quantity { get; set; }
    public int Priority { get; set; }
}

// Custom DateTime range builder
public class RandomRangedDateTimeBuilder : ISpecimenBuilder
{
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly HashSet<string> _targetProperties;

    public RandomRangedDateTimeBuilder(DateTime minDate, DateTime maxDate, params string[] targetProperties)
    {
        _minDate = minDate;
        _maxDate = maxDate;
        _targetProperties = new HashSet<string>(targetProperties);
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo) return new NoSpecimen();
        if (propertyInfo.PropertyType != typeof(DateTime)) return new NoSpecimen();
        if (!_targetProperties.Contains(propertyInfo.Name)) return new NoSpecimen();
        return GenerateRandomDateTime();
    }

    private DateTime GenerateRandomDateTime()
    {
        var range = _maxDate - _minDate;
        var randomTicks = (long)(Random.Shared.NextDouble() * range.Ticks);
        return _minDate.AddTicks(randomTicks);
    }
}

// Simple numeric range builder
public class RandomRangedNumericSequenceBuilder : ISpecimenBuilder
{
    private readonly int _min;
    private readonly int _max;
    private readonly HashSet<string> _targetProperties;

    public RandomRangedNumericSequenceBuilder(int min, int max, params string[] targetProperties)
    {
        _min = min;
        _max = max;
        _targetProperties = new HashSet<string>(targetProperties);
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo) return new NoSpecimen();
        if (propertyInfo.PropertyType != typeof(int)) return new NoSpecimen();
        if (!_targetProperties.Contains(propertyInfo.Name)) return new NoSpecimen();
        return Random.Shared.Next(_min, _max);
    }
}

// Improved numeric range builder with Predicate
public class ImprovedRandomRangedNumericSequenceBuilder : ISpecimenBuilder
{
    private readonly int _min;
    private readonly int _max;
    private readonly Func<PropertyInfo, bool> _predicate;

    public ImprovedRandomRangedNumericSequenceBuilder(int min, int max, Func<PropertyInfo, bool> predicate)
    {
        _min = min;
        _max = max;
        _predicate = predicate;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo) return new NoSpecimen();
        if (propertyInfo.PropertyType != typeof(int)) return new NoSpecimen();
        if (!_predicate(propertyInfo)) return new NoSpecimen();
        return Random.Shared.Next(_min, _max);
    }
}

// Tests
public class CustomSpecimenBuilderTests
{
    [Fact]
    public void RandomRangedDateTimeBuilder_OnlyControlsSpecifiedProperties()
    {
        var fixture = new Fixture();
        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 12, 31);

        fixture.Customizations.Add(new RandomRangedDateTimeBuilder(minDate, maxDate, "UpdateTime"));
        var member = fixture.Create<Member>();

        member.UpdateTime.Should().BeOnOrAfter(minDate);
        member.UpdateTime.Should().BeOnOrBefore(maxDate);
        member.CreateTime.Should().NotBe(default);
    }

    [Fact]
    public void RandomRangedDateTimeBuilder_ControlsMultipleProperties()
    {
        var fixture = new Fixture();
        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 6, 30);

        fixture.Customizations.Add(new RandomRangedDateTimeBuilder(minDate, maxDate, "OrderDate", "ShipDate"));
        var orders = fixture.CreateMany<Order>(10).ToList();

        orders.Should().AllSatisfy(order =>
        {
            order.OrderDate.Should().BeOnOrAfter(minDate);
            order.OrderDate.Should().BeOnOrBefore(maxDate);
            order.ShipDate.Should().BeOnOrAfter(minDate);
            order.ShipDate.Should().BeOnOrBefore(maxDate);
        });
    }

    [Fact]
    public void UsingInsertAtZero_EnsuresHighestPriority()
    {
        var fixture = new Fixture();

        fixture.Customizations.Insert(0, new ImprovedRandomRangedNumericSequenceBuilder(
            30, 50, prop => prop.Name == "Age" && prop.DeclaringType == typeof(Member)));

        var members = fixture.CreateMany<Member>(20).ToList();
        members.Should().AllSatisfy(m => m.Age.Should().BeInRange(30, 49));
    }

    [Fact]
    public void UsingPredicate_PreciselyControlsMultipleProperties()
    {
        var fixture = new Fixture();

        fixture.Customizations.Insert(0, new ImprovedRandomRangedNumericSequenceBuilder(
            1, 100, prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Order)));
        fixture.Customizations.Insert(0, new ImprovedRandomRangedNumericSequenceBuilder(
            1, 5, prop => prop.Name == "Priority" && prop.DeclaringType == typeof(Order)));

        var orders = fixture.CreateMany<Order>(20).ToList();

        orders.Should().AllSatisfy(order =>
        {
            order.Quantity.Should().BeInRange(1, 99);
            order.Priority.Should().BeInRange(1, 4);
        });
    }

    [Fact]
    public void CombiningMultipleCustomBuilders()
    {
        var fixture = new Fixture();
        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 12, 31);

        fixture.Customizations.Add(new RandomRangedDateTimeBuilder(minDate, maxDate, "OrderDate"));
        fixture.Customizations.Insert(0, new ImprovedRandomRangedNumericSequenceBuilder(
            1, 100, prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Order)));

        var order = fixture.Create<Order>();

        order.OrderDate.Should().BeOnOrAfter(minDate);
        order.OrderDate.Should().BeOnOrBefore(maxDate);
        order.Quantity.Should().BeInRange(1, 99);
    }
}

// NoSpecimen explanation
public class NoSpecimenExplanation
{
    // Wrong example: Not returning NoSpecimen causes issues
    public class BadSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo propertyInfo && propertyInfo.Name == "Age")
                return 25;
            return null!; // Wrong: returning null causes other properties to become null
        }
    }

    // Correct example: Return NoSpecimen to continue the chain
    public class GoodSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo propertyInfo && propertyInfo.Name == "Age")
                return 25;
            return new NoSpecimen(); // Correct: Other builders can handle this
        }
    }
}
