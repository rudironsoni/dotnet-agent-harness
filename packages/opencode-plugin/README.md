# @rudironsoni/opencode-plugin

OpenCode plugin for dotnet-agent-harness - bundles agents, skills, commands, and rules for .NET development.

## Installation

```bash
npm config set @rudironsoni:registry https://npm.pkg.github.com
npm install @rudironsoni/opencode-plugin
```

For private package authentication, configure a GitHub token with `read:packages` in your `.npmrc`.

## Configuration

Add to your `opencode.json`:

```json
{
  "plugin": ["@rudironsoni/opencode-plugin"]
}
```

## What's Included

This plugin bundles:

- **14 Specialist Agents** - Architecture, Blazor, MAUI, Testing, Security, Performance, and more
- **131 Skills** - .NET development patterns, best practices, and tooling guides
- **Commands** - Project initialization, validation, and workflow automation
- **Rules** - Conventions and guidelines for consistent development
- **Hooks** - Lifecycle automation for session management

## Usage

Once installed, the plugin automatically:

1. Installs all bundled agents into `.opencode/agents/`
2. Installs all skills into `.opencode/skills/`
3. Installs rules into `.opencode/rules/`
4. Installs commands into `.opencode/commands/`
5. Installs hooks into `.opencode/hooks/`

### Using Agents

Agents are available as subagents that can be invoked via @mention:

- `@dotnet-architect` - Architecture recommendations
- `@dotnet-blazor-specialist` - Blazor development
- `@dotnet-maui-specialist` - MAUI development
- `@dotnet-testing-specialist` - Test architecture
- `@dotnet-security-reviewer` - Security reviews
- And 9 more...

### Using Skills

Skills can be referenced in prompts:

- `Use the dotnet-csharp-async-patterns skill`
- `Apply dotnet-ef-core-best-practices`
- `Follow dotnet-azure-deployment skill`

### Using Commands

Commands are available as slash commands:

- `/init-project` - Initialize a new .NET project

## License

MIT License - see LICENSE file for details.
