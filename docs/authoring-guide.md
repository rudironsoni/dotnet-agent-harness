# Authoring Guide

This document contains authoring conventions for maintaining `.rulesync/` content.

## Frontmatter Portability Contract

### Shared fields (top-level, all platforms read these)

| Field         | Required | Notes                                    |
| ------------- | -------- | ---------------------------------------- |
| `name`        | yes      | Unique identifier for the skill/subagent |
| `description` | yes      | One-line summary                         |
| `targets`     | yes      | Array of target platforms (e.g. `["*"]`) |
| `tags`        | optional | Categorization tags                       |
| `version`     | optional | Semantic version string                   |
| `author`      | optional | Toolkit or author name                    |
| `license`     | optional | License identifier                        |

### Banned at top-level

| Field            | Reason                                                                  |
| ---------------- | ----------------------------------------------------------------------- |
| `tools`          | Platform-specific — belongs inside each platform block |
| `model`          | Platform-specific (Claude: `sonnet`/`inherit`, OpenCode: `provider/id`) |
| `user-invocable` | Not a RuleSync concept                                   |
| `capabilities`   | Informational only — belongs in body text                |
| `context`        | Removed entirely                                         |

### Platform-specific blocks

```yaml
claudecode:
  model: optional # sonnet | opus | haiku | inherit
  allowed-tools: optional # canonical names

opencode:
  mode: required # primary | subagent
  tools: optional # boolean map
  permission: optional

copilot:
  tools:
    optional
    # agent/runSubagent is included automatically

codexcli:
  sandbox_mode: optional
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

## Command Portability

### Portability Classification

| Classification | Description | Best For |
|---------------|-------------|----------|
| `universal` | Works on all targets | Bootstrap, simple queries |
| `claude-opencode` | Requires rich tool surface | Complex multi-step operations |
| `copilot-gemini` | Works with mapped tool names | Metadata, profile |
| `codex-global` | Global mode only | Read-only analysis |
| `antigravity` | Workflow-triggered | Quality gates, hooks |

### Naming Conventions

1. **Prefix Convention**: Always use `<toolkit>-` prefix
   - Good: `dotnet-agent-harness-search`
   - Bad: `search`

2. **Verb-Noun Pattern**: Action-oriented naming
   - Good: `generate`, `analyze`, `validate`
   - Bad: `helper`, `utils`

3. **Kebab-Case**: Lowercase with hyphens
   - Good: `deep-wiki-generate`
   - Bad: `deepWikiGenerate`

## Skill Taxonomy

Skills MUST include taxonomy classification in frontmatter.

### Required Taxonomy Fields

| Field         | Required | Values                                                |
| ------------- | -------- | ----------------------------------------------------- |
| `category`    | yes      | fundamentals, testing, architecture, web, data, performance, security, devops, platforms, tooling |
| `subcategory` | yes      | See TAXONOMY.md for valid subcategories |
| `complexity`  | yes      | beginner, intermediate, advanced |
| `tags`        | yes      | Array including complexity + domain tags |

### Complexity Levels

| Level        | Target Audience              |
| ------------ | ---------------------------- |
| `beginner`     | New to .NET                  |
| `intermediate` | Working developers           |
| `advanced`     | Senior developers/architects |

### Discovering Skills

1. **Browse INDEX.md** - Complete listing at `.rulesync/skills/INDEX.md`
2. **Check TAXONOMY.md** - Schema at `.rulesync/skills/TAXONOMY.md`
3. **Use meta-skills** - Category overviews like `dotnet-testing`
4. **Filter by complexity** - beginner/intermediate/advanced tags
5. **Follow related_skills** - Cross-references in frontmatter
