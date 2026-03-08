namespace DotnetAgentHarness.Cli.Models;

public enum TargetPlatform
{
    ClaudeCode,
    Copilot,
    OpenCode,
    GeminiCli,
    FactoryDroid,
    CodexCli,
    Antigravity
}

public static class TargetPlatformExtensions
{
    public static string ToTargetString(this TargetPlatform platform) => platform switch
    {
        TargetPlatform.ClaudeCode => "claudecode",
        TargetPlatform.Copilot => "copilot",
        TargetPlatform.OpenCode => "opencode",
        TargetPlatform.GeminiCli => "geminicli",
        TargetPlatform.FactoryDroid => "factorydroid",
        TargetPlatform.CodexCli => "codexcli",
        TargetPlatform.Antigravity => "antigravity",
        _ => throw new ArgumentOutOfRangeException(nameof(platform))
    };

    public static string GetGeneratedFile(this TargetPlatform platform) => platform switch
    {
        TargetPlatform.ClaudeCode => "AGENTS.md",
        TargetPlatform.Copilot => ".github/prompts",
        TargetPlatform.OpenCode => "opencode.jsonc",
        TargetPlatform.GeminiCli => "geminicli.jsonc",
        TargetPlatform.FactoryDroid => "factory-rules",
        TargetPlatform.CodexCli => "codex.json",
        TargetPlatform.Antigravity => ".antigravity",
        _ => throw new ArgumentOutOfRangeException(nameof(platform))
    };
}
