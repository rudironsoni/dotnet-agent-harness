---
id: dotnet-maui-testing
title: .NET MAUI Testing
description: Tests .NET MAUI apps. Appium device automation, XHarness, platform validation.
tags: ["testing", "dotnet", "maui", "appium", "xharness", "mobile-testing", "ui-testing"]
source: dotnet-artisan
---

# dotnet-maui-testing

Testing .NET MAUI applications using Appium for UI automation and XHarness for cross-platform test execution. Covers device and emulator testing, platform-specific behavior validation, element location strategies for MAUI controls, and test infrastructure for mobile/desktop apps.

**Version assumptions:** .NET 8.0+ baseline, Appium 2.x with UIAutomator2 (Android) and XCUITest (iOS) drivers, XHarness 1.x. Examples use the latest Appium .NET client (5.x+).

## Scope

- Appium 2.x UI automation for Android, iOS, and Windows
- XHarness cross-platform test execution
- Platform-specific behavior validation
- Element location strategies for MAUI controls
- Test infrastructure for mobile/desktop apps

## Out of scope

- Shared UI testing patterns (page object model, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Browser-based testing -- see [skill:dotnet-playwright]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** MAUI test project scaffolded via [skill:dotnet-add-testing]. Appium server installed (npm install -g appium). For Android: Android SDK with emulator configured. For iOS: Xcode with simulator (macOS only). For Windows: WinAppDriver installed.

Cross-references: [skill:dotnet-ui-testing-core] for page object model, test selectors, and async wait patterns, [skill:dotnet-xunit] for xUnit fixtures and test organization, [skill:dotnet-maui-development] for MAUI project structure, XAML/MVVM patterns, and platform services, [skill:dotnet-maui-aot] for Native AOT on iOS/Mac Catalyst and AOT build testing considerations.

---

## Appium Setup for MAUI

### Packages

Appium.WebDriver, xunit.v3, xunit.runner.visualstudio.

### Driver Initialization

Configure Appium driver for Android, iOS, or Windows based on target platform.

### Test Configuration

Use environment variables or test runsettings for platform-specific paths and settings.

---

## Element Location with AutomationId

MAUI's AutomationId property maps to the platform-native accessibility identifier. This is the most reliable selector for cross-platform tests.

### Setting AutomationId in XAML

Add AutomationId attributes to controls in XAML.

### Finding Elements in Tests

Use MobileBy.AccessibilityId to find elements by their automation ID.

---

## Page Object Model for MAUI

Apply the page object model pattern with Appium's driver.

---

## Platform-Specific Behavior Testing

### Conditional Tests by Platform

Use Assert.SkipWhen to skip tests on unsupported platforms.

### Screen Size and Orientation

Test responsive layouts by changing device orientation.

---

## XHarness Test Execution

XHarness is a command-line tool for running tests on devices and emulators across platforms. It handles app installation, test execution, and result collection.

### Running Tests with XHarness

Commands for running tests on Android and iOS.

### XHarness with Device Runner

For xUnit tests running directly on device, add the device runner NuGet package.

---

## Key Principles

- Use AutomationId for all testable elements. It is the cross-platform equivalent of data-testid.
- Run tests against real emulators/simulators, not just unit tests.
- Use explicit waits, never implicit waits or delays.
- Tag platform-specific tests with [Trait] and Assert.SkipWhen.
- Apply the page object model for maintainability.

---

## Agent Gotchas

1. Do not use FindElement without a wait strategy. Elements may not be available immediately after navigation.
2. Do not hardcode emulator/simulator names. Use environment variables or test configuration.
3. Do not forget to set AutomationId on MAUI controls. Without it, Appium falls back to platform-specific selectors.
4. Do not run iOS tests on non-macOS machines. iOS simulators require Xcode.
5. Do not leave the Appium server unmanaged. Start Appium as a fixture or CI service.

---

## References

- [Appium Documentation](https://appium.io/docs/en/latest/)
- [Appium .NET Client](https://github.com/appium/dotnet-client)
- [.NET MAUI Testing](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/uitest)
- [XHarness](https://github.com/dotnet/xharness)
