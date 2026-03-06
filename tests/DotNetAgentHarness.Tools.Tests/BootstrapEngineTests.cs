using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class BootstrapEngineTests
{
    [Fact]
    public void Bootstrap_CreatesToolManifestRuleSyncConfigAndState()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var report = BootstrapEngine.Bootstrap(repo.Root, new BootstrapOptions
        {
            Targets = [PromptPlatforms.ClaudeCode, PromptPlatforms.OpenCode, PromptPlatforms.CodexCli, PromptPlatforms.GeminiCli, PromptPlatforms.Copilot, PromptPlatforms.Antigravity, PromptPlatforms.FactoryDroid]
        });

        Assert.True(report.Passed);
        Assert.True(File.Exists(Path.Combine(repo.Root, ".config", "dotnet-tools.json")));
        Assert.True(File.Exists(Path.Combine(repo.Root, "rulesync.jsonc")));
        Assert.True(File.Exists(Path.Combine(repo.Root, ".dotnet-agent-harness", "project-profile.json")));
        Assert.True(File.Exists(Path.Combine(repo.Root, ".dotnet-agent-harness", "recommendations.json")));
        Assert.True(File.Exists(Path.Combine(repo.Root, ".dotnet-agent-harness", "doctor-report.json")));
        Assert.True(File.Exists(Path.Combine(repo.Root, ".dotnet-agent-harness", "bootstrap-report.json")));
        Assert.Equal(BootstrapProfileCatalog.PlatformNative, report.Profile);
        Assert.Contains("hooks", report.Features);
        Assert.Contains(report.Targets, target => target.Id == PromptPlatforms.GeminiCli);
        Assert.Contains(report.Targets, target => target.Id == PromptPlatforms.Antigravity);
        Assert.Contains(report.Targets, target => target.Id == PromptPlatforms.FactoryDroid);
        Assert.Contains(report.Warnings, warning => warning.Contains(".rulesync/", System.StringComparison.Ordinal));
        Assert.Contains(report.Targets.Single(target => target.Id == PromptPlatforms.GeminiCli).Surfaces, surface => surface == "commands");
        Assert.Contains(report.Targets.Single(target => target.Id == PromptPlatforms.GeminiCli).Surfaces, surface => surface == "hooks");
        Assert.Contains(report.Targets.Single(target => target.Id == PromptPlatforms.Copilot).Surfaces, surface => surface == "hooks");
        Assert.DoesNotContain("subagents", report.Targets.Single(target => target.Id == PromptPlatforms.GeminiCli).Surfaces);
        Assert.Contains(report.Targets.Single(target => target.Id == PromptPlatforms.Antigravity).Surfaces, surface => surface == "skills");
        Assert.DoesNotContain("mcp", report.Targets.Single(target => target.Id == PromptPlatforms.Antigravity).Surfaces);
        Assert.Equal(["rules", "hooks", "mcp"], report.Targets.Single(target => target.Id == PromptPlatforms.FactoryDroid).Surfaces);

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo.Root, ".config", "dotnet-tools.json")));
        Assert.True(manifest.RootElement.TryGetProperty("tools", out var tools));
        Assert.True(tools.TryGetProperty(ToolkitRuntimeMetadata.PackageId.ToLowerInvariant(), out var toolEntry));
        Assert.Equal(ToolkitRuntimeMetadata.ToolCommandName, toolEntry.GetProperty("commands")[0].GetString());

        using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo.Root, "rulesync.jsonc")));
        Assert.Equal(ToolkitRuntimeMetadata.RuleSyncSourceRepository, config.RootElement.GetProperty("sources")[0].GetProperty("source").GetString());
        Assert.Contains(PromptPlatforms.Antigravity, config.RootElement.GetProperty("targets").EnumerateArray().Select(item => item.GetString()));
        Assert.Contains(PromptPlatforms.FactoryDroid, config.RootElement.GetProperty("targets").EnumerateArray().Select(item => item.GetString()));
        var features = config.RootElement.GetProperty("features");
        Assert.Equal(JsonValueKind.Object, features.ValueKind);
        Assert.Equal(
            ["rules", "mcp", "commands", "subagents", "skills", "hooks"],
            features.GetProperty(PromptPlatforms.ClaudeCode).EnumerateArray().Select(item => item.GetString()!).ToArray());
        Assert.Equal(
            ["rules", "mcp", "subagents", "skills"],
            features.GetProperty(PromptPlatforms.CodexCli).EnumerateArray().Select(item => item.GetString()!).ToArray());
        Assert.Equal(
            ["rules", "commands", "skills"],
            features.GetProperty(PromptPlatforms.Antigravity).EnumerateArray().Select(item => item.GetString()!).ToArray());
        Assert.Equal(
            ["rules", "mcp", "hooks"],
            features.GetProperty(PromptPlatforms.FactoryDroid).EnumerateArray().Select(item => item.GetString()!).ToArray());
    }

    [Fact]
    public void Bootstrap_EnableDotNetIntelligencePack_AddsSlopwatchToolAndConfig()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var report = BootstrapEngine.Bootstrap(repo.Root, new BootstrapOptions
        {
            Profile = BootstrapProfileCatalog.Core,
            EnablePacks = [BootstrapPackCatalog.DotNetIntelligence]
        });

        Assert.True(report.Passed);
        Assert.Equal(BootstrapProfileCatalog.Core, report.Profile);
        Assert.DoesNotContain("hooks", report.Features);
        var pack = Assert.Single(report.Packs, item => item.Id == BootstrapPackCatalog.DotNetIntelligence);
        Assert.Contains("Slopwatch.Cmd", pack.ToolPackageIds);
        Assert.True(File.Exists(Path.Combine(repo.Root, ".slopwatch", "config.json")));

        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo.Root, ".config", "dotnet-tools.json")));
        Assert.True(manifest.RootElement.GetProperty("tools").TryGetProperty("slopwatch.cmd", out var slopwatchTool));
        Assert.Equal("0.4.0", slopwatchTool.GetProperty("version").GetString());
        Assert.False(slopwatchTool.GetProperty("rollForward").GetBoolean());

        using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo.Root, ".slopwatch", "config.json")));
        Assert.Equal(0, config.RootElement.GetProperty("globalSuppressions").GetArrayLength());

        using var rulesyncConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo.Root, "rulesync.jsonc")));
        Assert.Equal(
            ["rules", "commands", "skills"],
            rulesyncConfig.RootElement.GetProperty("features").GetProperty(PromptPlatforms.ClaudeCode).EnumerateArray().Select(item => item.GetString()!).ToArray());
        Assert.Equal(
            ["rules"],
            rulesyncConfig.RootElement.GetProperty("features").GetProperty(PromptPlatforms.FactoryDroid).EnumerateArray().Select(item => item.GetString()!).ToArray());
    }

    [Fact]
    public void Bootstrap_RejectsUnsupportedTargets()
    {
        using var repo = new TestRepositoryBuilder();

        var exception = Assert.Throws<System.ArgumentException>(() => BootstrapEngine.Bootstrap(repo.Root, new BootstrapOptions
        {
            Targets = ["cursor"]
        }));

        Assert.Contains("Unsupported prompt platform", exception.Message);
    }

    [Fact]
    public void Bootstrap_NoSave_SkipsImplicitRepoLocalPersistence()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var report = BootstrapEngine.Bootstrap(repo.Root, new BootstrapOptions
        {
            EnablePacks = [BootstrapPackCatalog.DotNetIntelligence],
            WriteToolManifest = false,
            WritePackFiles = false,
            WriteState = false
        });

        Assert.True(report.Passed);
        Assert.False(File.Exists(Path.Combine(repo.Root, ".config", "dotnet-tools.json")));
        Assert.False(File.Exists(Path.Combine(repo.Root, ".slopwatch", "config.json")));
        Assert.False(Directory.Exists(Path.Combine(repo.Root, ".dotnet-agent-harness")));
        Assert.True(File.Exists(Path.Combine(repo.Root, "rulesync.jsonc")));
        Assert.Equal("skipped", report.ToolManifest.Status);
        var pack = Assert.Single(report.Packs);
        var packFile = Assert.Single(pack.Files);
        Assert.Equal("skipped", packFile.Status);
    }

    [Fact]
    public void Bootstrap_Command_NoSave_SkipsImplicitRepoLocalPersistence()
    {
        using var repo = new TestRepositoryBuilder();
        repo.WriteFile("src/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);

            var exitCode = DotNetAgentHarness.Tools.Program.Main(
            [
                "bootstrap",
                "--repo", repo.Root,
                "--enable-pack", BootstrapPackCatalog.DotNetIntelligence,
                "--no-save",
                "--format", "json"
            ]);

            Assert.Equal(0, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        Assert.False(File.Exists(Path.Combine(repo.Root, ".config", "dotnet-tools.json")));
        Assert.False(File.Exists(Path.Combine(repo.Root, ".slopwatch", "config.json")));
        Assert.False(Directory.Exists(Path.Combine(repo.Root, ".dotnet-agent-harness")));
        Assert.True(File.Exists(Path.Combine(repo.Root, "rulesync.jsonc")));
        Assert.True(string.IsNullOrWhiteSpace(stderr.ToString()));
    }
}
