namespace DotnetAgentHarness.Cli.Services;

public sealed record PrerequisiteResult(
    bool Success,
    string RulesyncVersion,
    string? ErrorMessage = null);
