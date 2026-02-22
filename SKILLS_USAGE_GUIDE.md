# .NET Testing Agent Skills User Manual

This manual provides detailed instructions on how to use .NET Testing Agent Skills on various AI assistant platforms, including a complete skills list, usage scenarios, and best practices.

---

## 📋 Table of Contents

> 💡 **Quick Start**: Want to look something up quickly? Please refer to the [SKILLS_QUICK_REFERENCE.md](SKILLS_QUICK_REFERENCE.md) quick reference guide

1. [Introduction](#introduction)
2. [Supported Platforms and Requirements](#supported-platforms-and-requirements)
3. [Installation and Setup](#installation-and-setup)
4. [Skills Overview](#skills-overview)
5. [How to Use](#how-to-use)
6. [Detailed Skill Descriptions](#detailed-skill-descriptions)
7. [Usage Scenario Examples](#usage-scenario-examples)
8. [Recommended Skill Combinations](#recommended-skill-combinations)
9. [FAQ](#faq)
10. [Advanced Usage](#advanced-usage)

---

## Introduction

### What are .NET Testing Agent Skills?

This is a collection of Agent Skills designed specifically for .NET test development, distilled from the "Old-School Software Engineer's Testing Practice - 30 Day Challenge" (2025 iThome Ironman Software Development Category Champion).

**Following the [agentskills.io](https://agentskills.io) open standard**, these skills are **cross-platform compatible** and can be used with GitHub Copilot, Claude, Cursor, and other AI tools.

These skills enable AI assistants to:

- 🎯 **Auto-recognize**: Automatically load relevant testing skills based on your conversation content (optimized keywords through description Keywords)
- 📚 **Provide professional guidance**: Give testing advice that follows industry best practices
- 🔧 **Generate code**: Produce test code and project structures that follow specifications
- 🔄 **Integrate workflows**: Assist with the complete flow from unit testing to integration testing
- 🌐 **Cross-platform support**: Skill content follows open standards and can be used with any AI tool supporting agentskills.io

### Skills Latest Optimization (2026-02-01)

This skills collection has been fully optimized and **complies with [agentskills.io](https://agentskills.io/specification) official specifications**:

- ✅ **29 Skills fully optimized**: description includes core description + Keywords keywords
- ✅ **Description multi-line format**: Clear explanation of core functions, applicable scenarios, coverage scope, and integrated trigger keywords
- ✅ **Entry skill navigation**: Two overview skills provide intelligent recommendations
- ✅ **Compliant with official specifications**: Removed non-standard `triggers` field, replaced with `Keywords:` format in description

**Details**: Please refer to [OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md)

### Skills Coverage Scope

| Category | Skill Count | Topics Covered |
| -------- | ----------- | -------------- |
| Overview Skills | 2 | Intelligent navigation and skill recommendations |
| Basic Skills (dotnet-testing) | 19 | Unit testing, assertions, mocking, test data generation |
| Advanced Skills (dotnet-testing-advanced) | 8 | Integration testing, containerized testing, framework migration |
| **Total** | **29** | Overview guidance + Day 01-30 complete content |

---

## Supported Platforms and Requirements

### Supported AI Assistant Platforms

> 💡 This skills collection follows the [agentskills.io](https://agentskills.io) open standard and can be used on multiple AI platforms

| Platform | Support Status | Notes |
| -------- | -------------- | ----- |
| **GitHub Copilot (VS Code)** | ✅ Full Support | Copy to `.github/skills/` (v1.109+ also supports `.claude/skills/` and other paths) |
| **GitHub Copilot CLI** | ✅ Full Support | Same as above |
| **Claude Desktop** | ✅ Full Support | Use `/plugin` command or copy to project |
| **Claude Code CLI** | ✅ Full Support | Copy to `.claude/skills/` |
| **Cursor** | ✅ Full Support | Copy to `.cursor/skills/` |
| **Other Agent Skills Tools** | ✅ Universal | Compliant with agentskills.io standard, refer to that tool's documentation |

> 💡 **GitHub Copilot v1.109+ Update**: Agent Skills is now officially GA and enabled by default, with flexible search path support. See [GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md) for details.

### Requirements

#### Basic Skills

- .NET 8.0 SDK or later
- Any supported IDE (VS Code, Visual Studio, Rider)

#### Advanced Skills (Integration Testing)

- Docker Desktop (for Testcontainers)
- WSL2 (Windows environment)
- .NET Aspire Workload (for Aspire Testing)

---

## Installation and Setup

### Method 1: Using npx skills install (Recommended)

```bash
# Install directly from GitHub to Claude Code global skills
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Or install to current workspace
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git --workspace
```

### Method 2: Direct Copy

#### Copy to GitHub Copilot (VS Code)

**Linux / macOS (Bash)**
```bash
# 1. Clone this repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 2. Copy to your project (GitHub Copilot official recommended path .github/skills)
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# Done! VS Code v1.109+ has Agent Skills enabled by default, no additional setup needed
```

**Windows (PowerShell)**
```powershell
# 1. Clone this repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# 2. Copy to your project (GitHub Copilot official recommended path .github/skills)
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# Done! VS Code v1.109+ has Agent Skills enabled by default, no additional setup needed
```

> 💡 **v1.109+ Multi-tool sharing**: If you use both GitHub Copilot and Claude Code, just copy to `.claude/skills/` in one location, and both tools can access it. See [GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md) for details.

#### Copy to Claude Code

**Linux / macOS (Bash)**
```bash
# Copy to Claude Code workspace skills
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/

# Or copy to global skills
cp -r dotnet-testing-agent-skills/skills ~/.config/claude/
```

**Windows (PowerShell)**
```powershell
# Copy to Claude Code workspace skills
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.claude\" -Recurse

# Or copy to global skills
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "$env:APPDATA\claude\" -Recurse
```

### Method 3: Git Submodule

```bash
cd /your-project

# For GitHub Copilot: add submodule to .github/skills
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills .github/skills
cd .github/skills && cp -r skills/* . && cd ../..

# For Claude Code: add submodule to .claude/skills
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills .claude/skills
cd .claude/skills && cp -r skills/* . && cd ../..
```

### Method 4: Selective Copy

Only need specific skills?

#### Linux / macOS (Bash)

```bash
# Copy only unit testing fundamentals
cp -r dotnet-testing-agent-skills/skills/dotnet-testing-unit-test-fundamentals /your-project/.github/skills/

# Copy only AutoFixture series
cp -r dotnet-testing-agent-skills/skills/dotnet-testing-autofixture-* /your-project/.github/skills/

# Copy only overview skills
cp -r dotnet-testing-agent-skills/skills/dotnet-testing /your-project/.github/skills/
cp -r dotnet-testing-agent-skills/skills/dotnet-testing-advanced /your-project/.github/skills/
```

#### Windows (PowerShell)

```powershell
# Copy only unit testing fundamentals
Copy-Item -Path "dotnet-testing-agent-skills\skills\dotnet-testing-unit-test-fundamentals" -Destination "\your-project\.github\skills\" -Recurse

# Copy only AutoFixture series
Get-ChildItem -Path "dotnet-testing-agent-skills\skills\dotnet-testing-autofixture-*" | Copy-Item -Destination "\your-project\.github\skills\" -Recurse

# Copy only overview skills
Copy-Item -Path "dotnet-testing-agent-skills\skills\dotnet-testing" -Destination "\your-project\.github\skills\" -Recurse
Copy-Item -Path "dotnet-testing-agent-skills\skills\dotnet-testing-advanced" -Destination "\your-project\.github\skills\" -Recurse
```

### VS Code Settings

**VS Code v1.109+ (After 2026-02-04)**: Agent Skills is officially GA and **enabled by default**, no manual configuration needed.

**Before VS Code v1.109**: Need to manually enable Agent Skills support:

1. Open settings (`Ctrl+,` or `Cmd+,`)
2. Search for `chat.useAgentSkills`
3. Confirm it's checked to enable

> 💡 For more v1.109 update info (flexible search paths, multi-tool sharing, diagnostics tools, etc.), please refer to [GITHUB_COPILOT_UPDATE.md](GITHUB_COPILOT_UPDATE.md).

---

## Skills Overview

### 🎯 Overview Skills (2) - New!

> **NEW!** Two overview skills provide intelligent navigation. When you're unsure which skill to use, they automatically analyze requirements and recommend appropriate skill combinations.

| Skill | Description | When to Use |
|-------|-------------|-------------|
| `dotnet-testing` | Basic testing skills overview and guidance center | Automatically triggered when asking general questions like "How to write .NET tests", "Testing getting started", etc. |
| `dotnet-testing-advanced` | Advanced testing skills overview and guidance center | Automatically triggered when asking advanced questions like "Integration testing", "API testing", "Microservice testing", etc. |

**Overview Skills Value**:
- ✅ **Intelligent recommendations**: Recommend 1-4 most suitable sub-skill combinations based on your specific needs
- ✅ **Learning paths**: Provide progressive learning recommendations (beginner path, advanced path)
- ✅ **Decision support**: Quickly find needed skills through decision trees
- ✅ **Example-driven**: Each task has complete prompt examples

---

### Basic Skills (dotnet-testing) - 19 Skills

#### Phase 1: Testing Fundamentals and Assertions

| Skill Name | Description | Source |
| ---------- | ----------- | ------ |
| `dotnet-testing-unit-test-fundamentals` | Unit testing fundamentals, FIRST principles, 3A Pattern | Day 01 |
| `dotnet-testing-test-naming-conventions` | Three-part test naming conventions | Day 01 |
| `dotnet-testing-xunit-project-setup` | xUnit test project setup and structure | Day 02, 03 |
| `dotnet-testing-awesome-assertions-guide` | AwesomeAssertions fluent assertions guide | Day 04, 05 |
| `dotnet-testing-complex-object-comparison` | Complex object deep comparison techniques | Day 05 |
| `dotnet-testing-code-coverage-analysis` | Code coverage analysis and reports | Day 06 |
| `dotnet-testing-nsubstitute-mocking` | NSubstitute test doubles (Mock/Stub/Spy) | Day 07 |
| `dotnet-testing-test-output-logging` | xUnit ITestOutputHelper and ILogger integration | Day 08 |
| `dotnet-testing-private-internal-testing` | Private/Internal member testing strategies | Day 09 |
| `dotnet-testing-fluentvalidation-testing` | FluentValidation validator testing | Day 18 |

#### Phase 2: Testability Abstractions

| Skill Name | Description | Source |
| ---------- | ----------- | ------ |
| `dotnet-testing-datetime-testing-timeprovider` | TimeProvider time abstraction testing | Day 16 |
| `dotnet-testing-filesystem-testing-abstractions` | System.IO.Abstractions file system testing | Day 17 |

#### Phase 3: Test Data Generation and Integration

| Skill Name | Description | Source |
| ---------- | ----------- | ------ |
| `dotnet-testing-test-data-builder-pattern` | Manual Builder Pattern test data construction | Day 03 |
| `dotnet-testing-autofixture-basics` | AutoFixture basics and anonymous test data | Day 10 |
| `dotnet-testing-autofixture-customization` | AutoFixture customization strategies | Day 11 |
| `dotnet-testing-autodata-xunit-integration` | AutoData and xUnit integration | Day 12 |
| `dotnet-testing-autofixture-nsubstitute-integration` | AutoFixture + NSubstitute automatic mocking | Day 13 |
| `dotnet-testing-bogus-fake-data` | Bogus realistic test data generation | Day 14 |
| `dotnet-testing-autofixture-bogus-integration` | AutoFixture and Bogus integration | Day 15 |

### Advanced Skills (dotnet-testing-advanced)

#### Phase 4: Integration Testing

| Skill Name | Description | Source |
| ---------- | ----------- | ------ |
| `dotnet-testing-advanced-aspnet-integration-testing` | ASP.NET Core WebApplicationFactory integration testing | Day 19 |
| `dotnet-testing-advanced-testcontainers-database` | Testcontainers database containerized testing | Day 20, 21 |
| `dotnet-testing-advanced-testcontainers-nosql` | Testcontainers MongoDB/Redis testing | Day 22 |
| `dotnet-testing-advanced-webapi-integration-testing` | WebAPI complete integration testing workflow | Day 23 |
| `dotnet-testing-advanced-aspire-testing` | .NET Aspire Testing framework | Day 24, 25 |

#### Phase 5: Framework Migration Guide

| Skill Name | Description | Source |
| ---------- | ----------- | ------ |
| `dotnet-testing-advanced-xunit-upgrade-guide` | xUnit 2.9.x to 3.x upgrade guide | Day 26 |
| `dotnet-testing-advanced-tunit-fundamentals` | TUnit new generation testing framework introduction | Day 28 |
| `dotnet-testing-advanced-tunit-advanced` | TUnit data-driven and integration testing advanced | Day 29, 30 |

---

## How to Use

### Automatic Trigger Mode

Skills load automatically based on your conversation content. Just ask naturally:

```text
👤: Help me create an xUnit test project

🤖: [Automatically loads dotnet-testing-xunit-project-setup skill]
    I will help you create a standard xUnit test project structure...
```

### Common Trigger Phrases

#### Overview Skill Triggers

| What You Say | Triggered Skill |
| ------------ | --------------- |
| "I want to learn .NET testing" | `dotnet-testing` |
| "How to write .NET tests" | `dotnet-testing` |
| "Testing getting started" | `dotnet-testing` |
| "Create integration tests" | `dotnet-testing-advanced` |
| "How to do API testing" | `dotnet-testing-advanced` |
| "Microservice testing" | `dotnet-testing-advanced` |
| "Using Testcontainers" | `dotnet-testing-advanced` |

#### Specialized Skill Triggers

| What You Say | Triggered Skill |
| ------------ | --------------- |
| "Create test project" | `dotnet-testing-xunit-project-setup` |
| "Write unit test for this method" | `dotnet-testing-unit-test-fundamentals`, `dotnet-testing-test-naming-conventions` |
| "Generate test data" | `dotnet-testing-autofixture-basics` or `dotnet-testing-bogus-fake-data` |
| "This class has Mock requirements" | `dotnet-testing-nsubstitute-mocking` |
| "Check code coverage" | `dotnet-testing-code-coverage-analysis` |
| "Using Docker to test database" | `dotnet-testing-advanced-testcontainers-database` |

### Explicitly Specify Skill

You can also explicitly request to use a specific skill:

```text
👤: Use the dotnet-testing-test-naming-conventions skill to check my test naming

👤: Refer to the dotnet-testing-autofixture-nsubstitute-integration skill to refactor this test
```

---

## Detailed Skill Descriptions

### dotnet-testing-unit-test-fundamentals

**Purpose**: Create unit tests following best practices

**Core Content**:

- FIRST principles (Fast, Isolated, Repeatable, Self-validating, Timely)
- 3A Pattern (Arrange, Act, Assert)
- Testing pyramid concept
- Correct mindset for test coverage

**Trigger Scenarios**:

- Asking about unit testing fundamentals
- Requesting to write tests for a method
- Asking about testing best practices

---

### dotnet-testing-test-naming-conventions

**Purpose**: Create clear and consistent test naming

**Core Content**:

- Three-part naming convention: `MethodName_Scenario_ExpectedResult`
- Chinese naming recommendations
- Avoiding ambiguous names

**Example**:

```csharp
// ✅ Good naming
public void Add_InputPositiveNumbers_ReturnsCorrectSum()

// ❌ Names to avoid
public void TestAdd()
```

---

### dotnet-testing-xunit-project-setup

**Purpose**: Create standardized xUnit test projects

**Core Content**:

- Project structure recommendations
- .csproj configuration templates
- Required NuGet packages
- xunit.runner.json configuration

**Output**:

- Complete test project structure
- Correct package references
- Test execution configuration

---

### dotnet-testing-nsubstitute-mocking

**Purpose**: Create test doubles using NSubstitute

**Core Content**:

- Mock vs Stub vs Spy concepts
- NSubstitute basic syntax
- Return value setup
- Behavior verification
- Argument matchers

**Example**:

```csharp
var repository = Substitute.For<IUserRepository>();
repository.GetById(Arg.Any<int>()).Returns(new User { Name = "Test" });
repository.Received(1).GetById(1);
```

---

### dotnet-testing-autofixture-basics

**Purpose**: Use AutoFixture to automatically generate test data

**Core Content**:

- Fixture basic usage
- Create and CreateMany
- Anonymous variable concept
- Circular reference handling

**Applicable Scenarios**:

- Objects have multiple properties that need filling
- Tests don't care about specific data values
- Need large amounts of test data

---

### dotnet-testing-advanced-testcontainers-database

**Purpose**: Use Testcontainers for containerized database testing

**Core Content**:

- PostgreSQL/MSSQL container setup
- EF Core integration
- Dapper integration
- Test data isolation strategies
- Collection Fixture container sharing

**Prerequisites**:

- Docker Desktop
- WSL2 (Windows)

---

## Usage Scenario Examples

### Scenario 1: Create a New Test Project

**Your Need**: Create an xUnit test project from scratch

**Conversation Flow**:

```text
👤: I want to create a test project for MyProject

🤖: I will help you create a test project. According to the dotnet-testing-xunit-project-setup skill...

    1. Create project structure
    2. Configure csproj file
    3. Install required packages
    4. Setup test execution environment
```

### Scenario 2: Write Tests for Existing Method

**Your Need**: Write tests for Calculator.Add method

**Conversation Flow**:

```text
👤: Help me write unit tests for this method
    public int Add(int a, int b) => a + b;

🤖: According to the dotnet-testing-unit-test-fundamentals and dotnet-testing-test-naming-conventions skills,
    I will create tests following the 3A Pattern...

    [Fact]
    public void Add_InputPositiveNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();

        // Act
        var result = calculator.Add(2, 3);

        // Assert
        result.Should().Be(5);
    }
```

### Scenario 3: Refactor Tests to Use AutoFixture

**Your Need**: Simplify test data preparation

**Conversation Flow**:

```text
👤: This test has too much manual setup, can it be simplified?

🤖: According to the dotnet-testing-autofixture-basics skill, we can use AutoFixture 
    to automatically generate unimportant test data...

    [Theory, AutoData]
    public void CreateUser_WithValidData_ShouldCreateSuccessfully(User user)
    {
        // user is automatically generated by AutoFixture
        var result = _service.Create(user);
        result.Should().NotBeNull();
    }
```

### Scenario 4: Create Integration Test Environment

**Your Need**: Test requires real PostgreSQL database

**Conversation Flow**:

```text
👤: I want to use a real database to test Repository

🤖: According to the dotnet-testing-advanced-testcontainers-database skill, I will help you set up 
    Testcontainers environment...

    1. Install Testcontainers.PostgreSql package
    2. Create DatabaseFixture class
    3. Configure Collection Fixture
    4. Implement test base class
```

---

## Recommended Skill Combinations

> 💡 **Tip**: Unsure which skills to use? Just ask the overview skills (`dotnet-testing` or `dotnet-testing-advanced`), they will automatically recommend the most suitable skill combinations based on your needs!

### 🆕 Beginner Starter Pack

For developers just starting to learn .NET testing:

```text
1. dotnet-testing-unit-test-fundamentals    → Understand testing basics
2. dotnet-testing-test-naming-conventions   → Establish naming habits
3. dotnet-testing-xunit-project-setup       → Setup test environment
4. dotnet-testing-awesome-assertions-guide  → Learn fluent assertions
```

### 🎯 Quality Assurance Pack

For teams needing to improve test quality:

```text
1. dotnet-testing-code-coverage-analysis       → Monitor coverage
2. dotnet-testing-complex-object-comparison    → Precise assertions
3. dotnet-testing-test-output-logging          → Debugging support
4. dotnet-testing-fluentvalidation-testing     → Validation logic testing
```

### 🚀 Efficiency Boost Pack

For developers wanting to accelerate test development:

```text
1. dotnet-testing-autofixture-basics                    → Automated test data
2. dotnet-testing-autofixture-customization             → Customization strategies
3. dotnet-testing-autofixture-nsubstitute-integration   → Automatic Mock
4. dotnet-testing-autodata-xunit-integration            → Theory integration
```

### 🔗 Integration Testing Pack

For projects needing to establish integration tests:

```text
1. dotnet-testing-advanced-aspnet-integration-testing    → API testing basics
2. dotnet-testing-advanced-testcontainers-database       → Database containerization
3. dotnet-testing-advanced-testcontainers-nosql          → NoSQL testing
4. dotnet-testing-advanced-webapi-integration-testing    → Complete workflow
```

### 🔄 Framework Migration Pack

For teams planning to upgrade or migrate testing frameworks:

```text
1. dotnet-testing-advanced-xunit-upgrade-guide   → xUnit 3.x upgrade
2. dotnet-testing-advanced-tunit-fundamentals    → TUnit introduction
3. dotnet-testing-advanced-tunit-advanced       → TUnit advanced
```

---

## FAQ

### Q1: What if skills don't trigger automatically?

**A**: Confirm the following:

1. Skill directory structure is correct (GitHub Copilot uses `.github/skills/`, Claude Code uses `.claude/skills/`)
2. VS Code before v1.109 needs to confirm `chat.useAgentSkills` setting is enabled (v1.109+ enabled by default)
3. Each skill folder has a `SKILL.md` file
4. Try describing your needs more explicitly, or directly ask "I want to learn .NET testing" to trigger overview skills
5. Use VS Code v1.109+ Chat Customization Diagnostics tool to confirm skills loading status (right-click in Chat panel)

---

### Q2: Can I use multiple skills at once?

**A**: Yes, AI will automatically combine relevant skills based on conversation content. For example, when you ask "Write unit tests for this service", it may trigger:

- `dotnet-testing-unit-test-fundamentals` - Test structure
- `dotnet-testing-test-naming-conventions` - Naming conventions
- `dotnet-testing-nsubstitute-mocking` - Dependency mocking

---

### Q3: Do skills consume too many Tokens?

**A**: No. Agent Skills uses a "progressive loading" mechanism:

1. Normally only reads skill name and description (about 30-50 tokens)
2. Only loads full content when needed
3. Unused skills don't consume Tokens

---

### Q4: Can I modify skill content?

**A**: Of course! Skills are Markdown files, you can:

- Adjust recommendations according to team standards
- Add project-specific examples
- Remove inapplicable parts
- Add custom skills

---

### Q5: Do these skills work with NUnit or MSTest?

**A**: These skills are primarily designed for xUnit, but many concepts are universal:

- Test naming conventions
- 3A Pattern
- Test data generation strategies
- Mocking techniques

For NUnit/MSTest support, you can modify skill content or create new skills.

---

### Q6: How do I update to the latest version of skills?

**A**: Based on your installation method:

**npx skills install**: Re-run the installation command

```bash
# Update global skills
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Update workspace skills
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git --workspace
```

**Direct Copy**: Re-download and copy

```bash
# For GitHub Copilot
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# For Claude Code
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/
```

**Git Submodule**:

```bash
cd .github/skills  # or .claude/skills
git pull origin main
```

---

## Advanced Usage

### Create Custom Skills

You can create proprietary skills based on team needs:

```markdown
---
name: my-team-testing-standards
description: Our team's testing standards. Use this skill when asked to write tests.
---

# Team Testing Standards

## Naming Rules
- Use Chinese naming
- Must use three-part naming

## Required Assertions
- Use AwesomeAssertions
- Prohibit using Assert.Equal to directly compare objects

## Mocking Standards
- Use NSubstitute
- Prohibit using Moq
```

### Integrate with MCP Tools

Skills can teach AI how to use MCP tools:

```markdown
## Using Microsoft Learn MCP

When official documentation is needed:
1. Use microsoft_docs_search to search for relevant topics
2. Use microsoft_docs_fetch to get complete content
3. Use microsoft_code_sample_search to find code examples
```

### Skill Combination Files

Create a "combination skill" to integrate multiple related skills:

```markdown
---
name: complete-testing-workflow
description: Complete testing workflow, from unit testing to integration testing
---

# Complete Testing Workflow

This skill integrates the essence of the following skills:
- dotnet-testing-unit-test-fundamentals
- dotnet-testing-autofixture-basics
- dotnet-testing-nsubstitute-mocking
- dotnet-testing-advanced-testcontainers-database

## Workflow Steps
1. Confirm test type (unit/integration)
2. Setup test data
3. Create necessary mocks
4. Write tests
5. Verify coverage
```

---

## Related Resources

### Project Documentation

- **[SKILLS_QUICK_REFERENCE.md](SKILLS_QUICK_REFERENCE.md)**: Quick reference guide (Recommended!)
  - Keyword quick reference table
  - Prompt templates and usage examples
  - Common scenario quick guides
  - Skills optimization status explanation

- **[OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md)**: Skills optimization summary report
  - Detailed optimization strategies and results
  - Keywords integration explanation
  - Expected effects analysis

### Original Content

- **iThome Ironman Series**: [Old-School Software Engineer's Testing Practice - 30 Day Challenge](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
- **Complete Sample Code**: [30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)
- **Public Repository**: [dotnet-testing-agent-skills](https://github.com/kevintsengtw/dotnet-testing-agent-skills)

### Agent Skills Standards

- **Official Website**: [agentskills.io](https://agentskills.io)
- **GitHub Documentation**: [About Agent Skills](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)
- **Anthropic Skills**: [anthropics/skills](https://github.com/anthropics/skills)

### Skills Directory Structure

```text
skills/
├── dotnet-testing/                              # ⭐ Overview: Basic skills navigation (19 sub-skills)
├── dotnet-testing-advanced/                     # ⭐ Overview: Advanced skills navigation (8 sub-skills)
├── dotnet-testing-unit-test-fundamentals/
├── dotnet-testing-test-naming-conventions/
├── dotnet-testing-xunit-project-setup/
├── dotnet-testing-awesome-assertions-guide/
├── dotnet-testing-complex-object-comparison/
├── dotnet-testing-code-coverage-analysis/
├── dotnet-testing-nsubstitute-mocking/
├── dotnet-testing-test-output-logging/
├── dotnet-testing-private-internal-testing/
├── dotnet-testing-fluentvalidation-testing/
├── dotnet-testing-datetime-testing-timeprovider/
├── dotnet-testing-filesystem-testing-abstractions/
├── dotnet-testing-test-data-builder-pattern/
├── dotnet-testing-autofixture-basics/
├── dotnet-testing-autofixture-customization/
├── dotnet-testing-autodata-xunit-integration/
├── dotnet-testing-autofixture-nsubstitute-integration/
├── dotnet-testing-bogus-fake-data/
├── dotnet-testing-autofixture-bogus-integration/
├── dotnet-testing-advanced-aspnet-integration-testing/
├── dotnet-testing-advanced-testcontainers-database/
├── dotnet-testing-advanced-testcontainers-nosql/
├── dotnet-testing-advanced-webapi-integration-testing/
├── dotnet-testing-advanced-aspire-testing/
├── dotnet-testing-advanced-xunit-upgrade-guide/
├── dotnet-testing-advanced-tunit-fundamentals/
└── dotnet-testing-advanced-tunit-advanced/
```

> **Notes**:
> - Skills use flat structure, using prefix naming to distinguish basic skills (`dotnet-testing-*`) from advanced skills (`dotnet-testing-advanced-*`)
> - ⭐ Two overview skills provide intelligent navigation, automatically recommending suitable sub-skill combinations
> - After installation, skills will be copied to corresponding locations based on target environment (`.github/skills/` or `.claude/skills/`)

---

## License

MIT License

---

**Last Updated**: 2026-02-09
