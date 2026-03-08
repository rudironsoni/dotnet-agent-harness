using System.Runtime.InteropServices;
using DotnetAgentHarness.Cli.Utils;

namespace DotnetAgentHarness.Cli.Services;

public interface IPrerequisiteChecker
{
    Task<PrerequisiteResult> CheckAsync();
}

public record PrerequisiteResult(bool Success, string? ErrorMessage, Version? RulesyncVersion);

public class PrerequisiteChecker : IPrerequisiteChecker
{
    private readonly IProcessRunner _processRunner;
    private readonly Version _minimumVersion = new(7, 15, 0);

    public PrerequisiteChecker(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<PrerequisiteResult> CheckAsync()
    {
        var rulesyncCommand = GetRulesyncCommand();
        
        // Check if rulesync is installed
        var checkResult = await _processRunner.RunAsync(rulesyncCommand, "--version");
        
        if (checkResult.ExitCode != 0)
        {
            return new PrerequisiteResult(
                false, 
                "rulesync is not installed. Install it with: npm install -g @codewyre/rulesync", 
                null
            );
        }

        // Parse version
        var version = ParseVersion(checkResult.Output);
        if (version == null)
        {
            return new PrerequisiteResult(
                false,
                "Could not parse rulesync version",
                null
            );
        }

        // Check minimum version
        if (version < _minimumVersion)
        {
            return new PrerequisiteResult(
                false,
                $"rulesync version {version} is too old. Minimum required: {_minimumVersion}",
                version
            );
        }

        return new PrerequisiteResult(true, null, version);
    }

    private static string GetRulesyncCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On Windows, try rulesync.cmd first, then rulesync
            return "rulesync.cmd";
        }
        return "rulesync";
    }

    private static Version? ParseVersion(string output)
    {
        // Parse output like "rulesync 7.15.0" or "7.15.0"
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            // Try to find version pattern
            var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"(\d+)\.(\d+)(?:\.(\d+))?");
            if (match.Success)
            {
                var versionString = match.Value;
                if (Version.TryParse(versionString, out var version))
                {
                    return version;
                }
            }
        }
        return null;
    }
}
