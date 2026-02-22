# Common Dependency Customization

> This file is extracted from [SKILL.md](../SKILL.md), containing customization methods for special dependencies like IMapper.

---

## IMapper Customization (Mapster Example)

Some dependencies are not suitable for Mocking and should use real instances:

```csharp
using AutoFixture;
using Mapster;
using MapsterMapper;

namespace MyProject.Tests.AutoFixtureConfigurations;

/// <summary>
/// Mapster mapper customization
/// </summary>
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
            typeAdapterConfig.Scan(typeof(ServiceMapRegister).Assembly);
            this._mapper = new Mapper(typeAdapterConfig);
            return this._mapper;
        }
    }
}
```

**Why not Mock IMapper?**

1. **Utility Dependency**: Mapper is not business logic, it's an object mapping tool
2. **Validate Mapping Logic**: Tests need to verify mapping correctness, Mocking loses this capability
3. **Configuration Complexity**: Setting Returns for each mapping method increases complexity
4. **Test Intent**: We want to test business logic, not Mapper behavior

## AutoMapper Customization Example

```csharp
using AutoFixture;
using AutoMapper;

namespace MyProject.Tests.AutoFixtureConfigurations;

public class AutoMapperCustomization : ICustomization
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

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(MappingProfile).Assembly);
            });

            this._mapper = configuration.CreateMapper();
            return this._mapper;
        }
    }
}
```
