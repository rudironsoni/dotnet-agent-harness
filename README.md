# dotnet-agent-harness

> **The definitive .NET development companion for AI coding tools.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Use in a Repo

Install the toolkit in your .NET repository:

```bash
# Install the tool
dotnet new tool-manifest
dotnet tool install Rudironsoni.DotNetAgentHarness

# Bootstrap your repo with agents
dotnet agent-harness bootstrap
```

That's it! The bootstrap command will install the appropriate agent configurations for your project.

## Available Commands

After installation, use these commands:

- `dotnet agent-harness bootstrap` - Install/update agent configurations
- `dotnet agent-harness doctor` - Check installation health
- `dotnet agent-harness recommend` - Get skill recommendations for your project
- `dotnet agent-harness search <query>` - Search the skill catalog
- `dotnet agent-harness prepare` - Assemble prompt bundles
- `dotnet agent-harness validate` - Run validation checks

## Documentation

- **[Consumer Guide](docs/guide/)** - Installation, daily commands, troubleshooting
- **[Maintainer Guide](docs/maintainer/)** - Authoring skills, bundle generation, releases

## What You Get

| Component       | Count | Description                                           |
| --------------- | ----- | ----------------------------------------------------- |
| **Skills**      | 189   | Self-contained guidance documents for .NET topics     |
| **Subagents**   | 15    | Specialized AI agents for specific domains            |
| **Commands**    | 20    | CLI commands for common workflows                     |

**Coverage Areas:**

- Modern C# (patterns, nullable types, async/await)
- ASP.NET Core (Minimal APIs, Blazor, gRPC)
- Data Access (EF Core, Dapper)
- Testing (xUnit, integration testing)
- Cloud-Native (Docker, Kubernetes, Aspire)
- Mobile (MAUI)
- Security (OWASP)
- Performance (profiling, optimization)

## License

MIT License. See [LICENSE](LICENSE).
