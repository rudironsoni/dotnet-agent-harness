# AutoFixture Practical Application Scenarios: Complete Code Examples

> This document is extracted from SKILL.md `## Practical Application Scenarios`, containing complete code examples for Entity testing, DTO validation, and large data testing.

## Entity Testing

```csharp
[Theory]
[InlineData(0, CustomerLevel.Bronze)]
[InlineData(15000, CustomerLevel.Silver)]
[InlineData(60000, CustomerLevel.Gold)]
[InlineData(120000, CustomerLevel.Diamond)]
public void GetLevel_DifferentSpendingAmounts_ShouldReturnCorrectLevel(decimal totalSpent, CustomerLevel expected)
{
    var fixture = new Fixture();
    var customer = fixture.Build<Customer>()
        .With(x => x.TotalSpent, totalSpent)
        .Create();

    var level = customer.GetLevel();

    level.Should().Be(expected);
}
```

## DTO Validation

```csharp
[Fact]
public void ValidateRequest_ValidData_ShouldPassValidation()
{
    var fixture = new Fixture();
    
    var request = fixture.Build<CreateCustomerRequest>()
        .With(x => x.Name, fixture.Create<string>()[..50])
        .With(x => x.Email, fixture.Create<MailAddress>().Address)
        .With(x => x.Age, Random.Shared.Next(18, 78))
        .Create();

    var context = new ValidationContext(request);
    var results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(request, context, results, true);

    isValid.Should().BeTrue();
}
```

## Large Data Testing

```csharp
[Fact]
public void ProcessBatch_LargeData_ShouldProcessCorrectly()
{
    var fixture = new Fixture();
    var records = fixture.CreateMany<DataRecord>(1000).ToList();
    var processor = new DataProcessor();

    var stopwatch = Stopwatch.StartNew();
    var result = processor.ProcessBatch(records);
    stopwatch.Stop();

    result.ProcessedCount.Should().Be(1000);
    result.ErrorCount.Should().Be(0);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
}
```
