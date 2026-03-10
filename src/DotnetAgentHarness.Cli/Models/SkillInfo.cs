namespace DotnetAgentHarness.Cli.Models;

/// <summary>
/// Represents metadata about a skill parsed from its SKILL.md frontmatter.
/// </summary>
public sealed class SkillInfo
{
    /// <summary>
    /// The unique identifier for the skill (from frontmatter name field).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The display name or title of the skill.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// The primary category (e.g., testing, architecture, fundamentals).
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// The subcategory within the primary category.
    /// </summary>
    public string? Subcategory { get; init; }

    /// <summary>
    /// A brief description of the skill.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Tags for filtering and discovery.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Target platforms this skill supports (e.g., *, claudecode, opencode).
    /// </summary>
    public IReadOnlyList<string> Targets { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The semantic version of the skill.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// The author of the skill.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Whether this skill can be invoked by users.
    /// </summary>
    public bool Invocable { get; init; }

    /// <summary>
    /// Complexity level (beginner, intermediate, advanced).
    /// </summary>
    public string? Complexity { get; init; }

    /// <summary>
    /// Related skills for cross-referencing.
    /// </summary>
    public IReadOnlyList<string> RelatedSkills { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The file path to the skill definition.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// The directory containing the skill.
    /// </summary>
    public string? DirectoryPath { get; init; }

    /// <summary>
    /// The approximate line count of the skill content.
    /// </summary>
    public int LineCount { get; init; }

    /// <summary>
    /// Platform-specific configuration blocks parsed from frontmatter.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> PlatformConfig { get; init; } = new();
}
