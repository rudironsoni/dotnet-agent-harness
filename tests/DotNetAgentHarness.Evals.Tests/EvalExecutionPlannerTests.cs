using System.Linq;
using DotNetAgentHarness.Evals.Engine;
using DotNetAgentHarness.Evals.Models;
using Xunit;

namespace DotNetAgentHarness.Evals.Tests;

public class EvalExecutionPlannerTests
{
    [Fact]
    public void Build_FiltersByPlatformAndAddsRetirementScenario()
    {
        EvalCase[] cases =
        [
            new EvalCase
            {
                Id = "claude-case",
                Prompt = "Review this change.",
                ExpectedTrigger = "dotnet-code-review-agent",
                TrialCount = 3,
                Platforms = ["claudecode"]
            },
            new EvalCase
            {
                Id = "retirement-case",
                Prompt = "Decide how to test this value object.",
                ExpectedTrigger = "dotnet-testing-strategy",
                UnloadedExpectedTrigger = "none",
                TrialCount = 4,
                Platforms = ["codexcli"]
            }
        ];

        var plan = EvalExecutionPlanner.Build(cases, "codex", defaultTrialCount: 3);

        var plannedCase = Assert.Single(plan);
        Assert.Equal("retirement-case", plannedCase.EvalCase.Id);
        Assert.Equal("codexcli", plannedCase.SelectedPlatform);
        Assert.Equal(4, plannedCase.TrialCount);
        Assert.Equal(2, plannedCase.Scenarios.Count);
        Assert.Equal(["loaded", "unloaded"], plannedCase.Scenarios.Select(item => item.Name).ToArray());
        Assert.Contains("Platform context: codexcli.", plannedCase.Scenarios[0].Prompt);
        Assert.Contains("treat 'dotnet-testing-strategy' as unavailable/retired", plannedCase.Scenarios[1].Prompt);
    }

    [Theory]
    [InlineData("codex", "codexcli")]
    [InlineData("claude", "claudecode")]
    [InlineData("gemini", "geminicli")]
    [InlineData("github-copilot-cli", "copilot")]
    [InlineData("google-antigravity", "antigravity")]
    public void NormalizePlatform_MapsKnownAliases(string input, string expected)
    {
        var normalized = EvalExecutionPlanner.NormalizePlatform(input);

        Assert.Equal(expected, normalized);
    }
}
