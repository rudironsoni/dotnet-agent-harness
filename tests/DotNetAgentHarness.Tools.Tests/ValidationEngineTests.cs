using System.IO;
using System.Linq;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class ValidationEngineTests
{
    [Fact]
    public void Validate_RunDotNet_BuildsSimpleConsoleRepository()
    {
        using var repo = new TestRepositoryBuilder();
        CreateConsoleRepository(repo, "SampleApp");

        var report = ValidationEngine.Validate(repo.Root, "repo", new ValidationOptions
        {
            RunDotNet = true,
            TimeoutMs = 120_000
        });

        Assert.True(report.Passed);
        Assert.True(
            report.Target.EndsWith("SampleApp.sln", System.StringComparison.OrdinalIgnoreCase)
            || report.Target.EndsWith("SampleApp.slnx", System.StringComparison.OrdinalIgnoreCase),
            $"Unexpected validation target '{report.Target}'.");
        Assert.Contains(report.Checks, check => check.Name == "dotnet-target" && check.Passed);
        Assert.Contains(report.Checks, check => check.Name == "dotnet-restore" && check.Passed);
        Assert.Contains(report.Checks, check => check.Name == "dotnet-build" && check.Passed);
        Assert.Contains(report.Checks, check => check.Name == "dotnet-test-skipped" && check.Passed);
    }

    [Fact]
    public void Validate_RunDotNet_ReportsCompilerDiagnostics()
    {
        using var repo = new TestRepositoryBuilder();
        CreateConsoleRepository(repo, "BrokenApp");
        repo.WriteFile("src/BrokenApp/Program.cs", """
            Console.WriteLine("broken")
            """);

        var report = ValidationEngine.Validate(repo.Root, "repo", new ValidationOptions
        {
            RunDotNet = true,
            SkipTest = true,
            TimeoutMs = 120_000
        });

        Assert.False(report.Passed);
        var buildCheck = Assert.Single(report.Checks, check => check.Name == "dotnet-build");
        Assert.False(buildCheck.Passed);
        Assert.Contains("CS", buildCheck.Message);
        Assert.Contains("compiler error", buildCheck.Remediation, System.StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(buildCheck.Evidence));
    }

    [Fact]
    public void Validate_ContractsMode_ChecksRuntimeDocsAndExamples()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WriteGeneratedArtifactMatrix(repo);
        repo.WriteFile("README.md", """
            # dotnet-agent-harness

            dotnet agent-harness bootstrap --profile platform-native --enable-pack dotnet-intelligence --run-rulesync
            dotnet agent-harness recommend --platform codexcli --category data --format json
            dotnet agent-harness test all --format junit --output results.xml
            .factory/
            rulesync generate --targets "claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid" --features "*"
            """);
        repo.WriteFile(".rulesync/rules/overview.md", """
            # dotnet-agent-harness

            Factory Droid

            ```bash
            rulesync generate --targets "claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid" --features "*"
            ```

            ```jsonc
            {
              "sources": [{ "source": "rudironsoni/dotnet-agent-harness", "path": ".rulesync" }]
            }
            ```
            """);
        repo.WriteFile(".rulesync/skills/dotnet-agent-harness-recommender/SKILL.md", """
            # recommender

            /dotnet-agent-harness:recommend
            /dotnet-agent-harness:recommend --platform codexcli
            /dotnet-agent-harness:recommend --category data
            """);
        repo.WriteFile(".rulesync/skills/rulesync/supported-tools.md", """
            | Tool               | --targets    | rules | ignore |   mcp    | commands | subagents | skills | hooks |
            | Factory Droid      | factorydroid | ✅ 🌏 |        |  ✅ 🌏   |          |           |        |   ✅   |
            | Gemini CLI         | geminicli    | ✅ 🌏 |   ✅   |  ✅ 🌏   |  ✅ 🌏   |           | ✅ 🌏  |   ✅   |
            | GitHub Copilot     | copilot      | ✅ 🌏 |        |    ✅    |    ✅    |    ✅     |   ✅   |   ✅   |
            """);
        repo.WriteFile(".rulesync/skills/dotnet-agent-harness-test-framework/SKILL.md", """
            # test framework

            dotnet-agent-harness:test all --format junit --output results.xml
            """);

        foreach (var command in RuntimeCommandCatalog.All.Where(item => !string.IsNullOrWhiteSpace(item.CommandDocPath)))
        {
            var commandText = command.Name switch
            {
                "bootstrap" => """
                    ---
                    description: test doc
                    targets: ['*']
                    ---

                    # /bootstrap

                    dotnet agent-harness bootstrap --profile platform-native --enable-pack dotnet-intelligence --run-rulesync
                    """,
                "metadata" => """
                    ---
                    description: test doc
                    targets: ['*']
                    ---

                    # /metadata

                    dotnet agent-harness metadata type --target src/App/App.csproj --type App.Core.OrderService --build
                    """,
                "test" => """
                    ---
                    description: test doc
                    targets: ['*']
                    ---

                    # /test

                    dotnet agent-harness test all --format junit --output results.xml
                    dotnet agent-harness test eval --platform codexcli --trials 3 --unloaded-check
                    """,
                "export-mcp" => """
                    ---
                    description: test doc
                    targets: ['*']
                    ---

                    # /export-mcp

                    dotnet agent-harness export-mcp --platform geminicli --output .dotnet-agent-harness/exports/mcp --report-output .dotnet-agent-harness/exports/mcp-report.json --format json
                    prompts/index.json
                    resources/index.json
                    """,
                _ => $$"""
                    ---
                    description: test doc
                    targets: ['*']
                    ---

                    # /{{command.Name}}

                    dotnet agent-harness {{command.Name}} --format json
                    """
            };
            repo.WriteFile(command.CommandDocPath!, commandText);
        }

        var report = ValidationEngine.Validate(repo.Root, "contracts");

        Assert.True(report.Passed);
        Assert.Contains(report.Checks, check => check.Name == "runtime-command-doc:recommend" && check.Passed);
        Assert.Contains(report.Checks, check => check.Name == "runtime-doc-contract:README.md" && check.Passed);
    }

    [Fact]
    public void Validate_PlatformsMode_FailsWhenPlatformBlocksAreMissing()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);
        repo.WriteFile(".rulesync/skills/dotnet-advisor/SKILL.md", """
            ---
            name: dotnet-advisor
            description: Routes .NET work
            targets: ['*']
            tags: ['dotnet']
            ---
            # dotnet-advisor
            """);

        var report = ValidationEngine.Validate(repo.Root, "platforms");

        Assert.False(report.Passed);
        var skillsCoverage = Assert.Single(report.Checks, check => check.Name == "platform-coverage-skills");
        Assert.False(skillsCoverage.Passed);
        Assert.Contains("dotnet-advisor", skillsCoverage.Evidence);
    }

    [Fact]
    public void Validate_RunDotNet_ReportsSlopwatchBaselineWarningWhenPackEnabled()
    {
        using var repo = new TestRepositoryBuilder();
        CreateConsoleRepository(repo, "QualityGateApp");
        var engine = new BootstrapEngine();
        engine.Bootstrap(repo.Root, new BootstrapOptions
        {
            EnablePacks = true
        });

        var report = ValidationEngine.Validate(repo.Root, "repo", new ValidationOptions
        {
            RunDotNet = true,
            TimeoutMs = 120_000
        });

        Assert.True(report.Passed);
        Assert.Contains(report.Checks, check => check.Name == "slopwatch-pack" && check.Passed);
        Assert.Contains(report.Checks, check => check.Name == "slopwatch-baseline" && check.Passed && check.Severity == "warning");
    }

    private static void CreateConsoleRepository(TestRepositoryBuilder repo, string solutionName)
    {
        ProcessRunner.Run("dotnet", $"new sln -n {solutionName}", repo.Root, 120_000);
        ProcessRunner.Run("dotnet", $"new console -n {solutionName} -o src/{solutionName}", repo.Root, 120_000);
        var solutionPath = Directory.EnumerateFiles(repo.Root, $"{solutionName}.sln*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Single();
        ProcessRunner.Run("dotnet", $"sln {solutionPath} add src/{solutionName}/{solutionName}.csproj", repo.Root, 120_000);
    }
}
