// CSV/JSON External Data Integration Examples
using System.Globalization;
using System.Text.Json;
using AutoFixture.Xunit2;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Xunit;

namespace AutoDataXunitIntegration.Templates;

// Test model classes
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
}

public class Order
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// CSV record class
public class ProductCsvRecord
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

// JSON record class
public class CustomerJsonRecord
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
}

// CSV integration tests
public class CsvIntegrationTests
{
    public static IEnumerable<object[]> GetProductsFromCsv()
    {
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "products.csv");

        // Fallback data if file doesn't exist
        if (!File.Exists(csvPath))
        {
            yield return new object[] { 1, "iPhone 15", "Electronics", 35900m, true };
            yield return new object[] { 2, "MacBook Pro", "Electronics", 89900m, true };
            yield return new object[] { 3, "Nike Air Max", "Sports", 4200m, true };
            yield break;
        }

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
                record.ProductId, record.Name, record.Category, record.Price, record.IsAvailable
            };
        }
    }

    [Theory]
    [MemberAutoData(nameof(GetProductsFromCsv))]
    public void CsvIntegration_ProductValidation(
        int productId, string productName, string category, decimal price, bool isAvailable,
        Customer customer, Order order)
    {
        productId.Should().BePositive();
        productName.Should().NotBeNullOrEmpty();
        category.Should().NotBeNullOrEmpty();
        price.Should().BePositive();

        customer.Should().NotBeNull();
        order.Should().NotBeNull();
    }

    public static IEnumerable<object[]> GetElectronicsFromCsv()
    {
        return GetProductsFromCsv().Where(data => data[2].ToString() == "Electronics");
    }

    [Theory]
    [MemberAutoData(nameof(GetElectronicsFromCsv))]
    public void CsvIntegration_ElectronicsFilter(
        int productId, string productName, string category, decimal price, bool isAvailable, Order order)
    {
        category.Should().Be("Electronics");
        productId.Should().BePositive();
        price.Should().BeGreaterThan(1000m);
    }
}

// JSON integration tests
public class JsonIntegrationTests
{
    public static IEnumerable<object[]> GetCustomersFromJson()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "customers.json");

        if (!File.Exists(jsonPath))
        {
            yield return new object[] { 1, "John", "john@example.com", "VIP", 100000m };
            yield return new object[] { 2, "Jane", "jane@example.com", "Premium", 50000m };
            yield return new object[] { 3, "Bob", "bob@example.com", "Regular", 20000m };
            yield break;
        }

        var jsonContent = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var customers = JsonSerializer.Deserialize<List<CustomerJsonRecord>>(jsonContent, options)!;

        foreach (var customer in customers)
        {
            yield return new object[]
            {
                customer.CustomerId, customer.Name, customer.Email, customer.Type, customer.CreditLimit
            };
        }
    }

    [Theory]
    [MemberAutoData(nameof(GetCustomersFromJson))]
    public void JsonIntegration_CustomerValidation(
        int customerId, string name, string email, string customerType, decimal creditLimit, Order order)
    {
        customerId.Should().BePositive();
        name.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
        customerType.Should().BeOneOf("VIP", "Premium", "Regular");
        creditLimit.Should().BePositive();

        order.Should().NotBeNull();
    }

    public static IEnumerable<object[]> GetVipCustomersFromJson()
    {
        return GetCustomersFromJson().Where(data => data[3].ToString() == "VIP");
    }

    [Theory]
    [MemberAutoData(nameof(GetVipCustomersFromJson))]
    public void JsonIntegration_VipCustomerFilter(
        int customerId, string name, string email, string customerType, decimal creditLimit, Order order)
    {
        customerType.Should().Be("VIP");
        creditLimit.Should().BeGreaterOrEqualTo(100000m);
    }
}

// Mixed data source tests
public class MixedDataSourceTests
{
    public static IEnumerable<object[]> GetOrderScenarios()
    {
        yield return new object[] { "VIP", 100000m, "iPhone 15", 35900m };
        yield return new object[] { "Premium", 50000m, "MacBook Pro", 89900m };
        yield return new object[] { "Regular", 20000m, "AirPods Pro", 7490m };
    }

    [Theory]
    [MemberAutoData(nameof(GetOrderScenarios))]
    public void MixedDataSources_OrderScenarioTesting(
        string customerType, decimal creditLimit, string productName, decimal productPrice, Order order)
    {
        var customer = new Customer
        {
            Type = customerType,
            CreditLimit = creditLimit
        };

        var product = new Product
        {
            Name = productName,
            Price = productPrice
        };

        order.Amount = productPrice;
        var canOrder = customer.CreditLimit >= order.Amount;

        if (customerType == "Regular" && productName == "MacBook Pro")
        {
            canOrder.Should().BeFalse("Regular customer credit limit insufficient for MacBook Pro");
        }
        else
        {
            canOrder.Should().BeTrue();
        }
    }
}

// Test data helper
public static class TestDataHelper
{
    public static string GetTestDataPath(string fileName) =>
        Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);

    public static bool TestDataFileExists(string fileName) =>
        File.Exists(GetTestDataPath(fileName));

    public static IEnumerable<T> ReadCsvSafely<T>(string fileName)
    {
        var path = GetTestDataPath(fileName);
        if (!File.Exists(path)) return Enumerable.Empty<T>();

        using var reader = new StreamReader(path);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }

    public static IEnumerable<T> ReadJsonSafely<T>(string fileName)
    {
        var path = GetTestDataPath(fileName);
        if (!File.Exists(path)) return Enumerable.Empty<T>();

        var jsonContent = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<T>>(jsonContent, options) ?? new List<T>();
    }
}
