namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using System.IO.Abstractions.TestingHelpers;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class ExportCommandTests
{
    private readonly ISkillCatalog skillCatalog;
    private readonly MockFileSystem fileSystem;
    private readonly ExportCommand command;

    public ExportCommandTests()
    {
        this.skillCatalog = Substitute.For<ISkillCatalog>();
        this.fileSystem = new MockFileSystem();
        this.command = new ExportCommand(this.skillCatalog, this.fileSystem);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("export");
        this.command.Description.Should().Contain("Export");
    }

    [Fact]
    public void Constructor_HasOutputArgument()
    {
        // Act
        Argument? outputArg = this.command.Arguments.FirstOrDefault(a => a.Name == "output");

        // Assert
        outputArg.Should().NotBeNull();
        outputArg!.Description.Should().Contain("Output");
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
    public void Constructor_HasIncludeSkillsOption()
    {
        // Act
        Option? skillsOpt = this.command.Options.FirstOrDefault(o => o.Name == "include-skills");

        // Assert
        skillsOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasIncludeSubagentsOption()
    {
        // Act
        Option? subagentsOpt = this.command.Options.FirstOrDefault(o => o.Name == "include-subagents");

        // Assert
        subagentsOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasIncludeCommandsOption()
    {
        // Act
        Option? commandsOpt = this.command.Options.FirstOrDefault(o => o.Name == "include-commands");

        // Assert
        commandsOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasIncludeRulesOption()
    {
        // Act
        Option? rulesOpt = this.command.Options.FirstOrDefault(o => o.Name == "include-rules");

        // Assert
        rulesOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasPrettyOption()
    {
        // Act
        Option? prettyOpt = this.command.Options.FirstOrDefault(o => o.Name == "pretty");

        // Assert
        prettyOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasVerboseOption()
    {
        // Act
        Option? verboseOpt = this.command.Options.FirstOrDefault(o => o.Name == "verbose");

        // Assert
        verboseOpt.Should().NotBeNull();
    }
}
