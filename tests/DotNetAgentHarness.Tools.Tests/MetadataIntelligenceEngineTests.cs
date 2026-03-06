using System.IO;
using System.Linq;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class MetadataIntelligenceEngineTests
{
    [Fact]
    public void InspectPackages_ResolvesCentralVersionsAndOverrides()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("Directory.Packages.props", """
            <Project>
              <ItemGroup>
                <PackageVersion Include="Serilog" Version="4.0.0" />
                <PackageVersion Include="Serilog.Sinks.Console" Version="5.0.0" />
              </ItemGroup>
            </Project>
            """);
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Serilog" />
                <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
                <PackageReference Include="Serilog.Sinks.Console" VersionOverride="6.0.0" />
              </ItemGroup>
            </Project>
            """);

        var report = MetadataIntelligenceEngine.Inspect(repo.Root, new MetadataQuery
        {
            Mode = "packages",
            TargetPath = "src/App/App.csproj"
        });

        Assert.Equal("packages", report.Mode);
        var project = Assert.Single(report.Projects);
        Assert.Equal("App", project.ProjectName);

        var serilog = Assert.Single(project.Packages, package => package.Id == "Serilog");
        Assert.Equal("4.0.0", serilog.Version);
        Assert.Equal("central", serilog.Source);

        var json = Assert.Single(project.Packages, package => package.Id == "Newtonsoft.Json");
        Assert.Equal("13.0.3", json.Version);
        Assert.Equal("direct", json.Source);

        var sink = Assert.Single(project.Packages, package => package.Id == "Serilog.Sinks.Console");
        Assert.Equal("6.0.0", sink.Version);
        Assert.Equal("version-override", sink.Source);
        Assert.True(sink.HasVersionOverride);

        Assert.Contains(report.PackageUsages, usage => usage.Id == "Serilog" && usage.ProjectCount == 1);
    }

    [Fact]
    public void InspectTypes_BuildsProjectAndReturnsFqnBasedMetadata()
    {
        using var repo = new TestRepositoryBuilder();
        ProcessRunner.Run("dotnet", "new classlib -n DemoLib -o src/DemoLib", repo.Root, 120_000);
        repo.WriteFile("src/DemoLib/OrderService.cs", """
            namespace DemoLib.Core;

            public sealed class OrderService : IDisposable
            {
                public string Name { get; } = "orders";

                public int Count => 1;

                public void Process(int id)
                {
                }

                public ValueTask<int> ProcessAsync(CancellationToken cancellationToken = default)
                {
                    return ValueTask.FromResult(1);
                }

                public void Dispose()
                {
                }
            }
            """);

        var namespaces = MetadataIntelligenceEngine.Inspect(repo.Root, new MetadataQuery
        {
            Mode = "namespaces",
            TargetPath = "src/DemoLib/DemoLib.csproj",
            BuildIfNeeded = true
        });
        Assert.Contains("DemoLib.Core", namespaces.Namespaces);

        var types = MetadataIntelligenceEngine.Inspect(repo.Root, new MetadataQuery
        {
            Mode = "types",
            TargetPath = "src/DemoLib/DemoLib.csproj",
            NamespaceFilter = "DemoLib.Core",
            BuildIfNeeded = true
        });
        var type = Assert.Single(types.Types, item => item.FullName == "DemoLib.Core.OrderService");
        Assert.True(type.MethodCount >= 2);
        Assert.True(type.PropertyCount >= 2);

        var detail = MetadataIntelligenceEngine.Inspect(repo.Root, new MetadataQuery
        {
            Mode = "type",
            TargetPath = "src/DemoLib/DemoLib.csproj",
            TypeName = "DemoLib.Core.OrderService",
            BuildIfNeeded = true
        });

        Assert.NotNull(detail.Type);
        Assert.Contains(detail.Type!.Methods, method => method.Contains("ProcessAsync", System.StringComparison.Ordinal));
        Assert.Contains(detail.Type.Properties, property => property.Contains("Name", System.StringComparison.Ordinal));
        Assert.True(File.Exists(detail.AssemblyPath));
    }
}
