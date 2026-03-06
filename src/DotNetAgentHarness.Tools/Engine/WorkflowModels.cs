using System.Collections.Generic;

namespace DotNetAgentHarness.Tools.Engine;

public sealed class ReviewReport
{
    public string RepoRoot { get; init; } = string.Empty;
    public int ScannedFiles { get; init; }
    public List<ReviewFinding> Findings { get; init; } = new();
}

public sealed class ReviewFinding
{
    public string Severity { get; init; } = string.Empty;
    public string RuleId { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public int LineNumber { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Guidance { get; init; } = string.Empty;
    public string Evidence { get; init; } = string.Empty;
}

public sealed class SkillTestSuiteResult
{
    public List<SkillTestResult> Skills { get; init; } = new();
    public bool Passed { get; init; }
    public int TotalCases { get; init; }
    public int SkillsWithCases { get; init; }
    public int SkillsWithoutCases { get; init; }
    public int TotalChecks { get; init; }
    public int FailedChecks { get; init; }
}

public sealed class EvalHarnessOptions
{
    public string? Platform { get; init; }
    public int TrialCount { get; init; } = 3;
    public bool UseDummyMode { get; init; } = true;
    public bool UnloadedCheckOnly { get; init; }
    public string? CaseFilePath { get; init; }
    public string? ArtifactId { get; init; }
    public string? ArtifactPath { get; init; }
    public string? Provider { get; init; }
    public string? Model { get; init; }
    public int TimeoutMs { get; init; } = 300_000;
}

public sealed class EvalHarnessRunResult
{
    public string Command { get; init; } = string.Empty;
    public int ExitCode { get; init; }
    public bool TimedOut { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public string Platform { get; init; } = string.Empty;
    public int TrialCount { get; init; }
    public bool UseDummyMode { get; init; }
    public bool UnloadedCheckOnly { get; init; }
    public string ArtifactPath { get; init; } = string.Empty;
    public EvalHarnessArtifactSummary? Artifact { get; init; }
    public bool Passed => !TimedOut && ExitCode == 0 && (Artifact?.Overall.FailedTrials ?? 0) == 0;
}

public sealed class EvalHarnessArtifactSummary
{
    public string RunId { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? PlatformFilter { get; init; }
    public string CaseFilePath { get; init; } = string.Empty;
    public int DefaultTrialCount { get; init; }
    public string Gate { get; init; } = string.Empty;
    public string PolicyProfile { get; init; } = string.Empty;
    public string? PromptEvidenceId { get; init; }
    public EvalHarnessOverallSummary Overall { get; init; } = new();
    public List<EvalHarnessCaseSummary> Cases { get; init; } = new();
}

public sealed class EvalHarnessOverallSummary
{
    public int CaseCount { get; init; }
    public int TrialCount { get; init; }
    public int PassedTrials { get; init; }
    public int FailedTrials { get; init; }
    public double PassRate { get; init; }
}

public sealed class EvalHarnessCaseSummary
{
    public string CaseId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CaseType { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public string ExpectedTrigger { get; init; } = string.Empty;
    public string UnloadedExpectedTrigger { get; init; } = string.Empty;
    public string SelectedPlatform { get; init; } = string.Empty;
    public List<string> Platforms { get; init; } = new();
    public int TrialCount { get; init; }
    public int PassedTrials { get; init; }
    public int FailedTrials { get; init; }
    public double PassRate { get; init; }
    public double AverageElapsedMilliseconds { get; init; }
    public List<EvalHarnessFailureSummary> Failures { get; init; } = new();
}

public sealed class EvalHarnessFailureSummary
{
    public string Scenario { get; init; } = string.Empty;
    public int TrialNumber { get; init; }
    public string TriggerMessage { get; init; } = string.Empty;
    public List<string> AssertionMessages { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
}

public sealed class McpExportOptions
{
    public string OutputDirectory { get; init; } = string.Empty;
    public string? Platform { get; init; }
    public string Kind { get; init; } = "all";
}

public sealed class McpExportReport
{
    public string RepoRoot { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
    public string Platform { get; init; } = string.Empty;
    public string Kind { get; init; } = "all";
    public string ManifestPath { get; init; } = string.Empty;
    public string PromptIndexPath { get; init; } = string.Empty;
    public string ResourceIndexPath { get; init; } = string.Empty;
    public int PromptCount { get; init; }
    public int ResourceCount { get; init; }
    public List<McpPromptExportItem> Prompts { get; init; } = new();
    public List<McpResourceExportItem> Resources { get; init; } = new();
}

public sealed class McpPromptExportItem
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Uri { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string ExportPath { get; init; } = string.Empty;
    public List<string> Platforms { get; init; } = new();
    public List<string> Tags { get; init; } = new();
}

public sealed class McpResourceExportItem
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Uri { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string ExportPath { get; init; } = string.Empty;
    public List<string> Platforms { get; init; } = new();
    public List<string> Tags { get; init; } = new();
    public List<string> References { get; init; } = new();
}

public sealed class SkillTestResult
{
    public string SkillId { get; init; } = string.Empty;
    public string SkillPath { get; init; } = string.Empty;
    public int CaseCount { get; init; }
    public List<SkillTestCheck> Checks { get; init; } = new();
    public bool Passed { get; init; }
}

public sealed class SkillTestCheck
{
    public string Name { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Message { get; init; } = string.Empty;
    public string SourceFile { get; init; } = string.Empty;
    public string CaseName { get; init; } = string.Empty;
    public string TestName { get; init; } = string.Empty;
}

public sealed class SkillTestCaseDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public SkillTestScope Scope { get; set; } = new();
    public List<SkillTestCommandDefinition> SetupCommands { get; set; } = new();
    public List<SkillTestDefinition> Tests { get; set; } = new();
    public List<SkillTestCommandDefinition> TeardownCommands { get; set; } = new();
}

public sealed class SkillTestScope
{
    public List<string> IncludeSkills { get; set; } = new();
    public List<string> ExcludeSkills { get; set; } = new();
    public List<string> IncludeTags { get; set; } = new();
    public List<string> ExcludeTags { get; set; } = new();
}

public sealed class SkillTestCommandDefinition
{
    public string Command { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 60_000;
    public bool ContinueOnError { get; set; }
}

public sealed class SkillTestDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 60_000;
    public SkillTestExpectation Expected { get; set; } = new();
}

public sealed class SkillTestExpectation
{
    public string Status { get; set; } = string.Empty;
    public List<string> OutputContains { get; set; } = new();
    public List<string> OutputMatches { get; set; } = new();
    public List<string> ErrorContains { get; set; } = new();
    public List<string> FileExists { get; set; } = new();
    public List<string> FileNotExists { get; set; } = new();
    public List<string> SkillContains { get; set; } = new();
    public List<string> SkillMatches { get; set; } = new();
    public Dictionary<string, string> Frontmatter { get; set; } = new();
    public bool NoErrors { get; set; }
}

public sealed class ProcessExecutionResult
{
    public string Command { get; init; } = string.Empty;
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public bool TimedOut { get; init; }
}

public sealed class ScaffoldTemplate
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? DotNetTemplate { get; init; }
}

public sealed class ScaffoldPlan
{
    public string TemplateId { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public string SolutionName { get; init; } = string.Empty;
    public List<ScaffoldStep> Steps { get; init; } = new();
}

public sealed class ScaffoldStep
{
    public string Kind { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Command { get; init; } = string.Empty;
}
