namespace DotnetAgentHarness.Cli.Models;

/// <summary>
/// Represents metadata about a subagent parsed from its markdown frontmatter.
/// </summary>
public sealed class SubagentInfo
{
    /// <summary>
    /// The unique identifier for the subagent (from frontmatter name field).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A brief description of the subagent.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Target platforms this subagent supports.
    /// </summary>
    public IReadOnlyList<string> Targets { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The semantic version of the subagent.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// The author of the subagent.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// The primary role or specialty of this subagent.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Tags for categorization and discovery.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The file path to the subagent definition.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// The approximate line count of the subagent content.
    /// </summary>
    public int LineCount { get; init; }

    /// <summary>
    /// Platform-specific configuration blocks parsed from frontmatter.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> PlatformConfig { get; init; } = new();
}
