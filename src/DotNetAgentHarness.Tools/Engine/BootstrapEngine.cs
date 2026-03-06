using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetAgentHarness.Tools.Engine;

/// <summary>
/// Bootstrap engine using bundle-based installation (v2.0).
/// </summary>
public sealed class BootstrapEngine
{
    private readonly Assembly _assembly = typeof(BootstrapEngine).Assembly;

    /// <summary>
    /// Runs the bootstrap process with the specified options.
    /// </summary>
    public async Task<BootstrapReport> RunAsync(
        string repoRoot,
        BootstrapOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
            return BootstrapReport.Failure("Repository root path is required.");

        var normalizedRoot = Path.GetFullPath(repoRoot);

        // Check for v1.x installation
        var hasV1 = DetectV1Installation(normalizedRoot);

        if (options.ListTargets)
            return await ListTargetsAsync(cancellationToken);

        var targets = options.Targets.Count > 0
            ? options.Targets
            : GetEmbeddedTargets();

        if (targets.Count == 0)
            return BootstrapReport.Failure("No targets available for installation.", hasV1);

        var results = new List<BootstrapTargetResult>();
        foreach (var target in targets)
        {
            var result = await ExtractBundleAsync(target, normalizedRoot, options.Force, cancellationToken);
            results.Add(result);

            if (!result.Success && !options.Force)
                break;
        }

        var version = ToolkitRuntimeMetadata.ResolveToolVersion(null);
        return BootstrapReport.Success(normalizedRoot, version, results, hasV1);
    }

    /// <summary>
    /// Synchronous wrapper for backward compatibility.
    /// </summary>
    public BootstrapReport Bootstrap(string repoRoot, BootstrapOptions options)
        => RunAsync(repoRoot, options).GetAwaiter().GetResult();

    private static bool DetectV1Installation(string repoRoot)
    {
        if (string.IsNullOrWhiteSpace(repoRoot))
            return false;

        var normalizedRoot = Path.GetFullPath(repoRoot);
        return Directory.Exists(Path.Combine(normalizedRoot, ".rulesync"))
            || File.Exists(Path.Combine(normalizedRoot, "rulesync.jsonc"));
    }

    private async Task<BootstrapReport> ListTargetsAsync(CancellationToken cancellationToken)
    {
        var targets = GetEmbeddedTargets();

        Console.WriteLine("Available targets:");
        foreach (var target in targets.OrderBy(t => t, StringComparer.OrdinalIgnoreCase))
        {
            var capability = PlatformCapabilityCatalog.Resolve(target);
            Console.WriteLine($"  {target}");
            Console.WriteLine($"    Output paths: {string.Join(", ", capability.OutputPaths)}");
            Console.WriteLine($"    Surfaces: {string.Join(", ", capability.Surfaces)}");
            Console.WriteLine();
        }

        return BootstrapReport.SuccessList(targets);
    }

    private List<string> GetEmbeddedTargets()
    {
        var prefix = "DotNetAgentHarness.Tools.Bundles.";
        var suffix = ".tar.gz";

        return _assembly.GetManifestResourceNames()
            .Where(r => r.StartsWith(prefix) && r.EndsWith(suffix))
            .Select(r => r[prefix.Length..^suffix.Length])
            .ToList();
    }

    private async Task<BootstrapTargetResult> ExtractBundleAsync(
        string target,
        string repoRoot,
        bool force,
        CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;
        string? tempDir = null;

        try
        {
            Console.WriteLine($"Extracting bundle for target: {target}");

            var capability = PlatformCapabilityCatalog.Resolve(target);
            var outputPath = capability.OutputPaths.Count > 0
                ? Path.Combine(repoRoot, capability.OutputPaths[0].TrimEnd('/', '\\'))
                : repoRoot;

            if (!force && Directory.Exists(outputPath) && Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories).Length > 0)
            {
                return new BootstrapTargetResult
                {
                    Target = target,
                    Success = false,
                    ErrorMessage = "Target files already exist. Use --force to overwrite.",
                    Duration = DateTimeOffset.UtcNow - startTime
                };
            }

            // Extract bundle
            var resourceName = $"DotNetAgentHarness.Tools.Bundles.{target}.tar.gz";
            using var stream = _assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return new BootstrapTargetResult
                {
                    Target = target,
                    Success = false,
                    ErrorMessage = $"Bundle '{target}' not found.",
                    Duration = DateTimeOffset.UtcNow - startTime
                };
            }

            tempDir = Path.Combine(Path.GetTempPath(), $"harness-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            // Extract tar.gz
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            using (var tar = new TarReader(gzip))
            {
                TarEntry? entry;
                while ((entry = tar.GetNextEntry()) != null)
                {
                    if (entry.EntryType == TarEntryType.RegularFile)
                    {
                        var destPath = Path.Combine(tempDir, entry.Name);
                        var destDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);
                        entry.ExtractToFile(destPath, overwrite: true);
                    }
                }
            }

            // Move files to repo
            var extractedFiles = new List<string>();
            foreach (var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(tempDir, file);
                if (relPath == "manifest.json")
                    continue;

                var destPath = Path.Combine(outputPath, relPath);
                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                File.Move(file, destPath, overwrite: true);
                extractedFiles.Add(relPath);
            }

            Console.WriteLine($"  ✓ Extracted {extractedFiles.Count} files to {outputPath}");

            return new BootstrapTargetResult
            {
                Target = target,
                Success = true,
                ExtractedFiles = extractedFiles,
                Duration = DateTimeOffset.UtcNow - startTime
            };
        }
        catch (OperationCanceledException)
        {
            return new BootstrapTargetResult
            {
                Target = target,
                Success = false,
                ErrorMessage = "Extraction was cancelled.",
                Duration = DateTimeOffset.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Failed: {ex.Message}");
            return new BootstrapTargetResult
            {
                Target = target,
                Success = false,
                ErrorMessage = ex.Message,
                Duration = DateTimeOffset.UtcNow - startTime
            };
        }
        finally
        {
            if (tempDir != null && Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, recursive: true); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to cleanup temporary directory '{tempDir}': {ex.Message}");
                }
            }
        }
    }
}
