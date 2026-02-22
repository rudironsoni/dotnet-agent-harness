# xUnit 2.x -> 3.x Upgrade Checklist

## Pre-Upgrade Preparation

### Environment Verification

- [ ] **Target Framework Version**
  - [ ] .NET 8.0 or newer (recommended)
  - [ ] Or .NET Framework 4.7.2 or newer
  - [ ] Note: Not supported on .NET Core 3.1, .NET 5/6/7

- [ ] **Project Format**
  - [ ] Confirm SDK-style project format
  - [ ] Project file should start with `<Project Sdk="Microsoft.NET.Sdk">`

- [ ] **IDE Version**
  - [ ] Visual Studio 2022 17.8+
  - [ ] Or Rider 2023.3+
  - [ ] Or VS Code (latest)

### Code Inventory

- [ ] **Identify async void test methods**
  - [ ] Search pattern: `async void`
  - [ ] Regex: `async\s+void.*\[(Fact|Theory)\]`
  - [ ] Record number of files to modify: ______

- [ ] **Check IAsyncLifetime implementations**
  - [ ] Search for classes implementing `IAsyncLifetime`
  - [ ] Confirm if also implementing `IDisposable`
  - [ ] Plan to move `Dispose` logic to `DisposeAsync`

- [ ] **Identify SkippableFact/SkippableTheory usage**
  - [ ] Search for `[SkippableFact]` and `[SkippableTheory]`
  - [ ] Plan to use `Assert.Skip` or `SkipUnless`

- [ ] **Check custom attributes**
  - [ ] Identify custom classes inheriting from `DataAttribute`
  - [ ] Plan to update to new async API

### Dependency Assessment

- [ ] **Record current package versions**
  - [ ] xunit: ______
  - [ ] xunit.runner.visualstudio: ______
  - [ ] Microsoft.NET.Test.Sdk: ______
  - [ ] AwesomeAssertions/FluentAssertions: ______
  - [ ] NSubstitute/Moq: ______
  - [ ] AutoFixture: ______

- [ ] **Verify compatibility**
  - [ ] Check if packages support xUnit 3.x
  - [ ] Pay special attention to AutoFixture.Xunit3 package

### Backup

- [ ] **Create upgrade branch**
  ```bash
  git checkout -b feature/upgrade-xunit-v3
  git push -u origin feature/upgrade-xunit-v3
  ```

---

## Upgrade Execution

### Project File Modifications

- [ ] **Update OutputType**
  ```xml
  <OutputType>Exe</OutputType>
  ```

- [ ] **Update package references**
  - [ ] Remove `xunit` -> Add `xunit.v3`
  - [ ] Remove `xunit.abstractions` (no longer needed)
  - [ ] Update `xunit.runner.visualstudio` to 3.x version
  - [ ] Update `Microsoft.NET.Test.Sdk` to latest version

- [ ] **Add xunit.runner.json** (optional)
  ```json
  {
    "$schema": "https://xunit.net/schema/v3/xunit.runner.schema.json",
    "parallelAlgorithm": "conservative",
    "maxParallelThreads": 4
  }
  ```

### Code Fixes

- [ ] **Fix async void tests**
  - [ ] Change all `async void` to `async Task`
  - [ ] Verify number of changes: ______

- [ ] **Update using statements**
  - [ ] Remove `using Xunit.Abstractions;`

- [ ] **Fix IAsyncLifetime implementations**
  - [ ] Move `Dispose` logic into `DisposeAsync`

- [ ] **Fix SkippableFact/SkippableTheory**
  - [ ] Use `Assert.Skip` or `SkipUnless/SkipWhen` attributes

- [ ] **Update custom DataAttribute**
  - [ ] Implement new `GetDataAsync` method

### Build and Test

- [ ] **Clean and restore**
  ```bash
  dotnet clean
  dotnet restore
  ```

- [ ] **Build**
  ```bash
  dotnet build
  ```
  - [ ] Record number of build errors: ______
  - [ ] Resolve build errors one by one

- [ ] **Run tests**
  ```bash
  dotnet test --verbosity normal
  ```
  - [ ] Record test results: Passed ______ / Failed ______ / Skipped ______

---

## Post-Upgrade Verification

### Functional Verification

- [ ] **All tests pass**
  - [ ] Unit tests: ______
  - [ ] Integration tests: ______

- [ ] **Performance comparison**
  - [ ] Pre-upgrade execution time: ______
  - [ ] Post-upgrade execution time: ______

### CI/CD Verification

- [ ] **Test execution**
  ```bash
  dotnet test --configuration Release --logger trx
  ```

- [ ] **Test reports**
  - [ ] Confirm report format parses correctly
  - [ ] Verify test results display correctly

- [ ] **Parallel execution settings**
  - [ ] Adjust `maxParallelThreads` based on CI environment

### Documentation and Training

- [ ] **Update project documentation**
  - [ ] README.md
  - [ ] CONTRIBUTING.md

- [ ] **Team knowledge transfer**
  - [ ] Share upgrade experience
  - [ ] Introduce new feature usage

---

## New Feature Enablement (Optional)

- [ ] **Dynamic skip tests**
  - [ ] Use `Assert.Skip` or `SkipUnless/SkipWhen`

- [ ] **Explicit tests**
  - [ ] Mark `[Fact(Explicit = true)]`

- [ ] **Assembly Fixtures**
  - [ ] Create global resource management

- [ ] **Test Pipeline Startup**
  - [ ] Implement global initialization

---

## Issue Tracking

| Issue Description | Solution | Status |
| ----------------- | -------- | ------ |
|                   |          |        |
|                   |          |        |
|                   |          |        |

---

## Sign-off

- [ ] Developer confirmation: ______________ Date: ______________
- [ ] Code review: ______________ Date: ______________
- [ ] Test confirmation: ______________ Date: ______________
