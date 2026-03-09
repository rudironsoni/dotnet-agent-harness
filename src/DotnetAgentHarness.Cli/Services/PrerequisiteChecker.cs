namespace DotnetAgentHarness.Cli.Services;

using System.Diagnostics;
using System.Text.RegularExpressions;
using DotnetAgentHarness.Cli.Utils;

public partial class PrerequisiteChecker(IProcessRunner processRunner) : IPrerequisiteChecker
{
    private readonly IProcessRunner processRunner = processRunner;

    public async Task<PrerequisiteResult> CheckAsync()
    {
        // Check if rulesync is installed
        ProcessResult result = await this.processRunner.RunAsync("which", "rulesync");

        if (result.ExitCode != 0)
        {
            // Try checking with 'command -v rulesync' (POSIX)
            result = await this.processRunner.RunAsync("command", "-v rulesync");

            if (result.ExitCode != 0)
            {
                // Try with 'where' on Windows
                result = await this.processRunner.RunAsync("where", "rulesync");

                if (result.ExitCode != 0)
                {
                    return new PrerequisiteResult(
                        false,
                        string.Empty,
                        "rulesync is not installed. Please install it first: npm install -g @rulesync/cli");
                }
            }
        }

        // Get rulesync version
        ProcessResult versionResult = await this.processRunner.RunAsync("rulesync", "--version");
        string version = string.Empty;

        if (versionResult.ExitCode == 0)
        {
            Match match = MyRegex().Match(versionResult.Output);
            if (match.Success)
            {
                version = match.Groups[1].Value;
            }
            else
            {
                version = versionResult.Output.Trim();
            }
        }

        return new PrerequisiteResult(true, version);
    }

    [GeneratedRegex(@"(\d+\.\d+\.\d+)")]
    private static partial Regex MyRegex();
}

public interface IPrerequisiteChecker
{
    Task<PrerequisiteResult> CheckAsync();
}
