using System.IO;
using DotNetAgentHarness.Evals.Engine;
using Xunit;

namespace DotNetAgentHarness.Evals.Tests;

public class YamlParserTests
{
    [Fact]
    public void LoadCases_Throws_WhenYamlDeserializesToNull()
    {
        var path = WriteTempYaml("null");
        try
        {
            var exception = Assert.Throws<InvalidDataException>(() => YamlParser.LoadCases(path));

            Assert.Contains("could not be deserialized", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadCases_Throws_WhenYamlDeserializesToEmptyCaseList()
    {
        var path = WriteTempYaml("[]");
        try
        {
            var exception = Assert.Throws<InvalidDataException>(() => YamlParser.LoadCases(path));

            Assert.Contains("did not contain any eval cases", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadCases_LoadsPlatformAndRetirementFields()
    {
        var path = WriteTempYaml("""
            - id: "retirement-001"
              prompt: "Decide how to test a value object."
              expected_trigger: "dotnet-testing-strategy"
              trial_count: 3
              case_type: "retirement"
              platforms: ["claudecode", "codexcli"]
              unloaded_expected_trigger: "none"
              fixture_response: "Use unit tests."
              fixture_trigger: "dotnet-testing-strategy"
              assertions:
                - type: "contains"
                  value: "unit"
            """);
        try
        {
            var cases = YamlParser.LoadCases(path);

            var evalCase = Assert.Single(cases);
            Assert.Equal("retirement", evalCase.CaseType);
            Assert.Equal("none", evalCase.UnloadedExpectedTrigger);
            Assert.Equal(2, evalCase.Platforms.Count);
            Assert.Contains("codexcli", evalCase.Platforms);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WriteTempYaml(string contents)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, contents);
        return path;
    }
}
