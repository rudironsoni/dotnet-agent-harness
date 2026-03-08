using Xunit;
using NSubstitute;
using DotnetAgentHarness.Cli.Services;
using DotnetAgentHarness.Cli.Utils;
using FluentAssertions;

namespace DotnetAgentHarness.Cli.Tests.Services;

public class PrerequisiteCheckerTests
{
    private readonly IProcessRunner _processRunner;
    private readonly PrerequisiteChecker _checker;

    public PrerequisiteCheckerTests()
    {
        _processRunner = Substitute.For<IProcessRunner>();
        _checker = new PrerequisiteChecker(_processRunner);
    }

    [Fact]
    public async Task CheckAsync_WhenRulesyncNotInstalled_ReturnsFailure()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(1, "", "command not found")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not installed");
    }

    [Fact]
    public async Task CheckAsync_WhenVersionBelowMinimum_ReturnsFailure()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, "rulesync 7.14.0", "")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("too old");
        result.RulesyncVersion.Should().Be(new Version(7, 14, 0));
    }

    [Fact]
    public async Task CheckAsync_WhenVersionAtMinimum_ReturnsSuccess()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, "rulesync 7.15.0", "")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.RulesyncVersion.Should().Be(new Version(7, 15, 0));
    }

    [Fact]
    public async Task CheckAsync_WhenVersionAboveMinimum_ReturnsSuccess()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, "rulesync 8.0.0", "")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.RulesyncVersion.Should().Be(new Version(8, 0, 0));
    }

    [Fact]
    public async Task CheckAsync_ParsesVersionWithoutPrefix()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, "7.15.0", "")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.RulesyncVersion.Should().Be(new Version(7, 15, 0));
    }

    [Fact]
    public async Task CheckAsync_WhenVersionCannotBeParsed_ReturnsFailure()
    {
        // Arrange
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, "invalid version output", "")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Could not parse");
    }

    [Fact]
    public async Task CheckAsync_ParsesVersionFromMultilineOutput()
    {
        // Arrange
        var output = @"rulesync CLI
Version: 7.15.0
Copyright 2024";
        _processRunner.RunAsync(Arg.Any<string>(), "--version", Arg.Any<string>())
            .Returns(Task.FromResult(new ProcessResult(0, output, "")));

        // Act
        var result = await _checker.CheckAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.RulesyncVersion.Should().Be(new Version(7, 15, 0));
    }
}
