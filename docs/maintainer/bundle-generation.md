# Bundle Generation

## Overview

Bundles are the packaging format that distributes RuleSync content to consumers. This document covers bundle generation and management.

## Bundle Format

### Structure

```
{target}.tar.gz
├── manifest.json          # Bundle metadata
├── CLAUDE.md              # Root documentation (if target=claudecode)
├── .claude/               # Claude-specific files
│   ├── settings.json
│   └── prompts/
└── .opencode/             # OpenCode-specific files (if bundled)
    └── skill/
```

### Manifest Schema

```json
{
  "version": "2.0.0",
  "target": "claudecode",
  "platform": "all",
  "generated": "2026-03-06T10:30:00Z",
  "checksums": {
    "CLAUDE.md": "sha256:abc123...",
    ".claude/settings.json": "sha256:def456..."
  }
}
```

## Generation Process

### Local Generation

```bash
# Generate all targets locally
./scripts/build/generate_bundles.sh

# Or manually:
rulesync generate \
  --source . \
  --output bundles/ \
  --targets "*" \
  --features "*"

# Create archives
cd bundles
for dir in */; do
  target="${dir%/}"
  tar -czf "${target}.tar.gz" "$target"
  rm -rf "$target"
done
```

### CI Generation

The bundle-packaging workflow handles CI generation:

1. Triggered on push to main or tags
2. Installs RuleSync CLI
3. Generates all targets
4. Creates tar.gz archives
5. Uploads as artifacts

### Embedding in Package

Bundles are embedded in the .NET tool package:

```xml
<!-- In .csproj -->
<ItemGroup Condition="Exists('../../bundles')">
  <EmbeddedResource Include="../../bundles/*.tar.gz">
    <LogicalName>DotNetAgentHarness.Tools.Bundles.%(Filename)</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

## Bundle Targets

| Target | Description |
|--------|-------------|
| claudecode | Claude Code configuration |
| opencode | OpenCode configuration |
| codexcli | Codex CLI configuration |
| geminicli | Gemini CLI configuration |
| copilot | GitHub Copilot configuration |
| antigravity | Antigravity configuration |
| factorydroid | Factory Droid configuration |

## Extraction at Runtime

The `bootstrap` command extracts bundles:

```csharp
private async Task ExtractBundleAsync(string target)
{
    var assembly = typeof(BootstrapEngine).Assembly;
    var resource = $"DotNetAgentHarness.Tools.Bundles.{target}";
    
    using var stream = assembly.GetManifestResourceStream(resource);
    if (stream == null) 
        throw new InvalidOperationException($"Bundle not found: {target}");
    
    // Extract to temp, then move to repo
    var tempDir = Path.Combine(Path.GetTempPath(), $"harness-{Guid.NewGuid()}");
    await ExtractTarGzAsync(stream, tempDir);
    
    // Move files to final location
    foreach (var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
    {
        var relPath = Path.GetRelativePath(tempDir, file);
        var destPath = Path.Combine(_repoRoot, relPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        File.Move(file, destPath, overwrite: true);
    }
    
    Directory.Delete(tempDir, recursive: true);
}
```

## Validation

Verify bundles are correct:

```bash
# Check bundle contents
tar -tzf bundles/claudecode.tar.gz | head -20

# Verify manifest
 tar -xzf bundles/claudecode.tar.gz -O manifest.json | jq .
```

## Troubleshooting

### Bundle not found at runtime

Error: `Bundle not found: {target}`

Check:
1. Bundle exists in `bundles/` directory
2. Bundle is embedded in .nupkg: `unzip -l package.nupkg | grep bundles`
3. Logical name matches: `assembly.GetManifestResourceNames()`

### Bundle extraction fails

Check:
1. Disk space available
2. Write permissions in target directory
3. Corrupted bundle file
