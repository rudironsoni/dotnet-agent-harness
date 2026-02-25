# dotnet-artisan RuleSync Toolkit

Agent-independent .NET skill toolkit ported from `novotnyllc/dotnet-artisan` and standardized with RuleSync.

## Goals

- One source of truth in `.rulesync/`
- Generate compatible outputs for Claude Code, OpenCode, Codex CLI, Gemini CLI, and GitHub Copilot CLI
- Keep skills, subagents, commands, hooks, and MCP definitions interoperable

## Repository Layout

- `.rulesync/skills/` - 131 .NET skills
- `.rulesync/subagents/` - 14 specialist subagents
- `.rulesync/commands/` - shared slash commands
- `.rulesync/hooks.json` + `.rulesync/hooks/` - lifecycle hooks
- `.rulesync/mcp.json` - MCP server definitions (Serena, Context7, Microsoft Docs, WinDbg)
- `.rulesync/rules/` - project rules and conventions
- `rulesync.jsonc` - generator configuration

## Install

```bash
npm install
npx rulesync validate
npx rulesync install
npx rulesync generate
```

## Declarative Sources

This repo supports RuleSync declarative sources in `rulesync.jsonc` (`sources` array). It is currently prepared for future upstream pull-in but defaults to local source content in `.rulesync/skills`.

## Serena MCP Onboarding

Use the `/init-project` command to initialize project context and run Serena onboarding checks.

## Attribution

Original content source: `https://github.com/novotnyllc/dotnet-artisan`
