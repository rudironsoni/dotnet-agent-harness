---
id: dotnet-test-quality
title: .NET Test Quality
description: Measures test effectiveness. Coverlet code coverage, Stryker.NET mutation testing, flaky tests.
tags: ["testing", "dotnet", "code-coverage", "mutation-testing", "coverlet", "stryker", "quality"]
source: dotnet-artisan
---

# dotnet-test-quality

Test quality analysis for .NET projects. Covers code coverage collection with coverlet, human-readable coverage reports with ReportGenerator, CRAP (Change Risk Anti-Patterns) score analysis to identify undertested complex code, mutation testing with Stryker.NET to evaluate test suite effectiveness, and strategies for detecting and managing flaky tests.

**Version assumptions:** Coverlet 6.x+, ReportGenerator 5.x+, Stryker.NET 4.x+ (.NET 8.0+ baseline).

## Scope

- Coverlet code coverage collection and configuration
- ReportGenerator for human-readable coverage reports
- CRAP score analysis for undertested complex code
- Stryker.NET mutation testing for test suite evaluation
- Flaky test detection and management strategies

## Out of scope

- Test project scaffolding -- see [skill:dotnet-add-testing]
- Testing strategy -- see [skill:dotnet-testing-strategy]
- CI test reporting -- see [skill:dotnet-gha-build-test] and [skill:dotnet-ado-build-test]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with coverlet packages referenced. .NET 8.0+ baseline required.

Cross-references: [skill:dotnet-testing-strategy] for deciding what to test and coverage target guidance, [skill:dotnet-xunit] for xUnit test framework features and configuration.

---

## Code Coverage with Coverlet

Coverlet is the standard open-source code coverage library for .NET.

### Packages

coverlet.collector (recommended data collector approach).

### Collecting Coverage

Use dotnet test --collect:"XPlat Code Coverage".

### Filtering Coverage

Exclude generated code, test projects, or specific namespaces.

---

## Coverage Reports with ReportGenerator

ReportGenerator converts raw coverage data into human-readable HTML reports.

### Installation

dotnet tool install -g dotnet-reportgenerator-globaltool

### Generating Reports

Merge coverage from multiple test projects into a single report.

---

## CRAP Analysis

CRAP (Change Risk Anti-Patterns) scores identify methods that are both complex and poorly tested.

### Formula

CRAP(m) = complexity(m)^2 * (1 - coverage(m)/100)^3 + complexity(m)

### Interpreting CRAP Scores

| Score | Risk Level | Action |
|-------|------------|--------|
| < 5 | Low | Method is simple or well-tested |
| 5-15 | Moderate | Review -- may need additional tests |
| 15-30 | High | Prioritize: add tests or reduce complexity |
| > 30 | Critical | Refactor and add tests immediately |

---

## Mutation Testing with Stryker.NET

Mutation testing evaluates test suite quality by introducing small changes to production code and checking whether tests detect them.

### Installation

dotnet tool install -g dotnet-stryker

### Running Stryker.NET

Run from the test project directory.

### Understanding Mutation Results

- Killed: Test detected the mutation (good)
- Survived: No test detected the mutation (gap)
- No Coverage: No test covers the mutated code (gap)
- Timeout: Mutation caused infinite loop (usually counts as killed)

### Mutation Score

Mutation Score = Killed / (Killed + Survived + NoCoverage) * 100

---

## Flaky Test Detection

Flaky tests pass and fail intermittently without code changes.

### Common Causes

| Cause | Fix |
|-------|-----|
| Shared mutable state | Use proper test isolation |
| Time-dependent logic | Inject TimeProvider |
| Race conditions | Use ICollectionFixture |
| External dependencies | Mock or use Testcontainers |
| Port conflicts | Use dynamic port allocation |

---

## Key Principles

- Coverage is a lagging indicator, not a target. High coverage does not guarantee good tests.
- Use CRAP scores to prioritize. Focus on high complexity + low coverage.
- Run mutation testing on critical paths. It is computationally expensive.
- Fix flaky tests immediately or quarantine them.
- Measure trends, not snapshots.
- Exclude generated code from coverage.

---

## Agent Gotchas

1. Do not confuse coverlet.collector with coverlet.msbuild.
2. Do not hardcode coverage result paths. The GUID changes every run.
3. Do not set coverage thresholds too high initially.
4. Do not run Stryker.NET on the entire solution for CI.
5. Do not ignore survived mutations in trivial code.
6. Do not use [ExcludeFromCodeCoverage] as a blanket fix.

---

## References

- [Coverlet GitHub](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator GitHub](https://github.com/danielpalme/ReportGenerator)
- [Stryker.NET documentation](https://stryker-mutator.io/docs/stryker-net/)
- [TimeProvider in .NET 8](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider)
- [CRAP metric](https://testing.googleblog.com/2011/02/this-code-is-crap.html)
