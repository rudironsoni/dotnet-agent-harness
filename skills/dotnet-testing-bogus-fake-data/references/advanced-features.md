# Advanced Features

> This document is extracted from the "Advanced Features" chapter of [SKILL.md](../SKILL.md), providing complete code examples and detailed explanations.

---

## Reproducibility Control (Seed)

By setting a seed, ensure the same data sequence is generated each time:

```csharp
// Set global seed
Randomizer.Seed = new Random(12345);

var productFaker = new Faker<Product>()
    .RuleFor(p => p.Name, f => f.Commerce.ProductName());

// Generates the same product name sequence each execution
var products1 = productFaker.Generate(5);

// Reset seed and regenerate
Randomizer.Seed = new Random(12345);
var products2 = productFaker.Generate(5); // Same data

// Reset to random
Randomizer.Seed = new Random();
```

## Conditional Generation and Probability Control

```csharp
var userFaker = new Faker<User>()
    .RuleFor(u => u.Name, f => f.Person.FullName)
    // 80% chance of Premium membership
    .RuleFor(u => u.IsPremium, f => f.Random.Bool(0.8f))
    // OrNull: 50% chance of null
    .RuleFor(u => u.MiddleName, f => f.Name.FirstName().OrNull(f, 0.5f))
    // Random array element selection
    .RuleFor(u => u.Department, f => f.PickRandom("IT", "HR", "Finance", "Marketing"))
    // Weighted random selection
    .RuleFor(u => u.Role, f => f.PickRandomWeighted(
        new[] { "User", "Admin", "SuperAdmin" },
        new[] { 0.7f, 0.25f, 0.05f }));
```

## Related Data and Nested Objects

```csharp
// Generate order data with relationships
var orderFaker = new Faker<Order>()
    .RuleFor(o => o.Id, f => f.IndexFaker)
    .RuleFor(o => o.CustomerName, f => f.Person.FullName)
    .RuleFor(o => o.OrderDate, f => f.Date.Past())
    // Generate 1-5 order items
    .RuleFor(o => o.Items, f => 
    {
        var itemFaker = new Faker<OrderItem>()
            .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(i => i.UnitPrice, f => decimal.Parse(f.Commerce.Price(10, 100)));
        
        return itemFaker.Generate(f.Random.Int(1, 5));
    })
    // Calculate total amount (reference other properties)
    .RuleFor(o => o.TotalAmount, (f, o) => 
        o.Items.Sum(item => item.Quantity * item.UnitPrice));
```

## Complex Business Logic Constraints

```csharp
// Employee data generation with complex business logic
var employeeFaker = new Faker<Employee>()
    .RuleFor(e => e.Id, f => f.Random.Guid())
    .RuleFor(e => e.FirstName, f => f.Person.FirstName)
    .RuleFor(e => e.LastName, f => f.Person.LastName)
    // Generate Email based on name
    .RuleFor(e => e.Email, (f, e) => 
        f.Internet.Email(e.FirstName, e.LastName, "company.com"))
    // Age range limitation
    .RuleFor(e => e.Age, f => f.Random.Int(22, 65))
    // Determine level based on age
    .RuleFor(e => e.Level, (f, e) => e.Age switch
    {
        < 25 => "Junior",
        < 35 => "Senior",
        < 45 => "Lead",
        _ => "Principal"
    })
    // Determine salary range based on level
    .RuleFor(e => e.Salary, (f, e) => e.Level switch
    {
        "Junior" => f.Random.Decimal(35000, 50000),
        "Senior" => f.Random.Decimal(50000, 80000),
        "Lead" => f.Random.Decimal(80000, 120000),
        "Principal" => f.Random.Decimal(120000, 200000),
        _ => f.Random.Decimal(35000, 50000)
    });
```

## Custom DataSet Extension

```csharp
// Create custom Taiwan data generator
public static class TaiwanDataSetExtensions
{
    private static readonly string[] TaiwanCities = 
    {
        "Taipei City", "New Taipei City", "Taoyuan City", "Taichung City", 
        "Tainan City", "Kaohsiung City", "Keelung City", "Hsinchu City", 
        "Chiayi City", "Yilan County", "Hsinchu County", "Miaoli County"
    };
    
    private static readonly string[] TaiwanCompanies = 
    {
        "TSMC", "Foxconn", "MediaTek", "Chunghwa Telecom", "Formosa Plastics", "Uni-President"
    };
    
    public static string TaiwanCity(this Faker faker)
        => faker.PickRandom(TaiwanCities);
    
    public static string TaiwanCompany(this Faker faker)
        => faker.PickRandom(TaiwanCompanies);
    
    public static string TaiwanMobilePhone(this Faker faker)
    {
        var prefix = "09";
        var middle = faker.Random.Int(0, 9);
        var suffix = faker.Random.String2(7, "0123456789");
        return $"{prefix}{middle}{suffix}";
    }
    
    public static string TaiwanIdCard(this Faker faker)
    {
        var firstChar = faker.PickRandom("ABCDEFGHJKLMNPQRSTUVXYWZIO");
        var genderDigit = faker.Random.Int(1, 2);
        var digits = faker.Random.String2(8, "0123456789");
        return $"{firstChar}{genderDigit}{digits}";
    }
}

// Use custom extension
var taiwanPersonFaker = new Faker<TaiwanPerson>()
    .RuleFor(p => p.City, f => f.TaiwanCity())
    .RuleFor(p => p.Company, f => f.TaiwanCompany())
    .RuleFor(p => p.Mobile, f => f.TaiwanMobilePhone())
    .RuleFor(p => p.IdCard, f => f.TaiwanIdCard());
```
