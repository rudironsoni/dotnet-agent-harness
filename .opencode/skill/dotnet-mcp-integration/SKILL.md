---
name: dotnet-mcp-integration
description: 'Integrates MCP servers for external tool connectivity in dotnet-harness skills'
triggered_on:
  - "mcp"
  - "model context protocol"
  - ".mcp.json"
allowed-tools:
  - Read
  - Bash
  - Write
  - Edit
---

# dotnet-mcp-integration

> Configure and use MCP servers to extend dotnet-harness skills with external tools and APIs.

This skill enables integration with Model Context Protocol (MCP) servers, allowing skills to access external services like GitHub, Docker, Microsoft Docs, and custom tools. MCP provides a standardized way for agents to discover and invoke external capabilities.

## Scope

- **In scope:**
  - Reading and validating `.mcp.json` configuration
  - Configuring MCP servers (stdio and http)
  - Error handling for MCP connection failures
  - Fallback strategies when MCP is unavailable
  - Best practices for MCP integration in skills

- **Out of scope:**
  - Creating custom MCP servers (see [skill:mcp-server-authoring])
  - MCP protocol internals (see [skill:mcp-protocol])
  - Specific tool usage (see individual skill documentation)

---

## Quick Start

```bash
# Read current MCP configuration
Read: .mcp.json

# Validate MCP servers are configured
Bash: cat .mcp.json | jq '.mcpServers | keys'
```

### Expected Output
```json
[
  "serena",
  "context7",
  "microsoftdocs-mcp",
  "github-mcp",
  "docker-mcp",
  "deepwiki",
  "mcp-windbg"
]
```

---

## Core Workflows

### Workflow 1: Configure a New MCP Server

**When to use:** Adding a new MCP server to the project

```bash
# Step 1: Read existing configuration
Read: .mcp.json

# Step 2: Add new server configuration
Edit: .mcp.json

# Step 3: Validate the configuration
Bash: cat .mcp.json | jq empty
```

**Example server configuration:**

```json
{
  "mcpServers": {
    "example-server": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@example/mcp-server@latest"],
      "env": {}
    }
  }
}
```

**Expected result:** Configuration validates without errors

### Workflow 2: Handle MCP Connection Failure

**When to use:** MCP server fails to connect or is unavailable

```bash
# Step 1: Check if MCP server is available
Bash: timeout 5 <mcp-command> --version 2>&1 || echo "MCP unavailable"

# Step 2: Fall back to traditional tools if needed
# Use Read, Grep, Bash instead of MCP-specific tools
```

**Fallback strategy:**
```
if MCP_AVAILABLE:
  use MCP tools (serena_find_symbol, github-mcp, etc.)
else:
  use traditional tools (Read, Grep, Bash)
  log: "MCP unavailable, using fallback"
```

---

## Common Commands

| Command | Purpose | Example |
|---------|---------|---------|
| `cat .mcp.json \| jq '.mcpServers'` | List all MCP servers | Shows configured servers |
| `cat .mcp.json \| jq '.mcpServers.<name>'` | Get specific server config | Shows server details |
| `timeout 5 <cmd> --version` | Check MCP availability | Tests if server responds |

---

## Configuration & Options

### Required Setup

- `.mcp.json` file at project root
- At least one MCP server configured
- Proper tool permissions in skill frontmatter

### MCP Server Types

**stdio (Standard I/O):**
```json
{
  "type": "stdio",
  "command": "npx",
  "args": ["-y", "@modelcontextprotocol/server-github"],
  "env": {
    "GITHUB_PERSONAL_ACCESS_TOKEN": "${GITHUB_TOKEN}"
  }
}
```

**http (HTTP endpoint):**
```json
{
  "type": "http",
  "url": "https://learn.microsoft.com/api/mcp"
}
```

---

## Edge Cases & Troubleshooting

### MCP Server Not Responding

**Symptom:** Tool calls timeout or return connection errors

**Solution:**
```bash
# Test server availability
timeout 5 npx -y @modelcontextprotocol/server-github --version

# If failing, use fallback:
# - For GitHub: use curl API calls instead of github-mcp
# - For docs: use web search instead of microsoftdocs-mcp
```

### Authentication Failures

**Symptom:** 401 Unauthorized errors from MCP servers

**Solution:**
1. Check environment variables are set: `echo $GITHUB_TOKEN`
2. Verify token permissions in GitHub/GitLab settings
3. Use fallback to manual API calls with explicit headers

### Schema Mismatch

**Symptom:** MCP server rejects requests with schema validation errors

**Solution:**
```bash
# Check MCP server version
npm show @modelcontextprotocol/server-github version

# Update if outdated
npm update -g @modelcontextprotocol/server-github
```

---

## Related Skills

- [skill:dotnet-version-detection] - Detect .NET version first
- [skill:github-mcp] - GitHub-specific MCP operations
- [skill:docker-mcp] - Docker container management via MCP
- [skill:microsoft-learn-mcp] - Microsoft Learn documentation access

---

## Best Practices

1. **Always declare MCP tools in frontmatter** - Explicitly list which MCP servers your skill uses
2. **Implement graceful degradation** - Provide fallback behavior when MCP is unavailable
3. **Validate configuration** - Check `.mcp.json` exists and is valid before using MCP
4. **Prefer stdio for local tools** - Use stdio type for CLI-based MCP servers
5. **Use http for hosted services** - Use http type for cloud-based MCP endpoints

---

## Technical Reference

### Supported MCP Servers

| Server | Type | Purpose |
|--------|------|---------|
| serena | stdio | Code symbol navigation via LSP |
| context7 | stdio | Library documentation queries |
| microsoftdocs-mcp | http | Microsoft Learn documentation |
| github-mcp | stdio | GitHub repository operations |
| docker-mcp | stdio | Container management |
| deepwiki | http | Repository documentation |
| mcp-windbg | stdio | Windows debugging |

### Default Behavior

- MCP tools are available if `.mcp.json` is present and valid
- Skills should check for MCP availability before relying on it
- Fallback to traditional tools is always recommended

### Performance Considerations

- MCP stdio servers have startup cost (use `npx -y` to ensure latest)
- HTTP servers may have network latency
- Cache MCP responses when possible to reduce round trips
