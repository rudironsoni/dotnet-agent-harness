using DotNetAgentHarness.Tools.Engine;
using Xunit;

namespace DotNetAgentHarness.Tools.Tests;

public class ToolkitCatalogLoaderTests
{
    [Fact]
    public void Load_IncludesPersonasInCatalogStats()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var catalog = ToolkitCatalogLoader.Load(repo.Root);

        Assert.Equal(4, catalog.Stats.Personas);
        Assert.Contains(catalog.Items, item => item.Kind == CatalogKinds.Persona && item.Id == "reviewer");
        Assert.Contains(catalog.Items, item => item.Kind == CatalogKinds.Persona && item.References.Contains("dotnet-code-review-agent"));
    }

    [Fact]
    public void SearchAndCompare_WorkForPersonaItems()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var catalog = ToolkitCatalogLoader.Load(repo.Root);
        var searchResults = ToolkitCatalogLoader.Search(catalog, new CatalogSearchQuery
        {
            Query = "review",
            Kind = CatalogKinds.Persona,
            Limit = 5
        });

        Assert.Contains(searchResults, result => result.Item.Id == "reviewer");

        var comparison = ToolkitCatalogLoader.Compare(catalog, "reviewer", "implementer");

        Assert.Equal(CatalogKinds.Persona, comparison.Left.Kind);
        Assert.Equal(CatalogKinds.Persona, comparison.Right.Kind);
        Assert.Contains("persona", comparison.SharedTags);
    }

    [Fact]
    public void Search_RespectsPlatformCapabilitySurfaces()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var catalog = ToolkitCatalogLoader.Load(repo.Root);
        var geminiSubagents = ToolkitCatalogLoader.Search(catalog, new CatalogSearchQuery
        {
            Query = "testing",
            Kind = CatalogKinds.Subagent,
            Platform = PromptPlatforms.GeminiCli,
            Limit = 5
        });
        var codexSubagents = ToolkitCatalogLoader.Search(catalog, new CatalogSearchQuery
        {
            Query = "testing",
            Kind = CatalogKinds.Subagent,
            Platform = PromptPlatforms.CodexCli,
            Limit = 5
        });

        Assert.Empty(geminiSubagents);
        Assert.Contains(codexSubagents, result => result.Item.Id == "dotnet-testing-specialist");
    }

    [Fact]
    public void Search_AllowsPersonasForFactoryDroidWhileFilteringUnsupportedKinds()
    {
        using var repo = new TestRepositoryBuilder();
        ToolkitTestContent.WritePromptToolkit(repo);

        var catalog = ToolkitCatalogLoader.Load(repo.Root);
        var personas = ToolkitCatalogLoader.Search(catalog, new CatalogSearchQuery
        {
            Query = "review",
            Kind = CatalogKinds.Persona,
            Platform = PromptPlatforms.FactoryDroid,
            Limit = 5
        });
        var skills = ToolkitCatalogLoader.Search(catalog, new CatalogSearchQuery
        {
            Query = "dotnet",
            Kind = CatalogKinds.Skill,
            Platform = PromptPlatforms.FactoryDroid,
            Limit = 5
        });

        Assert.Contains(personas, result => result.Item.Id == "reviewer");
        Assert.Empty(skills);
    }
}
