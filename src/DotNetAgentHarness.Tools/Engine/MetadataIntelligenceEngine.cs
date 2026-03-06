using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace DotNetAgentHarness.Tools.Engine;

public static class MetadataIntelligenceEngine
{
    public static MetadataReport Inspect(string repoRoot, MetadataQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var normalizedRoot = Path.GetFullPath(repoRoot);
        var mode = NormalizeMode(query.Mode);

        return mode switch
        {
            "packages" => InspectPackages(normalizedRoot, query),
            "namespaces" => InspectNamespaces(normalizedRoot, query),
            "types" => InspectTypes(normalizedRoot, query),
            "type" => InspectType(normalizedRoot, query),
            _ => throw new ArgumentException($"Unsupported metadata mode '{query.Mode}'. Supported modes: packages, namespaces, types, type.")
        };
    }

    private static MetadataReport InspectPackages(string repoRoot, MetadataQuery query)
    {
        var warnings = new List<string>();
        var projectPaths = ResolveProjectPaths(repoRoot, query.TargetPath, warnings);
        var projectReports = projectPaths
            .Select(projectPath => LoadProjectPackageReport(repoRoot, projectPath, warnings))
            .Where(report => report is not null)
            .Cast<ProjectPackageReport>()
            .ToList();

        var packageQuery = (query.Query ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(packageQuery))
        {
            projectReports = projectReports
                .Select(project => new ProjectPackageReport
                {
                    ProjectName = project.ProjectName,
                    RelativePath = project.RelativePath,
                    Packages = project.Packages
                        .Where(package => Matches(package.Id, packageQuery))
                        .ToList()
                })
                .Where(project => project.Packages.Count > 0)
                .ToList();
        }

        var usage = projectReports
            .SelectMany(project => project.Packages.Select(package => (project.ProjectName, Package: package)))
            .GroupBy(entry => entry.Package.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var versions = group
                    .Select(entry => entry.Package.Version)
                    .Where(version => !string.IsNullOrWhiteSpace(version))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(version => version, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var projects = group
                    .Select(entry => entry.ProjectName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new PackageUsageReport
                {
                    Id = group.Key,
                    Versions = versions,
                    Projects = projects,
                    ProjectCount = projects.Count,
                    HasVersionDrift = versions.Count > 1
                };
            })
            .OrderBy(report => report.Id, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, query.Limit))
            .ToList();

        if (usage.Any(package => package.HasVersionDrift))
        {
            warnings.Add("Package version drift detected across projects. Prefer central package management or explicit review before upgrading only one project.");
        }

        return new MetadataReport
        {
            RepoRoot = repoRoot,
            Mode = "packages",
            ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
            Projects = projectReports,
            PackageUsages = usage,
            Warnings = warnings
        };
    }

    private static MetadataReport InspectNamespaces(string repoRoot, MetadataQuery query)
    {
        var warnings = new List<string>();
        var assemblyPath = ResolveAssemblyPath(repoRoot, query, warnings);
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            return new MetadataReport
            {
                RepoRoot = repoRoot,
                Mode = "namespaces",
                ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
                Warnings = warnings
            };
        }

        var types = LoadTypes(assemblyPath, warnings);
        var namespaces = types
            .Select(type => type.Namespace ?? string.Empty)
            .Where(@namespace => !string.IsNullOrWhiteSpace(@namespace))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(@namespace => @namespace, StringComparer.OrdinalIgnoreCase)
            .Where(@namespace => string.IsNullOrWhiteSpace(query.Query) || Matches(@namespace, query.Query!))
            .Take(Math.Max(1, query.Limit))
            .ToList();

        return new MetadataReport
        {
            RepoRoot = repoRoot,
            Mode = "namespaces",
            ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
            AssemblyPath = assemblyPath,
            Namespaces = namespaces,
            Warnings = warnings
        };
    }

    private static MetadataReport InspectTypes(string repoRoot, MetadataQuery query)
    {
        var warnings = new List<string>();
        var assemblyPath = ResolveAssemblyPath(repoRoot, query, warnings);
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            return new MetadataReport
            {
                RepoRoot = repoRoot,
                Mode = "types",
                ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
                Warnings = warnings
            };
        }

        var namespaceFilter = (query.NamespaceFilter ?? string.Empty).Trim();
        var search = (query.Query ?? string.Empty).Trim();
        var types = LoadTypes(assemblyPath, warnings)
            .Where(type => string.IsNullOrWhiteSpace(namespaceFilter)
                           || string.Equals(type.Namespace, namespaceFilter, StringComparison.OrdinalIgnoreCase))
            .Where(type => string.IsNullOrWhiteSpace(search)
                           || Matches(type.Name, search)
                           || Matches(type.FullName ?? string.Empty, search))
            .Select(ToTypeSummary)
            .OrderBy(type => type.FullName, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, query.Limit))
            .ToList();

        return new MetadataReport
        {
            RepoRoot = repoRoot,
            Mode = "types",
            ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
            AssemblyPath = assemblyPath,
            Types = types,
            Warnings = warnings
        };
    }

    private static MetadataReport InspectType(string repoRoot, MetadataQuery query)
    {
        var warnings = new List<string>();
        var assemblyPath = ResolveAssemblyPath(repoRoot, query, warnings);
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            return new MetadataReport
            {
                RepoRoot = repoRoot,
                Mode = "type",
                ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
                Warnings = warnings
            };
        }

        var typeName = (query.TypeName ?? query.Query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(typeName))
        {
            warnings.Add("Type inspection requires --type <Fully.Qualified.Name> or a non-empty query.");
            return new MetadataReport
            {
                RepoRoot = repoRoot,
                Mode = "type",
                ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
                AssemblyPath = assemblyPath,
                Warnings = warnings
            };
        }

        var type = LoadTypes(assemblyPath, warnings)
            .FirstOrDefault(candidate =>
                string.Equals(candidate.FullName, typeName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(candidate.Name, typeName, StringComparison.OrdinalIgnoreCase));

        if (type is null)
        {
            warnings.Add($"Type '{typeName}' was not found in '{assemblyPath}'.");
        }

        return new MetadataReport
        {
            RepoRoot = repoRoot,
            Mode = "type",
            ResolvedTarget = ResolveDisplayTarget(repoRoot, query.TargetPath),
            AssemblyPath = assemblyPath,
            Type = type is null ? null : ToTypeDetail(type),
            Warnings = warnings
        };
    }

    private static string NormalizeMode(string? mode)
    {
        return string.IsNullOrWhiteSpace(mode)
            ? "packages"
            : mode.Trim().ToLowerInvariant();
    }

    private static List<string> ResolveProjectPaths(string repoRoot, string? targetPath, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return Directory.EnumerateFiles(repoRoot, "*.csproj", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                               && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        var resolved = Path.GetFullPath(Path.IsPathRooted(targetPath) ? targetPath : Path.Combine(repoRoot, targetPath));
        if (File.Exists(resolved) && resolved.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return [resolved];
        }

        if (Directory.Exists(resolved))
        {
            return Directory.EnumerateFiles(resolved, "*.csproj", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (File.Exists(resolved) && (resolved.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || resolved.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)))
        {
            return Directory.EnumerateFiles(Path.GetDirectoryName(resolved)!, "*.csproj", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        warnings.Add($"Could not resolve projects from target '{targetPath}'.");
        return [];
    }

    private static ProjectPackageReport? LoadProjectPackageReport(string repoRoot, string projectPath, List<string> warnings)
    {
        try
        {
            var document = XDocument.Load(projectPath);
            var centralVersions = LoadCentralPackageVersions(projectPath, repoRoot);
            var packages = document.Descendants()
                .Where(element => element.Name.LocalName == "PackageReference")
                .Select(element => ToPackageReference(element, centralVersions))
                .OrderBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new ProjectPackageReport
            {
                ProjectName = Path.GetFileNameWithoutExtension(projectPath),
                RelativePath = Path.GetRelativePath(repoRoot, projectPath),
                Packages = packages
            };
        }
        catch (Exception ex)
        {
            warnings.Add($"Failed to read package metadata from '{Path.GetRelativePath(repoRoot, projectPath)}': {ex.Message}");
            return null;
        }
    }

    private static Dictionary<string, string> LoadCentralPackageVersions(string projectPath, string repoRoot)
    {
        var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var current = Path.GetDirectoryName(projectPath);

        while (!string.IsNullOrWhiteSpace(current)
               && current.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase))
        {
            var propsPath = Path.Combine(current, "Directory.Packages.props");
            if (File.Exists(propsPath))
            {
                var document = XDocument.Load(propsPath);
                foreach (var package in document.Descendants().Where(element => element.Name.LocalName == "PackageVersion"))
                {
                    var id = package.Attribute("Include")?.Value ?? package.Attribute("Update")?.Value ?? string.Empty;
                    var version = package.Attribute("Version")?.Value ?? package.Element(package.Name.Namespace + "Version")?.Value ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(version) && !versions.ContainsKey(id))
                    {
                        versions[id] = version.Trim();
                    }
                }
            }

            if (string.Equals(current, repoRoot, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            current = Path.GetDirectoryName(current);
        }

        return versions;
    }

    private static PackageReferenceReport ToPackageReference(XElement element, IReadOnlyDictionary<string, string> centralVersions)
    {
        var id = element.Attribute("Include")?.Value ?? element.Attribute("Update")?.Value ?? string.Empty;
        var versionOverride = GetAttributeOrChildValue(element, "VersionOverride");
        var explicitVersion = GetAttributeOrChildValue(element, "Version");
        var version = !string.IsNullOrWhiteSpace(versionOverride)
            ? versionOverride
            : !string.IsNullOrWhiteSpace(explicitVersion)
                ? explicitVersion
                : centralVersions.TryGetValue(id, out var centralVersion)
                    ? centralVersion
                    : string.Empty;

        var source = !string.IsNullOrWhiteSpace(versionOverride)
            ? "version-override"
            : !string.IsNullOrWhiteSpace(explicitVersion)
                ? "direct"
                : centralVersions.ContainsKey(id)
                    ? "central"
                    : "implicit";

        return new PackageReferenceReport
        {
            Id = id,
            Version = version,
            Source = source,
            IsFloatingVersion = version.Contains('*', StringComparison.Ordinal) || version.Contains('$', StringComparison.Ordinal),
            HasVersionOverride = !string.IsNullOrWhiteSpace(versionOverride)
        };
    }

    private static string GetAttributeOrChildValue(XElement element, string name)
    {
        return element.Attribute(name)?.Value
               ?? element.Element(element.Name.Namespace + name)?.Value
               ?? string.Empty;
    }

    private static string ResolveAssemblyPath(string repoRoot, MetadataQuery query, List<string> warnings)
    {
        if (!string.IsNullOrWhiteSpace(query.AssemblyPath))
        {
            var directAssembly = Path.GetFullPath(Path.IsPathRooted(query.AssemblyPath) ? query.AssemblyPath : Path.Combine(repoRoot, query.AssemblyPath));
            if (File.Exists(directAssembly))
            {
                return directAssembly;
            }

            warnings.Add($"Assembly '{query.AssemblyPath}' was not found.");
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(query.TargetPath))
        {
            warnings.Add("Assembly inspection requires --target <project-or-dll> or --assembly <path>.");
            return string.Empty;
        }

        var resolvedTarget = Path.GetFullPath(Path.IsPathRooted(query.TargetPath) ? query.TargetPath : Path.Combine(repoRoot, query.TargetPath));
        if (File.Exists(resolvedTarget) && resolvedTarget.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return resolvedTarget;
        }

        if (!File.Exists(resolvedTarget) || !resolvedTarget.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"Assembly inspection target '{query.TargetPath}' must be a .csproj or .dll path.");
            return string.Empty;
        }

        if (query.BuildIfNeeded)
        {
            BuildProject(resolvedTarget, repoRoot, query, warnings);
        }

        var assemblyPath = InferProjectAssemblyPath(resolvedTarget, query);
        if (!File.Exists(assemblyPath))
        {
            warnings.Add($"Assembly '{assemblyPath}' does not exist. Build the project first or rerun with --build.");
            return string.Empty;
        }

        return assemblyPath;
    }

    private static void BuildProject(string projectPath, string workingDirectory, MetadataQuery query, List<string> warnings)
    {
        var arguments = $"build \"{projectPath}\" --nologo --configuration \"{query.Configuration}\"";
        if (!string.IsNullOrWhiteSpace(query.Framework))
        {
            arguments += $" --framework \"{query.Framework}\"";
        }

        var result = ProcessRunner.Run("dotnet", arguments, workingDirectory, timeoutMs: 120_000, throwOnError: false);
        if (result.ExitCode != 0 || result.TimedOut)
        {
            warnings.Add(result.TimedOut
                ? $"dotnet build timed out while preparing metadata for '{projectPath}'."
                : $"dotnet build failed while preparing metadata for '{projectPath}'.");
        }
    }

    private static string InferProjectAssemblyPath(string projectPath, MetadataQuery query)
    {
        var document = XDocument.Load(projectPath);
        var assemblyName = document.Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "AssemblyName")
            ?.Value
            ?.Trim();
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            assemblyName = Path.GetFileNameWithoutExtension(projectPath);
        }

        var framework = !string.IsNullOrWhiteSpace(query.Framework)
            ? query.Framework
            : document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "TargetFramework")
                ?.Value
                ?.Trim();

        if (string.IsNullOrWhiteSpace(framework))
        {
            var targetFrameworks = document.Descendants()
                .FirstOrDefault(element => element.Name.LocalName == "TargetFrameworks")
                ?.Value
                ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            framework = targetFrameworks?.FirstOrDefault() ?? string.Empty;
        }

        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        return Path.Combine(projectDirectory, "bin", query.Configuration, framework, $"{assemblyName}.dll");
    }

    private static List<Type> LoadTypes(string assemblyPath, List<string> warnings)
    {
        var loadContext = new MetadataAssemblyLoadContext(assemblyPath);
        try
        {
            var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            try
            {
                return assembly.GetTypes()
                    .OrderBy(type => type.FullName ?? type.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                warnings.AddRange(ex.LoaderExceptions
                    .Where(exception => exception is not null)
                    .Select(exception => exception!.Message)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(5));

                return ex.Types
                    .Where(type => type is not null)
                    .Cast<Type>()
                    .OrderBy(type => type.FullName ?? type.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private static TypeSummaryReport ToTypeSummary(Type type)
    {
        return new TypeSummaryReport
        {
            Name = type.Name,
            FullName = type.FullName ?? type.Name,
            Namespace = type.Namespace ?? string.Empty,
            BaseType = type.BaseType?.FullName ?? string.Empty,
            IsPublic = type.IsPublic || type.IsNestedPublic,
            IsAbstract = type.IsAbstract,
            IsInterface = type.IsInterface,
            MethodCount = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length,
            PropertyCount = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length
        };
    }

    private static TypeDetailReport ToTypeDetail(Type type)
    {
        const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        return new TypeDetailReport
        {
            Name = type.Name,
            FullName = type.FullName ?? type.Name,
            Namespace = type.Namespace ?? string.Empty,
            BaseType = type.BaseType?.FullName ?? string.Empty,
            Interfaces = type.GetInterfaces()
                .Select(@interface => @interface.FullName ?? @interface.Name)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Constructors = type.GetConstructors(MemberFlags)
                .Select(FormatConstructorSignature)
                .OrderBy(signature => signature, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Methods = type.GetMethods(MemberFlags)
                .Where(method => !method.IsSpecialName)
                .Select(FormatMethodSignature)
                .OrderBy(signature => signature, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Properties = type.GetProperties(MemberFlags)
                .Select(property => $"{FormatTypeName(property.PropertyType)} {property.Name}")
                .OrderBy(signature => signature, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Fields = type.GetFields(MemberFlags)
                .Select(field => $"{FormatTypeName(field.FieldType)} {field.Name}")
                .OrderBy(signature => signature, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            IsPublic = type.IsPublic || type.IsNestedPublic,
            IsAbstract = type.IsAbstract,
            IsInterface = type.IsInterface
        };
    }

    private static string FormatConstructorSignature(ConstructorInfo constructor)
    {
        return $"{constructor.DeclaringType?.Name}({string.Join(", ", constructor.GetParameters().Select(FormatParameterSignature))})";
    }

    private static string FormatMethodSignature(MethodInfo method)
    {
        return $"{FormatTypeName(method.ReturnType)} {method.Name}({string.Join(", ", method.GetParameters().Select(FormatParameterSignature))})";
    }

    private static string FormatParameterSignature(ParameterInfo parameter)
    {
        var modifier = parameter.IsOut
            ? "out "
            : parameter.ParameterType.IsByRef
                ? "ref "
                : string.Empty;
        var parameterType = parameter.ParameterType.IsByRef
            ? parameter.ParameterType.GetElementType() ?? parameter.ParameterType
            : parameter.ParameterType;
        return $"{modifier}{FormatTypeName(parameterType)} {parameter.Name}";
    }

    private static string FormatTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericName = type.Name[..type.Name.IndexOf('`')];
        var arguments = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
        return $"{genericName}<{arguments}>";
    }

    private static string ResolveDisplayTarget(string repoRoot, string? targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return repoRoot;
        }

        var resolved = Path.GetFullPath(Path.IsPathRooted(targetPath) ? targetPath : Path.Combine(repoRoot, targetPath));
        return Path.GetRelativePath(repoRoot, resolved);
    }

    private static bool Matches(string candidate, string query)
    {
        return candidate.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class MetadataAssemblyLoadContext(string assemblyPath) : AssemblyLoadContext(isCollectible: true)
    {
        private readonly string assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var candidate = Path.Combine(assemblyDirectory, $"{assemblyName.Name}.dll");
            return File.Exists(candidate) ? LoadFromAssemblyPath(candidate) : null;
        }
    }
}
