namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class UpdateCommandTests
{
    private readonly IRulesyncRunner rulesyncRunner;
    private readonly IHookDownloader hookDownloader;
    private readonly UpdateCommand command;

    public UpdateCommandTests()
    {
        this.rulesyncRunner = Substitute.For<IRulesyncRunner>();
        this.hookDownloader = Substitute.For<IHookDownloader>();
        this.command = new UpdateCommand(this.rulesyncRunner, this.hookDownloader);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("update");
        this.command.Description.Should().Contain("Update");
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
    public void Constructor_HasDryRunOption()
    {
        // Act
        Option? dryRunOpt = this.command.Options.FirstOrDefault(o => o.Name == "dry-run");

        // Assert
        dryRunOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullRulesyncRunner_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new UpdateCommand(null!, this.hookDownloader);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullHookDownloader_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new UpdateCommand(this.rulesyncRunner, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithAllNullDependencies_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new UpdateCommand(null!, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }
}
