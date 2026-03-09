namespace DotnetAgentHarness.Cli.Models;

public sealed class InstallOptions
{
    public string Source { get; set; } = "rudironsoni/dotnet-agent-harness";

    public string Targets { get; set; } = "claudecode,copilot,opencode,geminicli,factorydroid,codexcli,antigravity";

    public string Path { get; set; } = ".";

    public bool Force { get; set; }

    public bool DryRun { get; set; }

    public bool Verbose { get; set; }
}
