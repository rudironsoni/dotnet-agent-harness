using System;
using System.IO;
using System.Reflection;

namespace DotNetAgentHarness.Evals.Engine;

public static class DefaultEvalCaseCatalog
{
    private const string ResourceName = "DotNetAgentHarness.Evals.Defaults.routing.yaml";

    public static string EnsureDefaultCasesPath()
    {
        var targetPath = Path.Combine(
            Path.GetTempPath(),
            "dotnet-agent-harness",
            "eval-cases",
            "routing.yaml");

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(targetPath) && new FileInfo(targetPath).Length > 0)
        {
            return targetPath;
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded default eval cases resource '{ResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        File.WriteAllText(targetPath, reader.ReadToEnd());
        return targetPath;
    }
}
