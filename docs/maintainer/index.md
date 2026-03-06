# Maintainer Guide

This guide is for contributors and maintainers of dotnet-agent-harness.

## Overview

This toolkit provides runtime-first AI assistance for .NET development through:
- **189 skills** covering modern .NET patterns
- **15 specialist subagents** for specific domains  
- **Bundled configurations** for multiple AI tools (Claude, OpenCode, Gemini, etc.)

## Architecture

### Source Structure

```
.rulesync/              # Source of truth (RuleSync format)
├── rules/              # Rule files
├── agents/             # Agent definitions
├── subagents/          # Subagent definitions
├── skills/             # Skill definitions
└── templates/          # Bundle templates

src/                    # Runtime implementation
├── DotNetAgentHarness.Tools/    # CLI tool
└── DotNetAgentHarness.Evals/    # Eval runner
```

### How It Works

1. **Source**: Authors edit `.rulesync/` files
2. **CI**: RuleSync generates bundles for all targets
3. **Package**: Bundles embedded in NuGet package
4. **Runtime**: `bootstrap` extracts bundles to consumer repos

### Bundle Flow

```
.rulesync/ (source)
    ↓
RuleSync generate (CI)
    ↓
bundles/{target}.tar.gz
    ↓
Embedded in .nupkg
    ↓
Extracted by bootstrap
```

## Development Workflow

1. **Edit**: Modify files in `.rulesync/`
2. **Validate**: Run `npm run ci:rulesync`
3. **Test**: `dotnet run -- bootstrap`
4. **Submit**: Create PR with changes

## Key Concepts

### RuleSync Format

Rules define behavior for AI coding tools:

```yaml
---
name: dotnet-advisor
version: "2.0"
---
# Content here
```

### Bundle Targets

Each target represents an AI tool platform:
- `claudecode` - Claude Code
- `opencode` - OpenCode
- `geminicli` - Gemini CLI
- `codexcli` - Codex CLI
- etc.

### Skills

Self-contained guidance documents:
- Live in `.opencode/skill/`
- Loaded by skill name
- Include metadata (version, dependencies)

## Release Process

1. Bump version in relevant files
2. Tag: `git tag vX.Y.Z`
3. Push: `git push origin vX.Y.Z`
4. CI generates bundles and publishes

## Documentation Structure

- **Consumer Guide** (`docs/guide/`) - End-user documentation
- **Maintainer Guide** (`docs/maintainer/`) - This documentation
- **Skills** (`.opencode/skill/`) - Individual skill docs

## Contributing

See repository CONTRIBUTING.md for:
- Code style
- PR process
- Testing requirements
