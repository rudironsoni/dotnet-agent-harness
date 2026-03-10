namespace DotnetAgentHarness.Cli.Services;

using System.Text;
using System.Text.RegularExpressions;
using DotnetAgentHarness.Cli.Models;

/// <summary>
/// Implementation of ISkillCatalog that parses YAML frontmatter from markdown files.
/// </summary>
public sealed partial class SkillCatalog : ISkillCatalog
{
    // Regex to match YAML frontmatter: --- at start and end
    [GeneratedRegex(@"^---\s*\n(.*?)^---\s*\n(.*)$", RegexOptions.Singleline | RegexOptions.Multiline)]
    private static partial Regex FrontmatterRegex();

    // Regex to parse simple YAML key-value pairs
    [GeneratedRegex(@"^(\w+):\s*(.*)$", RegexOptions.Multiline)]
    private static partial Regex YamlKeyValueRegex();

    private readonly string basePath;
    private readonly string rulesyncPath;

    /// <inheritdoc />
    public string BasePath => this.basePath;

    /// <summary>
    /// Creates a new SkillCatalog instance.
    /// </summary>
    public SkillCatalog(string? basePath = null)
    {
        this.basePath = basePath ?? Directory.GetCurrentDirectory();
        this.rulesyncPath = Path.Combine(this.basePath, ".rulesync");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SkillInfo>> GetSkillsAsync(CancellationToken ct = default)
    {
        var skills = new List<SkillInfo>();
        string skillsPath = Path.Combine(this.rulesyncPath, "skills");

        if (!Directory.Exists(skillsPath))
        {
            return skills;
        }

        // Find all SKILL.md files recursively
        foreach (string skillFile in Directory.GetFiles(skillsPath, "SKILL.md", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            SkillInfo? skill = await this.ParseSkillFileAsync(skillFile, ct);
            if (skill != null)
            {
                skills.Add(skill);
            }
        }

        return skills;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SubagentInfo>> GetSubagentsAsync(CancellationToken ct = default)
    {
        var subagents = new List<SubagentInfo>();
        string subagentsPath = Path.Combine(this.rulesyncPath, "subagents");

        if (!Directory.Exists(subagentsPath))
        {
            return subagents;
        }

        // Find all markdown files in subagents directory
        foreach (string subagentFile in Directory.GetFiles(subagentsPath, "*.md", SearchOption.TopDirectoryOnly))
        {
            ct.ThrowIfCancellationRequested();
            SubagentInfo? subagent = await this.ParseSubagentFileAsync(subagentFile, ct);
            if (subagent != null)
            {
                subagents.Add(subagent);
            }
        }

        return subagents;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommandInfo>> GetCommandsAsync(CancellationToken ct = default)
    {
        var commands = new List<CommandInfo>();
        string commandsPath = Path.Combine(this.rulesyncPath, "commands");

        if (!Directory.Exists(commandsPath))
        {
            return commands;
        }

        // Find all markdown files in commands directory
        foreach (string commandFile in Directory.GetFiles(commandsPath, "*.md", SearchOption.TopDirectoryOnly))
        {
            ct.ThrowIfCancellationRequested();
            CommandInfo? command = await this.ParseCommandFileAsync(commandFile, ct);
            if (command != null)
            {
                commands.Add(command);
            }
        }

        return commands;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SkillInfo>> SearchSkillsAsync(
        string? query = null,
        string? category = null,
        string? subcategory = null,
        string? complexity = null,
        string? platform = null,
        int limit = 10,
        CancellationToken ct = default)
    {
        var skills = await this.GetSkillsAsync(ct);
        var results = skills.AsEnumerable();

        // Apply query filter
        if (!string.IsNullOrWhiteSpace(query))
        {
            string normalizedQuery = query.ToLowerInvariant();
            results = results.Where(s =>
                s.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                (s.Description?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                s.Tags.Any(t => t.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(category))
        {
            results = results.Where(s =>
                s.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Apply subcategory filter
        if (!string.IsNullOrWhiteSpace(subcategory))
        {
            results = results.Where(s =>
                s.Subcategory?.Equals(subcategory, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Apply complexity filter
        if (!string.IsNullOrWhiteSpace(complexity))
        {
            results = results.Where(s =>
                s.Complexity?.Equals(complexity, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Apply platform filter
        if (!string.IsNullOrWhiteSpace(platform))
        {
            results = results.Where(s =>
                s.Targets.Contains("*") ||
                s.Targets.Any(t => t.Equals(platform, StringComparison.OrdinalIgnoreCase)));
        }

        return results.Take(limit).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SubagentInfo>> SearchSubagentsAsync(
        string? query = null,
        string? platform = null,
        int limit = 10,
        CancellationToken ct = default)
    {
        var subagents = await this.GetSubagentsAsync(ct);
        var results = subagents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            string normalizedQuery = query.ToLowerInvariant();
            results = results.Where(s =>
                s.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                (s.Description?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                s.Tags.Any(t => t.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(platform))
        {
            results = results.Where(s =>
                s.Targets.Contains("*") ||
                s.Targets.Any(t => t.Equals(platform, StringComparison.OrdinalIgnoreCase)));
        }

        return results.Take(limit).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommandInfo>> SearchCommandsAsync(
        string? query = null,
        string? platform = null,
        int limit = 10,
        CancellationToken ct = default)
    {
        var commands = await this.GetCommandsAsync(ct);
        var results = commands.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            string normalizedQuery = query.ToLowerInvariant();
            results = results.Where(c =>
                c.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                (c.Description?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                c.Tags.Any(t => t.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(platform))
        {
            results = results.Where(c =>
                c.Targets.Contains("*") ||
                c.Targets.Any(t => t.Equals(platform, StringComparison.OrdinalIgnoreCase)));
        }

        return results.Take(limit).ToList();
    }

    /// <inheritdoc />
    public async Task<SkillInfo?> GetSkillByNameAsync(string name, CancellationToken ct = default)
    {
        var skills = await this.GetSkillsAsync(ct);
        return skills.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<SubagentInfo?> GetSubagentByNameAsync(string name, CancellationToken ct = default)
    {
        var subagents = await this.GetSubagentsAsync(ct);
        return subagents.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<CommandInfo?> GetCommandByNameAsync(string name, CancellationToken ct = default)
    {
        var commands = await this.GetCommandsAsync(ct);
        return commands.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<CatalogStats> GetStatsAsync(CancellationToken ct = default)
    {
        var skills = await this.GetSkillsAsync(ct);
        var subagents = await this.GetSubagentsAsync(ct);
        var commands = await this.GetCommandsAsync(ct);

        var skillsByCategory = skills
            .Where(s => !string.IsNullOrEmpty(s.Category))
            .GroupBy(s => s.Category!)
            .ToDictionary(g => g.Key, g => g.Count());

        var skillsByComplexity = skills
            .Where(s => !string.IsNullOrEmpty(s.Complexity))
            .GroupBy(s => s.Complexity!)
            .ToDictionary(g => g.Key, g => g.Count());

        var allTags = skills.SelectMany(s => s.Tags).ToList();
        var topTags = allTags
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(kvp => kvp.Value)
            .Take(20)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        int totalLines = skills.Sum(s => s.LineCount) +
                         subagents.Sum(s => s.LineCount) +
                         commands.Sum(c => c.LineCount);

        return new CatalogStats
        {
            TotalSkills = skills.Count,
            TotalSubagents = subagents.Count,
            TotalCommands = commands.Count,
            TotalLines = totalLines,
            SkillsByCategory = skillsByCategory,
            SkillsByComplexity = skillsByComplexity,
            TotalTags = allTags.Distinct().Count(),
            TopTags = topTags
        };
    }

    private async Task<SkillInfo?> ParseSkillFileAsync(string filePath, CancellationToken ct)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath, ct);
            var frontmatter = this.ParseFrontmatter(content);

            if (frontmatter == null || !frontmatter.ContainsKey("name"))
            {
                return null;
            }

            var directoryName = Path.GetFileName(Path.GetDirectoryName(filePath));

            return new SkillInfo
            {
                Name = frontmatter.GetValueOrDefault("name")?.ToString() ?? directoryName ?? "unknown",
                Title = frontmatter.GetValueOrDefault("title")?.ToString(),
                Category = frontmatter.GetValueOrDefault("category")?.ToString(),
                Subcategory = frontmatter.GetValueOrDefault("subcategory")?.ToString(),
                Description = frontmatter.GetValueOrDefault("description")?.ToString(),
                Tags = this.ParseStringList(frontmatter.GetValueOrDefault("tags")),
                Targets = this.ParseStringList(frontmatter.GetValueOrDefault("targets")),
                Version = frontmatter.GetValueOrDefault("version")?.ToString(),
                Author = frontmatter.GetValueOrDefault("author")?.ToString(),
                Invocable = this.ParseBool(frontmatter.GetValueOrDefault("invocable")),
                Complexity = frontmatter.GetValueOrDefault("complexity")?.ToString(),
                RelatedSkills = this.ParseStringList(frontmatter.GetValueOrDefault("related_skills")),
                FilePath = filePath,
                DirectoryPath = Path.GetDirectoryName(filePath),
                LineCount = content.Split(['\n']).Length,
                PlatformConfig = this.ParsePlatformBlocks(frontmatter)
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<SubagentInfo?> ParseSubagentFileAsync(string filePath, CancellationToken ct)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath, ct);
            var frontmatter = this.ParseFrontmatter(content);

            if (frontmatter == null || !frontmatter.ContainsKey("name"))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);

            return new SubagentInfo
            {
                Name = frontmatter.GetValueOrDefault("name")?.ToString() ?? fileName,
                Description = frontmatter.GetValueOrDefault("description")?.ToString(),
                Targets = this.ParseStringList(frontmatter.GetValueOrDefault("targets")),
                Version = frontmatter.GetValueOrDefault("version")?.ToString(),
                Author = frontmatter.GetValueOrDefault("author")?.ToString(),
                Role = frontmatter.GetValueOrDefault("role")?.ToString(),
                Tags = this.ParseStringList(frontmatter.GetValueOrDefault("tags")),
                FilePath = filePath,
                LineCount = content.Split(['\n']).Length,
                PlatformConfig = this.ParsePlatformBlocks(frontmatter)
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<CommandInfo?> ParseCommandFileAsync(string filePath, CancellationToken ct)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath, ct);
            var frontmatter = this.ParseFrontmatter(content);

            if (frontmatter == null || !frontmatter.ContainsKey("name"))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);

            return new CommandInfo
            {
                Name = frontmatter.GetValueOrDefault("name")?.ToString() ?? fileName,
                Description = frontmatter.GetValueOrDefault("description")?.ToString(),
                Targets = this.ParseStringList(frontmatter.GetValueOrDefault("targets")),
                Portability = frontmatter.GetValueOrDefault("portability")?.ToString(),
                FlatteningRisk = frontmatter.GetValueOrDefault("flattening-risk")?.ToString(),
                Simulated = this.ParseBool(frontmatter.GetValueOrDefault("simulated")),
                Version = frontmatter.GetValueOrDefault("version")?.ToString(),
                Author = frontmatter.GetValueOrDefault("author")?.ToString(),
                Tags = this.ParseStringList(frontmatter.GetValueOrDefault("tags")),
                FilePath = filePath,
                LineCount = content.Split(['\n']).Length,
                PlatformConfig = this.ParsePlatformBlocks(frontmatter)
            };
        }
        catch
        {
            return null;
        }
    }

    private Dictionary<string, object>? ParseFrontmatter(string content)
    {
        var match = FrontmatterRegex().Match(content);
        if (!match.Success)
        {
            return null;
        }

        string yamlContent = match.Groups[1].Value;
        var result = new Dictionary<string, object>();

        // Simple YAML parsing - key-value pairs
        foreach (Match m in YamlKeyValueRegex().Matches(yamlContent))
        {
            string key = m.Groups[1].Value.Trim();
            string value = m.Groups[2].Value.Trim();
            result[key] = this.ParseYamlValue(value);
        }

        return result;
    }

    private object ParseYamlValue(string value)
    {
        value = value.Trim();

        // Array: [item1, item2, item3]
        if (value.StartsWith('[') && value.EndsWith(']'))
        {
            string inner = value[1..^1];
            if (string.IsNullOrWhiteSpace(inner))
            {
                return Array.Empty<string>();
            }

            return inner.Split(',')
                .Select(s => s.Trim().Trim('\'', '\"'))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        // Boolean
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Number
        if (int.TryParse(value, out int intValue))
        {
            return intValue;
        }

        // String (remove quotes if present)
        return value.Trim('\'', '\"');
    }

    private IReadOnlyList<string> ParseStringList(object? value)
    {
        if (value is List<string> list)
        {
            return list.AsReadOnly();
        }

        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return new[] { str };
        }

        return Array.Empty<string>();
    }

    private bool ParseBool(object? value)
    {
        return value is bool b && b;
    }

    private Dictionary<string, Dictionary<string, object>> ParsePlatformBlocks(Dictionary<string, object> frontmatter)
    {
        var platforms = new[] { "claudecode", "opencode", "copilot", "codexcli", "geminicli", "antigravity", "factorydroid" };
        var result = new Dictionary<string, Dictionary<string, object>>();

        foreach (var platform in platforms)
        {
            if (frontmatter.TryGetValue(platform, out var value) && value is Dictionary<string, object> dict)
            {
                result[platform] = dict;
            }
        }

        return result;
    }
}
