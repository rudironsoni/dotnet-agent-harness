# dotnet-harness

Comprehensive .NET skills, subagents, commands, hooks, and MCP config for AI coding tools, maintained in RuleSync
format.

## What this repository provides

- 131 .NET-focused skills
- 14 specialist subagents/agents
- shared commands, hooks, and MCP definitions

## Install (project mode, full toolkit)

Use this when you want the full harness (rules + skills + subagents + commands + hooks + MCP).

```bash
rulesync fetch rudironsoni/dotnet-harness:.rulesync
rulesync generate --targets "*" --features "*"
```

## Install (declarative sources)

RuleSync `install` is best for declarative source workflows, especially curated skills. Declare this repo as a source
and then generate.

```jsonc
{
  "sources": [{ "source": "rudironsoni/dotnet-harness", "path": ".rulesync" }],
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

## Distribution strategy decision

### Decision

- Keep **two repos**:
  - `dotnet-harness` = authored source + generation orchestration
  - `dotnet-harness-plugin` = generated artifacts only (marketplace/distribution surface)
- Treat the plugin repo as **bot-managed output**, not a development repo.

### Why this is the best fit

- Matches your channel reality:
  - Claude Code uses repo as marketplace
  - OpenCode uses npm artifacts
  - Other platforms need manual-copy bundles
- Reduces contributor friction by keeping generated churn out of source `main`.
- Gives a clean public "install surface" for consumers while preserving clean source ownership.

### Ownership boundaries

- **Humans edit only in source repo** (`dotnet-harness`):
  - `.rulesync/**`
  - generation scripts/workflows/docs
  - platform package source under `packages/dotnet-harness-*/**` (except generated bundle payload)
- **Generated-only in distribution repo** (`dotnet-harness-plugin`):
  - `.claude/**`, `.opencode/**`, `.codex/**`, `.gemini/**`, `.agents/**`, `.agent/**`
  - `.github/agents/**`, `.github/skills/**`, `.github/instructions/**`, `.github/prompts/**`,
    `.github/copilot-instructions.md`
  - `AGENTS.md`, `GEMINI.md`
  - `packages/dotnet-harness-*/**` (all platform package assets)

### Branch protection + policy

- On `dotnet-harness-plugin` `main`:
  - require PRs (no direct pushes)
  - require status checks:
    - `enforce-generated-only`
    - `enforce-bot-author-for-generated`
  - restrict who can push/merge (bot + maintainers)
- Policy rules:
  - any change under generated paths must be authored by `github-actions[bot]` (or your dedicated bot account)
  - optional: block non-generated files from changing in distribution repo except allowlist (`README.md`, `LICENSE`,
    workflow files)

### Minimal workflow set

In `dotnet-harness`:

- `build-and-release.yml` (already present):
  - keeps zip release + GitHub Packages publish
- `update-distribution.yml` (existing concept, keep but harden):
  - trigger on source-of-truth path changes + manual dispatch
  - generate bundles + platform package outputs
  - sync into `dotnet-harness-plugin`
  - open/refresh PR in distribution repo
- `verify-generation.yml` (recommended new):
  - PR check in source repo
  - regenerate outputs
  - fail if generated output doesn't match expectations (drift detection)

In `dotnet-harness-plugin`:

- `enforce-generated-policy.yml` (recommended new):
  - on PR/push, ensure:
    - changed generated paths are bot-authored
    - only allowed file areas changed
  - fail otherwise

### Release and distribution matrix (single source of truth doc)

- Claude Code marketplace: `dotnet-harness-plugin` repo content
- Platform npm packages on GitHub Packages:
  - `@rudironsoni/dotnet-harness-opencode`
  - `@rudironsoni/dotnet-harness-claudecode`
  - `@rudironsoni/dotnet-harness-copilot`
  - `@rudironsoni/dotnet-harness-codexcli`
  - `@rudironsoni/dotnet-harness-geminicli`
  - `@rudironsoni/dotnet-harness-agentsmd`
  - `@rudironsoni/dotnet-harness-antigravity`
- Manual platforms: zip artifacts from source repo releases (`dist/*.zip`) and/or distribution repo snapshot
- Source authoring: only `dotnet-harness`

### Runbook (ops)

- **Normal update**
  - merge source change to `dotnet-harness/main`
  - bot workflow opens/updates PR in plugin repo
  - checks pass, merge PR
- **Sync failed**
  - rerun failed workflow in source repo
  - if PR branch stale/conflicted, close and regenerate branch via rerun
- **Policy check failed in plugin repo**
  - inspect changed files: usually manual edit slipped in
  - revert manual change, let bot regenerate
- **Hotfix required for consumer**
  - make fix in source repo only
  - trigger manual dispatch for distribution sync
- **Rollback**
  - revert source commit in source repo
  - rerun sync workflow to regenerate prior known-good state in plugin repo

### Governance rules

- No human manual edits to generated payload in plugin repo.
- No git hooks relied upon for security enforcement.
- CI + branch protection are the enforcement mechanism.
- Document this in both repos' README "Contributing" sections.

## Attribution

- Original corpus: `https://github.com/novotnyllc/dotnet-artisan`
- RuleSync: `https://github.com/dyoshikawa/rulesync`

## License

MIT. See `LICENSE`.
