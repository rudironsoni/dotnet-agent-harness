namespace DotnetAgentHarness.Cli.Models;

/// <summary>
/// Represents analysis results of a .NET project.
/// </summary>
public sealed class ProjectProfile
{
    /// <summary>
    /// The project file path.
    /// </summary>
    public string? ProjectPath { get; init; }

    /// <summary>
    /// The solution file path (if found).
    /// </summary>
    public string? SolutionPath { get; init; }

    /// <summary>
    /// The target framework(s) detected.
    /// </summary>
    public IReadOnlyList<string> TargetFrameworks { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The project type (Web, Test, ClassLib, Console, etc.).
    /// </summary>
    public string? ProjectType { get; init; }

    /// <summary>
    /// NuGet packages referenced.
    /// </summary>
    public IReadOnlyList<PackageReference> Packages { get; init; } = Array.Empty<PackageReference>();

    /// <summary>
    /// Project references.
    /// </summary>
    public IReadOnlyList<ProjectReference> Projects { get; init; } = Array.Empty<ProjectReference>();

    /// <summary>
    /// CI/CD configuration detected.
    /// </summary>
    public IReadOnlyList<CiConfig> CiConfigs { get; init; } = Array.Empty<CiConfig>();

    /// <summary>
    /// Test frameworks detected.
    /// </summary>
    public IReadOnlyList<string> TestFrameworks { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Whether this is a test project.
    /// </summary>
    public bool IsTestProject { get; init; }

    /// <summary>
    /// Whether this is a web project.
    /// </summary>
    public bool IsWebProject { get; init; }

    /// <summary>
    /// Whether Entity Framework is used.
    /// </summary>
    public bool HasEntityFramework { get; init; }

    /// <summary>
    /// Whether Aspire is used.
    /// </summary>
    public bool HasAspire { get; init; }

    /// <summary>
    /// Whether Docker is used.
    /// </summary>
    public bool HasDocker { get; init; }
}

/// <summary>
/// Represents a NuGet package reference.
/// </summary>
public sealed class PackageReference
{
    /// <summary>
    /// The package name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The package version.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Whether this is a private asset.
    /// </summary>
    public bool IsPrivateAsset { get; init; }
}

/// <summary>
/// Represents a project reference.
/// </summary>
public sealed class ProjectReference
{
    /// <summary>
    /// The referenced project path.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// The referenced project name.
    /// </summary>
    public string? Name { get; init; }
}

/// <summary>
/// Represents CI/CD configuration.
/// </summary>
public sealed class CiConfig
{
    /// <summary>
    /// The CI/CD platform (GitHub Actions, Azure DevOps, etc.).
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// The configuration file path.
    /// </summary>
    public string? ConfigPath { get; init; }
}
