// =============================================================================
// File System Abstraction Basics: From Untestable to Testable Refactoring
// System.IO.Abstractions Basics - Refactoring for Testability
// =============================================================================

using System.IO.Abstractions;
using System.Text.Json;

namespace FileSystemTestingExamples;

#region Problem Example: Untestable Code

/// <summary>
/// ❌ Untestable configuration service (anti-pattern example)
/// Uses System.IO static classes directly, cannot be unit tested
/// </summary>
public class LegacyConfigurationService
{
    /// <summary>
    /// Loads configuration file - directly depends on file system
    /// </summary>
    public string LoadConfig(string configPath)
    {
        // ❌ Problem: cannot control file content in tests
        return File.ReadAllText(configPath);
    }

    /// <summary>
    /// Saves configuration file - produces side effects on disk
    /// </summary>
    public void SaveConfig(string configPath, string content)
    {
        // ❌ Problem: tests will actually write to disk, affecting other tests
        File.WriteAllText(configPath, content);
    }

    /// <summary>
    /// Checks if configuration file exists
    /// </summary>
    public bool ConfigExists(string configPath)
    {
        // ❌ Problem: depends on real file system state
        return File.Exists(configPath);
    }
}

/*
 * LegacyConfigurationService problems:
 *
 * 1. Speed: Disk IO is 10-100x slower than memory operations
 * 2. Environment dependent: Test results affected by file system state
 * 3. Side effects: Leaves files on disk, affecting other tests
 * 4. Concurrency: Multiple tests operating on same file cause race conditions
 * 5. Error simulation: Cannot easily simulate exceptions like permission denied
 */

#endregion

#region Solution: Use IFileSystem Abstraction

/// <summary>
/// ✅ Testable configuration service
/// Achieves testability through IFileSystem dependency injection
/// </summary>
public class ConfigurationService
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Constructor injection for IFileSystem
    /// </summary>
    /// <param name="fileSystem">File system abstraction interface</param>
    public ConfigurationService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Loads configuration, returns default if file doesn't exist
    /// </summary>
    /// <param name="filePath">Configuration file path</param>
    /// <param name="defaultValue">Default value (used when file doesn't exist or read fails)</param>
    /// <returns>Configuration content</returns>
    public async Task<string> LoadConfigurationAsync(string filePath, string defaultValue = "")
    {
        // ✅ Use injected _fileSystem instead of static File class
        if (!_fileSystem.File.Exists(filePath))
        {
            return defaultValue;
        }

        try
        {
            return await _fileSystem.File.ReadAllTextAsync(filePath);
        }
        catch (Exception)
        {
            // Return default on read failure
            return defaultValue;
        }
    }

    /// <summary>
    /// Saves configuration to file, automatically creates necessary directory structure
    /// </summary>
    /// <param name="filePath">Configuration file path</param>
    /// <param name="value">Value to save</param>
    public async Task SaveConfigurationAsync(string filePath, string value)
    {
        // ✅ Use Path.GetDirectoryName for path handling
        var directory = _fileSystem.Path.GetDirectoryName(filePath);

        // ✅ Auto-create directory (if doesn't exist)
        if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        await _fileSystem.File.WriteAllTextAsync(filePath, value);
    }

    /// <summary>
    /// Loads JSON format configuration file
    /// </summary>
    /// <typeparam name="T">Configuration object type</typeparam>
    /// <param name="filePath">Configuration file path</param>
    /// <returns>Deserialized configuration object, returns null on failure</returns>
    public async Task<T?> LoadJsonConfigurationAsync<T>(string filePath) where T : class
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return default;
        }

        try
        {
            var jsonContent = await _fileSystem.File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonContent);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// Saves JSON format configuration file
    /// </summary>
    /// <typeparam name="T">Configuration object type</typeparam>
    /// <param name="filePath">Configuration file path</param>
    /// <param name="settings">Configuration object to save</param>
    public async Task SaveJsonConfigurationAsync<T>(string filePath, T settings) where T : class
    {
        var directory = _fileSystem.Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        var jsonContent = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await _fileSystem.File.WriteAllTextAsync(filePath, jsonContent);
    }
}

#endregion

#region File Management Service

/// <summary>
/// File management service, provides advanced file and directory operations
/// </summary>
public class FileManagerService
{
    private readonly IFileSystem _fileSystem;

    public FileManagerService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Copies file to specified directory
    /// </summary>
    /// <param name="sourceFilePath">Source file path</param>
    /// <param name="targetDirectory">Target directory</param>
    /// <returns>Full path of target file</returns>
    /// <exception cref="FileNotFoundException">Source file doesn't exist</exception>
    public string CopyFileToDirectory(string sourceFilePath, string targetDirectory)
    {
        // Check if source file exists
        if (!_fileSystem.File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException($"Source file doesn't exist: {sourceFilePath}");
        }

        // Auto-create target directory
        if (!_fileSystem.Directory.Exists(targetDirectory))
        {
            _fileSystem.Directory.CreateDirectory(targetDirectory);
        }

        // Use Path.GetFileName to get file name
        var fileName = _fileSystem.Path.GetFileName(sourceFilePath);
        var targetFilePath = _fileSystem.Path.Combine(targetDirectory, fileName);

        // Copy file (overwrite existing)
        _fileSystem.File.Copy(sourceFilePath, targetFilePath, overwrite: true);
        return targetFilePath;
    }

    /// <summary>
    /// Backup file (adds timestamp)
    /// </summary>
    /// <param name="filePath">File path to backup</param>
    /// <returns>Full path of backup file</returns>
    /// <exception cref="FileNotFoundException">File doesn't exist</exception>
    public string BackupFile(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException($"File doesn't exist: {filePath}");
        }

        var directory = _fileSystem.Path.GetDirectoryName(filePath);
        var fileNameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(filePath);
        var extension = _fileSystem.Path.GetExtension(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var backupFileName = $"{fileNameWithoutExtension}_{timestamp}{extension}";
        var backupFilePath = _fileSystem.Path.Combine(directory ?? "", backupFileName);

        _fileSystem.File.Copy(filePath, backupFilePath);
        return backupFilePath;
    }

    /// <summary>
    /// Gets file information
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns>File information, returns null if file doesn't exist</returns>
    public FileInfoData? GetFileInfo(string filePath)
    {
        if (!_fileSystem.File.Exists(filePath))
        {
            return null;
        }

        // Use IFileInfo to get detailed information
        var fileInfo = _fileSystem.FileInfo.New(filePath);
        return new FileInfoData
        {
            Name = fileInfo.Name,
            FullPath = fileInfo.FullName,
            Size = fileInfo.Length,
            CreationTime = fileInfo.CreationTime,
            LastWriteTime = fileInfo.LastWriteTime,
            IsReadOnly = fileInfo.IsReadOnly
        };
    }

    /// <summary>
    /// Lists all files in directory
    /// </summary>
    /// <param name="directoryPath">Directory path</param>
    /// <param name="searchPattern">Search pattern (default *.*)</param>
    /// <returns>List of file paths</returns>
    public IEnumerable<string> ListFiles(string directoryPath, string searchPattern = "*.*")
    {
        if (!_fileSystem.Directory.Exists(directoryPath))
        {
            return Enumerable.Empty<string>();
        }

        return _fileSystem.Directory.GetFiles(directoryPath, searchPattern);
    }

    /// <summary>
    /// Ensures directory exists, creates if doesn't exist
    /// </summary>
    /// <param name="directoryPath">Directory path</param>
    /// <returns>Whether directory was successfully created or already exists</returns>
    public bool EnsureDirectoryExists(string directoryPath)
    {
        try
        {
            if (!_fileSystem.Directory.Exists(directoryPath))
            {
                _fileSystem.Directory.CreateDirectory(directoryPath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// File information data class
    /// </summary>
    public class FileInfoData
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public bool IsReadOnly { get; set; }
    }
}

#endregion

#region File Permission Service (Error Handling Example)

/// <summary>
/// File permission service, demonstrates handling various IO exceptions
/// </summary>
public class FilePermissionService
{
    private readonly IFileSystem _fileSystem;

    public FilePermissionService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Attempts to read file, handles various possible exceptions
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <param name="content">Read content (if successful)</param>
    /// <returns>Whether read was successful</returns>
    public bool TryReadFile(string filePath, out string? content)
    {
        content = null;

        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                return false;
            }

            content = _fileSystem.File.ReadAllText(filePath);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // Permission denied
            return false;
        }
        catch (IOException)
        {
            // File locked or other IO error
            return false;
        }
    }

    /// <summary>
    /// Attempts to write file, auto-creates directory and handles exceptions
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <param name="content">Content to write</param>
    /// <returns>Whether write was successful</returns>
    public async Task<bool> TrySaveFileAsync(string filePath, string content)
    {
        try
        {
            await _fileSystem.File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            // Try to create directory and retry
            var directory = _fileSystem.Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                return false;
            }

            try
            {
                _fileSystem.Directory.CreateDirectory(directory);
                await _fileSystem.File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch
            {
                return false;
            }
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}

#endregion

#region DI Registration Example

/*
 * DI registration in ASP.NET Core:
 *
 * // Program.cs or Startup.cs
 *
 * // Register IFileSystem real implementation
 * services.AddSingleton<IFileSystem, FileSystem>();
 *
 * // Register services using IFileSystem
 * services.AddScoped<ConfigurationService>();
 * services.AddScoped<FileManagerService>();
 * services.AddScoped<FilePermissionService>();
 */

#endregion
