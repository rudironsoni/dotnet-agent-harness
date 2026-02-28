# dotnet-harness

Comprehensive .NET development guidance for modern C#, ASP.NET Core, MAUI, Blazor, and cloud-native apps.

## Overview

This toolkit provides:

- 131 skills
- 14 specialist agents/subagents
- shared RuleSync rules, commands, hooks, and MCP config

Compatible targets include Claude Code, GitHub Copilot CLI, OpenCode, Codex CLI, Gemini CLI, and Antigravity.

## Quick Start

### Install the toolkit in a project

```bash
rulesync fetch rudironsoni/dotnet-harness:.rulesync
rulesync generate --targets "*" --features "*"
```

**Expected output:**

```text
Fetching rudironsoni/dotnet-harness:.rulesync...
Generating rules for targets: *
Features: *
Done! Rules installed to .rulesync/
```

### Alternative: Declarative install

```jsonc
// rulesync.jsonc
{
  "sources": [{ "source": "rudironsoni/dotnet-harness", "path": ".rulesync" }],
}
```

Then run:

```bash
rulesync install && rulesync generate --targets "*" --features "*"
```

---

## Project Structure

```text
dotnet-harness/
├── .opencode/
│   └── skill/          # 131 skill definitions (SKILL.md files)
│       ├── SKILL_TEMPLATE.md
│       ├── dotnet-version-detection/
│       ├── dotnet-containers/
│       └── ...
├── .rulesync/          # Generated RuleSync configuration
│   ├── agents/         # Agent definitions
│   ├── commands/       # CLI commands
│   ├── hooks/          # Lifecycle hooks
│   └── rules/          # Rule files
├── .mcp.json           # MCP server configuration
├── CLAUDE.md           # This file
└── package.json        # npm scripts for validation
```

---

## Critical Requirements

### Must Do

- **Add tests for new features** - Every new skill must have corresponding test coverage
- **Use RuleSync for generation** - Always regenerate rules after editing `.rulesync/` source files
- **Follow skill naming convention** - Use `dotnet-{topic}` or `{framework}-{action}` format
- **Include YAML frontmatter** - Every SKILL.md must have `name`, `description`, and `allowed-tools`
- **Validate with `npm run ci:rulesync`** - Before committing changes to `.rulesync/`

### Must Not Do

- **Edit generated files directly** - Only edit source files in `.rulesync/`, never the generated output
- **Use non-descriptive skill names** - Avoid generic names like "helper" or "utils"
- **Skip allowed-tools declaration** - Always explicitly declare which tools a skill can use
- **Commit without validation** - Always run the CI validation script before pushing

---

## OpenCode Behavior

- **Tab cycles** primary agents only
- **`@mention`** invokes subagents
- **`dotnet-architect`** is configured as a primary OpenCode agent so it appears in Tab rotation

---

## Development Workflow

### Adding a New Skill

1. Create skill directory: `.opencode/skill/<skill-name>/`
2. Copy template: `cp SKILL_TEMPLATE.md .opencode/skill/<skill-name>/SKILL.md`
3. Edit frontmatter and content
4. Test the skill with sample inputs
5. Run validation: `npm run ci:rulesync`
6. Commit with descriptive message

### Testing Skills

```bash
# Run skill validation
npm run ci:rulesync

# Expected output:
# ✓ All skills validated
# ✓ No syntax errors
# ✓ All required fields present
```

---

## MCP Integration

This project uses Model Context Protocol (MCP) for external tool integration:

- **Configuration:** `.mcp.json`
- **Servers:** serena, context7, microsoftdocs-mcp, github-mcp, docker-mcp, deepwiki, mcp-windbg
- **Fallback:** If MCP unavailable, skills use traditional tools (Read, Grep, Bash)

---

## Troubleshooting

### RuleSync Issues

**Issue:** `Multiple root rulesync rules found`
**Solution:** Ensure only one root overview rule exists in `.rulesync/rules/`

**Issue:** Changes not reflecting
**Solution:** Regenerate with `rulesync generate --targets "*" --features "*"`

### Validation Failures

**Issue:** `npm run ci:rulesync` fails

**Solution:**

1. Check for syntax errors in `.rulesync/` source files
2. Verify all skills have required YAML frontmatter
3. Ensure no circular dependencies in skill references

---

## Contributing

Edit source files in `.rulesync/` and validate with `npm run ci:rulesync`.

## License

MIT License. See `LICENSE`.
