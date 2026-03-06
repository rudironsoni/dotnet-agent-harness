using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetAgentHarness.Tools.Engine;

public static class PlatformCoverageEngine
{
    private static readonly string[] SkillPlatforms =
    [
        PromptPlatforms.ClaudeCode,
        PromptPlatforms.OpenCode,
        PromptPlatforms.CodexCli,
        PromptPlatforms.Copilot,
        PromptPlatforms.GeminiCli,
        PromptPlatforms.Antigravity
    ];

    private static readonly string[] SubagentPlatforms =
    [
        PromptPlatforms.ClaudeCode,
        PromptPlatforms.OpenCode,
        PromptPlatforms.CodexCli,
        PromptPlatforms.Copilot
    ];

    public static IReadOnlyList<ValidationCheck> Validate(string repoRoot)
    {
        return
        [
            ValidateGroup(
                repoRoot,
                kindName: "skills",
                rootPath: Path.Combine(repoRoot, ".rulesync", "skills"),
                fileMatcher: filePath => Path.GetFileName(filePath).Equals("SKILL.md", StringComparison.OrdinalIgnoreCase),
                requiredPlatforms: SkillPlatforms),
            ValidateGroup(
                repoRoot,
                kindName: "subagents",
                rootPath: Path.Combine(repoRoot, ".rulesync", "subagents"),
                fileMatcher: filePath => Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase),
                requiredPlatforms: SubagentPlatforms)
        ];
    }

    private static ValidationCheck ValidateGroup(
        string repoRoot,
        string kindName,
        string rootPath,
        Func<string, bool> fileMatcher,
        IReadOnlyList<string> requiredPlatforms)
    {
        if (!Directory.Exists(rootPath))
        {
            return new ValidationCheck
            {
                Name = $"platform-coverage-{kindName}",
                Passed = false,
                Severity = "error",
                Message = $"Required RuleSync {kindName} directory '{rootPath}' is missing.",
                Remediation = $"Restore '{rootPath}' so platform coverage can be validated."
            };
        }

        var files = Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories)
            .Where(fileMatcher)
            .OrderBy(filePath => filePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var missing = new List<string>();
        var parseErrors = new List<string>();

        foreach (var filePath in files)
        {
            try
            {
                var frontmatter = MarkdownFrontmatter.Parse(File.ReadAllText(filePath));
                var missingPlatforms = requiredPlatforms
                    .Where(platform => !frontmatter.ContainsKey(platform))
                    .ToList();

                if (missingPlatforms.Count > 0)
                {
                    missing.Add($"{Path.GetRelativePath(repoRoot, filePath)} -> {string.Join(", ", missingPlatforms)}");
                }
            }
            catch (Exception ex)
            {
                parseErrors.Add($"{Path.GetRelativePath(repoRoot, filePath)} -> {ex.Message}");
            }
        }

        var evidence = missing
            .Concat(parseErrors)
            .Take(20)
            .ToList();
        var passed = missing.Count == 0 && parseErrors.Count == 0;
        var coverageTarget = string.Join(", ", requiredPlatforms);

        return new ValidationCheck
        {
            Name = $"platform-coverage-{kindName}",
            Passed = passed,
            Severity = passed ? "info" : "error",
            Message = passed
                ? $"All {files.Count} {kindName} declare {coverageTarget} platform blocks."
                : $"{missing.Count} of {files.Count} {kindName} are missing at least one required platform block.",
            Evidence = string.Join(Environment.NewLine, evidence),
            Remediation = passed
                ? string.Empty
                : $"Add the missing platform blocks to each RuleSync {kindName[..^1]} frontmatter entry so platform-native generation stays complete."
        };
    }
}
