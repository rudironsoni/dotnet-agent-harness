using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAgentHarness.Tools.Engine;

public sealed class PlatformCapabilityProfile
{
    public string Id { get; init; } = PromptPlatforms.Generic;
    public List<string> OutputPaths { get; init; } = new();
    public List<string> Surfaces { get; init; } = new();
    public bool SupportsIgnore { get; init; }
    public bool SupportsSkills { get; init; }
    public bool SupportsSubagents { get; init; }
    public bool SupportsCommands { get; init; }
    public bool SupportsHooks { get; init; }
    public bool SupportsMcp { get; init; }
}

public static class PlatformCapabilityCatalog
{
    private static readonly PlatformCapabilityProfile Generic = new()
    {
        Id = PromptPlatforms.Generic,
        OutputPaths = [],
        Surfaces = ["skills", "subagents", "commands", "rules", "hooks", "mcp"],
        SupportsSkills = true,
        SupportsSubagents = true,
        SupportsCommands = true,
        SupportsHooks = true,
        SupportsMcp = true
    };

    private static readonly IReadOnlyDictionary<string, PlatformCapabilityProfile> Profiles =
        new Dictionary<string, PlatformCapabilityProfile>(StringComparer.OrdinalIgnoreCase)
        {
            [PromptPlatforms.ClaudeCode] = new()
            {
                Id = PromptPlatforms.ClaudeCode,
                OutputPaths = [".claude/"],
                Surfaces = ["rules", "skills", "subagents", "commands", "hooks", "mcp"],
                SupportsIgnore = true,
                SupportsSkills = true,
                SupportsSubagents = true,
                SupportsCommands = true,
                SupportsHooks = true,
                SupportsMcp = true
            },
            [PromptPlatforms.OpenCode] = new()
            {
                Id = PromptPlatforms.OpenCode,
                OutputPaths = [".opencode/", "AGENTS.md"],
                Surfaces = ["rules", "skills", "subagents", "commands", "hooks", "mcp"],
                SupportsSkills = true,
                SupportsSubagents = true,
                SupportsCommands = true,
                SupportsHooks = true,
                SupportsMcp = true
            },
            [PromptPlatforms.CodexCli] = new()
            {
                Id = PromptPlatforms.CodexCli,
                OutputPaths = [".codex/", "AGENTS.md"],
                Surfaces = ["rules", "skills", "subagents", "mcp"],
                SupportsSkills = true,
                SupportsSubagents = true,
                SupportsMcp = true
            },
            [PromptPlatforms.GeminiCli] = new()
            {
                Id = PromptPlatforms.GeminiCli,
                OutputPaths = [".gemini/", "GEMINI.md"],
                Surfaces = ["rules", "skills", "commands", "hooks", "mcp"],
                SupportsIgnore = true,
                SupportsSkills = true,
                SupportsCommands = true,
                SupportsHooks = true,
                SupportsMcp = true
            },
            [PromptPlatforms.Copilot] = new()
            {
                Id = PromptPlatforms.Copilot,
                OutputPaths = [".github/agents/", ".github/instructions/", ".github/prompts/", ".github/skills/", ".github/copilot-instructions.md"],
                Surfaces = ["rules", "skills", "subagents", "commands", "hooks", "mcp"],
                SupportsSkills = true,
                SupportsSubagents = true,
                SupportsCommands = true,
                SupportsHooks = true,
                SupportsMcp = true
            },
            [PromptPlatforms.Antigravity] = new()
            {
                Id = PromptPlatforms.Antigravity,
                OutputPaths = [".agent/"],
                Surfaces = ["rules", "skills", "commands"],
                SupportsSkills = true,
                SupportsCommands = true
            },
            [PromptPlatforms.FactoryDroid] = new()
            {
                Id = PromptPlatforms.FactoryDroid,
                OutputPaths = [".factory/"],
                Surfaces = ["rules", "hooks", "mcp"],
                SupportsHooks = true,
                SupportsMcp = true
            }
        };

    public static PlatformCapabilityProfile Resolve(string? platform)
    {
        if (string.IsNullOrWhiteSpace(platform)
            || platform.Equals(PromptPlatforms.Generic, StringComparison.OrdinalIgnoreCase))
        {
            return Generic;
        }

        var normalized = PromptBundleRenderer.NormalizePlatform(platform);
        return Profiles.TryGetValue(normalized, out var profile) ? profile : Generic;
    }

    public static IReadOnlyList<PlatformCapabilityProfile> GetTargetProfiles()
    {
        return Profiles.Values
            .OrderBy(profile => profile.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool SupportsCatalogKind(string? platform, string kind)
    {
        var profile = Resolve(platform);
        return kind switch
        {
            CatalogKinds.Skill => profile.SupportsSkills,
            CatalogKinds.Subagent => profile.SupportsSubagents,
            CatalogKinds.Command => profile.SupportsCommands,
            CatalogKinds.Persona => true,
            _ => true
        };
    }

    public static bool SupportsItem(string? platform, CatalogItem item)
    {
        if (!SupportsCatalogKind(platform, item.Kind))
        {
            return false;
        }

        var profile = Resolve(platform);
        if (profile.Id == PromptPlatforms.Generic)
        {
            return true;
        }

        return item.Platforms.Contains("*", StringComparer.OrdinalIgnoreCase)
               || item.Platforms.Any(candidate => candidate.Equals(profile.Id, StringComparison.OrdinalIgnoreCase));
    }
}
