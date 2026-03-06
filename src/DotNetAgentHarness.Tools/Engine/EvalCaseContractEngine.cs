using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DotNetAgentHarness.Tools.Engine;

public static class EvalCaseContractEngine
{
    private static readonly string[] RequiredPlatforms =
    [
        PromptPlatforms.ClaudeCode,
        PromptPlatforms.OpenCode,
        PromptPlatforms.CodexCli,
        PromptPlatforms.GeminiCli,
        PromptPlatforms.Copilot,
        PromptPlatforms.Antigravity
    ];

    public static IReadOnlyList<ValidationCheck> Validate(string repoRoot)
    {
        var casePath = Path.Combine(repoRoot, "tests", "eval", "cases", "routing.yaml");
        if (!File.Exists(casePath))
        {
            return [];
        }

        try
        {
            var cases = LoadCases(casePath);
            return
            [
                ValidatePlatformMatrix(cases),
                ValidateTrialPolicy(cases),
                ValidateControlCoverage(cases)
            ];
        }
        catch (Exception ex)
        {
            return
            [
                new ValidationCheck
                {
                    Name = "eval-case-contracts",
                    Passed = false,
                    Severity = "error",
                    Message = $"Failed to validate eval case contracts: {ex.Message}",
                    Remediation = "Fix tests/eval/cases/routing.yaml so eval contract validation can parse and enforce platform coverage."
                }
            ];
        }
    }

    private static List<EvalContractCase> LoadCases(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<List<EvalContractCase>>(File.ReadAllText(filePath)) ?? [];
    }

    private static ValidationCheck ValidatePlatformMatrix(IReadOnlyList<EvalContractCase> cases)
    {
        var declaredPlatforms = cases
            .SelectMany(@case => @case.Platforms)
            .Select(PromptBundleRenderer.NormalizePlatform)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = RequiredPlatforms
            .Where(platform => !declaredPlatforms.Contains(platform))
            .ToList();
        var casesMissingPlatforms = cases
            .Where(@case => @case.Platforms.Count == 0)
            .Select(@case => @case.Id)
            .ToList();
        var passed = missing.Count == 0 && casesMissingPlatforms.Count == 0;

        var evidence = new List<string>();
        if (missing.Count > 0)
        {
            evidence.Add($"missing-platforms: {string.Join(", ", missing)}");
        }

        if (casesMissingPlatforms.Count > 0)
        {
            evidence.Add($"cases-without-platforms: {string.Join(", ", casesMissingPlatforms)}");
        }

        return new ValidationCheck
        {
            Name = "eval-platform-matrix",
            Passed = passed,
            Severity = passed ? "info" : "error",
            Message = passed
                ? $"Eval cases cover all {RequiredPlatforms.Length} target platforms."
                : "Eval cases do not yet cover the full platform matrix.",
            Evidence = string.Join(Environment.NewLine, evidence),
            Remediation = passed
                ? string.Empty
                : "Add explicit `platforms` lists to eval cases until Claude, OpenCode, Codex CLI, Gemini CLI, Copilot, and Antigravity are all represented."
        };
    }

    private static ValidationCheck ValidateTrialPolicy(IReadOnlyList<EvalContractCase> cases)
    {
        var invalid = cases
            .Where(@case => @case.TrialCount is < 3 or > 5)
            .Select(@case => $"{@case.Id}={@case.TrialCount}")
            .ToList();

        return new ValidationCheck
        {
            Name = "eval-trial-policy",
            Passed = invalid.Count == 0,
            Severity = invalid.Count == 0 ? "info" : "error",
            Message = invalid.Count == 0
                ? "All eval cases use 3 to 5 trials."
                : "Some eval cases do not meet the 3 to 5 trial policy.",
            Evidence = string.Join(Environment.NewLine, invalid),
            Remediation = invalid.Count == 0
                ? string.Empty
                : "Set `trial_count` to a value between 3 and 5 for each eval case to match the multi-trial harness policy."
        };
    }

    private static ValidationCheck ValidateControlCoverage(IReadOnlyList<EvalContractCase> cases)
    {
        var hasNegative = cases.Any(@case =>
            @case.ExpectedTrigger.Equals("none", StringComparison.OrdinalIgnoreCase)
            || @case.CaseType.Equals("negative", StringComparison.OrdinalIgnoreCase));
        var hasRetirement = cases.Any(@case =>
            !string.IsNullOrWhiteSpace(@case.UnloadedExpectedTrigger)
            || @case.CaseType.Equals("retirement", StringComparison.OrdinalIgnoreCase));
        var passed = hasNegative && hasRetirement;

        return new ValidationCheck
        {
            Name = "eval-control-coverage",
            Passed = passed,
            Severity = passed ? "info" : "error",
            Message = passed
                ? "Eval cases include negative controls and at least one retirement/unloaded check."
                : "Eval cases are missing negative controls or retirement coverage.",
            Evidence = $"negative={hasNegative}; retirement={hasRetirement}",
            Remediation = passed
                ? string.Empty
                : "Add at least one negative control (`expected_trigger: none`) and one retirement case with `unloaded_expected_trigger`."
        };
    }

    private sealed class EvalContractCase
    {
        public string Id { get; set; } = string.Empty;
        public string ExpectedTrigger { get; set; } = string.Empty;
        public int? TrialCount { get; set; }
        public string CaseType { get; set; } = string.Empty;
        public List<string> Platforms { get; set; } = new();
        public string UnloadedExpectedTrigger { get; set; } = string.Empty;
    }
}
