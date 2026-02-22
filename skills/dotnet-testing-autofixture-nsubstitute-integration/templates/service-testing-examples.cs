// Service Testing Examples
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using NSubstitute;
using Xunit;
using MapsterMapper;
using Mapster;

namespace MyProject.Tests.Services;

public interface IShipperService
{
    Task<bool> IsExistsAsync(int shipperId);
    Task<ShipperDto?> GetAsync(int shipperId);
}

public interface IShipperRepository
{
    Task<bool> IsExistsAsync(int shipperId);
    Task<ShipperModel?> GetAsync(int shipperId);
}

public class ShipperModel
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class ShipperDto
{
    public int ShipperId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public interface IResult
{
    bool IsSuccess { get; }
    string? ErrorMessage { get; }
}

public class MapsterMapperCustomization : ICustomization
{
    private IMapper? _mapper;

    public void Customize(IFixture fixture)
    {
        fixture.Register(() => this.Mapper);
    }

    private IMapper Mapper
    {
        get
        {
            if (_mapper is not null) return _mapper;
            var config = new TypeAdapterConfig();
            config.NewConfig<ShipperModel, ShipperDto>();
            _mapper = new Mapper(config);
            return _mapper;
        }
    }
}

public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture) { }
    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new MapsterMapperCustomization());
    }
}

public class ShipperService : IShipperService
{
    private readonly IMapper _mapper;
    private readonly IShipperRepository _shipperRepository;

    public ShipperService(IMapper mapper, IShipperRepository shipperRepository)
    {
        _mapper = mapper;
        _shipperRepository = shipperRepository;
    }

    public async Task<bool> IsExistsAsync(int shipperId)
    {
        if (shipperId <= 0)
            throw new ArgumentOutOfRangeException(nameof(shipperId), shipperId, "ShipperId must be greater than 0");
        return await _shipperRepository.IsExistsAsync(shipperId);
    }

    public async Task<ShipperDto?> GetAsync(int shipperId)
    {
        if (shipperId <= 0)
            throw new ArgumentOutOfRangeException(nameof(shipperId), shipperId, "ShipperId must be greater than 0");
        var exists = await _shipperRepository.IsExistsAsync(shipperId);
        if (!exists) return null;
        var model = await _shipperRepository.GetAsync(shipperId);
        return _mapper.Map<ShipperDto>(model);
    }
}

public class ShipperServiceTests
{
    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_ShipperIdIsZero_ShouldThrowArgumentOutOfRangeException(ShipperService sut)
    {
        var shipperId = 0;
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.IsExistsAsync(shipperId));
        exception.ParamName.Should().Be(nameof(shipperId));
    }

    [Theory]
    [AutoDataWithCustomization]
    public async Task IsExistsAsync_ShipperIdExists_ShouldReturnTrue(
        [Frozen] IShipperRepository shipperRepository, ShipperService sut)
    {
        var shipperId = 1;
        shipperRepository.IsExistsAsync(shipperId).Returns(true);
        var actual = await sut.IsExistsAsync(shipperId);
        actual.Should().BeTrue();
        await shipperRepository.Received(1).IsExistsAsync(shipperId);
    }

    [Theory]
    [AutoDataWithCustomization]
    public async Task GetAsync_ShipperIdExists_ShouldReturnModel(
        [Frozen] IShipperRepository shipperRepository, ShipperService sut, ShipperModel model)
    {
        var shipperId = model.ShipperId;
        shipperRepository.IsExistsAsync(shipperId).Returns(true);
        shipperRepository.GetAsync(shipperId).Returns(model);
        var actual = await sut.GetAsync(shipperId);
        actual.Should().NotBeNull();
        actual!.ShipperId.Should().Be(shipperId);
    }
}
