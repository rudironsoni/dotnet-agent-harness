namespace DotnetAgentHarness.Cli.Utils;

using System.Diagnostics;
using System.Text;

public class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(
        string command,
        string arguments,
        string? workingDirectory = null)
    {
        ProcessStartInfo startInfo = new(command, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        using Process process = new() { StartInfo = startInfo };

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            output.ToString().TrimEnd(),
            error.ToString().TrimEnd());
    }
}

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string command, string arguments, string? workingDirectory = null);
}
