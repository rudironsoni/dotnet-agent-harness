// =============================================================================
// Bogus Advanced Patterns and Custom Extensions
// Demonstrates complex business logic, custom DataSet, performance optimization, test integration
// =============================================================================

using Bogus;
using FluentAssertions;
using Xunit;

namespace BogusAdvanced.Templates;

#region Test Model Classes

// =============================================================================
// Test Model Classes
// =============================================================================

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Level { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public string Department { get; set; } = string.Empty;
    public bool IsManager { get; set; }
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> Technologies { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class TaiwanPerson
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IdCard { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
}

public class GlobalUser
{
    public Guid Id { get; set; }
    public string Locale { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class TestBoundaryData
{
    public string? NullableString { get; set; }
    public string ShortString { get; set; } = string.Empty;
    public string LongString { get; set; } = string.Empty;
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public int ZeroValue { get; set; }
    public int NegativeValue { get; set; }
    public int PositiveValue { get; set; }
    public string SpecialChars { get; set; } = string.Empty;
    public DateTime MinDate { get; set; }
    public DateTime MaxDate { get; set; }
}

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}

#endregion

#region Complex Business Logic Constraints

// =============================================================================
// Complex Business Logic Constraints
// =============================================================================

public class ComplexBusinessLogicExamples
{
    /// <summary>
    /// Employee data generation with complex business logic
    /// </summary>
    [Fact]
    public void Employee_ComplexBusinessLogicConstraints()
    {
        var employeeFaker = new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            // Generate Email based on name
            .RuleFor(e => e.Email, (f, e) =>
                f.Internet.Email(e.FirstName, e.LastName, "company.com"))
            // Age range constraint
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
            })
            // Hire date constraint (within age - 22 years)
            .RuleFor(e => e.HireDate, (f, e) =>
            {
                var maxYearsAgo = Math.Max(1, e.Age - 22);
                return f.Date.Past(maxYearsAgo);
            })
            // Generate skills list
            .RuleFor(e => e.Skills, f =>
            {
                var allSkills = new[]
                {
                    "C#", ".NET", "JavaScript", "TypeScript", "React", "Angular", "Vue",
                    "SQL Server", "PostgreSQL", "MongoDB", "Redis",
                    "Azure", "AWS", "Docker", "Kubernetes", "Git"
                };
                return f.PickRandom(allSkills, f.Random.Int(2, 6)).ToList();
            })
            // Department selection
            .RuleFor(e => e.Department, f =>
                f.PickRandom("Engineering", "Product", "Design", "QA", "DevOps"))
            // 30% chance of Manager for Lead and above
            .RuleFor(e => e.IsManager, (f, e) =>
                (e.Level == "Lead" || e.Level == "Principal") && f.Random.Bool(0.3f))
            // Generate project experience
            .RuleFor(e => e.Projects, (f, e) =>
            {
                var projectFaker = new Faker<Project>()
                    .RuleFor(p => p.Id, f => f.Random.Guid())
                    .RuleFor(p => p.Name, f => f.Company.CatchPhrase())
                    .RuleFor(p => p.Description, f => f.Lorem.Sentence())
                    .RuleFor(p => p.StartDate, f => f.Date.Between(e.HireDate, DateTime.Now.AddMonths(-1)))
                    .RuleFor(p => p.EndDate, (f, p) =>
                        f.Random.Bool(0.8f) ? f.Date.Between(p.StartDate, DateTime.Now) : null)
                    .RuleFor(p => p.Status, (f, p) =>
                        p.EndDate.HasValue ? "Completed" : f.PickRandom("In Progress", "On Hold"))
                    .RuleFor(p => p.Technologies, f =>
                        f.PickRandom(e.Skills, f.Random.Int(1, Math.Min(3, e.Skills.Count))).ToList());

                var yearsOfExperience = (DateTime.Now - e.HireDate).Days / 365;
                var projectCount = Math.Max(1, yearsOfExperience / 2);
                return projectFaker.Generate(f.Random.Int(1, projectCount));
            });

        // Generate employee
        var employee = employeeFaker.Generate();

        // Verify business logic constraints
        employee.Age.Should().BeInRange(22, 65);
        employee.Email.Should().EndWith("@company.com");
        employee.HireDate.Should().BeBefore(DateTime.Now);
        employee.Skills.Should().HaveCountGreaterOrEqualTo(2);

        // Verify level and salary correspondence
        if (employee.Level == "Junior")
        {
            employee.Salary.Should().BeInRange(35000, 50000);
        }
    }

    /// <summary>
    /// Generate organization data with hierarchical relationships
    /// </summary>
    [Fact]
    public void Organization_HierarchicalRelationships()
    {
        var departments = new[] { "Engineering", "Product", "Design", "QA", "DevOps" };

        var managerFaker = new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.Age, f => f.Random.Int(35, 55))
            .RuleFor(e => e.Level, _ => "Lead")
            .RuleFor(e => e.IsManager, _ => true)
            .RuleFor(e => e.Salary, f => f.Random.Decimal(100000, 150000));

        var employeeFaker = new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.Age, f => f.Random.Int(22, 40))
            .RuleFor(e => e.Level, f => f.PickRandom("Junior", "Senior"))
            .RuleFor(e => e.IsManager, _ => false)
            .RuleFor(e => e.Salary, (f, e) => e.Level == "Junior"
                ? f.Random.Decimal(35000, 50000)
                : f.Random.Decimal(50000, 80000));

        // One manager per department, 3-8 employees
        var organization = departments.Select(dept =>
        {
            var manager = managerFaker.Generate();
            manager.Department = dept;

            var teamSize = new Faker().Random.Int(3, 8);
            var team = employeeFaker.Generate(teamSize);
            team.ForEach(e => e.Department = dept);

            return new { Department = dept, Manager = manager, Team = team };
        }).ToList();

        organization.Should().HaveCount(5);
        organization.All(o => o.Manager.IsManager).Should().BeTrue();
        organization.All(o => o.Team.All(e => !e.IsManager)).Should().BeTrue();
    }
}

#endregion

#region Custom DataSet Extensions

// =============================================================================
// Custom DataSet Extensions
// =============================================================================

/// <summary>
/// Taiwan data generator extension methods
/// </summary>
public static class TaiwanDataSetExtensions
{
    private static readonly string[] TaiwanCities =
    {
        "Taipei City", "New Taipei City", "Taoyuan City", "Taichung City", "Tainan City", "Kaohsiung City",
        "Keelung City", "Hsinchu City", "Chiayi City", "Yilan County", "Hsinchu County", "Miaoli County",
        "Changhua County", "Nantou County", "Yunlin County", "Chiayi County", "Pingtung County", "Taitung County",
        "Hualien County", "Penghu County", "Kinmen County", "Lienchiang County"
    };

    private static readonly string[] TaiwanDistricts =
    {
        "Zhongzheng District", "Datong District", "Zhongshan District", "Songshan District", "Daan District", "Wanhua District",
        "Xinyi District", "Shilin District", "Beitou District", "Neihu District", "Nangang District", "Wenshan District"
    };

    private static readonly string[] TaiwanUniversities =
    {
        "National Taiwan University", "National Tsing Hua University", "National Chiao Tung University", "National Cheng Kung University", "National Sun Yat-sen University",
        "National Chengchi University", "National Central University", "National Chung Cheng University", "National Chung Hsing University", "National Taiwan Normal University",
        "National Taipei University of Technology", "National Taiwan University of Science and Technology", "National Kaohsiung University of Science and Technology"
    };

    private static readonly string[] TaiwanCompanies =
    {
        "TSMC", "Hon Hai/Foxconn", "MediaTek", "Chunghwa Telecom", "Formosa Plastics", "Uni-President",
        "Fubon Group", "CTBC", "Cathay", "Far EasTone", "ASUS", "Acer", "Quanta"
    };

    /// <summary>
    /// Generate Taiwan city name
    /// </summary>
    public static string TaiwanCity(this Faker faker)
        => faker.PickRandom(TaiwanCities);

    /// <summary>
    /// Generate Taiwan district name
    /// </summary>
    public static string TaiwanDistrict(this Faker faker)
        => faker.PickRandom(TaiwanDistricts);

    /// <summary>
    /// Generate Taiwan university name
    /// </summary>
    public static string TaiwanUniversity(this Faker faker)
        => faker.PickRandom(TaiwanUniversities);

    /// <summary>
    /// Generate Taiwan company name
    /// </summary>
    public static string TaiwanCompany(this Faker faker)
        => faker.PickRandom(TaiwanCompanies);

    /// <summary>
    /// Generate Taiwan ID card number (correct format but not actually valid)
    /// </summary>
    public static string TaiwanIdCard(this Faker faker)
    {
        // First character: English letter (corresponding to city)
        var firstChar = faker.PickRandom("ABCDEFGHJKLMNPQRSTUVXYWZIO".ToCharArray());
        // Second digit: 1=Male, 2=Female
        var genderDigit = faker.Random.Int(1, 2);
        // Last 8 digits: random numbers
        var digits = faker.Random.String2(8, "0123456789");
        return $"{firstChar}{genderDigit}{digits}";
    }

    /// <summary>
    /// Generate Taiwan mobile phone number
    /// </summary>
    public static string TaiwanMobilePhone(this Faker faker)
    {
        // Format: 09XX-XXX-XXX
        var thirdDigit = faker.Random.Int(0, 9);
        var fourthDigit = faker.Random.Int(0, 9);
        var middle = faker.Random.String2(3, "0123456789");
        var suffix = faker.Random.String2(3, "0123456789");
        return $"09{thirdDigit}{fourthDigit}-{middle}-{suffix}";
    }

    /// <summary>
    /// Generate Taiwan landline phone number
    /// </summary>
    public static string TaiwanLandlinePhone(this Faker faker)
    {
        // Format: (02) XXXX-XXXX or (04) XXX-XXXX
        var areaCodes = new[] { "02", "03", "04", "05", "06", "07", "08" };
        var areaCode = faker.PickRandom(areaCodes);

        var part1 = areaCode == "02"
            ? faker.Random.String2(4, "0123456789")
            : faker.Random.String2(3, "0123456789");
        var part2 = faker.Random.String2(4, "0123456789");

        return $"({areaCode}) {part1}-{part2}";
    }

    /// <summary>
    /// Generate Taiwan full address
    /// </summary>
    public static string TaiwanFullAddress(this Faker faker)
    {
        var city = faker.TaiwanCity();
        var district = faker.TaiwanDistrict();
        var road = faker.PickRandom("Zhongzheng Road", "Zhongshan Road", "Minsheng Road", "Zhongxiao Road", "Fuxing Road", "Jianguo Road");
        var number = faker.Random.Int(1, 500);
        var floor = faker.Random.Int(1, 20);

        return $"{city}{district}{road} No. {number}, {floor}F";
    }
}

public class TaiwanDataSetTests
{
    /// <summary>
    /// Use Taiwan custom DataSet
    /// </summary>
    [Fact]
    public void TaiwanDataSet_FullUsage()
    {
        var taiwanPersonFaker = new Faker<TaiwanPerson>("zh_TW")
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Person.FullName)
            .RuleFor(p => p.IdCard, f => f.TaiwanIdCard())
            .RuleFor(p => p.City, f => f.TaiwanCity())
            .RuleFor(p => p.District, f => f.TaiwanDistrict())
            .RuleFor(p => p.Address, f => f.TaiwanFullAddress())
            .RuleFor(p => p.Mobile, f => f.TaiwanMobilePhone())
            .RuleFor(p => p.Company, f => f.TaiwanCompany())
            .RuleFor(p => p.University, f => f.TaiwanUniversity());

        var person = taiwanPersonFaker.Generate();

        person.IdCard.Should().HaveLength(10);
        person.Mobile.Should().StartWith("09");
        person.Address.Should().NotBeNullOrEmpty();
    }
}

#endregion

#region Multi-language Advanced Application

// =============================================================================
// Multi-language Advanced Application
// =============================================================================

public class MultiLanguageAdvancedExamples
{
    /// <summary>
    /// Dynamic locale selection to generate international user data
    /// </summary>
    [Fact]
    public void GlobalUser_MultiLanguageDynamicGeneration()
    {
        var locales = new[] { "en_US", "zh_TW", "ja", "ko", "fr", "de" };

        var globalUserFaker = new Faker<GlobalUser>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            // First randomly select locale
            .RuleFor(u => u.Locale, f => f.PickRandom(locales))
            // Generate corresponding data based on locale
            .RuleFor(u => u.Name, (f, u) =>
            {
                var localFaker = new Faker(u.Locale);
                return localFaker.Person.FullName;
            })
            .RuleFor(u => u.Address, (f, u) =>
            {
                var localFaker = new Faker(u.Locale);
                return localFaker.Address.FullAddress();
            })
            .RuleFor(u => u.Phone, (f, u) =>
            {
                var localFaker = new Faker(u.Locale);
                return localFaker.Phone.PhoneNumber();
            });

        var users = globalUserFaker.Generate(10);

        users.Should().HaveCount(10);
        users.All(u => !string.IsNullOrEmpty(u.Name)).Should().BeTrue();
    }
}

#endregion

#region Boundary Test Data Generation

// =============================================================================
// Boundary Test Data Generation
// =============================================================================

public class BoundaryTestExamples
{
    /// <summary>
    /// Generate various boundary test data
    /// </summary>
    [Fact]
    public void BoundaryTest_BoundaryValueGeneration()
    {
        var boundaryFaker = new Faker<TestBoundaryData>()
            // String boundaries
            .RuleFor(t => t.NullableString, f => f.PickRandom<string?>(null, "", " ", "valid"))
            .RuleFor(t => t.ShortString, f => f.Random.String2(1, 10))
            .RuleFor(t => t.LongString, f => f.Random.String2(255, 1000))
            // Numeric boundaries
            .RuleFor(t => t.MinValue, _ => int.MinValue)
            .RuleFor(t => t.MaxValue, _ => int.MaxValue)
            .RuleFor(t => t.ZeroValue, _ => 0)
            .RuleFor(t => t.NegativeValue, f => f.Random.Int(int.MinValue, -1))
            .RuleFor(t => t.PositiveValue, f => f.Random.Int(1, int.MaxValue))
            // Special characters
            .RuleFor(t => t.SpecialChars, f => f.PickRandom(
                "!@#$%^&*()",
                "<script>alert('xss')</script>",
                "Chinese characters",
                "Japanese test",
                "Korean test",
                "emoji: smile party fire"))
            // Date boundaries
            .RuleFor(t => t.MinDate, _ => DateTime.MinValue)
            .RuleFor(t => t.MaxDate, _ => DateTime.MaxValue);

        var boundaryData = boundaryFaker.Generate();

        boundaryData.MinValue.Should().Be(int.MinValue);
        boundaryData.MaxValue.Should().Be(int.MaxValue);
    }
}

#endregion

#region Performance Optimization

// =============================================================================
// Performance Optimization
// =============================================================================

/// <summary>
/// Optimized data generator
/// </summary>
public static class OptimizedDataGenerator
{
    // Static Faker instance to avoid repeated creation
    private static readonly Faker _faker = new();

    // Pre-compiled Faker<T>
    private static readonly Faker<User> _userFaker = CreateUserFaker();
    private static readonly Faker<Product> _productFaker = CreateProductFaker();

    private static Faker<User> CreateUserFaker()
    {
        return new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80));
    }

    private static Faker<Product> CreateProductFaker()
    {
        return new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
            .RuleFor(p => p.Category, f => f.Commerce.Department());
    }

    /// <summary>
    /// Generate users (using pre-compiled Faker)
    /// </summary>
    public static List<User> GenerateUsers(int count)
        => _userFaker.Generate(count);

    /// <summary>
    /// Generate products (using pre-compiled Faker)
    /// </summary>
    public static List<Product> GenerateProducts(int count)
        => _productFaker.Generate(count);

    /// <summary>
    /// Batch generate users (using yield return to reduce memory)
    /// </summary>
    public static IEnumerable<User> GenerateUsersBatch(int totalCount, int batchSize = 1000)
    {
        var generated = 0;
        while (generated < totalCount)
        {
            var currentBatchSize = Math.Min(batchSize, totalCount - generated);
            var batch = _userFaker.Generate(currentBatchSize);

            foreach (var user in batch)
            {
                yield return user;
            }

            generated += currentBatchSize;
        }
    }
}

/// <summary>
/// Faker using Lazy initialization
/// </summary>
public static class LazyFakerProvider
{
    // Lazy initialization, created only when first used
    private static readonly Lazy<Faker<Employee>> _employeeFaker =
        new(() => CreateEmployeeFaker(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static Faker<Employee> CreateEmployeeFaker()
    {
        return new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Person.FirstName)
            .RuleFor(e => e.LastName, f => f.Person.LastName)
            .RuleFor(e => e.Email, f => f.Internet.Email())
            .RuleFor(e => e.Age, f => f.Random.Int(22, 65))
            .RuleFor(e => e.Level, f => f.PickRandom("Junior", "Senior", "Lead", "Principal"))
            .RuleFor(e => e.Salary, f => f.Random.Decimal(35000, 200000));
    }

    public static Employee GenerateEmployee()
        => _employeeFaker.Value.Generate();

    public static List<Employee> GenerateEmployees(int count)
        => _employeeFaker.Value.Generate(count);
}

public class PerformanceOptimizationTests
{
    /// <summary>
    /// Use optimized generator
    /// </summary>
    [Fact]
    public void OptimizedGenerator_LargeDataGeneration()
    {
        // Act - Use pre-compiled Faker
        var users = OptimizedDataGenerator.GenerateUsers(1000);
        var products = OptimizedDataGenerator.GenerateProducts(500);

        // Assert
        users.Should().HaveCount(1000);
        products.Should().HaveCount(500);
    }

    /// <summary>
    /// Use batch generation
    /// </summary>
    [Fact]
    public void BatchGeneration_ReduceMemoryUsage()
    {
        // Act - Batch generation using yield return
        var users = OptimizedDataGenerator.GenerateUsersBatch(10000, 500);

        // Only take first 100, won't generate all 10000
        var sample = users.Take(100).ToList();

        // Assert
        sample.Should().HaveCount(100);
    }

    /// <summary>
    /// Use Lazy initialization
    /// </summary>
    [Fact]
    public void LazyInitialization_DelayedLoading()
    {
        // Act - Use Lazy initialized Faker
        var employees = LazyFakerProvider.GenerateEmployees(50);

        // Assert
        employees.Should().HaveCount(50);
        employees.All(e => e.Age >= 22 && e.Age <= 65).Should().BeTrue();
    }
}

#endregion

#region Test Integration Examples

// =============================================================================
// Test Integration Examples
// =============================================================================

// Simulated service interface
public interface IEmailService
{
    string GenerateWelcomeEmail(User user);
}

// Simulated service implementation
public class EmailService : IEmailService
{
    public string GenerateWelcomeEmail(User user)
    {
        return $"Dear {user.Name},\n\nWelcome to our service!\n\nYour registered email: {user.Email}";
    }
}

public class EmailServiceTests
{
    /// <summary>
    /// Use Bogus to generate realistic test data for service testing
    /// </summary>
    [Fact]
    public void GenerateWelcomeEmail_UsingBogusGeneratedTestData()
    {
        // Arrange
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var user = userFaker.Generate();
        var emailService = new EmailService();

        // Act
        var emailContent = emailService.GenerateWelcomeEmail(user);

        // Assert
        emailContent.Should().Contain(user.Name);
        emailContent.Should().Contain(user.Email);
        emailContent.Should().Contain("Welcome");
    }

    /// <summary>
    /// Use Seed to ensure test reproducibility
    /// </summary>
    [Fact]
    public void GenerateWelcomeEmail_UsingSeedToEnsureReproducibility()
    {
        // Arrange - Set seed
        Randomizer.Seed = new Random(42);

        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var user = userFaker.Generate();
        var expectedName = user.Name;  // Record generated name

        // Reset seed
        Randomizer.Seed = new Random(42);
        var user2 = userFaker.Generate();

        // Assert - Same seed produces same data
        user2.Name.Should().Be(expectedName);

        // Cleanup
        Randomizer.Seed = new Random();
    }
}

/// <summary>
/// Database seeding example
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Generate database seed data
    /// </summary>
    public static List<User> GenerateSeedUsers(int count = 100)
    {
        // Set seed to ensure same data is generated each time
        Randomizer.Seed = new Random(42);

        var userFaker = new Faker<User>("zh_TW")
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 70));

        var users = userFaker.Generate(count);

        // Reset seed
        Randomizer.Seed = new Random();

        return users;
    }

    /// <summary>
    /// Generate product seed data
    /// </summary>
    public static List<Product> GenerateSeedProducts(int count = 50)
    {
        Randomizer.Seed = new Random(42);

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Decimal(100, 10000))
            .RuleFor(p => p.Category, f => f.Commerce.Department());

        var products = productFaker.Generate(count);

        Randomizer.Seed = new Random();

        return products;
    }
}

#endregion
