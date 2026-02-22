# External Test Data Integration

> This document is extracted from [SKILL.md](../SKILL.md), providing a complete guide for CSV/JSON external data integration with AutoData.

## Test Project File Configuration

Configure external data files in `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- CSV files -->
    <Content Include="TestData\*.csv">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    
    <!-- JSON files -->
    <Content Include="TestData\*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CsvHelper" Version="33.0.1" />
  </ItemGroup>
</Project>
```

## CSV File Integration

**TestData/products.csv**

```csv
ProductId,Name,Category,Price,IsAvailable
1,"iPhone 15","Electronics",35900,true
2,"MacBook Pro","Electronics",89900,true
3,"AirPods Pro","Electronics",7490,false
4,"Nike Air Max","Sports",4200,true
```

**CSV Reading and Integration**

```csharp
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

public class ExternalDataIntegrationTests
{
    public static IEnumerable<object[]> GetProductsFromCsv()
    {
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "products.csv");
        
        using var reader = new StreamReader(csvPath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };
        
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<ProductCsvRecord>().ToList();
        
        foreach (var record in records)
        {
            yield return new object[]
            {
                record.ProductId,
                record.Name,
                record.Category,
                record.Price,
                record.IsAvailable
            };
        }
    }

    [Theory]
    [MemberAutoData(nameof(GetProductsFromCsv))]
    public void CSVIntegrationTest_ProductValidation(
        int productId,
        string productName,
        string category,
        decimal price,
        bool isAvailable,
        Customer customer,
        Order order)
    {
        // Assert - CSV data
        productId.Should().BePositive();
        productName.Should().NotBeNullOrEmpty();
        category.Should().BeOneOf("Electronics", "Sports");
        price.Should().BePositive();

        // Assert - AutoFixture generated data
        customer.Should().NotBeNull();
        order.Should().NotBeNull();
    }
}

public class ProductCsvRecord
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}
```

## JSON File Integration

**TestData/customers.json**

```json
[
  {
    "customerId": 1,
    "name": "John",
    "email": "john@example.com",
    "type": "VIP",
    "creditLimit": 100000
  },
  {
    "customerId": 2,
    "name": "Jane",
    "email": "jane@example.com",
    "type": "Premium",
    "creditLimit": 50000
  }
]
```

**JSON Reading and Integration**

```csharp
using System.Text.Json;

public static IEnumerable<object[]> GetCustomersFromJson()
{
    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "customers.json");
    var jsonContent = File.ReadAllText(jsonPath);
    
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    var customers = JsonSerializer.Deserialize<List<CustomerJsonRecord>>(jsonContent, options)!;
    
    foreach (var customer in customers)
    {
        yield return new object[]
        {
            customer.CustomerId,
            customer.Name,
            customer.Email,
            customer.Type,
            customer.CreditLimit
        };
    }
}

[Theory]
[MemberAutoData(nameof(GetCustomersFromJson))]
public void JSONIntegrationTest_CustomerValidation(
    int customerId,
    string name,
    string email,
    string customerType,
    decimal creditLimit,
    Order order)
{
    // Assert - JSON data
    customerId.Should().BePositive();
    name.Should().NotBeNullOrEmpty();
    email.Should().Contain("@");
    customerType.Should().BeOneOf("VIP", "Premium", "Regular");
    creditLimit.Should().BePositive();

    // Assert - AutoFixture generated data
    order.Should().NotBeNull();
}
```
