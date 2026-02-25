---
id: dotnet-uno-testing
title: .NET Uno Platform Testing
description: Tests Uno Platform apps. Playwright for WASM, platform-specific patterns, runtime heads.
tags: ["testing", "dotnet", "uno-platform", "playwright", "wasm", "cross-platform"]
source: dotnet-artisan
---

# dotnet-uno-testing

Testing Uno Platform applications across target heads (WASM, Desktop, Mobile). Covers Playwright-based browser automation for Uno WASM apps, platform-specific testing patterns for different runtime heads, and test infrastructure for cross-platform Uno projects.

**Version assumptions:** .NET 8.0+ baseline, Uno Platform 5.x+, Playwright 1.40+ for WASM testing. Uno Platform uses single-project structure with multiple target frameworks.

## Scope

- Playwright-based browser automation for Uno WASM apps
- Platform-specific testing patterns for different runtime heads
- Test infrastructure for cross-platform Uno projects

## Out of scope

- Shared UI testing patterns (page object model, selectors, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Playwright fundamentals (installation, CI caching, trace viewer) -- see [skill:dotnet-playwright]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** Uno Platform application with WASM head configured. For WASM testing: Playwright browsers installed (see [skill:dotnet-playwright]). For mobile testing: platform SDKs configured (Android SDK, Xcode).

Cross-references: [skill:dotnet-ui-testing-core] for page object model and selector strategies, [skill:dotnet-playwright] for Playwright installation, CI caching, and trace viewer, [skill:dotnet-uno-platform] for Uno Extensions, MVUX, Toolkit, and theme guidance, [skill:dotnet-uno-targets] for per-target deployment and platform-specific gotchas.

---

## Uno Testing Strategy by Head

Uno Platform apps run on multiple heads (WASM, Desktop/Skia, iOS, Android, Windows). Each head has different testing tools and trade-offs.

| Head | Testing Approach | Tool | Speed | Fidelity |
|------|-----------------|------|-------|----------|
| WASM | Browser automation | Playwright | Medium | High |
| Desktop (Skia/GTK, WPF) | UI automation | Appium / WinAppDriver | Medium | High |
| iOS | Simulator automation | Appium + XCUITest | Slow | Highest |
| Android | Emulator automation | Appium + UIAutomator2 | Slow | Highest |
| Unit (shared logic) | In-memory | xUnit (no UI) | Fast | N/A |

**Recommended priority:** Test shared business logic with unit tests first. Use Playwright against the WASM head for UI verification. Add platform-specific Appium tests only for behaviors that differ between heads.

---

## Playwright for Uno WASM

The WASM head renders Uno apps in a browser, making Playwright the natural choice for UI testing.

### Test Infrastructure

Fixture that starts the WASM app and initializes Playwright.

### WASM UI Tests

Tests that verify navigation and UI elements.

### Form Interaction Tests

Tests for form inputs and submissions.

---

## Platform-Specific Testing

### AutomationProperties for Cross-Platform Selectors

Uno maps AutomationProperties.AutomationId to each platform's native identifier.

Platform mapping:
- WASM: Rendered as data-testid attribute
- Android: Maps to content-desc
- iOS: Maps to accessibilityIdentifier
- Windows: Maps to AutomationId

### Testing Platform-Specific Code

For code that varies by platform, use conditional compilation and separate test classes.

---

## Runtime Head Validation

Validate that the same UI logic works correctly across different Uno runtime heads using shared test logic with platform-specific drivers.

### Shared Test Logic Pattern

Abstract base class that defines UI tests once, with concrete subclasses per platform.

---

## Key Principles

- Test shared logic with unit tests first. Uno's MVVM pattern means most business logic is testable without any UI framework.
- Use Playwright + WASM as the primary UI testing path. It is faster than mobile emulators.
- Use AutomationProperties.AutomationId on all testable controls. It is the only selector strategy that works identically across all Uno heads.
- Separate shared tests from platform-specific tests. Use abstract base classes for shared test logic, concrete subclasses per platform.
- Add platform-specific tests only for platform-divergent behavior.

---

## Agent Gotchas

1. Do not assume Uno WASM apps load instantly. The Uno runtime must initialize, which takes several seconds.
2. Do not use CSS selectors for Uno WASM elements. Use data-testid exclusively.
3. Do not forget to build the WASM head before running Playwright tests.
4. Do not test mobile-specific features in the WASM head. File system access, push notifications, biometrics, and NFC are not available in the browser.
5. Do not run all platform tests in a single CI job. Each platform requires its own SDK.

---

## References

- [Uno Platform Testing Documentation](https://platform.uno/docs/articles/features/working-with-accessibility.html)
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [Uno Platform WASM Head](https://platform.uno/docs/articles/getting-started/wizard/wasm.html)
