# Commands Reference

Complete reference for all dotnet-agent-harness commands.

## Primary Commands

### bootstrap

Install or update agent configurations in your repository.

```bash
dotnet agent-harness bootstrap [options]
```

**Options:**

| Option | Description | Default |
|--------|-------------|---------|
| `--targets <list>` | Comma-separated list of targets | All available |
| `--force` | Overwrite existing files | false |
| `--list-targets` | List available targets | false |

**Examples:**

```bash
# Install all available targets
dotnet agent-harness bootstrap

# Install specific targets
dotnet agent-harness bootstrap --targets claudecode,opencode

# List available targets
dotnet agent-harness bootstrap --list-targets
```

### doctor

Check installation health.

```bash
dotnet agent-harness doctor
```

### recommend

Get skill recommendations.

```bash
dotnet agent-harness recommend
```

### search

Search the skill catalog.

```bash
dotnet agent-harness search <query>
```

### prepare

Assemble prompt bundles.

```bash
dotnet agent-harness prepare <request>
```

### validate

Run validation checks.

```bash
dotnet agent-harness validate
```

### analyze

Run comprehensive code analysis using Roslyn analyzers, StyleCop, and Sonar rules.

```bash
dotnet agent-harness analyze [path] [options]
```

**Arguments:**

| Argument | Description | Default |
|----------|-------------|---------|
| `path` | Path to project, solution, or directory | Current directory |

**Options:**

| Option | Description | Default |
|--------|-------------|---------|
| `--severity`, `-s` | Minimum severity (error, warning, info) | info |
| `--format`, `-f` | Output format (text, json, sarif) | text |
| `--output`, `-o` | Output file path | Console |
| `--warnings-as-errors`, `-w` | Treat warnings as errors | false |
| `--stylecop`, `-sc` | Enable StyleCop analysis | true |
| `--sonar`, `-so` | Enable Sonar analysis | false |
| `--verbose`, `-v` | Show detailed output | false |

**Exit Codes:**

| Code | Meaning |
|------|---------|
| 0 | Success - no issues found |
| 1 | Violations found (or warnings with `--warnings-as-errors`) |
| 2 | Analysis failure (e.g., invalid project) |

**Examples:**

```bash
# Analyze current directory
dotnet agent-harness analyze

# Analyze specific solution
dotnet agent-harness analyze ./MyApp.sln

# CI/CD integration - fail on warnings
dotnet agent-harness analyze --severity warning --warnings-as-errors

# Export SARIF for GitHub Advanced Security
dotnet agent-harness analyze --format sarif --output results.sarif

# JSON output for tooling integration
dotnet agent-harness analyze --format json --output results.json
```

**Output Formats:**

- **text** - Human-readable colored console output with summary tables
- **json** - Machine-parseable JSON with full issue details
- **sarif** - SARIF 2.1.0 format for CI/CD integration (GitHub, Azure DevOps)

### export

Export agent-harness configuration as a portable bundle.

```bash
dotnet agent-harness export [output] [options]
```

**Arguments:**

| Argument | Description | Default |
|----------|-------------|---------|
| `output` | Output file path | `agent-harness-bundle.json` |

**Options:**

| Option | Description | Default |
|--------|-------------|---------|
| `--format`, `-f` | Output format (json, yaml) | json |
| `--include-skills`, `-s` | Include skills | true |
| `--include-subagents`, `-a` | Include subagents | true |
| `--include-commands`, `-c` | Include commands | true |
| `--include-rules`, `-r` | Include rules | false |
| `--pretty`, `-p` | Pretty-print output | true |
| `--verbose`, `-v` | Show detailed output | false |

**Examples:**

```bash
# Export default bundle
dotnet agent-harness export

# Export to specific file
dotnet agent-harness export ./my-bundle.json

# Export as YAML
dotnet agent-harness export --format yaml

# Export only skills and subagents
dotnet agent-harness export --include-commands=false --include-rules=false

# Export complete configuration including rules
dotnet agent-harness export --include-rules
```
