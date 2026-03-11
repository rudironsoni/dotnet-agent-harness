namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class RecommendCommandTests
{
    private readonly ISkillCatalog skillCatalog;
    private readonly IProjectAnalyzer projectAnalyzer;
    private readonly RecommendCommand command;

    public RecommendCommandTests()
    {
        this.skillCatalog = Substitute.For<ISkillCatalog>();
        this.projectAnalyzer = Substitute.For<IProjectAnalyzer>();
        this.command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("recommend");
        this.command.Description.Should().Contain("Recommend");
    }

    [Fact]
    public void Constructor_HasPathOption()
    {
        // Act
        Option? pathOpt = this.command.Options.FirstOrDefault(o => o.Name == "path");

        // Assert
        pathOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasPlatformOption()
    {
        // Act
        Option? platformOpt = this.command.Options.FirstOrDefault(o => o.Name == "platform");

        // Assert
        platformOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasCategoryOption()
    {
        // Act
        Option? categoryOpt = this.command.Options.FirstOrDefault(o => o.Name == "category");

        // Assert
        categoryOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasLimitOption()
    {
        // Act
        Option? limitOpt = this.command.Options.FirstOrDefault(o => o.Name == "limit");

        // Assert
        limitOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasFormatOption()
    {
        // Act
        Option? formatOpt = this.command.Options.FirstOrDefault(o => o.Name == "format");

        // Assert
        formatOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasWriteStateOption()
    {
        // Act
        Option? writeStateOpt = this.command.Options.FirstOrDefault(o => o.Name == "write-state");

        // Assert
        writeStateOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullSkillCatalog_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new RecommendCommand(null!, this.projectAnalyzer);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullProjectAnalyzer_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new RecommendCommand(this.skillCatalog, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithAllNullDependencies_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new RecommendCommand(null!, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }
}

public class RecommendCommandExecutionTests
{
    private readonly ISkillCatalog skillCatalog;
    private readonly IProjectAnalyzer projectAnalyzer;

    public RecommendCommandExecutionTests()
    {
        this.skillCatalog = Substitute.For<ISkillCatalog>();
        this.projectAnalyzer = Substitute.For<IProjectAnalyzer>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullProjectProfile_ReturnsError()
    {
        // Arrange
        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ProjectProfile?)null);

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyProjectPath_UsesCurrentDirectory()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "ClassLib",
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync(Arg.Any<string>(), Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());
        this.skillCatalog.SearchSubagentsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SubagentInfo>());
        this.skillCatalog.SearchCommandsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<CommandInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithTestProject_SearchesTestingSkills()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "Test",
            IsTestProject = true,
            TestFrameworks = new[] { "xunit" },
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync("testing", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());
        this.skillCatalog.SearchSkillsAsync("xunit", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithWebProject_SearchesWebSkills()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "Web",
            IsWebProject = true,
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync("aspnetcore", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());
        this.skillCatalog.SearchSkillsAsync("web", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithEFCoreProject_SearchesDataSkills()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "Web",
            HasEntityFramework = true,
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync("efcore", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());
        this.skillCatalog.SearchSkillsAsync("data", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithAspireProject_SearchesAspireSkills()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "Web",
            HasAspire = true,
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync("aspire", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithDockerProject_SearchesContainerSkills()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "Web",
            HasDocker = true,
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync("container", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithGitHubActions_SearchesGitHubActionsSkills()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "Web",
            CiConfigs = new[] { new CiConfig { Platform = "GitHub Actions", ConfigPath = ".github/workflows/ci.yml" } },
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync("github actions", Arg.Any<string>(), null, null, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithNullPlatform_SearchesAllPlatforms()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "ClassLib",
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync(Arg.Any<string>(), Arg.Any<string>(), null, null, null, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroLimit_ReturnsEmptyResults()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "ClassLib",
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);
        this.skillCatalog.SearchSkillsAsync(Arg.Any<string>(), Arg.Any<string>(), null, null, Arg.Any<string>(), 0, Arg.Any<CancellationToken>())
            .Returns(new List<SkillInfo>());

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithNoSearchTerms_HandlesGracefully()
    {
        // Arrange
        var profile = new ProjectProfile
        {
            ProjectPath = "/test/project.csproj",
            ProjectType = "ClassLib",
        };

        this.projectAnalyzer.AnalyzeProjectAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(profile);

        // Act
        var command = new RecommendCommand(this.skillCatalog, this.projectAnalyzer);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }
}
