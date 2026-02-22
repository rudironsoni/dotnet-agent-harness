# Learning Path Recommendations

> This document is extracted from `SKILL.md`, providing complete learning path planning for beginners and advanced users.

---

### Beginner Path (1-2 Weeks)

**Goal**: Establish testing fundamentals, able to write basic unit tests

#### Phase 1: Build Foundation (Day 1-5)

**Day 1-2: Testing Fundamentals**
- Skill: `dotnet-testing-unit-test-fundamentals`
- Learning focus:
  - FIRST principles
  - 3A Pattern
  - [Fact] and [Theory] usage
  - Basic assertion methods
- Hands-on practice: Write tests for a simple Calculator class

**Day 3: Naming Conventions**
- Skill: `dotnet-testing-test-naming-conventions`
- Learning focus:
  - Three-part naming method
  - Chinese naming suggestions
  - Test class naming
- Hands-on practice: Rename tests from Day 1-2

**Day 4-5: Set Up Test Project**
- Skill: `dotnet-testing-xunit-project-setup`
- Learning focus:
  - Project structure planning
  - .csproj configuration
  - NuGet package management
  - xunit.runner.json configuration
- Hands-on practice: Create test project for existing project

#### Phase 2: Improve Quality (Day 6-10)

**Day 6-7: Fluent Assertions**
- Skill: `dotnet-testing-awesome-assertions-guide`
- Learning focus:
  - FluentAssertions basics
  - Collection assertions
  - Exception assertions
  - Object comparison
- Hands-on practice: Rewrite previous tests using FluentAssertions

**Day 8: Test Output**
- Skill: `dotnet-testing-test-output-logging`
- Learning focus:
  - ITestOutputHelper usage
  - Integrating ILogger
  - Debugging techniques
- Hands-on practice: Add output to complex tests

**Day 9-10: Handle Dependencies**
- Skill: `dotnet-testing-nsubstitute-mocking`
- Learning focus:
  - Mock vs Stub vs Spy
  - Basic mocking setup
  - Verify calls
  - Returns and Throws
- Hands-on practice: Test Service with Repository dependency

#### Phase 3: Automated Test Data (Day 11-14)

**Day 11-12: AutoFixture Basics**
- Skill: `dotnet-testing-autofixture-basics`
- Learning focus:
  - Anonymous test data
  - Create and CreateMany
  - Reduce test boilerplate code
- Hands-on practice: Simplify tests using AutoFixture

**Day 13-14: Integration Application**
- Skills:
  - `dotnet-testing-autodata-xunit-integration`
  - `dotnet-testing-autofixture-nsubstitute-integration`
- Learning focus:
  - AutoData attributes
  - Auto-create Mock
  - Combined usage
- Hands-on practice: Comprehensive application of AutoFixture + NSubstitute

#### Phase 4: Summary and Practice (Day 15)

- Create complete tests for a small project
- Apply all learned skills
- Set up Code Coverage

---

### Advanced Path (2-3 Weeks)

**Prerequisite**: Complete beginner path

#### Week 1: Test Data Mastery

**Day 1-2: Advanced AutoFixture**
- Skill: `dotnet-testing-autofixture-customization`
- Learning focus:
  - Customizations
  - Specimens
  - Behaviors
  - Custom generation rules

**Day 3-4: Realistic Data**
- Skill: `dotnet-testing-bogus-fake-data`
- Learning focus:
  - Faker usage
  - Realistic data generation
  - Localized data

**Day 5: Builder Pattern**
- Skill: `dotnet-testing-test-data-builder-pattern`
- Learning focus:
  - Create Test Data Builder
  - Fluent Interface
  - Default value settings

**Day 6-7: Integrated Application**
- Skill: `dotnet-testing-autofixture-bogus-integration`
- Learning focus:
  - Integrate AutoFixture with Bogus
  - Choose appropriate tools

#### Week 2: Special Scenarios

**Day 1-2: Time Testing**
- Skill: `dotnet-testing-datetime-testing-timeprovider`
- Learning focus:
  - TimeProvider abstraction
  - FakeTimeProvider usage
  - Timezone handling

**Day 3-4: File System Testing**
- Skill: `dotnet-testing-filesystem-testing-abstractions`
- Learning focus:
  - IFileSystem interface
  - MockFileSystem usage
  - File operation testing

**Day 5-6: Complex Object Comparison**
- Skill: `dotnet-testing-complex-object-comparison`
- Learning focus:
  - Deep object comparison
  - Exclude specific properties
  - Custom comparison rules

**Day 7: Private Member Testing**
- Skill: `dotnet-testing-private-internal-testing`
- Learning focus:
  - InternalsVisibleTo
  - Testing strategies
  - When to avoid

#### Week 3: Quality and Integration

**Day 1-2: Validation Testing**
- Skill: `dotnet-testing-fluentvalidation-testing`
- Learning focus:
  - TestHelper usage
  - Validation rule testing
  - Error message validation

**Day 3-4: Code Coverage**
- Skill: `dotnet-testing-code-coverage-analysis`
- Learning focus:
  - Coverlet usage
  - Report generation
  - CI/CD integration

**Day 5-7: Comprehensive Practice**
- Create complete tests for a medium-sized project
- Apply all learned skills
- Create testing best practices documentation

---

### Quick Reference: What Should I Learn First?

**I'm new, never written tests before**
→ Beginner Path Phase 1: unit-test-fundamentals → test-naming-conventions → xunit-project-setup

**I can write basic tests, want to improve quality**
→ Beginner Path Phase 2: awesome-assertions-guide → nsubstitute-mocking

**I want to improve testing efficiency**
→ Advanced Path Week 1: Test data generation series

**I encounter special scenarios (time, file system)**
→ Advanced Path Week 2: Special scenario handling series

**I want to evaluate test quality**
→ code-coverage-analysis
