# .NET Testing Skills Quick Reference Guide

> 💡 **For AI Agents and Developers**: This is a complete skills usage guide, including keyword mappings, usage examples, and best practices

---

## 📖 About This Guide

This guide integrates two important aspects:
- 🤖 **AI Agent Usage Guide**: Keyword mapping tables, workflow templates
- 👨‍💻 **Developer Quick Reference**: Prompt templates, scenario combination recommendations

**Important Reminders**:
- ✅ **Keywords Auto-Matching**: 29 skills have been optimized, with Keywords in the description. AI will automatically load based on conversation content
- ✅ **Entry Skills**: When unsure, use `dotnet-testing` or `dotnet-testing-advanced` for intelligent recommendations
- ✅ **Compliant with Official Specs**: Follows [agentskills.io](https://agentskills.io/specification) standard format

---

## 🎯 Overview Skills (Use this when unsure!)

| Skill Name | Purpose | When to Use |
|-----------|---------|-------------|
| `dotnet-testing` | Basic skills navigation (19 sub-skills) | General testing questions, unsure which basic skill to use |
| `dotnet-testing-advanced` | Advanced skills navigation (8 sub-skills) | Integration testing, API testing, microservices, and other advanced needs |

**Overview Skills Value**: Automatically analyze requirements, recommend 1-4 most suitable skill combinations, provide learning paths and examples.

---

## 🔍 Keyword Quick Reference Table

> 💡 **For AI Agents**: This table helps you quickly find the corresponding Skill. Since Keywords are already included in the description, most cases will auto-match.

| When User Mentions... | Use This Skill | Full Skill Path |
|-----------------------|----------------|-----------------|
| **Validator, Validation, FluentValidation, CreateUserValidator** | `dotnet-testing-fluentvalidation-testing` | `/skills/dotnet-testing/dotnet-testing-fluentvalidation-testing/` |
| **AutoFixture, Test Data Generation, CreateMany** | `dotnet-testing-autofixture-basics` | `/skills/dotnet-testing/dotnet-testing-autofixture-basics/` |
| **Mock, Mocking, NSubstitute, Substitute.For** | `dotnet-testing-nsubstitute-mocking` | `/skills/dotnet-testing/dotnet-testing-nsubstitute-mocking/` |
| **Time Testing, DateTime, TimeProvider, FakeTimeProvider** | `dotnet-testing-datetime-testing-timeprovider` | `/skills/dotnet-testing/dotnet-testing-datetime-testing-timeprovider/` |
| **File Testing, File, IFileSystem, MockFileSystem** | `dotnet-testing-filesystem-testing-abstractions` | `/skills/dotnet-testing/dotnet-testing-filesystem-testing-abstractions/` |
| **Bogus, Fake Data, Faker, Realistic Data** | `dotnet-testing-bogus-fake-data` | `/skills/dotnet-testing/dotnet-testing-bogus-fake-data/` |
| **Builder, Test Data Builder, WithXxx** | `dotnet-testing-test-data-builder-pattern` | `/skills/dotnet-testing/dotnet-testing-test-data-builder-pattern/` |
| **FluentAssertions, Should(), BeEquivalentTo** | `dotnet-testing-awesome-assertions-guide` | `/skills/dotnet-testing/dotnet-testing-awesome-assertions-guide/` |
| **Object Comparison, Deep Comparison, DTO Comparison** | `dotnet-testing-complex-object-comparison` | `/skills/dotnet-testing/dotnet-testing-complex-object-comparison/` |
| **API Testing, WebApplicationFactory, Integration Testing** | `dotnet-testing-advanced-aspnet-integration-testing` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-aspnet-integration-testing/` |
| **WebAPI Testing, CRUD Testing, Endpoint Testing** | `dotnet-testing-advanced-webapi-integration-testing` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-webapi-integration-testing/` |
| **Testcontainers, Container Testing, SQL Server Container** | `dotnet-testing-advanced-testcontainers-database` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-testcontainers-database/` |
| **MongoDB, Redis, Elasticsearch, NoSQL** | `dotnet-testing-advanced-testcontainers-nosql` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-testcontainers-nosql/` |
| **Aspire, Microservices, DistributedApplication** | `dotnet-testing-advanced-aspire-testing` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-aspire-testing/` |
| **xUnit Upgrade, xUnit 3.x** | `dotnet-testing-advanced-xunit-upgrade-guide` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-xunit-upgrade-guide/` |
| **TUnit, New Testing Framework** | `dotnet-testing-advanced-tunit-fundamentals` | `/skills/dotnet-testing-advanced/dotnet-testing-advanced-tunit-fundamentals/` |
| **Test Coverage, Coverlet, Code Coverage** | `dotnet-testing-code-coverage-analysis` | `/skills/dotnet-testing/dotnet-testing-code-coverage-analysis/` |
| **Private Members, internal, InternalsVisibleTo** | `dotnet-testing-private-internal-testing` | `/skills/dotnet-testing/dotnet-testing-private-internal-testing/` |
| **AutoData, [AutoData], Theory** | `dotnet-testing-autodata-xunit-integration` | `/skills/dotnet-testing/dotnet-testing-autodata-xunit-integration/` |
| **ITestOutputHelper, Test Output, ILogger** | `dotnet-testing-test-output-logging` | `/skills/dotnet-testing/dotnet-testing-test-output-logging/` |

---

## 📋 Complete Skills Catalog

### Basic Testing Skills (dotnet-testing) - 19 Skills

#### 🎓 Testing Fundamentals
1. `dotnet-testing-unit-test-fundamentals` - Unit testing fundamentals (FIRST principles, 3A Pattern)
2. `dotnet-testing-test-naming-conventions` - Test naming conventions
3. `dotnet-testing-xunit-project-setup` - xUnit project setup

#### 📦 Test Data Generation
4. `dotnet-testing-autofixture-basics` - AutoFixture basics
5. `dotnet-testing-autofixture-customization` - AutoFixture customization
6. `dotnet-testing-bogus-fake-data` - Bogus fake data generation
7. `dotnet-testing-test-data-builder-pattern` - Test Data Builder Pattern
8. `dotnet-testing-autofixture-bogus-integration` - AutoFixture + Bogus integration

#### 🎭 Test Doubles
9. `dotnet-testing-nsubstitute-mocking` - NSubstitute Mock framework
10. `dotnet-testing-autofixture-nsubstitute-integration` - AutoFixture + NSubstitute integration

#### ✅ Assertion Verification
11. `dotnet-testing-awesome-assertions-guide` - FluentAssertions fluent assertions
12. `dotnet-testing-complex-object-comparison` - Complex object comparison
13. `dotnet-testing-fluentvalidation-testing` - FluentValidation testing ⭐

#### 🔧 Special Scenarios
14. `dotnet-testing-datetime-testing-timeprovider` - Time-related testing
15. `dotnet-testing-filesystem-testing-abstractions` - File system testing
16. `dotnet-testing-private-internal-testing` - Private/internal member testing

#### 📊 Test Metrics
17. `dotnet-testing-code-coverage-analysis` - Code coverage analysis

#### 🔗 Framework Integration
18. `dotnet-testing-autodata-xunit-integration` - AutoData + xUnit integration
19. `dotnet-testing-test-output-logging` - Test output and logging

---

### Advanced Testing Skills (dotnet-testing-advanced) - 8 Skills

#### 🌐 Integration Testing
1. `dotnet-testing-advanced-aspnet-integration-testing` - ASP.NET Core integration testing
2. `dotnet-testing-advanced-webapi-integration-testing` - WebAPI complete testing
3. `dotnet-testing-advanced-testcontainers-database` - Testcontainers database testing
4. `dotnet-testing-advanced-testcontainers-nosql` - Testcontainers NoSQL testing

#### ☁️ Microservices Testing
5. `dotnet-testing-advanced-aspire-testing` - .NET Aspire testing

#### 🔄 Framework Migration
6. `dotnet-testing-advanced-xunit-upgrade-guide` - xUnit upgrade guide
7. `dotnet-testing-advanced-tunit-fundamentals` - TUnit fundamentals
8. `dotnet-testing-advanced-tunit-advanced` - TUnit advanced

---

## 💬 Recommended Prompt Templates

### Template 1: Explicitly Specify Skills

```text
Please refer to the {skill-name} skill to help me with {task description}
```

**Example**:

```text
Please refer to the dotnet-testing-nsubstitute-mocking skill to help me create a Mock Repository for UserService
```

### Template 2: Multiple Skills Combination

```text
Please use the following skills:
- {skill-1} - {purpose}
- {skill-2} - {purpose}
- {skill-3} - {purpose}

To help me with {task description}
```

### Template 3: Exploratory Inquiry

```text
I want to {task description}, please suggest which skills I should use?
```

---

## 🎯 Common Scenario Combinations

### Scenario 1: Create Test Project from Scratch

```text
Please use the following skills to help me create a complete test project:
1. dotnet-testing-xunit-project-setup - Create project structure
2. dotnet-testing-test-naming-conventions - Set naming conventions
3. dotnet-testing-unit-test-fundamentals - Create first test
```

### Scenario 2: Write Tests for Service with Dependencies

```text
Please use the following skills to create tests for this service class:
1. dotnet-testing-unit-test-fundamentals - Test structure
2. dotnet-testing-nsubstitute-mocking - Mock dependencies
3. dotnet-testing-autofixture-basics - Generate test data
4. dotnet-testing-awesome-assertions-guide - Write assertions
```

### Scenario 3: Create Integration Tests

```text
Please use the following skills to create complete integration tests:
1. dotnet-testing-advanced-testcontainers-database - Setup database container
2. dotnet-testing-advanced-aspnet-integration-testing - API testing basics
3. dotnet-testing-advanced-webapi-integration-testing - Complete workflow
```

---

## 🎯 Common Scenario Quick Guides

### Scenario 1: Create Tests for Validator
**Trigger Keywords**: Validator, Validation, CreateUserValidator, UpdateProductValidator

**Use Skill**:
```
/skill dotnet-testing-fluentvalidation-testing
```

**Why**: This skill specifically handles FluentValidation testing, including:
- Complete TestHelper usage
- ShouldHaveValidationErrorFor method
- ShouldNotHaveValidationErrorFor method
- Complex validation rule testing
- Custom validator testing

---

### Scenario 2: Need to Generate Large Amounts of Test Data
**Trigger Keywords**: Test Data, AutoFixture, CreateMany, Fake Data

**Use Skill**:
- Automated data → `dotnet-testing-autofixture-basics`
- Realistic data → `dotnet-testing-bogus-fake-data`
- Both combined → `dotnet-testing-autofixture-bogus-integration`

---

### Scenario 3: Have External Dependencies to Mock
**Trigger Keywords**: Mock, Mocking, Repository, Service Dependency

**Use Skill**:
```
/skill dotnet-testing-nsubstitute-mocking
```

If already using AutoFixture:
```
/skill dotnet-testing-autofixture-nsubstitute-integration
```

---

### Scenario 4: Testing Time-Related Logic
**Trigger Keywords**: DateTime, Time Testing, Expiration Time, TimeProvider

**Use Skill**:
```
/skill dotnet-testing-datetime-testing-timeprovider
```

---

### Scenario 5: Testing Web API
**Trigger Keywords**: API Testing, Controller Testing, Integration Testing, Endpoint Testing

**Use Skill**:
- Basic API testing → `dotnet-testing-advanced-aspnet-integration-testing`
- Complete CRUD testing → `dotnet-testing-advanced-webapi-integration-testing`
- Need real database → `dotnet-testing-advanced-testcontainers-database`

---

## 🚀 AI Agent Workflow Template

```
Step 1: Receive User Request
   └─> User: "Please help me create tests for CreateUserValidator"

Step 2: Keywords Auto-Match ✅
   └─> System automatically triggers based on Keywords "Validator" in description
   └─> Load `dotnet-testing-fluentvalidation-testing`

Step 3: Execute Skill Guidance ✅
   └─> Follow skill content to create tests
   └─> Use TestHelper
   └─> Write validation rule tests

Step 4: Respond to User ✅
   └─> Provide complete test code
   └─> Explain covered scenarios
```

**Important Reminder**: Since Keywords are already included in the description (compliant with official spec), in most cases AI will automatically load the correct skill without manual specification.

---

## 📚 Reference Resources

### Entry Guides
If unsure which skill to use, first check the entry guides:
- [Basic Testing Overview](/skills/dotnet-testing/SKILL.md)
- [Advanced Testing Overview](/skills/dotnet-testing-advanced/SKILL.md)

These files contain:
- Complete decision trees
- Learning path recommendations
- Common task mapping tables
- Skill combination recommendations

### Original Data Sources
- **iThome Ironman Series**: [Old-School Software Engineer's Testing Practice](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
- **Sample Code**: [30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)

---

## ⚠️ Important Reminders

### For AI Agents
1. **Keywords Auto-Matching is Most Important** - 29 skills have Keywords in description, will auto-trigger
2. **Follow Skill Guidance** - Each skill contains best practices and complete examples
3. **Use Entry Skill When Unsure** - `dotnet-testing` or `dotnet-testing-advanced` will provide intelligent recommendations
4. **Compliant with Official Spec** - Follow [agentskills.io](https://agentskills.io/specification) standard

### For Developers
1. **Explicitly Specify Skill** - If you know which skill to use, directly mention it in conversation
2. **Provide Keywords** - Provide more relevant keywords to help AI match skills
3. **Refer to Entry Guides** - When unsure, check dotnet-testing or dotnet-testing-advanced SKILL.md
4. **Use Prompt Templates** - Refer to template examples above to guide AI

---

## 📊 Skills Optimization Status

This skills collection has been fully optimized (2026-02-01), **compliant with agentskills.io official specification**:

### Format Specifications

- **29 Skills** fully optimized
- **description** includes core description + `Keywords:` keywords
- Removed non-standard `triggers` field, integrated into description

### Official Specification Fields

- `name` - skill name
- `description` - Function description and Keywords keywords (within 1024 characters)
- `license` - License information
- `metadata` - Additional metadata

Details please refer to: [OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md)

---

**Last Updated**: 2026-02-01
**Maintainer**: Kevin Tseng
**Version**: 2.1.0 (Compliant with official specification)
