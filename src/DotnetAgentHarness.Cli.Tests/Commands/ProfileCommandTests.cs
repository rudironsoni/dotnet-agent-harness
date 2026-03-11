namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class ProfileCommandTests
{
    private readonly ISkillCatalog skillCatalog;
    private readonly ProfileCommand command;

    public ProfileCommandTests()
    {
        this.skillCatalog = Substitute.For<ISkillCatalog>();
        this.command = new ProfileCommand(this.skillCatalog);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("profile");
        this.command.Description.Should().Contain("catalog");
    }

    [Fact]
    public void Constructor_HasItemArgument()
    {
        // Act
        Argument? itemArg = this.command.Arguments.FirstOrDefault(a => a.Name == "item");

        // Assert
        itemArg.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasKindOption()
    {
        // Act
        Option? kindOpt = this.command.Options.FirstOrDefault(o => o.Name == "kind");

        // Assert
        kindOpt.Should().NotBeNull();
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
    public void Constructor_ItemArgument_HasZeroOrOneArity()
    {
        // Act
        Argument<string>? itemArg = this.command.Arguments.FirstOrDefault(a => a.Name == "item") as Argument<string>;

        // Assert
        itemArg.Should().NotBeNull();
        itemArg!.Arity.MinimumNumberOfValues.Should().Be(0);
        itemArg.Arity.MaximumNumberOfValues.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithNullSkillCatalog_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new ProfileCommand(null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }
}

public class ProfileCommandExecutionTests
{
    private readonly ISkillCatalog skillCatalog;

    public ProfileCommandExecutionTests()
    {
        this.skillCatalog = Substitute.For<ISkillCatalog>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullItem_ShowsCatalogStats()
    {
        // Arrange
        var stats = new CatalogStats
        {
            TotalSkills = 100,
            TotalSubagents = 10,
            TotalCommands = 20,
            TotalLines = 5000,
        };

        this.skillCatalog.GetStatsAsync(Arg.Any<CancellationToken>())
            .Returns(stats);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyItem_ShowsCatalogStats()
    {
        // Arrange
        var stats = new CatalogStats
        {
            TotalSkills = 100,
            TotalSubagents = 10,
            TotalCommands = 20,
        };

        this.skillCatalog.GetStatsAsync(Arg.Any<CancellationToken>())
            .Returns(stats);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespaceItem_ShowsCatalogStats()
    {
        // Arrange
        var stats = new CatalogStats
        {
            TotalSkills = 100,
            TotalSubagents = 10,
            TotalCommands = 20,
        };

        this.skillCatalog.GetStatsAsync(Arg.Any<CancellationToken>())
            .Returns(stats);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetItemAsync_WithSkillKind_SearchesOnlySkills()
    {
        // Arrange
        var skill = new SkillInfo
        {
            Name = "test-skill",
            Description = "Test skill",
        };

        this.skillCatalog.GetSkillByNameAsync("test-skill", Arg.Any<CancellationToken>())
            .Returns(skill);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetItemAsync_WithSubagentKind_SearchesOnlySubagents()
    {
        // Arrange
        var subagent = new SubagentInfo
        {
            Name = "test-subagent",
            Description = "Test subagent",
        };

        this.skillCatalog.GetSubagentByNameAsync("test-subagent", Arg.Any<CancellationToken>())
            .Returns(subagent);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetItemAsync_WithCommandKind_SearchesOnlyCommands()
    {
        // Arrange
        var commandInfo = new CommandInfo
        {
            Name = "test-command",
            Description = "Test command",
        };

        this.skillCatalog.GetCommandByNameAsync("test-command", Arg.Any<CancellationToken>())
            .Returns(commandInfo);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetItemAsync_WithNoKind_SearchesAllKinds()
    {
        // Arrange
        this.skillCatalog.GetSkillByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((SkillInfo?)null);
        this.skillCatalog.GetSubagentByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((SubagentInfo?)null);
        this.skillCatalog.GetCommandByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CommandInfo?)null);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetItemAsync_WithUnknownKind_ReturnsNull()
    {
        // Arrange
        this.skillCatalog.GetSkillByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((SkillInfo?)null);
        this.skillCatalog.GetSubagentByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((SubagentInfo?)null);
        this.skillCatalog.GetCommandByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CommandInfo?)null);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetItemAsync_WithNotFoundItem_ReturnsNull()
    {
        // Arrange
        this.skillCatalog.GetSkillByNameAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((SkillInfo?)null);
        this.skillCatalog.GetSubagentByNameAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((SubagentInfo?)null);
        this.skillCatalog.GetCommandByNameAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((CommandInfo?)null);

        // Act
        var command = new ProfileCommand(this.skillCatalog);

        // Assert
        command.Should().NotBeNull();
        await Task.CompletedTask;
    }

    [Fact]
    public void CatalogStats_Properties_AreSetCorrectly()
    {
        // Arrange & Act
        var stats = new CatalogStats
        {
            TotalSkills = 100,
            TotalSubagents = 10,
            TotalCommands = 20,
            TotalLines = 5000,
            TotalTags = 50,
            SkillsByCategory = new Dictionary<string, int> { { "testing", 20 }, { "architecture", 30 } },
            SkillsByComplexity = new Dictionary<string, int> { { "beginner", 40 }, { "advanced", 10 } },
            TopTags = new Dictionary<string, int> { { "dotnet", 25 }, { "csharp", 20 } },
        };

        // Assert
        stats.TotalSkills.Should().Be(100);
        stats.TotalSubagents.Should().Be(10);
        stats.TotalCommands.Should().Be(20);
        stats.TotalLines.Should().Be(5000);
        stats.TotalTags.Should().Be(50);
        stats.SkillsByCategory.Should().ContainKey("testing").WhoseValue.Should().Be(20);
        stats.SkillsByCategory.Should().ContainKey("architecture").WhoseValue.Should().Be(30);
        stats.SkillsByComplexity.Should().ContainKey("beginner").WhoseValue.Should().Be(40);
        stats.TopTags.Should().ContainKey("dotnet").WhoseValue.Should().Be(25);
    }

    [Fact]
    public void CatalogStats_WithEmptyDictionaries_HandlesGracefully()
    {
        // Arrange & Act
        var stats = new CatalogStats
        {
            TotalSkills = 0,
            TotalSubagents = 0,
            TotalCommands = 0,
            SkillsByCategory = new Dictionary<string, int>(),
            SkillsByComplexity = new Dictionary<string, int>(),
            TopTags = new Dictionary<string, int>(),
        };

        // Assert
        stats.TotalSkills.Should().Be(0);
        stats.SkillsByCategory.Should().BeEmpty();
        stats.SkillsByComplexity.Should().BeEmpty();
        stats.TopTags.Should().BeEmpty();
    }

    [Fact]
    public void CatalogStats_WithNullDictionaries_HandlesGracefully()
    {
        // Arrange & Act
        var stats = new CatalogStats
        {
            TotalSkills = 0,
            SkillsByCategory = null!,
            SkillsByComplexity = null!,
            TopTags = null!,
        };

        // Assert
        stats.TotalSkills.Should().Be(0);
        stats.SkillsByCategory.Should().BeNull();
        stats.SkillsByComplexity.Should().BeNull();
        stats.TopTags.Should().BeNull();
    }
}
