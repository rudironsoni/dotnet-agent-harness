namespace DotnetAgentHarness.Cli.Tests.Services;

using System.Xml.Linq;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using Xunit;

public class ProjectAnalyzerTests : IDisposable
{
    private readonly string testDir;
    private readonly ProjectAnalyzer analyzer;
    private bool disposedValue;

    public ProjectAnalyzerTests()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);
        this.analyzer = new ProjectAnalyzer();
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        string nonExistentPath = Path.Combine(this.testDir, "nonexistent");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(nonExistentPath);

        // Assert
        profile.Should().BeNull();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        string nonExistentFile = Path.Combine(this.testDir, "nonexistent.csproj");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(nonExistentFile);

        // Assert
        profile.Should().BeNull();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithEmptyDirectory_ReturnsNull()
    {
        // Arrange
        string emptyDir = Path.Combine(this.testDir, "empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(emptyDir);

        // Assert
        profile.Should().BeNull();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithValidCsproj_ReturnsProfile()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "net10.0"),
                    new XElement("OutputType", "Library"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.ProjectPath.Should().Be(csprojPath);
        profile.ProjectType.Should().Be("ClassLib");
        profile.TargetFrameworks.Should().Contain("net10.0");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithMultipleTargetFrameworks_ReturnsAllFrameworks()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFrameworks", "net8.0;net9.0;net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.TargetFrameworks.Should().HaveCount(3);
        profile.TargetFrameworks.Should().Contain("net8.0");
        profile.TargetFrameworks.Should().Contain("net9.0");
        profile.TargetFrameworks.Should().Contain("net10.0");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithPackageReferences_ReturnsPackages()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Newtonsoft.Json"),
                        new XAttribute("Version", "13.0.1")),
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Microsoft.NET.Test.Sdk"),
                        new XAttribute("Version", "17.0.0")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.Packages.Should().HaveCount(2);
        profile.Packages.Should().Contain(p => p.Name == "Newtonsoft.Json" && p.Version == "13.0.1");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithTestSdk_ReturnsIsTestProject()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Microsoft.NET.Test.Sdk"),
                        new XAttribute("Version", "17.0.0")),
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "xunit"),
                        new XAttribute("Version", "2.4.1")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.IsTestProject.Should().BeTrue();
        profile.ProjectType.Should().Be("Test");
        profile.TestFrameworks.Should().Contain("xunit");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithWebSdk_ReturnsIsWebProject()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk.Web"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.IsWebProject.Should().BeTrue();
        profile.ProjectType.Should().Be("Web");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithAspNetCorePackage_ReturnsIsWebProject()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Microsoft.AspNetCore.App"),
                        new XAttribute("Version", "10.0.0")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.IsWebProject.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithEFCorePackage_ReturnsHasEntityFramework()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Microsoft.EntityFrameworkCore"),
                        new XAttribute("Version", "8.0.0")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.HasEntityFramework.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithAspirePackage_ReturnsHasAspire()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Aspire.Hosting"),
                        new XAttribute("Version", "8.0.0")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.HasAspire.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithDockerfile_ReturnsHasDocker()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "Dockerfile"), "FROM mcr.microsoft.com/dotnet/sdk:10.0");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.HasDocker.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithProjectReferences_ReturnsProjects()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "ProjectReference",
                        new XAttribute("Include", "..\\OtherProject\\OtherProject.csproj")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.Projects.Should().HaveCount(1);
        profile.Projects[0].Name.Should().Be("OtherProject");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithSolution_ReturnsSolutionPath()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        string slnPath = Path.Combine(this.testDir, "TestSolution.sln");

        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());
        await File.WriteAllTextAsync(slnPath, "Microsoft Visual Studio Solution File");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.SolutionPath.Should().Be(slnPath);
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithGitHubActions_ReturnsCiConfig()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        string workflowsDir = Path.Combine(this.testDir, ".github", "workflows");
        Directory.CreateDirectory(workflowsDir);

        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());
        await File.WriteAllTextAsync(Path.Combine(workflowsDir, "ci.yml"), "name: CI");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.CiConfigs.Should().HaveCount(1);
        profile.CiConfigs[0].Platform.Should().Be("GitHub Actions");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithAzureDevOps_ReturnsCiConfig()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        string adoDir = Path.Combine(this.testDir, ".azure-pipelines");
        Directory.CreateDirectory(adoDir);

        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());
        await File.WriteAllTextAsync(Path.Combine(adoDir, "ci.yml"), "trigger:");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.CiConfigs.Should().HaveCount(1);
        profile.CiConfigs[0].Platform.Should().Be("Azure DevOps");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithAzureDevOpsRootYml_ReturnsCiConfig()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");

        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "azure-pipelines.yml"), "trigger:");

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.CiConfigs.Should().HaveCount(1);
        profile.CiConfigs[0].Platform.Should().Be("Azure DevOps");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithWorkerSdk_ReturnsWorkerType()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk.Worker"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.ProjectType.Should().Be("Worker");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithBlazorSdk_ReturnsBlazorType()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk.BlazorWebAssembly"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.ProjectType.Should().Be("Blazor");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithConsoleOutputType_ReturnsConsoleType()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "net10.0"),
                    new XElement("OutputType", "Exe"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.ProjectType.Should().Be("Console");
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithPrivateAssets_ReturnsIsPrivateAsset()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0")),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "SomeAnalyzer"),
                        new XAttribute("Version", "1.0.0"),
                        new XAttribute("PrivateAssets", "all")))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(csprojPath);

        // Assert
        profile.Should().NotBeNull();
        profile!.Packages.Should().ContainSingle(p => p.IsPrivateAsset);
    }

    [Fact]
    public async Task AnalyzeProjectAsync_FromDirectory_FindsProject()
    {
        // Arrange
        string csprojPath = Path.Combine(this.testDir, "TestProject.csproj");
        var csproj = new XDocument(
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0"))));
        await File.WriteAllTextAsync(csprojPath, csproj.ToString());

        // Act
        var profile = await this.analyzer.AnalyzeProjectAsync(this.testDir);

        // Assert
        profile.Should().NotBeNull();
        profile!.ProjectPath.Should().Be(csprojPath);
    }

    [Fact]
    public async Task FindProjectsAsync_WithNoProjects_ReturnsEmpty()
    {
        // Act
        var projects = await this.analyzer.FindProjectsAsync(this.testDir);

        // Assert
        projects.Should().BeEmpty();
    }

    [Fact]
    public async Task FindProjectsAsync_WithProjects_ReturnsProjects()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "Project1.csproj"), "<Project></Project>");
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "Project2.csproj"), "<Project></Project>");

        // Act
        var projects = await this.analyzer.FindProjectsAsync(this.testDir);

        // Assert
        projects.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindProjectsAsync_WithNestedProjects_ReturnsAllProjects()
    {
        // Arrange
        string nestedDir = Path.Combine(this.testDir, "src", "Project");
        Directory.CreateDirectory(nestedDir);
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "Root.csproj"), "<Project></Project>");
        await File.WriteAllTextAsync(Path.Combine(nestedDir, "Nested.csproj"), "<Project></Project>");

        // Act
        var projects = await this.analyzer.FindProjectsAsync(this.testDir);

        // Assert
        projects.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindProjectsAsync_WithNonExistentDirectory_ReturnsEmpty()
    {
        // Act
        var projects = await this.analyzer.FindProjectsAsync("/nonexistent/path");

        // Assert
        projects.Should().BeEmpty();
    }

    [Fact]
    public async Task FindSolutionsAsync_WithNoSolutions_ReturnsEmpty()
    {
        // Act
        var solutions = await this.analyzer.FindSolutionsAsync(this.testDir);

        // Assert
        solutions.Should().BeEmpty();
    }

    [Fact]
    public async Task FindSolutionsAsync_WithSolution_ReturnsSolution()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "Test.sln"), "Microsoft Visual Studio Solution File");

        // Act
        var solutions = await this.analyzer.FindSolutionsAsync(this.testDir);

        // Assert
        solutions.Should().ContainSingle();
    }

    [Fact]
    public async Task FindSolutionsAsync_WithNestedSolutions_DoesNotReturnNested()
    {
        // Arrange
        string nestedDir = Path.Combine(this.testDir, "src");
        Directory.CreateDirectory(nestedDir);
        await File.WriteAllTextAsync(Path.Combine(this.testDir, "Root.sln"), "Microsoft Visual Studio Solution File");
        await File.WriteAllTextAsync(Path.Combine(nestedDir, "Nested.sln"), "Microsoft Visual Studio Solution File");

        // Act
        var solutions = await this.analyzer.FindSolutionsAsync(this.testDir);

        // Assert
        solutions.Should().ContainSingle();
    }

    [Fact]
    public async Task FindSolutionsAsync_WithNonExistentDirectory_ReturnsEmpty()
    {
        // Act
        var solutions = await this.analyzer.FindSolutionsAsync("/nonexistent/path");

        // Assert
        solutions.Should().BeEmpty();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposedValue)
        {
            return;
        }

        if (disposing && Directory.Exists(this.testDir))
        {
            Directory.Delete(this.testDir, true);
        }

        this.disposedValue = true;
    }
}
