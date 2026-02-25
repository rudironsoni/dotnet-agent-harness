---
id: dotnet-blazor-testing
title: .NET Blazor Testing
description: Tests Blazor components. bUnit rendering, events, cascading params, JS interop mocking.
tags: ["testing", "dotnet", "blazor", "bunit", "components", "javascript-interop"]
source: dotnet-artisan
---

# dotnet-blazor-testing

bUnit testing for Blazor components. Covers component rendering and markup assertions, event handling, cascading parameters and cascading values, JavaScript interop mocking, and async component lifecycle testing. bUnit provides an in-memory Blazor renderer that executes components without a browser.

**Version assumptions:** .NET 8.0+ baseline, bUnit 1.x (stable). Examples use the latest bUnit APIs. bUnit supports both Blazor Server and Blazor WebAssembly components.

## Scope

- bUnit component rendering and markup assertions
- Event handling and user interaction simulation
- Cascading parameters and cascading values
- JavaScript interop mocking
- Async component lifecycle testing

## Out of scope

- Browser-based E2E testing of Blazor apps -- see [skill:dotnet-playwright]
- Shared UI testing patterns (page object model, selectors, wait strategies) -- see [skill:dotnet-ui-testing-core]
- Test project scaffolding -- see [skill:dotnet-add-testing]

**Prerequisites:** A Blazor test project scaffolded via [skill:dotnet-add-testing] with bUnit packages referenced. The component under test must be in a referenced Blazor project.

Cross-references: [skill:dotnet-ui-testing-core] for shared UI testing patterns (POM, selectors, wait strategies), [skill:dotnet-xunit] for xUnit fixtures and test organization, [skill:dotnet-blazor-patterns] for hosting models and render modes, [skill:dotnet-blazor-components] for component architecture and state management.

---

## Package Setup

bUnit with xUnit integration.

bUnit test classes inherit from TestContext (or use it via composition).

---

## Component Rendering

### Basic Rendering and Markup Assertions

Render components and assert on text content and specific elements.

### Rendering with Child Content

Test components that accept child content or RenderFragment parameters.

### Rendering with Dependency Injection

Register services before rendering components that depend on them.

---

## Event Handling

### Click Events

Simulate button clicks and verify state changes.

### Form Input Events

Simulate typing and form submissions.

### EventCallback Parameters

Test components that invoke callbacks.

---

## Cascading Parameters

### CascadingValue Setup

Provide cascading values to components that expect them.

### Named Cascading Values

Test components with named cascading parameters.

---

## JavaScript Interop Mocking

Blazor components that call JavaScript via IJSRuntime require mock setup in bUnit. bUnit provides a built-in JS interop mock.

### Basic JSInterop Setup

Set up expected JS calls and verify they were made.

### JSInterop with Return Values

Mock JS calls that return values to the component.

### Catch-All JSInterop Mode

Use loose mode for components with many JS calls.

---

## Async Component Lifecycle

### Testing OnInitializedAsync

Test components that load data asynchronously.

### Testing Error States

Test component behavior when async operations fail.

---

## Key Principles

- Render components in isolation. bUnit tests individual components without a browser, making them fast and deterministic.
- Register all dependencies before rendering. Any service the component injects via [Inject] must be registered in Services before RenderComponent is called.
- Use WaitForState and WaitForAssertion for async components. Do not use Task.Delay.
- Mock JS interop explicitly. Unhandled JS interop calls throw by default in bUnit strict mode.
- Test the rendered output, not component internals. Assert on markup, text content, and element attributes.

---

## Agent Gotchas

1. Do not forget to register services before RenderComponent. bUnit throws at render time if an [Inject]-ed service is missing.
2. Do not use cut.Instance to access private members. Instance exposes the component's public API only.
3. Do not forget to call cut.WaitForState() after triggering async operations.
4. Do not mix bUnit and Playwright in the same test class. They serve different purposes and have incompatible lifecycles.
5. Do not forget cascading values for components that expect them. A component with [CascadingParameter] will receive null if no CascadingValue is provided.

---

## References

- [bUnit Documentation](https://bunit.dev/)
- [bUnit Getting Started](https://bunit.dev/docs/getting-started/)
- [bUnit JS Interop](https://bunit.dev/docs/test-doubles/emulating-ijsruntime)
- [Blazor Component Testing](https://learn.microsoft.com/en-us/aspnet/core/blazor/test)
