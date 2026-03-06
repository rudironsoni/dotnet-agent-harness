# Migration Guide: v1.x to v2.0

This guide helps you migrate from dotnet-agent-harness v1.x to v2.0.

## Overview

Version 2.0 introduces a **runtime-first model** that eliminates the RuleSync dependency for consumers. Bundles are now embedded in the NuGet package and extracted at runtime.

## Breaking Changes

### Commands Removed

| Old Command | Replacement | Reason |
|-------------|-------------|--------|
| `init` | `bootstrap` | Consolidated into single command |
| `install-agent` | `bootstrap` | Was alias, now primary command |

### Bootstrap Options Removed

| Option | Status | Migration |
|--------|--------|-----------|
| `--profile` | Removed | Bundles are pre-built |
| `--features` | Removed | All features included in bundles |
| `--source` | Removed | Source now embedded |
| `--source-path` | Removed | Not needed with embedded bundles |
| `--config` | Removed | No rulesync.jsonc needed |
| `--run-rulesync` | Removed | RuleSync runs in CI only |
| `--tool-version` | Removed | Bundles versioned with tool |
| `--no-save` | Removed | Not applicable |

### Options Kept

| Option | Behavior |
|--------|----------|
| `--targets` | Install specific targets (default: all) |
| `--force` | Overwrite existing files |
| `--list-targets` | Show available targets |

## Migration Steps

### Step 1: Update the Tool

```bash
# Update to v2.0
dotnet tool update Rudironsoni.DotNetAgentHarness

# Verify version
dotnet agent-harness --version
# Should show 2.0.0 or later
```

### Step 2: Clean v1.x Artifacts

Remove old v1.x configuration files:

```bash
# Remove rulesync.jsonc if present
rm -f rulesync.jsonc

# Remove .rulesync/ if you don't need source (optional)
rm -rf .rulesync/

# Remove .config/dotnet-tools.json entries for rulesync
# (Keep dotnet-agent-harness entry)
```

### Step 3: Run Bootstrap

```bash
# Bootstrap with new runtime-first flow
dotnet agent-harness bootstrap
```

This will:
- Extract embedded bundles
- Install agent configurations
- Create `.dotnet-agent-harness/install-manifest.json`

### Step 4: Verify Installation

```bash
# Check everything is working
dotnet agent-harness doctor

# List installed targets
dotnet agent-harness bootstrap --list-targets
```

## What Changed Internally

### Before (v1.x)
```
.rulesync/ (source)
  ↓
rulesync generate (local or CI)
  ↓
Generated files in repo
```

### After (v2.0)
```
.rulesync/ (source - CI only)
  ↓
CI generates bundles
  ↓
Bundles embedded in NuGet package
  ↓
bootstrap extracts bundles at runtime
```

## For Maintainers

If you're maintaining this toolkit, see [Maintainer Guide](docs/maintainer/) for:
- Bundle generation process
- CI/CD workflows
- Release procedures

## Troubleshooting

### "Command not found: init"

**Error**: `Unknown command 'init'`

**Solution**: Use `bootstrap` instead:
```bash
dotnet agent-harness bootstrap
```

### "Bundle not found"

**Error**: `Bundle not found: claudecode`

**Solution**: 
1. Check tool version: `dotnet agent-harness --version`
2. Reinstall tool: `dotnet tool uninstall Rudironsoni.DotNetAgentHarness && dotnet tool install Rudironsoni.DotNetAgentHarness`
3. Try bootstrap again

### Missing targets after migration

**Symptom**: Not all targets installed

**Solution**: Install specific targets:
```bash
dotnet agent-harness bootstrap --targets claudecode,opencode,codexcli
```

## Rollback

If you need to rollback to v1.x:

```bash
# Install specific v1.x version
dotnet tool install Rudironsoni.DotNetAgentHarness --version 1.9.9

# Re-run old init flow
dotnet agent-harness init
```

## FAQ

**Q: Do I need RuleSync anymore?**  
A: No, for consumption. Yes, if you're maintaining the toolkit.

**Q: Can I still customize agents?**  
A: Yes, bootstrap creates files you can customize. Changes persist across updates.

**Q: Will my custom .rulesync/ modifications work?**  
A: No, v2.0 uses embedded bundles. Keep backups of custom modifications.

**Q: How do I update to new versions?**  
A: Same as before: `dotnet tool update` then `dotnet agent-harness bootstrap`

## Support

- [Consumer Guide](docs/guide/)
- [Troubleshooting](docs/guide/troubleshooting.md)
- GitHub Issues: https://github.com/rudironsoni/dotnet-agent-harness/issues
