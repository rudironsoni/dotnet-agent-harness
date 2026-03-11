namespace DotnetAgentHarness.Cli.Tests.Services;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class CodeAnalyzerTests : IDisposable
{
    private readonly string testDir;
    private readonly MockFileSystem fileSystem;
    private readonly IProjectAnalyzer projectAnalyzer;
    private readonly CodeAnalyzer codeAnalyzer;
    private bool disposedValue;

    public CodeAnalyzerTests()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);
        this.fileSystem = new MockFileSystem();
        this.projectAnalyzer = Substitute.For<IProjectAnalyzer>();
        this.codeAnalyzer = new CodeAnalyzer(this.projectAnalyzer, this.fileSystem);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AnalyzeAsync_WithNoProjects_ReturnsFailure()
    {
        // Arrange
        var options = new AnalysisOptions
        {
            ProjectPath = this.testDir,
        };

        this.projectAnalyzer.FindProjectsAsync(this.testDir, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        // Act
        AnalysisResult result = await this.codeAnalyzer.AnalyzeAsync(options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No .csproj files found");
    }

    [Fact]
    public async Task AnalyzeAsync_WithNoProjects_ReturnsFailureResult()
    {
        // Arrange
        string projectPath = Path.Combine(this.testDir, "NonExistent.csproj");

        var options = new AnalysisOptions
        {
            ProjectPath = projectPath,
        };

        this.projectAnalyzer.FindProjectsAsync(projectPath, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        // Act
        AnalysisResult result = await this.codeAnalyzer.AnalyzeAsync(options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No .csproj files found");
    }

    [Fact]
    public async Task HasAnalyzersConfiguredAsync_WithRoslynAnalyzerPackage_ReturnsTrue()
    {
        // Arrange
        string projectPath = "/test/project.csproj";
        string csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""Microsoft.CodeAnalysis.Analyzers"" Version=""3.3.4"" />
  </ItemGroup>
</Project>";
        this.fileSystem.AddFile(projectPath, csprojContent);

        // Act
        bool result = await this.codeAnalyzer.HasAnalyzersConfiguredAsync(projectPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnalyzersConfiguredAsync_WithStyleCopPackage_ReturnsTrue()
    {
        // Arrange
        string projectPath = "/test/project.csproj";
        string csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.2.0"" />
  </ItemGroup>
</Project>";
        this.fileSystem.AddFile(projectPath, csprojContent);

        // Act
        bool result = await this.codeAnalyzer.HasAnalyzersConfiguredAsync(projectPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnalyzersConfiguredAsync_WithSonarPackage_ReturnsTrue()
    {
        // Arrange
        string projectPath = "/test/project.csproj";
        string csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""SonarAnalyzer.CSharp"" Version=""9.0.0"" />
  </ItemGroup>
</Project>";
        this.fileSystem.AddFile(projectPath, csprojContent);

        // Act
        bool result = await this.codeAnalyzer.HasAnalyzersConfiguredAsync(projectPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnalyzersConfiguredAsync_WithNoAnalyzers_ReturnsFalse()
    {
        // Arrange
        string projectPath = "/test/project.csproj";
        string csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>";
        this.fileSystem.AddFile(projectPath, csprojContent);

        // Act
        bool result = await this.codeAnalyzer.HasAnalyzersConfiguredAsync(projectPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnalyzersConfiguredAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        string projectPath = "/nonexistent/project.csproj";

        // Act
        bool result = await this.codeAnalyzer.HasAnalyzersConfiguredAsync(projectPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AnalysisResult_GetSummary_WithNoIssues_ReturnsPassedMessage()
    {
        // Arrange
        var result = new AnalysisResult
        {
            Success = true,
            ErrorCount = 0,
            WarningCount = 0,
            InfoCount = 0,
        };

        // Act
        string summary = result.GetSummary();

        // Assert
        summary.Should().Be("All checks passed");
    }

    [Fact]
    public void AnalysisResult_GetSummary_WithIssues_ReturnsCounts()
    {
        // Arrange
        var result = new AnalysisResult
        {
            Success = true,
            ErrorCount = 2,
            WarningCount = 5,
            InfoCount = 10,
        };

        // Act
        string summary = result.GetSummary();

        // Assert
        summary.Should().Contain("2 error(s)");
        summary.Should().Contain("5 warning(s)");
        summary.Should().Contain("10 info");
    }

    [Fact]
    public void AnalysisResult_GetSummary_WithFailure_ReturnsErrorMessage()
    {
        // Arrange
        var result = new AnalysisResult
        {
            Success = false,
            ErrorMessage = "Build failed",
        };

        // Act
        string summary = result.GetSummary();

        // Assert
        summary.Should().Be("Analysis failed: Build failed");
    }

    [Fact]
    public void AnalysisIssue_Properties_AreSetCorrectly()
    {
        // Arrange & Act
        var issue = new AnalysisIssue
        {
            Analyzer = "Roslyn",
            RuleId = "CS0168",
            Message = "The variable is declared but never used",
            FilePath = "/test/file.cs",
            LineNumber = 42,
            ColumnNumber = 10,
            Severity = AnalysisSeverity.Warning,
            HelpUrl = "https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/cs0168",
        };

        // Assert
        issue.Analyzer.Should().Be("Roslyn");
        issue.RuleId.Should().Be("CS0168");
        issue.Message.Should().Be("The variable is declared but never used");
        issue.FilePath.Should().Be("/test/file.cs");
        issue.LineNumber.Should().Be(42);
        issue.ColumnNumber.Should().Be(10);
        issue.Severity.Should().Be(AnalysisSeverity.Warning);
        issue.HelpUrl.Should().NotBeNull();
    }

    [Fact]
    public void AnalysisOptions_Defaults_AreSetCorrectly()
    {
        // Arrange & Act
        var options = new AnalysisOptions();

        // Assert
        options.ProjectPath.Should().Be(".");
        options.MinimumSeverity.Should().Be(AnalysisSeverity.Info);
        options.OutputFormat.Should().Be(AnalysisOutputFormat.Text);
        options.TreatWarningsAsErrors.Should().BeFalse();
        options.RunStyleCop.Should().BeTrue();
        options.RunSonar.Should().BeFalse();
        options.Verbose.Should().BeFalse();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing && Directory.Exists(this.testDir))
            {
                Directory.Delete(this.testDir, true);
            }

            this.disposedValue = true;
        }
    }
}
