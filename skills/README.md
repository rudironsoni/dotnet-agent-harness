# .NET Testing Agent Skills

This directory contains 29 .NET testing-related Agent Skills (2 overview skills + 27 specialized skills).

## 🎯 Overview Skills

These two overview skills provide intelligent navigation, automatically analyzing requirements and recommending appropriate skill combinations:

- **[dotnet-testing](dotnet-testing/)** - Basic testing skills overview (19 sub-skills)
- **[dotnet-testing-advanced](dotnet-testing-advanced/)** - Advanced testing skills overview (8 sub-skills)

## 📦 Installation

### Using npx skills install (Recommended)

```bash
# Install from GitHub to Claude Code global
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Or install to current workspace
npx skills install https://github.com/kevintsengtw/dotnet-testing-agent-skills.git --workspace
```

### Manual Installation

#### For GitHub Copilot (VS Code)

Copy the `skills/` directory to your project's `.github/skills/`:

```bash
cp -r skills/* /your-project/.github/skills/
```

#### For Claude Code

Copy the `skills/` directory to workspace or global:

```bash
# Workspace
cp -r skills/* /your-project/.claude/skills/

# Global
cp -r skills/* ~/.config/claude/skills/
```

## 📚 Skills List

### Overview Skills (2)

| Skill | Description |
|-------|-------------|
| `dotnet-testing` | Basic testing skills overview and guidance center |
| `dotnet-testing-advanced` | Advanced testing skills overview and guidance center |

### Basic Skills (19)

- `dotnet-testing-unit-test-fundamentals`
- `dotnet-testing-test-naming-conventions`
- `dotnet-testing-xunit-project-setup`
- `dotnet-testing-awesome-assertions-guide`
- `dotnet-testing-complex-object-comparison`
- `dotnet-testing-code-coverage-analysis`
- `dotnet-testing-nsubstitute-mocking`
- `dotnet-testing-test-output-logging`
- `dotnet-testing-private-internal-testing`
- `dotnet-testing-fluentvalidation-testing`
- `dotnet-testing-datetime-testing-timeprovider`
- `dotnet-testing-filesystem-testing-abstractions`
- `dotnet-testing-test-data-builder-pattern`
- `dotnet-testing-autofixture-basics`
- `dotnet-testing-autofixture-customization`
- `dotnet-testing-autodata-xunit-integration`
- `dotnet-testing-autofixture-nsubstitute-integration`
- `dotnet-testing-bogus-fake-data`
- `dotnet-testing-autofixture-bogus-integration`

### Advanced Skills (8)

- `dotnet-testing-advanced-aspnet-integration-testing`
- `dotnet-testing-advanced-testcontainers-database`
- `dotnet-testing-advanced-testcontainers-nosql`
- `dotnet-testing-advanced-webapi-integration-testing`
- `dotnet-testing-advanced-aspire-testing`
- `dotnet-testing-advanced-xunit-upgrade-guide`
- `dotnet-testing-advanced-tunit-fundamentals`
- `dotnet-testing-advanced-tunit-advanced`

## 📖 Detailed Documentation

Please refer to [PUBLIC_REPO_README.md](../PUBLIC_REPO_README.md) in the project root directory for complete usage instructions and learning resources.

## 🏆 Source

Based on "Old-School Software Engineer's Testing Practice - 30 Day Challenge" (2025 iThome Ironman Software Development Category Champion).

## 📄 License

MIT License
