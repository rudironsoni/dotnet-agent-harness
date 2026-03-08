using Xunit;
using DotnetAgentHarness.Cli.Services;
using FluentAssertions;

namespace DotnetAgentHarness.Cli.Tests.Services;

public class ConfigDetectorTests : IDisposable
{
    private readonly string _tempDir;

    public ConfigDetectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task HasDeleteTrueAsync_WhenFileNotExists_ReturnsFalse()
    {
        // Arrange
        var detector = new ConfigDetector();
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasDeleteTrueAsync_WhenDeleteTrue_ReturnsTrue()
    {
        // Arrange
        var detector = new ConfigDetector();
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        
        await File.WriteAllTextAsync(
            Path.Combine(rulesyncDir, "rulesync.jsonc"),
            @"{ ""delete"": true }");
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasDeleteTrueAsync_WhenDeleteFalse_ReturnsFalse()
    {
        // Arrange
        var detector = new ConfigDetector();
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        
        await File.WriteAllTextAsync(
            Path.Combine(rulesyncDir, "rulesync.jsonc"),
            @"{ ""delete"": false }");
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasDeleteTrueAsync_StripsSingleLineComments()
    {
        // Arrange
        var detector = new ConfigDetector();
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        
        await File.WriteAllTextAsync(
            Path.Combine(rulesyncDir, "rulesync.jsonc"),
            @"// This is a comment
{ ""delete"": true } // another comment");
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasDeleteTrueAsync_StripsMultiLineComments()
    {
        // Arrange
        var detector = new ConfigDetector();
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        
        await File.WriteAllTextAsync(
            Path.Combine(rulesyncDir, "rulesync.jsonc"),
            @"/* Multi-line
               comment */ { ""delete"": true }");
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasDeleteTrueAsync_WhenDeleteMissing_ReturnsFalse()
    {
        // Arrange
        var detector = new ConfigDetector();
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        
        await File.WriteAllTextAsync(
            Path.Combine(rulesyncDir, "rulesync.jsonc"),
            @"{ ""verbose"": true }");
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasDeleteTrueAsync_WhenInvalidJson_ReturnsFalse()
    {
        // Arrange
        var detector = new ConfigDetector();
        var rulesyncDir = Path.Combine(_tempDir, ".rulesync");
        Directory.CreateDirectory(rulesyncDir);
        
        await File.WriteAllTextAsync(
            Path.Combine(rulesyncDir, "rulesync.jsonc"),
            @"invalid json content");
        
        // Act
        var result = await detector.HasDeleteTrueAsync(_tempDir);
        
        // Assert
        result.Should().BeFalse();
    }
}
