# Consumer Guide

Welcome to dotnet-agent-harness! This guide covers everything you need to use the toolkit in your .NET projects.

## Quick Start

Install and bootstrap in 3 commands:

```bash
# 1. Install the tool
dotnet new tool-manifest
dotnet tool install Rudironsoni.DotNetAgentHarness

# 2. Bootstrap your repo
dotnet agent-harness bootstrap

# 3. Start coding!
```

The bootstrap command installs AI agent configurations tailored to your project.

## Available Commands

### bootstrap
Install or update agent configurations.

```bash
dotnet agent-harness bootstrap [options]
```

**Options:**
- `--targets <list>` - Specific targets (default: all available)
- `--force` - Overwrite existing files
- `--list-targets` - Show available targets without installing

**Examples:**
```bash
# Install all targets
dotnet agent-harness bootstrap

# Install specific targets
dotnet agent-harness bootstrap --targets claudecode,opencode

# Show what's available
dotnet agent-harness bootstrap --list-targets

# Force overwrite existing files
dotnet agent-harness bootstrap --force
```

### doctor
Check installation health and diagnose issues.

```bash
dotnet agent-harness doctor
```

Checks:
- Tool installation status
- Bootstrap completion
- Configuration validity
- Available updates

### recommend
Get skill recommendations for your project.

```bash
dotnet agent-harness recommend
```

Analyzes your project and suggests relevant skills based on:
- Frameworks detected
- Project type
- Dependencies
- Common patterns

### search
Search the skill catalog.

```bash
dotnet agent-harness search <query>

# Examples
dotnet agent-harness search "ef core"
dotnet agent-harness search "testing"
dotnet agent-harness search "async"
```

### prepare
Assemble prompt bundles for specific tasks.

```bash
dotnet agent-harness prepare "implement JWT authentication"
dotnet agent-harness prepare "create integration tests"
```

### validate
Run validation checks on your project.

```bash
dotnet agent-harness validate
```

### analyze
Run comprehensive code analysis using Roslyn analyzers, StyleCop, and Sonar rules.

```bash
# Analyze current project
dotnet agent-harness analyze

# CI/CD integration with SARIF output
dotnet agent-harness analyze --format sarif --output results.sarif

# Fail on warnings
dotnet agent-harness analyze --warnings-as-errors
```

### export
Export agent-harness configuration as a portable bundle.

```bash
# Export default bundle
dotnet agent-harness export

# Export as YAML
dotnet agent-harness export --format yaml

# Export complete configuration
dotnet agent-harness export --include-rules
```

## What You Get

After bootstrap, your repo contains:

```
.dotnet-agent-harness/
└── install-manifest.json    # Tracks installed version/targets

# AI tool configurations (varies by target):
.claude/                     # Claude Code settings
.opencode/                   # OpenCode configuration
.codex/                      # Codex CLI configuration
# etc.
```

## Next Steps

- [Installation Details](installation.md)
- [Commands Reference](commands.md)
- [Troubleshooting](troubleshooting.md)
- [Available Skills](../skills/)
