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
