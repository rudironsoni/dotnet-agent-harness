namespace DotnetAgentHarness.Cli.Tests.Commands;

using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

public class AnalyzeCommandTests
{
    private readonly ICodeAnalyzer codeAnalyzer;
    private readonly AnalyzeCommand command;

    public AnalyzeCommandTests()
    {
        this.codeAnalyzer = Substitute.For<ICodeAnalyzer>();
        this.command = new AnalyzeCommand(this.codeAnalyzer);
    }

    [Fact]
    public void Constructor_SetsCommandNameAndDescription()
    {
        // Assert
        this.command.Name.Should().Be("analyze");
        this.command.Description.Should().Contain("code analysis");
    }

    [Fact]
    public void Constructor_HasPathArgument()
    {
        // Act
        Argument? pathArg = this.command.Arguments.FirstOrDefault(a => a.Name == "path");

        // Assert
        pathArg.Should().NotBeNull();
        pathArg!.Description.Should().Contain("project");
    }

    [Fact]
    public void Constructor_HasSeverityOption()
    {
        // Act
        Option? severityOpt = this.command.Options.FirstOrDefault(o => o.Name == "severity");

        // Assert
        severityOpt.Should().NotBeNull();
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
    public void Constructor_HasOutputOption()
    {
        // Act
        Option? outputOpt = this.command.Options.FirstOrDefault(o => o.Name == "output");

        // Assert
        outputOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasWarningsAsErrorsOption()
    {
        // Act
        Option? warningsOpt = this.command.Options.FirstOrDefault(o => o.Name == "warnings-as-errors");

        // Assert
        warningsOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasStyleCopOption()
    {
        // Act
        Option? styleCopOpt = this.command.Options.FirstOrDefault(o => o.Name == "stylecop");

        // Assert
        styleCopOpt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_HasSonarOption()
    {
        // Act
        Option? sonarOpt = this.command.Options.FirstOrDefault(o => o.Name == "sonar");

        // Assert
        sonarOpt.Should().NotBeNull();
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
    public void AnalysisSeverity_HasCorrectValues()
    {
        // Assert
        AnalysisSeverity.Error.Should().Be(AnalysisSeverity.Error);
        AnalysisSeverity.Warning.Should().Be(AnalysisSeverity.Warning);
        AnalysisSeverity.Info.Should().Be(AnalysisSeverity.Info);
    }

    [Fact]
    public void AnalysisOutputFormat_HasCorrectValues()
    {
        // Assert
        AnalysisOutputFormat.Text.Should().Be(AnalysisOutputFormat.Text);
        AnalysisOutputFormat.Json.Should().Be(AnalysisOutputFormat.Json);
        AnalysisOutputFormat.Sarif.Should().Be(AnalysisOutputFormat.Sarif);
    }
}
