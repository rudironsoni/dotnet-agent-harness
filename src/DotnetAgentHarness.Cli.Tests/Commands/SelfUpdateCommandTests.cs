namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using FluentAssertions;
using Xunit;

public class SelfUpdateCommandTests
{
    private readonly SelfUpdateCommand command;

    public SelfUpdateCommandTests()
    {
        this.command = new SelfUpdateCommand();
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("self-update");
        this.command.Description.Should().Contain("Update");
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
    public void Constructor_HasCorrectAlias()
    {
        // Act
        Option<bool>? forceOpt = this.command.Options.FirstOrDefault(o => o.Name == "force") as Option<bool>;

        // Assert
        forceOpt.Should().NotBeNull();
        forceOpt!.Aliases.Should().Contain("-f");
    }

    [Fact]
    public void Constructor_WithNoDependencies_CreatesValidCommand()
    {
        // Arrange & Act
        var command = new SelfUpdateCommand();

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("self-update");
        command.Options.Should().HaveCount(1);
    }
}
