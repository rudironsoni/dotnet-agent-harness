using DotnetAgentHarness.Cli.Utils;

namespace DotnetAgentHarness.Cli.Services;

public interface IRulesyncRunner
{
    Task<RulesyncResult> FetchAsync(string source, string path);
    Task<RulesyncResult> InstallAsync(string path);
    Task<RulesyncResult> GenerateAsync(string targets, string path, bool useConfigFile, bool dryRun);
}

public record RulesyncResult(bool Success, string Output, string Error);

public class RulesyncRunner : IRulesyncRunner
{
    private readonly IProcessRunner _processRunner;

    public RulesyncRunner(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<RulesyncResult> FetchAsync(string source, string path)
    {
        var fetchSpec = $"{source}:.rulesync";
        var result = await _processRunner.RunAsync("rulesync", $"fetch \"{fetchSpec}\"", path);
        
        return new RulesyncResult(
            result.ExitCode == 0,
            result.Output,
            result.Error
        );
    }

    public async Task<RulesyncResult> InstallAsync(string path)
    {
        // For declarative sources workflow: rulesync install
        var result = await _processRunner.RunAsync("rulesync", "install", path);
        
        return new RulesyncResult(
            result.ExitCode == 0,
            result.Output,
            result.Error
        );
    }

    public async Task<RulesyncResult> GenerateAsync(string targets, string path, bool useConfigFile, bool dryRun)
    {
        var args = new System.Text.StringBuilder("generate");
        
        if (dryRun)
        {
            args.Append(" --dry-run");
        }
        
        if (useConfigFile)
        {
            // Use config file settings (respect delete: true)
            // Don't pass --targets or --features
        }
        else
        {
            args.Append($" --targets {targets} --features \"*\"");
        }

        var result = await _processRunner.RunAsync("rulesync", args.ToString(), path);
        
        return new RulesyncResult(
            result.ExitCode == 0,
            result.Output,
            result.Error
        );
    }
}
