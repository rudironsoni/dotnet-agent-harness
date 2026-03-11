namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Services;
using DotnetAgentHarness.Cli.Utils;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class InstallCommandTests
{
    private readonly IPrerequisiteChecker prerequisiteChecker;
    private readonly IRulesyncRunner rulesyncRunner;
    private readonly IConfigDetector configDetector;
    private readonly ITransactionManager transactionManager;
    private readonly IHookDownloader hookDownloader;
    private readonly InstallCommand command;

    public InstallCommandTests()
    {
        this.prerequisiteChecker = Substitute.For<IPrerequisiteChecker>();
        this.rulesyncRunner = Substitute.For<IRulesyncRunner>();
        this.configDetector = Substitute.For<IConfigDetector>();
        this.transactionManager = Substitute.For<ITransactionManager>();
        this.hookDownloader = Substitute.For<IHookDownloader>();
        this.command = new InstallCommand(
            this.prerequisiteChecker,
            this.rulesyncRunner,
            this.configDetector,
            this.transactionManager,
            this.hookDownloader);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("install");
        this.command.Description.Should().Contain("Install");
    }

    [Fact]
    public void Constructor_HasSourceOption()
    {
        // Act
        Option? sourceOpt = this.command.Options.FirstOrDefault(o => o.Name == "source");

        // Assert
        sourceOpt.Should().NotBeNull();
        sourceOpt!.Description.Should().Contain("Source");
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
    public void Constructor_HasPathOption()
    {
        // Act
        Option? pathOpt = this.command.Options.FirstOrDefault(o => o.Name == "path");

        // Assert
        pathOpt.Should().NotBeNull();
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
    public void Constructor_HasDryRunOption()
    {
        // Act
        Option? dryRunOpt = this.command.Options.FirstOrDefault(o => o.Name == "dry-run");

        // Assert
        dryRunOpt.Should().NotBeNull();
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

public class InstallCommandExecutionTests : IDisposable
{
    private readonly string testDir;
    private readonly IPrerequisiteChecker prerequisiteChecker;
    private readonly IRulesyncRunner rulesyncRunner;
    private readonly IConfigDetector configDetector;
    private readonly ITransactionManager transactionManager;
    private readonly IHookDownloader hookDownloader;
    private readonly InstallCommand command;
    private bool disposedValue;

    public InstallCommandExecutionTests()
    {
        this.testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.testDir);

        this.prerequisiteChecker = Substitute.For<IPrerequisiteChecker>();
        this.rulesyncRunner = Substitute.For<IRulesyncRunner>();
        this.configDetector = Substitute.For<IConfigDetector>();
        this.transactionManager = Substitute.For<ITransactionManager>();
        this.hookDownloader = Substitute.For<IHookDownloader>();
        this.command = new InstallCommand(
            this.prerequisiteChecker,
            this.rulesyncRunner,
            this.configDetector,
            this.transactionManager,
            this.hookDownloader);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithNullPrerequisiteChecker_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        Action act = () => new InstallCommand(
            null!,
            this.rulesyncRunner,
            this.configDetector,
            this.transactionManager,
            this.hookDownloader);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRulesyncRunner_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new InstallCommand(
            this.prerequisiteChecker,
            null!,
            this.configDetector,
            this.transactionManager,
            this.hookDownloader);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullConfigDetector_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new InstallCommand(
            this.prerequisiteChecker,
            this.rulesyncRunner,
            null!,
            this.transactionManager,
            this.hookDownloader);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullTransactionManager_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new InstallCommand(
            this.prerequisiteChecker,
            this.rulesyncRunner,
            this.configDetector,
            null!,
            this.hookDownloader);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullHookDownloader_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new InstallCommand(
            this.prerequisiteChecker,
            this.rulesyncRunner,
            this.configDetector,
            this.transactionManager,
            null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithAllNullDependencies_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
#pragma warning disable CS8604
        Action act = () => new InstallCommand(null!, null!, null!, null!, null!);
#pragma warning restore CS8604

        act.Should().Throw<ArgumentNullException>();
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
