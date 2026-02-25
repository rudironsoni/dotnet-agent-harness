Please also reference the following rules as needed. The list below is provided in TOON format, and `@` stands for the project root directory.

rules[2]:
  - path: @.gemini/memories/10-conventions.md
    description: "dotnet-harness-toolkit authoring conventions for skills, subagents, commands, and hooks"
    applyTo[1]: .rulesync/**/*
  - path: @.gemini/memories/20-workflow.md
    description: Workflow for RuleSync-based multi-agent generation
    applyTo[1]: **/*

# Additional Conventions Beyond the Built-in Functions

As this project's AI coding tool, you must follow the additional conventions below, in addition to the built-in functions.

# dotnet-harness-toolkit

Comprehensive .NET development skills for modern C#, ASP.NET, MAUI, Blazor, and cloud-native applications.

## Overview

**dotnet-harness-toolkit** is an agent-independent toolkit providing 131 skills and 14 specialist agents for .NET development. It follows the [Agent Skills](https://github.com/anthropics/agent-skills) open standard and is compatible with Claude Code, GitHub Copilot CLI, OpenCode, OpenAI Codex, and Gemini CLI.

## Installation

### Via RuleSync (Recommended)

Add to your `rulesync.jsonc`:

```jsonc
{
  "sources": [
    { "source": "rudironsoni/dotnet-harness-toolkit", "path": ".rulesync/skills" }
  ]
}
```

Then run:

```bash
npx rulesync install && npx rulesync generate
```

### Manual Installation

Clone and configure for your preferred agent:

- **Claude Code**: Use `CLAUDE.md`
- **GitHub Copilot**: Use `.github/copilot-instructions.md`
- **OpenCode**: Use `opencode.json`
- **Codex**: Use `.codex/config.json`
- **Gemini**: Use `.gemini/config.json`

## Skill Categories

- **Foundation** (4 skills): Core project analysis and detection
- **Core C#** (18 skills): Language patterns, async, concurrency
- **Project Structure** (7 skills): Scaffolding and organization
- **Architecture** (15 skills): Patterns, DI, messaging
- **Testing** (10 skills): xUnit, integration, Playwright
- **UI Frameworks** (14 skills): Blazor, MAUI, Uno, WPF
- **Native AOT** (4 skills): Trimming and compilation
- **Performance** (5 skills): Benchmarking and profiling
- **CI/CD** (8 skills): GitHub Actions and Azure DevOps
- **Documentation** (5 skills): Mermaid, XML docs
- ...and more (total: 131 skills)

## Agent Architecture

The central `dotnet-architect` agent analyzes context and delegates to specialists:

- `dotnet-blazor-specialist`
- `dotnet-maui-specialist`
- `dotnet-uno-specialist`
- `dotnet-csharp-concurrency-specialist`
- `dotnet-security-reviewer`
- `dotnet-performance-analyst`
- `dotnet-testing-specialist`
- `dotnet-cloud-specialist`
- ...and more (total: 14 agents)

## MCP Integration

Includes Serena MCP for semantic code analysis:

```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": ["--from", "git+https://github.com/oraios/serena", "serena", "start-mcp-server", "--context", "ide-assistant", "--project", "."]
    }
  }
}
```

## Hooks

Smart defaults for .NET development:
- **SessionStart**: Detect project context, inject advisor
- **PostToolUse**: Auto-format on save, suggest tests
- **UserPromptSubmit** (Claude only): Remind about advisor

## Contributing

See `CONTRIBUTING.md` for skill authoring guidelines.

## License

MIT License - See `LICENSE` file for details.

## Acknowledgements

- Original skills from [novotnyllc/dotnet-artisan](https://github.com/novotnyllc/dotnet-artisan)
- RuleSync for multi-agent configuration management
- Serena for semantic code analysis
