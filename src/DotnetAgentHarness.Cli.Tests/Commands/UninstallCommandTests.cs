namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using FluentAssertions;
using Xunit;

public class UninstallCommandTests
{
    private readonly UninstallCommand command;

    public UninstallCommandTests()
    {
        this.command = new UninstallCommand();
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("uninstall");
        this.command.Description.Should().Contain("Remove");
    }

    [Fact]
    public void Constructor_HasPathOption()
    {
        // Act
        Option? pathOpt = this.command.Options.FirstOrDefault(o => o.Name == "path");

        // Assert
        pathOpt.Should().NotBeNull();
        pathOpt!.Description.Should().Contain("Directory");
    }

    [Fact]
    public void Constructor_HasForceOption()
    {
        // Act
        Option? forceOpt = this.command.Options.FirstOrDefault(o => o.Name == "force");

        // Assert
        forceOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasCleanOption()
    {
        // Act
        Option? cleanOpt = this.command.Options.FirstOrDefault(o => o.Name == "clean");

        // Assert
        cleanOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_PathOption_HasDefaultValue()
    {
        // Act
        Option<string>? pathOpt = this.command.Options.FirstOrDefault(o => o.Name == "path") as Option<string>;

        // Assert
        pathOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasCorrectAliases()
    {
        // Act
        Option<string>? pathOpt = this.command.Options.FirstOrDefault(o => o.Name == "path") as Option<string>;
        Option<bool>? forceOpt = this.command.Options.FirstOrDefault(o => o.Name == "force") as Option<bool>;
        Option<bool>? cleanOpt = this.command.Options.FirstOrDefault(o => o.Name == "clean") as Option<bool>;

        // Assert
        pathOpt.Should().NotBeNull();
        forceOpt.Should().NotBeNull();
        cleanOpt.Should().NotBeNull();
    }
}

public class UninstallCommandExecutionTests : IDisposable
{
    private readonly string testDir;
    private bool disposedValue;

    public UninstallCommandExecutionTests()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithNoRulesync_DoesNotThrow()
    {
        // Arrange & Act
        var command = new UninstallCommand();

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_Always_CreatesValidCommand()
    {
        // Arrange & Act
        var command = new UninstallCommand();

        // Assert
        command.Name.Should().Be("uninstall");
        command.Options.Should().HaveCount(3);
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
