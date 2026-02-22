# Core Integration Techniques

> This document is extracted from [SKILL.md](../SKILL.md) "Core Integration Techniques" chapter, containing complete SpecimenBuilder implementation examples and extension methods.

---

### 1. Property-Level SpecimenBuilder

Through the `ISpecimenBuilder` interface, decide whether to use Bogus based on property name:

```csharp
public class EmailSpecimenBuilder : ISpecimenBuilder
{
    private readonly Faker _faker = new();

    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo property && 
            property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase))
        {
            return _faker.Internet.Email();
        }
        
        return new NoSpecimen();
    }
}
```

**Common SpecimenBuilders**:

| Builder                    | Matching Condition          | Bogus Method                           |
| -------------------------- | --------------------------- | ------------------------------------ |
| EmailSpecimenBuilder       | Contains "Email"            | `Internet.Email()`                   |
| PhoneSpecimenBuilder       | Contains "Phone"            | `Phone.PhoneNumber()`                |
| NameSpecimenBuilder        | FirstName/LastName/FullName | `Person.FirstName/LastName/FullName` |
| AddressSpecimenBuilder     | Street/City/Postal/Country  | `Address.*`                          |
| WebsiteSpecimenBuilder     | Contains "Website"          | `Internet.Url()`                     |
| CompanyNameSpecimenBuilder | Company type Name           | `Company.CompanyName()`              |

### 2. Type-Level SpecimenBuilder

Create complete Bogus generators for entire types:

```csharp
public class BogusSpecimenBuilder : ISpecimenBuilder
{
    private readonly Dictionary<Type, object> _fakers;

    public BogusSpecimenBuilder()
    {
        _fakers = new Dictionary<Type, object>();
        RegisterFakers();
    }

    private void RegisterFakers()
    {
        _fakers[typeof(User)] = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName))
            .Ignore(u => u.Company); // Avoid circular references

        _fakers[typeof(Address)] = new Faker<Address>()
            .RuleFor(a => a.Street, f => f.Address.StreetAddress())
            .RuleFor(a => a.City, f => f.Address.City());
    }

    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && _fakers.TryGetValue(type, out var faker))
        {
            return GenerateWithFaker(faker);
        }
        return new NoSpecimen();
    }
}
```

### 3. Extension Methods Integration

```csharp
public static class FixtureExtensions
{
    /// <summary>
    /// Add Bogus integration functionality to AutoFixture
    /// </summary>
    public static IFixture WithBogus(this IFixture fixture)
    {
        // Handle circular references first
        fixture.WithOmitOnRecursion();

        // Add property-level integration
        fixture.Customizations.Add(new EmailSpecimenBuilder());
        fixture.Customizations.Add(new PhoneSpecimenBuilder());
        fixture.Customizations.Add(new NameSpecimenBuilder());
        fixture.Customizations.Add(new AddressSpecimenBuilder());

        // Add type-level integration
        fixture.Customizations.Add(new BogusSpecimenBuilder());

        return fixture;
    }

    /// <summary>
    /// Handle circular references
    /// </summary>
    public static IFixture WithOmitOnRecursion(this IFixture fixture)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    /// <summary>
    /// Set random seed
    /// </summary>
    public static IFixture WithSeed(this IFixture fixture, int seed)
    {
        Bogus.Randomizer.Seed = new Random(seed);
        return fixture;
    }
}
```
