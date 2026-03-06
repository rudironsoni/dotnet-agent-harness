using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DotNetAgentHarness.Tools.Engine;

public static class BootstrapEngine
{
    private static readonly string[] CanonicalRuleSyncFeatures =
    [
        "rules",
        "ignore",
        "mcp",
        "commands",
        "subagents",
        "skills",
        "hooks"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static BootstrapReport Bootstrap(string repoRoot, BootstrapOptions options)
    {
        var normalizedRoot = Path.GetFullPath(repoRoot);
        var profile = BootstrapProfileCatalog.Resolve(options.Profile);
        var targets = NormalizeTargets(options.Targets);
        var features = NormalizeFeatures(options.Features, profile);
        var packs = BootstrapPackCatalog.ResolveMany(options.EnablePacks);
        var toolVersion = ToolkitRuntimeMetadata.ResolveToolVersion(options.ToolVersion);
        var warnings = new List<string>();

        if (packs.Any(pack => pack.Id.Equals(BootstrapPackCatalog.DotNetIntelligence, StringComparison.OrdinalIgnoreCase))
            && !features.Contains("*", StringComparer.OrdinalIgnoreCase)
            && !features.Contains("hooks", StringComparer.OrdinalIgnoreCase))
        {
            warnings.Add("dotnet-intelligence is most effective when RuleSync generates hooks. Add the 'hooks' feature or use --profile platform-native.");
        }

        var toolManifest = options.WriteToolManifest
            ? WriteToolManifest(normalizedRoot, toolVersion, packs.SelectMany(pack => pack.ToolSpecs))
            : new BootstrapFileResult
            {
                Path = Path.Combine(normalizedRoot, ".config", "dotnet-tools.json"),
                Status = "skipped",
                Message = "Skipped writing the local tool manifest because persistence was disabled."
            };
        var rulesyncConfig = WriteRuleSyncConfig(normalizedRoot, options, targets, features);
        var packResults = packs
            .Select(pack => new BootstrapPackResult
            {
                Id = pack.Id,
                Description = pack.Description,
                ToolPackageIds = pack.ToolSpecs.Select(tool => tool.PackageId).ToList(),
                Files = BootstrapPackCatalog.Apply(normalizedRoot, pack, options.WritePackFiles),
                Notes = pack.Notes.ToList()
            })
            .ToList();

        var ruleSyncAvailable = IsCommandAvailable("rulesync", normalizedRoot);
        var commands = new List<BootstrapCommandResult>();
        var generationStatus = options.RunRuleSync ? "requested" : "not-requested";
        var passed = true;

        if (options.RunRuleSync)
        {
            if (!ruleSyncAvailable)
            {
                passed = false;
                generationStatus = "rulesync-unavailable";
                warnings.Add("rulesync is not installed or not on PATH. Install RuleSync before using --run-rulesync.");
            }
            else
            {
                var installCommand = "rulesync install";
                var installResult = ProcessRunner.RunShell(installCommand, normalizedRoot, timeoutMs: 180_000);
                commands.Add(ToCommandResult(installResult));
                if (installResult.ExitCode != 0 || installResult.TimedOut)
                {
                    passed = false;
                    generationStatus = installResult.TimedOut ? "install-timed-out" : "install-failed";
                }
                else
                {
                    var generateCommand = "rulesync generate";
                    var generateResult = ProcessRunner.RunShell(generateCommand, normalizedRoot, timeoutMs: 180_000);
                    commands.Add(ToCommandResult(generateResult));
                    if (generateResult.ExitCode != 0 || generateResult.TimedOut)
                    {
                        passed = false;
                        generationStatus = generateResult.TimedOut ? "generate-timed-out" : "generate-failed";
                    }
                    else
                    {
                        generationStatus = "generated";
                    }
                }
            }
        }

        var environment = ProjectAnalyzer.ProbeDotNetEnvironment();
        var repositoryProfile = ProjectAnalyzer.Analyze(normalizedRoot, environment);
        var doctor = DoctorEngine.BuildReport(repositoryProfile, environment);
        var recommendations = LoadRecommendations(normalizedRoot, repositoryProfile, options.SkillLimit, warnings);
        var init = new InitReport
        {
            Profile = repositoryProfile,
            Recommendations = recommendations,
            Doctor = doctor
        };

        var stateFiles = options.WriteState
            ? WriteBootstrapState(normalizedRoot, repositoryProfile, recommendations, doctor)
            : [];

        var nextSteps = BuildNextSteps(options, profile, targets, features, packResults, ruleSyncAvailable, generationStatus);

        var report = new BootstrapReport
        {
            RepoRoot = normalizedRoot,
            ToolPackageId = ToolkitRuntimeMetadata.PackageId,
            ToolCommandName = ToolkitRuntimeMetadata.ToolCommandName,
            ToolVersion = toolVersion,
            Profile = profile.Id,
            Targets = targets.Select(BuildTargetProfile).ToList(),
            Features = features,
            Packs = packResults,
            ToolManifest = toolManifest,
            RuleSyncConfig = rulesyncConfig,
            StateFiles = stateFiles,
            RuleSyncAvailable = ruleSyncAvailable,
            RuleSyncGenerationStatus = generationStatus,
            Commands = commands,
            Warnings = warnings,
            NextSteps = nextSteps,
            Init = init,
            Passed = passed
        };

        if (options.WriteState)
        {
            var reportPath = RepoStateStore.WriteJson(normalizedRoot, Path.Combine(".dotnet-agent-harness", "bootstrap-report.json"), report);
            stateFiles.Add(new BootstrapFileResult
            {
                Path = reportPath,
                Status = "written",
                Message = "Bootstrap summary persisted for repo-local reuse."
            });
        }

        return report;
    }

    private static List<string> NormalizeTargets(IReadOnlyList<string> requestedTargets)
    {
        var rawTargets = requestedTargets.Count == 0
            ? KnownPlatforms.All.ToList()
            : requestedTargets;

        return rawTargets
            .Select(PromptBundleRenderer.NormalizePlatform)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> NormalizeFeatures(IReadOnlyList<string> requestedFeatures, BootstrapProfileDefinition profile)
    {
        var features = requestedFeatures.Count == 0 ? profile.Features : requestedFeatures;
        return features
            .Select(feature => feature.Trim())
            .Where(feature => !string.IsNullOrWhiteSpace(feature))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static BootstrapFileResult WriteToolManifest(string repoRoot, string toolVersion, IEnumerable<BootstrapToolInstallSpec> additionalTools)
    {
        var manifestPath = Path.Combine(repoRoot, ".config", "dotnet-tools.json");
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);

        JsonObject root;
        string status;
        string message;

        if (File.Exists(manifestPath))
        {
            root = JsonNode.Parse(File.ReadAllText(manifestPath)) as JsonObject
                   ?? throw new InvalidDataException($"Tool manifest '{manifestPath}' is not a valid JSON object.");
            status = "updated";
            message = "Merged dotnet-agent-harness into the local tool manifest.";
        }
        else
        {
            root = new JsonObject();
            status = "created";
            message = "Created a local tool manifest for dotnet-agent-harness.";
        }

        root["version"] = 1;
        root["isRoot"] = true;
        var tools = root["tools"] as JsonObject ?? new JsonObject();
        MergeToolManifestEntry(tools, new BootstrapToolInstallSpec
        {
            PackageId = ToolkitRuntimeMetadata.PackageId,
            Version = toolVersion,
            Commands = [ToolkitRuntimeMetadata.ToolCommandName]
        });

        foreach (var tool in additionalTools)
        {
            MergeToolManifestEntry(tools, tool);
        }

        root["tools"] = tools;

        File.WriteAllText(manifestPath, root.ToJsonString(JsonOptions));
        return new BootstrapFileResult
        {
            Path = manifestPath,
            Status = status,
            Message = message
        };
    }

    private static void MergeToolManifestEntry(JsonObject tools, BootstrapToolInstallSpec tool)
    {
        var entry = new JsonObject
        {
            ["version"] = tool.Version,
            ["commands"] = new JsonArray(tool.Commands.Select(command => (JsonNode?)command).ToArray())
        };

        if (tool.RollForward.HasValue)
        {
            entry["rollForward"] = tool.RollForward.Value;
        }

        tools[tool.PackageId.ToLowerInvariant()] = entry;
    }

    private static BootstrapFileResult WriteRuleSyncConfig(string repoRoot, BootstrapOptions options, IReadOnlyList<string> targets, IReadOnlyList<string> features)
    {
        var configPath = Path.Combine(repoRoot, options.ConfigPath);
        var configDirectory = Path.GetDirectoryName(configPath);
        var existed = File.Exists(configPath);
        if (!string.IsNullOrWhiteSpace(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        var config = new JsonObject
        {
            ["$schema"] = "https://raw.githubusercontent.com/dyoshikawa/rulesync/refs/heads/main/config-schema.json",
            ["targets"] = new JsonArray(targets.Select(target => (JsonNode?)target).ToArray()),
            ["features"] = BuildRuleSyncFeaturesNode(targets, features),
            ["sources"] = new JsonArray(
                new JsonObject
                {
                    ["source"] = options.SourceRepository,
                    ["path"] = options.SourcePath
                })
        };

        var content = config.ToJsonString(JsonOptions);
        if (File.Exists(configPath) && !options.Force)
        {
            var existing = File.ReadAllText(configPath);
            return new BootstrapFileResult
            {
                Path = configPath,
                Status = string.Equals(existing.Trim(), content.Trim(), StringComparison.Ordinal) ? "unchanged" : "skipped",
                Message = string.Equals(existing.Trim(), content.Trim(), StringComparison.Ordinal)
                    ? "Existing RuleSync config already matches the bootstrap defaults."
                    : "Existing RuleSync config was preserved. Re-run with --force to overwrite it."
            };
        }

        File.WriteAllText(configPath, content);
        return new BootstrapFileResult
        {
            Path = configPath,
            Status = existed ? "overwritten" : "created",
            Message = "Wrote a RuleSync config for the selected agent targets."
        };
    }

    private static JsonNode BuildRuleSyncFeaturesNode(IReadOnlyList<string> targets, IReadOnlyList<string> requestedFeatures)
    {
        var normalizedRequested = requestedFeatures
            .Select(feature => feature.Trim())
            .Where(feature => !string.IsNullOrWhiteSpace(feature))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var requestedAll = normalizedRequested.Contains("*", StringComparer.OrdinalIgnoreCase);
        var requestedSet = requestedAll
            ? new HashSet<string>(CanonicalRuleSyncFeatures, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(normalizedRequested, StringComparer.OrdinalIgnoreCase);

        var featureMap = new JsonObject();
        foreach (var target in targets)
        {
            var capability = PlatformCapabilityCatalog.Resolve(target);
            var allowed = capability.Surfaces
                .Concat(capability.SupportsIgnore ? ["ignore"] : [])
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var features = CanonicalRuleSyncFeatures
                .Where(feature => requestedSet.Contains(feature) && allowed.Contains(feature))
                .Select(feature => (JsonNode?)feature)
                .ToArray();

            featureMap[target] = new JsonArray(features);
        }

        return featureMap;
    }

    private static RecommendationBundle LoadRecommendations(string repoRoot, RepositoryProfile profile, int skillLimit, ICollection<string> warnings)
    {
        var rulesyncRoot = Path.Combine(repoRoot, ".rulesync");
        if (!Directory.Exists(rulesyncRoot))
        {
            warnings.Add("Recommendation catalog unavailable until RuleSync content exists under .rulesync/. Run rulesync install/generate to enable skill and command recommendations.");
            return new RecommendationBundle
            {
                Profile = profile
            };
        }

        var catalog = ToolkitCatalogLoader.Load(repoRoot);
        return RecommendationEngine.Recommend(profile, catalog, Math.Max(1, skillLimit));
    }

    private static List<BootstrapFileResult> WriteBootstrapState(string repoRoot, RepositoryProfile profile, RecommendationBundle recommendations, DoctorReport doctor)
    {
        return
        [
            new BootstrapFileResult
            {
                Path = RepoStateStore.WriteJson(repoRoot, Path.Combine(".dotnet-agent-harness", "project-profile.json"), profile),
                Status = "written",
                Message = "Repository profile persisted for prompt assembly and validation."
            },
            new BootstrapFileResult
            {
                Path = RepoStateStore.WriteJson(repoRoot, Path.Combine(".dotnet-agent-harness", "recommendations.json"), recommendations),
                Status = "written",
                Message = "Current recommendation snapshot persisted."
            },
            new BootstrapFileResult
            {
                Path = RepoStateStore.WriteJson(repoRoot, Path.Combine(".dotnet-agent-harness", "doctor-report.json"), doctor),
                Status = "written",
                Message = "Environment and repository diagnostics persisted."
            }
        ];
    }

    private static BootstrapTargetProfile BuildTargetProfile(string target)
    {
        var capability = PlatformCapabilityCatalog.Resolve(target);

        return new BootstrapTargetProfile
        {
            Id = capability.Id,
            OutputPaths = capability.OutputPaths.ToList(),
            Surfaces = capability.Surfaces.ToList()
        };
    }

    private static BootstrapCommandResult ToCommandResult(ProcessExecutionResult result)
    {
        return new BootstrapCommandResult
        {
            Command = result.Command,
            Passed = result.ExitCode == 0 && !result.TimedOut,
            ExitCode = result.ExitCode,
            TimedOut = result.TimedOut,
            StandardOutput = result.StandardOutput,
            StandardError = result.StandardError
        };
    }

    private static bool IsCommandAvailable(string command, string workingDirectory)
    {
        var checker = OperatingSystem.IsWindows()
            ? ProcessRunner.Run("where", command, workingDirectory, timeoutMs: 15_000, throwOnError: false)
            : ProcessRunner.RunShell($"command -v {command}", workingDirectory, timeoutMs: 15_000);

        return checker.ExitCode == 0 && !checker.TimedOut;
    }

    private static List<string> BuildNextSteps(BootstrapOptions options, BootstrapProfileDefinition profile, IReadOnlyList<string> targets, IReadOnlyList<string> features, IReadOnlyList<BootstrapPackResult> packs, bool ruleSyncAvailable, string generationStatus)
    {
        var nextSteps = new List<string>
        {
            $"Install or restore the local tool with 'dotnet tool restore' or 'dotnet tool install {ToolkitRuntimeMetadata.PackageId}'."
        };

        if (!options.RunRuleSync)
        {
            nextSteps.Add("Run 'rulesync install && rulesync generate' to materialize platform files from the generated RuleSync config.");
        }
        else if (!string.Equals(generationStatus, "generated", StringComparison.OrdinalIgnoreCase))
        {
            nextSteps.Add($"Fix RuleSync setup, then rerun '{ToolkitRuntimeMetadata.ToolCommandName} bootstrap --run-rulesync --targets {string.Join(',', targets)}'.");
        }

        if (!ruleSyncAvailable)
        {
            nextSteps.Add("Install RuleSync from https://github.com/dyoshikawa/rulesync before generating agent files.");
        }

        if (packs.Any(pack => pack.Id.Equals(BootstrapPackCatalog.DotNetIntelligence, StringComparison.OrdinalIgnoreCase)))
        {
            nextSteps.Add("Initialize a Slopwatch baseline intentionally with 'dotnet tool restore && dotnet tool run slopwatch init' before expecting repo validation to enforce the quality gate.");
        }

        nextSteps.Add($"Bootstrap profile '{profile.Id}' resolved to features: {string.Join(", ", features)}.");
        nextSteps.Add($"Use '{ToolkitRuntimeMetadata.ToolCommandName} prepare-message \"<request>\" --platform {targets[0]}' before non-trivial .NET work.");
        nextSteps.Add($"Use '{ToolkitRuntimeMetadata.ToolCommandName} validate --mode repo --run' after edits to enforce repo-native verification.");
        return nextSteps;
    }
}
