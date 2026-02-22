// =============================================================================
// Stream Processing and Configuration Management Examples
// Stream Processing and Configuration Management Examples
// =============================================================================

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace FileSystemTestingExamples;

#region Stream Processing Service

/// <summary>
/// Stream processing service
/// Demonstrates handling large files using streams instead of loading entire file
/// </summary>
public class StreamProcessorService
{
    private readonly IFileSystem _fileSystem;

    public StreamProcessorService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Counts file lines (using stream, memory efficient)
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns>Line count</returns>
    public async Task<int> CountLinesAsync(string filePath)
    {
        using var stream = _fileSystem.File.OpenRead(filePath);
        using var reader = new StreamReader(stream);

        int lineCount = 0;
        while (await reader.ReadLineAsync() != null)
        {
            lineCount++;
        }

        return lineCount;
    }

    /// <summary>
    /// Processes large file line by line
    /// </summary>
    /// <param name="inputPath">Input file path</param>
    /// <param name="outputPath">Output file path</param>
    /// <param name="processor">Processing function for each line</param>
    public async Task ProcessLargeFileAsync(
        string inputPath,
        string outputPath,
        Func<string, string> processor)
    {
        using var inputStream = _fileSystem.File.OpenRead(inputPath);
        using var outputStream = _fileSystem.File.Create(outputPath);
        using var reader = new StreamReader(inputStream);
        using var writer = new StreamWriter(outputStream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var processedLine = processor(line);
            await writer.WriteLineAsync(processedLine);
        }
    }

    /// <summary>
    /// Gets file statistics
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns>Statistics</returns>
    public async Task<FileStatistics> GetFileStatisticsAsync(string filePath)
    {
        var stats = new FileStatistics();

        using var stream = _fileSystem.File.OpenRead(filePath);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            stats.LineCount++;
            stats.CharacterCount += line.Length;
            stats.WordCount += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }

        return stats;
    }

    /// <summary>
    /// File statistics information
    /// </summary>
    public class FileStatistics
    {
        public int LineCount { get; set; }
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
    }
}

#endregion

#region Configuration Management Service

/// <summary>
/// Integrated configuration management service
/// Demonstrates complete configuration file lifecycle management
/// </summary>
public class ConfigManagerService
{
    private readonly IFileSystem _fileSystem;
    private readonly string _configDirectory;

    public ConfigManagerService(IFileSystem fileSystem, string configDirectory = "config")
    {
        _fileSystem = fileSystem;
        _configDirectory = configDirectory;
    }

    /// <summary>
    /// Initializes configuration directory
    /// </summary>
    public void InitializeConfigDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_configDirectory) &&
            !_fileSystem.Directory.Exists(_configDirectory))
        {
            _fileSystem.Directory.CreateDirectory(_configDirectory);
        }
    }

    /// <summary>
    /// Loads application settings
    /// </summary>
    /// <returns>Application settings</returns>
    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");

        if (!_fileSystem.File.Exists(configPath))
        {
            // Create default settings when file doesn't exist
            var defaultSettings = new AppSettings();
            await SaveAppSettingsAsync(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var jsonContent = await _fileSystem.File.ReadAllTextAsync(configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(jsonContent);
            return settings ?? new AppSettings();
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// Saves application settings
    /// </summary>
    /// <param name="settings">Application settings</param>
    public async Task SaveAppSettingsAsync(AppSettings settings)
    {
        InitializeConfigDirectory();

        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");
        var jsonContent = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await _fileSystem.File.WriteAllTextAsync(configPath, jsonContent);
    }

    /// <summary>
    /// Backs up existing configuration
    /// </summary>
    /// <returns>Backup file path</returns>
    public string BackupConfiguration()
    {
        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");

        if (!_fileSystem.File.Exists(configPath))
        {
            throw new FileNotFoundException("Configuration file to backup not found");
        }

        var backupDirectory = _fileSystem.Path.Combine(_configDirectory, "backup");
        if (!_fileSystem.Directory.Exists(backupDirectory))
        {
            _fileSystem.Directory.CreateDirectory(backupDirectory);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"appsettings_{timestamp}.json";
        var backupPath = _fileSystem.Path.Combine(backupDirectory, backupFileName);

        _fileSystem.File.Copy(configPath, backupPath);
        return backupPath;
    }

    /// <summary>
    /// Lists all backups
    /// </summary>
    /// <returns>List of backup file paths</returns>
    public IEnumerable<string> ListBackups()
    {
        var backupDirectory = _fileSystem.Path.Combine(_configDirectory, "backup");

        if (!_fileSystem.Directory.Exists(backupDirectory))
        {
            return Enumerable.Empty<string>();
        }

        return _fileSystem.Directory.GetFiles(backupDirectory, "appsettings_*.json")
                          .OrderByDescending(f => f);
    }

    /// <summary>
    /// Restores from backup
    /// </summary>
    /// <param name="backupPath">Backup file path</param>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        if (!_fileSystem.File.Exists(backupPath))
        {
            throw new FileNotFoundException($"Backup file doesn't exist: {backupPath}");
        }

        var configPath = _fileSystem.Path.Combine(_configDirectory, "appsettings.json");
        var content = await _fileSystem.File.ReadAllTextAsync(backupPath);
        await _fileSystem.File.WriteAllTextAsync(configPath, content);
    }

    /// <summary>
    /// Application settings
    /// </summary>
    public class AppSettings
    {
        public string ApplicationName { get; set; } = "FileSystem Testing Demo";
        public string Version { get; set; } = "1.0.0";
        public DatabaseSettings Database { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    /// <summary>
    /// Database settings
    /// </summary>
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = "Server=localhost;Database=TestDb;";
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Logging settings
    /// </summary>
    public class LoggingSettings
    {
        public string Level { get; set; } = "Information";
        public bool EnableFileLogging { get; set; } = true;
        public string LogDirectory { get; set; } = "logs";
    }
}

#endregion

#region Test Classes

/// <summary>
/// Stream processing service tests
/// </summary>
public class StreamProcessorServiceTests
{
    [Fact]
    public async Task CountLinesAsync_MultiLineFile_ShouldReturnCorrectLineCount()
    {
        // Arrange
        var content = "Line 1\nLine 2\nLine 3\nLine 4";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["test.txt"] = new MockFileData(content)
        });

        var service = new StreamProcessorService(mockFileSystem);

        // Act
        var result = await service.CountLinesAsync("test.txt");

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public async Task CountLinesAsync_EmptyFile_ShouldReturnZero()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["empty.txt"] = new MockFileData("")
        });

        var service = new StreamProcessorService(mockFileSystem);

        // Act
        var result = await service.CountLinesAsync("empty.txt");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ProcessLargeFileAsync_ProcessEachLine_ShouldCorrectlyTransformAndWrite()
    {
        // Arrange
        var inputContent = "hello\nworld\ntest";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["input.txt"] = new MockFileData(inputContent)
        });

        var service = new StreamProcessorService(mockFileSystem);

        // Act - convert each line to uppercase
        await service.ProcessLargeFileAsync("input.txt", "output.txt", line => line.ToUpper());

        // Assert
        var outputContent = mockFileSystem.File.ReadAllText("output.txt");
        outputContent.Should().Contain("HELLO");
        outputContent.Should().Contain("WORLD");
        outputContent.Should().Contain("TEST");
    }

    [Fact]
    public async Task GetFileStatisticsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var content = "Hello World\nThis is a test\nThird line";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["stats.txt"] = new MockFileData(content)
        });

        var service = new StreamProcessorService(mockFileSystem);

        // Act
        var result = await service.GetFileStatisticsAsync("stats.txt");

        // Assert
        result.LineCount.Should().Be(3);
        result.WordCount.Should().Be(8); // Hello, World, This, is, a, test, Third, line
    }
}

/// <summary>
/// Configuration management service tests
/// </summary>
public class ConfigManagerServiceTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly ConfigManagerService _service;

    public ConfigManagerServiceTests()
    {
        _mockFileSystem = new MockFileSystem();
        _service = new ConfigManagerService(_mockFileSystem, "test-config");
    }

    [Fact]
    public async Task LoadAppSettingsAsync_SettingsFileNotExists_ShouldReturnAndCreateDefaultSettings()
    {
        // Act
        var result = await _service.LoadAppSettingsAsync();

        // Assert
        result.Should().NotBeNull();
        result.ApplicationName.Should().Be("FileSystem Testing Demo");
        result.Version.Should().Be("1.0.0");
        result.Database.Should().NotBeNull();
        result.Logging.Should().NotBeNull();

        // Should create default settings file
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.File.Exists(configPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAppSettingsAsync_SaveSettings_ShouldCorrectlyWriteFile()
    {
        // Arrange
        var settings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Test App",
            Version = "2.0.0",
            Database = new ConfigManagerService.DatabaseSettings
            {
                ConnectionString = "Server=test;Database=TestDb;",
                TimeoutSeconds = 60
            }
        };

        // Act
        await _service.SaveAppSettingsAsync(settings);

        // Assert
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.File.Exists(configPath).Should().BeTrue();

        var savedContent = await _mockFileSystem.File.ReadAllTextAsync(configPath);
        var savedSettings = JsonSerializer.Deserialize<ConfigManagerService.AppSettings>(savedContent);

        savedSettings!.ApplicationName.Should().Be("Test App");
        savedSettings.Version.Should().Be("2.0.0");
        savedSettings.Database.ConnectionString.Should().Be("Server=test;Database=TestDb;");
        savedSettings.Database.TimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public async Task LoadAppSettingsAsync_SettingsFileExists_ShouldReturnCorrectSettings()
    {
        // Arrange
        var settings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Existing App",
            Version = "3.0.0"
        };

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.AddFile(configPath, new MockFileData(json));

        // Act
        var result = await _service.LoadAppSettingsAsync();

        // Assert
        result.ApplicationName.Should().Be("Existing App");
        result.Version.Should().Be("3.0.0");
    }

    [Fact]
    public void BackupConfiguration_SettingsFileExists_ShouldCreateBackupFile()
    {
        // Arrange
        var settings = new ConfigManagerService.AppSettings();
        var json = JsonSerializer.Serialize(settings);
        var configPath = @"test-config\appsettings.json";
        _mockFileSystem.AddFile(configPath, new MockFileData(json));

        // Act
        var backupPath = _service.BackupConfiguration();

        // Assert
        _mockFileSystem.File.Exists(backupPath).Should().BeTrue();
        backupPath.Should().StartWith(@"test-config\backup\appsettings_");
        backupPath.Should().EndWith(".json");

        var backupContent = _mockFileSystem.File.ReadAllText(backupPath);
        backupContent.Should().Be(json);
    }

    [Fact]
    public void BackupConfiguration_SettingsFileNotExists_ShouldThrowFileNotFoundException()
    {
        // Act & Assert
        var action = () => _service.BackupConfiguration();
        action.Should().Throw<FileNotFoundException>()
              .WithMessage("*Configuration file to backup not found*");
    }

    [Fact]
    public void ListBackups_HasMultipleBackups_ShouldReturnInDescendingTimeOrder()
    {
        // Arrange
        _mockFileSystem.AddFile(@"test-config\backup\appsettings_20240101_100000.json",
            new MockFileData("{}"));
        _mockFileSystem.AddFile(@"test-config\backup\appsettings_20240102_100000.json",
            new MockFileData("{}"));
        _mockFileSystem.AddFile(@"test-config\backup\appsettings_20240103_100000.json",
            new MockFileData("{}"));

        // Act
        var backups = _service.ListBackups().ToList();

        // Assert
        backups.Should().HaveCount(3);
        backups[0].Should().Contain("20240103"); // newest first
    }

    [Fact]
    public void ListBackups_NoBackups_ShouldReturnEmptyList()
    {
        // Act
        var backups = _service.ListBackups();

        // Assert
        backups.Should().BeEmpty();
    }

    [Fact]
    public async Task RestoreFromBackupAsync_BackupExists_ShouldRestoreSettings()
    {
        // Arrange
        var originalSettings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Original App"
        };
        var backupSettings = new ConfigManagerService.AppSettings
        {
            ApplicationName = "Backup App"
        };

        var configPath = @"test-config\appsettings.json";
        var backupPath = @"test-config\backup\appsettings_backup.json";

        _mockFileSystem.AddFile(configPath,
            new MockFileData(JsonSerializer.Serialize(originalSettings)));
        _mockFileSystem.AddFile(backupPath,
            new MockFileData(JsonSerializer.Serialize(backupSettings)));

        // Act
        await _service.RestoreFromBackupAsync(backupPath);

        // Assert
        var restoredContent = await _mockFileSystem.File.ReadAllTextAsync(configPath);
        var restoredSettings = JsonSerializer.Deserialize<ConfigManagerService.AppSettings>(restoredContent);
        restoredSettings!.ApplicationName.Should().Be("Backup App");
    }

    [Fact]
    public async Task RestoreFromBackupAsync_BackupNotExists_ShouldThrowFileNotFoundException()
    {
        // Act & Assert
        var action = async () => await _service.RestoreFromBackupAsync(@"nonexistent.json");
        await action.Should().ThrowAsync<FileNotFoundException>();
    }
}

#endregion

#region Integration Test Examples

/// <summary>
/// Complete workflow integration test
/// Demonstrates full configuration file lifecycle
/// </summary>
public class ConfigManagerIntegrationTests
{
    [Fact]
    public async Task CompleteSettingsLifecycle_CreateModifyBackupRestore()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigManagerService(mockFileSystem, "app-config");

        // Step 1: Load default settings (should auto-create)
        var settings = await service.LoadAppSettingsAsync();
        settings.ApplicationName.Should().Be("FileSystem Testing Demo");

        // Step 2: Modify settings
        settings.ApplicationName = "Modified App";
        settings.Database.ConnectionString = "Server=production;Database=ProdDb;";
        await service.SaveAppSettingsAsync(settings);

        // Step 3: Create backup
        var backupPath = service.BackupConfiguration();
        mockFileSystem.File.Exists(backupPath).Should().BeTrue();

        // Step 4: Modify settings again
        settings.ApplicationName = "Another Modification";
        await service.SaveAppSettingsAsync(settings);

        // Step 5: Restore from backup
        await service.RestoreFromBackupAsync(backupPath);

        // Assert: Verify restored settings
        var restoredSettings = await service.LoadAppSettingsAsync();
        restoredSettings.ApplicationName.Should().Be("Modified App");
        restoredSettings.Database.ConnectionString.Should().Be("Server=production;Database=ProdDb;");
    }
}

#endregion
