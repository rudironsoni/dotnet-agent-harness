# Guided Conversation Examples

The following examples show how AI interacts with you to help you choose the correct advanced skills.

### Example 1: API Testing Requirements

**You**: I want to test Web API

**AI**: I'll help you set up Web API testing! Please first confirm your testing requirements:

**1. API Complexity**:
- [ ] Simple API (few endpoints)
- [ ] Medium API (full CRUD)
- [ ] Complex API (multi-layer architecture, complex business logic)

**2. Database Requirements**:
- [ ] No database needed
- [ ] In-memory database is sufficient
- [ ] Need real database

**3. Database Type** (if real database needed):
- [ ] SQL Server / PostgreSQL / MySQL
- [ ] MongoDB / Redis / Elasticsearch
- [ ] Both

Please tell me your choices, and I'll recommend the appropriate skill combination.

---

**You**: Medium API, need real SQL Server database

**AI**: Perfect! Based on your requirements, I recommend the following skill combination:

**Step 1: Learn Basic Integration Testing (1-2 days)**
`dotnet-testing-advanced-aspnet-integration-testing`
- Understand WebApplicationFactory
- Learn HTTP testing basics

**Step 2: Add Real Database (2 days)**
`dotnet-testing-advanced-testcontainers-database`
- Use Testcontainers to set up SQL Server
- Learn database test isolation

**Step 3: Complete Process Testing (2-3 days)**
`dotnet-testing-advanced-webapi-integration-testing`
- Implement complete CRUD tests
- Test data management strategies
- Error handling validation

**Expected Outcome**:
After completion, you'll be able to create complete integration test suites for API projects using real SQL Server databases to verify behavior.

Would you like me to help you get started with these skills?

---

### Example 2: Microservices Testing

**You**: My project is .NET Aspire microservices architecture, how do I test it?

**AI**: .NET Aspire microservices testing requires special approaches! I recommend using the `dotnet-testing-advanced-aspire-testing` skill.

**Prerequisites**:
1. Have you installed .NET Aspire Workload?
   - If not: `dotnet workload install aspire`

2. What services does your project have?
   - API Service
   - Worker Service
   - Other services

3. Are you using databases?
   - If yes, we'll combine with `dotnet-testing-advanced-testcontainers-database`

**Testing Focus**:
- DistributedApplication testing
- Service-to-service communication validation
- Dependency service management
- End-to-end process testing

Would you like me to use the `dotnet-testing-advanced-aspire-testing` skill to help you create tests?

---

### Example 3: Framework Upgrade

**You**: My project uses xUnit 2.9, should I upgrade to 3.x?

**AI**: That's a great question! I'll help you evaluate. Let me first understand your situation:

**1. Upgrade Motivation**:
- [ ] Need xUnit 3.x new features
- [ ] Resolve known issues
- [ ] Just want to keep up with latest version
- [ ] Other reasons

**2. Project Scale**:
- [ ] Small (< 100 tests)
- [ ] Medium (100-500 tests)
- [ ] Large (> 500 tests)

**3. Upgrade Timeline Pressure**:
- [ ] Not urgent, can migrate slowly
- [ ] Need to complete quickly

---

**You**: Medium project, mainly want to keep up with latest version, not urgent

**AI**: Understood! I recommend using the `dotnet-testing-advanced-xunit-upgrade-guide` skill.

**Upgrade Plan**:

**Phase 1: Understand Differences (1 day)**
- Read breaking changes list
- Understand which features are affected
- Assess migration effort

**Phase 2: Prepare for Upgrade (half day)**
- Backup current code
- Update .csproj package versions
- Check compilation errors

**Phase 3: Handle Compatibility (1-2 days)**
- Fix compilation errors
- Handle runtime issues
- Verify all tests pass

**Estimated Total Time**: 2-3 days

**Risk Assessment**: ⭐⭐ Low-Medium
xUnit 3.x upgrades are usually smooth, most changes are backward compatible.

Would you like me to use the `dotnet-testing-advanced-xunit-upgrade-guide` skill to help you start the upgrade?

---

### Example 4: TUnit Evaluation

**You**: I heard TUnit is pretty good, should I migrate?

**AI**: TUnit is indeed a promising testing framework! Let me help you evaluate.

**TUnit Main Advantages**:
1. ⚡ **Better Performance**: Parallel execution optimization
2. 💉 **Native Dependency Injection**: No additional setup needed
3. 🎯 **Modernized API**: More intuitive design
4. 🔧 **Flexibility**: More customization options

**Evaluation Focus**:

**1. Project Status**:
- New project → Can consider using TUnit directly
- Existing project → Need to assess migration cost

**2. Team Skills**:
- Team's familiarity with xUnit
- Willingness to learn new frameworks

**3. Migration Cost**:
- Number of tests
- Special test patterns (like Theory, ClassFixture, etc.)

**My Recommendations**:

**If New Project**:
Use `dotnet-testing-advanced-tunit-fundamentals` skill to learn about TUnit, consider adopting it directly.

**If Existing Project**:
1. First use `dotnet-testing-advanced-tunit-fundamentals` to understand basics
2. Experimental migration of one test file
3. Assess actual migration workload
4. Decide whether to fully migrate

Would you like me to help you evaluate?
