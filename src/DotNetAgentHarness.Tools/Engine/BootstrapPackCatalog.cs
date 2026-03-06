using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DotNetAgentHarness.Tools.Engine;

public static class BootstrapPackCatalog
{
    public const string DotNetIntelligence = "dotnet-intelligence";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly Dictionary<string, BootstrapPackDefinition> Packs = new(StringComparer.OrdinalIgnoreCase)
    {
        [DotNetIntelligence] = new()
        {
            Id = DotNetIntelligence,
            Description = "Install Slopwatch and seed a repo-local intelligence quality gate for agent-driven .NET edits.",
            ToolSpecs =
            [
                new BootstrapToolInstallSpec
                {
                    PackageId = "Slopwatch.Cmd",
                    Version = "0.4.0",
                    Commands = ["slopwatch"],
                    RollForward = false
                }
            ],
            Notes =
            [
                "Shared generated hooks stay advisory-only and emit findings as context instead of blocking edits.",
                "Runtime repo validation can enforce Slopwatch with `dotnet agent-harness validate --mode repo --run` once a baseline exists."
            ]
        }
    };

    public static List<BootstrapPackDefinition> ResolveMany(IReadOnlyList<string> requestedPacks)
    {
        if (requestedPacks.Count == 0)
        {
            return [];
        }

        var resolved = new List<BootstrapPackDefinition>();
        foreach (var pack in requestedPacks
                     .Select(value => value.Trim())
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Packs.TryGetValue(pack, out var definition))
            {
                throw new ArgumentException($"Unsupported bootstrap pack '{pack}'. Supported packs: {string.Join(", ", Packs.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))}.");
            }

            resolved.Add(new BootstrapPackDefinition
            {
                Id = definition.Id,
                Description = definition.Description,
                ToolSpecs = definition.ToolSpecs.ToList(),
                Notes = definition.Notes.ToList()
            });
        }

        return resolved;
    }

    public static List<BootstrapFileResult> Apply(string repoRoot, BootstrapPackDefinition pack, bool writeFiles)
    {
        return pack.Id.Equals(DotNetIntelligence, StringComparison.OrdinalIgnoreCase)
            ? ApplyDotNetIntelligence(repoRoot, writeFiles)
            : [];
    }

    private static List<BootstrapFileResult> ApplyDotNetIntelligence(string repoRoot, bool writeFiles)
    {
        var configPath = Path.Combine(repoRoot, ".slopwatch", "config.json");

        if (!writeFiles)
        {
            return
            [
                new BootstrapFileResult
                {
                    Path = configPath,
                    Status = "skipped",
                    Message = "Skipped writing the dotnet-intelligence pack files because persistence was disabled."
                }
            ];
        }

        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

        var content = new JsonObject
        {
            ["globalSuppressions"] = new JsonArray(),
            ["suppressions"] = new JsonArray()
        }.ToJsonString(JsonOptions);

        var existed = File.Exists(configPath);
        if (existed && string.Equals(File.ReadAllText(configPath).Trim(), content.Trim(), StringComparison.Ordinal))
        {
            return
            [
                new BootstrapFileResult
                {
                    Path = configPath,
                    Status = "unchanged",
                    Message = "Existing Slopwatch config already matches the dotnet-intelligence pack defaults."
                }
            ];
        }

        File.WriteAllText(configPath, content);
        return
        [
            new BootstrapFileResult
            {
                Path = configPath,
                Status = existed ? "overwritten" : "created",
                Message = "Wrote a repo-local Slopwatch config for the dotnet-intelligence pack."
            }
        ];
    }
}

public sealed class BootstrapToolInstallSpec
{
    public string PackageId { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public List<string> Commands { get; init; } = new();
    public bool? RollForward { get; init; }
}

public sealed class BootstrapPackDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<BootstrapToolInstallSpec> ToolSpecs { get; init; } = new();
    public List<string> Notes { get; init; } = new();
}
