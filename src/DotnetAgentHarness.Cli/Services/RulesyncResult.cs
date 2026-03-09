namespace DotnetAgentHarness.Cli.Services;

public sealed record RulesyncResult(
    bool Success,
    string? Error = null);
