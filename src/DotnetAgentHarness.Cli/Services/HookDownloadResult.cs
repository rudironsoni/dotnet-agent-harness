namespace DotnetAgentHarness.Cli.Services;

public sealed record HookDownloadResult(
    bool Success,
    string[] DownloadedHooks,
    string ErrorMessage);
