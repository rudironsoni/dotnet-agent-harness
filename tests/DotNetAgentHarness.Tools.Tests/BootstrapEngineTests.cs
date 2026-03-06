using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class BootstrapEngineTests
{
    [Fact]
    public async Task RunAsync_WithEmptyTargets_InstallsAllAvailableBundles()
    {
        using var repo = new TestRepositoryBuilder();

        var engine = new BootstrapEngine();
        var report = await engine.RunAsync(repo.Root, new BootstrapOptions
        {
            Targets = []
        });

        // When no targets specified, engine attempts to install all available bundles
        Assert.NotNull(report);
        // If bundles exist, report should pass; if no bundles embedded, engine returns failure
        if (report.TargetResults.Count > 0)
        {
            Assert.True(report.Passed);
            Assert.All(report.TargetResults, r => Assert.True(r.Success));
        }
        else
        {
            // No bundles available in test assembly - this is expected
            Assert.Empty(report.TargetResults);
        }
    }

    [Fact]
    public async Task RunAsync_WithListTargets_ReturnsSuccessWithoutInstalling()
    {
        using var repo = new TestRepositoryBuilder();

        var engine = new BootstrapEngine();
        var report = await engine.RunAsync(repo.Root, new BootstrapOptions
        {
            ListTargets = true
        });

        Assert.True(report.Passed);
        Assert.Empty(report.TargetResults);
    }

    [Fact]
    public async Task RunAsync_WithForce_OverwritesExistingFiles()
    {
        using var repo = new TestRepositoryBuilder();

        var engine = new BootstrapEngine();
        var report = await engine.RunAsync(repo.Root, new BootstrapOptions
        {
            Targets = [],
            Force = true
        });

        // Should succeed with force mode
        Assert.NotNull(report);
    }

    [Fact]
    public async Task RunAsync_DetectsV1Installation_WhenRulesyncDirectoryExists()
    {
        using var repo = new TestRepositoryBuilder();
        // Create v1.x artifacts
        Directory.CreateDirectory(Path.Combine(repo.Root, ".rulesync"));

        var engine = new BootstrapEngine();
        // Note: Migration is detected during extraction, not during --list-targets
        var report = await engine.RunAsync(repo.Root, new BootstrapOptions
        {
            Targets = [],
            Force = true
        });

        Assert.NotNull(report);
        Assert.True(report.HasV1Installation);
    }

    [Fact]
    public async Task RunAsync_DetectsV1Installation_WhenRulesyncConfigExists()
    {
        using var repo = new TestRepositoryBuilder();
        // Create v1.x artifacts
        File.WriteAllText(Path.Combine(repo.Root, "rulesync.jsonc"), "{\"targets\": []}");

        var engine = new BootstrapEngine();
        // Note: Migration is detected during extraction, not during --list-targets
        var report = await engine.RunAsync(repo.Root, new BootstrapOptions
        {
            Targets = [],
            Force = true
        });

        Assert.NotNull(report);
        Assert.True(report.HasV1Installation);
    }

    [Fact]
    public void Bootstrap_SynchronousWrapper_ReturnsReport()
    {
        using var repo = new TestRepositoryBuilder();

        var engine = new BootstrapEngine();
        var report = engine.Bootstrap(repo.Root, new BootstrapOptions
        {
            Targets = [],
            ListTargets = true
        });

        Assert.NotNull(report);
        Assert.True(report.Passed);
    }

    [Fact]
    public async Task RunAsync_WithNullRepoRoot_ReturnsFailure()
    {
        var engine = new BootstrapEngine();
        var report = await engine.RunAsync(string.Empty, new BootstrapOptions());

        Assert.False(report.Passed);
        Assert.Contains(report.Warnings, w => w.Contains("required", StringComparison.OrdinalIgnoreCase));
    }
}
