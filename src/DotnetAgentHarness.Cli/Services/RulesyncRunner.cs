namespace DotnetAgentHarness.Cli.Services;

using DotnetAgentHarness.Cli.Utils;

public class RulesyncRunner(IProcessRunner processRunner) : IRulesyncRunner
{
    private readonly IProcessRunner processRunner = processRunner;

    public async Task<RulesyncResult> FetchAsync(string source, string path)
    {
        // Validate source format (owner/repo)
        if (!source.Contains('/'))
        {
            return new RulesyncResult(false, "Invalid source format. Expected: owner/repo");
        }

        string rulesyncDir = Path.Combine(path, ".rulesync");

        // Create directory if it doesn't exist
        if (!Directory.Exists(rulesyncDir))
        {
            Directory.CreateDirectory(rulesyncDir);
        }

        // Use rulesync fetch
        ProcessResult result = await this.processRunner.RunAsync(
            "rulesync",
            $"fetch {source}:{rulesyncDir}");

        if (result.ExitCode != 0)
        {
            return new RulesyncResult(false, $"Fetch failed: {result.Error}");
        }

        return new RulesyncResult(true);
    }

    public async Task<RulesyncResult> GenerateAsync(
        string targets,
        string path,
        bool deleteTrue = false,
        bool dryRun = false)
    {
        string rulesyncDir = Path.Combine(path, ".rulesync");

        if (!Directory.Exists(rulesyncDir))
        {
            return new RulesyncResult(false, ".rulesync directory does not exist");
        }

        List<string> args = new()
        {
            "generate",
        };

        if (!string.IsNullOrEmpty(targets))
        {
            args.Add($"--targets \"{targets}\"");
        }

        if (deleteTrue)
        {
            args.Add("--delete");
        }

        if (dryRun)
        {
            args.Add("--dry-run");
        }

        ProcessResult result = await this.processRunner.RunAsync(
            "rulesync",
            string.Join(" ", args),
            path);

        if (result.ExitCode != 0)
        {
            return new RulesyncResult(false, $"Generate failed: {result.Error}");
        }

        return new RulesyncResult(true);
    }

    public async Task<RulesyncResult> InstallAsync(string path)
    {
        string rulesyncDir = Path.Combine(path, ".rulesync");

        if (!Directory.Exists(rulesyncDir))
        {
            return new RulesyncResult(false, ".rulesync directory does not exist");
        }

        // Check for declarative sources
        string configPath = Path.Combine(rulesyncDir, "rulesync.jsonc");
        if (!File.Exists(configPath))
        {
            return new RulesyncResult(true); // No config, nothing to install
        }

        ProcessResult result = await this.processRunner.RunAsync(
            "rulesync",
            "install",
            rulesyncDir);

        if (result.ExitCode != 0)
        {
            return new RulesyncResult(false, $"Install failed: {result.Error}");
        }

        return new RulesyncResult(true);
    }
}

public interface IRulesyncRunner
{
    Task<RulesyncResult> FetchAsync(string source, string path);

    Task<RulesyncResult> GenerateAsync(string targets, string path, bool deleteTrue = false, bool dryRun = false);

    Task<RulesyncResult> InstallAsync(string path);
}
