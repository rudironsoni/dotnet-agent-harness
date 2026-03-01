# MCP Error Handling Guide

## Overview

This document defines error handling strategies for MCP (Model Context Protocol) integration in dotnet-agent-harness
skills.

## Failure Modes

### 1. Connection Timeout

**Symptoms:**

- Tool call hangs indefinitely
- Error: "Connection timed out"
- No response from MCP server

**Root Causes:**

- MCP server process didn't start
- Network latency (for HTTP servers)
- Resource constraints

**Resolution Strategy:**

```text
1. Check if server is running: timeout 5 <command> --version
2. Retry with exponential backoff (wait 1s, 2s, 4s)
3. After 3 failures: mark as unavailable, use fallback
```

**Fallback:** Use traditional tools (Read, Grep, Bash) instead of MCP tools

---

### 2. Authentication Failure

**Symptoms:**

- Error: "401 Unauthorized"
- Error: "Authentication required"
- Empty responses from authenticated endpoints

**Root Causes:**

- Missing or expired API tokens
- Incorrect token permissions
- Token not in environment variables

**Resolution Strategy:**

```text
1. Check environment variable: echo $TOKEN_NAME
2. Verify token in provider settings (GitHub, Docker, etc.)
3. Regenerate token if expired
4. Restart MCP server with new token
```

**Fallback:** Use manual API calls with explicit headers

---

### 3. Server Unavailable

**Symptoms:**

- Error: "Connection refused"
- Error: "Service unavailable (503)"
- Process exit with error code

**Root Causes:**

- MCP server not installed
- Port conflicts
- Service down (for HTTP servers)

**Resolution Strategy:**

```text
1. Check if command exists: which <command>
2. Install if missing: npm install -g <package>
3. Check for port conflicts: lsof -i :<port>
4. Restart service (for hosted MCP servers)
```

**Fallback:** Disable MCP-dependent features, use traditional tools

---

### 4. Schema Mismatch

**Symptoms:**

- Error: "Invalid request format"
- Error: "Schema validation failed"
- Unexpected response structure

**Root Causes:**

- MCP server version mismatch
- Breaking changes in MCP protocol
- Outdated client libraries

**Resolution Strategy:**

```text
1. Check MCP server version: <command> --version
2. Update to latest: npm update -g <package>
3. Check MCP specification compatibility
4. Adjust request format if needed
```

**Fallback:** Use older API endpoints or traditional tools

---

### 5. Permission Denied

**Symptoms:**

- Error: "403 Forbidden"
- Error: "Access denied"
- Tool not in allowed-tools list

**Root Causes:**

- Skill doesn't declare tool in allowed-tools
- User lacks permissions
- Rate limiting

**Resolution Strategy:**

```text
1. Add tool to skill's allowed-tools frontmatter
2. Verify user has required permissions
3. Check rate limit headers (for HTTP)
4. Wait before retrying
```

**Fallback:** Request manual permission or use alternative tool

---

### 6. Resource Exhaustion

**Symptoms:**

- Error: "Out of memory"
- Error: "Rate limit exceeded"
- Slow responses

**Root Causes:**

- Too many concurrent requests
- Large response payloads
- API rate limits

**Resolution Strategy:**

```text
1. Implement request queuing
2. Add pagination for large datasets
3. Cache responses when possible
4. Respect rate limit headers
```

**Fallback:** Batch operations or use cached data

---

## Implementation Patterns

### Retry Logic

```javascript
// Exponential backoff with max retries
async function withRetry(operation, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await operation();
    } catch (error) {
      if (i === maxRetries - 1) throw error;
      await sleep(Math.pow(2, i) * 1000); // 1s, 2s, 4s
    }
  }
}
```

### Availability Check

```javascript
// Check MCP server availability before use
async function isMCPAvailable(serverName) {
  try {
    await timeout(
      mcpServers[serverName].ping(),
      5000 // 5 second timeout
    );
    return true;
  } catch {
    return false;
  }
}
```

### Fallback Chain

```javascript
// Try MCP first, fall back to alternatives
async function executeWithFallback(primary, fallback) {
  try {
    return await primary();
  } catch (error) {
    console.log(`Primary failed: ${error.message}`);
    console.log('Falling back to alternative...');
    return await fallback();
  }
}
```

---

## Error Messages

### User-Facing Messages

| Error Type  | Message                                     |
| ----------- | ------------------------------------------- |
| Timeout     | "MCP server not responding. Retrying..."    |
| Auth        | "Authentication failed. Check your token."  |
| Unavailable | "MCP service unavailable. Using fallback."  |
| Schema      | "Protocol version mismatch. Please update." |
| Permission  | "Tool not permitted. Add to allowed-tools." |

### Logging

Always log:

- Error type and code
- Timestamp
- Server name
- Retry attempt number
- Fallback action taken

---

## Best Practices

1. **Always check availability** before using MCP
2. **Implement graceful degradation** - skills should work without MCP
3. **Log all failures** for debugging
4. **Provide clear error messages** to users
5. **Use exponential backoff** for retries
6. **Respect rate limits** - don't hammer servers
7. **Cache when possible** - reduce unnecessary calls

---

## Testing

### Test Scenarios

1. **Happy Path**: MCP server available and working
2. **Timeout**: Simulate slow response
3. **Auth Failure**: Use invalid token
4. **Unavailable**: Stop MCP server
5. **Schema Mismatch**: Use old client version
6. **Rate Limit**: Exceed API limits

### Validation Checklist

- [ ] Timeout handling works correctly
- [ ] Auth failures show clear message
- [ ] Unavailable servers trigger fallback
- [ ] Schema errors handled gracefully
- [ ] Rate limiting respected
- [ ] All errors logged appropriately
