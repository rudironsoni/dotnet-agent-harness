using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAgentHarness.Tools.Engine;

/// <summary>
/// Configuration options for the bootstrap command.
/// </summary>
public sealed class BootstrapOptions
{
    /// <summary>
    /// Target platforms to install (e.g., "claudecode", "opencode").
    /// </summary>
    public List<string> Targets { get; init; } = new();

    /// <summary>
    /// Overwrite existing files without prompting.
    /// </summary>
    public bool Force { get; init; }

    /// <summary>
    /// List available targets without installing.
    /// </summary>
    public bool ListTargets { get; init; }

    /// <summary>
    /// Write pack files for selected targets.
    /// </summary>
    public bool WritePackFiles { get; init; }

    /// <summary>
    /// Enable pack-based skill selection.
    /// </summary>
    public bool EnablePacks { get; init; }

    /// <summary>
    /// Maximum number of skills to include per pack.
    /// </summary>
    public int SkillLimit { get; init; }
}

/// <summary>
/// Result of a file operation during pack application.
/// </summary>
public sealed class BootstrapFileResult
{
    /// <summary>
    /// Path to the file.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Status of the file operation (created, overwritten, unchanged, skipped).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable message about the operation.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Result of a single target extraction during bootstrap.
/// </summary>
public sealed class BootstrapTargetResult
{
    /// <summary>
    /// Target identifier (e.g., "claudecode").
    /// </summary>
    public string Target { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether extraction succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if extraction failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Files that were extracted (relative paths).
    /// </summary>
    public List<string> ExtractedFiles { get; init; } = new();

    /// <summary>
    /// Duration of the extraction operation.
    /// </summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Complete bootstrap report with all results.
/// </summary>
public sealed class BootstrapReport
{
    /// <summary>
    /// Repository root where bootstrap was run.
    /// </summary>
    public string RepoRoot { get; init; } = string.Empty;

    /// <summary>
    /// Harness version that performed the installation.
    /// </summary>
    public string InstalledVersion { get; init; } = string.Empty;

    /// <summary>
    /// All targets that were processed.
    /// </summary>
    public List<BootstrapTargetResult> TargetResults { get; init; } = new();

    /// <summary>
    /// True if v1.x RuleSync installation was detected.
    /// </summary>
    public bool HasV1Installation { get; init; }

    /// <summary>
    /// Warnings encountered during bootstrap.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Suggested next steps for the user.
    /// </summary>
    public List<string> NextSteps { get; init; } = new();

    /// <summary>
    /// Overall success status. True if all targets succeeded.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// Creates a successful report for list-targets mode.
    /// </summary>
    public static BootstrapReport SuccessList(IReadOnlyList<string> availableTargets)
    {
        return new BootstrapReport
        {
            Passed = true,
            NextSteps = new List<string>
            {
                "Run 'dotnet agent-harness bootstrap --targets <platform1,platform2>' to install selected platforms."
            }
        };
    }

    /// <summary>
    /// Creates a successful report after extraction.
    /// </summary>
    public static BootstrapReport Success(
        string repoRoot,
        string version,
        List<BootstrapTargetResult> results,
        bool hasV1Installation = false)
    {
        var warnings = new List<string>();
        var nextSteps = new List<string>
        {
            "Installation complete. Your agent configuration files are ready.",
            "Restart your AI coding assistant to pick up the new configuration."
        };

        if (hasV1Installation)
        {
            warnings.Add("v1.x RuleSync installation detected. Consider removing .rulesync/ and rulesync.jsonc after verifying the new installation.");
        }

        var failedTargets = results.Where(r => !r.Success).ToList();
        if (failedTargets.Count > 0)
        {
            warnings.Add($"Some targets failed: {string.Join(", ", failedTargets.Select(t => t.Target))}");
        }

        return new BootstrapReport
        {
            RepoRoot = repoRoot,
            InstalledVersion = version,
            TargetResults = results,
            HasV1Installation = hasV1Installation,
            Warnings = warnings,
            NextSteps = nextSteps,
            Passed = results.All(r => r.Success)
        };
    }

    /// <summary>
    /// Creates a failure report when extraction fails.
    /// </summary>
    public static BootstrapReport Failure(string errorMessage, bool hasV1Installation = false)
    {
        return new BootstrapReport
        {
            Passed = false,
            Warnings = new List<string> { errorMessage },
            HasV1Installation = hasV1Installation
        };
    }
}
