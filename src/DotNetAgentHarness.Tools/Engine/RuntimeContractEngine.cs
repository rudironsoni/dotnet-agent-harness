using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetAgentHarness.Tools.Engine;

public static class RuntimeContractEngine
{
    private static readonly IReadOnlyList<DocumentationContract> DocumentationContracts =
    [
        new("README.md",
            ["dotnet agent-harness bootstrap", "--profile platform-native", "--enable-pack dotnet-intelligence", "dotnet agent-harness recommend", "dotnet agent-harness test all --format junit --output results.xml", ".factory/", "rulesync generate --targets \"claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid\" --features \"*\""],
            [
                "/dotnet-agent-harness:test --coverage",
                "/dotnet-agent-harness:profile --compare-with"
            ]),
        new(".rulesync/rules/overview.md",
            ["Factory Droid", "rulesync generate --targets \"claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid\" --features \"*\""],
            ["````bash", "```jsonc\n\n{"]),
        new(".rulesync/skills/dotnet-agent-harness-recommender/SKILL.md",
            ["/dotnet-agent-harness:recommend", "--platform codexcli", "--category data"],
            ["--interactive"]),
        new(".rulesync/skills/rulesync/supported-tools.md",
            ["| Factory Droid      | factorydroid |", "| Gemini CLI         | geminicli    |", "| GitHub Copilot     | copilot      |"],
            []),
        new(".rulesync/skills/dotnet-agent-harness-test-framework/SKILL.md",
            ["dotnet-agent-harness:test all --format junit --output results.xml"],
            [
                "--watch"
            ]),
        new(".rulesync/commands/dotnet-agent-harness-test.md",
            ["dotnet agent-harness test all --format junit --output results.xml", "dotnet agent-harness test eval", "--unloaded-check"],
            []),
        new(".rulesync/commands/dotnet-agent-harness-export-mcp.md",
            ["dotnet agent-harness export-mcp", "prompts/index.json", "resources/index.json"],
            []),
        new(".rulesync/commands/dotnet-agent-harness-bootstrap.md",
            ["dotnet agent-harness bootstrap", "--profile platform-native", "--enable-pack dotnet-intelligence"],
            []),
        new(".rulesync/commands/dotnet-agent-harness-metadata.md",
            ["dotnet agent-harness metadata", "--type", "--build"],
            []),
    ];

    public static IReadOnlyList<ValidationCheck> Validate(string repoRoot)
    {
        var checks = new List<ValidationCheck>();
        checks.AddRange(GeneratedArtifactContractEngine.Validate(repoRoot));
        checks.AddRange(ValidateRuntimeCommandDocs(repoRoot));
        checks.AddRange(ValidateDocumentationContracts(repoRoot));
        return checks;
    }

    private static IEnumerable<ValidationCheck> ValidateRuntimeCommandDocs(string repoRoot)
    {
        foreach (var command in RuntimeCommandCatalog.All.Where(command => !string.IsNullOrWhiteSpace(command.CommandDocPath)))
        {
            var path = Path.Combine(repoRoot, command.CommandDocPath!);
            if (!File.Exists(path))
            {
                yield return new ValidationCheck
                {
                    Name = $"runtime-command-doc:{command.Name}",
                    Passed = false,
                    Severity = "error",
                    Message = $"Runtime-backed command doc '{command.CommandDocPath}' is missing.",
                    Remediation = $"Add or restore '{command.CommandDocPath}' so generated agent commands stay aligned with 'dotnet agent-harness {command.Name}'."
                };
                continue;
            }

            var content = File.ReadAllText(path);
            var requiredSnippet = $"dotnet agent-harness {command.Name}";
            var passed = content.Contains(requiredSnippet, StringComparison.Ordinal);
            yield return new ValidationCheck
            {
                Name = $"runtime-command-doc:{command.Name}",
                Passed = passed,
                Severity = passed ? "info" : "error",
                Message = passed
                    ? $"Command doc '{command.CommandDocPath}' references the local runtime."
                    : $"Command doc '{command.CommandDocPath}' does not reference '{requiredSnippet}'.",
                Remediation = passed
                    ? string.Empty
                    : $"Update '{command.CommandDocPath}' to call '{requiredSnippet}' instead of duplicating runtime behavior."
            };
        }
    }

    private static IEnumerable<ValidationCheck> ValidateDocumentationContracts(string repoRoot)
    {
        foreach (var contract in DocumentationContracts)
        {
            var path = Path.Combine(repoRoot, contract.RelativePath);
            if (!File.Exists(path))
            {
                continue;
            }

            var content = File.ReadAllText(path);
            var missing = contract.RequiredSnippets
                .Where(snippet => !content.Contains(snippet, StringComparison.Ordinal))
                .ToList();
            var forbidden = contract.ForbiddenSnippets
                .Where(snippet => content.Contains(snippet, StringComparison.Ordinal))
                .ToList();

            var passed = missing.Count == 0 && forbidden.Count == 0;
            var evidence = new List<string>();
            if (missing.Count > 0)
            {
                evidence.Add($"missing: {string.Join("; ", missing)}");
            }

            if (forbidden.Count > 0)
            {
                evidence.Add($"forbidden: {string.Join("; ", forbidden)}");
            }

            yield return new ValidationCheck
            {
                Name = $"runtime-doc-contract:{contract.RelativePath}",
                Passed = passed,
                Severity = passed ? "info" : "error",
                Message = passed
                    ? $"Documentation contract passed for '{contract.RelativePath}'."
                    : $"Documentation drift detected in '{contract.RelativePath}'.",
                Evidence = string.Join(Environment.NewLine, evidence),
                Remediation = passed
                    ? string.Empty
                    : $"Update '{contract.RelativePath}' so documented examples match the current runtime command contract."
            };
        }
    }

    private sealed record DocumentationContract(
        string RelativePath,
        IReadOnlyList<string> RequiredSnippets,
        IReadOnlyList<string> ForbiddenSnippets);
}
