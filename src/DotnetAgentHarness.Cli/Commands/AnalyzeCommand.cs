namespace DotnetAgentHarness.Cli.Commands;

using System.CommandLine;
using System.Text.Json;
using DotnetAgentHarness.Cli.Models;
using DotnetAgentHarness.Cli.Services;
using Spectre.Console;

/// <summary>
/// Command to run comprehensive code analysis using Roslyn analyzers, StyleCop, and Sonar rules.
/// </summary>
public class AnalyzeCommand : Command
{
    private readonly ICodeAnalyzer codeAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzeCommand"/> class.
    /// </summary>
    public AnalyzeCommand(ICodeAnalyzer codeAnalyzer)
        : base("analyze", "Run comprehensive code analysis (Roslyn analyzers, StyleCop, Sonar)")
    {
        this.codeAnalyzer = codeAnalyzer;

        // Path argument - supports .csproj, .sln, or directory
        Argument<string> pathArgument = new(
            "path",
            () => ".",
            "Path to project, solution, or directory to analyze");

        // Severity filter option
        Option<string> severityOption = new(
            new[] { "--severity", "-s" },
            () => "info",
            "Minimum severity to report (error, warning, info)");

        // Output format option
        Option<string> formatOption = new(
            new[] { "--format", "-f" },
            () => "text",
            "Output format (text, json, sarif)");

        // Output file option
        Option<string?> outputFileOption = new(
            new[] { "--output", "-o" },
            "Output file path (default: console)");

        // Treat warnings as errors option
        Option<bool> warningsAsErrorsOption = new(
            new[] { "--warnings-as-errors", "-w" },
            () => false,
            "Treat warnings as errors (non-zero exit code)");

        // StyleCop option
        Option<bool> styleCopOption = new(
            new[] { "--stylecop", "-sc" },
            () => true,
            "Enable StyleCop analysis");

        // Sonar option
        Option<bool> sonarOption = new(
            new[] { "--sonar", "-so" },
            () => false,
            "Enable Sonar analysis");

        // Verbose option
        Option<bool> verboseOption = new(
            new[] { "--verbose", "-v" },
            () => false,
            "Show detailed output");

        this.AddArgument(pathArgument);
        this.AddOption(severityOption);
        this.AddOption(formatOption);
        this.AddOption(outputFileOption);
        this.AddOption(warningsAsErrorsOption);
        this.AddOption(styleCopOption);
        this.AddOption(sonarOption);
        this.AddOption(verboseOption);

        this.SetHandler(async (string path, string severity, string format, string? outputFile, bool warningsAsErrors, bool styleCop, bool sonar, bool verbose) =>
        {
            await this.ExecuteAsync(path, severity, format, outputFile, warningsAsErrors, styleCop, sonar, verbose);
        }, pathArgument, severityOption, formatOption, outputFileOption, warningsAsErrorsOption, styleCopOption, sonarOption, verboseOption);
    }

    private async Task<int> ExecuteAsync(
        string path,
        string severity,
        string format,
        string? outputFile,
        bool warningsAsErrors,
        bool styleCop,
        bool sonar,
        bool verbose)
    {
        try
        {
            // Parse options
            AnalysisSeverity minSeverity = ParseSeverity(severity);
            AnalysisOutputFormat outputFormat = ParseFormat(format);

            var options = new AnalysisOptions
            {
                ProjectPath = path,
                MinimumSeverity = minSeverity,
                OutputFormat = outputFormat,
                OutputFile = outputFile,
                TreatWarningsAsErrors = warningsAsErrors,
                RunStyleCop = styleCop,
                RunSonar = sonar,
                Verbose = verbose,
            };

            if (verbose)
            {
                await Console.Out.WriteLineAsync("Running code analysis...");
                await Console.Out.WriteLineAsync($"  Path: {Path.GetFullPath(path)}");
                await Console.Out.WriteLineAsync($"  Severity: {severity}");
                await Console.Out.WriteLineAsync($"  Format: {format}");
                if (!string.IsNullOrEmpty(outputFile))
                {
                    await Console.Out.WriteLineAsync($"  Output: {outputFile}");
                }

                await Console.Out.WriteLineAsync();
            }

            // Run analysis
            AnalysisResult result = await this.codeAnalyzer.AnalyzeAsync(options);

            // Output results
            await this.OutputResultsAsync(result, options);

            // Determine exit code
            if (!result.Success)
            {
                return 2; // Analysis failed
            }

            if (result.ErrorCount > 0)
            {
                return 1; // Errors found
            }

            if (warningsAsErrors && result.WarningCount > 0)
            {
                return 1; // Warnings treated as errors
            }

            return 0; // Success
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");
            if (verbose)
            {
                await Console.Error.WriteLineAsync(ex.StackTrace);
            }

            return 2;
        }
    }

    private async Task OutputResultsAsync(AnalysisResult result, AnalysisOptions options)
    {
        string output = options.OutputFormat switch
        {
            AnalysisOutputFormat.Json => FormatAsJson(result),
            AnalysisOutputFormat.Sarif => FormatAsSarif(result),
            _ => FormatAsText(result),
        };

        if (!string.IsNullOrEmpty(options.OutputFile))
        {
            await File.WriteAllTextAsync(options.OutputFile, output);
            await Console.Out.WriteLineAsync($"Results written to: {options.OutputFile}");
        }
        else
        {
            await Console.Out.WriteLineAsync(output);
        }
    }

    private static string FormatAsText(AnalysisResult result)
    {
        if (!result.Success)
        {
            return $"Analysis failed: {result.ErrorMessage}";
        }

        var lines = new List<string>();

        // Header
        lines.Add("Code Analysis Results");
        lines.Add(new string('=', 50));
        lines.Add($"Duration: {result.Duration.TotalSeconds:F2}s");
        lines.Add($"Total Issues: {result.TotalIssues}");
        lines.Add("");

        // Summary by analyzer
        lines.Add("Summary by Analyzer:");
        lines.Add(new string('-', 50));

        foreach (var kvp in result.IssuesByAnalyzer.OrderBy(kvp => kvp.Key))
        {
            int errors = kvp.Value.Count(i => i.Severity == AnalysisSeverity.Error);
            int warnings = kvp.Value.Count(i => i.Severity == AnalysisSeverity.Warning);
            int infos = kvp.Value.Count(i => i.Severity == AnalysisSeverity.Info);

            var parts = new List<string>();
            if (errors > 0) parts.Add($"{errors} error(s)");
            if (warnings > 0) parts.Add($"{warnings} warning(s)");
            if (infos > 0) parts.Add($"{infos} info");

            lines.Add($"  {kvp.Key}: {string.Join(", ", parts)}");
        }

        lines.Add("");

        // Issues detail
        if (result.Issues.Count > 0)
        {
            lines.Add("Issues:");
            lines.Add(new string('-', 50));

            foreach (AnalysisIssue issue in result.Issues.OrderBy(i => i.Severity).ThenBy(i => i.FilePath).ThenBy(i => i.LineNumber))
            {
                string severityIcon = issue.Severity switch
                {
                    AnalysisSeverity.Error => "[red]✗[/]",
                    AnalysisSeverity.Warning => "[yellow]⚠[/]",
                    _ => "[blue]ℹ[/]",
                };

                string fileName = Path.GetFileName(issue.FilePath);
                string location = issue.LineNumber > 0
                    ? $"{fileName}:{issue.LineNumber}"
                    : fileName;

                lines.Add($"{severityIcon} [{issue.RuleId}] {issue.Message}");
                lines.Add($"   at {location}");

                if (!string.IsNullOrEmpty(issue.HelpUrl))
                {
                    lines.Add($"   Learn more: {issue.HelpUrl}");
                }

                lines.Add("");
            }
        }
        else
        {
            lines.Add("✓ All checks passed - no issues found!");
        }

        return string.Join("\n", lines);
    }

    private static string FormatAsJson(AnalysisResult result)
    {
        var json = new
        {
            success = result.Success,
            durationMs = result.Duration.TotalMilliseconds,
            summary = new
            {
                total = result.TotalIssues,
                errors = result.ErrorCount,
                warnings = result.WarningCount,
                info = result.InfoCount,
            },
            byAnalyzer = result.IssuesByAnalyzer.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    count = kvp.Value.Count,
                    errors = kvp.Value.Count(i => i.Severity == AnalysisSeverity.Error),
                    warnings = kvp.Value.Count(i => i.Severity == AnalysisSeverity.Warning),
                    info = kvp.Value.Count(i => i.Severity == AnalysisSeverity.Info),
                }),
            issues = result.Issues.Select(i => new
            {
                analyzer = i.Analyzer,
                ruleId = i.RuleId,
                severity = i.Severity.ToString().ToLowerInvariant(),
                message = i.Message,
                filePath = i.FilePath,
                lineNumber = i.LineNumber,
                columnNumber = i.ColumnNumber,
                helpUrl = i.HelpUrl,
            }),
            errorMessage = result.ErrorMessage,
        };

        return JsonSerializer.Serialize(json, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }

    private static string FormatAsSarif(AnalysisResult result)
    {
        // Generate SARIF 2.1.0 compatible output
        var sarif = new Dictionary<string, object>
        {
            ["$schema"] = "https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0.json",
            ["version"] = "2.1.0",
            ["runs"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["tool"] = new Dictionary<string, object>
                    {
                        ["driver"] = new Dictionary<string, object>
                        {
                            ["name"] = "dotnet-agent-harness",
                            ["version"] = "1.0.0",
                            ["informationUri"] = "https://github.com/rudironsoni/dotnet-agent-harness",
                        },
                    },
                    ["results"] = result.Issues.Select(i => new Dictionary<string, object>
                    {
                        ["ruleId"] = i.RuleId,
                        ["level"] = i.Severity switch
                        {
                            AnalysisSeverity.Error => "error",
                            AnalysisSeverity.Warning => "warning",
                            _ => "note",
                        },
                        ["message"] = new Dictionary<string, object>
                        {
                            ["text"] = i.Message,
                        },
                        ["locations"] = new[]
                        {
                            new Dictionary<string, object>
                            {
                                ["physicalLocation"] = new Dictionary<string, object>
                                {
                                    ["artifactLocation"] = new Dictionary<string, object>
                                    {
                                        ["uri"] = i.FilePath,
                                    },
                                    ["region"] = new Dictionary<string, object>
                                    {
                                        ["startLine"] = i.LineNumber,
                                        ["startColumn"] = i.ColumnNumber,
                                    },
                                },
                            },
                        },
                    }).ToList(),
                },
            },
        };

        return JsonSerializer.Serialize(sarif, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }

    private static AnalysisSeverity ParseSeverity(string severity)
    {
        return severity.ToLowerInvariant() switch
        {
            "error" => AnalysisSeverity.Error,
            "warning" or "warn" => AnalysisSeverity.Warning,
            _ => AnalysisSeverity.Info,
        };
    }

    private static AnalysisOutputFormat ParseFormat(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "json" => AnalysisOutputFormat.Json,
            "sarif" => AnalysisOutputFormat.Sarif,
            _ => AnalysisOutputFormat.Text,
        };
    }
}
