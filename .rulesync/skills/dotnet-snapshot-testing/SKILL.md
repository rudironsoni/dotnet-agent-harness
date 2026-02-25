---
id: dotnet-snapshot-testing
title: .NET Snapshot Testing
description: Verifies complex outputs with Verify. API responses, scrubbing non-deterministic values.
tags: ["testing", "dotnet", "snapshot-testing", "verify", "approval-testing", "api-testing"]
source: dotnet-artisan
---

# dotnet-snapshot-testing

Snapshot (approval) testing with the Verify library for .NET. Covers verifying API responses, serialized objects, rendered emails, and other complex outputs by comparing them against approved baseline files. Includes scrubbing and filtering patterns to handle non-deterministic values (dates, GUIDs, timestamps), custom converters for domain-specific types, and strategies for organizing and reviewing snapshot files.

**Version assumptions:** Verify 20.x+ (.NET 8.0+ baseline). Examples use the Verify.Xunit integration package; equivalent packages exist for NUnit and MSTest.

## Scope

- Verify library setup and snapshot lifecycle
- Scrubbing and filtering non-deterministic values (dates, GUIDs)
- Custom converters for domain-specific types
- Organizing and reviewing snapshot files
- Integration with xUnit, NUnit, and MSTest

## Out of scope

- Test project scaffolding -- see [skill:dotnet-add-testing]
- Testing strategy -- see [skill:dotnet-testing-strategy]
- Integration test infrastructure -- see [skill:dotnet-integration-testing]

**Prerequisites:** Test project already scaffolded via [skill:dotnet-add-testing] with Verify packages referenced. .NET 8.0+ baseline required.

Cross-references: [skill:dotnet-testing-strategy] for deciding when snapshot tests are appropriate, [skill:dotnet-integration-testing] for combining Verify with WebApplicationFactory and Testcontainers.

---

## Setup

### Packages

Verify.Xunit and Verify.Http for HTTP response verification.

### Module Initializer

Verify requires a one-time initialization per test assembly.

### Source Control

Add *.received.* to .gitignore. Add .gitattributes for line endings.

---

## Basic Usage

### Verifying Objects

Verify serializes the object to JSON and compares against a .verified.txt file.

### Verifying Strings and Streams

Verify HTML, XML, or any string content.

---

## Scrubbing and Filtering

Non-deterministic values (dates, GUIDs, auto-incremented IDs) change between test runs. Scrubbing replaces them with stable placeholders.

### Built-In Scrubbers

Verify includes scrubbers for common non-deterministic types.

### Custom Scrubbers

Add custom scrubbers for application-specific patterns.

### Ignoring Members

Exclude specific properties from verification.

---

## Verifying HTTP Responses

Verify HTTP responses from WebApplicationFactory integration tests.

### Setup

Use Verify.Http package.

### Verifying Full HTTP Responses

Capture status code, headers, and body.

---

## Verifying Rendered Emails

Snapshot-test email templates by verifying the rendered HTML output.

---

## Custom Converters

Custom converters control how specific types are serialized for verification.

### Writing a Custom Converter

Implement WriteOnlyJsonConverter<T> for domain types.

---

## Snapshot File Organization

### Default Naming

Verify names snapshot files based on the test class and method.

### Unique Directory

Move verified files into a dedicated directory.

### Parameterized Tests

For [Theory] tests, Verify appends parameter values to the file name.

---

## Workflow: Accepting Changes

When a snapshot test fails, Verify creates a .received.txt file. Review the diff and accept or reject.

### Diff Tool Integration

Verify launches a diff tool automatically when a test fails.

### CLI Acceptance

Use verify accept command to accept pending changes.

---

## Key Principles

- Snapshot test complex outputs, not simple values. Use Assert.Equal for single values.
- Scrub all non-deterministic values. Dates, GUIDs, and timestamps must be scrubbed.
- Commit .verified.txt files to source control. These are the approved baselines.
- Review snapshot diffs carefully. Accepting changes without review can approve regressions.
- Use custom converters for domain readability.
- Keep snapshots focused. Use IgnoreMember to exclude volatile fields.

---

## Agent Gotchas

1. Do not forget [UsesVerify] on the test class.
2. Do not commit .received.txt files. Add *.received.* to .gitignore.
3. Do not skip UseParameters() in parameterized tests.
4. Do not scrub values that are part of the contract.
5. Do not use snapshot testing for rapidly evolving APIs.

---

## References

- [Verify GitHub repository](https://github.com/VerifyTests/Verify)
- [Verify documentation](https://github.com/VerifyTests/Verify/blob/main/docs/readme.md)
- [Verify.Http](https://github.com/VerifyTests/Verify.Http)
