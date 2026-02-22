// AutoFixture DataAnnotations Integration Examples
using System.ComponentModel.DataAnnotations;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AutoFixtureCustomization.Templates;

// Model classes with DataAnnotations
public class Person
{
    public Guid Id { get; set; }

    [StringLength(10)]
    public string Name { get; set; } = string.Empty;

    [Range(10, 80)]
    public int Age { get; set; }

    public DateTime CreateTime { get; set; }
}

public class Employee
{
    public Guid Id { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(18, 65)]
    public int Age { get; set; }

    [Range(typeof(decimal), "25000", "200000")]
    public decimal Salary { get; set; }

    [StringLength(100)]
    public string Department { get; set; } = string.Empty;
}

// Tests
public class DataAnnotationsIntegrationTests
{
    [Fact]
    public void AutoFixture_ShouldRecognizeDataAnnotations()
    {
        var fixture = new Fixture();
        var person = fixture.Create<Person>();

        person.Name.Length.Should().Be(10);        // StringLength(10) generates exactly 10 chars
        person.Age.Should().BeInRange(10, 80);     // Range(10, 80) generates values in range
        person.Id.Should().NotBeEmpty();
        person.CreateTime.Should().NotBe(default);
    }

    [Fact]
    public void AutoFixture_BatchCreation_AllShouldRespectConstraints()
    {
        var fixture = new Fixture();
        var persons = fixture.CreateMany<Person>(10).ToList();

        persons.Should().HaveCount(10);
        persons.Should().AllSatisfy(person =>
        {
            person.Name.Length.Should().Be(10);
            person.Age.Should().BeInRange(10, 80);
            person.Id.Should().NotBeEmpty();
        });
    }

    [Fact]
    public void AutoFixture_ShouldRecognizeComplexValidationRules()
    {
        var fixture = new Fixture();
        var employees = fixture.CreateMany<Employee>(5).ToList();

        employees.Should().AllSatisfy(employee =>
        {
            employee.Name.Length.Should().BeInRange(2, 50);
            employee.Age.Should().BeInRange(18, 65);
            employee.Salary.Should().BeInRange(25000m, 200000m);
            employee.Department.Length.Should().BeLessOrEqualTo(100);
        });
    }
}

// Member class for .With() method tests
public class Member
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime JoinDate { get; set; }
}

public class WithMethodTests
{
    [Fact]
    public void WithMethod_FixedValueVsDynamicValue_Difference()
    {
        var fixture = new Fixture();

        // Fixed value: Random.Shared.Next() only executes once, all objects same age
        var fixedAgeMembers = fixture.Build<Member>()
            .With(x => x.Age, Random.Shared.Next(30, 50))
            .CreateMany(5)
            .ToList();

        // Dynamic value: Lambda executes for each object
        var dynamicAgeMembers = fixture.Build<Member>()
            .With(x => x.Age, () => Random.Shared.Next(30, 50))
            .CreateMany(5)
            .ToList();

        fixedAgeMembers.Select(m => m.Age).Distinct().Count().Should().Be(1);
        dynamicAgeMembers.Select(m => m.Age).Distinct().Count().Should().BeGreaterThan(1);
    }

    [Fact]
    public void MultipleDynamicValueSettings()
    {
        var fixture = new Fixture();
        var baseDate = new DateTime(2025, 1, 1);

        var members = fixture.Build<Member>()
            .With(x => x.Age, () => Random.Shared.Next(20, 60))
            .With(x => x.JoinDate, () => baseDate.AddDays(Random.Shared.Next(0, 365)))
            .CreateMany(10)
            .ToList();

        members.Should().AllSatisfy(m =>
        {
            m.Age.Should().BeInRange(20, 59);
            m.JoinDate.Should().BeOnOrAfter(baseDate);
            m.JoinDate.Should().BeBefore(baseDate.AddDays(365));
        });
    }
}

public class RandomComparisonTests
{
    [Fact]
    public void NewRandom_MayProduceSameSequence()
    {
        // When created in quick succession, new Random() may use same seed
        var values = Enumerable.Range(0, 10)
            .Select(_ => Random.Shared.Next(100))
            .Distinct()
            .Count();

        values.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task RandomShared_IsThreadSafe()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() =>
                Enumerable.Range(0, 100).Select(_ => Random.Shared.Next(1000)).ToList()))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(list => list.Should().HaveCount(100));
    }
}
