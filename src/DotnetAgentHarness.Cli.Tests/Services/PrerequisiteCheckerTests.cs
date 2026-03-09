namespace DotnetAgentHarness.Cli.Tests.Services;

using DotnetAgentHarness.Cli.Services;
using DotnetAgentHarness.Cli.Utils;
using NSubstitute;
using Xunit;

public class PrerequisiteCheckerTests
{
    [Fact]
    public async Task CheckAsync_WhenRulesyncInstalled_ReturnsSuccess()
    {
        // Arrange
        IProcessRunner processRunner = Substitute.For<IProcessRunner>();
        processRunner
            .RunAsync("which", "rulesync", null)
            .Returns(Task.FromResult(new ProcessResult(0, "/usr/local/bin/rulesync", string.Empty)));
        processRunner
            .RunAsync("rulesync", "--version", null)
            .Returns(Task.FromResult(new ProcessResult(0, "1.0.0", string.Empty)));

        PrerequisiteChecker checker = new(processRunner);

        // Act
        PrerequisiteResult result = await checker.CheckAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("1.0.0", result.RulesyncVersion);
    }

    [Fact]
    public async Task CheckAsync_WhenRulesyncNotInstalled_ReturnsFailure()
    {
        // Arrange
        IProcessRunner processRunner = Substitute.For<IProcessRunner>();
        processRunner
            .RunAsync("which", "rulesync", null)
            .Returns(Task.FromResult(new ProcessResult(1, string.Empty, "not found")));
        processRunner
            .RunAsync("command", "-v rulesync", null)
            .Returns(Task.FromResult(new ProcessResult(1, string.Empty, "not found")));
        processRunner
            .RunAsync("where", "rulesync", null)
            .Returns(Task.FromResult(new ProcessResult(1, string.Empty, "not found")));

        PrerequisiteChecker checker = new(processRunner);

        // Act
        PrerequisiteResult result = await checker.CheckAsync();

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }
}
