---
id: dotnet-integration-testing
title: .NET Integration Testing
description: Tests with real infrastructure. WebApplicationFactory, Testcontainers, Aspire, fixtures.
tags: ["testing", "dotnet", "integration-testing", "webapplicationfactory", "testcontainers", "aspire", "database"]
source: dotnet-artisan
---

# dotnet-integration-testing

Integration testing patterns for .NET applications using WebApplicationFactory, Testcontainers, and .NET Aspire testing. Covers in-process API testing, disposable infrastructure via containers, database fixture management, and test isolation strategies.

**Version assumptions:** .NET 8.0+ baseline, Testcontainers 3.x+, .NET Aspire 9.0+. Package versions for Microsoft.AspNetCore.Mvc.Testing must match the project's target framework major version (e.g., 8.x for net8.0, 9.x for net9.0, 10.x for net10.0). Examples below use Testcontainers 4.x APIs; the patterns apply equally to 3.x with minor namespace differences.

## Scope

- In-process API testing with WebApplicationFactory
- Disposable infrastructure via Testcontainers
- .NET Aspire distributed application testing
- Database fixture management and test isolation
- Authentication and authorization test setup

## Out of scope

- Test project scaffolding (creating projects, package references) -- see [skill:dotnet-add-testing]
- Testing strategy and test type selection -- see [skill:dotnet-testing-strategy]
- Snapshot testing for verifying API response structures -- see [skill:dotnet-snapshot-testing]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with integration test packages referenced. Docker daemon running (required by Testcontainers). Run [skill:dotnet-version-detection] to confirm .NET 8.0+ baseline.

Cross-references: [skill:dotnet-testing-strategy] for deciding when integration tests are appropriate, [skill:dotnet-xunit] for xUnit fixtures and parallel execution configuration, [skill:dotnet-snapshot-testing] for verifying API response structures with Verify.

---

## WebApplicationFactory

WebApplicationFactory<TEntryPoint> creates an in-process test server for ASP.NET Core applications. Tests send HTTP requests without network overhead, exercising the full middleware pipeline, routing, model binding, and serialization.

### Package

Microsoft.AspNetCore.Mvc.Testing - Version must match target framework: 8.x for net8.0, 9.x for net9.0, etc.

### Basic Usage

Tests create an HttpClient from the factory and make requests against the in-process server.

### Customizing the Test Server

Override services, configuration, or middleware using WithWebHostBuilder.

### Authenticated Requests

Test authenticated endpoints by configuring an authentication handler for the Test scheme.

---

## Testcontainers

Testcontainers spins up real infrastructure (databases, message brokers, caches) in Docker containers for tests. Each test run gets a fresh, disposable environment.

### Packages

- Testcontainers
- Testcontainers.PostgreSql
- Testcontainers.MsSql
- Testcontainers.Redis

### PostgreSQL Example

Create a fixture that starts a PostgreSQL container and provides the connection string.

### SQL Server Example

Create a fixture that starts a SQL Server container.

### Combining WebApplicationFactory with Testcontainers

The most common pattern: use Testcontainers for the database and WebApplicationFactory for the API.

---

## .NET Aspire Testing

.NET Aspire provides DistributedApplicationTestingBuilder for testing multi-service applications orchestrated with Aspire. This tests the actual distributed topology including service discovery, configuration, and health checks.

### Package

Aspire.Hosting.Testing

### Basic Aspire Test

Use DistributedApplicationTestingBuilder to create and test distributed applications.

---

## Database Fixture Patterns

### Per-Test Isolation with Transactions

Roll back each test's changes using a transaction scope.

### Per-Test Isolation with Respawn

Use Respawn to reset database state between tests by deleting data instead of rolling back transactions.

---

## Test Isolation Strategies

### Strategy Comparison

| Strategy | Speed | Isolation | Complexity | Best For |
|----------|-------|-----------|------------|----------|
| Transaction rollback | Fastest | High | Low | Tests that use a single DbContext |
| Respawn (data deletion) | Fast | High | Medium | Tests where code commits its own transactions |
| Fresh container per class | Slow | Highest | Low | Tests that modify schema or need complete isolation |
| Shared container + cleanup | Moderate | Medium | Medium | Test suites with many classes sharing infrastructure |

---

## Key Principles

- Use WebApplicationFactory for API tests. It is faster, more reliable, and more deterministic than testing against a deployed instance.
- Use Testcontainers for real infrastructure. Do not mock DbContext -- test against a real database to verify LINQ-to-SQL translation and constraint enforcement.
- Share containers across test classes via ICollectionFixture to avoid the overhead of starting a new container per class.
- Choose the right isolation strategy. Transaction rollback is fastest and simplest; use Respawn when you cannot control transaction boundaries.
- Always clean up test data. Leftover data from one test causes flaky failures in another. Use transaction rollback, Respawn, or fresh containers.
- Match Microsoft.AspNetCore.Mvc.Testing version to TFM. Using the wrong version causes runtime binding failures.

---

## Agent Gotchas

1. Do not hardcode Microsoft.AspNetCore.Mvc.Testing versions. The package version must match the project's target framework major version.
2. Do not forget InternalsVisibleTo for the Program class. Without it, WebApplicationFactory<Program> cannot access the entry point.
3. Do not use EnsureCreated() with Respawn. Use Database.MigrateAsync() for production schemas.
4. Do not dispose WebApplicationFactory before HttpClient. The factory owns the test server; disposing it invalidates all clients.
5. Do not use localhost ports with Testcontainers. Testcontainers maps random host ports to container ports. Always use GetConnectionString().
6. Do not skip Docker availability checks in CI. Testcontainers requires a running Docker daemon.

---

## References

- [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [.NET Aspire testing](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [Respawn](https://github.com/jbogard/Respawn)
