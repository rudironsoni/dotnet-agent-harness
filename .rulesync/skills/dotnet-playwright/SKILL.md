---
id: dotnet-playwright
title: .NET Playwright Testing
description: Automates browser tests in .NET. Playwright E2E, CI browser caching, trace viewer, codegen.
tags: ["testing", "dotnet", "playwright", "e2e-testing", "browser-automation", "ci"]
source: dotnet-artisan
---

# dotnet-playwright

Playwright for .NET: browser automation and end-to-end testing. Covers browser lifecycle management, page interactions, assertions, CI caching of browser binaries, trace viewer for debugging failures, and codegen for rapid test scaffolding.

**Version assumptions:** Playwright 1.40+ for .NET, .NET 8.0+ baseline. Playwright supports Chromium, Firefox, and WebKit browsers.

## Scope

- Browser lifecycle management (Chromium, Firefox, WebKit)
- Page interactions and locator-based assertions
- CI caching of browser binaries
- Trace viewer for debugging test failures
- Codegen for rapid test scaffolding

## Out of scope

- Shared UI testing patterns (page object model, selectors, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Testing strategy (when E2E vs unit vs integration) -- see [skill:dotnet-testing-strategy]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** Test project scaffolded via [skill:dotnet-add-testing] with Playwright packages referenced. Browsers installed via pwsh bin/Debug/net8.0/playwright.ps1 install or dotnet tool run playwright install.

Cross-references: [skill:dotnet-ui-testing-core] for page object model and selector strategies, [skill:dotnet-testing-strategy] for deciding when E2E tests are appropriate.

---

## Package Setup

Microsoft.Playwright package with xUnit or NUnit integration.

### Installing Browsers

Playwright requires downloading browser binaries before tests can run.

---

## Basic Test Structure

### With Playwright xUnit Base Class

PageTest provides Page, Browser, BrowserContext, and Playwright properties.

### Manual Setup (Without Base Class)

Create Playwright, Browser, Context, and Page manually in IAsyncLifetime.

---

## Locators and Interactions

### Recommended Locator Strategies

| Priority | Locator | Example |
|----------|---------|---------|
| 1 | Role-based | Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }) |
| 2 | Test ID | Page.Locator("[data-testid='email-input']") |
| 3 | Label text | Page.GetByLabel("Full Name") |
| 4 | Placeholder | Page.GetByPlaceholder("Search...") |

### Common Interactions

Fill, click, select options, checkboxes, file upload, keyboard, hover.

### Assertions (Expect API)

Playwright assertions auto-retry until the condition is met or the timeout expires.

---

## Network Interception

### Mocking API Responses

Intercept API calls and return mock data.

### Waiting for Network Requests

Wait for specific API responses after actions.

---

## CI Browser Caching

Downloading browser binaries on every CI run is slow (500MB+). Cache them to speed up builds.

### GitHub Actions Caching

Cache the ~/.cache/ms-playwright directory.

### Azure DevOps Caching

Use the Cache task with appropriate key.

---

## Trace Viewer

Playwright's trace viewer captures a full recording of test execution for debugging failures.

### Enabling Traces

Start tracing before each test and stop on completion.

### Viewing Traces

Open trace files with Playwright CLI or upload to trace.playwright.dev.

---

## Codegen

Playwright's code generator records browser interactions and generates test code.

### Running Codegen

Open codegen with your app URL to generate test code.

---

## Key Principles

- Use Playwright assertions (Expect) instead of raw xUnit Assert. Playwright assertions auto-retry.
- Cache browser binaries in CI. Downloading 500MB+ of browsers per run wastes time.
- Enable trace viewer for debugging CI failures.
- Use codegen to bootstrap tests, then refine.
- Prefer role-based or data-testid locators over CSS classes or XPath.

---

## Agent Gotchas

1. Do not forget to install browsers after adding the Playwright package.
2. Do not use Task.Delay for waiting. Playwright's auto-waiting handles timing automatically.
3. Do not hardcode localhost ports. Use configuration or environment variables.
4. Do not skip --with-deps on first CI install. Playwright browsers need system libraries.
5. Do not store trace files in the repository. Write them to a test-results directory that is git-ignored.

---

## References

- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Playwright Locators](https://playwright.dev/dotnet/docs/locators)
- [Playwright Assertions](https://playwright.dev/dotnet/docs/test-assertions)
- [Playwright Trace Viewer](https://playwright.dev/dotnet/docs/trace-viewer)
