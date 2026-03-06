using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DotNetAgentHarness.Tools.Engine;

/// <summary>
/// Tracks installed bundles in the target directory.
/// </summary>
public sealed class InstallManifest
{
    /// <summary>
    /// Version of the harness that was installed.
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// List of target identifiers that were installed.
    /// </summary>
    public List<string> Targets { get; set; } = new();

    /// <summary>
    /// Timestamp when the installation occurred.
    /// </summary>
    public DateTime InstalledAt { get; set; }

    /// <summary>
    /// Loads an existing install manifest from the specified path.
    /// </summary>
    public static InstallManifest? Load(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<InstallManifest>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves the install manifest to the specified path.
    /// </summary>
    public void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
