using System.IO;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class EvalCaseContractEngineTests
{
    [Fact]
    public void Validate_PassesForPlatformNegativeAndRetirementCoverage()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("tests/eval/cases/routing.yaml", """
            - id: "case-1"
              expected_trigger: "dotnet-architect"
              trial_count: 3
              platforms: ["claudecode", "opencode", "codexcli", "geminicli", "copilot", "antigravity"]
              assertions:
                - type: "contains"
                  value: "dotnet"
            - id: "case-2"
              expected_trigger: "none"
              case_type: "negative"
              trial_count: 3
              platforms: ["claudecode", "opencode", "codexcli", "geminicli", "copilot", "antigravity"]
              assertions:
                - type: "not_contains"
                  value: "WCF"
            - id: "case-3"
              expected_trigger: "dotnet-testing-strategy"
              unloaded_expected_trigger: "none"
              case_type: "retirement"
              trial_count: 4
              platforms: ["claudecode", "opencode", "codexcli", "geminicli", "copilot", "antigravity"]
              assertions:
                - type: "contains"
                  value: "unit"
            """);

        var checks = EvalCaseContractEngine.Validate(repo.Root);

        Assert.Contains(checks, check => check.Name == "eval-platform-matrix" && check.Passed);
        Assert.Contains(checks, check => check.Name == "eval-trial-policy" && check.Passed);
        Assert.Contains(checks, check => check.Name == "eval-control-coverage" && check.Passed);
    }

    [Fact]
    public void Validate_FailsWhenCoverageContractsAreMissing()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("tests/eval/cases/routing.yaml", """
            - id: "case-1"
              expected_trigger: "dotnet-architect"
              trial_count: 1
              platforms: ["claudecode"]
              assertions:
                - type: "contains"
                  value: "dotnet"
            """);

        var checks = EvalCaseContractEngine.Validate(repo.Root);

        Assert.Contains(checks, check => check.Name == "eval-platform-matrix" && !check.Passed);
        Assert.Contains(checks, check => check.Name == "eval-trial-policy" && !check.Passed);
        Assert.Contains(checks, check => check.Name == "eval-control-coverage" && !check.Passed);
    }
}
