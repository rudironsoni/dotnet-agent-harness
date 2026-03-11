namespace DotnetAgentHarness.Cli.Models;

/// <summary>
/// Represents the severity level of an analysis issue.
/// </summary>
public enum AnalysisSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning level issue.
    /// </summary>
    Warning,

    /// <summary>
    /// Error level issue.
    /// </summary>
    Error,
}

/// <summary>
/// Represents a single analysis issue found by code analyzers.
/// </summary>
public sealed class AnalysisIssue
{
    /// <summary>
    /// The analyzer that reported this issue (e.g., "Roslyn", "StyleCop", "Sonar").
    /// </summary>
    public required string Analyzer { get; set; }

    /// <summary>
    /// The rule ID (e.g., "CS0168", "SA1200", "S125").
    /// </summary>
    public required string RuleId { get; set; }

    /// <summary>
    /// Human-readable description of the issue.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Path to the file containing the issue.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Line number where the issue occurs (1-based).
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Column number where the issue occurs (1-based).
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// Severity level of the issue.
    /// </summary>
    public AnalysisSeverity Severity { get; set; }

    /// <summary>
    /// Optional: URL to documentation for this rule.
    /// </summary>
    public string? HelpUrl { get; set; }
}

/// <summary>
/// Represents the output format for analysis results.
/// </summary>
public enum AnalysisOutputFormat
{
    /// <summary>
    /// Human-readable text format.
    /// </summary>
    Text,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// SARIF (Static Analysis Results Interchange Format) for CI integration.
    /// </summary>
    Sarif,
}

/// <summary>
/// Represents the complete result of a code analysis operation.
/// </summary>
public sealed class AnalysisResult
{
    /// <summary>
    /// Whether the analysis completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Total number of issues found.
    /// </summary>
    public int TotalIssues { get; set; }

    /// <summary>
    /// Number of error-level issues.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Number of warning-level issues.
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// Number of info-level issues.
    /// </summary>
    public int InfoCount { get; set; }

    /// <summary>
    /// Issues grouped by analyzer.
    /// </summary>
    public Dictionary<string, List<AnalysisIssue>> IssuesByAnalyzer { get; set; } = new();

    /// <summary>
    /// All issues found during analysis.
    /// </summary>
    public List<AnalysisIssue> Issues { get; set; } = new();

    /// <summary>
    /// Duration of the analysis operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Error message if analysis failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets a summary of the analysis results.
    /// </summary>
    public string GetSummary()
    {
        if (!this.Success)
        {
            return $"Analysis failed: {this.ErrorMessage}";
        }

        var parts = new List<string>();
        if (this.ErrorCount > 0)
        {
            parts.Add($"{this.ErrorCount} error(s)");
        }

        if (this.WarningCount > 0)
        {
            parts.Add($"{this.WarningCount} warning(s)");
        }

        if (this.InfoCount > 0)
        {
            parts.Add($"{this.InfoCount} info");
        }

        return parts.Count == 0
            ? "All checks passed"
            : string.Join(", ", parts);
    }
}

/// <summary>
/// Options for configuring code analysis.
/// </summary>
public sealed class AnalysisOptions
{
    /// <summary>
    /// Path to the project or solution to analyze.
    /// </summary>
    public string ProjectPath { get; set; } = ".";

    /// <summary>
    /// Minimum severity to report.
    /// </summary>
    public AnalysisSeverity MinimumSeverity { get; set; } = AnalysisSeverity.Info;

    /// <summary>
    /// Output format for results.
    /// </summary>
    public AnalysisOutputFormat OutputFormat { get; set; } = AnalysisOutputFormat.Text;

    /// <summary>
    /// Optional: Output file path. If not specified, outputs to console.
    /// </summary>
    public string? OutputFile { get; set; }

    /// <summary>
    /// Whether to treat warnings as errors (affects exit code).
    /// </summary>
    public bool TreatWarningsAsErrors { get; set; }

    /// <summary>
    /// Whether to run StyleCop analysis.
    /// </summary>
    public bool RunStyleCop { get; set; } = true;

    /// <summary>
    /// Whether to run Sonar analysis.
    /// </summary>
    public bool RunSonar { get; set; }

    /// <summary>
    /// Whether to show verbose output.
    /// </summary>
    public bool Verbose { get; set; }
}
