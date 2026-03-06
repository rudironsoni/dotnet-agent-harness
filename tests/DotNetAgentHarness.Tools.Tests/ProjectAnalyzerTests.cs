using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class ProjectAnalyzerTests
{
    [Fact]
    public void Analyze_DetectsProjectKindsTechnologiesAndRepoSignals()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("global.json", """
            {
              "sdk": {
                "version": "10.0.100-preview.2"
              }
            }
            """);
        repo.WriteFile("Directory.Build.props", "<Project />");
        repo.WriteFile(".editorconfig", "root = true");
        repo.WriteFile(".config/dotnet-tools.json", """
            {
              "version": 1,
              "isRoot": true,
              "tools": {}
            }
            """);
        repo.WriteFile(".github/workflows/ci.yml", "name: ci");
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0-preview.2.25163.2" />
                <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0-preview.2.25163.2" />
              </ItemGroup>
            </Project>
            """);
        repo.WriteFile("tests/App.Tests/App.Tests.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <IsTestProject>true</IsTestProject>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
                <PackageReference Include="xunit" Version="2.9.3" />
              </ItemGroup>
            </Project>
            """);

        var profile = ProjectAnalyzer.Analyze(repo.Root, new DotNetEnvironmentReport
        {
            IsAvailable = true,
            InstalledSdkVersions = ["10.0.100-preview.2"]
        });

        Assert.Equal(2, profile.Projects.Count);
        Assert.Equal("web", profile.DominantProjectKind);
        Assert.Equal("xunit", profile.DominantTestFramework);
        Assert.Contains("aspnetcore", profile.Technologies);
        Assert.Contains("efcore", profile.Technologies);
        Assert.Contains("openapi", profile.Technologies);
        Assert.Contains("github-actions", profile.CiProviders);
        Assert.True(profile.HasDotNetToolManifest);
        Assert.True(profile.UsesDotNetLocalTools);
        Assert.True(profile.HasDirectoryBuildProps);
        Assert.True(profile.HasEditorConfig);
    }

    [Fact]
    public void Analyze_DetectsDotNetLocalToolUsageFromAutomationScripts()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        repo.WriteFile("scripts/dev/restore-tools.sh", """
            #!/usr/bin/env bash
            dotnet tool restore
            dotnet tool run slopwatch analyze -d .
            """);

        var profile = ProjectAnalyzer.Analyze(repo.Root, new DotNetEnvironmentReport
        {
            IsAvailable = true,
            InstalledSdkVersions = ["10.0.103"]
        });

        Assert.False(profile.HasDotNetToolManifest);
        Assert.True(profile.UsesDotNetLocalTools);
    }

    [Fact]
    public void Analyze_IgnoresToolManifestPathChecksWithoutLocalToolCommands()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        repo.WriteFile(".github/workflows/smoke.yml", """
            jobs:
              smoke:
                runs-on: ubuntu-latest
                steps:
                  - run: test -f .config/dotnet-tools.json
            """);

        var profile = ProjectAnalyzer.Analyze(repo.Root, new DotNetEnvironmentReport
        {
            IsAvailable = true,
            InstalledSdkVersions = ["10.0.103"]
        });

        Assert.False(profile.HasDotNetToolManifest);
        Assert.False(profile.UsesDotNetLocalTools);
    }

    [Fact]
    public void Analyze_IgnoresGeneratedAgentDirectories()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        repo.WriteFile(".factory/generated/FactoryShadow.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        repo.WriteFile(".gemini/generated/GeminiShadow.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        repo.WriteFile(".agent/generated/Shadow.slnx", "<Solution />");

        var profile = ProjectAnalyzer.Analyze(repo.Root, new DotNetEnvironmentReport
        {
            IsAvailable = true,
            InstalledSdkVersions = ["10.0.103"]
        });

        Assert.Single(profile.Projects);
        Assert.Empty(profile.Solutions);
    }
}
