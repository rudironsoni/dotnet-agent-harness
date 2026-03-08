using System.Text.Json;
using System.Text.RegularExpressions;

namespace DotnetAgentHarness.Cli.Services;

public interface IConfigDetector
{
    Task<bool> HasDeleteTrueAsync(string path);
}

public class ConfigDetector : IConfigDetector
{
    public async Task<bool> HasDeleteTrueAsync(string path)
    {
        var configPath = Path.Combine(path, ".rulesync", "rulesync.jsonc");
        
        if (!File.Exists(configPath))
        {
            return false;
        }

        var content = await File.ReadAllTextAsync(configPath);
        
        // Strip comments from JSONC
        content = StripComments(content);
        
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("delete", out var deleteProp))
            {
                return deleteProp.GetBoolean();
            }
        }
        catch
        {
            // Invalid JSON or parse error, treat as false
            return false;
        }
        
        return false;
    }

    private static string StripComments(string jsonc)
    {
        // Remove single-line comments //...
        jsonc = Regex.Replace(jsonc, @"//.*$", "", RegexOptions.Multiline);
        
        // Remove multi-line comments /* ... */
        jsonc = Regex.Replace(jsonc, @"/\*[\s\S]*?\*/", "");
        
        return jsonc;
    }
}
