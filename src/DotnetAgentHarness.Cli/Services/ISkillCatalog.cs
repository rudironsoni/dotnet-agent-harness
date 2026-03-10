namespace DotnetAgentHarness.Cli.Services;

using DotnetAgentHarness.Cli.Models;

/// <summary>
/// Interface for discovering and querying skills, subagents, and commands.
/// </summary>
public interface ISkillCatalog
{
    /// <summary>
    /// Gets the base path where .rulesync is located.
    /// </summary>
    string BasePath { get; }

    /// <summary>
    /// Discovers all skills from the .rulesync directory.
    /// </summary>
    Task<IReadOnlyList<SkillInfo>> GetSkillsAsync(CancellationToken ct = default);

    /// <summary>
    /// Discovers all subagents from the .rulesync directory.
    /// </summary>
    Task<IReadOnlyList<SubagentInfo>> GetSubagentsAsync(CancellationToken ct = default);

    /// <summary>
    /// Discovers all commands from the .rulesync directory.
    /// </summary>
    Task<IReadOnlyList<CommandInfo>> GetCommandsAsync(CancellationToken ct = default);

    /// <summary>
    /// Searches skills by keyword, category, or tags.
    /// </summary>
    Task<IReadOnlyList<SkillInfo>> SearchSkillsAsync(
        string? query = null,
        string? category = null,
        string? subcategory = null,
        string? complexity = null,
        string? platform = null,
        int limit = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Searches subagents by keyword or tags.
    /// </summary>
    Task<IReadOnlyList<SubagentInfo>> SearchSubagentsAsync(
        string? query = null,
        string? platform = null,
        int limit = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Searches commands by keyword or tags.
    /// </summary>
    Task<IReadOnlyList<CommandInfo>> SearchCommandsAsync(
        string? query = null,
        string? platform = null,
        int limit = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a specific skill by name.
    /// </summary>
    Task<SkillInfo?> GetSkillByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific subagent by name.
    /// </summary>
    Task<SubagentInfo?> GetSubagentByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific command by name.
    /// </summary>
    Task<CommandInfo?> GetCommandByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets catalog statistics.
    /// </summary>
    Task<CatalogStats> GetStatsAsync(CancellationToken ct = default);
}

/// <summary>
/// Statistics about the skill catalog.
/// </summary>
public sealed class CatalogStats
{
    /// <summary>
    /// Total number of skills.
    /// </summary>
    public int TotalSkills { get; init; }

    /// <summary>
    /// Total number of subagents.
    /// </summary>
    public int TotalSubagents { get; init; }

    /// <summary>
    /// Total number of commands.
    /// </summary>
    public int TotalCommands { get; init; }

    /// <summary>
    /// Total lines of content across all catalog items.
    /// </summary>
    public int TotalLines { get; init; }

    /// <summary>
    /// Skills grouped by category.
    /// </summary>
    public Dictionary<string, int> SkillsByCategory { get; init; } = new();

    /// <summary>
    /// Skills grouped by complexity level.
    /// </summary>
    public Dictionary<string, int> SkillsByComplexity { get; init; } = new();

    /// <summary>
    /// Total number of unique tags.
    /// </summary>
    public int TotalTags { get; init; }

    /// <summary>
    /// Most common tags and their counts.
    /// </summary>
    public Dictionary<string, int> TopTags { get; init; } = new();
}
