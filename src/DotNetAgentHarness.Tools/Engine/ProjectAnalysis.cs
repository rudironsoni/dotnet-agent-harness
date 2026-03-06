using System.Collections.Generic;
using System.Linq;

namespace DotNetAgentHarness.Tools.Engine;

public sealed class RepositoryProfile
{
    public string RepoRoot { get; init; } = string.Empty;
    public List<string> Solutions { get; init; } = new();
    public List<ProjectSummary> Projects { get; init; } = new();
    public string? GlobalJsonSdkVersion { get; init; }
    public List<string> InstalledSdkVersions { get; init; } = new();
    public bool HasDirectoryBuildProps { get; init; }
    public bool HasEditorConfig { get; init; }
    public bool HasDotNetToolManifest { get; init; }
    public bool UsesDotNetLocalTools { get; init; }
    public bool HasRulesync { get; init; }
    public List<string> CiProviders { get; init; } = new();
    public List<string> TargetFrameworks { get; init; } = new();
    public List<string> Technologies { get; init; } = new();
    public List<string> ProjectKinds { get; init; } = new();
    public string DominantProjectKind { get; init; } = "unknown";
    public string DominantTestFramework { get; init; } = "none";
    public int TestProjectCount { get; init; }
    public List<string> PackageIds { get; init; } = new();
    public List<string> LoadErrors { get; init; } = new();
}

public sealed class ProjectSummary
{
    public string Name { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sdk { get; init; } = string.Empty;
    public List<string> TargetFrameworks { get; init; } = new();
    public string OutputType { get; init; } = string.Empty;
    public bool UseMaui { get; init; }
    public bool IsTestProject { get; init; }
    public string Kind { get; init; } = string.Empty;
    public List<string> PackageIds { get; init; } = new();
}

public sealed class RecommendationBundle
{
    public RepositoryProfile Profile { get; init; } = new();
    public string RequestedPlatform { get; init; } = PromptPlatforms.Generic;
    public string RequestedCategory { get; init; } = string.Empty;
    public List<string> PlatformSurfaces { get; init; } = new();
    public List<RecommendationItem> Skills { get; init; } = new();
    public List<RecommendationItem> Subagents { get; init; } = new();
    public List<RecommendationItem> Commands { get; init; } = new();
}

public sealed class RecommendationItem
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public int Score { get; init; }
    public List<string> Reasons { get; init; } = new();
    public string FilePath { get; init; } = string.Empty;
}

public sealed class RecommendationQuery
{
    public int LimitPerKind { get; init; } = 5;
    public string? Platform { get; init; }
    public string? Category { get; init; }
}

public sealed class MetadataQuery
{
    public string Mode { get; init; } = "packages";
    public string? TargetPath { get; init; }
    public string? AssemblyPath { get; init; }
    public string? NamespaceFilter { get; init; }
    public string? TypeName { get; init; }
    public string? Query { get; init; }
    public string Configuration { get; init; } = "Debug";
    public string? Framework { get; init; }
    public bool BuildIfNeeded { get; init; }
    public int Limit { get; init; } = 25;
}

public sealed class MetadataReport
{
    public string RepoRoot { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string ResolvedTarget { get; init; } = string.Empty;
    public string AssemblyPath { get; init; } = string.Empty;
    public List<ProjectPackageReport> Projects { get; init; } = new();
    public List<PackageUsageReport> PackageUsages { get; init; } = new();
    public List<string> Namespaces { get; init; } = new();
    public List<TypeSummaryReport> Types { get; init; } = new();
    public TypeDetailReport? Type { get; init; }
    public List<string> Warnings { get; init; } = new();
}

public sealed class ProjectPackageReport
{
    public string ProjectName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public List<PackageReferenceReport> Packages { get; init; } = new();
}

public sealed class PackageReferenceReport
{
    public string Id { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public bool IsFloatingVersion { get; init; }
    public bool HasVersionOverride { get; init; }
}

public sealed class PackageUsageReport
{
    public string Id { get; init; } = string.Empty;
    public List<string> Versions { get; init; } = new();
    public List<string> Projects { get; init; } = new();
    public int ProjectCount { get; init; }
    public bool HasVersionDrift { get; init; }
}

public sealed class TypeSummaryReport
{
    public string Name { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string BaseType { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public bool IsAbstract { get; init; }
    public bool IsInterface { get; init; }
    public int MethodCount { get; init; }
    public int PropertyCount { get; init; }
}

public sealed class TypeDetailReport
{
    public string Name { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string BaseType { get; init; } = string.Empty;
    public List<string> Interfaces { get; init; } = new();
    public List<string> Constructors { get; init; } = new();
    public List<string> Methods { get; init; } = new();
    public List<string> Properties { get; init; } = new();
    public List<string> Fields { get; init; } = new();
    public bool IsPublic { get; init; }
    public bool IsAbstract { get; init; }
    public bool IsInterface { get; init; }
}

public sealed class DoctorReport
{
    public string RepoRoot { get; init; } = string.Empty;
    public List<DoctorFinding> Findings { get; init; } = new();
    public bool Passed => Findings.All(finding => !finding.Severity.Equals("error", System.StringComparison.OrdinalIgnoreCase));
}

public sealed class DoctorFinding
{
    public string Severity { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Remediation { get; init; } = string.Empty;
}

public sealed class ValidationReport
{
    public string Mode { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public List<ValidationCheck> Checks { get; init; } = new();
    public bool Passed => Checks.All(check => check.Passed);
}

public sealed class ValidationCheck
{
    public string Name { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
    public string Command { get; init; } = string.Empty;
    public int? ExitCode { get; init; }
    public double? DurationMs { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public string Remediation { get; init; } = string.Empty;
}

public sealed class InitReport
{
    public RepositoryProfile Profile { get; init; } = new();
    public RecommendationBundle Recommendations { get; init; } = new();
    public DoctorReport Doctor { get; init; } = new();
}

public sealed class DotNetEnvironmentReport
{
    public bool IsAvailable { get; init; }
    public List<string> InstalledSdkVersions { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

public sealed class ValidationOptions
{
    public bool RunDotNet { get; init; }
    public bool SkipRestore { get; init; }
    public bool SkipBuild { get; init; }
    public bool SkipTest { get; init; }
    public string Configuration { get; init; } = "Debug";
    public string? Framework { get; init; }
    public string? TargetPath { get; init; }
    public int TimeoutMs { get; init; } = 120_000;
}
