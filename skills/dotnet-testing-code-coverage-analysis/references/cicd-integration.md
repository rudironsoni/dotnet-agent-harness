# CI/CD Coverage Integration: Complete Configuration Examples

> This document is extracted from SKILL.md `## CI/CD Integration` and contains complete YAML configuration examples for GitHub Actions and Azure DevOps.

## GitHub Actions Example

```yaml
name: Test with Coverage

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Run tests with coverage
        run: dotnet test --collect:"XPlat Code Coverage"
      
      - name: Generate coverage report
        run: |
          dotnet tool install -g dotnet-reportgenerator-globaltool
          reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

## Azure DevOps Example

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run tests with coverage'
  inputs:
    command: 'test'
    arguments: '--collect:"XPlat Code Coverage"'
    publishTestResults: true

- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/*coverage.cobertura.xml'
```
