using System.IO;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class EvalHarnessEngineTests
{
    [Fact]
    public void Run_DummyModeExecutesWithEmbeddedDefaultCasesFromAnyRepo()
    {
        using var repo = new TestRepositoryBuilder();
        var artifactPath = Path.Combine(repo.Root, ".dotnet-agent-harness", "evidence", "evals", "embedded-defaults.json");

        var result = EvalHarnessEngine.Run(repo.Root, new EvalHarnessOptions
        {
            Platform = "codexcli",
            TrialCount = 3,
            UseDummyMode = true,
            UnloadedCheckOnly = true,
            ArtifactPath = artifactPath,
            ArtifactId = "embedded-defaults"
        });

        Assert.True(result.Passed, result.StandardError);
        Assert.NotNull(result.Artifact);
        Assert.Equal("codexcli", result.Artifact!.PlatformFilter);
        var evalCase = Assert.Single(result.Artifact.Cases);
        Assert.Equal("retirement", evalCase.CaseType);
        Assert.Equal("none", evalCase.UnloadedExpectedTrigger);
        Assert.True(File.Exists(artifactPath));
    }

    [Fact]
    public void Run_DummyModeExecutesUnloadedPlatformSlice()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("routing.yaml", """
            - id: "retirement-001"
              description: "Retirement check"
              prompt: "Decide how to test a value object."
              expected_trigger: "dotnet-testing-strategy"
              unloaded_expected_trigger: "none"
              trial_count: 3
              case_type: "retirement"
              platforms: ["codexcli"]
              fixture_trigger: "dotnet-testing-strategy"
              fixture_response: "Prefer unit tests."
              assertions:
                - type: "contains"
                  value: "unit"
            """);
        var artifactPath = Path.Combine(repo.Root, ".dotnet-agent-harness", "evidence", "evals", "codex-retirement-test.json");

        var result = EvalHarnessEngine.Run(repo.Root, new EvalHarnessOptions
        {
            Platform = "codexcli",
            TrialCount = 3,
            UseDummyMode = true,
            UnloadedCheckOnly = true,
            CaseFilePath = Path.Combine(repo.Root, "routing.yaml"),
            ArtifactPath = artifactPath,
            ArtifactId = "codex-retirement-test"
        });

        Assert.True(result.Passed, result.StandardError);
        Assert.False(result.TimedOut);
        Assert.NotNull(result.Artifact);
        Assert.Equal("codexcli", result.Artifact!.PlatformFilter);
        var evalCase = Assert.Single(result.Artifact.Cases);
        Assert.Equal("retirement", evalCase.CaseType);
        Assert.Equal("none", evalCase.UnloadedExpectedTrigger);
        Assert.Equal("codexcli", evalCase.SelectedPlatform);
    }
}
