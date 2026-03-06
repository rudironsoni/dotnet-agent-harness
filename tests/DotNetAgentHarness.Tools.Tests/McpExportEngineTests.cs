using System;
using System.IO;
using System.Linq;
using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class McpExportEngineTests
{
    [Fact]
    public void Export_WritesManifestPromptsResourcesAndSpecialResources()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);
        repo.WriteFile(".rulesync/rules/overview.md", "# overview");
        repo.WriteFile(".rulesync/mcp.json", """
            {
              "mcpServers": {
                "github": {
                  "type": "http",
                  "url": "https://api.githubcopilot.com/mcp/"
                }
              }
            }
            """);

        var report = McpExportEngine.Export(repo.Root, new McpExportOptions
        {
            Platform = PromptPlatforms.GeminiCli,
            OutputDirectory = ".dotnet-agent-harness/exports/mcp"
        });

        Assert.True(File.Exists(report.ManifestPath));
        Assert.True(File.Exists(report.PromptIndexPath));
        Assert.True(File.Exists(report.ResourceIndexPath));
        Assert.NotEmpty(report.Prompts);
        Assert.NotEmpty(report.Resources);
        Assert.DoesNotContain(report.Resources, item => item.Kind == CatalogKinds.Subagent);
        Assert.Contains(report.Resources, item => item.Kind == "rule");
        Assert.Contains(report.Resources, item => item.Kind == "mcp");
        Assert.Contains(report.Prompts, item => item.Id == "architect" && item.Kind == CatalogKinds.Persona);
        Assert.Contains(report.Prompts, item => item.Id == "dotnet-agent-harness-bootstrap" && item.Kind == CatalogKinds.Command);
        Assert.Contains(report.Resources, item => item.Id == "dotnet-advisor" && item.Kind == CatalogKinds.Skill);
    }

    [Fact]
    public void Export_CommandKindOnly_FiltersToCommandPromptsAndResources()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var report = McpExportEngine.Export(repo.Root, new McpExportOptions
        {
            Kind = CatalogKinds.Command,
            OutputDirectory = ".dotnet-agent-harness/exports/commands-only"
        });

        Assert.NotEmpty(report.Prompts);
        Assert.NotEmpty(report.Resources);
        Assert.All(report.Prompts, item => Assert.Equal(CatalogKinds.Command, item.Kind));
        Assert.All(report.Resources, item => Assert.Equal(CatalogKinds.Command, item.Kind));
        Assert.DoesNotContain(report.Resources, item => item.Kind == CatalogKinds.Skill);
        Assert.True(report.Resources.All(item => item.ExportPath.Contains("commands", System.StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Export_RejectsUnsupportedKind()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var exception = Assert.Throws<System.ArgumentException>(() => McpExportEngine.Export(repo.Root, new McpExportOptions
        {
            Kind = "workflow"
        }));

        Assert.Contains("Unsupported MCP export kind", exception.Message);
    }

    [Fact]
    public void Export_Command_AllowsOutputDirectoryAndJsonReportTogether()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var reportPath = Path.Combine(repo.Root, ".dotnet-agent-harness", "exports", "mcp-report.json");
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
                "export-mcp",
                "--repo", repo.Root,
                "--platform", PromptPlatforms.GeminiCli,
                "--output", ".dotnet-agent-harness/exports/mcp",
                "--report-output", reportPath,
                "--format", "json"
            ]);

            Assert.Equal(0, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        var exportDirectory = Path.Combine(repo.Root, ".dotnet-agent-harness", "exports", "mcp");
        Assert.True(Directory.Exists(exportDirectory));
        Assert.True(File.Exists(Path.Combine(exportDirectory, "manifest.json")));
        Assert.True(File.Exists(Path.Combine(exportDirectory, "prompts", "index.json")));
        Assert.True(File.Exists(Path.Combine(exportDirectory, "resources", "index.json")));
        Assert.True(File.Exists(reportPath));
        Assert.Contains("\"PromptCount\"", File.ReadAllText(reportPath));
        Assert.True(string.IsNullOrWhiteSpace(stderr.ToString()));
    }
}
