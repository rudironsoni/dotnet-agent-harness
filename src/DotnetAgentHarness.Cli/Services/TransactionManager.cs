namespace DotnetAgentHarness.Cli.Services;

public interface ITransactionManager
{
    Task<string> BackupAsync(string path);
    Task RestoreAsync(string backupPath);
    Task CleanupAsync(string backupPath);
}

public class TransactionManager : ITransactionManager
{
    public async Task<string> BackupAsync(string path)
    {
        var rulesyncPath = Path.Combine(path, ".rulesync");
        
        if (!Directory.Exists(rulesyncPath))
        {
            return string.Empty; // Nothing to backup
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = $"{rulesyncPath}.backup.{timestamp}";

        // Copy directory recursively
        await Task.Run(() => CopyDirectory(rulesyncPath, backupPath));
        
        return backupPath;
    }

    public async Task RestoreAsync(string backupPath)
    {
        if (string.IsNullOrEmpty(backupPath) || !Directory.Exists(backupPath))
        {
            return;
        }

        // Backup path format: {originalPath}.backup.{timestamp}
        // e.g., /tmp/xyz/.rulesync.backup.20250308_123456
        var backupFileName = Path.GetFileName(backupPath);
        var parentDir = Path.GetDirectoryName(backupPath)!;
        
        // Extract original directory name by removing .backup.{timestamp}
        var originalDirName = System.Text.RegularExpressions.Regex.Replace(
            backupFileName, 
            @"\.backup\.\d{8}_\d{6}$", 
            "");
        
        var rulesyncPath = Path.Combine(parentDir, originalDirName);
        
        // Remove current .rulesync if exists
        if (Directory.Exists(rulesyncPath))
        {
            await Task.Run(() => Directory.Delete(rulesyncPath, true));
        }

        // Restore from backup
        await Task.Run(() => CopyDirectory(backupPath, rulesyncPath));
    }

    public async Task CleanupAsync(string backupPath)
    {
        if (string.IsNullOrEmpty(backupPath) || !Directory.Exists(backupPath))
        {
            return;
        }

        await Task.Run(() => Directory.Delete(backupPath, true));
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
            CopyDirectory(subDir, destSubDir);
        }
    }
}
