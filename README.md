# dotnet-agent-harness

Comprehensive .NET skills, subagents, commands, hooks, and MCP config for AI coding tools, maintained in RuleSync
format.

## What this repository provides

- 131 .NET-focused skills
- 14 specialist subagents/agents
- shared commands, hooks, and MCP definitions

## Install (project mode, full toolkit)

Use this when you want the full harness (rules + skills + subagents + commands + hooks + MCP).

```bash
rulesync fetch rudironsoni/dotnet-agent-harness:.rulesync
rulesync generate --targets "*" --features "*"
```

## Install (declarative sources)

RuleSync `install` is best for declarative source workflows, especially curated skills. Declare this repo as a source
and then generate.

```jsonc
{
  "sources": [{ "source": "rudironsoni/dotnet-agent-harness", "path": ".rulesync" }],
}
```

```bash
rulesync install && rulesync generate --targets "*" --features "*"
```

## OpenCode: making `dotnet-architect` visible in Tab cycling

OpenCode cycles only **primary** agents with Tab. Subagents are invoked with `@mention`.

- `dotnet-architect` is configured for OpenCode as a **primary** agent in this repo.
- After generation, it should appear in OpenCode's main agent rotation.

## Global mode notes

RuleSync global mode is useful for global rules, but it has feature limitations compared to project mode.

- If your goal is full harness behavior (including agent workflows), prefer **project mode** installation.
- For OpenCode specifically, primary-agent UX is most reliable in project mode generation.

## Troubleshooting

### Error: `Multiple root rulesync rules found`

This usually happens when both files exist:

- `.rulesync/rules/overview.md` (from `rulesync init`)
- `.rulesync/rules/00-overview.md` (from some fetched repos)

Keep only one `root: true` overview file.

```bash
rm -f .rulesync/rules/00-overview.md
```

or

```bash
rm -f .rulesync/rules/overview.md
```

## Local development in this repository

```bash
npm install
npm run ci:rulesync
```

## Source layout

All authored content lives exclusively under `.rulesync/`:

- `.rulesync/rules/` — rules
- `.rulesync/skills/` — skills
- `.rulesync/subagents/` — agent definitions
- `.rulesync/commands/` — slash commands
- `.rulesync/hooks.json` and `.rulesync/hooks/` — hook config and scripts
- `.rulesync/mcp.json` — MCP servers

Generated output (`.github/agents/`, `.github/skills/`, `AGENTS.md`, etc.) is gitignored and produced by
`rulesync generate`.

## Attribution

- Original corpus: `https://github.com/novotnyllc/dotnet-artisan`
- RuleSync: `https://github.com/dyoshikawa/rulesync`

## License

MIT. See `LICENSE`.
