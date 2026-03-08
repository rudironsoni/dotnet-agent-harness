using System.CommandLine;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using DotnetAgentHarness.Cli.Utils;

namespace DotnetAgentHarness.Cli.Commands;

public class InstallCommand : Command
{
    private readonly IPrerequisiteChecker _prerequisiteChecker;
    private readonly IRulesyncRunner _rulesyncRunner;
    private readonly IConfigDetector _configDetector;
    private readonly ITransactionManager _transactionManager;
    private readonly IHookDownloader _hookDownloader;

    private static readonly string[] HookScripts = new[]
    {
        "dotnet-agent-harness-session-start.sh",
        "dotnet-agent-harness-post-edit-roslyn.sh",
        "dotnet-agent-harness-slopwatch.sh",
        "dotnet-agent-harness-error-recovery.sh",
        "dotnet-agent-harness-inline-error-recovery.sh"
    };

    public InstallCommand(
        IPrerequisiteChecker prerequisiteChecker,
        IRulesyncRunner rulesyncRunner,
        IConfigDetector configDetector,
        ITransactionManager transactionManager,
        IHookDownloader hookDownloader)
        : base("install", "Install the dotnet-agent-harness toolkit")
    {
        _prerequisiteChecker = prerequisiteChecker;
        _rulesyncRunner = rulesyncRunner;
        _configDetector = configDetector;
        _transactionManager = transactionManager;
        _hookDownloader = hookDownloader;

        var sourceOption = new Option<string>(
            new[] { "--source", "-s" },
            () => "rudironsoni/dotnet-agent-harness",
            "Source GitHub repository");

        var targetsOption = new Option<string>(
            new[] { "--targets", "-t" },
            () => "claudecode,copilot,opencode,geminicli,factorydroid,codexcli,antigravity",
            "Comma-separated list of target platforms");

        var pathOption = new Option<string>(
            new[] { "--path", "-p" },
            () => ".",
            "Installation directory");

        var forceOption = new Option<bool>(
            new[] { "--force", "-f" },
            () => false,
            "Skip confirmation prompts");

        var dryRunOption = new Option<bool>(
            new[] { "--dry-run", "-d" },
            () => false,
            "Show what would be done without making changes");

        var verboseOption = new Option<bool>(
            new[] { "--verbose", "-v" },
            () => false,
            "Show detailed output");

        AddOption(sourceOption);
        AddOption(targetsOption);
        AddOption(pathOption);
        AddOption(forceOption);
        AddOption(dryRunOption);
        AddOption(verboseOption);

        this.SetHandler(async (string source, string targets, string path, bool force, bool dryRun, bool verbose) =>
        {
            await ExecuteAsync(source, targets, path, force, dryRun, verbose);
        }, sourceOption, targetsOption, pathOption, forceOption, dryRunOption, verboseOption);
    }

    private async Task ExecuteAsync(string source, string targets, string path, bool force, bool dryRun, bool verbose)
    {
        var fullPath = Path.GetFullPath(path);
        
        Console.WriteLine($"Installing dotnet-agent-harness toolkit...");
        Console.WriteLine($"  Source: {source}");
        Console.WriteLine($"  Targets: {targets}");
        Console.WriteLine($"  Path: {fullPath}");
        if (dryRun) Console.WriteLine($"  [DRY RUN - No changes will be made]");
        Console.WriteLine();

        try
        {
            // Step 1: Check prerequisites
            Console.WriteLine("==> Checking prerequisites...");
            var prereqResult = await _prerequisiteChecker.CheckAsync();
            if (!prereqResult.Success)
            {
                Console.Error.WriteLine($"Error: {prereqResult.ErrorMessage}");
                Environment.Exit(1);
            }
            Console.WriteLine($"  ✓ rulesync {prereqResult.RulesyncVersion} installed");

            // Step 2: Check for existing installation
            var rulesyncPath = Path.Combine(fullPath, ".rulesync");
            var backupPath = string.Empty;
            
            if (Directory.Exists(rulesyncPath) && !force)
            {
                if (!dryRun)
                {
                    Console.Write("  .rulesync directory already exists. Overwrite? [y/N] ");
                    var response = Console.ReadLine();
                    if (!response?.Equals("y", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        Console.WriteLine("Installation cancelled.");
                        return;
                    }
                }
            }

            // Step 3: Backup (unless dry-run)
            if (!dryRun)
            {
                Console.WriteLine("==> Creating backup...");
                backupPath = await _transactionManager.BackupAsync(fullPath);
                if (!string.IsNullOrEmpty(backupPath))
                {
                    Console.WriteLine($"  ✓ Backup created: {backupPath}");
                }
            }

            // Step 4: Fetch .rulesync
            Console.WriteLine("==> Fetching .rulesync...");
            if (!dryRun)
            {
                var fetchResult = await _rulesyncRunner.FetchAsync(source, fullPath);
                if (!fetchResult.Success)
                {
                    Console.Error.WriteLine($"  ✗ Fetch failed: {fetchResult.Error}");
                    await RollbackAsync(backupPath, fullPath);
                    Environment.Exit(1);
                }
            }
            Console.WriteLine($"  ✓ Fetched from {source}");

            // Step 5: Check for declarative sources
            Console.WriteLine("==> Checking for declarative sources...");
            var hasDeleteTrue = await _configDetector.HasDeleteTrueAsync(fullPath);
            if (hasDeleteTrue)
            {
                Console.WriteLine("  ✓ rulesync.jsonc has delete: true");
            }

            // Step 6: Run rulesync install (for declarative sources)
            Console.WriteLine("==> Installing declarative sources...");
            if (!dryRun)
            {
                var installResult = await _rulesyncRunner.InstallAsync(fullPath);
                if (!installResult.Success && verbose)
                {
                    Console.WriteLine($"  Note: {installResult.Error}");
                }
            }
            Console.WriteLine($"  ✓ Install complete");

            // Step 7: Run rulesync generate
            Console.WriteLine("==> Generating configuration...");
            if (!dryRun)
            {
                var generateResult = await _rulesyncRunner.GenerateAsync(targets, fullPath, hasDeleteTrue, dryRun);
                if (!generateResult.Success)
                {
                    Console.Error.WriteLine($"  ✗ Generate failed: {generateResult.Error}");
                    await RollbackAsync(backupPath, fullPath);
                    Environment.Exit(1);
                }
            }
            Console.WriteLine($"  ✓ Generated for: {targets}");

            // Step 8: Download hooks
            Console.WriteLine("==> Downloading hook scripts...");
            if (!dryRun)
            {
                var hooksResult = await _hookDownloader.DownloadHooksAsync(HookScripts, source, fullPath);
                if (!hooksResult.Success)
                {
                    Console.Error.WriteLine($"  ✗ Hook download failed: {hooksResult.ErrorMessage}");
                    await RollbackAsync(backupPath, fullPath);
                    Environment.Exit(1);
                }

                // Make hooks executable
                foreach (var hook in hooksResult.DownloadedHooks)
                {
                    var hookPath = Path.Combine(fullPath, ".rulesync", "hooks", hook);
                    PlatformHelper.MakeExecutable(hookPath);
                }
            }
            Console.WriteLine($"  ✓ Downloaded {HookScripts.Length} hook scripts");

            // Step 9: Cleanup backup on success
            if (!dryRun && !string.IsNullOrEmpty(backupPath))
            {
                await _transactionManager.CleanupAsync(backupPath);
            }

            // Summary
            Console.WriteLine();
            Console.WriteLine($"Installation Complete!");
            Console.WriteLine();
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  Source: {source}");
            Console.WriteLine($"  Targets: {targets}");
            Console.WriteLine($"  Path: {fullPath}/.rulesync");
            Console.WriteLine();
            Console.WriteLine($"Next Steps:");
            Console.WriteLine($"  1. Review the generated configuration");
            Console.WriteLine($"  2. Restart your AI coding tool session");
            Console.WriteLine($"  3. Run 'rulesync generate --check' to verify");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nError: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine(ex.StackTrace);
            }
            Environment.Exit(1);
        }
    }

    private async Task RollbackAsync(string backupPath, string fullPath)
    {
        if (string.IsNullOrEmpty(backupPath))
        {
            return;
        }

        Console.WriteLine("==> Rolling back changes...");
        try
        {
            await _transactionManager.RestoreAsync(backupPath);
            Console.WriteLine("  ✓ Rollback complete");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"  ✗ Rollback failed: {ex.Message}");
            Console.Error.WriteLine($"  Manual restore may be needed from: {backupPath}");
        }
    }
}
