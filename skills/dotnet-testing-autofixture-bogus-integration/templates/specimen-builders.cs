// AutoFixture with Bogus Integration - SpecimenBuilder Examples
using AutoFixture;
using AutoFixture.Kernel;
using Bogus;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace AutoFixtureBogusIntegration.Templates;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public Address? HomeAddress { get; set; }
    public Company? Company { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class Address
{
    public Guid Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public List<User> Employees { get; set; } = new();
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class Order
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public User? Customer { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}

public enum OrderStatus
{
    Pending, Processing, Shipped, Delivered, Cancelled
}

// Email SpecimenBuilder
public class EmailSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property &&
            property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType == typeof(string))
        {
            return _faker.Internet.Email();
        }
        return new NoSpecimen();
    }
}

// Phone SpecimenBuilder
public class PhoneSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property &&
            property.Name.Contains("Phone", StringComparison.OrdinalIgnoreCase) &&
            property.PropertyType == typeof(string))
        {
            return _faker.Phone.PhoneNumber();
        }
        return new NoSpecimen();
    }
}

// Name SpecimenBuilder
public class NameSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("firstname") => _faker.Person.FirstName,
                var name when name.Contains("lastname") => _faker.Person.LastName,
                var name when name == "fullname" => _faker.Person.FullName,
                _ => new NoSpecimen()
            };
        }
        return new NoSpecimen();
    }
}

// Address SpecimenBuilder
public class AddressSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("street") => _faker.Address.StreetAddress(),
                var name when name.Contains("city") => _faker.Address.City(),
                var name when name.Contains("postal") || name.Contains("zip") => _faker.Address.ZipCode(),
                var name when name.Contains("country") => _faker.Address.Country(),
                var name when name.Contains("state") || name.Contains("province") => _faker.Address.State(),
                _ => new NoSpecimen()
            };
        }
        return new NoSpecimen();
    }
}

// Website SpecimenBuilder
public class WebsiteSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property &&
            property.PropertyType == typeof(string) &&
            (property.Name.Contains("Website", StringComparison.OrdinalIgnoreCase) ||
             property.Name.Contains("Url", StringComparison.OrdinalIgnoreCase)))
        {
            return _faker.Internet.Url();
        }
        return new NoSpecimen();
    }
}

// Company Name SpecimenBuilder
public class CompanyNameSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property &&
            property.PropertyType == typeof(string) &&
            property.DeclaringType?.Name == "Company" &&
            property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
        {
            return _faker.Company.CompanyName();
        }
        return new NoSpecimen();
    }
}

// Product SpecimenBuilder
public class ProductSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            if (property.DeclaringType?.Name != "Product")
                return new NoSpecimen();

            return property.Name.ToLower() switch
            {
                "name" => _faker.Commerce.ProductName(),
                "description" => _faker.Commerce.ProductDescription(),
                "category" => _faker.Commerce.Department(),
                _ => new NoSpecimen()
            };
        }
        return new NoSpecimen();
    }
}

// Industry SpecimenBuilder
public class IndustrySpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property &&
            property.PropertyType == typeof(string) &&
            property.Name.Equals("Industry", StringComparison.OrdinalIgnoreCase))
        {
            return _faker.Commerce.Department();
        }
        return new NoSpecimen();
    }
}

// Bogus SpecimenBuilder
public class BogusSpecimenBuilder : ISpecimenBuilder
{
    private readonly Dictionary<Type, object> _fakers;

    public BogusSpecimenBuilder()
    {
        _fakers = new Dictionary<Type, object>();
        RegisterFakers();
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && _fakers.TryGetValue(type, out var faker))
        {
            return GenerateWithFaker(faker);
        }
        return new NoSpecimen();
    }

    private void RegisterFakers()
    {
        _fakers[typeof(User)] = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.LastName, f => f.Person.LastName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.BirthDate, f => f.Person.DateOfBirth)
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80))
            .Ignore(u => u.HomeAddress)
            .Ignore(u => u.Company)
            .Ignore(u => u.Orders);

        _fakers[typeof(Address)] = new Faker<Address>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.Street, f => f.Address.StreetAddress())
            .RuleFor(a => a.City, f => f.Address.City())
            .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
            .RuleFor(a => a.Country, f => f.Address.Country());

        _fakers[typeof(Company)] = new Faker<Company>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Industry, f => f.Commerce.Department())
            .RuleFor(c => c.Website, f => f.Internet.Url())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .Ignore(c => c.Address)
            .Ignore(c => c.Employees);

        _fakers[typeof(Product)] = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000))
            .RuleFor(p => p.Category, f => f.Commerce.Department())
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.8f));

        _fakers[typeof(OrderItem)] = new Faker<OrderItem>()
            .RuleFor(oi => oi.Id, f => f.Random.Guid())
            .RuleFor(oi => oi.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(oi => oi.UnitPrice, f => f.Random.Decimal(1, 500))
            .Ignore(oi => oi.Product);

        _fakers[typeof(Order)] = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.OrderDate, f => f.Date.Recent(30))
            .RuleFor(o => o.TotalAmount, f => f.Random.Decimal(10, 5000))
            .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>())
            .Ignore(o => o.Customer)
            .Ignore(o => o.Items);
    }

    private object GenerateWithFaker(object faker)
    {
        var generateMethod = faker.GetType().GetMethod("Generate", Type.EmptyTypes);
        return generateMethod?.Invoke(faker, null) ?? new NoSpecimen();
    }
}

// Generic Typed Bogus SpecimenBuilder
public class TypedBogusSpecimenBuilder<T> : ISpecimenBuilder where T : class
{
    private readonly Faker<T> _faker;

    public TypedBogusSpecimenBuilder(Faker<T> faker)
    {
        _faker = faker;
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(T))
        {
            return _faker.Generate();
        }
        return new NoSpecimen();
    }
}

// Taiwan Locale SpecimenBuilder
public class TaiwanSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new("zh_TW");

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("firstname") => _faker.Person.FirstName,
                var name when name.Contains("lastname") => _faker.Person.LastName,
                var name when name.Contains("city") => _faker.Address.City(),
                var name when name.Contains("street") => _faker.Address.StreetAddress(),
                _ => new NoSpecimen()
            };
        }
        return new NoSpecimen();
    }
}

// Localized SpecimenBuilder
public class LocalizedSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker;
    private readonly string _locale;

    public LocalizedSpecimenBuilder(string locale = "en")
    {
        _locale = locale;
        _faker = new Faker(locale);
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && property.PropertyType == typeof(string))
        {
            return property.Name.ToLower() switch
            {
                var name when name.Contains("email") => _faker.Internet.Email(),
                var name when name.Contains("phone") => _faker.Phone.PhoneNumber(),
                var name when name.Contains("firstname") => _faker.Person.FirstName,
                var name when name.Contains("lastname") => _faker.Person.LastName,
                var name when name.Contains("city") => _faker.Address.City(),
                var name when name.Contains("country") => _faker.Address.Country(),
                _ => new NoSpecimen()
            };
        }
        return new NoSpecimen();
    }
}

// Tests
public class SpecimenBuilderTests
{
    [Fact]
    public void EmailSpecimenBuilder_ShouldGenerateValidEmailFormat()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        var user = fixture.Create<User>();
        user.Email.Should().Contain("@");
        user.Email.Should().MatchRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$");
    }

    [Fact]
    public void NameSpecimenBuilder_ShouldGenerateRealNames()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new NameSpecimenBuilder());
        var user = fixture.Create<User>();
        user.FirstName.Should().NotBeNullOrEmpty();
        user.LastName.Should().NotBeNullOrEmpty();
        user.FirstName.Should().NotContain("FirstName");
    }

    [Fact]
    public void MultipleSpecimenBuilders_ShouldCombineCorrectly()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        fixture.Customizations.Add(new PhoneSpecimenBuilder());
        fixture.Customizations.Add(new NameSpecimenBuilder());
        fixture.Customizations.Add(new AddressSpecimenBuilder());

        var user = fixture.Create<User>();
        var address = fixture.Create<Address>();

        user.Email.Should().Contain("@");
        user.Phone.Should().MatchRegex("[\\d\\-\\(\\)\\s\\+\\.x]+");
        user.FirstName.Should().NotBeNullOrEmpty();
        address.City.Should().NotBeNullOrEmpty();
        address.Country.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BogusSpecimenBuilder_ShouldGenerateCompleteObjects()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new BogusSpecimenBuilder());
        var product = fixture.Create<Product>();

        product.Id.Should().NotBeEmpty();
        product.Name.Should().NotBeNullOrEmpty();
        product.Description.Should().NotBeNullOrEmpty();
        product.Price.Should().BeGreaterThan(0);
        product.Category.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TypedBogusSpecimenBuilder_ShouldUseCustomFaker()
    {
        var customUserFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, _ => "John")
            .RuleFor(u => u.LastName, _ => "Doe")
            .RuleFor(u => u.Email, _ => "john.doe@test.com")
            .RuleFor(u => u.Age, _ => 30);

        var fixture = new Fixture();
        fixture.Customizations.Add(new TypedBogusSpecimenBuilder<User>(customUserFaker));
        var user = fixture.Create<User>();

        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Email.Should().Be("john.doe@test.com");
        user.Age.Should().Be(30);
    }

    [Fact]
    public void LocalizedSpecimenBuilder_ShouldGenerateLocalizedData()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new LocalizedSpecimenBuilder("zh_TW"));
        var user = fixture.Create<User>();
        user.FirstName.Should().NotBeNullOrEmpty();
    }
}
