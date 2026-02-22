// AutoFixture and Bogus Integration - Hybrid Generator and Extensions
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Bogus;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace AutoFixtureBogusIntegration.Templates;

// Unified test data generator interface
public interface ITestDataGenerator
{
    T Generate<T>();
    IEnumerable<T> Generate<T>(int count);
    T Generate<T>(Action<T> configure);
    IEnumerable<T> Generate<T>(int count, Action<T> configure);
}

// Hybrid test data generator combining AutoFixture and Bogus
public class HybridTestDataGenerator : ITestDataGenerator
{
    private readonly Fixture _fixture;
    private readonly Dictionary<Type, object> _registeredFakers;

    public HybridTestDataGenerator()
    {
        _fixture = new Fixture();
        _registeredFakers = new Dictionary<Type, object>();
        ConfigureDefaults();
    }

    private void ConfigureDefaults()
    {
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customizations.Add(new EmailSpecimenBuilder());
        _fixture.Customizations.Add(new PhoneSpecimenBuilder());
        _fixture.Customizations.Add(new NameSpecimenBuilder());
        _fixture.Customizations.Add(new AddressSpecimenBuilder());
        _fixture.Customizations.Add(new WebsiteSpecimenBuilder());
    }

    public HybridTestDataGenerator WithFaker<T>(Faker<T> faker) where T : class
    {
        _registeredFakers[typeof(T)] = faker;
        _fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return this;
    }

    public HybridTestDataGenerator WithSeed(int seed)
    {
        Randomizer.Seed = new Random(seed);
        return this;
    }

    public HybridTestDataGenerator WithRepeatCount(int count)
    {
        _fixture.RepeatCount = count;
        return this;
    }

    public T Generate<T>() => _fixture.Create<T>();
    public IEnumerable<T> Generate<T>(int count) => _fixture.CreateMany<T>(count);
    public T Generate<T>(Action<T> configure)
    {
        var instance = _fixture.Create<T>();
        configure(instance);
        return instance;
    }
    public IEnumerable<T> Generate<T>(int count, Action<T> configure)
    {
        return _fixture.CreateMany<T>(count).Select(item => { configure(item); return item; });
    }
}

// Fixture extension methods
public static class FixtureExtensions
{
    public static Fixture WithBogus(this Fixture fixture)
    {
        fixture.WithOmitOnRecursion();
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        fixture.Customizations.Add(new PhoneSpecimenBuilder());
        fixture.Customizations.Add(new NameSpecimenBuilder());
        fixture.Customizations.Add(new AddressSpecimenBuilder());
        fixture.Customizations.Add(new WebsiteSpecimenBuilder());
        fixture.Customizations.Add(new CompanyNameSpecimenBuilder());
        fixture.Customizations.Add(new IndustrySpecimenBuilder());
        fixture.Customizations.Add(new ProductSpecimenBuilder());
        return fixture;
    }

    public static Fixture WithOmitOnRecursion(this Fixture fixture, int recursionDepth = 1)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth));
        return fixture;
    }

    public static Fixture WithSeed(this Fixture fixture, int seed)
    {
        Randomizer.Seed = new Random(seed);
        return fixture;
    }

    public static Fixture WithRepeatCount(this Fixture fixture, int count)
    {
        fixture.RepeatCount = count;
        return fixture;
    }

    public static Fixture WithBogusFor<T>(this Fixture fixture, Faker<T> faker) where T : class
    {
        fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return fixture;
    }

    public static Fixture WithBogusFor<T>(this Fixture fixture, Action<Faker<T>> configure) where T : class
    {
        var faker = new Faker<T>();
        configure(faker);
        fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return fixture;
    }

    public static Fixture WithSpecimenBuilder(this Fixture fixture, ISpecimenBuilder builder)
    {
        fixture.Customizations.Add(builder);
        return fixture;
    }

    public static Fixture WithLocale(this Fixture fixture, string locale)
    {
        fixture.Customizations.Add(new LocalizedSpecimenBuilder(locale));
        return fixture;
    }
}

// BogusAutoData attribute
public class BogusAutoDataAttribute : AutoDataAttribute
{
    public BogusAutoDataAttribute() : base(() => CreateFixture()) { }
    private static Fixture CreateFixture() => new Fixture().WithBogus();
}

// Localized BogusAutoData attribute
public class LocalizedBogusAutoDataAttribute : AutoDataAttribute
{
    public LocalizedBogusAutoDataAttribute(string locale = "en") : base(() => CreateFixture(locale)) { }
    private static Fixture CreateFixture(string locale)
    {
        return new Fixture()
            .WithOmitOnRecursion()
            .WithLocale(locale);
    }
}

// Seeded BogusAutoData attribute
public class SeededBogusAutoDataAttribute : AutoDataAttribute
{
    public SeededBogusAutoDataAttribute(int seed) : base(() => CreateFixture(seed)) { }
    private static Fixture CreateFixture(int seed)
    {
        return new Fixture()
            .WithBogus()
            .WithSeed(seed);
    }
}

// Test base class
public abstract class BogusTestBase
{
    protected readonly Fixture Fixture;
    protected readonly ITestDataGenerator Generator;

    protected BogusTestBase()
    {
        Fixture = new Fixture().WithBogus();
        Generator = new HybridTestDataGenerator();
    }

    protected T Create<T>() => Fixture.Create<T>();
    protected IEnumerable<T> CreateMany<T>(int count = 3) => Fixture.CreateMany<T>(count);
    protected T Create<T>(Action<T> configure)
    {
        var instance = Fixture.Create<T>();
        configure(instance);
        return instance;
    }
}

// Seeded test base class
public abstract class SeededBogusTestBase : BogusTestBase
{
    protected SeededBogusTestBase(int seed = 12345)
    {
        Randomizer.Seed = new Random(seed);
    }
}

// Tests
public class HybridTestDataGeneratorTests
{
    [Fact]
    public void Generate_ShouldCreateSingleObject()
    {
        var generator = new HybridTestDataGenerator();
        var user = generator.Generate<User>();
        user.Should().NotBeNull();
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generate_MultipleObjects_ShouldCreateSpecifiedCount()
    {
        var generator = new HybridTestDataGenerator();
        var users = generator.Generate<User>(5).ToList();
        users.Should().HaveCount(5);
        users.Select(u => u.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Generate_WithCustomConfiguration_ShouldApplyCorrectly()
    {
        var generator = new HybridTestDataGenerator();
        var user = generator.Generate<User>(u => { u.Age = 25; u.FirstName = "TestUser"; });
        user.Age.Should().Be(25);
        user.FirstName.Should().Be("TestUser");
    }

    [Fact]
    public void WithFaker_ShouldUseCustomFaker()
    {
        var customFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, _ => "Custom Product")
            .RuleFor(p => p.Price, _ => 99.99m);

        var generator = new HybridTestDataGenerator().WithFaker(customFaker);
        var product = generator.Generate<Product>();
        product.Name.Should().Be("Custom Product");
        product.Price.Should().Be(99.99m);
    }
}

public class FixtureExtensionsTests
{
    [Fact]
    public void WithBogus_ShouldAddAllDefaultSpecimenBuilders()
    {
        var fixture = new Fixture().WithBogus();
        var user = fixture.Create<User>();
        var company = fixture.Create<Company>();

        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
        company.Name.Should().NotBeNullOrEmpty();
        company.Website.Should().StartWith("http");
    }

    [Fact]
    public void WithBogusFor_ShouldRegisterCustomFaker()
    {
        var customProductFaker = new Faker<Product>()
            .RuleFor(p => p.Name, _ => "Fixed Product Name")
            .RuleFor(p => p.Price, _ => 100m);

        var fixture = new Fixture()
            .WithOmitOnRecursion()
            .WithBogusFor(customProductFaker);

        var product = fixture.Create<Product>();
        product.Name.Should().Be("Fixed Product Name");
        product.Price.Should().Be(100m);
    }

    [Fact]
    public void WithLocale_ShouldGenerateLocalizedData()
    {
        var fixture = new Fixture()
            .WithOmitOnRecursion()
            .WithLocale("zh_TW");
        var user = fixture.Create<User>();
        user.FirstName.Should().NotBeNullOrEmpty();
    }
}

public class BogusAutoDataAttributeTests
{
    [Theory]
    [BogusAutoData]
    public void BogusAutoData_ShouldAutoInjectBogusGeneratedData(User user)
    {
        user.Should().NotBeNull();
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
        user.LastName.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [SeededBogusAutoData(12345)]
    public void SeededBogusAutoData_ShouldUseFixedSeed(User user)
    {
        user.Should().NotBeNull();
    }
}

public class BogusTestBaseTests : BogusTestBase
{
    [Fact]
    public void Create_ShouldUseIntegratedFixture()
    {
        var user = Create<User>();
        user.Email.Should().Contain("@");
        user.FirstName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateMany_ShouldCreateSpecifiedCount()
    {
        var users = CreateMany<User>(5).ToList();
        users.Should().HaveCount(5);
    }
}
