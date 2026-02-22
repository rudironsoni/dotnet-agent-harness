# CRUD Operation Test Examples

> This document is extracted from [SKILL.md](../SKILL.md) and provides complete integration test code examples for CRUD operations.

## GET Request Testing

```csharp
[Fact]
public async Task GetShipper_WhenShipperExists_ShouldReturnSuccessResult()
{
    // Arrange
    await CleanupDatabaseAsync();
    var shipperId = await SeedShipperAsync("SF Express", "02-2345-6789");

    // Act
    var response = await Client.GetAsync($"/api/shippers/{shipperId}");

    // Assert
    response.Should().Be200Ok()
            .And
            .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
            {
                result.Status.Should().Be("Success");
                result.Data!.ShipperId.Should().Be(shipperId);
                result.Data.CompanyName.Should().Be("SF Express");
            });
}

[Fact]
public async Task GetShipper_WhenShipperNotExists_ShouldReturn404NotFound()
{
    // Arrange
    var nonExistentShipperId = 9999;

    // Act
    var response = await Client.GetAsync($"/api/shippers/{nonExistentShipperId}");

    // Assert
    response.Should().Be404NotFound();
}
```

## POST Request Testing

```csharp
[Fact]
public async Task CreateShipper_InputValidData_ShouldCreateSuccessfully()
{
    // Arrange
    await CleanupDatabaseAsync();
    var createParameter = new ShipperCreateParameter
    {
        CompanyName = "Black Cat Delivery",
        Phone = "02-1234-5678"
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/shippers", createParameter);

    // Assert
    response.Should().Be201Created()
            .And
            .Satisfy<SuccessResultOutputModel<ShipperOutputModel>>(result =>
            {
                result.Status.Should().Be("Success");
                result.Data!.ShipperId.Should().BeGreaterThan(0);
                result.Data.CompanyName.Should().Be("Black Cat Delivery");
            });
}
```

## Validation Error Testing

```csharp
[Fact]
public async Task CreateShipper_WhenCompanyNameEmpty_ShouldReturn400BadRequest()
{
    // Arrange
    var createParameter = new ShipperCreateParameter
    {
        CompanyName = "",
        Phone = "02-1234-5678"
    };

    // Act
    var response = await Client.PostAsJsonAsync("/api/shippers", createParameter);

    // Assert
    response.Should().Be400BadRequest()
            .And
            .Satisfy<ValidationProblemDetails>(problem =>
            {
                problem.Status.Should().Be(400);
                problem.Errors.Should().ContainKey("CompanyName");
            });
}
```

## Collection Data Testing

```csharp
[Fact]
public async Task GetAllShippers_ShouldReturnAllShippers()
{
    // Arrange
    await CleanupDatabaseAsync();
    await SeedShipperAsync("Company A", "02-1111-1111");
    await SeedShipperAsync("Company B", "02-2222-2222");

    // Act
    var response = await Client.GetAsync("/api/shippers");

    // Assert
    response.Should().Be200Ok()
            .And
            .Satisfy<SuccessResultOutputModel<List<ShipperOutputModel>>>(result =>
            {
                result.Data!.Count.Should().Be(2);
                result.Data.Should().Contain(s => s.CompanyName == "Company A");
                result.Data.Should().Contain(s => s.CompanyName == "Company B");
            });
}
```
