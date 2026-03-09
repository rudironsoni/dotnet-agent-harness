namespace DotnetAgentHarness.Cli.Utils;

using System.Runtime.InteropServices;

public static class PlatformHelper
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static void MakeExecutable(string path)
    {
        if (IsWindows)
        {
            return; // Windows doesn't use executable bit
        }

        try
        {
            ProcessRunner runner = new();
            ProcessResult result = runner.RunAsync("chmod", $"+x \"{path}\"").GetAwaiter().GetResult();

            if (result.ExitCode != 0)
            {
                // Fallback: Try using bash
                runner.RunAsync("bash", $"-c 'chmod +x \"{path}\"'").GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Failed to make executable: {ex.Message}");
        }
    }

    public static string GetShellName()
    {
        string? shell = Environment.GetEnvironmentVariable("SHELL");

        if (string.IsNullOrEmpty(shell))
        {
            return IsWindows ? "powershell" : "bash";
        }

        return Path.GetFileName(shell);
    }

    public static string GetShellProfilePath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string shell = GetShellName();

        return shell switch
        {
            "zsh" => Path.Combine(home, ".zshrc"),
            "bash" => Path.Combine(home, ".bashrc"),
            "fish" => Path.Combine(home, ".config", "fish", "config.fish"),
            "powershell" => Path.Combine(home, "Documents", "PowerShell", "Microsoft.PowerShell_profile.ps1"),
            _ => Path.Combine(home, ".bashrc"),
        };
    }
}
