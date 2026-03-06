using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetAgentHarness.Tools.Engine;

public sealed class BootstrapProfileDefinition
{
    public string Id { get; init; } = string.Empty;
    public List<string> Features { get; init; } = new();
}

public static class BootstrapProfileCatalog
{
    public const string Core = "core";
    public const string PlatformNative = "platform-native";
    public const string Full = "full";

    private static readonly Dictionary<string, BootstrapProfileDefinition> Profiles = new(StringComparer.OrdinalIgnoreCase)
    {
        [Core] = new()
        {
            Id = Core,
            Features = ["rules", "skills", "commands"]
        },
        [PlatformNative] = new()
        {
            Id = PlatformNative,
            Features = ["rules", "skills", "subagents", "commands", "hooks", "mcp"]
        },
        [Full] = new()
        {
            Id = Full,
            Features = ["*"]
        }
    };

    public static BootstrapProfileDefinition Resolve(string? profile)
    {
        var normalized = string.IsNullOrWhiteSpace(profile) ? PlatformNative : profile.Trim().ToLowerInvariant();
        if (!Profiles.TryGetValue(normalized, out var definition))
        {
            throw new ArgumentException($"Unsupported bootstrap profile '{profile}'. Supported profiles: {string.Join(", ", Profiles.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))}.");
        }

        return new BootstrapProfileDefinition
        {
            Id = definition.Id,
            Features = definition.Features.ToList()
        };
    }
}
