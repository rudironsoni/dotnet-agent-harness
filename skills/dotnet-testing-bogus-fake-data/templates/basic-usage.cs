// =============================================================================
// Bogus Basic Usage Examples
// Demonstrates Faker<T> basic syntax, RuleFor rule settings, and data generation methods
// =============================================================================

using Bogus;
using FluentAssertions;
using Xunit;

namespace BogusBasics.Templates;

#region Test Model Classes

// =============================================================================
// Test Model Classes
// =============================================================================

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsAvailable { get; set; }
}

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public bool IsPremium { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

#endregion

#region Basic Faker<T> Usage

// =============================================================================
// Basic Faker<T> Usage
// =============================================================================

public class BasicFakerUsageExamples
{
    /// <summary>
    /// Basic Faker usage
    /// </summary>
    [Fact]
    public void BasicFaker_GenerateSingleRecord()
    {
        // Arrange - Create Faker<T> and define rules
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)              // Incrementing index
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())   // Product name
            .RuleFor(p => p.Category, f => f.Commerce.Department()) // Category
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000)) // Price
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())   // Description
            .RuleFor(p => p.CreatedDate, f => f.Date.Past())        // Past date
            .RuleFor(p => p.IsAvailable, f => f.Random.Bool());     // Boolean

        // Act - Generate single record
        var product = productFaker.Generate();

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BeInRange(10, 1000);
        product.CreatedDate.Should().BeBefore(DateTime.Now);
    }

    /// <summary>
    /// Generate multiple records
    /// </summary>
    [Fact]
    public void BasicFaker_GenerateMultipleRecords()
    {
        // Arrange
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000));

        // Act - Generate 10 records
        var products = productFaker.Generate(10);

        // Assert
        products.Should().HaveCount(10);
        products.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// Difference between Generate and GenerateLazy
    /// </summary>
    [Fact]
    public void GenerateLazy_LazyGeneration()
    {
        // Arrange
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName());

        // Act - GenerateLazy returns IEnumerable, lazy execution
        IEnumerable<Product> lazyProducts = productFaker.GenerateLazy(100);

        // Only take first 5, won't generate all 100
        var firstFive = lazyProducts.Take(5).ToList();

        // Assert
        firstFive.Should().HaveCount(5);
    }
}

#endregion

#region RuleFor Rule Settings

// =============================================================================
// RuleFor Rule Settings
// =============================================================================

public class RuleForExamples
{
    /// <summary>
    /// Basic RuleFor using various data types
    /// </summary>
    [Fact]
    public void RuleFor_VariousDataTypes()
    {
        var customerFaker = new Faker<Customer>()
            // GUID
            .RuleFor(c => c.Id, f => f.Random.Guid())
            // String - Using Person DataSet
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // String - Using Internet DataSet
            .RuleFor(c => c.Email, f => f.Internet.Email())
            // String - Using Phone DataSet
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            // String - Using Address DataSet
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            // Date
            .RuleFor(c => c.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
            // Boolean
            .RuleFor(c => c.IsPremium, f => f.Random.Bool());

        var customer = customerFaker.Generate();

        customer.Id.Should().NotBe(Guid.Empty);
        customer.Email.Should().Contain("@");
    }

    /// <summary>
    /// RuleFor referencing other properties
    /// </summary>
    [Fact]
    public void RuleFor_ReferenceOtherProperties()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // Generate Email based on Name (using (f, c) parameters)
            .RuleFor(c => c.Email, (f, c) =>
            {
                var nameParts = c.Name.Split(' ');
                var firstName = nameParts.FirstOrDefault() ?? "user";
                var lastName = nameParts.LastOrDefault() ?? "name";
                return f.Internet.Email(firstName, lastName, "company.com");
            });

        var customer = customerFaker.Generate();

        customer.Email.Should().EndWith("@company.com");
    }

    /// <summary>
    /// Using IndexFaker to generate incrementing ID
    /// </summary>
    [Fact]
    public void IndexFaker_IncrementingId()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName());

        var products = productFaker.Generate(5);

        products[0].Id.Should().Be(0);
        products[1].Id.Should().Be(1);
        products[2].Id.Should().Be(2);
        products[3].Id.Should().Be(3);
        products[4].Id.Should().Be(4);
    }

    /// <summary>
    /// Using RuleFor to set nested objects
    /// </summary>
    [Fact]
    public void RuleFor_NestedObjects()
    {
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker)
            .RuleFor(o => o.CustomerId, f => f.Random.Guid())
            .RuleFor(o => o.CustomerName, f => f.Person.FullName)
            .RuleFor(o => o.OrderDate, f => f.Date.Past())
            .RuleFor(o => o.Status, f => f.PickRandom("Pending", "Processing", "Shipped", "Delivered"))
            // Generate 1-5 order items
            .RuleFor(o => o.Items, f =>
            {
                var itemFaker = new Faker<OrderItem>()
                    .RuleFor(i => i.Id, f => f.IndexFaker)
                    .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
                    .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
                    .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(10, 500));

                return itemFaker.Generate(f.Random.Int(1, 5));
            })
            // Calculate total amount
            .RuleFor(o => o.TotalAmount, (f, o) => o.Items.Sum(i => i.Subtotal));

        var order = orderFaker.Generate();

        order.Items.Should().HaveCountGreaterOrEqualTo(1);
        order.TotalAmount.Should().Be(order.Items.Sum(i => i.Subtotal));
    }
}

#endregion

#region Reproducibility Control (Seed)

// =============================================================================
// Reproducibility Control (Seed)
// =============================================================================

public class SeedExamples
{
    /// <summary>
    /// Use Seed to ensure reproducible test data
    /// </summary>
    [Fact]
    public void Seed_EnsureDataReproducibility()
    {
        // Arrange - Set same seed
        Randomizer.Seed = new Random(12345);

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 100));

        var products1 = productFaker.Generate(3);

        // Reset seed
        Randomizer.Seed = new Random(12345);
        var products2 = productFaker.Generate(3);

        // Assert - Same seed produces same data
        for (int i = 0; i < 3; i++)
        {
            products1[i].Name.Should().Be(products2[i].Name);
            products1[i].Price.Should().Be(products2[i].Price);
        }

        // Cleanup - Reset to random
        Randomizer.Seed = new Random();
    }

    /// <summary>
    /// Use UseSeed method to set single Faker seed
    /// </summary>
    [Fact]
    public void UseSeed_SetSingleFakerSeed()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .UseSeed(42);  // Set this Faker's seed

        var products1 = productFaker.Generate(3);

        // Recreate Faker with same seed
        var productFaker2 = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .UseSeed(42);

        var products2 = productFaker2.Generate(3);

        // Same result
        for (int i = 0; i < 3; i++)
        {
            products1[i].Name.Should().Be(products2[i].Name);
        }
    }
}

#endregion

#region Conditional Generation

// =============================================================================
// Conditional Generation
// =============================================================================

public class ConditionalGenerationExamples
{
    /// <summary>
    /// Use PickRandom to select from fixed options
    /// </summary>
    [Fact]
    public void PickRandom_FixedOptionsSelection()
    {
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Status, f => f.PickRandom("Pending", "Processing", "Shipped", "Delivered"));

        var orders = orderFaker.Generate(100);

        orders.All(o => new[] { "Pending", "Processing", "Shipped", "Delivered" }.Contains(o.Status))
            .Should().BeTrue();
    }

    /// <summary>
    /// Use PickRandomWeighted for weighted selection
    /// </summary>
    [Fact]
    public void PickRandomWeighted_WeightedSelection()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 70% User, 25% Admin, 5% SuperAdmin
            .RuleFor(c => c.Address, f => f.PickRandomWeighted(
                new[] { "User", "Admin", "SuperAdmin" },
                new[] { 0.7f, 0.25f, 0.05f }));

        var customers = customerFaker.Generate(1000);

        // Statistical distribution should be close to set weights
        var userCount = customers.Count(c => c.Address == "User");
        userCount.Should().BeGreaterThan(600); // About 70%
    }

    /// <summary>
    /// Use OrNull to generate potentially null values
    /// </summary>
    [Fact]
    public void OrNull_GeneratePotentiallyNullValues()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 50% chance of null
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber().OrNull(f, 0.5f));

        var customers = customerFaker.Generate(100);

        var nullCount = customers.Count(c => c.Phone == null);
        nullCount.Should().BeGreaterThan(30).And.BeLessThan(70); // About 50%
    }

    /// <summary>
    /// Use Bool to set probability
    /// </summary>
    [Fact]
    public void Bool_ProbabilityControl()
    {
        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Name, f => f.Person.FullName)
            // 20% chance of Premium
            .RuleFor(c => c.IsPremium, f => f.Random.Bool(0.2f));

        var customers = customerFaker.Generate(1000);

        var premiumCount = customers.Count(c => c.IsPremium);
        premiumCount.Should().BeGreaterThan(150).And.BeLessThan(250); // About 20%
    }
}

#endregion

#region Multi-language Support

// =============================================================================
// Multi-language Support
// =============================================================================

public class LocalizationExamples
{
    /// <summary>
    /// Use Traditional Chinese locale
    /// </summary>
    [Fact]
    public void Localization_TraditionalChinese()
    {
        // Use zh_TW locale
        var customerFaker = new Faker<Customer>("zh_TW")
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Person.FullName)
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber());

        var customer = customerFaker.Generate();

        customer.Name.Should().NotBeNullOrEmpty();
        customer.Address.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Use Japanese locale
    /// </summary>
    [Fact]
    public void Localization_Japanese()
    {
        var customerFaker = new Faker<Customer>("ja")
            .RuleFor(c => c.Name, f => f.Person.FullName)
            .RuleFor(c => c.Address, f => f.Address.FullAddress());

        var customer = customerFaker.Generate();

        customer.Name.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Dynamic locale selection
    /// </summary>
    [Fact]
    public void Localization_DynamicLocale()
    {
        var locales = new[] { "en_US", "zh_TW", "ja", "ko", "fr" };

        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Address, f => f.PickRandom(locales)) // Store locale
            .RuleFor(c => c.Name, (f, c) =>
            {
                var localFaker = new Faker(c.Address); // Use stored locale
                return localFaker.Person.FullName;
            });

        var customers = customerFaker.Generate(5);

        customers.Should().AllSatisfy(c => c.Name.Should().NotBeNullOrEmpty());
    }
}

#endregion
