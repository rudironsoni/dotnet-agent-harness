// =============================================================================
// Custom AutoData Attribute Templates
// AutoFixture + NSubstitute Integration AutoData Attribute Implementation Examples
// =============================================================================

#region Basic Usage Examples

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Mapster;
using MapsterMapper;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace MyProject.Tests.AutoFixtureConfigurations;

// =============================================================================
// Basic AutoData Attribute (AutoNSubstitute only)
// =============================================================================

/// <summary>
/// Basic automatic mocking AutoData attribute
/// Automatically creates NSubstitute substitutes for all interfaces and abstract classes
/// </summary>
/// <example>
/// <code>
/// [Theory]
/// [AutoNSubstituteData]
/// public void Test([Frozen] IRepository repo, MyService sut)
/// {
///     repo.GetAsync(1).Returns(someData);
///     // ...
/// }
/// </code>
/// </example>
public class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture().Customize(new AutoNSubstituteCustomization());
    }
}

/// <summary>
/// Basic InlineAutoData version
/// </summary>
/// <example>
/// <code>
/// [Theory]
/// [InlineAutoNSubstituteData(0)]
/// [InlineAutoNSubstituteData(-1)]
/// public void Test_InvalidId(int invalidId, MyService sut)
/// {
///     // invalidId is fixed value, sut is auto-generated
/// }
/// </code>
/// </example>
public class InlineAutoNSubstituteDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoNSubstituteDataAttribute(params object[] values)
        : base(new AutoNSubstituteDataAttribute(), values)
    {
    }
}

#endregion

#region Advanced AutoData Attributes (with multiple Customizations)

// =============================================================================
// AutoData Attributes with multiple Customizations
// =============================================================================

/// <summary>
/// AutoData attribute with full project customization settings
/// Integrates AutoNSubstitute + Mapster + Domain model settings
/// </summary>
/// <remarks>
/// Suitable for tests that need:
/// - Automatic creation of NSubstitute substitutes for interfaces
/// - Real Mapster mapper usage
/// - Domain model customization rules
/// </remarks>
/// <example>
/// <code>
/// [Theory]
/// [AutoDataWithCustomization]
/// public async Task GetAsync_WhenDataExists_ShouldReturnCorrectData(
///     [Frozen] IRepository repository,
///     MyService sut,
///     MyModel model)
/// {
///     repository.GetAsync(Arg.Any&lt;int&gt;()).Returns(model);
///     var result = await sut.GetAsync(model.Id);
///     result.Should().BeEquivalentTo(model);
/// }
/// </code>
/// </example>
public class AutoDataWithCustomizationAttribute : AutoDataAttribute
{
    public AutoDataWithCustomizationAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            // Automatically create NSubstitute substitutes for interfaces
            .Customize(new AutoNSubstituteCustomization())
            // Use real Mapster mapper
            .Customize(new MapsterMapperCustomization())
            // Apply domain model customization rules
            .Customize(new DomainModelCustomization());

        return fixture;
    }
}

/// <summary>
/// InlineAutoData attribute with full project customization settings
/// </summary>
/// <remarks>
/// Used for scenarios requiring mixed fixed test values with auto-generated objects:
/// - Boundary value testing
/// - Exception parameter testing
/// - Multiple fixed condition testing
/// </remarks>
/// <example>
/// <code>
/// [Theory]
/// [InlineAutoDataWithCustomization(0, 10, nameof(from))]
/// [InlineAutoDataWithCustomization(-1, 10, nameof(from))]
/// [InlineAutoDataWithCustomization(1, 0, nameof(size))]
/// public async Task GetCollection_InvalidParameters_ShouldThrowException(
///     int from,
///     int size,
///     string parameterName,
///     MyService sut)
/// {
///     var ex = await Assert.ThrowsAsync&lt;ArgumentOutOfRangeException&gt;(
///         () => sut.GetCollectionAsync(from, size));
///     ex.Message.Should().Contain(parameterName);
/// }
/// </code>
/// </example>
public class InlineAutoDataWithCustomizationAttribute : InlineAutoDataAttribute
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="values">Fixed values (will populate the first few parameters of the test method)</param>
    public InlineAutoDataWithCustomizationAttribute(params object[] values)
        : base(new AutoDataWithCustomizationAttribute(), values)
    {
    }
}

#endregion

#region Mapster Customization

// =============================================================================
// Mapster Mapper Customization
// =============================================================================

/// <summary>
/// Mapster mapper customization
/// Provides real IMapper instance instead of Mock
/// </summary>
/// <remarks>
/// Why not use Mock:
/// 1. IMapper is a utility dependency, not business logic
/// 2. Tests need to verify mapping logic is correct
/// 3. Setting up Returns for each mapping method increases complexity
/// </remarks>
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
            if (this._mapper is not null)
            {
                return this._mapper;
            }

            var typeAdapterConfig = new TypeAdapterConfig();
            
            // Scan assemblies containing mapping configurations
            // Replace with your project's MapRegister class
            typeAdapterConfig.Scan(typeof(ServiceMapRegister).Assembly);
            
            this._mapper = new Mapper(typeAdapterConfig);
            return this._mapper;
        }
    }
}

/// <summary>
/// Example: Mapster mapping configuration
/// </summary>
public class ServiceMapRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configure ShipperModel -> ShipperDto mapping rules
        config.NewConfig<ShipperModel, ShipperDto>();
        
        // Configure ShipperDto -> ShipperModel mapping rules
        config.NewConfig<ShipperDto, ShipperModel>();
    }
}

#endregion

#region AutoMapper Customization

// =============================================================================
// AutoMapper Customization (Alternative)
// =============================================================================

/// <summary>
/// AutoMapper customization
/// Suitable for projects using AutoMapper
/// </summary>
public class AutoMapperCustomization : ICustomization
{
    private IMapper? _mapper;

    public void Customize(IFixture fixture)
    {
        fixture.Register<IMapper>(() => this.Mapper);
    }

    private IMapper Mapper
    {
        get
        {
            if (this._mapper is not null)
            {
                return this._mapper;
            }

            var configuration = new MapperConfiguration(cfg =>
            {
                // Scan assemblies containing Profiles
                cfg.AddMaps(typeof(MappingProfile).Assembly);
            });

            this._mapper = configuration.CreateMapper();
            return this._mapper;
        }
    }
}

/// <summary>
/// Example: AutoMapper Profile
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ShipperModel, ShipperDto>().ReverseMap();
    }
}

#endregion

#region Domain Model Customization

// =============================================================================
// Domain Model Customization
// =============================================================================

/// <summary>
/// Domain model customization
/// Sets creation rules for specific types
/// </summary>
public class DomainModelCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Configure ShipperModel creation rules
        fixture.Customize<ShipperModel>(composer => composer
            .With(x => x.ShipperId, () => fixture.Create<int>() % 1000 + 1)
            .With(x => x.CompanyName, () => $"Company_{fixture.Create<string>()[..8]}")
            .With(x => x.Phone, () => $"02-{fixture.Create<int>() % 90000000 + 10000000}"));

        // Configure OrderModel creation rules
        fixture.Customize<OrderModel>(composer => composer
            .With(x => x.OrderId, () => fixture.Create<int>() % 10000 + 1)
            .With(x => x.OrderDate, () => DateTime.Today.AddDays(-fixture.Create<int>() % 365))
            .With(x => x.TotalAmount, () => Math.Round(fixture.Create<decimal>() % 10000, 2)));
    }
}

#endregion

#region Special Purpose AutoData Attributes

// =============================================================================
// Special Purpose AutoData Attributes
// =============================================================================

/// <summary>
/// AutoData attribute with Logger substitute
/// Automatically creates verifiable substitutes for ILogger&lt;T&gt;
/// </summary>
public class AutoDataWithLoggerAttribute : AutoDataAttribute
{
    public AutoDataWithLoggerAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new LoggerCustomization());

        return fixture;
    }
}

/// <summary>
/// Logger customization
/// Provides verifiable ILogger instances
/// </summary>
public class LoggerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        // Register ILoggerFactory
        fixture.Register<ILoggerFactory>(() => NSubstitute.Substitute.For<ILoggerFactory>());
    }
}

/// <summary>
/// AutoData attribute without recursion
/// Prevents generating complex object graphs with self-references
/// </summary>
public class AutoDataNoRecursionAttribute : AutoDataAttribute
{
    public AutoDataNoRecursionAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());
        
        // Set recursion behavior to throw exception (instead of infinite recursion)
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}

/// <summary>
/// AutoData attribute with limited recursion depth
/// Suitable for objects with complex nested structures
/// </summary>
public class AutoDataWithRecursionDepthAttribute : AutoDataAttribute
{
    public AutoDataWithRecursionDepthAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());
        
        // Remove default recursion behavior
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        
        // Set recursion depth to 1 (only generate one level of nesting)
        fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 1));

        return fixture;
    }
}

#endregion

#region Combining Multiple AutoData Attributes

// =============================================================================
// Using CompositeCustomization to combine multiple configurations
// =============================================================================

/// <summary>
/// Helper class for combining multiple Customizations
/// </summary>
public class ProjectCustomization : CompositeCustomization
{
    public ProjectCustomization()
        : base(
            new AutoNSubstituteCustomization(),
            new MapsterMapperCustomization(),
            new DomainModelCustomization())
    {
    }
}

/// <summary>
/// AutoData attribute using CompositeCustomization
/// </summary>
public class ProjectAutoDataAttribute : AutoDataAttribute
{
    public ProjectAutoDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        return new Fixture().Customize(new ProjectCustomization());
    }
}

/// <summary>
/// InlineAutoData attribute using CompositeCustomization
/// </summary>
public class ProjectInlineAutoDataAttribute : InlineAutoDataAttribute
{
    public ProjectInlineAutoDataAttribute(params object[] values)
        : base(new ProjectAutoDataAttribute(), values)
    {
    }
}

#endregion

#region Example Domain Models

// =============================================================================
// Example Domain Models (for examples above)
// =============================================================================

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

public class OrderModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

#endregion
