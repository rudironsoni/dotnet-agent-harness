using System.Diagnostics;

namespace DotnetAgentHarness.Cli.Utils;

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null);
}

public record ProcessResult(int ExitCode, string Output, string Error);

public class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null) outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null) errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            outputBuilder.ToString(),
            errorBuilder.ToString()
        );
    }
}
