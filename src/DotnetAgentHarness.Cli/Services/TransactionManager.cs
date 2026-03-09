namespace DotnetAgentHarness.Cli.Services;

public class TransactionManager : ITransactionManager
{
    public async Task<string> BackupAsync(string path)
    {
        string rulesyncPath = Path.Combine(path, ".rulesync");

        if (!Directory.Exists(rulesyncPath))
        {
            return string.Empty;
        }

        string backupPath = Path.Combine(
            Path.GetTempPath(),
            $"rulesync-backup-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}");

        await Task.Run(() =>
        {
            Directory.CreateDirectory(backupPath);
            CopyDirectory(rulesyncPath, Path.Combine(backupPath, ".rulesync"), true);
        });

        return backupPath;
    }

    public async Task RestoreAsync(string backupPath)
    {
        if (!Directory.Exists(backupPath))
        {
            throw new DirectoryNotFoundException($"Backup not found: {backupPath}");
        }

        string targetPath = Path.GetDirectoryName(backupPath)
            ?? throw new InvalidOperationException("Invalid backup path");

        string rulesyncPath = Path.Combine(targetPath, ".rulesync");
        string backupRulesync = Path.Combine(backupPath, ".rulesync");

        await Task.Run(() =>
        {
            // Remove existing .rulesync if present
            if (Directory.Exists(rulesyncPath))
            {
                Directory.Delete(rulesyncPath, true);
            }

            // Restore from backup
            if (Directory.Exists(backupRulesync))
            {
                CopyDirectory(backupRulesync, rulesyncPath, true);
            }
        });
    }

    public async Task CleanupAsync(string backupPath)
    {
        if (!Directory.Exists(backupPath))
        {
            return;
        }

        await Task.Run(() =>
        {
            Directory.Delete(backupPath, true);
        });
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        DirectoryInfo dir = new(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the directory and copy them to the new location
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}

public interface ITransactionManager
{
    Task<string> BackupAsync(string path);

    Task RestoreAsync(string backupPath);

    Task CleanupAsync(string backupPath);
}
