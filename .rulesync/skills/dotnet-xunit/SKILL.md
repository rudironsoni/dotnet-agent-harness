---
id: dotnet-xunit
title: .NET xUnit Testing
description: Authors xUnit v3 tests -- Facts, Theories, fixtures, parallelism, IAsyncLifetime.
tags: ["testing", "dotnet", "xunit", "unit-testing", "fixtures", "async"]
source: dotnet-artisan
---

# dotnet-xunit

xUnit v3 testing framework features for .NET. Covers [Fact] and [Theory] attributes, test fixtures (IClassFixture, ICollectionFixture), parallel execution configuration, IAsyncLifetime for async setup/teardown, custom assertions, and xUnit analyzers. Includes v2 compatibility notes where behavior differs.

**Version assumptions:** xUnit v3 primary (.NET 8.0+ baseline). Where v3 behavior differs from v2, compatibility notes are provided inline. xUnit v2 remains widely used; many projects will encounter both versions during migration.

## Scope

- [Fact] and [Theory] test attributes and data sources
- Test fixtures (IClassFixture, ICollectionFixture) and shared context
- Parallel execution configuration and collection ordering
- IAsyncLifetime for async setup/teardown
- xUnit analyzers and custom assertions
- xUnit v3 migration from v2 (TheoryDataRow, ValueTask lifecycle)

## Out of scope

- Test project scaffolding -- see [skill:dotnet-add-testing]
- Testing strategy and test type decisions -- see [skill:dotnet-testing-strategy]
- Integration testing patterns (WebApplicationFactory, Testcontainers) -- see [skill:dotnet-integration-testing]
- Snapshot testing with Verify -- see [skill:dotnet-snapshot-testing]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with xUnit packages referenced. Run [skill:dotnet-version-detection] to confirm .NET 8.0+ baseline for xUnit v3 support.

Cross-references: [skill:dotnet-testing-strategy] for deciding what to test and how, [skill:dotnet-integration-testing] for combining xUnit with WebApplicationFactory and Testcontainers.

---

## xUnit v3 vs v2: Key Changes

| Feature | xUnit v2 | xUnit v3 |
|---------|----------|----------|
| **Package** | xunit (2.x) | xunit.v3 |
| **Runner** | xunit.runner.visualstudio | xunit.runner.visualstudio (3.x) |
| **Async lifecycle** | IAsyncLifetime | IAsyncLifetime (now returns ValueTask) |
| **Assert package** | Bundled | Separate xunit.v3.assert (or xunit.v3.assert.source for extensibility) |
| **Parallelism default** | Per-collection | Per-collection (same, but configurable per-assembly) |
| **Timeout** | Timeout property on [Fact] and [Theory] | Timeout property on [Fact] and [Theory] (unchanged) |
| **Test output** | ITestOutputHelper | ITestOutputHelper (unchanged) |
| **[ClassData]** | Returns IEnumerable<object[]> | Returns IEnumerable<TheoryDataRow<T>> (strongly typed) |
| **[MemberData]** | Returns IEnumerable<object[]> | Supports TheoryData<T> and TheoryDataRow<T> |
| **Assertion messages** | Optional string parameter on Assert methods | Removed in favor of custom assertions (v3.0); use Assert.Fail() for explicit messages |

**v2 compatibility note:** If migrating from v2, replace xunit package with xunit.v3. Most [Fact] and [Theory] tests work without changes. The primary migration effort is in IAsyncLifetime (return type changes to ValueTask), [ClassData] (strongly typed row format), and removed assertion message parameters.

---

## Facts and Theories

### [Fact] -- Single Test Case

Use [Fact] for tests with no parameters.

### [Theory] -- Parameterized Tests

Use [Theory] to run the same test logic with different inputs.

#### [InlineData]

Best for simple value types.

#### [MemberData] with TheoryData<T>

Best for complex data or shared datasets.

#### [ClassData]

Best for data shared across multiple test classes.

---

## Fixtures: Shared Setup and Teardown

Fixtures provide shared, expensive resources across tests while maintaining test isolation.

### IClassFixture<T> -- Shared Per Test Class

Use when multiple tests in the same class share an expensive resource (database connection, configuration).

### ICollectionFixture<T> -- Shared Across Test Classes

Use when multiple test classes need the same expensive resource.

### IAsyncLifetime on Test Classes

For per-test async setup/teardown without a shared fixture.

---

## Parallel Execution

### Default Behavior

xUnit runs test classes within a collection sequentially but runs different collections in parallel. Each test class without an explicit [Collection] attribute is its own implicit collection, so by default test classes run in parallel.

### Controlling Parallelism

Place tests that share mutable state in the same collection, or configure assembly-level settings via xunit.runner.json.

---

## Test Output

### ITestOutputHelper

Capture diagnostic output that appears in test results.

### Integrating with ILogger

Bridge xUnit output to Microsoft.Extensions.Logging for integration tests.

---

## Custom Assertions

### Extending Assert with Custom Methods

Create domain-specific assertions for cleaner test code.

### Using Assert.Multiple (xUnit v3)

Group related assertions so all are evaluated even if one fails.

---

## xUnit Analyzers

The xunit.analyzers package (included with xUnit v3) catches common test authoring mistakes at compile time.

---

## Key Principles

- One fact per [Fact], one concept per [Theory]. If a [Theory] tests fundamentally different scenarios, split into separate [Fact] methods.
- Use IClassFixture for expensive shared resources within a single test class. Use ICollectionFixture when multiple classes share the same resource.
- Do not disable parallelism globally. Instead, group tests that share mutable state into named collections.
- Use IAsyncLifetime for async setup/teardown instead of constructors and IDisposable. Constructors cannot be async, and IDisposable.Dispose() does not await.
- Keep test data close to the test. Prefer [InlineData] for simple cases. Use [MemberData] or [ClassData] only when data is complex or shared.
- Enable xUnit analyzers in all test projects. They catch common mistakes that lead to false-passing or flaky tests.

---

## Agent Gotchas

1. Do not use constructor-injected ITestOutputHelper in static methods. ITestOutputHelper is per-test-instance; store it in an instance field, not a static one.
2. Do not forget to make fixture classes public. xUnit requires fixture types to be public with a public parameterless constructor (or IAsyncLifetime). Non-public fixtures cause silent failures.
3. Do not mix [Fact] and [Theory] on the same method. A method is either a fact or a theory, not both.
4. Do not return void from async test methods. Return Task or ValueTask. async void tests report false success because xUnit cannot observe the async completion.
5. Do not use [Collection] without a matching [CollectionDefinition]. An unmatched collection name silently creates an implicit collection with default behavior, defeating the purpose.

---

## References

- [xUnit Documentation](https://xunit.net/)
- [xUnit v3 migration guide](https://xunit.net/docs/getting-started/v3/migration)
- [xUnit analyzers](https://xunit.net/xunit.analyzers/rules/)
- [Shared context in xUnit](https://xunit.net/docs/shared-context)
- [Configuring xUnit with JSON](https://xunit.net/docs/configuration-files)
