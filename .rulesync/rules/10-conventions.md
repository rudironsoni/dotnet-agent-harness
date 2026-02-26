---
root: false
targets: ["*"]
description: "dotnet-agent-harness authoring conventions for skills, subagents, commands, and hooks"
globs: [".rulesync/**/*"]
---

# Authoring Conventions

- Keep frontmatter compatible across targets using the portability contract below.
- Add tool-specific blocks only when required (`claudecode`, `opencode`, `codexcli`, `copilot`, `geminicli`).
- Use `version: "0.0.1"` for newly ported content unless a higher semantic version is intentionally introduced.
- Use ASCII in shell scripts and docs unless a source file already requires Unicode.
- Keep hook scripts advisory-only and return success to avoid blocking user workflows.
- Keep commands deterministic; avoid destructive actions unless explicitly requested.

## Frontmatter Portability Contract

### Shared fields (top-level, all platforms read these)

| Field            | Required | Notes                                                       |
| ---------------- | -------- | ----------------------------------------------------------- |
| `name`           | yes      | Unique identifier for the skill/subagent                    |
| `description`    | yes      | One-line summary                                            |
| `targets`        | yes      | Array of target platforms (e.g. `["*"]`)                    |
| `tools`          | shared   | Canonical tool names: `Read`, `Grep`, `Glob`, `Bash`, `Edit`, `Write` |
| `user-invocable` | shared   | Whether agent can be directly invoked by user               |
| `tags`           | optional | Categorization tags                                         |
| `version`        | optional | Semantic version string                                     |
| `author`         | optional | Toolkit or author name                                      |
| `license`        | optional | License identifier                                          |

### Banned at top-level

| Field          | Reason                                                                  |
| -------------- | ----------------------------------------------------------------------- |
| `model`        | Platform-specific (Claude: `sonnet`/`inherit`, OpenCode: `provider/id`) |
| `capabilities` | Informational only — belongs in body text, not frontmatter              |
| `context`      | Removed entirely                                                        |

### Platform-specific blocks

```yaml
claudecode:
  model: optional      # sonnet | opus | haiku | inherit
  allowed-tools: optional
  user-invocable: optional  # override if different from top-level

opencode:
  mode: required       # primary | subagent | skill
  model: optional      # provider/model-id format
  temperature: optional
  version: optional

copilot:
  tools: optional      # platform-mapped names (see table below)

codexcli:
  short-description: optional
```

### Tool name mapping (canonical → Copilot)

| Canonical | Copilot   |
| --------- | --------- |
| `Read`    | `read`    |
| `Grep`    | `search`  |
| `Glob`    | `search`  |
| `Bash`    | `execute` |
| `Edit`    | `edit`    |
| `Write`   | `edit`    |
