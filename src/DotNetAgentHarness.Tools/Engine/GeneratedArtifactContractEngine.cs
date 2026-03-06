using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotNetAgentHarness.Tools.Engine;

public static class GeneratedArtifactContractEngine
{
    private static readonly IReadOnlyList<ArtifactContract> Contracts =
    [
        new(
            "rulesync-generated-rules",
            "Shared rules are generated for the focused target matrix.",
            "Run `rulesync generate --targets \"claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid\" --features \"*\"` before validating generated rules.",
            [
                new("claudecode", "CLAUDE.md", ["Factory Droid", "rulesync generate --targets \"claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid\" --features \"*\""]),
                new("claudecode", ".claude/rules/10-conventions.md", ["# Authoring Conventions"]),
                new("codexcli", ".codex/memories/10-conventions.md", ["# Authoring Conventions"]),
                new("opencode", ".opencode/memories/10-conventions.md", ["# Authoring Conventions"]),
                new("geminicli", ".gemini/memories/10-conventions.md", ["# Authoring Conventions"]),
                new("antigravity", ".agent/rules/10-conventions.md", ["# Authoring Conventions"]),
                new("copilot", ".github/instructions/10-conventions.instructions.md", ["# Authoring Conventions"]),
                new("factorydroid", ".factory/rules/10-conventions.md", ["# Authoring Conventions"])
            ]),
        new(
            "rulesync-generated-skills",
            "Skill artifacts are generated where the focused targets support skills.",
            "Regenerate the focused RuleSync targets so skill outputs stay aligned with `.rulesync/skills/`.",
            [
                new("claudecode", ".claude/skills/dotnet-advisor/SKILL.md", ["# dotnet-advisor"]),
                new("codexcli", ".codex/skills/dotnet-advisor/SKILL.md", ["dotnet-advisor"]),
                new("opencode", ".opencode/skill/dotnet-advisor/SKILL.md", ["# dotnet-advisor"]),
                new("geminicli", ".gemini/skills/dotnet-advisor/SKILL.md", ["# dotnet-advisor"]),
                new("antigravity", ".agent/skills/dotnet-advisor/SKILL.md", ["# dotnet-advisor"]),
                new("copilot", ".github/skills/dotnet-advisor/SKILL.md", ["# dotnet-advisor"])
            ]),
        new(
            "rulesync-generated-subagents",
            "Subagent artifacts are generated where the focused targets support subagents.",
            "Regenerate the focused RuleSync targets so specialist agents stay aligned with `.rulesync/subagents/`.",
            [
                new("claudecode", ".claude/agents/dotnet-architect.md", ["# dotnet-architect"]),
                new("codexcli", ".codex/agents/dotnet-architect.toml", ["name = \"dotnet-architect\""]),
                new("opencode", ".opencode/agent/dotnet-architect.md", ["# dotnet-architect"]),
                new("copilot", ".github/agents/dotnet-architect.md", ["# dotnet-architect"])
            ]),
        new(
            "rulesync-generated-commands",
            "Runtime-backed command artifacts are generated where the focused targets support commands.",
            "Regenerate the focused RuleSync targets so command prompts stay aligned with the local runtime contract.",
            [
                new("claudecode", ".claude/commands/dotnet-agent-harness-export-mcp.md", ["dotnet agent-harness export-mcp"]),
                new("opencode", ".opencode/command/dotnet-agent-harness-export-mcp.md", ["dotnet agent-harness export-mcp"]),
                new("geminicli", ".gemini/commands/dotnet-agent-harness-export-mcp.toml", ["dotnet agent-harness export-mcp"]),
                new("antigravity", ".agent/workflows/dotnet-agent-harness-export-mcp.md", ["dotnet agent-harness export-mcp"]),
                new("copilot", ".github/prompts/dotnet-agent-harness-export-mcp.prompt.md", ["dotnet agent-harness export-mcp"])
            ]),
        new(
            "rulesync-generated-mcp",
            "MCP configuration artifacts are generated where the focused targets support MCP.",
            "Regenerate the focused RuleSync targets so MCP server configuration stays aligned with `.rulesync/mcp.json`.",
            [
                new("claudecode", ".mcp.json", ["\"mcpServers\"", "\"serena\""]),
                new("codexcli", ".codex/config.toml", ["[mcp_servers.serena]"]),
                new("opencode", "opencode.json", ["\"mcp\"", "\"serena\""]),
                new("geminicli", ".gemini/settings.json", ["\"mcpServers\"", "\"serena\""]),
                new("copilot", ".vscode/mcp.json", ["\"servers\"", "\"serena\""]),
                new("factorydroid", ".factory/mcp.json", ["\"mcpServers\"", "\"serena\""])
            ]),
        new(
            "rulesync-generated-hooks",
            "Hook artifacts are generated where the focused targets support hooks.",
            "Regenerate the focused RuleSync targets so hook outputs stay aligned with `.rulesync/hooks.json`.",
            [
                new("claudecode", ".claude/settings.json", ["\"PostToolUse\"", ".rulesync/hooks/slopwatch-advisory.sh"]),
                new("opencode", ".opencode/plugins/rulesync-hooks.js", ["session.created", ".rulesync/hooks/slopwatch-advisory.sh"]),
                new("geminicli", ".gemini/settings.json", ["\"SessionStart\"", "session-start-context.sh"]),
                new("copilot", ".github/hooks/copilot-hooks.json", ["\"sessionStart\"", "session-start-context.sh"]),
                new("factorydroid", ".factory/settings.json", ["\"SessionStart\"", "$FACTORY_PROJECT_DIR/"])
            ])
    ];

    private static readonly string[] ExpectedTargets = KnownPlatforms.All;

    public static IReadOnlyList<ValidationCheck> Validate(string repoRoot)
    {
        var checks = new List<ValidationCheck>
        {
            ValidateRulesyncConfig(repoRoot)
        };

        checks.AddRange(Contracts.Select(contract => ValidateContract(repoRoot, contract)));
        return checks;
    }

    private static ValidationCheck ValidateRulesyncConfig(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "rulesync.jsonc");
        if (!File.Exists(path))
        {
            return new ValidationCheck
            {
                Name = "rulesync-config-target-matrix",
                Passed = false,
                Severity = "error",
                Message = "rulesync.jsonc is missing.",
                Remediation = "Restore `rulesync.jsonc` and keep it aligned with the focused target matrix."
            };
        }

        var content = File.ReadAllText(path);
        var configuredTargets = ExtractArray(content, "targets");
        var geminiFeatures = ExtractTargetFeatures(content, "geminicli");
        var antigravityFeatures = ExtractTargetFeatures(content, "antigravity");
        var factoryFeatures = ExtractTargetFeatures(content, "factorydroid");
        var failures = new List<string>();

        var missingTargets = ExpectedTargets
            .Where(target => !configuredTargets.Contains(target, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (missingTargets.Count > 0)
        {
            failures.Add($"missing-targets: {string.Join(", ", missingTargets)}");
        }

        if (!geminiFeatures.Contains("hooks", StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("geminicli is missing hooks");
        }

        if (antigravityFeatures.Contains("mcp", StringComparer.OrdinalIgnoreCase)
            || antigravityFeatures.Contains("hooks", StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("antigravity should not request mcp/hooks");
        }

        var expectedFactoryFeatures = new[] { "rules", "mcp", "hooks" };
        if (!expectedFactoryFeatures.All(feature => factoryFeatures.Contains(feature, StringComparer.OrdinalIgnoreCase))
            || factoryFeatures.Count != expectedFactoryFeatures.Length)
        {
            failures.Add($"factorydroid features should be exactly: {string.Join(", ", expectedFactoryFeatures)}");
        }

        foreach (var target in ExpectedTargets)
        {
            var configuredFeatures = ExtractTargetFeatures(content, target)
                .Where(feature => !feature.Equals("ignore", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(feature => feature, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var runtimeSurfaces = PlatformCapabilityCatalog.Resolve(target).Surfaces
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(feature => feature, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!configuredFeatures.SequenceEqual(runtimeSurfaces, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add($"{target} runtime surfaces mismatch rulesync features: config=[{string.Join(", ", configuredFeatures)}] runtime=[{string.Join(", ", runtimeSurfaces)}]");
            }
        }

        var passed = failures.Count == 0;
        return new ValidationCheck
        {
            Name = "rulesync-config-target-matrix",
            Passed = passed,
            Severity = passed ? "info" : "error",
            Message = passed
                ? "rulesync.jsonc matches the focused RuleSync target matrix."
                : "rulesync.jsonc is out of sync with the focused RuleSync target matrix.",
            Evidence = string.Join(Environment.NewLine, failures),
            Remediation = passed
                ? string.Empty
                : "Keep `rulesync.jsonc` aligned with `claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid`, with Gemini hooks enabled and Factory Droid limited to rules/mcp/hooks."
        };
    }

    private static ValidationCheck ValidateContract(string repoRoot, ArtifactContract contract)
    {
        var failures = new List<string>();

        foreach (var artifact in contract.Artifacts)
        {
            var path = Path.Combine(repoRoot, artifact.RelativePath);
            if (!File.Exists(path))
            {
                failures.Add($"{artifact.Target}: missing {artifact.RelativePath}");
                continue;
            }

            var content = File.ReadAllText(path);
            var missingSnippets = artifact.RequiredSnippets
                .Where(snippet => !content.Contains(snippet, StringComparison.Ordinal))
                .ToList();

            if (missingSnippets.Count > 0)
            {
                failures.Add($"{artifact.Target}: {artifact.RelativePath} missing {string.Join(", ", missingSnippets)}");
            }
        }

        var passed = failures.Count == 0;
        return new ValidationCheck
        {
            Name = contract.Name,
            Passed = passed,
            Severity = passed ? "info" : "error",
            Message = passed ? contract.SuccessMessage : $"{contract.SuccessMessage.TrimEnd('.')} Failed targets detected.",
            Evidence = string.Join(Environment.NewLine, failures),
            Remediation = passed ? string.Empty : contract.Remediation
        };
    }

    private static List<string> ExtractArray(string content, string propertyName)
    {
        var match = Regex.Match(
            content,
            $"\"{Regex.Escape(propertyName)}\"\\s*:\\s*\\[(?<items>.*?)\\]",
            RegexOptions.Singleline);

        if (!match.Success)
        {
            return [];
        }

        return Regex.Matches(match.Groups["items"].Value, "\"([^\"]+)\"")
            .Select(result => result.Groups[1].Value)
            .ToList();
    }

    private static List<string> ExtractTargetFeatures(string content, string target)
    {
        var match = Regex.Match(
            content,
            $"\"{Regex.Escape(target)}\"\\s*:\\s*\\[(?<items>.*?)\\]",
            RegexOptions.Singleline);

        if (!match.Success)
        {
            return [];
        }

        return Regex.Matches(match.Groups["items"].Value, "\"([^\"]+)\"")
            .Select(result => result.Groups[1].Value)
            .ToList();
    }

    private sealed record ArtifactContract(
        string Name,
        string SuccessMessage,
        string Remediation,
        IReadOnlyList<ArtifactExpectation> Artifacts);

    private sealed record ArtifactExpectation(
        string Target,
        string RelativePath,
        IReadOnlyList<string> RequiredSnippets);
}
