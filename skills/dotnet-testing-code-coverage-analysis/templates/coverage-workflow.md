# Code Coverage Workflow Guide

This document explains the complete workflow for performing code coverage analysis in different environments.

---

## Workflow Overview

```text
Configure Project → Run Tests → Collect Coverage → Generate Report → Analyze Results → Improve Tests
```

---

## Method 1: Using .NET CLI (Recommended for CI/CD)

### Step 1: Verify Project Configuration

Ensure the test project has the required packages installed:

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.3">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

### Step 2: Run Tests and Collect Coverage

**Basic execution:**

```powershell
# Run tests and collect coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Specify output directory:**

```powershell
# Specify coverage results output location
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

**Using runsettings file:**

```powershell
# Use advanced settings
dotnet test --settings coverage.runsettings
```

### Step 3: Generate HTML Report

Install ReportGenerator tool:

```powershell
# Global installation
dotnet tool install -g dotnet-reportgenerator-globaltool

# Or local installation
dotnet new tool-manifest
dotnet tool install dotnet-reportgenerator-globaltool
```

Generate report:

```powershell
# Generate HTML report
reportgenerator `
  -reports:"**\coverage.cobertura.xml" `
  -targetdir:"coveragereport" `
  -reporttypes:Html

# Open report
start coveragereport\index.html
```

### Step 4: Analyze Results

Open the generated HTML report and check:

1. **Overall Coverage**: Line Coverage, Branch Coverage
2. **Module Coverage**: Coverage distribution across projects
3. **File Coverage**: Identify files with low coverage
4. **Risk Areas**: Uncovered code marked in red

---

## Method 2: Using Visual Studio + Fine Code Coverage

### Step 1: Install Fine Code Coverage

1. Open Visual Studio
2. Extensions → Manage Extensions
3. Search for "Fine Code Coverage"
4. Install and restart

### Step 2: Configure Options

1. Tools → Options → Fine Code Coverage
2. Enable the following settings:
   - Run (Common) → Enable: `True`
   - Editor Colouring Line Highlighting: `True`

### Step 3: Run Tests

1. Open Test Explorer (Test → Test Explorer)
2. Run all tests or specific tests
3. Fine Code Coverage will automatically display results

### Step 4: View Coverage

**Open Fine Code Coverage window:**

- View → Other Windows → Fine Code Coverage

**Enable editor indicators:**

- Tools → FCC Toggle Indicators

**Color coding:**

- Green: Covered by tests
- Yellow: Partially covered (some branches not tested)
- Red: Not covered

### Step 5: Improve Coverage

Based on red indicators:

1. Identify uncovered code blocks
2. Analyze if testing is needed
3. Write new test cases
4. Re-run tests to verify

---

## Method 3: Using VS Code

### Step 1: Install Extensions

Ensure C# Dev Kit is installed:

1. Press `Ctrl+Shift+X` to open Extensions
2. Search for "C# Dev Kit"
3. Install and reload

### Step 2: Open Test Explorer

1. Click the beaker icon in the activity bar
2. Or run command: `Testing: Focus on Test Explorer View`

### Step 3: Run Coverage Tests

In Test Explorer:

1. Click "Run Coverage Tests" icon
2. Wait for tests to complete

### Step 4: View Results

**Test Coverage View:**

- Displays coverage information in tree structure
- Coverage percentage for each file

**Editor display:**

- Green: Covered code
- Red: Uncovered code
- Execution count: Shows how many times each line was executed

**File Explorer display:**

- Shows coverage percentage next to file names

### Step 5: Toggle Inline Coverage

Use shortcut `Ctrl+; Ctrl+Shift+I` or run command:

- `Test: Show Inline Coverage`

---

## Method 4: CI/CD Integration

### GitHub Actions

Create `.github/workflows/test-coverage.yml`:

```yaml
name: Test with Coverage

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run tests with coverage
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Install ReportGenerator
        run: dotnet tool install -g dotnet-reportgenerator-globaltool
      
      - name: Generate coverage report
        run: |
          reportgenerator \
            -reports:**/coverage.cobertura.xml \
            -targetdir:coverage \
            -reporttypes:Html;Cobertura
      
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v4
        with:
          files: coverage/Cobertura.xml
          fail_ci_if_error: true
```

### Azure DevOps

Create `azure-pipelines.yml`:

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      packageType: 'sdk'
      version: '9.0.x'
  
  - task: DotNetCoreCLI@2
    displayName: 'Restore packages'
    inputs:
      command: 'restore'
  
  - task: DotNetCoreCLI@2
    displayName: 'Build solution'
    inputs:
      command: 'build'
      arguments: '--no-restore'
  
  - task: DotNetCoreCLI@2
    displayName: 'Run tests with coverage'
    inputs:
      command: 'test'
      arguments: '--no-build --collect:"XPlat Code Coverage"'
      publishTestResults: true
  
  - task: PublishCodeCoverageResults@1
    displayName: 'Publish coverage report'
    inputs:
      codeCoverageTool: 'Cobertura'
      summaryFileLocation: '$(Agent.TempDirectory)/**/*coverage.cobertura.xml'
      reportDirectory: '$(Build.SourcesDirectory)/coverage'
```

---

## Coverage Improvement Strategy

### Phase 1: Establish Baseline (Target 60-70%)

1. **Identify Core Modules**:
   - Business logic
   - Data validation
   - Calculation logic

2. **Write Basic Tests**:
   - Main flow tests
   - Basic boundary tests

3. **Execute Coverage**:
   - Establish initial baseline

### Phase 2: Supplement Critical Tests (Target 70-80%)

1. **Analyze Gaps**:
   - Review red areas
   - Identify critical uncovered code

2. **Supplement Tests**:
   - Boundary condition tests
   - Exception scenario tests
   - Branch coverage tests

3. **Continuous Monitoring**:
   - Check coverage changes for each PR

### Phase 3: Refine Tests (Target 80-85%)

1. **Deep Analysis**:
   - Check yellow areas (partial coverage)
   - Ensure all branches are tested

2. **Quality Improvement**:
   - Improve test assertions
   - Add boundary conditions
   - Test error handling

3. **Exclude Unnecessary Code**:
   - Use `[ExcludeFromCodeCoverage]`
   - Exclude in runsettings

### Phase 4: Maintenance and Monitoring

1. **Set CI/CD Gates**:
   - Coverage cannot decrease
   - New code must have tests

2. **Regular Review**:
   - Review coverage reports weekly
   - Identify risk areas

3. **Continuous Improvement**:
   - Refactor high-complexity code
   - Supplement missing tests

---

## Coverage Report Interpretation

### Key Metrics

1. **Line Coverage**
   - Calculation: Executed code lines / Total code lines
   - Recommendation: ≥ 70%

2. **Branch Coverage**
   - Calculation: Executed branches / Total branches
   - Recommendation: ≥ 60%
   - **More important than line coverage**

3. **Method Coverage**
   - Calculation: Executed methods / Total methods
   - Recommendation: ≥ 75%

### Color Coding Meaning

| Color | Coverage Range | Status | Recommended Action       |
|-------|----------------|--------|--------------------------|
| Green | ≥ 75%          | Good   | Maintain current state   |
| Yellow| 50-74%         | Warning| Evaluate if tests needed |
| Red   | < 50%          | Danger | Prioritize adding tests  |
| Gray  | N/A            | Excluded| Verify exclusion settings |

---

## Troubleshooting Common Issues

### Issue 1: Coverage Shows 0%

**Possible Causes:**

- `coverlet.collector` not installed
- runsettings configuration error
- Tests not actually executed

**Solution:**

```powershell
# Verify package installation
dotnet list package | Select-String "coverlet"

# Reinstall
dotnet add package coverlet.collector

# Clear cache and re-test
dotnet clean
dotnet test --collect:"XPlat Code Coverage"
```

### Issue 2: ReportGenerator Cannot Find Coverage Files

**Solution:**

```powershell
# Use absolute path
reportgenerator `
  -reports:"$(Get-Location)\TestResults\**\coverage.cobertura.xml" `
  -targetdir:"coveragereport" `
  -reporttypes:Html

# Or specify full path
reportgenerator `
  -reports:"C:\Projects\MyApp\TestResults\{guid}\coverage.cobertura.xml" `
  -targetdir:"coveragereport" `
  -reporttypes:Html
```

### Issue 3: VS Code Cannot Display Coverage

**Solution:**

1. Ensure C# Dev Kit is installed
2. Re-run "Run Coverage Tests"
3. Check if lcov file is generated
4. Reload window (`Ctrl+Shift+P` → `Reload Window`)

### Issue 4: CI/CD Coverage Fails

**Solution:**

```yaml
# Debug in GitHub Actions
- name: Display coverage files
  run: |
    echo "Coverage files:"
    find . -name "coverage.cobertura.xml"
    
- name: Run tests with verbose output
  run: dotnet test --verbosity detailed --collect:"XPlat Code Coverage"
```

---

## Checklist

Before running coverage analysis, verify the following:

### Project Configuration

- [ ] `coverlet.collector` package installed
- [ ] Test project configured correctly (`IsTestProject=true`)
- [ ] runsettings file format correct (if used)

### Run Tests

- [ ] All tests pass
- [ ] Using correct collector parameters
- [ ] Coverage files successfully generated

### View Reports

- [ ] Report files open correctly
- [ ] Coverage data meets expectations
- [ ] Areas needing improvement identified

### CI/CD Integration

- [ ] Pipeline runs successfully
- [ ] Coverage reports uploaded correctly
- [ ] Gate settings meet team standards

---

## Best Practices

### DO (Should Do)

✅ Review coverage reports regularly
✅ Focus on branch coverage rather than line coverage
✅ Exclude unnecessary code (e.g., auto-generated code)
✅ Integrate coverage checks in CI/CD
✅ Set reasonable coverage targets (70-85%)
✅ Combine with complexity metrics to evaluate testing needs

### DON'T (Should Not Do)

❌ Use coverage as a KPI
❌ Write tests without assertions just to increase numbers
❌ Pursue 100% coverage
❌ Ignore test quality while only looking at coverage numbers
❌ Test simple getters/setters
❌ Blindly add tests without analysis

---

## Related Resources

- [Coverlet Official Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [Fine Code Coverage](https://github.com/FortuneN/FineCodeCoverage)
- [Microsoft Coverage Documentation](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage)
