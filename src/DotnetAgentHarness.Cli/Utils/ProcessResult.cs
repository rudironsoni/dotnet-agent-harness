namespace DotnetAgentHarness.Cli.Utils;

public sealed record ProcessResult(
    int ExitCode,
    string Output,
    string Error);
