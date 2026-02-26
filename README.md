# dotnet-harness-toolkit

An agent-independent .NET skills toolkit, ported from `novotnyllc/dotnet-artisan`, distributed via RuleSync.

## Why this repository exists

AI coding CLIs all use different file formats and conventions (`CLAUDE.md`, Copilot instructions, Codex config, OpenCode config, Gemini config, etc.). Maintaining separate copies of the same rules and skills quickly becomes brittle.

This repository solves that by using **RuleSync** as the source of truth.

- Define once in `.rulesync/`
- Generate for multiple agent CLIs
- Keep skills, agents, commands, hooks, and MCP config consistent across tools

## Goals

- Make .NET guidance reusable across Claude Code, OpenCode, Codex CLI, Gemini CLI, GitHub Copilot CLI, and Antigravity
- Keep frontmatter and metadata interoperable across agent ecosystems
- Provide a strong .NET default workflow with onboarding and routing
- Preserve and evolve the dotnet-harness-toolkit knowledge base in a portable format

## Quick Start

Add this repository as a declarative source in your `rulesync.jsonc`:

```jsonc
{
  "sources": [
    { "source": "rudironsoni/dotnet-harness-toolkit", "path": ".rulesync" }
  ]
}
```

Then install and generate:

```bash
npx rulesync install && npx rulesync generate
```

This will install all 131 skills, 14 agents, hooks, commands, and MCP configuration into your project.

## What's included

### Skills (131 total)

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
- ...and more

See `AGENTS.md` for a complete skill listing.

### Agents (14 total)

- `dotnet-architect` - Main orchestrator for user invocation
- `dotnet-blazor-specialist`
- `dotnet-maui-specialist`
- `dotnet-uno-specialist`
- `dotnet-csharp-concurrency-specialist`
- `dotnet-security-reviewer`
- `dotnet-performance-analyst`
- `dotnet-testing-specialist`
- `dotnet-cloud-specialist`
- ...and more

See `AGENTS.md` for complete agent descriptions.

## RuleSync Architecture

### Source-of-truth files

- `rulesync.jsonc` - generation targets, features, and declarative sources
- `.rulesync/rules/` - global and scoped rule documents
- `.rulesync/skills/` - portable skill definitions
- `.rulesync/subagents/` - portable agent definitions
- `.rulesync/commands/` - shared slash commands
- `.rulesync/hooks.json` + `.rulesync/hooks/` - lifecycle hooks
- `.rulesync/mcp.json` - MCP servers (Serena, Context7, Microsoft Docs, WinDbg)

### Generated outputs

RuleSync generates tool-specific files from the source above:
- `.claude/*` for Claude Code
- `.github/*` for GitHub Copilot
- `.codex/*` for Codex CLI
- `.gemini/*` for Gemini CLI
- `opencode.json` for OpenCode
- `.agent/*` for Antigravity

### Workspace files

The root-level configuration files (CLAUDE.md, opencode.json, GEMINI.md, etc.) in this repository are auto-generated from `.rulesync/`. They serve as living examples of what RuleSync produces and are used for development of this toolkit itself.

## How to use RuleSync in this repo

### 1) Install dependencies

```bash
npm install
```

### 2) Install declarative sources (if configured)

```bash
npx rulesync install
```

### 3) Generate all configured targets

```bash
npx rulesync generate
```

### 4) Run validation

```bash
npm run ci:rulesync
```

## Hooks and onboarding

- Session and post-edit hooks are defined in `.rulesync/hooks.json`
- Hook scripts live in `.rulesync/hooks/`
- `/init-project` initializes .NET context and Serena onboarding checks

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

## CI/CD

- `.github/workflows/rulesync-validate.yml` - Validates RuleSync configuration

## Contributing

1. Edit only `.rulesync/*`, `rulesync.jsonc`, and automation under `scripts/`
2. Run `npm run ci:rulesync`
3. Commit source changes

See `CONTRIBUTING.md` for skill authoring guidelines.

## Attribution

- Original skills and agent corpus: `https://github.com/novotnyllc/dotnet-artisan`
- RuleSync: `https://github.com/dyoshikawa/rulesync`

## License

MIT License - See `LICENSE` file for details.
