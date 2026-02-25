---
id: dotnet-ui-testing-core
title: .NET UI Testing Core
description: Tests UI across frameworks. Page objects, test selectors, async waits, accessibility.
tags: ["testing", "dotnet", "ui-testing", "page-objects", "accessibility", "selectors"]
source: dotnet-artisan
---

# dotnet-ui-testing-core

Core UI testing patterns applicable across .NET UI frameworks (Blazor, MAUI, Uno Platform). Covers the page object model for maintainable test structure, test selector strategies for reliable element identification, async wait patterns for non-deterministic UI, and accessibility testing approaches.

**Version assumptions:** .NET 8.0+ baseline. Framework-specific details are delegated to dedicated skills.

## Scope

- Page object model for maintainable test structure
- Test selector strategies for reliable element identification
- Async wait patterns for non-deterministic UI
- Accessibility testing approaches

## Out of scope

- Blazor component testing (bUnit) -- see [skill:dotnet-blazor-testing]
- MAUI UI testing (Appium/XHarness) -- see [skill:dotnet-maui-testing]
- Uno Platform WASM testing -- see [skill:dotnet-uno-testing]
- Browser automation specifics -- see [skill:dotnet-playwright]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** A test project scaffolded via [skill:dotnet-add-testing]. Familiarity with test strategy decisions from [skill:dotnet-testing-strategy].

Cross-references: [skill:dotnet-testing-strategy] for deciding when UI tests are appropriate, [skill:dotnet-playwright] for browser-based E2E automation, [skill:dotnet-blazor-testing] for Blazor component testing, [skill:dotnet-maui-testing] for mobile/desktop UI testing, [skill:dotnet-uno-testing] for Uno Platform testing.

---

## Page Object Model

The page object model (POM) encapsulates page structure and interactions behind a class, isolating tests from UI implementation details. When the UI changes, only the page object needs updating -- not every test that touches that page.

### Structure

PageObjects/ directory contains page classes that represent application screens.

### Example: Generic Page Object Base

Base class for page objects. Subclass per framework: Playwright uses IPage, bUnit uses IRenderedComponent, Appium uses AppiumDriver.

### Page Object Principles

- Return the next page object from navigation actions.
- Never expose raw selectors from page objects.
- Keep assertions in tests, not page objects.
- Compose page objects from reusable components.

---

## Test Selector Strategies

Selectors determine how tests find UI elements. Fragile selectors are the leading cause of flaky UI tests.

### Selector Priority (Most to Least Reliable)

| Priority | Selector Type | Example | Reliability |
|----------|--------------|---------|-------------|
| 1 | data-testid | [data-testid='submit-btn'] | Highest |
| 2 | Accessibility role + name | GetByRole(AriaRole.Button, new() { Name = "Submit" }) | High |
| 3 | Label text | GetByLabel("Email address") | High |
| 4 | Placeholder text | GetByPlaceholder("Enter email") | Medium |
| 5 | CSS class | .btn-primary | Low |
| 6 | XPath / DOM structure | //div[3]/button[1] | Lowest |

### Adding Test IDs

Add data-testid attributes to elements that tests interact with. They are invisible to users and stable across refactors.

---

## Async Wait Strategies

UI tests deal with asynchronous rendering, network requests, and animations. Hardcoded delays cause flaky tests and slow suites.

### Wait Strategy Decision Tree

Check if element is in DOM, then check visibility/actionability, then wait appropriately.

### Framework-Specific Wait Patterns

Each framework has its own wait mechanisms: Playwright has auto-waiting, bUnit has WaitForState.

---

## Accessibility Testing

Accessibility testing verifies that UI components are usable by people with disabilities and compatible with assistive technologies.

### Automated Accessibility Checks with Playwright

Use Deque.AxeCore.Playwright to run accessibility audits.

### Accessibility Checklist for UI Tests

| Check | How to Test | Tool |
|-------|------------|------|
| Color contrast | Automated axe-core rule | Deque.AxeCore.Playwright |
| Keyboard navigation | Tab through all interactive elements | Playwright page.Keyboard |
| ARIA labels | Verify aria-label / aria-labelledby present | Playwright locators + assertions |
| Focus management | Verify focus moves to dialogs/modals | Playwright page.Locator(':focus') |
| Screen reader text | Verify aria-live regions update | Manual + assertion on ARIA attributes |

---

## Key Principles

- Use the page object model for any UI test suite with more than a handful of tests. The upfront cost pays for itself quickly in reduced maintenance.
- Prefer data-testid or accessibility-based selectors over CSS or DOM-structure selectors. Stable selectors are the single most effective defense against flaky tests.
- Never use Thread.Sleep or Task.Delay as a wait strategy. Use framework-native waits that poll for conditions with timeouts.
- Run accessibility checks as part of the standard test suite, not as a separate audit. Catching violations early prevents accessibility debt.
- Keep page objects framework-agnostic where possible. The patterns (POM, selector strategy, wait patterns) are universal; only the driver API changes between Playwright, bUnit, and Appium.

---

## Agent Gotchas

1. Do not add data-testid attributes to production code without team agreement. Some teams strip them in production builds.
2. Do not use WaitForTimeout (hardcoded delay) in Playwright tests. Use WaitForSelectorAsync or Expect(...).ToBeVisibleAsync() instead.
3. Do not assert on element count without waiting for the list to load. Use WaitForState or WaitForAssertion first.
4. Do not skip accessibility testing because "it's not a requirement." WCAG compliance is increasingly a legal requirement.
5. Do not create deeply nested page objects. If a page object has page objects inside page objects, flatten the hierarchy.

---

## References

- [Page Object Model (Martin Fowler)](https://martinfowler.com/bliki/PageObject.html)
- [Playwright Locators Best Practices](https://playwright.dev/dotnet/docs/locators)
- [axe-core Accessibility Rules](https://dequeuniversity.com/rules/axe/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
