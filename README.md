# dotnet-harness-toolkit

An agent-independent .NET skills toolkit, ported from `novotnyllc/dotnet-artisan`, with a RuleSync-first architecture.

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

## Distribution modes

- Plugin/extension bundles: `claudecode`, `copilot`, `geminicli`, `opencode`
- Manual bundles: `codexcli`, `antigravity`
- CD writes all distributables to `plugins/{agent}` on merges to `main`

See `docs/install.md` for install commands per target.

## What is in this repo

- **131 skills** in `.rulesync/skills/`
- **14 subagents** in `.rulesync/subagents/`
- **1 main user-invocable orchestrator agent**: `dotnet-architect`
- **13 specialist subagents** configured as non-user-invocable
- Shared commands, hooks, rules, and MCP definitions for multi-agent workflows

## RuleSync architecture

### Source-of-truth files

- `rulesync.jsonc` - generation targets, features, and declarative sources
- `.rulesync/rules/` - global and scoped rule documents
- `.rulesync/skills/` - portable skill definitions
- `.rulesync/subagents/` - portable agent definitions
- `.rulesync/commands/` - shared slash commands
- `.rulesync/hooks.json` + `.rulesync/hooks/` - lifecycle hooks
- `.rulesync/mcp.json` - MCP servers (Serena, Context7, Microsoft Docs, WinDbg)

### Generated outputs

RuleSync generates tool-specific files from the source above (for example `.claude/*`, `.github/*`, `.codex/*`, `.gemini/*`, `.agent/*`, `opencode.json`, and related files).

CD then packages these generated outputs into distributable bundles under `plugins/`:

- `plugins/claudecode`
- `plugins/copilot`
- `plugins/geminicli`
- `plugins/opencode`
- `plugins/codexcli`
- `plugins/antigravity`

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

### 4) Run full local validation

```bash
npm run ci:rulesync
npm run ci:bundles
```

## Contributor workflow

1. Edit only `.rulesync/*`, `rulesync.jsonc`, and bash automation under `scripts/`
2. Run `npm run ci:rulesync`
3. Run `npm run ci:bundles`
4. Commit source changes (plugin bundles are committed by CD on `main`)

## Declarative sources

This repo is configured to support RuleSync declarative source ingestion via `sources` in `rulesync.jsonc`.

Use this when you want to import skills from upstream repos without manual copying. Example patterns are included in `rulesync.jsonc` comments.

## Hooks and onboarding

- Session and post-edit hooks are defined in `.rulesync/hooks.json`
- Hook scripts live in `.rulesync/hooks/`
- `/init-project` initializes .NET context and Serena onboarding checks

## Agent model

- `dotnet-architect` is the main entry agent for user invocation
- All other agents are specialist subagents and are non-user-invocable
- Routing relies on foundation skills such as:
  - `dotnet-advisor`
  - `dotnet-version-detection`
  - `dotnet-project-analysis`

## CI/CD

GitHub workflows validate and package RuleSync outputs:

- `.github/workflows/rulesync-validate.yml`
  - Validates RuleSync generation in an isolated workspace
  - Builds bundles with bash-only scripts
  - Validates manifests and runs install/runtime smoke checks
- `.github/workflows/rulesync-generate.yml`
  - Runs on merge/push to `main`
  - Rebuilds and validates bundles
  - Commits only `plugins/**` back to the repository

## Attribution

- Original skills and agent corpus: `https://github.com/novotnyllc/dotnet-artisan`
- RuleSync: `https://github.com/dyoshikawa/rulesync`
