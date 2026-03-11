namespace DotnetAgentHarness.Cli.Commands;

using System.CommandLine;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DotnetAgentHarness.Cli.Services;

/// <summary>
/// Command to export the agent-harness configuration as a portable bundle.
/// </summary>
public class ExportCommand : Command
{
    private readonly ISkillCatalog skillCatalog;
    private readonly IFileSystem fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportCommand"/> class.
    /// </summary>
    public ExportCommand(ISkillCatalog skillCatalog, IFileSystem fileSystem)
        : base("export", "Export agent-harness configuration as a portable bundle")
    {
        this.skillCatalog = skillCatalog;
        this.fileSystem = fileSystem;

        Argument<string?> outputArg = new(
            "output",
            () => null,
            "Output file path (default: agent-harness-bundle.json)");

        Option<string> formatOption = new(
            new[] { "--format", "-f" },
            () => "json",
            "Output format (json, yaml)");

        Option<bool> includeSkillsOption = new(
            new[] { "--include-skills", "-s" },
            () => true,
            "Include skills in export");

        Option<bool> includeSubagentsOption = new(
            new[] { "--include-subagents", "-a" },
            () => true,
            "Include subagents in export");

        Option<bool> includeCommandsOption = new(
            new[] { "--include-commands", "-c" },
            () => true,
            "Include commands in export");

        Option<bool> includeRulesOption = new(
            new[] { "--include-rules", "-r" },
            () => false,
            "Include rules in export");

        Option<bool> prettyOption = new(
            new[] { "--pretty", "-p" },
            () => true,
            "Pretty-print the output");

        Option<bool> verboseOption = new(
            new[] { "--verbose", "-v" },
            () => false,
            "Show detailed output");

        this.AddArgument(outputArg);
        this.AddOption(formatOption);
        this.AddOption(includeSkillsOption);
        this.AddOption(includeSubagentsOption);
        this.AddOption(includeCommandsOption);
        this.AddOption(includeRulesOption);
        this.AddOption(prettyOption);
        this.AddOption(verboseOption);

        this.SetHandler(async (string? output, string format, bool includeSkills, bool includeSubagents, bool includeCommands, bool includeRules, bool pretty, bool verbose) =>
        {
            await this.ExecuteAsync(output, format, includeSkills, includeSubagents, includeCommands, includeRules, pretty, verbose);
        }, outputArg, formatOption, includeSkillsOption, includeSubagentsOption, includeCommandsOption, includeRulesOption, prettyOption, verboseOption);
    }

    private async Task<int> ExecuteAsync(
        string? output,
        string format,
        bool includeSkills,
        bool includeSubagents,
        bool includeCommands,
        bool includeRules,
        bool pretty,
        bool verbose)
    {
        try
        {
            // Determine output file
            string outputFile = output ?? $"agent-harness-bundle.{format.ToLowerInvariant()}";
            string fullOutputPath = this.fileSystem.Path.GetFullPath(outputFile);

            if (verbose)
            {
                await Console.Out.WriteLineAsync("Exporting agent-harness configuration...");
                await Console.Out.WriteLineAsync($"  Output: {fullOutputPath}");
                await Console.Out.WriteLineAsync($"  Format: {format}");
                await Console.Out.WriteLineAsync($"  Skills: {(includeSkills ? "yes" : "no")}");
                await Console.Out.WriteLineAsync($"  Subagents: {(includeSubagents ? "yes" : "no")}");
                await Console.Out.WriteLineAsync($"  Commands: {(includeCommands ? "yes" : "no")}");
                await Console.Out.WriteLineAsync($"  Rules: {(includeRules ? "yes" : "no")}");
                await Console.Out.WriteLineAsync();
            }

            // Load catalog
            var skills = await this.skillCatalog.GetSkillsAsync(CancellationToken.None);
            var subagents = await this.skillCatalog.GetSubagentsAsync(CancellationToken.None);
            var commands = await this.skillCatalog.GetCommandsAsync(CancellationToken.None);

            // Build export object
            var export = new Dictionary<string, object?>
            {
                ["version"] = "1.0.0",
                ["generatedAt"] = DateTime.UtcNow.ToString("O"),
                ["source"] = "dotnet-agent-harness",
            };

            if (includeSkills)
            {
                export["skills"] = skills.Select(s => new
                {
                    name = s.Name,
                    description = s.Description,
                    category = s.Category,
                    tags = s.Tags,
                }).ToList();
            }

            if (includeSubagents)
            {
                export["subagents"] = subagents.Select(s => new
                {
                    name = s.Name,
                    description = s.Description,
                    role = s.Role,
                    targets = s.Targets,
                }).ToList();
            }

            if (includeCommands)
            {
                export["commands"] = commands.Select(c => new
                {
                    name = c.Name,
                    description = c.Description,
                    portability = c.Portability,
                    targets = c.Targets,
                }).ToList();
            }

            // Serialize based on format
            string content;
            if (format.Equals("yaml", StringComparison.OrdinalIgnoreCase))
            {
                content = ConvertToYaml(export);
            }
            else
            {
                content = JsonSerializer.Serialize(export, new JsonSerializerOptions
                {
                    WriteIndented = pretty,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                });
            }

            // Write output
            await this.fileSystem.File.WriteAllTextAsync(fullOutputPath, content);

            // Report
            await Console.Out.WriteLineAsync($"Exported to: {fullOutputPath}");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("Bundle contents:");
            if (includeSkills) await Console.Out.WriteLineAsync($"  Skills: {skills.Count}");
            if (includeSubagents) await Console.Out.WriteLineAsync($"  Subagents: {subagents.Count}");
            if (includeCommands) await Console.Out.WriteLineAsync($"  Commands: {commands.Count}");

            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            if (verbose)
            {
                await Console.Error.WriteLineAsync(ex.StackTrace);
            }

            return 1;
        }
    }

    private static string ConvertToYaml(Dictionary<string, object?> data)
    {
        var lines = new List<string>();
        lines.Add($"version: \"{data["version"]}\"");
        lines.Add($"generatedAt: {data["generatedAt"]}");
        lines.Add($"source: {data["source"]}");
        lines.Add("");

        foreach (var kvp in data.Where(kvp => kvp.Value is System.Collections.IEnumerable && kvp.Key != "version" && kvp.Key != "generatedAt" && kvp.Key != "source"))
        {
            lines.Add($"{kvp.Key}:");

            if (kvp.Value is System.Collections.IEnumerable items)
            {
                foreach (var item in items)
                {
                    lines.Add("  -");
                    if (item is JsonElement element)
                    {
                        foreach (var prop in element.EnumerateObject())
                        {
                            lines.Add($"    {prop.Name}: {prop.Value}");
                        }
                    }
                    else if (item != null)
                    {
                        var props = item.GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            var value = prop.GetValue(item);
                            if (value != null)
                            {
                                lines.Add($"    {ToCamelCase(prop.Name)}: {value}");
                            }
                        }
                    }
                }
            }

            lines.Add("");
        }

        return string.Join("\n", lines);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
