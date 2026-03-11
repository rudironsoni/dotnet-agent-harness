namespace DotnetAgentHarness.Cli.Tests.Services;

using System.IO.Abstractions.TestingHelpers;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using Xunit;

public class SkillCatalogTests : IDisposable
{
    private readonly string testDir;
    private readonly MockFileSystem fileSystem;
    private bool disposedValue;

    public SkillCatalogTests()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);
        this.fileSystem = new MockFileSystem();
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithNoBasePath_UsesCurrentDirectory()
    {
        // Arrange & Act
        var catalog = new SkillCatalog();

        // Assert
        catalog.BasePath.Should().Be(Directory.GetCurrentDirectory());
    }

    [Fact]
    public void Constructor_WithBasePath_SetsBasePath()
    {
        // Arrange & Act
        var catalog = new SkillCatalog(this.testDir);

        // Assert
        catalog.BasePath.Should().Be(this.testDir);
    }

    [Fact]
    public void Constructor_WithNullBasePath_UsesCurrentDirectory()
    {
        // Arrange & Act
        var catalog = new SkillCatalog(null!);

        // Assert
        catalog.BasePath.Should().Be(Directory.GetCurrentDirectory());
    }

    [Fact]
    public async Task GetSkillsAsync_WithNoRulesync_ReturnsEmptyList()
    {
        // Arrange
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.GetSkillsAsync();

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSkillsAsync_WithNoSkillsDirectory_ReturnsEmptyList()
    {
        // Arrange
        string rulesyncDir = Path.Combine(this.testDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.GetSkillsAsync();

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSkillsAsync_WithEmptySkillsDirectory_ReturnsEmptyList()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills");
        Directory.CreateDirectory(skillsDir);
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.GetSkillsAsync();

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSubagentsAsync_WithNoRulesync_ReturnsEmptyList()
    {
        // Arrange
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var subagents = await catalog.GetSubagentsAsync();

        // Assert
        subagents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCommandsAsync_WithNoRulesync_ReturnsEmptyList()
    {
        // Arrange
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var commands = await catalog.GetCommandsAsync();

        // Assert
        commands.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithNoQuery_ReturnsAllSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, null, null, 100);

        // Assert
        skills.Should().ContainSingle();
        skills[0].Name.Should().Be("test-skill");
    }

    [Fact]
    public async Task SearchSkillsAsync_WithEmptyQuery_ReturnsAllSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(string.Empty, null, null, null, null, 100);

        // Assert
        skills.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithWhitespaceQuery_ReturnsAllSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync("   ", null, null, null, null, 100);

        // Assert
        skills.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithCategoryFilter_ReturnsFilteredSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\ncategory: testing\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, "testing", null, null, null, 100);

        // Assert
        skills.Should().ContainSingle();
        skills[0].Category.Should().Be("testing");
    }

    [Fact]
    public async Task SearchSkillsAsync_WithNonMatchingCategory_ReturnsEmpty()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\ncategory: testing\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, "architecture", null, null, null, 100);

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithComplexityFilter_ReturnsFilteredSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\ncomplexity: beginner\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, "beginner", null, 100);

        // Assert
        skills.Should().ContainSingle();
        skills[0].Complexity.Should().Be("beginner");
    }

    [Fact]
    public async Task SearchSkillsAsync_WithPlatformFilter_ReturnsFilteredSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\ntargets:\n  - claudecode\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, null, "claudecode", 100);

        // Assert
        skills.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithWildcardTarget_ReturnsAllSkills()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\ntargets:\n  - \"*\"\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, null, "claudecode", 100);

        // Assert
        skills.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills");
        for (int i = 0; i < 20; i++)
        {
            string skillDir = Path.Combine(skillsDir, $"test{i}");
            Directory.CreateDirectory(skillDir);
            await File.WriteAllTextAsync(
                Path.Combine(skillDir, "SKILL.md"),
                $"---\nname: test-skill-{i}\ndescription: Test skill\n---\n\nContent");
        }

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, null, null, 5);

        // Assert
        skills.Should().HaveCount(5);
    }

    [Fact]
    public async Task SearchSkillsAsync_WithZeroLimit_ReturnsEmpty()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, null, null, 0);

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSkillsAsync_WithNegativeLimit_ReturnsEmpty()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skills = await catalog.SearchSkillsAsync(null, null, null, null, null, -1);

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSkillByNameAsync_WithExistingSkill_ReturnsSkill()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skill = await catalog.GetSkillByNameAsync("test-skill");

        // Assert
        skill.Should().NotBeNull();
        skill!.Name.Should().Be("test-skill");
    }

    [Fact]
    public async Task GetSkillByNameAsync_WithCaseInsensitiveMatch_ReturnsSkill()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: Test-Skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skill = await catalog.GetSkillByNameAsync("test-skill");

        // Assert
        skill.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSkillByNameAsync_WithNonExistingSkill_ReturnsNull()
    {
        // Arrange
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skill = await catalog.GetSkillByNameAsync("nonexistent");

        // Assert
        skill.Should().BeNull();
    }

    [Fact]
    public async Task GetSkillByNameAsync_WithNullName_ReturnsNull()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skill = await catalog.GetSkillByNameAsync(null!);

        // Assert
        skill.Should().BeNull();
    }

    [Fact]
    public async Task GetSkillByNameAsync_WithEmptyName_ReturnsNull()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var skill = await catalog.GetSkillByNameAsync(string.Empty);

        // Assert
        skill.Should().BeNull();
    }

    [Fact]
    public async Task GetStatsAsync_WithNoRulesync_ReturnsEmptyStats()
    {
        // Arrange
        var catalog = new SkillCatalog(this.testDir);

        // Act
        var stats = await catalog.GetStatsAsync();

        // Assert
        stats.TotalSkills.Should().Be(0);
        stats.TotalSubagents.Should().Be(0);
        stats.TotalCommands.Should().Be(0);
        stats.TotalLines.Should().Be(0);
    }

    [Fact]
    public async Task GetStatsAsync_WithSkills_ReturnsStats()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills", "test");
        Directory.CreateDirectory(skillsDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "SKILL.md"),
            "---\nname: test-skill\ndescription: Test skill\ncategory: testing\ntags:\n  - dotnet\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var stats = await catalog.GetStatsAsync();

        // Assert
        stats.TotalSkills.Should().Be(1);
        stats.TotalLines.Should().BeGreaterThan(0);
        stats.SkillsByCategory.Should().ContainKey("testing");
        stats.TopTags.Should().ContainKey("dotnet");
    }

    [Fact]
    public async Task GetStatsAsync_WithMultipleCategories_GroupsByCategory()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills");
        Directory.CreateDirectory(Path.Combine(skillsDir, "test1"));
        Directory.CreateDirectory(Path.Combine(skillsDir, "test2"));
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test1", "SKILL.md"),
            "---\nname: test-skill-1\ndescription: Test skill\ncategory: testing\n---\n\nContent");
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test2", "SKILL.md"),
            "---\nname: test-skill-2\ndescription: Test skill\ncategory: architecture\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var stats = await catalog.GetStatsAsync();

        // Assert
        stats.SkillsByCategory.Should().ContainKey("testing").WhoseValue.Should().Be(1);
        stats.SkillsByCategory.Should().ContainKey("architecture").WhoseValue.Should().Be(1);
    }

    [Fact]
    public async Task GetStatsAsync_WithMultipleComplexityLevels_GroupsByComplexity()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills");
        Directory.CreateDirectory(Path.Combine(skillsDir, "test1"));
        Directory.CreateDirectory(Path.Combine(skillsDir, "test2"));
        Directory.CreateDirectory(Path.Combine(skillsDir, "test3"));
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test1", "SKILL.md"),
            "---\nname: test-skill-1\ndescription: Test skill\ncomplexity: beginner\n---\n\nContent");
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test2", "SKILL.md"),
            "---\nname: test-skill-2\ndescription: Test skill\ncomplexity: intermediate\n---\n\nContent");
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test3", "SKILL.md"),
            "---\nname: test-skill-3\ndescription: Test skill\ncomplexity: advanced\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var stats = await catalog.GetStatsAsync();

        // Assert
        stats.SkillsByComplexity.Should().ContainKey("beginner").WhoseValue.Should().Be(1);
        stats.SkillsByComplexity.Should().ContainKey("intermediate").WhoseValue.Should().Be(1);
        stats.SkillsByComplexity.Should().ContainKey("advanced").WhoseValue.Should().Be(1);
    }

    [Fact]
    public async Task GetStatsAsync_WithDuplicateTags_CountsCorrectly()
    {
        // Arrange
        string skillsDir = Path.Combine(this.testDir, ".rulesync", "skills");
        Directory.CreateDirectory(Path.Combine(skillsDir, "test1"));
        Directory.CreateDirectory(Path.Combine(skillsDir, "test2"));
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test1", "SKILL.md"),
            "---\nname: test-skill-1\ndescription: Test skill\ntags:\n  - dotnet\n  - csharp\n---\n\nContent");
        await File.WriteAllTextAsync(
            Path.Combine(skillsDir, "test2", "SKILL.md"),
            "---\nname: test-skill-2\ndescription: Test skill\ntags:\n  - dotnet\n  - testing\n---\n\nContent");

        var catalog = new SkillCatalog(this.testDir);

        // Act
        var stats = await catalog.GetStatsAsync();

        // Assert
        stats.TotalTags.Should().Be(3); // dotnet, csharp, testing
        stats.TopTags.Should().ContainKey("dotnet").WhoseValue.Should().Be(2);
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
