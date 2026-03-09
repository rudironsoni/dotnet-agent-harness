namespace DotnetAgentHarness.Cli.Services;

using System.Text.Json;
using System.Text.RegularExpressions;

public partial class ConfigDetector : IConfigDetector
{
    private static readonly Regex RulesyncJsoncRegex = MyRegex();

    public Task<bool> HasDeleteTrueAsync(string path)
    {
        string configPath = Path.Combine(path, ".rulesync", "rulesync.jsonc");

        if (!File.Exists(configPath))
        {
            return Task.FromResult(false);
        }

        try
        {
            string content = File.ReadAllText(configPath);

            // Simple check for delete: true (handles JSONC with comments)
            if (RulesyncJsoncRegex.IsMatch(content))
            {
                return Task.FromResult(true);
            }

            // Fallback: Try to parse as JSON (ignoring comments)
            string jsonContent = StripComments(content);
            using JsonDocument doc = JsonDocument.Parse(jsonContent);

            if (doc.RootElement.TryGetProperty("delete", out JsonElement deleteElement))
            {
                return Task.FromResult(deleteElement.GetBoolean());
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return Task.FromResult(false);
    }

    public Task<bool> HasDeclarativeSourcesAsync(string path)
    {
        string configPath = Path.Combine(path, ".rulesync", "rulesync.jsonc");

        if (!File.Exists(configPath))
        {
            return Task.FromResult(false);
        }

        try
        {
            string content = File.ReadAllText(configPath);
            string jsonContent = StripComments(content);
            using JsonDocument doc = JsonDocument.Parse(jsonContent);

            if (doc.RootElement.TryGetProperty("sources", out JsonElement sourcesElement))
            {
                return Task.FromResult(sourcesElement.GetArrayLength() > 0);
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return Task.FromResult(false);
    }

    public Task<string[]> GetTargetPlatformsAsync(string path)
    {
        string configPath = Path.Combine(path, ".rulesync", "rulesync.jsonc");

        if (!File.Exists(configPath))
        {
            return Task.FromResult(Array.Empty<string>());
        }

        try
        {
            string content = File.ReadAllText(configPath);
            string jsonContent = StripComments(content);
            using JsonDocument doc = JsonDocument.Parse(jsonContent);

            if (doc.RootElement.TryGetProperty("targets", out JsonElement targetsElement))
            {
                string[] targets = targetsElement.EnumerateArray()
                    .Select(e => e.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                return Task.FromResult(targets);
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return Task.FromResult(Array.Empty<string>());
    }

    private static string StripComments(string jsonc)
    {
        // Remove single-line comments
        string result = Regex.Replace(jsonc, @"//.*$", string.Empty, RegexOptions.Multiline);

        // Remove multi-line comments and return
        return Regex.Replace(result, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
    }

    [GeneratedRegex(@"""delete""\s*:\s*true", RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex();
}

public interface IConfigDetector
{
    Task<bool> HasDeleteTrueAsync(string path);

    Task<bool> HasDeclarativeSourcesAsync(string path);

    Task<string[]> GetTargetPlatformsAsync(string path);
}
