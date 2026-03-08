using Xunit;
using DotnetAgentHarness.Cli.Services;
using AwesomeAssertions;

namespace DotnetAgentHarness.Cli.Tests.Services;

public class TransactionManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly TransactionManager _manager;

    public TransactionManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _manager = new TransactionManager();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public async Task BackupAsync_WhenDirectoryNotExists_ReturnsEmptyString()
    {
        // Arrange - directory doesn't exist
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent");

        // Act
        var result = await _manager.BackupAsync(nonExistentPath);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BackupAsync_CreatesBackupWithCorrectName()
    {
        // Arrange
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        await File.WriteAllTextAsync(Path.Combine(rulesyncDir, "config.json"), "test");

        // Act
        var result = await _manager.BackupAsync(_tempDir);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(".backup.");
        Directory.Exists(result).Should().BeTrue();
        File.Exists(Path.Combine(result, "config.json")).Should().BeTrue();
    }

    [Fact]
    public async Task BackupAsync_CopiesNestedDirectories()
    {
        // Arrange
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        var nestedDir = Path.Combine(rulesyncDir, "hooks");
        Directory.CreateDirectory(nestedDir);
        await File.WriteAllTextAsync(Path.Combine(nestedDir, "hook.sh"), "#!/bin/bash");

        // Act
        var result = await _manager.BackupAsync(_tempDir);

        // Assert
        File.Exists(Path.Combine(result, "hooks", "hook.sh")).Should().BeTrue();
    }

    [Fact]
    public async Task RestoreAsync_RestoresFromBackup()
    {
        // Arrange
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        await File.WriteAllTextAsync(Path.Combine(rulesyncDir, "original.txt"), "original");
        
        var backupPath = await _manager.BackupAsync(_tempDir);
        
        // Modify original
        await File.WriteAllTextAsync(Path.Combine(rulesyncDir, "modified.txt"), "modified");
        File.Delete(Path.Combine(rulesyncDir, "original.txt"));

        // Act
        await _manager.RestoreAsync(backupPath);

        // Assert
        File.Exists(Path.Combine(rulesyncDir, "original.txt")).Should().BeTrue();
        File.Exists(Path.Combine(rulesyncDir, "modified.txt")).Should().BeFalse();
    }

    [Fact]
    public async Task RestoreAsync_WhenBackupEmpty_DoesNothing()
    {
        // Arrange
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        await File.WriteAllTextAsync(Path.Combine(rulesyncDir, "file.txt"), "content");

        // Act
        await _manager.RestoreAsync("");

        // Assert
        File.Exists(Path.Combine(rulesyncDir, "file.txt")).Should().BeTrue();
    }

    [Fact]
    public async Task CleanupAsync_RemovesBackupDirectory()
    {
        // Arrange
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        var backupPath = await _manager.BackupAsync(_tempDir);

        // Act
        await _manager.CleanupAsync(backupPath);

        // Assert
        Directory.Exists(backupPath).Should().BeFalse();
    }

    [Fact]
    public async Task CleanupAsync_WhenBackupEmpty_DoesNothing()
    {
        // Act - should not throw
        await _manager.CleanupAsync("");
        await _manager.CleanupAsync("/nonexistent/path");

        // Assert - no exception
        true.Should().BeTrue();
    }
}
