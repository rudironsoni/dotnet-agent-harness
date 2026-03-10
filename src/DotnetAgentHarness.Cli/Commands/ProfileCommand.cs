namespace DotnetAgentHarness.Cli.Commands;

using System.CommandLine;
using System.Text.Json;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;

/// <summary>
/// Command to show catalog statistics and item details.
/// </summary>
public class ProfileCommand : Command
{
    private readonly ISkillCatalog skillCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileCommand"/> class.
    /// </summary>
    public ProfileCommand(ISkillCatalog skillCatalog)
        : base("profile", "Show catalog statistics or detailed info about a specific item")
    {
        this.skillCatalog = skillCatalog;

        Argument<string?> itemArgument = new("item", "Optional item name to get detailed info for")
        {
            Arity = ArgumentArity.ZeroOrOne
        };

        Option<string> kindOption = new(
            ["--kind", "-k"],
            "Item kind: skill, subagent, or command (required with item name)");

        Option<string> formatOption = new(
            ["--format", "-f"],
            () => "text",
            "Output format: text or json");

        this.AddArgument(itemArgument);
        this.AddOption(kindOption);
        this.AddOption(formatOption);

        this.SetHandler(async (string? item, string? kind, string format) =>
        {
            await this.ExecuteAsync(item, kind, format);
        }, itemArgument, kindOption, formatOption);
    }

    private async Task ExecuteAsync(string? item, string? kind, string format)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                // Show overall catalog statistics
                CatalogStats stats = await this.skillCatalog.GetStatsAsync();

                if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    await this.OutputStatsJsonAsync(stats);
                }
                else
                {
                    await this.OutputStatsTextAsync(stats);
                }
            }
            else
            {
                // Show specific item details
                object? itemDetails = await this.GetItemAsync(item, kind);

                if (itemDetails == null)
                {
                    await Console.Error.WriteLineAsync($"Item not found: {item}");
                    Environment.Exit(1);
                    return;
                }

                if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    await this.OutputItemJsonAsync(itemDetails);
                }
                else
                {
                    await this.OutputItemTextAsync(itemDetails);
                }
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private async Task<object?> GetItemAsync(string item, string? kind)
    {
        // If kind is specified, search only that kind
        if (!string.IsNullOrWhiteSpace(kind))
        {
            return kind.ToLowerInvariant() switch
            {
                "skill" => await this.skillCatalog.GetSkillByNameAsync(item),
                "subagent" => await this.skillCatalog.GetSubagentByNameAsync(item),
                "command" => await this.skillCatalog.GetCommandByNameAsync(item),
                _ => null
            };
        }

        // Otherwise search all kinds and return first match
        var skill = await this.skillCatalog.GetSkillByNameAsync(item);
        if (skill != null) return skill;

        var subagent = await this.skillCatalog.GetSubagentByNameAsync(item);
        if (subagent != null) return subagent;

        var command = await this.skillCatalog.GetCommandByNameAsync(item);
        return command;
    }

    private async Task OutputStatsTextAsync(CatalogStats stats)
    {
        await Console.Out.WriteLineAsync("╔══════════════════════════════════════════════════════════╗");
        await Console.Out.WriteLineAsync("║           Dotnet Agent Harness Catalog Profile           ║");
        await Console.Out.WriteLineAsync("╚══════════════════════════════════════════════════════════╝");
        await Console.Out.WriteLineAsync();

        // Summary table
        await Console.Out.WriteLineAsync("Catalog Summary:");
        await Console.Out.WriteLineAsync(new string('-', 50));
        await Console.Out.WriteLineAsync($"  Total Skills:     {stats.TotalSkills,4}");
        await Console.Out.WriteLineAsync($"  Total Subagents:  {stats.TotalSubagents,4}");
        await Console.Out.WriteLineAsync($"  Total Commands:   {stats.TotalCommands,4}");
        await Console.Out.WriteLineAsync($"  Total Lines:      {stats.TotalLines,4:N0}");
        await Console.Out.WriteLineAsync($"  Unique Tags:      {stats.TotalTags,4}");
        await Console.Out.WriteLineAsync();

        // Skills by category
        if (stats.SkillsByCategory.Count > 0)
        {
            await Console.Out.WriteLineAsync("Skills by Category:");
            await Console.Out.WriteLineAsync(new string('-', 50));

            foreach (var category in stats.SkillsByCategory.OrderByDescending(kvp => kvp.Value))
            {
                string bar = new('█', Math.Min(category.Value, 40));
                await Console.Out.WriteLineAsync($"  {category.Key,-20} {category.Value,4} {bar}");
            }

            await Console.Out.WriteLineAsync();
        }

        // Skills by complexity
        if (stats.SkillsByComplexity.Count > 0)
        {
            await Console.Out.WriteLineAsync("Skills by Complexity:");
            await Console.Out.WriteLineAsync(new string('-', 50));

            foreach (var complexity in stats.SkillsByComplexity.OrderBy(kvp =>
                kvp.Key switch { "beginner" => 1, "intermediate" => 2, "advanced" => 3, _ => 4 }))
            {
                await Console.Out.WriteLineAsync($"  {complexity.Key,-20} {complexity.Value,4}");
            }

            await Console.Out.WriteLineAsync();
        }

        // Top tags
        if (stats.TopTags.Count > 0)
        {
            await Console.Out.WriteLineAsync("Top Tags:");
            await Console.Out.WriteLineAsync(new string('-', 50));

            foreach (var tag in stats.TopTags.Take(10))
            {
                await Console.Out.WriteLineAsync($"  {tag.Key,-30} {tag.Value,4}");
            }

            await Console.Out.WriteLineAsync();
        }

        // Calculate approximate tokens
        double estimatedTokens = stats.TotalLines * 4; // Rough estimate: ~4 tokens per line
        await Console.Out.WriteLineAsync("Estimates:");
        await Console.Out.WriteLineAsync(new string('-', 50));
        await Console.Out.WriteLineAsync($"  Approximate Tokens: {estimatedTokens:N0}");
        await Console.Out.WriteLineAsync();
    }

    private async Task OutputStatsJsonAsync(CatalogStats stats)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(stats, options);
        await Console.Out.WriteLineAsync(json);
    }

    private async Task OutputItemTextAsync(object item)
    {
        await Console.Out.WriteLineAsync("╔══════════════════════════════════════════════════════════╗");
        await Console.Out.WriteLineAsync("║               Catalog Item Details                       ║");
        await Console.Out.WriteLineAsync("╚══════════════════════════════════════════════════════════╝");
        await Console.Out.WriteLineAsync();

        switch (item)
        {
            case SkillInfo skill:
                await this.OutputSkillDetailsAsync(skill);
                break;
            case SubagentInfo subagent:
                await this.OutputSubagentDetailsAsync(subagent);
                break;
            case CommandInfo command:
                await this.OutputCommandDetailsAsync(command);
                break;
        }
    }

    private async Task OutputSkillDetailsAsync(SkillInfo skill)
    {
        await Console.Out.WriteLineAsync($"Type:     Skill");
        await Console.Out.WriteLineAsync($"Name:     {skill.Name}");
        await Console.Out.WriteLineAsync($"Category: {skill.Category ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Subcategory: {skill.Subcategory ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Complexity: {skill.Complexity ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Version:  {skill.Version ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Author:   {skill.Author ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Invocable: {(skill.Invocable ? "Yes" : "No")}");
        await Console.Out.WriteLineAsync($"Lines:    {skill.LineCount}");
        await Console.Out.WriteLineAsync();

        if (!string.IsNullOrEmpty(skill.Description))
        {
            await Console.Out.WriteLineAsync("Description:");
            await Console.Out.WriteLineAsync($"  {skill.Description}");
            await Console.Out.WriteLineAsync();
        }

        if (skill.Tags.Count > 0)
        {
            await Console.Out.WriteLineAsync($"Tags: {string.Join(", ", skill.Tags)}");
            await Console.Out.WriteLineAsync();
        }

        if (skill.Targets.Count > 0)
        {
            await Console.Out.WriteLineAsync($"Targets: {string.Join(", ", skill.Targets)}");
            await Console.Out.WriteLineAsync();
        }

        if (skill.RelatedSkills.Count > 0)
        {
            await Console.Out.WriteLineAsync("Related Skills:");
            foreach (string related in skill.RelatedSkills)
            {
                await Console.Out.WriteLineAsync($"  - {related}");
            }

            await Console.Out.WriteLineAsync();
        }

        if (!string.IsNullOrEmpty(skill.FilePath))
        {
            await Console.Out.WriteLineAsync($"File: {skill.FilePath}");
        }
    }

    private async Task OutputSubagentDetailsAsync(SubagentInfo subagent)
    {
        await Console.Out.WriteLineAsync($"Type:     Subagent");
        await Console.Out.WriteLineAsync($"Name:     {subagent.Name}");
        await Console.Out.WriteLineAsync($"Role:     {subagent.Role ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Version:  {subagent.Version ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Author:   {subagent.Author ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Lines:    {subagent.LineCount}");
        await Console.Out.WriteLineAsync();

        if (!string.IsNullOrEmpty(subagent.Description))
        {
            await Console.Out.WriteLineAsync("Description:");
            await Console.Out.WriteLineAsync($"  {subagent.Description}");
            await Console.Out.WriteLineAsync();
        }

        if (subagent.Tags.Count > 0)
        {
            await Console.Out.WriteLineAsync($"Tags: {string.Join(", ", subagent.Tags)}");
            await Console.Out.WriteLineAsync();
        }

        if (subagent.Targets.Count > 0)
        {
            await Console.Out.WriteLineAsync($"Targets: {string.Join(", ", subagent.Targets)}");
            await Console.Out.WriteLineAsync();
        }

        if (!string.IsNullOrEmpty(subagent.FilePath))
        {
            await Console.Out.WriteLineAsync($"File: {subagent.FilePath}");
        }
    }

    private async Task OutputCommandDetailsAsync(CommandInfo command)
    {
        await Console.Out.WriteLineAsync($"Type:     Command");
        await Console.Out.WriteLineAsync($"Name:     {command.Name}");
        await Console.Out.WriteLineAsync($"Portability: {command.Portability ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Flattening Risk: {command.FlatteningRisk ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Simulated: {(command.Simulated ? "Yes" : "No")}");
        await Console.Out.WriteLineAsync($"Version:  {command.Version ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Author:   {command.Author ?? "N/A"}");
        await Console.Out.WriteLineAsync($"Lines:    {command.LineCount}");
        await Console.Out.WriteLineAsync();

        if (!string.IsNullOrEmpty(command.Description))
        {
            await Console.Out.WriteLineAsync("Description:");
            await Console.Out.WriteLineAsync($"  {command.Description}");
            await Console.Out.WriteLineAsync();
        }

        if (command.Tags.Count > 0)
        {
            await Console.Out.WriteLineAsync($"Tags: {string.Join(", ", command.Tags)}");
            await Console.Out.WriteLineAsync();
        }

        if (command.Targets.Count > 0)
        {
            await Console.Out.WriteLineAsync($"Targets: {string.Join(", ", command.Targets)}");
            await Console.Out.WriteLineAsync();
        }

        if (!string.IsNullOrEmpty(command.FilePath))
        {
            await Console.Out.WriteLineAsync($"File: {command.FilePath}");
        }
    }

    private async Task OutputItemJsonAsync(object item)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(item, options);
        await Console.Out.WriteLineAsync(json);
    }
}
