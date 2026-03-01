using System;
using System.Collections.Generic;
using System.IO;
using DotNetAgentHarness.Evals.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DotNetAgentHarness.Evals.Engine;

public static class YamlParser
{
    public static List<EvalCase> LoadCases(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Evaluation cases file not found: {filePath}");

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var yaml = File.ReadAllText(filePath);
        var cases = deserializer.Deserialize<List<EvalCase>>(yaml);

        // Fail fast schema validation
        foreach (var c in cases)
        {
            if (string.IsNullOrWhiteSpace(c.Id)) throw new InvalidDataException("EvalCase missing 'id'");
            if (string.IsNullOrWhiteSpace(c.Prompt)) throw new InvalidDataException($"EvalCase {c.Id} missing 'prompt'");
        }

        return cases ?? new List<EvalCase>();
    }
}
