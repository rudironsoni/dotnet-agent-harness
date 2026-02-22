# Test Implementation Examples

> This file is extracted from [SKILL.md](../SKILL.md), containing complete test implementation examples for AutoFixture + NSubstitute integration.

---

## Basic Test: No Dependency Behavior Setup Required

When testing only needs to verify SUT logic (like parameter validation):

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task IsExistsAsync_WhenShipperIdIs0_ShouldThrowArgumentOutOfRangeException(
    ShipperService sut)
{
    // Arrange
    var shipperId = 0;

    // Act
    var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => sut.IsExistsAsync(shipperId));

    // Assert
    exception.Message.Should().Contain(nameof(shipperId));
}
```

## Advanced Test: Setting Dependency Behavior

Use `[Frozen]` to obtain dependencies and set their behavior:

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task IsExistsAsync_WhenShipperIdDataNotExists_ShouldReturnFalse(
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut)
{
    // Arrange
    var shipperId = 99;
    shipperRepository.IsExistsAsync(Arg.Any<int>()).Returns(false);

    // Act
    var actual = await sut.IsExistsAsync(shipperId);

    // Assert
    actual.Should().BeFalse();
}
```

## Using Auto-Generated Test Data

AutoFixture generates both SUT and test data:

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task GetAsync_WhenShipperIdDataExists_ShouldReturnModel(
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut,
    ShipperModel model)  // AutoFixture auto-generates
{
    // Arrange
    var shipperId = model.ShipperId;
    shipperRepository.IsExistsAsync(Arg.Any<int>()).Returns(true);
    shipperRepository.GetAsync(Arg.Any<int>()).Returns(model);

    // Act
    var actual = await sut.GetAsync(shipperId);

    // Assert
    actual.Should().NotBeNull();
    actual.ShipperId.Should().Be(shipperId);
}
```

## Parameterized Tests: InlineAutoData

Combine fixed test values with auto-generated SUT:

```csharp
[Theory]
[InlineAutoDataWithCustomization(0, 10, nameof(from))]
[InlineAutoDataWithCustomization(-1, 10, nameof(from))]
[InlineAutoDataWithCustomization(1, 0, nameof(size))]
[InlineAutoDataWithCustomization(1, -1, nameof(size))]
public async Task GetCollectionAsync_WhenFromAndSizeInputInvalid_ShouldThrowArgumentOutOfRangeException(
    int from,
    int size,
    string parameterName,
    ShipperService sut)  // Auto-generated
{
    // Act
    var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        () => sut.GetCollectionAsync(from, size));

    // Assert
    exception.Message.Should().Contain(parameterName);
}
```

## Using CollectionSize to Control Collection Size

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task GetAllAsync_WhenTableHas10Records_ShouldReturnCollectionWith10Items(
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut,
    [CollectionSize(10)] IEnumerable<ShipperModel> models)
{
    // Arrange
    shipperRepository.GetAllAsync().Returns(models);

    // Act
    var actual = await sut.GetAllAsync();

    // Assert
    actual.Should().NotBeEmpty();
    actual.Should().HaveCount(10);
}
```

## Complex Data Setup: Using IFixture

When precise control over test data is needed:

```csharp
[Theory]
[AutoDataWithCustomization]
public async Task SearchAsync_WhenCompanyNameInputHasMatchingData_ShouldReturnCollectionContainingMatchingData(
    IFixture fixture,
    [Frozen] IShipperRepository shipperRepository,
    ShipperService sut)
{
    // Arrange
    const string companyName = "test";
    
    var models = fixture.Build<ShipperModel>()
                        .With(x => x.CompanyName, companyName)
                        .CreateMany(1);

    shipperRepository.GetTotalCountAsync().Returns(1);
    shipperRepository.SearchAsync(Arg.Any<string>(), Arg.Any<string>())
                     .Returns(models);

    // Act
    var actual = await sut.SearchAsync(companyName, string.Empty);

    // Assert
    actual.Should().NotBeEmpty();
    actual.Should().HaveCount(1);
    actual.Any(x => x.CompanyName == companyName).Should().BeTrue();
}
```

## Nullable Reference Type Handling

Handling null or empty value parameters in tests:

```csharp
[Theory]
[InlineAutoDataWithCustomization(null!, null!)]
[InlineAutoDataWithCustomization("", "")]
[InlineAutoDataWithCustomization("   ", "   ")]
public async Task SearchAsync_WhenCompanyNameAndPhoneBothEmpty_ShouldThrowArgumentException(
    string? companyName,
    string? phone,
    ShipperService sut)
{
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => sut.SearchAsync(companyName!, phone!));
    
    exception.Message.Should().Contain("companyName and phone cannot both be empty");
}
```

**Handling Explanation**:

1. **Parameter Declaration Uses `string?`**: Tests need to pass `null` values
2. **Using `null!` in InlineAutoData**: Tells compiler this is intentional test data
3. **Using `!` Operator in Method Call**: Using null-forgiving operator in tests
