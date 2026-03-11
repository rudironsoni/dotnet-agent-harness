# dotnet-agent-harness

> **The definitive .NET development companion for AI coding tools.**
>
> 193 specialized skills · 18 expert subagents · 28 powerful commands

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Installation

### Option 1: CLI Tool (Recommended)

Install the `dotnet-agent-harness` CLI tool globally:

```bash
# Install as global .NET tool
dotnet tool install --global dotnet-agent-harness

# Or install from NuGet
dotnet tool install --global dotnet-agent-harness --source https://api.nuget.org/v3/index.json
```

### Option 2: Curl Installer

Install directly in your .NET repository using curl:

```bash
# One-line installer
curl -fsSL https://raw.githubusercontent.com/rudironsoni/dotnet-agent-harness/main/install-dotnet-agent-harness.sh | bash
```

Or with options:

```bash
# Download and run with specific targets
curl -fsSL https://raw.githubusercontent.com/rudironsoni/dotnet-agent-harness/main/install-dotnet-agent-harness.sh | bash -s -- --targets claudecode,copilot,opencode
```

The installer will:
1. Fetch the `.rulesync/` configuration from this repository
2. Download required hook scripts
3. Generate agent configurations for your specified targets

## Available Commands

### CLI Commands (`dotnet agent-harness`)

After installing the CLI tool, use these commands:

**Lifecycle Commands:**
- `dotnet agent-harness install` - Install the toolkit in current directory
- `dotnet agent-harness uninstall` - Remove the toolkit installation
- `dotnet agent-harness update` - Update to latest toolkit version
- `dotnet agent-harness self-update` - Update the CLI tool itself

**Discovery Commands:**
- `dotnet agent-harness search <query>` - Search skills, subagents, and commands
  - `--kind skill|subagent|command` - Filter by type
  - `--category <name>` - Filter skills by category
  - `--platform <name>` - Filter by platform compatibility
  - `--limit N` - Limit results (default: 10)
  - `--format json` - Output as JSON

- `dotnet agent-harness profile` - Show catalog statistics
  - `profile <item>` - Show details for specific item
  - `--kind <type>` - Specify item type
  - `--format json` - Output as JSON

- `dotnet agent-harness recommend` - Recommend skills for your .NET project
  - `--path <path>` - Path to project (default: current directory)
  - `--platform <name>` - Target platform for recommendations
  - `--category <name>` - Filter by category
  - `--limit N` - Maximum recommendations per kind
  - `--format json` - Output as JSON
  - `--write-state` - Save recommendations to `.dotnet-agent-harness/recommendations.json`

**Project Commands:**
- `dotnet agent-harness bootstrap <name>` - Bootstrap a new .NET project
  - Creates project with agent-harness pre-configured

**Analysis Commands:**
- `dotnet agent-harness analyze [path]` - Run comprehensive code analysis
  - Supports Roslyn analyzers, StyleCop, and Sonar rules
  - `--severity error|warning|info` - Minimum severity to report
  - `--format text|json|sarif` - Output format (SARIF for CI integration)
  - `--output <file>` - Write output to file
  - `--warnings-as-errors` - Treat warnings as errors (non-zero exit code)
  - `--stylecop` - Enable StyleCop analysis (default: true)
  - `--sonar` - Enable Sonar analysis (default: false)
  - Exit codes: 0=success, 1=violations found, 2=analysis failure

**Export Commands:**
- `dotnet agent-harness export [output]` - Export configuration as portable bundle
  - `--format json|yaml` - Output format (default: json)
  - `--include-skills` - Include skills in export (default: true)
  - `--include-subagents` - Include subagents in export (default: true)
  - `--include-commands` - Include commands in export (default: true)
  - `--include-rules` - Include rules in export (default: false)
  - Creates portable `agent-harness-bundle.json` for sharing

### RuleSync Commands

After installation, use these RuleSync commands:

- `rulesync generate` - Generate agent configurations
- `rulesync generate --check` - Validate generation is deterministic
- `rulesync install` - Install from declarative sources

## Documentation

- **[Consumer Guide](docs/guide/)** - Installation, daily commands, troubleshooting
- **[Maintainer Guide](docs/maintainer/)** - Authoring skills, bundle generation, releases

## What You Get

| Component       | Count | Description                                            |
| --------------- | ----- | ------------------------------------------------------ |
| **Skills**      | 193   | Self-contained guidance documents for .NET topics      |
| **Subagents**   | 18    | Specialized AI agents for specific domains             |
| **Commands**    | 28    | CLI commands for common workflows                      |
| **MCP Servers** | 6     | Model Context Protocol servers for AI tool integration |
| **CLI Tool**    | 1     | Cross-platform installer and discovery tool            |

### CLI Features

The `dotnet-agent-harness` CLI provides:

- **Rich console output** with Spectre.Console tables and colored panels
- **Automatic retry logic** with Polly for network resilience
- **Project analysis** detecting frameworks, test projects, CI/CD configs
- **Smart recommendations** based on project characteristics
- **Search capabilities** across all skills, subagents, and commands
- **Statistics and profiling** of the installed catalog

**Coverage Areas:**

- Modern C# (patterns, nullable types, async/await)
- ASP.NET Core (Minimal APIs, Blazor, gRPC)
- Data Access (EF Core, Dapper)
- Testing (xUnit, integration testing)
- Cloud-Native (Docker, Kubernetes, Aspire)
- Mobile (MAUI)
- Security (OWASP)
- Performance (profiling, optimization)

## Repository Structure

```text
.rulesync/
├── skills/        # 193 knowledge modules
├── subagents/     # 18 specialized agents
├── commands/      # 28 slash commands
├── agents/        # 3 primary agents
└── mcp.json       # MCP server definitions
```

## MCP Inventory

MCP inventory (source: .rulesync/mcp.json): `context7`, `deepwiki`, `github`, `microsoftdocs-mcp`, `serena`

| MCP Server          | Description                                  |
| ------------------- | -------------------------------------------- |
| `context7`          | Context7 MCP server for documentation        |
| `deepwiki`          | DeepWiki MCP for repository documentation    |
| `github`            | GitHub MCP for repository operations         |
| `microsoftdocs-mcp` | Microsoft Learn documentation access         |
| `serena`            | Symbol-level code navigation and refactoring |

## License

MIT License. See [LICENSE](LICENSE).
