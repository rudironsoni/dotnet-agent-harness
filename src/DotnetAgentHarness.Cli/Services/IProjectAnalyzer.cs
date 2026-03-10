namespace DotnetAgentHarness.Cli.Services;

using DotnetAgentHarness.Cli.Models;

/// <summary>
/// Interface for analyzing .NET projects to extract metadata and dependencies.
/// </summary>
public interface IProjectAnalyzer
{
    /// <summary>
    /// Analyzes a project at the given path.
    /// </summary>
    Task<ProjectProfile?> AnalyzeProjectAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Finds all .NET projects in a directory.
    /// </summary>
    Task<IReadOnlyList<string>> FindProjectsAsync(string basePath, CancellationToken ct = default);

    /// <summary>
    /// Finds solution files in a directory.
    /// </summary>
    Task<IReadOnlyList<string>> FindSolutionsAsync(string basePath, CancellationToken ct = default);
}
