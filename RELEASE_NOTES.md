# Release Notes

## v2.0.0 - Complete Enhancement (2025-02-28)

### 🎉 Major Milestone: All 4 Phases Complete

This release represents a comprehensive enhancement of the dotnet-agent-harness toolkit with significant new
capabilities across architecture, tooling, and developer experience.

---

## Phase 1: Foundation & Quick Wins ✅

### New Commands (5)

- `/dotnet-agent-harness:search` - Semantic skill search with filtering
- `/dotnet-agent-harness:test` - Comprehensive skill testing framework
- `/dotnet-agent-harness:profile` - Local performance analysis (no telemetry)
- `/dotnet-agent-harness:graph` - Dependency visualization with Mermaid
- `/dotnet-agent-harness:compare` - Skill version comparison

### VS Code Extension

- Real-time frontmatter validation
- Skill autocomplete with descriptions
- Inline error highlighting
- Commands: search, validate, insert skill

### Infrastructure

- Hierarchical tagging system (`category/subcategory`)
- 131 skills updated with structured tags

---

## Phase 2: Core Architecture ✅

### New MCP Servers (2)

- **github-mcp** - Repository operations, PRs, issues, Actions
- **docker-mcp** - Container management, images, Compose

### MCP Health & Discovery

- Health validation for stdio and http servers
- Auto-discovery from official registries
- Session start health checks

### Skill Manifest System

- Dependency tracking (`depends_on`, `conflicts_with`)
- Topological sorting and circular dependency detection
- Version conflict validation
- Build tool: `dotnet-agent-harness:build-manifest`

### Docker & GitHub Actions

- Multi-stage Dockerfile with health checks
- Official GitHub Actions composite action
- Non-root user security
- Container metadata labels

---

## Phase 3: Advanced Features ✅

### Skill Recommendation Engine

- Project pattern detection for automatic suggestions
- Category-based filtering
- Interactive recommendation mode
- Local-only analysis (no external telemetry)

### Semantic Skill Search

- Full-text search across descriptions
- Tag and category filtering
- tf-idf scoring for relevance
- Local-only operation

### Offline Mode

- Air-gapped environment support
- Local MCP and skill caching
- Cache management commands
- Configurable cache size and TTL

### Skill Testing Framework

- YAML-based test cases
- Automated validation and reporting
- Test coverage metrics
- CI/CD integration

---

## Phase 4: Ecosystem ✅

### Quick-Start Templates (6)

- **web-api** - Minimal API with OpenAPI
- **blazor-app** - WebAssembly + Server
- **maui-mobile** - Cross-platform mobile
- **clean-arch** - Clean Architecture boilerplate
- **console** - CLI application
- **classlib** - Class library with NuGet

### Documentation Portal

- VitePress-based documentation site
- Searchable skill catalog
- API reference documentation
- Getting started guides

### Community Skill Registry Foundation

- External source support structure
- Rating system foundation
- Contribution guidelines

---

## Breaking Changes

None. This is a backward-compatible enhancement release.

## Migration Guide

No migration required. Existing projects continue to work.

To take advantage of new features:

```bash
# Update to latest
rulesync fetch rudironsoni/dotnet-agent-harness:.rulesync
rulesync generate --targets "*" --features "*"

# Try new commands
/dotnet-agent-harness:search testing
/dotnet-agent-harness:profile
```

## Statistics

- **Total Skills**: 131 → 147 (+16 new)
- **Total Subagents**: 15 → 15 (stable)
- **Total Commands**: 15 → 20 (+5 new)
- **MCP Servers**: 3 → 5 (+2 new)
- **Lines of Code**: ~15,000 → ~25,000

## Contributors

Thanks to everyone who contributed to this release!

## Support

- [GitHub Issues](https://github.com/rudironsoni/dotnet-agent-harness/issues)
- [Discussions](https://github.com/rudironsoni/dotnet-agent-harness/discussions)

---

**Full Changelog**: Compare with previous releases on GitHub
