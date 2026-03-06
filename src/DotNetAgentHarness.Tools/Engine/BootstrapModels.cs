using System.Collections.Generic;

namespace DotNetAgentHarness.Tools.Engine;

public sealed class BootstrapOptions
{
    public string Profile { get; init; } = BootstrapProfileCatalog.PlatformNative;
    public List<string> Targets { get; init; } = new();
    public List<string> Features { get; init; } = new();
    public List<string> EnablePacks { get; init; } = new();
    public string SourceRepository { get; init; } = ToolkitRuntimeMetadata.RuleSyncSourceRepository;
    public string SourcePath { get; init; } = ToolkitRuntimeMetadata.RuleSyncSourcePath;
    public string ConfigPath { get; init; } = "rulesync.jsonc";
    public bool Force { get; init; }
    public bool RunRuleSync { get; init; }
    public bool WriteToolManifest { get; init; } = true;
    public bool WritePackFiles { get; init; } = true;
    public bool WriteState { get; init; } = true;
    public int SkillLimit { get; init; } = 6;
    public string? ToolVersion { get; init; }
}

public sealed class BootstrapTargetProfile
{
    public string Id { get; init; } = string.Empty;
    public List<string> OutputPaths { get; init; } = new();
    public List<string> Surfaces { get; init; } = new();
}

public sealed class BootstrapPackResult
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> ToolPackageIds { get; init; } = new();
    public List<BootstrapFileResult> Files { get; init; } = new();
    public List<string> Notes { get; init; } = new();
}

public sealed class BootstrapFileResult
{
    public string Path { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed class BootstrapCommandResult
{
    public string Command { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int ExitCode { get; init; }
    public bool TimedOut { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
}

public sealed class BootstrapReport
{
    public string RepoRoot { get; init; } = string.Empty;
    public string ToolPackageId { get; init; } = string.Empty;
    public string ToolCommandName { get; init; } = string.Empty;
    public string ToolVersion { get; init; } = string.Empty;
    public string Profile { get; init; } = string.Empty;
    public List<BootstrapTargetProfile> Targets { get; init; } = new();
    public List<string> Features { get; init; } = new();
    public List<BootstrapPackResult> Packs { get; init; } = new();
    public BootstrapFileResult ToolManifest { get; init; } = new();
    public BootstrapFileResult RuleSyncConfig { get; init; } = new();
    public List<BootstrapFileResult> StateFiles { get; init; } = new();
    public bool RuleSyncAvailable { get; init; }
    public string RuleSyncGenerationStatus { get; init; } = string.Empty;
    public List<BootstrapCommandResult> Commands { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public List<string> NextSteps { get; init; } = new();
    public InitReport Init { get; init; } = new();
    public bool Passed { get; init; }
}
