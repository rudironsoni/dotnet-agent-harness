using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class GeneratedArtifactContractEngineTests
{
    [Fact]
    public void Validate_PassesForFocusedRuleSyncMatrix()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WriteGeneratedArtifactMatrix(repo);

        var checks = GeneratedArtifactContractEngine.Validate(repo.Root);

        Assert.All(checks, check => Assert.True(check.Passed, $"{check.Name}: {check.Evidence}"));
    }

    [Fact]
    public void Validate_FailsWhenFactoryDroidHooksAreMissing()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WriteGeneratedArtifactMatrix(repo);
        repo.WriteFile(".factory/settings.json", """
            {
              "hooks": {}
            }
            """);

        var checks = GeneratedArtifactContractEngine.Validate(repo.Root);

        var hookCheck = Assert.Single(checks, check => check.Name == "rulesync-generated-hooks");
        Assert.False(hookCheck.Passed);
        Assert.Contains("factorydroid", hookCheck.Evidence);
    }

    [Fact]
    public void Validate_FailsWhenRulesyncFeaturesDriftFromRuntimeCapabilities()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WriteGeneratedArtifactMatrix(repo);
        repo.WriteFile("rulesync.jsonc", """
            {
              "$schema": "https://raw.githubusercontent.com/dyoshikawa/rulesync/refs/heads/main/config-schema.json",
              "targets": ["claudecode", "copilot", "opencode", "geminicli", "codexcli", "antigravity", "factorydroid"],
              "features": {
                "claudecode": ["rules", "ignore", "mcp", "commands", "subagents", "skills", "hooks"],
                "copilot": ["rules", "mcp", "commands", "subagents", "skills", "hooks"],
                "opencode": ["rules", "mcp", "commands", "subagents", "skills", "hooks"],
                "geminicli": ["rules", "ignore", "mcp", "commands", "skills", "hooks"],
                "codexcli": ["rules", "mcp", "commands", "subagents", "skills"],
                "antigravity": ["rules", "commands", "skills"],
                "factorydroid": ["rules", "mcp", "hooks"]
              }
            }
            """);

        var checks = GeneratedArtifactContractEngine.Validate(repo.Root);

        var configCheck = Assert.Single(checks, check => check.Name == "rulesync-config-target-matrix");
        Assert.False(configCheck.Passed);
        Assert.Contains("codexcli runtime surfaces mismatch", configCheck.Evidence);
    }
}
