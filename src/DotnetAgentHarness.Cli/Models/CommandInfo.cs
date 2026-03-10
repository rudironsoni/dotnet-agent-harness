namespace DotnetAgentHarness.Cli.Models;

/// <summary>
/// Represents metadata about a command parsed from its markdown frontmatter.
/// </summary>
public sealed class CommandInfo
{
    /// <summary>
    /// The unique identifier for the command (from frontmatter name field).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A brief description of the command.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Target platforms this command supports.
    /// </summary>
    public IReadOnlyList<string> Targets { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Portability classification (universal, claude-opencode, copilot-gemini, etc.).
    /// </summary>
    public string? Portability { get; init; }

    /// <summary>
    /// Flattening risk level (low, medium, high).
    /// </summary>
    public string? FlatteningRisk { get; init; }

    /// <summary>
    /// Whether this command is simulated for certain platforms.
    /// </summary>
    public bool Simulated { get; init; }

    /// <summary>
    /// The semantic version of the command.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// The author of the command.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Tags for categorization and discovery.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The file path to the command definition.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// The approximate line count of the command content.
    /// </summary>
    public int LineCount { get; init; }

    /// <summary>
    /// Platform-specific configuration blocks parsed from frontmatter.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> PlatformConfig { get; init; } = new();
}
