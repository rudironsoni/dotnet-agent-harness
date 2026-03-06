# Release Workflow

## Overview

This document describes the release process for dotnet-agent-harness.

## Versioning

We use Semantic Versioning (SemVer):
- **MAJOR**: Breaking changes
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, backward compatible

## Release Process

### 1. Prepare Release

Update version numbers:

```bash
# Update version in relevant files
# - .rulesync/rules/00-overview.md
# - src/DotNetAgentHarness.Tools/DotNetAgentHarness.Tools.csproj
# - docs/index.md
```

### 2. Update Changelog

Document changes:

```markdown
## [2.0.0] - 2026-03-06

### Breaking Changes
- Removed `install-agent` and `init` commands
- Removed RuleSync-specific bootstrap options
- Runtime-first model (bundles embedded in package)

### Features
- Bundle-based distribution
- Simplified bootstrap command
- Migration detection
```

### 3. Create Tag

```bash
# Create annotated tag
git tag -a v2.0.0 -m "Release v2.0.0: Runtime-first simplification"

# Push tag
git push origin v2.0.0
```

### 4. CI Pipeline

The tag push triggers CI:

1. **Bundle Generation**
   - RuleSync generates all targets
   - Creates tar.gz archives
   - Uploads as artifacts

2. **Package Build**
   - Downloads bundle artifacts
   - Embeds in .NET tool package
   - Runs smoke tests

3. **Publish**
   - Publishes to GitHub Packages
   - Publishes to NuGet.org

### 5. Verify Release

Check the release:

```bash
# Install from GitHub Packages
dotnet tool install Rudironsoni.DotNetAgentHarness \
  --add-source https://nuget.pkg.github.com/rudironsoni/index.json \
  --version 2.0.0

# Test bootstrap
dotnet agent-harness bootstrap --list-targets
```

### 6. Update Documentation

- Update docs with new features
- Update migration guides
- Announce in discussions

## CI/CD Workflows

### bundle-packaging.yml

Generates bundles on every push to main:

```yaml
on:
  push:
    branches: [main]
    tags: ['v*']
```

### publish-dotnet-tool.yml

Publishes on tags:

```yaml
on:
  push:
    tags: ['v*']
```

## Rollback

If a release is broken:

1. Delete the tag: `git push origin --delete v2.0.0`
2. Delete GitHub release
3. Unlist from NuGet (if published)
4. Fix issues
5. Create new tag

## Pre-Release Checklist

- [ ] All tests passing
- [ ] Documentation updated
- [ ] Changelog updated
- [ ] Version bumped
- [ ] Breaking changes documented
- [ ] Migration guide updated (if needed)
- [ ] Smoke test passed locally

## Post-Release

- Monitor for issues
- Respond to feedback
- Plan next release
