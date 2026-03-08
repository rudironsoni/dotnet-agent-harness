using System.Runtime.InteropServices;

namespace DotnetAgentHarness.Cli.Utils;

public static class PlatformHelper
{
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    
    public static bool IsUnix() => !IsWindows();

    public static void MakeExecutable(string filePath)
    {
        if (IsUnix())
        {
            // Set executable permissions: owner, group, and others can execute
            File.SetUnixFileMode(filePath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        }
        // On Windows, no action needed - files are executable by default
    }

    public static string GetRulesyncCommand()
    {
        return IsWindows() ? "rulesync.cmd" : "rulesync";
    }
}
