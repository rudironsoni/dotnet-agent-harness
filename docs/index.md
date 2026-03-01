---
layout: home

hero:
  name: 'dotnet-agent-harness'
  text: 'Comprehensive .NET Development Toolkit'
  tagline:
    131 skills, 15 subagents, and expert guidance for modern C#, ASP.NET Core, MAUI, Blazor, and cloud-native apps
  image:
    src: /logo.png
    alt: dotnet-agent-harness
  actions:
    - theme: brand
      text: Get Started
      link: /guide/installation
    - theme: alt
      text: View Skills
      link: /skills/
    - theme: alt
      text: GitHub
      link: https://github.com/rudironsoni/dotnet-agent-harness

features:
  - icon: 🎯
    title: 131 Specialized Skills
    details: Deep expertise across the entire .NET ecosystem, from C# coding standards to cloud deployment patterns.

  - icon: 🤖
    title: 15 AI Subagents
    details: Specialized agents for MAUI, Blazor, Security, Performance, and more - each with focused tool profiles.

  - icon: 🛠️
    title: 15 Slash Commands
    details: Powerful commands for research, testing, profiling, and documentation generation.

  - icon: 🔌
    title: MCP Integration
    details: Seamless integration with Serena, Context7, Microsoft Learn, GitHub, and Docker MCP servers.

  - icon: 🐳
    title: Docker & GitHub Actions
    details: Containerized toolkit with official GitHub Actions for CI/CD integration.

  - icon: 🔍
    title: Semantic Search
    details: Find the right skill instantly with AI-powered semantic search across descriptions and tags.
---

## Quick Start

```bash
# Install rulesync
npm install -g rulesync

# Fetch the toolkit
rulesync fetch rudironsoni/dotnet-agent-harness:.rulesync

# Generate for your platform
rulesync generate --targets "*" --features "*"
```

## Platform Support

<div class="platforms">

| Platform       | Status             | Primary Agent      |
| -------------- | ------------------ | ------------------ |
| Claude Code    | ✅ Fully Supported | `dotnet-architect` |
| OpenCode       | ✅ Fully Supported | `dotnet-architect` |
| GitHub Copilot | ✅ Fully Supported | All agents         |
| Codex CLI      | ✅ Fully Supported | All agents         |
| Gemini CLI     | ✅ Fully Supported | All agents         |

</div>

## Architecture Overview

```mermaid
graph TB
    subgraph "dotnet-agent-harness Toolkit"
        SKILLS[131 Skills]
        SUBAGENTS[15 Subagents]
        COMMANDS[15 Commands]
        MCP[5 MCP Servers]
    end

    subgraph "Distribution"
        CLAUDE[.claude/]
        OPENCODE[.opencode/]
        COPILOT[.github/]
        CODEX[.codex/]
    end

    SKILLS --> CLAUDE
    SKILLS --> OPENCODE
    SKILLS --> COPILOT
    SKILLS --> CODEX
    SUBAGENTS --> CLAUDE
    SUBAGENTS --> OPENCODE
    COMMANDS --> CLAUDE
    COMMANDS --> OPENCODE
```

## Latest Enhancements

### Phase 4 Complete 🎉

- **5 New Commands**: `/dotnet-agent-harness:search`, `/dotnet-agent-harness:test`, `/dotnet-agent-harness:profile`,
  `/dotnet-agent-harness:graph`, `/dotnet-agent-harness:compare`
- **VS Code Extension**: Real-time validation and autocomplete
- **Skill Manifest**: Dependency tracking and version resolution
- **Quick-Start Templates**: web-api, blazor-app, maui-mobile, clean-arch, console, classlib
- **Offline Mode**: Air-gapped environment support
- **Docker & GitHub Actions**: Containerized toolkit

[View Full Changelog](/guide/changelog)
