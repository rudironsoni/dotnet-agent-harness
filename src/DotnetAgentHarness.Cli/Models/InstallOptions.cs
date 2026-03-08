namespace DotnetAgentHarness.Cli.Models;

public record InstallOptions
{
    public string Source { get; init; } = "rudironsoni/dotnet-agent-harness";
    public string Targets { get; init; } = "claudecode,copilot,opencode,geminicli,factorydroid,codexcli,antigravity";
    public string Path { get; init; } = ".";
    public bool Force { get; init; }
    public bool DryRun { get; init; }
    public bool Verbose { get; init; }
}
