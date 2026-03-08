using Xunit;
using NSubstitute;
using DotnetAgentHarness.Cli.Services;
using DotnetAgentHarness.Cli.Utils;
using FluentAssertions;

namespace DotnetAgentHarness.Cli.Tests.Services;

public class RulesyncRunnerTests
{
    private readonly IProcessRunner _processRunner;
    private readonly RulesyncRunner _runner;

    public RulesyncRunnerTests()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _runner = new RulesyncRunner(_processRunner);
    }

    [Fact]
    public async Task FetchAsync_ExecutesCorrectCommand()
    {
        // Arrange
        var source = "owner/repo";
        var path = "/test/path";
        _processRunner.RunAsync("rulesync", Arg.Any<string>(), path)
            .Returns(Task.FromResult(new ProcessResult(0, "Success", "")));

        // Act
        var result = await _runner.FetchAsync(source, path);

        // Assert
        result.Success.Should().BeTrue();
        await _processRunner.Received().RunAsync("rulesync", "fetch \"owner/repo:.rulesync\"", path);
    }

    [Fact]
    public async Task FetchAsync_ReturnsFailureOnError()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(1, "", "Failed")));

        // Act
        var result = await _runner.FetchAsync("owner/repo", "/path");

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task InstallAsync_ExecutesCorrectCommand()
    {
        // Arrange
        var path = "/test/path";
        _processRunner.RunAsync("rulesync", "install", path)
            .Returns(Task.FromResult(new ProcessResult(0, "Success", "")));

        // Act
        var result = await _runner.InstallAsync(path);

        // Assert
        result.Success.Should().BeTrue();
        await _processRunner.Received().RunAsync("rulesync", "install", path);
    }

    [Fact]
    public async Task GenerateAsync_WithConfigFile_DoesNotPassTargets()
    {
        // Arrange
        var path = "/test/path";
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), path)
            .Returns(Task.FromResult(new ProcessResult(0, "Success", "")));

        // Act
        var result = await _runner.GenerateAsync("claudecode,copilot", path, useConfigFile: true, dryRun: false);

        // Assert
        result.Success.Should().BeTrue();
        await _processRunner.Received().RunAsync("rulesync", "generate", path);
    }

    [Fact]
    public async Task GenerateAsync_WithoutConfigFile_PassesTargetsAndFeatures()
    {
        // Arrange
        var path = "/test/path";
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), path)
            .Returns(Task.FromResult(new ProcessResult(0, "Success", "")));

        // Act
        var result = await _runner.GenerateAsync("claudecode,copilot", path, useConfigFile: false, dryRun: false);

        // Assert
        result.Success.Should().BeTrue();
        await _processRunner.Received().RunAsync("rulesync", "generate --targets claudecode,copilot --features \"*\"", path);
    }

    [Fact]
    public async Task GenerateAsync_WithDryRun_AddsDryRunFlag()
    {
        // Arrange
        var path = "/test/path";
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), path)
            .Returns(Task.FromResult(new ProcessResult(0, "Success", "")));

        // Act
        var result = await _runner.GenerateAsync("claudecode", path, useConfigFile: false, dryRun: true);

        // Assert
        result.Success.Should().BeTrue();
        await _processRunner.Received().RunAsync("rulesync", Arg.Is<string>(s => s.Contains("--dry-run")), path);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsFailureOnError()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(1, "", "Failed")));

        // Act
        var result = await _runner.GenerateAsync("claudecode", "/path", false, false);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateAsync_CapturesOutput()
    {
        // Arrange
        var expectedOutput = "Generated successfully";
        _processRunner.RunAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, expectedOutput, "")));

        // Act
        var result = await _runner.GenerateAsync("claudecode", "/path", false, false);

        // Assert
        result.Output.Should().Be(expectedOutput);
    }
}
