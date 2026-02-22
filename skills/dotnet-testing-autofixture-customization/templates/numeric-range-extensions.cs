// AutoFixture Generic Numeric Range Builder and Extensions
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Xunit;

namespace AutoFixtureCustomization.Templates;

// Test model
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public double Rating { get; set; }
    public float Discount { get; set; }
    public long ViewCount { get; set; }
    public short StockLevel { get; set; }
}

// Generic numeric range builder
public class NumericRangeBuilder<TValue> : ISpecimenBuilder
    where TValue : struct, IComparable, IConvertible
{
    private readonly TValue _min;
    private readonly TValue _max;
    private readonly Func<PropertyInfo, bool> _predicate;

    public NumericRangeBuilder(TValue min, TValue max, Func<PropertyInfo, bool> predicate)
    {
        _min = min;
        _max = max;
        _predicate = predicate;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo) return new NoSpecimen();
        if (propertyInfo.PropertyType != typeof(TValue)) return new NoSpecimen();
        if (!_predicate(propertyInfo)) return new NoSpecimen();
        return GenerateRandomValue();
    }

    private TValue GenerateRandomValue()
    {
        var minDecimal = Convert.ToDecimal(_min);
        var maxDecimal = Convert.ToDecimal(_max);
        var range = maxDecimal - minDecimal;
        var randomValue = minDecimal + (decimal)Random.Shared.NextDouble() * range;

        return typeof(TValue).Name switch
        {
            nameof(Int32) => (TValue)(object)(int)randomValue,
            nameof(Int64) => (TValue)(object)(long)randomValue,
            nameof(Int16) => (TValue)(object)(short)randomValue,
            nameof(Byte) => (TValue)(object)(byte)randomValue,
            nameof(Single) => (TValue)(object)(float)randomValue,
            nameof(Double) => (TValue)(object)(double)randomValue,
            nameof(Decimal) => (TValue)(object)randomValue,
            _ => throw new NotSupportedException($"Type {typeof(TValue).Name} is not supported")
        };
    }
}

// Fixture extensions for numeric ranges
public static class FixtureRangedNumericExtensions
{
    public static IFixture AddRandomRange<T, TValue>(this IFixture fixture, TValue min, TValue max, Func<PropertyInfo, bool> predicate)
        where TValue : struct, IComparable, IConvertible
    {
        fixture.Customizations.Insert(0, new NumericRangeBuilder<TValue>(min, max, predicate));
        return fixture;
    }

    public static IFixture AddRandomRange<T, TValue>(this IFixture fixture, TValue min, TValue max, string propertyName)
        where TValue : struct, IComparable, IConvertible
    {
        return fixture.AddRandomRange<T, TValue>(min, max,
            prop => prop.Name == propertyName && prop.DeclaringType == typeof(T));
    }
}

// DateTime range builder
public class DateTimeRangeBuilder : ISpecimenBuilder
{
    private readonly DateTime _minDate;
    private readonly DateTime _maxDate;
    private readonly Func<PropertyInfo, bool> _predicate;

    public DateTimeRangeBuilder(DateTime minDate, DateTime maxDate, Func<PropertyInfo, bool> predicate)
    {
        _minDate = minDate;
        _maxDate = maxDate;
        _predicate = predicate;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not PropertyInfo propertyInfo) return new NoSpecimen();
        if (propertyInfo.PropertyType != typeof(DateTime)) return new NoSpecimen();
        if (!_predicate(propertyInfo)) return new NoSpecimen();

        var range = _maxDate - _minDate;
        var randomTicks = (long)(Random.Shared.NextDouble() * range.Ticks);
        return _minDate.AddTicks(randomTicks);
    }
}

// DateTime extensions
public static class FixtureDateTimeExtensions
{
    public static IFixture AddDateTimeRange<T>(this IFixture fixture, DateTime minDate, DateTime maxDate, string propertyName)
    {
        fixture.Customizations.Add(new DateTimeRangeBuilder(minDate, maxDate,
            prop => prop.Name == propertyName && prop.DeclaringType == typeof(T)));
        return fixture;
    }
}

// Tests
public class NumericRangeExtensionTests
{
    [Fact]
    public void MultipleNumericTypeRangeControl()
    {
        var fixture = new Fixture();

        fixture
            .AddRandomRange<Product, decimal>(50m, 500m,
                prop => prop.Name == "Price" && prop.DeclaringType == typeof(Product))
            .AddRandomRange<Product, int>(1, 50,
                prop => prop.Name == "Quantity" && prop.DeclaringType == typeof(Product))
            .AddRandomRange<Product, double>(1.0, 5.0,
                prop => prop.Name == "Rating" && prop.DeclaringType == typeof(Product))
            .AddRandomRange<Product, float>(0.0f, 0.5f,
                prop => prop.Name == "Discount" && prop.DeclaringType == typeof(Product));

        var products = fixture.CreateMany<Product>(10).ToList();

        products.Should().AllSatisfy(product =>
        {
            product.Price.Should().BeInRange(50m, 500m);
            product.Quantity.Should().BeInRange(1, 49);
            product.Rating.Should().BeInRange(1.0, 5.0);
            product.Discount.Should().BeInRange(0.0f, 0.5f);
        });
    }

    [Fact]
    public void UsingPropertyNameSimplifiedSetup()
    {
        var fixture = new Fixture();

        fixture
            .AddRandomRange<Product, decimal>(100m, 1000m, "Price")
            .AddRandomRange<Product, int>(10, 100, "Quantity")
            .AddRandomRange<Product, long>(1000L, 100000L, "ViewCount")
            .AddRandomRange<Product, short>((short)1, (short)100, "StockLevel");

        var product = fixture.Create<Product>();

        product.Price.Should().BeInRange(100m, 1000m);
        product.Quantity.Should().BeInRange(10, 99);
        product.ViewCount.Should().BeInRange(1000L, 99999L);
        product.StockLevel.Should().BeInRange((short)1, (short)99);
    }

    [Fact]
    public void CombinedWithBuildMethod()
    {
        var fixture = new Fixture();

        fixture
            .AddRandomRange<Product, decimal>(50m, 200m, "Price")
            .AddRandomRange<Product, int>(1, 10, "Quantity");

        var product = fixture.Build<Product>()
            .With(x => x.Name, "Test Product")
            .With(x => x.Rating, () => Math.Round(Random.Shared.NextDouble() * 4 + 1, 1))
            .Create();

        product.Name.Should().Be("Test Product");
        product.Price.Should().BeInRange(50m, 200m);
        product.Quantity.Should().BeInRange(1, 9);
        product.Rating.Should().BeInRange(1.0, 5.0);
    }
}

// Order class for integration tests
public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ShipDate { get; set; }
}

public class IntegrationTests
{
    [Fact]
    public void CompleteIntegrationTest()
    {
        var fixture = new Fixture();
        var minDate = new DateTime(2025, 1, 1);
        var maxDate = new DateTime(2025, 12, 31);

        fixture
            .AddRandomRange<Order, decimal>(100m, 10000m, "TotalAmount")
            .AddRandomRange<Order, int>(1, 20, "ItemCount")
            .AddDateTimeRange<Order>(minDate, maxDate, "OrderDate")
            .AddDateTimeRange<Order>(minDate, maxDate, "ShipDate");

        var orders = fixture.CreateMany<Order>(50).ToList();

        orders.Should().HaveCount(50);
        orders.Should().AllSatisfy(order =>
        {
            order.Id.Should().NotBeEmpty();
            order.CustomerName.Should().NotBeNullOrEmpty();
            order.TotalAmount.Should().BeInRange(100m, 10000m);
            order.ItemCount.Should().BeInRange(1, 19);
            order.OrderDate.Should().BeOnOrAfter(minDate);
            order.OrderDate.Should().BeOnOrBefore(maxDate);
            order.ShipDate.Should().BeOnOrAfter(minDate);
            order.ShipDate.Should().BeOnOrBefore(maxDate);
        });
    }
}
