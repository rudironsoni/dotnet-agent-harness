using System.Runtime.CompilerServices;

/// <summary>
/// InternalsVisibleTo configuration examples
/// Demonstrates four ways to configure InternalsVisibleTo
/// </summary>
///
// ========================================
// Method 1: Directly declare attribute in code
// ========================================
// Suitable for: simple projects, single test project

// Add to any .cs file in main project (usually AssemblyInfo.cs)
[assembly: InternalsVisibleTo("MyProject.Tests")]
[assembly: InternalsVisibleTo("MyProject.IntegrationTests")]

// If using signed assemblies, need to include public key
[assembly: InternalsVisibleTo("MyProject.Tests, PublicKey=0024000004800000...")]


// ========================================
// Method 2: Use AssemblyAttribute in .csproj
// ========================================
// Suitable for: projects needing MSBuild variables

/*
<!-- MyProject.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>$(AssemblyName).Tests</_Parameter1>
    </AssemblyAttribute>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>$(AssemblyName).IntegrationTests</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>
</Project>
*/


// ========================================
// Method 3: Use Meziantou.MSBuild.InternalsVisibleTo (Recommended)
// ========================================
// Suitable for: complex projects, projects needing NSubstitute/Moq dynamic proxy support

/*
<!-- MyProject.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <!-- 1. Install NuGet package -->
  <ItemGroup>
    <PackageReference Include="Meziantou.MSBuild.InternalsVisibleTo" Version="1.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <!-- 2. Declare visible test projects -->
  <ItemGroup>
    <InternalsVisibleTo Include="$(AssemblyName).Tests" />
    <InternalsVisibleTo Include="$(AssemblyName).IntegrationTests" />

    <!-- 3. Auto-support NSubstitute/Moq dynamic proxy (auto-includes correct public key) -->
    <InternalsVisibleTo Include="DynamicProxyGenAssembly2"
                        Key="0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7" />
  </ItemGroup>
</Project>

Install NuGet package:
dotnet add package Meziantou.MSBuild.InternalsVisibleTo

Reference resources:
- https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm
- https://github.com/meziantou/Meziantou.MSBuild.InternalsVisibleTo
*/


// ========================================
// Internal Class Testing Example
// ========================================

namespace MyProject.Core;

/// <summary>
/// Internal class: Price Calculator
/// </summary>
internal class PriceCalculator
{
    /// <summary>
    /// Calculate product level (Internal method)
    /// </summary>
    internal string CalculatePriceLevel(decimal price)
    {
        return price switch
        {
            >= 10000 => "Luxury",
            >= 5000 => "Premium",
            >= 1000 => "Standard",
            > 0 => "Economy",
            _ => "Invalid Price"
        };
    }

    /// <summary>
    /// Calculate discounted price (Internal method)
    /// </summary>
    internal decimal CalculateDiscountedPrice(decimal originalPrice, decimal discountRate)
    {
        if (discountRate is < 0 or > 1)
            throw new ArgumentException("Discount rate must be between 0 and 1", nameof(discountRate));

        return originalPrice * (1 - discountRate);
    }
}


// ========================================
// Test Project Testing Example
// ========================================

/*
using Xunit;
using AwesomeAssertions;

namespace MyProject.Tests;

/// <summary>
/// PriceCalculator test class
/// Can access internal members because InternalsVisibleTo is configured
/// </summary>
public class PriceCalculatorTests
{
    [Theory]
    [InlineData(15000, "Luxury")]
    [InlineData(8000, "Premium")]
    [InlineData(3000, "Standard")]
    [InlineData(500, "Economy")]
    [InlineData(0, "Invalid Price")]
    public void CalculatePriceLevel_DifferentPrices_ShouldReturnCorrectLevel(decimal price, string expected)
    {
        // Arrange
        var calculator = new PriceCalculator(); // Can access internal class

        // Act
        var actual = calculator.CalculatePriceLevel(price); // Can call internal method

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1000, 0.1, 900)]
    [InlineData(2000, 0.2, 1600)]
    [InlineData(500, 0.05, 475)]
    public void CalculateDiscountedPrice_NormalDiscount_ShouldCalculateCorrectPrice(
        decimal originalPrice, decimal discountRate, decimal expected)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act
        var actual = calculator.CalculateDiscountedPrice(originalPrice, discountRate);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void CalculateDiscountedPrice_InvalidDiscountRate_ShouldThrowException(decimal invalidDiscountRate)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act & Assert
        var action = () => calculator.CalculateDiscountedPrice(1000, invalidDiscountRate);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Discount rate must be between 0 and 1*");
    }
}
*/


// ========================================
// Method Comparison and Selection Recommendations
// ========================================

/*
Method Comparison:

| Method                                   | Pros                         | Cons                         | Use Cases               |
|------------------------------------------|------------------------------|------------------------------|-------------------------|
| 1. Direct attribute declaration          | Simple and direct            | Hard-coded project names   | Simple projects         |
| 2. .csproj AssemblyAttribute             | Can use MSBuild variables    | Complex syntax             | Need dynamic assembly names |
| 3. Meziantou.MSBuild.InternalsVisibleTo  | Auto-handles keys, readable  | Needs extra package        | Complex projects (recommended) |

Selection Recommendations:
- Simple projects: Method 1
- Need MSBuild variables: Method 2
- Complex projects or using Mock frameworks: Method 3 (recommended)

Notes:
1. Only open visibility for internal members that truly need testing
2. Avoid abusing InternalsVisibleTo, consider refactoring to better design first
3. Document why internal visibility is needed (design docs or comments)
4. Regularly review if these visibility settings are still needed
*/
