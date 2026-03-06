# Troubleshooting

Common issues and solutions.

## Installation Issues

### Tool not found

**Error:** `Could not execute because the specified command or file was not found`

**Solution:**
1. Ensure manifest exists: `dotnet new tool-manifest`
2. Install tool: `dotnet tool install Rudironsoni.DotNetAgentHarness`
3. Restore tools: `dotnet tool restore`

### Permission denied

**Error:** `Permission denied`

**Solution:**
- On Linux/Mac: `chmod +x .config/dotnet-tools.json`
- On Windows: Run as Administrator or adjust permissions

## Bootstrap Issues

### Bootstrap fails

**Error:** `Bootstrap failed: No bundles found`

**Causes:**
1. Tool package doesn't include bundles (CI issue)
2. Corrupted installation

**Solutions:**
1. Reinstall tool: `dotnet tool uninstall Rudironsoni.DotNetAgentHarness && dotnet tool install Rudironsoni.DotNetAgentHarness`
2. Check version: `dotnet agent-harness doctor`
3. Report issue with `dotnet agent-harness doctor` output

### Files not created

**Symptom:** Bootstrap succeeds but no files appear

**Check:**
1. Are you in repo root? `pwd`
2. Check install manifest: `cat .dotnet-agent-harness/install-manifest.json`
3. List files: `ls -la`

### Overwrite confirmation

**Error:** `File exists: ...`

**Solution:** Use `--force` flag:
```bash
dotnet agent-harness bootstrap --force
```

## Runtime Issues

### Command not recognized

**Error:** `Unknown command`

**Check:**
```bash
dotnet agent-harness --help
```

### Slow performance

**Cause:** Large project analysis

**Solution:** Use `--targets` to limit scope:
```bash
dotnet agent-harness bootstrap --targets claudecode
```

## Getting Help

1. Run doctor: `dotnet agent-harness doctor`
2. Check version: `dotnet agent-harness --version`
3. Enable verbose: `dotnet agent-harness <command> --verbose`
4. Open issue on GitHub with output
