# Skill Taxonomy Schema

This document defines the taxonomy classification system for the 189+ skills in dotnet-agent-harness.

## Overview

The taxonomy provides:
- **Discoverability**: Find skills by category, subcategory, or tags
- **Routing**: Navigate from high-level needs to specific implementations
- **Consistency**: Standardized classification across all skills
- **Extensibility**: Clear patterns for adding new skills

---

## Top-Level Categories (10)

| Category | Code | Description | Count |
|----------|------|-------------|-------|
| **fundamentals** | FND | Core C#/.NET language and runtime skills | ~25 |
| **testing** | TST | Unit, integration, and testing methodology | ~35 |
| **architecture** | ARC | Design patterns, principles, and system design | ~15 |
| **web** | WEB | ASP.NET Core, APIs, Blazor, web frameworks | ~20 |
| **data** | DAT | Data access, EF Core, caching, messaging | ~15 |
| **performance** | PERF | Optimization, benchmarking, profiling | ~10 |
| **security** | SEC | Security, OWASP, cryptography, auth | ~10 |
| **devops** | DEV | CI/CD, containers, deployment, automation | ~20 |
| **platforms** | PLAT | UI frameworks: MAUI, WPF, WinUI, Uno | ~20 |
| **tooling** | TOOL | CLI, analyzers, MSBuild, NuGet, docs | ~15 |

---

## Subcategory Definitions

### fundamentals (FND)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| coding-standards | CS | Naming, style, conventions | dotnet-csharp-coding-standards |
| language-patterns | LP | C# features, async, modern patterns | dotnet-csharp-modern-patterns |
| design-principles | DP | SOLID, DRY, clean code | dotnet-solid-principles |
| dependency-injection | DI | DI containers, lifetimes, scopes | dotnet-csharp-dependency-injection |
| configuration | CFG | Options pattern, configuration | dotnet-csharp-configuration |
| diagnostics | DIA | Debugging, analyzers, code smells | dotnet-csharp-code-smells |

### testing (TST)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| fundamentals | FUND | FIRST principles, 3A pattern, basics | dotnet-testing-unit-test-fundamentals |
| frameworks | FW | xUnit, TUnit, test runners | dotnet-xunit |
| assertions | AST | Assertion libraries, fluent assertions | dotnet-testing-awesome-assertions-guide |
| mocking | MOCK | NSubstitute, test doubles, isolation | dotnet-testing-nsubstitute-mocking |
| test-data | DATA | AutoFixture, Bogus, builders | dotnet-testing-autofixture-basics |
| integration | INT | WebApplicationFactory, Testcontainers | dotnet-integration-testing |
| specialized | SPEC | Time, filesystem, private testing | dotnet-testing-datetime-testing-timeprovider |
| coverage | COV | Code coverage, quality metrics | dotnet-testing-code-coverage-analysis |

### architecture (ARC)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| patterns | PAT | Vertical slices, pipelines, result pattern | dotnet-architecture-patterns |
| domain-modeling | DM | DDD, aggregates, value objects | dotnet-domain-modeling |
| messaging | MSG | Event-driven, pub/sub, outbox | dotnet-messaging-patterns |
| resilience | RES | Retry, circuit breaker, timeout | dotnet-resilience |

### web (WEB)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| minimal-apis | MIN | Minimal API patterns, filters | dotnet-minimal-apis |
| mvc | MVC | Controllers, views, traditional MVC | - |
| blazor | BLA | Blazor components, patterns, auth | dotnet-blazor-patterns |
| api-design | API | REST, versioning, OpenAPI | dotnet-api-versioning |
| security | SEC | Auth, JWT, CORS, rate limiting | dotnet-api-security |
| validation | VAL | Input validation, FluentValidation | dotnet-input-validation |

### data (DAT)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| ef-core | EF | DbContext, patterns, architecture | dotnet-efcore-patterns |
| data-access | ACC | Dapper, ADO.NET, strategy | dotnet-data-access-strategy |
| caching | CACHE | Memory, distributed, HybridCache | - |
| messaging | QUEUE | Channels, message queues | dotnet-channels |
| serialization | SER | JSON, Protobuf, MessagePack | dotnet-serialization |
| search | SRCH | Elasticsearch, search patterns | - |

### performance (PERF)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| patterns | PAT | Span, ArrayPool, struct optimization | dotnet-performance-patterns |
| memory | MEM | GC tuning, LOH, memory management | dotnet-gc-memory |
| benchmarking | BENCH | BenchmarkDotNet, measurement | dotnet-benchmarkdotnet |
| profiling | PROF | Profiling tools, diagnostics | dotnet-profiling |
| aot | AOT | Native AOT, trimming, WASM | dotnet-native-aot |

### security (SEC)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| owasp | OWASP | OWASP Top 10, vulnerabilities | dotnet-security-owasp |
| crypto | CRYP | Encryption, hashing, algorithms | dotnet-cryptography |
| auth | AUTH | Identity, OIDC, passkeys | dotnet-api-security |
| secrets | SECR | Secret management, rotation | dotnet-secrets-management |

### devops (DEV)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| github-actions | GHA | GitHub Actions workflows | dotnet-gha-build-test |
| azure-devops | ADO | Azure DevOps pipelines | dotnet-ado-build-test |
| containers | CONT | Docker, containerization | dotnet-containers |
| deployment | DEPLOY | Kubernetes, container deployment | dotnet-container-deployment |
| release | REL | Versioning, changelogs, releases | dotnet-release-management |
| ci-cd | CICD | General CI/CD patterns | dotnet-add-ci |

### platforms (PLAT)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| maui | MAUI | .NET MAUI mobile/desktop | dotnet-maui-development |
| wpf | WPF | WPF modern patterns | dotnet-wpf-modern |
| winui | WINUI | WinUI 3 desktop apps | dotnet-winui |
| uno | UNO | Uno Platform cross-platform | dotnet-uno-platform |
| winforms | WIN | WinForms modernization | dotnet-winforms-basics |
| blazor | BLA | Blazor web assembly/server | dotnet-blazor-patterns |

### tooling (TOOL)

| Subcategory | Code | Description | Examples |
|-------------|------|-------------|----------|
| cli | CLI | CLI architecture, System.CommandLine | dotnet-cli-architecture |
| analyzers | ANZ | Roslyn analyzers, code fixes | dotnet-roslyn-analyzers |
| msbuild | MSB | MSBuild authoring, tasks | dotnet-msbuild-authoring |
| nuget | NUG | NuGet authoring, packaging | dotnet-nuget-authoring |
| project | PROJ | Project structure, scaffolding | dotnet-project-structure |
| docs | DOCS | Documentation generation | dotnet-api-docs |
| serena | SPN | Serena MCP integration | dotnet-serena-code-navigation |
| rulesync | SYNC | RuleSync tool documentation | rulesync |

---

## Tagging Conventions

### Complexity Tags

| Tag | Description | Target Audience |
|-----|-------------|-----------------|
| `beginner` | Entry-level, foundational concepts | Developers new to .NET |
| `intermediate` | Practical application, integration | Working developers |
| `advanced` | Expert patterns, edge cases, optimization | Senior/architects |

### Domain Tags

| Tag | Description |
|-----|-------------|
| `csharp` | C# language-specific |
| `dotnet` | .NET runtime/framework |
| `aspnetcore` | ASP.NET Core specific |
| `web` | Web development |
| `mobile` | Mobile development |
| `desktop` | Desktop applications |
| `cloud` | Cloud-native patterns |
| `microservices` | Microservice architecture |
| `ai` | AI/ML integration |
| `mcp` | Model Context Protocol |

### Skill Type Tags

| Tag | Description |
|-----|-------------|
| `overview` | Navigation/meta skill with sub-skills |
| `guide` | Tutorial-style comprehensive guidance |
| `reference` | Quick reference, lookup documentation |
| `patterns` | Design patterns and best practices |
| `integration` | Integration with external systems |
| `testing` | Testing-related content |
| `security` | Security-focused |
| `performance` | Performance-focused |

### Target Platform Tags

| Tag | Description |
|-----|-------------|
| `claudecode` | Claude Code compatible |
| `opencode` | OpenCode compatible |
| `codexcli` | Codex CLI compatible |
| `copilot` | GitHub Copilot compatible |
| `geminicli` | Gemini CLI compatible |
| `antigravity` | Antigravity compatible |
| `factorydroid` | Factory Droid compatible |
| `all-targets` | Universal compatibility |

---

## Frontmatter Schema

Skills MUST include the following frontmatter fields for taxonomy:

```yaml
---
name: skill-name
description: Clear, concise description
license: MIT
targets: ['*']
category: fundamentals          # Top-level category
tags:                           # Array of tags
  - csharp
  - dotnet
  - skill
  - intermediate                 # Complexity
subcategory: coding-standards   # Specific subcategory
complexity: intermediate         # beginner | intermediate | advanced
version: '1.0.0'
author: 'Author Name'
related_skills:                  # Comma-separated or array
  - dotnet-csharp-modern-patterns
  - dotnet-solid-principles
# ... platform-specific blocks
---
```

### Field Requirements

| Field | Required | Values |
|-------|----------|--------|
| `category` | Yes | One of 10 top-level categories |
| `subcategory` | Yes | Valid subcategory for the category |
| `complexity` | Yes | beginner, intermediate, advanced |
| `tags` | Yes | Array including complexity + domain |
| `related_skills` | Recommended | Related skill names |

---

## Category Meta-Skills

High-level navigation skills that reference all skills in a category:

| Meta-Skill | Description | References |
|------------|-------------|------------|
| `dotnet-fundamentals` | Core language and runtime | All FND skills |
| `dotnet-testing` | Testing methodology and tools | All TST skills |
| `dotnet-architecture` | Design patterns and architecture | All ARC skills |
| `dotnet-performance` | Optimization and benchmarking | All PERF skills |
| `dotnet-security` | Security and hardening | All SEC skills |
| `dotnet-web` | Web development (optional) | All WEB skills |
| `dotnet-data` | Data access and storage (optional) | All DAT skills |
| `dotnet-devops` | CI/CD and deployment (optional) | All DEV skills |

---

## Classification Examples

### Example 1: Foundation Skill
```yaml
category: fundamentals
subcategory: coding-standards
tags: [csharp, dotnet, skill, beginner]
complexity: beginner
```

### Example 2: Testing Skill
```yaml
category: testing
subcategory: integration
tags: [dotnet, testing, integration, intermediate]
complexity: intermediate
```

### Example 3: Architecture Skill
```yaml
category: architecture
subcategory: patterns
tags: [dotnet, architecture, patterns, advanced]
complexity: advanced
```

### Example 4: Security Skill
```yaml
category: security
subcategory: owasp
tags: [dotnet, security, owasp, advanced]
complexity: advanced
```

---

## Maintenance

### Adding New Skills

1. Determine the appropriate category and subcategory
2. Set complexity based on target audience
3. Include relevant domain and skill type tags
4. Add `related_skills` for discoverability
5. Update INDEX.md with the new skill

### Updating Taxonomy

1. Propose changes via PR with rationale
2. Update TAXONOMY.md with new categories/subcategories
3. Update affected skills' frontmatter
4. Update INDEX.md references
5. Update category meta-skills if needed

---

## Quick Reference

### Category Codes
- FND = fundamentals
- TST = testing
- ARC = architecture
- WEB = web
- DAT = data
- PERF = performance
- SEC = security
- DEV = devops
- PLAT = platforms
- TOOL = tooling

### Complexity Distribution Goal
- ~30% beginner
- ~50% intermediate
- ~20% advanced
