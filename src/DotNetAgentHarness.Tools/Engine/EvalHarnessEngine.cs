using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DotNetAgentHarness.Tools.Engine;

public static class EvalHarnessEngine
{
    private static readonly object ConsoleSync = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static EvalHarnessRunResult Run(string repoRoot, EvalHarnessOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var artifactPath = ResolveArtifactPath(repoRoot, options);
        EnsureParentDirectory(artifactPath);
        if (File.Exists(artifactPath))
        {
            File.Delete(artifactPath);
        }

        var args = BuildArguments(artifactPath, options);
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var stdout = new StringWriter(new StringBuilder());
        using var stderr = new StringWriter(new StringBuilder());

        int exitCode;
        lock (ConsoleSync)
        {
            try
            {
                Console.SetOut(stdout);
                Console.SetError(stderr);
                exitCode = DotNetAgentHarness.Evals.Program.Main(args).GetAwaiter().GetResult();
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        var artifact = TryLoadArtifact(artifactPath);

        return new EvalHarnessRunResult
        {
            Command = $"DotNetAgentHarness.Evals.Program.Main({string.Join(' ', args)})",
            ExitCode = exitCode,
            TimedOut = false,
            StandardOutput = stdout.ToString(),
            StandardError = stderr.ToString(),
            Platform = options.Platform ?? string.Empty,
            TrialCount = options.TrialCount,
            UseDummyMode = options.UseDummyMode,
            UnloadedCheckOnly = options.UnloadedCheckOnly,
            ArtifactPath = artifactPath,
            Artifact = artifact
        };
    }

    private static string ResolveArtifactPath(string repoRoot, EvalHarnessOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ArtifactPath))
        {
            return Path.GetFullPath(Path.IsPathRooted(options.ArtifactPath)
                ? options.ArtifactPath
                : Path.Combine(repoRoot, options.ArtifactPath));
        }

        var artifactId = string.IsNullOrWhiteSpace(options.ArtifactId)
            ? $"eval-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
            : options.ArtifactId.Trim();
        return Path.Combine(repoRoot, ".dotnet-agent-harness", "evidence", "evals", $"{artifactId}.json");
    }

    private static string[] BuildArguments(string artifactPath, EvalHarnessOptions options)
    {
        var parts = new List<string>
        {
            "--dummy-mode",
            options.UseDummyMode ? "true" : "false",
            "--artifact-path",
            artifactPath,
            "--trials",
            options.TrialCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(options.Platform))
        {
            parts.Add("--platform");
            parts.Add(options.Platform);
        }

        if (options.UnloadedCheckOnly)
        {
            parts.Add("--unloaded-check");
        }

        if (!string.IsNullOrWhiteSpace(options.CaseFilePath))
        {
            parts.Add("--cases");
            parts.Add(options.CaseFilePath!);
        }

        if (!string.IsNullOrWhiteSpace(options.Provider))
        {
            parts.Add("--provider");
            parts.Add(options.Provider!);
        }

        if (!string.IsNullOrWhiteSpace(options.Model))
        {
            parts.Add("--model");
            parts.Add(options.Model!);
        }

        if (!string.IsNullOrWhiteSpace(options.ArtifactId))
        {
            parts.Add("--run-id");
            parts.Add(options.ArtifactId!);
        }

        return parts.ToArray();
    }

    private static EvalHarnessArtifactSummary? TryLoadArtifact(string artifactPath)
    {
        if (!File.Exists(artifactPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<EvalHarnessArtifactSummary>(File.ReadAllText(artifactPath), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void EnsureParentDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
