// =============================================================================
// MockFileSystem Testing Examples
// Testing with MockFileSystem from System.IO.Abstractions.TestingHelpers
// =============================================================================

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace FileSystemTestingExamples.Tests;

#region Basic MockFileSystem Tests

/// <summary>
/// ConfigurationService test class
/// Demonstrates how to use MockFileSystem for file operation testing
/// </summary>
public class ConfigurationServiceTests
{
    [Fact]
    public async Task LoadConfigurationAsync_FileExists_ShouldReturnFileContent()
    {
        // Arrange - use Dictionary to initialize mock file system
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["config.json"] = new MockFileData("{ \"key\": \"value\" }")
        });

        var service = new ConfigurationService(mockFileSystem);

        // Act
        var result = await service.LoadConfigurationAsync("config.json");

        // Assert
        result.Should().Be("{ \"key\": \"value\" }");
    }

    [Fact]
    public async Task LoadConfigurationAsync_FileNotExists_ShouldReturnDefaultValue()
    {
        // Arrange - empty file system
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigurationService(mockFileSystem);
        var defaultValue = "default_config";

        // Act
        var result = await service.LoadConfigurationAsync("nonexistent.json", defaultValue);

        // Assert
        result.Should().Be(defaultValue);
    }

    [Fact]
    public async Task SaveConfigurationAsync_SpecifiedContent_ShouldCorrectlyWriteFile()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigurationService(mockFileSystem);
        var configPath = "config.json";
        var content = "{ \"setting\": true }";

        // Act
        await service.SaveConfigurationAsync(configPath, content);

        // Assert - verify final file system state
        mockFileSystem.File.Exists(configPath).Should().BeTrue();
        var savedContent = await mockFileSystem.File.ReadAllTextAsync(configPath);
        savedContent.Should().Be(content);
    }

    [Fact]
    public async Task SaveConfigurationAsync_DirectoryNotExists_ShouldAutoCreateDirectory()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new ConfigurationService(mockFileSystem);
        var configPath = @"C:\configs\app\settings.json";

        // Act
        await service.SaveConfigurationAsync(configPath, "content");

        // Assert
        mockFileSystem.Directory.Exists(@"C:\configs\app").Should().BeTrue();
        mockFileSystem.File.Exists(configPath).Should().BeTrue();
    }

    [Fact]
    public async Task LoadJsonConfigurationAsync_ValidJson_ShouldCorrectlyDeserialize()
    {
        // Arrange
        var settings = new AppSettings
        {
            ApplicationName = "Test App",
            Version = "1.0.0"
        };
        var json = JsonSerializer.Serialize(settings);

        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["settings.json"] = new MockFileData(json)
        });

        var service = new ConfigurationService(mockFileSystem);

        // Act
        var result = await service.LoadJsonConfigurationAsync<AppSettings>("settings.json");

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationName.Should().Be("Test App");
        result.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task LoadJsonConfigurationAsync_InvalidJson_ShouldReturnNull()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["invalid.json"] = new MockFileData("{ invalid json }")
        });

        var service = new ConfigurationService(mockFileSystem);

        // Act
        var result = await service.LoadJsonConfigurationAsync<AppSettings>("invalid.json");

        // Assert
        result.Should().BeNull();
    }
}

/// <summary>
/// Test settings class
/// </summary>
public class AppSettings
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

#endregion

#region Directory Operation Tests

/// <summary>
/// FileManagerService test class
/// Demonstrates directory operation and file information testing
/// </summary>
public class FileManagerServiceTests
{
    [Fact]
    public void CopyFileToDirectory_FileExists_ShouldSuccessfullyCopy()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\source\test.txt"] = new MockFileData("test content")
        });

        var service = new FileManagerService(mockFileSystem);

        // Act
        var result = service.CopyFileToDirectory(@"C:\source\test.txt", @"C:\target");

        // Assert
        result.Should().Be(@"C:\target\test.txt");
        mockFileSystem.File.Exists(@"C:\target\test.txt").Should().BeTrue();
        mockFileSystem.File.ReadAllText(@"C:\target\test.txt").Should().Be("test content");
    }

    [Fact]
    public void CopyFileToDirectory_TargetDirectoryNotExists_ShouldAutoCreate()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\source\file.txt"] = new MockFileData("content")
        });

        var service = new FileManagerService(mockFileSystem);

        // Act
        service.CopyFileToDirectory(@"C:\source\file.txt", @"C:\target\subfolder");

        // Assert
        mockFileSystem.Directory.Exists(@"C:\target\subfolder").Should().BeTrue();
        mockFileSystem.File.Exists(@"C:\target\subfolder\file.txt").Should().BeTrue();
    }

    [Fact]
    public void CopyFileToDirectory_SourceFileNotExists_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);

        // Act & Assert
        var action = () => service.CopyFileToDirectory(@"C:\nonexistent.txt", @"C:\target");
        action.Should().Throw<FileNotFoundException>()
              .WithMessage("*Source file doesn't exist*");
    }

    [Fact]
    public void BackupFile_FileExists_ShouldCreateTimestampedBackup()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\data\important.txt"] = new MockFileData("important data")
        });

        var service = new FileManagerService(mockFileSystem);

        // Act
        var backupPath = service.BackupFile(@"C:\data\important.txt");

        // Assert
        backupPath.Should().StartWith(@"C:\data\important_");
        backupPath.Should().EndWith(".txt");
        mockFileSystem.File.Exists(backupPath).Should().BeTrue();
        mockFileSystem.File.ReadAllText(backupPath).Should().Be("important data");
    }

    [Fact]
    public void BackupFile_FileNotExists_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);

        // Act & Assert
        var action = () => service.BackupFile(@"C:\nonexistent.txt");
        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetFileInfo_FileExists_ShouldReturnCorrectInfo()
    {
        // Arrange
        var content = "Hello, World!";
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\test.txt"] = new MockFileData(content)
        });

        var service = new FileManagerService(mockFileSystem);

        // Act
        var result = service.GetFileInfo(@"C:\test.txt");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("test.txt");
        result.Size.Should().Be(content.Length);
    }

    [Fact]
    public void GetFileInfo_FileNotExists_ShouldReturnNull()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);

        // Act
        var result = service.GetFileInfo(@"C:\nonexistent.txt");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ListFiles_DirectoryHasFiles_ShouldReturnFileList()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\data\file1.txt"] = new MockFileData("content1"),
            [@"C:\data\file2.txt"] = new MockFileData("content2"),
            [@"C:\data\file3.csv"] = new MockFileData("content3"),
            [@"C:\other\file4.txt"] = new MockFileData("content4")
        });

        var service = new FileManagerService(mockFileSystem);

        // Act
        var result = service.ListFiles(@"C:\data", "*.txt").ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.EndsWith("file1.txt"));
        result.Should().Contain(f => f.EndsWith("file2.txt"));
    }

    [Fact]
    public void ListFiles_DirectoryNotExists_ShouldReturnEmptyList()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FileManagerService(mockFileSystem);

        // Act
        var result = service.ListFiles(@"C:\nonexistent");

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region Using NSubstitute for Error Scenarios

/// <summary>
/// Using NSubstitute to test error handling
/// When needing to simulate specific exceptions, NSubstitute is more flexible than MockFileSystem
/// </summary>
public class FilePermissionServiceTests
{
    [Fact]
    public void TryReadFile_FileExistsAndReadable_ShouldReturnTrueAndOutputContent()
    {
        // Arrange - use MockFileSystem (normal scenario)
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["readable.txt"] = new MockFileData("file content")
        });

        var service = new FilePermissionService(mockFileSystem);

        // Act
        var result = service.TryReadFile("readable.txt", out var content);

        // Assert
        result.Should().BeTrue();
        content.Should().Be("file content");
    }

    [Fact]
    public void TryReadFile_FileNotExists_ShouldReturnFalse()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FilePermissionService(mockFileSystem);

        // Act
        var result = service.TryReadFile("nonexistent.txt", out var content);

        // Assert
        result.Should().BeFalse();
        content.Should().BeNull();
    }

    [Fact]
    public void TryReadFile_PermissionDenied_ShouldReturnFalse()
    {
        // Arrange - use NSubstitute to simulate permission exception
        var mockFileSystem = Substitute.For<IFileSystem>();
        var mockFile = Substitute.For<IFile>();

        mockFileSystem.File.Returns(mockFile);
        mockFile.Exists("protected.txt").Returns(true);
        mockFile.ReadAllText("protected.txt")
                .Throws(new UnauthorizedAccessException("Access denied"));

        var service = new FilePermissionService(mockFileSystem);

        // Act
        var result = service.TryReadFile("protected.txt", out var content);

        // Assert
        result.Should().BeFalse();
        content.Should().BeNull();
    }

    [Fact]
    public void TryReadFile_FileLocked_ShouldReturnFalse()
    {
        // Arrange - use NSubstitute to simulate IO exception
        var mockFileSystem = Substitute.For<IFileSystem>();
        var mockFile = Substitute.For<IFile>();

        mockFileSystem.File.Returns(mockFile);
        mockFile.Exists("locked.txt").Returns(true);
        mockFile.ReadAllText("locked.txt")
                .Throws(new IOException("File is being used by another process"));

        var service = new FilePermissionService(mockFileSystem);

        // Act
        var result = service.TryReadFile("locked.txt", out var content);

        // Assert
        result.Should().BeFalse();
        content.Should().BeNull();
    }

    [Fact]
    public async Task TrySaveFileAsync_NormalWrite_ShouldReturnTrue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FilePermissionService(mockFileSystem);

        // Act
        var result = await service.TrySaveFileAsync("output.txt", "content");

        // Assert
        result.Should().BeTrue();
        mockFileSystem.File.Exists("output.txt").Should().BeTrue();
    }

    [Fact]
    public async Task TrySaveFileAsync_DirectoryNotExistsButCanCreate_ShouldSuccessfullyWrite()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new FilePermissionService(mockFileSystem);

        // Act
        var result = await service.TrySaveFileAsync(@"C:\new\folder\file.txt", "content");

        // Assert
        result.Should().BeTrue();
        mockFileSystem.File.Exists(@"C:\new\folder\file.txt").Should().BeTrue();
    }
}

#endregion

#region Advanced Test Patterns

/// <summary>
/// Advanced test patterns demonstration
/// </summary>
public class AdvancedFileSystemTestPatterns
{
    /// <summary>
    /// Tests complex directory structure
    /// </summary>
    [Fact]
    public void ComplexDirectoryStructure_ShouldCorrectlyHandle()
    {
        // Arrange - create complex directory structure
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [@"C:\app\configs\app.json"] = new MockFileData("""
                {
                  "apiUrl": "https://api.test.com",
                  "timeout": 30
                }
                """),
            [@"C:\app\logs\app.log"] = new MockFileData("2024-01-01 10:00:00 INFO Application started"),
            [@"C:\app\data\users.csv"] = new MockFileData("Name,Age\nJohn,25\nJane,30"),
            [@"C:\temp\"] = new MockDirectoryData()  // empty directory
        });

        // Assert - verify structure
        mockFileSystem.Directory.Exists(@"C:\app\configs").Should().BeTrue();
        mockFileSystem.Directory.Exists(@"C:\app\logs").Should().BeTrue();
        mockFileSystem.Directory.Exists(@"C:\temp").Should().BeTrue();
        mockFileSystem.File.Exists(@"C:\app\configs\app.json").Should().BeTrue();
    }

    /// <summary>
    /// Using AddFile to dynamically add files
    /// </summary>
    [Fact]
    public void AddFile_DynamicallyAddFile_ShouldBeReadable()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();

        // Act - dynamically add file
        mockFileSystem.AddFile(@"C:\dynamic\file.txt", new MockFileData("dynamic content"));

        // Assert
        mockFileSystem.File.Exists(@"C:\dynamic\file.txt").Should().BeTrue();
        mockFileSystem.File.ReadAllText(@"C:\dynamic\file.txt").Should().Be("dynamic content");
    }

    /// <summary>
    /// Tests multiple file name variations
    /// </summary>
    [Theory]
    [InlineData("simple.txt")]
    [InlineData("file with spaces.txt")]
    [InlineData("file-with-hyphens.txt")]
    [InlineData("file_with_underscores.txt")]
    [InlineData("file.txt")]  // unicode file name
    public void CopyFile_VariousFileNames_ShouldCorrectlyHandle(string fileName)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var sourceFile = $@"C:\source\{fileName}";
        mockFileSystem.AddFile(sourceFile, new MockFileData("test content"));

        var service = new FileManagerService(mockFileSystem);

        // Act
        var result = service.CopyFileToDirectory(sourceFile, @"C:\target");

        // Assert
        result.Should().Be($@"C:\target\{fileName}");
        mockFileSystem.File.Exists(result).Should().BeTrue();
    }

    /// <summary>
    /// Tests file system isolation - each test should use independent MockFileSystem
    /// </summary>
    [Fact]
    public void FileSystemIsolation_TestsDoNotAffectEachOther()
    {
        // Arrange - first isolated file system
        var mockFileSystem1 = new MockFileSystem();
        mockFileSystem1.AddFile("test.txt", new MockFileData("content1"));

        // Arrange - second isolated file system
        var mockFileSystem2 = new MockFileSystem();
        mockFileSystem2.AddFile("test.txt", new MockFileData("content2"));

        // Assert - two file systems are independent
        mockFileSystem1.File.ReadAllText("test.txt").Should().Be("content1");
        mockFileSystem2.File.ReadAllText("test.txt").Should().Be("content2");
    }
}

#endregion

#region Test Data Helper Classes

/// <summary>
/// Test data helper class
/// Used to create reusable test file structures
/// </summary>
public static class FileTestDataHelper
{
    /// <summary>
    /// Creates standard test file structure
    /// </summary>
    public static Dictionary<string, MockFileData> CreateTestFileStructure()
    {
        return new Dictionary<string, MockFileData>
        {
            [@"C:\app\configs\app.json"] = new MockFileData("""
                {
                  "apiUrl": "https://api.test.com",
                  "timeout": 30
                }
                """),
            [@"C:\app\logs\app.log"] = new MockFileData("2024-01-01 10:00:00 INFO Application started"),
            [@"C:\app\data\users.csv"] = new MockFileData("Name,Age\nJohn,25\nJane,30"),
            [@"C:\temp\"] = new MockDirectoryData()
        };
    }

    /// <summary>
    /// Creates configuration file test structure
    /// </summary>
    public static Dictionary<string, MockFileData> CreateConfigTestStructure()
    {
        return new Dictionary<string, MockFileData>
        {
            [@"C:\config\appsettings.json"] = new MockFileData("""
                {
                  "ConnectionStrings": {
                    "DefaultConnection": "Server=localhost;Database=TestDb;"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  }
                }
                """),
            [@"C:\config\appsettings.Development.json"] = new MockFileData("""
                {
                  "Logging": {
                    "LogLevel": {
                      "Default": "Debug"
                    }
                  }
                }
                """)
        };
    }
}

/// <summary>
/// Example using test data helper class
/// </summary>
public class FileTestDataHelperUsageTests
{
    [Fact]
    public void UsingPredefinedTestStructure()
    {
        // Arrange - use helper to create file system
        var mockFileSystem = new MockFileSystem(FileTestDataHelper.CreateTestFileStructure());

        // Assert
        mockFileSystem.File.Exists(@"C:\app\configs\app.json").Should().BeTrue();
        mockFileSystem.Directory.Exists(@"C:\temp").Should().BeTrue();
    }
}

#endregion
