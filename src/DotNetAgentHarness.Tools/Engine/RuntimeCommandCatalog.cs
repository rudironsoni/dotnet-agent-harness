using System.Collections.Generic;

namespace DotNetAgentHarness.Tools.Engine;

public sealed class RuntimeCommandSpec
{
    public string Name { get; init; } = string.Empty;
    public string Usage { get; init; } = string.Empty;
    public string? CommandDocPath { get; init; }
}

public static class RuntimeCommandCatalog
{
    public static readonly IReadOnlyList<RuntimeCommandSpec> All =
    [
        new() { Name = "lint-frontmatter", Usage = "lint-frontmatter" },
        new() { Name = "build-manifest", Usage = "build-manifest [--output path]" },
        new() { Name = "build-catalog", Usage = "build-catalog [--output path]" },
        new() { Name = "bootstrap", Usage = "bootstrap [--profile core|platform-native|full] [--targets claudecode,opencode,codexcli,geminicli,copilot,antigravity,factorydroid] [--features csv] [--enable-pack dotnet-intelligence] [--source owner/repo] [--source-path .rulesync] [--config path] [--tool-version x.y.z] [--run-rulesync] [--force] [--no-save] [--format text|json]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-bootstrap.md" },
        new() { Name = "analyze", Usage = "analyze [--format text|json] [--write-state]" },
        new() { Name = "recommend", Usage = "recommend [--format text|json] [--limit N] [--profile path] [--platform generic|codexcli|claudecode|opencode|geminicli|copilot|antigravity|factorydroid] [--category value] [--write-state]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-recommend.md" },
        new() { Name = "doctor", Usage = "doctor [--format text|json] [--profile path] [--write-state]" },
        new() { Name = "validate", Usage = "validate [--mode contracts|platforms|repo|skill|eval|all] [--run] [--target path] [--configuration Debug|Release] [--framework tfm] [--timeout-ms N] [--skip-restore] [--skip-build] [--skip-test] [--format text|json]" },
        new() { Name = "metadata", Usage = "metadata <packages|namespaces|types|type> [--target path|--assembly path] [--query value] [--namespace value] [--type Fully.Qualified.Name] [--configuration Debug|Release] [--framework tfm] [--build] [--limit N] [--format text|json]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-metadata.md" },
        new() { Name = "search", Usage = "search <query> [--kind skill|subagent|command|persona] [--category value] [--platform value] [--limit N]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-search.md" },
        new() { Name = "profile", Usage = "profile [catalog-item-id] [--format text|json]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-profile.md" },
        new() { Name = "compare", Usage = "compare <left-id> <right-id> [--format text|json]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-compare.md" },
        new() { Name = "graph", Usage = "graph [--item id|--skill id] [--kind skill|subagent|command|persona] [--category value] [--depth N] [--format mermaid|dot|json] [--output path]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-graph.md" },
        new() { Name = "export-mcp", Usage = "export-mcp [--output directory] [--report-output path] [--platform generic|codexcli|claudecode|opencode|geminicli|copilot|antigravity|factorydroid] [--kind skill|subagent|command|persona|rule|mcp|all] [--format text|json]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-export-mcp.md" },
        new() { Name = "compare-prompts", Usage = "compare-prompts <left-evidence-id> <right-evidence-id> [--format text|json]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-compare-prompts.md" },
        new() { Name = "prepare", Usage = "prepare <request> [--persona id] [--target path] [--platform generic|codexcli|claudecode|opencode|geminicli|copilot|antigravity|factorydroid] [--limit N] [--write-evidence] [--evidence-id id] [--format text|json|prompt]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-prepare-message.md" },
        new() { Name = "incident", Usage = "incident add <title> --prompt-evidence id [--incident-id id] [--severity low|medium|high|critical] [--owner name] [--notes text] [--format text|json]" },
        new() { Name = "review", Usage = "review [path] [--format text|json] [--limit N]" },
        new() { Name = "test", Usage = "test [skill <skill-name|all>|eval] [--all] [--format text|json|junit] [--filter value] [--fail-fast] [--platform generic|codexcli|claudecode|opencode|geminicli|copilot|antigravity] [--trials N] [--unloaded-check] [--dummy-mode true|false|--real-mode] [--cases path] [--artifact-id id]", CommandDocPath = ".rulesync/commands/dotnet-agent-harness-test.md" },
        new() { Name = "scaffold", Usage = "scaffold [list|template] [destination] [--name SolutionName] [--dry-run] [--force]" }
    ];
}
