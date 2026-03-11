namespace DotnetAgentHarness.Cli.Services;

using DotnetAgentHarness.Cli.Models;

/// <summary>
/// Interface for running code analysis using Roslyn analyzers, StyleCop, and Sonar.
/// </summary>
public interface ICodeAnalyzer
{
    /// <summary>
    /// Runs code analysis on the specified project or solution.
    /// </summary>
    /// <param name="options">Analysis options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Analysis results.</returns>
    Task<AnalysisResult> AnalyzeAsync(AnalysisOptions options, CancellationToken ct = default);

    /// <summary>
    /// Checks if a project has analyzers configured.
    /// </summary>
    /// <param name="projectPath">Path to the project file.</param>
    /// <returns>True if analyzers are configured.</returns>
    Task<bool> HasAnalyzersConfiguredAsync(string projectPath);
}
