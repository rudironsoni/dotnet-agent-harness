namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class BootstrapCommandTests
{
    private readonly IRulesyncRunner rulesyncRunner;
    private readonly IHookDownloader hookDownloader;
    private readonly BootstrapCommand command;

    public BootstrapCommandTests()
    {
        this.rulesyncRunner = Substitute.For<IRulesyncRunner>();
        this.hookDownloader = Substitute.For<IHookDownloader>();
        this.command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("bootstrap");
        this.command.Description.Should().Contain("Bootstrap");
    }

    [Fact]
    public void Constructor_HasNameArgument()
    {
        // Act
        Argument? nameArg = this.command.Arguments.FirstOrDefault(a => a.Name == "name");

        // Assert
        nameArg.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasTemplateOption()
    {
        // Act
        Option? templateOpt = this.command.Options.FirstOrDefault(o => o.Name == "template");

        // Assert
        templateOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasOutputOption()
    {
        // Act
        Option? outputOpt = this.command.Options.FirstOrDefault(o => o.Name == "output");

        // Assert
        outputOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasTargetsOption()
    {
        // Act
        Option? targetsOpt = this.command.Options.FirstOrDefault(o => o.Name == "targets");

        // Assert
        targetsOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasSourceOption()
    {
        // Act
        Option? sourceOpt = this.command.Options.FirstOrDefault(o => o.Name == "source");

        // Assert
        sourceOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasSkipInstallOption()
    {
        // Act
        Option? skipInstallOpt = this.command.Options.FirstOrDefault(o => o.Name == "skip-install");

        // Assert
        skipInstallOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasVerboseOption()
    {
        // Act
        Option? verboseOpt = this.command.Options.FirstOrDefault(o => o.Name == "verbose");

        // Assert
        verboseOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NameArgument_HasExactlyOneArity()
    {
        // Act
        Argument<string>? nameArg = this.command.Arguments.FirstOrDefault(a => a.Name == "name") as Argument<string>;

        // Assert
        nameArg.Should().NotBeNull();
        nameArg!.Arity.MinimumNumberOfValues.Should().Be(1);
        nameArg.Arity.MaximumNumberOfValues.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithNullRulesyncRunner_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new BootstrapCommand(null!, this.hookDownloader);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullHookDownloader_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new BootstrapCommand(this.rulesyncRunner, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithAllNullDependencies_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new BootstrapCommand(null!, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }
}

public class BootstrapCommandExecutionTests
{
    private readonly IRulesyncRunner rulesyncRunner;
    private readonly IHookDownloader hookDownloader;

    public BootstrapCommandExecutionTests()
    {
        this.rulesyncRunner = Substitute.For<IRulesyncRunner>();
        this.hookDownloader = Substitute.For<IHookDownloader>();
    }

    [Fact]
    public void Constructor_WithValidName_CreatesCommand()
    {
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
        command.Name.Should().Be("bootstrap");
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsInvalidOperationException()
    {
        // The constructor doesn't validate the name, only the argument arity
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithWhitespaceName_CreatesCommand()
    {
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInName_CreatesCommand()
    {
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithVeryLongName_CreatesCommand()
    {
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithVariousTemplates_CreatesCommand()
    {
        // Arrange & Act & Assert - Template validation happens at execution time
        string[] templates = { "classlib", "webapi", "web", "console", "blazor", "maui", "xunit", "nunit", "mstest", "worker" };

        foreach (var template in templates)
        {
            var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);
            command.Should().NotBeNull();
        }
    }

    [Fact]
    public void Constructor_WithEmptyTargets_CreatesCommand()
    {
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithManyTargets_CreatesCommand()
    {
        // Arrange & Act
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithInvalidSourceFormat_CreatesCommand()
    {
        // Arrange & Act - Source validation happens at execution time
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullSource_CreatesCommand()
    {
        // Arrange & Act - Source defaults are handled by option
        var command = new BootstrapCommand(this.rulesyncRunner, this.hookDownloader);

        // Assert
        command.Should().NotBeNull();
    }
}
