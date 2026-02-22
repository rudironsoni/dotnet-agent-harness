// AutoFixture and Bogus Integration - Integrated Factory and Test Scenarios
using AutoFixture;
using AutoFixture.Kernel;
using Bogus;
using FluentAssertions;
using System.Collections.Concurrent;
using System.Reflection;
using Xunit;

namespace AutoFixtureBogusIntegration.Templates;

// Integrated test data factory with caching and scenario creation
public class IntegratedTestDataFactory : IDisposable
{
    private readonly Fixture _fixture;
    private readonly ConcurrentDictionary<Type, object> _cache;
    private readonly Dictionary<string, object> _namedCache;
    private readonly object _cacheLock = new();

    public Fixture Fixture => _fixture;

    public IntegratedTestDataFactory()
    {
        _fixture = new Fixture();
        _cache = new ConcurrentDictionary<Type, object>();
        _namedCache = new Dictionary<string, object>();
        ConfigureFixture();
    }

    private void ConfigureFixture()
    {
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customizations.Add(new EmailSpecimenBuilder());
        _fixture.Customizations.Add(new PhoneSpecimenBuilder());
        _fixture.Customizations.Add(new NameSpecimenBuilder());
        _fixture.Customizations.Add(new AddressSpecimenBuilder());
        _fixture.Customizations.Add(new WebsiteSpecimenBuilder());
        _fixture.Customizations.Add(new CompanyNameSpecimenBuilder());
        _fixture.Customizations.Add(new IndustrySpecimenBuilder());
        _fixture.Customizations.Add(new ProductSpecimenBuilder());
    }

    public T CreateFresh<T>() => _fixture.Create<T>();
    public IEnumerable<T> CreateMany<T>(int count = 3) => _fixture.CreateMany<T>(count);
    public T CreateFresh<T>(Action<T> configure)
    {
        var instance = _fixture.Create<T>();
        configure(instance);
        return instance;
    }

    public T GetCached<T>() where T : class =>
        (T)_cache.GetOrAdd(typeof(T), _ => _fixture.Create<T>());

    public T GetCached<T>(string name) where T : class
    {
        var key = $"{typeof(T).FullName}:{name}";
        lock (_cacheLock)
        {
            if (!_namedCache.TryGetValue(key, out var cached))
            {
                cached = _fixture.Create<T>();
                _namedCache[key] = cached;
            }
            return (T)cached;
        }
    }

    public T GetCached<T>(string name, Action<T> configure) where T : class
    {
        var key = $"{typeof(T).FullName}:{name}";
        lock (_cacheLock)
        {
            if (!_namedCache.TryGetValue(key, out var cached))
            {
                cached = _fixture.Create<T>();
                configure((T)cached);
                _namedCache[key] = cached;
            }
            return (T)cached;
        }
    }

    public void ClearCache()
    {
        _cache.Clear();
        lock (_cacheLock) { _namedCache.Clear(); }
    }

    public void ClearCache<T>() where T : class
    {
        _cache.TryRemove(typeof(T), out _);
        lock (_cacheLock)
        {
            var keysToRemove = _namedCache.Keys.Where(k => k.StartsWith($"{typeof(T).FullName}:")).ToList();
            foreach (var key in keysToRemove) { _namedCache.Remove(key); }
        }
    }

    public TestScenario CreateTestScenario() => new TestScenario(this);

    public CompleteTestScenario CreateCompleteScenario()
    {
        var user = CreateFresh<User>(u => { u.HomeAddress = CreateFresh<Address>(); });
        var company = CreateFresh<Company>(c => { c.Address = CreateFresh<Address>(); });
        user.Company = company;
        var products = CreateMany<Product>(5).ToList();
        var order = CreateFresh<Order>(o =>
        {
            o.Customer = user;
            o.Items = products.Take(3).Select(p => new OrderItem
            {
                Id = Guid.NewGuid(),
                Product = p,
                Quantity = new Random().Next(1, 5),
                UnitPrice = p.Price
            }).ToList();
            o.TotalAmount = o.Items.Sum(i => i.TotalPrice);
            o.Status = OrderStatus.Pending;
        });
        return new CompleteTestScenario { User = user, Company = company, Products = products, Order = order };
    }

    public IntegratedTestDataFactory WithFaker<T>(Faker<T> faker) where T : class
    {
        _fixture.Customizations.Add(new TypedBogusSpecimenBuilder<T>(faker));
        return this;
    }

    public IntegratedTestDataFactory WithSeed(int seed)
    {
        Randomizer.Seed = new Random(seed);
        return this;
    }

    public IntegratedTestDataFactory WithRepeatCount(int count)
    {
        _fixture.RepeatCount = count;
        return this;
    }

    public void Dispose() => ClearCache();
}

// Test scenario builder with fluent API
public class TestScenario
{
    private readonly IntegratedTestDataFactory _factory;
    private readonly Dictionary<string, object> _entities;

    public TestScenario(IntegratedTestDataFactory factory)
    {
        _factory = factory;
        _entities = new Dictionary<string, object>();
    }

    public TestScenario WithUser(string name = "DefaultUser", Action<User>? configure = null)
    {
        var user = _factory.CreateFresh<User>(u =>
        {
            u.HomeAddress = _factory.CreateFresh<Address>();
            configure?.Invoke(u);
        });
        _entities[name] = user;
        return this;
    }

    public TestScenario WithCompany(string name = "DefaultCompany", Action<Company>? configure = null)
    {
        var company = _factory.CreateFresh<Company>(c =>
        {
            c.Address = _factory.CreateFresh<Address>();
            configure?.Invoke(c);
        });
        _entities[name] = company;
        return this;
    }

    public TestScenario WithProduct(string name = "DefaultProduct", Action<Product>? configure = null)
    {
        var product = _factory.CreateFresh<Product>(configure ?? (_ => { }));
        _entities[name] = product;
        return this;
    }

    public TestScenario WithProducts(int count, string prefix = "Product")
    {
        var products = _factory.CreateMany<Product>(count).ToList();
        for (int i = 0; i < products.Count; i++) { _entities[$"{prefix}{i + 1}"] = products[i]; }
        _entities[$"{prefix}s"] = products;
        return this;
    }

    public TestScenario LinkUserToCompany(string userName, string companyName)
    {
        if (_entities.TryGetValue(userName, out var userObj) && _entities.TryGetValue(companyName, out var companyObj))
        {
            var user = userObj as User;
            var company = companyObj as Company;
            if (user != null && company != null)
            {
                user.Company = company;
                if (!company.Employees.Contains(user)) { company.Employees.Add(user); }
            }
        }
        return this;
    }

    public T Get<T>(string name) where T : class =>
        _entities.TryGetValue(name, out var entity) ? (entity as T)! : null!;

    public bool TryGet<T>(string name, out T? entity) where T : class
    {
        if (_entities.TryGetValue(name, out var obj) && obj is T typedEntity)
        {
            entity = typedEntity;
            return true;
        }
        entity = null;
        return false;
    }

    public IEnumerable<T> GetAll<T>() where T : class => _entities.Values.OfType<T>();
}

public class CompleteTestScenario
{
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public List<Product> Products { get; set; } = new();
    public Order Order { get; set; } = null!;
}

// Test base class
public abstract class IntegratedTestBase : IDisposable
{
    protected readonly IntegratedTestDataFactory Factory;
    protected readonly Fixture Fixture;

    protected IntegratedTestBase()
    {
        Factory = new IntegratedTestDataFactory();
        Fixture = Factory.Fixture;
    }

    protected T Create<T>() => Factory.CreateFresh<T>();
    protected IEnumerable<T> CreateMany<T>(int count = 3) => Factory.CreateMany<T>(count);
    protected T Create<T>(Action<T> configure) => Factory.CreateFresh(configure);
    protected T GetCached<T>() where T : class => Factory.GetCached<T>();
    protected T GetCached<T>(string name) where T : class => Factory.GetCached<T>(name);
    protected TestScenario CreateScenario() => Factory.CreateTestScenario();
    protected CompleteTestScenario CreateCompleteScenario() => Factory.CreateCompleteScenario();
    public virtual void Dispose() => Factory.Dispose();
}

// Tests
public class IntegratedTestDataFactoryTests : IDisposable
{
    private readonly IntegratedTestDataFactory _factory;
    public IntegratedTestDataFactoryTests() { _factory = new IntegratedTestDataFactory(); }

    [Fact]
    public void CreateFresh_ShouldCreateNewObjectsEachTime()
    {
        var user1 = _factory.CreateFresh<User>();
        var user2 = _factory.CreateFresh<User>();
        user1.Id.Should().NotBe(user2.Id);
        user1.Email.Should().NotBe(user2.Email);
    }

    [Fact]
    public void CreateMany_ShouldCreateSpecifiedCount()
    {
        var users = _factory.CreateMany<User>(5).ToList();
        users.Should().HaveCount(5);
        users.Select(u => u.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetCached_ShouldReturnSameObject()
    {
        var user1 = _factory.GetCached<User>();
        var user2 = _factory.GetCached<User>();
        user1.Should().BeSameAs(user2);
    }

    [Fact]
    public void GetCached_Named_DifferentNamesShouldReturnDifferentObjects()
    {
        var user1 = _factory.GetCached<User>("Admin");
        var user2 = _factory.GetCached<User>("Customer");
        user1.Should().NotBeSameAs(user2);
        user1.Id.Should().NotBe(user2.Id);
    }

    [Fact]
    public void CreateCompleteScenario_ShouldCreateCompleteTestScenario()
    {
        var scenario = _factory.CreateCompleteScenario();
        scenario.User.Should().NotBeNull();
        scenario.User.HomeAddress.Should().NotBeNull();
        scenario.User.Company.Should().BeSameAs(scenario.Company);
        scenario.Company.Should().NotBeNull();
        scenario.Products.Should().HaveCount(5);
        scenario.Order.Should().NotBeNull();
        scenario.Order.Customer.Should().BeSameAs(scenario.User);
    }

    public void Dispose() => _factory.Dispose();
}

public class TestScenarioTests : IDisposable
{
    private readonly IntegratedTestDataFactory _factory;
    public TestScenarioTests() { _factory = new IntegratedTestDataFactory(); }

    [Fact]
    public void WithUser_ShouldAddUserToScenario()
    {
        var scenario = _factory.CreateTestScenario().WithUser("TestUser");
        var user = scenario.Get<User>("TestUser");
        user.Should().NotBeNull();
        user.HomeAddress.Should().NotBeNull();
    }

    [Fact]
    public void LinkUserToCompany_ShouldCreateRelationship()
    {
        var scenario = _factory.CreateTestScenario()
            .WithUser("Employee")
            .WithCompany("TechCorp")
            .LinkUserToCompany("Employee", "TechCorp");

        var user = scenario.Get<User>("Employee");
        var company = scenario.Get<Company>("TechCorp");
        user.Company.Should().BeSameAs(company);
        company.Employees.Should().Contain(user);
    }

    public void Dispose() => _factory.Dispose();
}
