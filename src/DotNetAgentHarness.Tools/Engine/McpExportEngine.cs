using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DotNetAgentHarness.Tools.Engine;

public static class McpExportEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static McpExportReport Export(string repoRoot, McpExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var normalizedRoot = Path.GetFullPath(repoRoot);
        var outputDirectory = ResolveOutputDirectory(normalizedRoot, options.OutputDirectory);
        var platform = PlatformCapabilityCatalog.Resolve(options.Platform);
        var kind = NormalizeKind(options.Kind);
        var catalog = ToolkitCatalogLoader.Load(normalizedRoot);

        ResetManagedOutputDirectories(outputDirectory);

        var prompts = new List<McpPromptExportItem>();
        var resources = new List<McpResourceExportItem>();

        foreach (var item in catalog.Items
                     .Where(item => PlatformCapabilityCatalog.SupportsItem(platform.Id, item))
                     .OrderBy(item => item.Kind, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase))
        {
            if (ShouldExportPrompt(item, kind))
            {
                prompts.Add(ExportPromptItem(normalizedRoot, outputDirectory, item));
            }

            if (ShouldExportResource(item, kind))
            {
                resources.Add(ExportResourceItem(normalizedRoot, outputDirectory, item));
            }
        }

        if (kind is "all" or "rule")
        {
            resources.AddRange(ExportRuleResources(normalizedRoot, outputDirectory));
        }

        if (kind is "all" or "mcp")
        {
            var mcpResource = ExportMcpConfig(normalizedRoot, outputDirectory);
            if (mcpResource is not null)
            {
                resources.Add(mcpResource);
            }
        }

        var promptIndexPath = Path.Combine(outputDirectory, "prompts", "index.json");
        var resourceIndexPath = Path.Combine(outputDirectory, "resources", "index.json");
        var manifestPath = Path.Combine(outputDirectory, "manifest.json");

        WriteJson(promptIndexPath, prompts);
        WriteJson(resourceIndexPath, resources);
        WriteJson(manifestPath, new
        {
            generatedAtUtc = DateTimeOffset.UtcNow,
            sourceOfTruth = ".rulesync",
            repoRoot = normalizedRoot,
            platform = platform.Id,
            kind,
            promptCount = prompts.Count,
            resourceCount = resources.Count,
            promptIndexPath = Path.GetRelativePath(outputDirectory, promptIndexPath),
            resourceIndexPath = Path.GetRelativePath(outputDirectory, resourceIndexPath)
        });

        return new McpExportReport
        {
            RepoRoot = normalizedRoot,
            OutputDirectory = outputDirectory,
            Platform = platform.Id,
            Kind = kind,
            ManifestPath = manifestPath,
            PromptIndexPath = promptIndexPath,
            ResourceIndexPath = resourceIndexPath,
            PromptCount = prompts.Count,
            ResourceCount = resources.Count,
            Prompts = prompts,
            Resources = resources
        };
    }

    private static McpPromptExportItem ExportPromptItem(string repoRoot, string outputDirectory, CatalogItem item)
    {
        var sourcePath = Path.Combine(repoRoot, item.FilePath);
        var extension = Path.GetExtension(sourcePath);
        var exportPath = Path.Combine(outputDirectory, "prompts", Pluralize(item.Kind), $"{item.Id}{extension}");
        CopyFile(sourcePath, exportPath);

        return new McpPromptExportItem
        {
            Id = item.Id,
            Kind = item.Kind,
            Name = item.Name,
            Description = item.Description,
            Uri = $"prompt://dotnet-agent-harness/{item.Kind}/{item.Id}",
            SourcePath = item.FilePath,
            ExportPath = Path.GetRelativePath(outputDirectory, exportPath),
            Platforms = item.Platforms,
            Tags = item.Tags
        };
    }

    private static McpResourceExportItem ExportResourceItem(string repoRoot, string outputDirectory, CatalogItem item)
    {
        var sourcePath = Path.Combine(repoRoot, item.FilePath);
        var extension = Path.GetExtension(sourcePath);
        var exportPath = Path.Combine(outputDirectory, "resources", Pluralize(item.Kind), $"{item.Id}{extension}");
        CopyFile(sourcePath, exportPath);

        return new McpResourceExportItem
        {
            Id = item.Id,
            Kind = item.Kind,
            Name = item.Name,
            Description = item.Description,
            Uri = $"resource://dotnet-agent-harness/{item.Kind}/{item.Id}",
            SourcePath = item.FilePath,
            ExportPath = Path.GetRelativePath(outputDirectory, exportPath),
            Platforms = item.Platforms,
            Tags = item.Tags,
            References = item.References
        };
    }

    private static IEnumerable<McpResourceExportItem> ExportRuleResources(string repoRoot, string outputDirectory)
    {
        var rulesDirectory = Path.Combine(repoRoot, ".rulesync", "rules");
        if (!Directory.Exists(rulesDirectory))
        {
            yield break;
        }

        foreach (var filePath in Directory.EnumerateFiles(rulesDirectory, "*.md", SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var id = Path.GetFileNameWithoutExtension(filePath);
            var exportPath = Path.Combine(outputDirectory, "resources", "rules", $"{id}.md");
            CopyFile(filePath, exportPath);

            yield return new McpResourceExportItem
            {
                Id = id,
                Kind = "rule",
                Name = id,
                Description = "RuleSync rule resource",
                Uri = $"resource://dotnet-agent-harness/rule/{id}",
                SourcePath = Path.GetRelativePath(repoRoot, filePath),
                ExportPath = Path.GetRelativePath(outputDirectory, exportPath),
                Platforms = ["*"],
                Tags = ["rulesync", "rule"]
            };
        }
    }

    private static McpResourceExportItem? ExportMcpConfig(string repoRoot, string outputDirectory)
    {
        var filePath = Path.Combine(repoRoot, ".rulesync", "mcp.json");
        if (!File.Exists(filePath))
        {
            return null;
        }

        var exportPath = Path.Combine(outputDirectory, "resources", "mcp", "mcp.json");
        CopyFile(filePath, exportPath);

        return new McpResourceExportItem
        {
            Id = "mcp-config",
            Kind = "mcp",
            Name = "mcp-config",
            Description = "RuleSync MCP server configuration",
            Uri = "resource://dotnet-agent-harness/mcp/config",
            SourcePath = Path.GetRelativePath(repoRoot, filePath),
            ExportPath = Path.GetRelativePath(outputDirectory, exportPath),
            Platforms = ["*"],
            Tags = ["mcp", "config"]
        };
    }

    private static bool ShouldExportPrompt(CatalogItem item, string kind)
    {
        if (kind != "all" && kind != item.Kind)
        {
            return false;
        }

        return item.Kind is CatalogKinds.Command or CatalogKinds.Persona;
    }

    private static bool ShouldExportResource(CatalogItem item, string kind)
    {
        return kind == "all" || kind == item.Kind;
    }

    private static string ResolveOutputDirectory(string repoRoot, string outputDirectory)
    {
        var relative = string.IsNullOrWhiteSpace(outputDirectory)
            ? Path.Combine(".dotnet-agent-harness", "exports", "mcp")
            : outputDirectory;
        return Path.GetFullPath(Path.IsPathRooted(relative) ? relative : Path.Combine(repoRoot, relative));
    }

    private static void ResetManagedOutputDirectories(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        foreach (var child in new[] { Path.Combine(outputDirectory, "prompts"), Path.Combine(outputDirectory, "resources") })
        {
            if (Directory.Exists(child))
            {
                Directory.Delete(child, recursive: true);
            }
        }
    }

    private static void CopyFile(string sourcePath, string destinationPath)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
    }

    private static void WriteJson<T>(string path, T payload)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static string NormalizeKind(string? kind)
    {
        var normalized = string.IsNullOrWhiteSpace(kind) ? "all" : kind.Trim().ToLowerInvariant();
        if (normalized is not ("all" or CatalogKinds.Skill or CatalogKinds.Subagent or CatalogKinds.Command or CatalogKinds.Persona or "rule" or "mcp"))
        {
            throw new ArgumentException($"Unsupported MCP export kind '{kind}'. Use skill, subagent, command, persona, rule, mcp, or all.");
        }

        return normalized;
    }

    private static string Pluralize(string kind)
    {
        return kind switch
        {
            CatalogKinds.Skill => "skills",
            CatalogKinds.Subagent => "subagents",
            CatalogKinds.Command => "commands",
            CatalogKinds.Persona => "personas",
            "rule" => "rules",
            "mcp" => "mcp",
            _ => $"{kind}s"
        };
    }
}
