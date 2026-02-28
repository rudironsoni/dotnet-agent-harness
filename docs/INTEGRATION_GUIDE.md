# Integration Guide

## Quick Start

### 1. Install Prerequisites

```bash
# Install Node.js (for MCP servers that use npx)
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install uv (for Python-based MCP servers)
curl -LsSf https://astral.sh/uv/install.sh | sh
```

### 2. Configure MCP Servers

The `.mcp.json` file is already configured with 7 MCP servers:

```json
{
  "mcpServers": {
    "serena": { "type": "stdio", "command": "uvx", ... },
    "context7": { "type": "stdio", "command": "npx", ... },
    "github-mcp": { "type": "stdio", "command": "npx", ... },
    "docker-mcp": { "type": "stdio", "command": "docker", ... },
    "microsoftdocs-mcp": { "type": "http", "url": "..." },
    "deepwiki": { "type": "http", "url": "..." },
    "mcp-windbg": { "type": "stdio", "command": "uvx", ... }
  }
}
```

### 3. Set Environment Variables

```bash
# Required for github-mcp
export GITHUB_TOKEN=your_github_personal_access_token

# Optional: For debugging
export MCP_DEBUG=true
```

### 4. Test MCP Integration

```bash
# Verify MCP configuration is valid
jq empty .mcp.json && echo "✓ MCP configuration is valid"

# Check server count
jq '.mcpServers | length' .mcp.json
# Expected: 7
```

### 5. Use Skills

```bash
# List available skills
ls .opencode/skill/*/SKILL.md

# Create a new skill from template
cp .opencode/skill/SKILL_TEMPLATE.md .opencode/skill/my-new-skill/SKILL.md
```

---

## Example Commands

### Example 1: Check MCP Server Status

```bash
# List all configured MCP servers
jq -r '.mcpServers | keys[]' .mcp.json
```

**Output:**

```text
serena
context7
microsoftdocs-mcp
mcp-windbg
deepwiki
github-mcp
docker-mcp
```

### Example 2: Use dotnet-mcp-integration Skill

```bash
# Validate MCP configuration
jq empty .mcp.json

# Get server details
jq '.mcpServers.serena' .mcp.json

# Check if HTTP servers are reachable
curl -s https://mcp.deepwiki.com/mcp | head -1
```

### Example 3: Create New Skill

```bash
# Create skill directory
mkdir -p .opencode/skill/my-skill

# Copy template
cp .opencode/skill/SKILL_TEMPLATE.md .opencode/skill/my-skill/SKILL.md

# Edit the skill
nano .opencode/skill/my-skill/SKILL.md
```

### Example 4: Update Marketplace

```bash
# Add skill to marketplace.json
jq '.skills += [{"name": "my-skill", "description": "My skill", "category": "Custom"}]' .claude-plugin/marketplace.json
```

### Example 5: Troubleshoot MCP

```bash
# Check if server command exists
which npx
which uvx

# Test HTTP endpoint
curl -s -o /dev/null -w "%{http_code}" https://mcp.deepwiki.com/mcp

# Verify JSON syntax
jq empty .mcp.json
```

---

## Workflow Examples

### Workflow 1: Add a New MCP Server

**Scenario:** Adding a new MCP server for database operations

```bash
# Step 1: Install the MCP server
npm install -g @modelcontextprotocol/server-postgres

# Step 2: Add to .mcp.json
jq '.mcpServers.postgres = {
  "type": "stdio",
  "command": "npx",
  "args": ["-y", "@modelcontextprotocol/server-postgres"],
  "env": {
    "DATABASE_URL": "postgresql://localhost/mydb"
  }
}' .mcp.json > .mcp.json.tmp && mv .mcp.json.tmp .mcp.json

# Step 3: Validate configuration
jq empty .mcp.json

# Step 4: Test connection
# (In agent: use the postgres MCP server)
```

### Workflow 2: Debug MCP Connection Issues

**Scenario:** MCP server not responding

```bash
# Step 1: Check if command exists
which uvx || echo "uvx not found, installing..."

# Step 2: Test with verbose output
uvx --from git+https://github.com/oraios/serena serena --version

# Step 3: Check for errors in output
# Look for: module not found, permission denied, etc.

# Step 4: Review error handling guide
cat docs/MCP_ERROR_HANDLING.md
```

### Workflow 3: Create Complete Skill Package

**Scenario:** Creating a skill with full documentation

```bash
# Step 1: Create skill directory
mkdir -p .opencode/skill/dotnet-database

# Step 2: Create SKILL.md from template
cp .opencode/skill/SKILL_TEMPLATE.md .opencode/skill/dotnet-database/SKILL.md

# Step 3: Edit frontmatter
# Set name: dotnet-database
# Set description: "Database operations for .NET projects"
# Set allowed-tools: [Read, Write, Grep, Bash(*)]

# Step 4: Add content
# - Quick start with EF Core commands
# - Connection string detection
# - Migration workflows
# - Troubleshooting common issues

# Step 5: Update marketplace
jq '.skills += [{
  "name": "dotnet-database",
  "description": "Database operations for .NET projects",
  "category": "Data"
}]' .claude-plugin/marketplace.json

# Step 6: Test
# Validate JSON: jq empty .claude-plugin/marketplace.json
# Validate frontmatter: grep -A5 "^---" .opencode/skill/dotnet-database/SKILL.md
```

---

## Troubleshooting

### Common Issues

| Issue                      | Solution                                                       |
| -------------------------- | -------------------------------------------------------------- |
| `command not found: npx`   | Install Node.js: `npm install -g npx`                          |
| `uvx: command not found`   | Install uv: `curl -LsSf https://astral.sh/uv/install.sh \| sh` |
| `MCP server timeout`       | Check network connection, increase timeout                     |
| `401 Unauthorized`         | Verify GITHUB_TOKEN is set                                     |
| `Schema validation failed` | Update MCP server to latest version                            |

### Getting Help

1. Check [MCP Error Handling Guide](MCP_ERROR_HANDLING.md)
2. Review [David Fowler's dotnet-skillz](https://github.com/davidfowl/dotnet-skillz)
3. Open an issue on [GitHub](https://github.com/rudironsoni/dotnet-harness/issues)

---

## Next Steps

- [ ] Explore existing skills in `.opencode/skill/`
- [ ] Create your first custom skill
- [ ] Share your skill with the community
- [ ] Contribute to dotnet-harness

**Time to complete:** ~5 minutes for basic setup, ~30 minutes for first custom skill.
