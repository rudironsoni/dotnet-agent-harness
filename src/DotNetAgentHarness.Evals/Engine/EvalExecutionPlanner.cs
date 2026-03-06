using System;
using System.Collections.Generic;
using System.Linq;
using DotNetAgentHarness.Evals.Models;

namespace DotNetAgentHarness.Evals.Engine;

public static class EvalExecutionPlanner
{
    public static IReadOnlyList<PlannedEvalCase> Build(
        IReadOnlyList<EvalCase> cases,
        string? platformFilter,
        int defaultTrialCount,
        bool unloadedCheckOnly = false)
    {
        ArgumentNullException.ThrowIfNull(cases);

        var selectedPlatform = NormalizePlatform(platformFilter);

        return cases
            .Where(@case => MatchesPlatform(@case, selectedPlatform) && MatchesUnloadedCheck(@case, unloadedCheckOnly))
            .Select(@case => BuildCase(@case, selectedPlatform, defaultTrialCount))
            .ToList();
    }

    private static PlannedEvalCase BuildCase(EvalCase evalCase, string selectedPlatform, int defaultTrialCount)
    {
        var scenarios = new List<PlannedEvalScenario>
        {
            new(
                Name: "loaded",
                Prompt: BuildPrompt(evalCase.Prompt, selectedPlatform, unavailableTrigger: null),
                ExpectedTrigger: evalCase.ExpectedTrigger)
        };

        if (!string.IsNullOrWhiteSpace(evalCase.UnloadedExpectedTrigger))
        {
            scenarios.Add(new PlannedEvalScenario(
                Name: "unloaded",
                Prompt: BuildPrompt(evalCase.Prompt, selectedPlatform, unavailableTrigger: evalCase.ExpectedTrigger),
                ExpectedTrigger: evalCase.UnloadedExpectedTrigger));
        }

        return new PlannedEvalCase(
            EvalCase: evalCase,
            TrialCount: evalCase.TrialCount ?? defaultTrialCount,
            SelectedPlatform: selectedPlatform,
            Scenarios: scenarios);
    }

    public static string BuildPrompt(string userPrompt, string? platform, string? unavailableTrigger)
    {
        var normalizedPlatform = NormalizePlatform(platform);
        var platformLine = string.IsNullOrWhiteSpace(normalizedPlatform)
            ? string.Empty
            : $"\nPlatform context: {normalizedPlatform}.";
        var retirementLine = string.IsNullOrWhiteSpace(unavailableTrigger)
            ? string.Empty
            : $"\nCapability retirement constraint: treat '{unavailableTrigger}' as unavailable/retired and choose a different route if needed.";

        return $$"""
            You are evaluating agent skill routing behavior.
            For the user prompt below, return ONLY JSON with this schema:
            { "response": "assistant answer", "trigger": "skill-id-or-none" }

            Rules:
            - trigger MUST be a single skill id (for example: dotnet-architect) or "none".
            - response MUST be plain text without markdown fences.
            - Do not add keys beyond response and trigger.{{platformLine}}{{retirementLine}}

            User prompt:
            {{userPrompt}}
            """;
    }

    private static bool MatchesPlatform(EvalCase evalCase, string selectedPlatform)
    {
        if (string.IsNullOrWhiteSpace(selectedPlatform))
        {
            return true;
        }

        if (evalCase.Platforms is null || evalCase.Platforms.Count == 0)
        {
            return true;
        }

        return evalCase.Platforms
            .Select(NormalizePlatform)
            .Any(platform => string.Equals(platform, selectedPlatform, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesUnloadedCheck(EvalCase evalCase, bool unloadedCheckOnly)
    {
        return !unloadedCheckOnly || !string.IsNullOrWhiteSpace(evalCase.UnloadedExpectedTrigger);
    }

    public static string NormalizePlatform(string? platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
        {
            return string.Empty;
        }

        return platform.Trim().ToLowerInvariant() switch
        {
            "codex" => "codexcli",
            "claude" => "claudecode",
            "gemini" => "geminicli",
            "github-copilot" => "copilot",
            "github-copilot-cli" => "copilot",
            "copilotcli" => "copilot",
            "google-antigravity" => "antigravity",
            _ => platform.Trim().ToLowerInvariant()
        };
    }
}

public sealed record PlannedEvalCase(
    EvalCase EvalCase,
    int TrialCount,
    string SelectedPlatform,
    IReadOnlyList<PlannedEvalScenario> Scenarios);

public sealed record PlannedEvalScenario(
    string Name,
    string Prompt,
    string ExpectedTrigger);
