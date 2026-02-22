# Data Source Design Patterns

> This document is extracted from [SKILL.md](../SKILL.md), providing design patterns for hierarchical data organization and reusable datasets.

## Hierarchical Data Organization

```csharp
namespace AutoData.Tests.DataSources;

/// <summary>
/// Base class for test data sources
/// </summary>
public abstract class BaseTestData
{
    protected static string GetTestDataPath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "TestData", fileName);
    }
}

/// <summary>
/// Product test data source
/// </summary>
public class ProductTestDataSource : BaseTestData
{
    public static IEnumerable<object[]> BasicProducts()
    {
        yield return new object[] { "iPhone", 35900m, true };
        yield return new object[] { "MacBook", 89900m, true };
        yield return new object[] { "AirPods", 7490m, false };
    }

    public static IEnumerable<object[]> ElectronicsProducts()
    {
        // Read from CSV file
        var csvPath = GetTestDataPath("electronics.csv");
        // ... reading logic
    }
}

/// <summary>
/// Customer test data source
/// </summary>
public class CustomerTestDataSource : BaseTestData
{
    public static IEnumerable<object[]> VipCustomers()
    {
        yield return new object[] { "John", "VIP", 100000m };
        yield return new object[] { "Jane", "VIP", 150000m };
    }
}
```

## Reusable Datasets

```csharp
/// <summary>
/// Reusable test datasets
/// </summary>
public static class ReusableTestDataSets
{
    public static class ProductCategories
    {
        public static IEnumerable<object[]> All()
        {
            yield return new object[] { "Electronics", "TECH" };
            yield return new object[] { "Apparel", "FASHION" };
            yield return new object[] { "Home & Garden", "HOME" };
        }

        public static IEnumerable<object[]> Electronics()
        {
            yield return new object[] { "Mobile", "MOBILE" };
            yield return new object[] { "Laptop", "LAPTOP" };
        }
    }

    public static class CustomerTypes
    {
        public static IEnumerable<object[]> All()
        {
            yield return new object[] { "VIP", 100000m, 0.15m };
            yield return new object[] { "Premium", 50000m, 0.10m };
            yield return new object[] { "Regular", 20000m, 0.05m };
        }
    }
}
```
