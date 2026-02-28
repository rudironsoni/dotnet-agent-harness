---
name: dotnet-harness-offline
description: 'Offline mode with local caching for air-gapped environments'
targets: ['*']
tags: ['harness', 'offline', 'cache']
version: '0.0.1'
author: 'dotnet-harness'
license: MIT
invocable: true
---

# Offline Mode

Enable offline operation with local skill and MCP caching.

## Activation

````bash
# Set offline mode
export DOTNET_HARNESS_OFFLINE=true

# Or use flag
rulesync generate --offline
```text
## Cache Structure

```text
.dotnet-harness/
├── cache/
│   ├── skills/          # Cached skill content
│   ├── mcp-responses/   # Cached MCP responses
│   └── registry/        # Cached registry data
└── index.json           # Search index
````

## Workflow

1. **Online Sync** (when connected):

   ```bash
   dotnet-harness:sync-cache
   ```

2. **Offline Operation**:
   - Uses cached skills exclusively
   - MCP servers return cached responses
   - Search uses local index

## Configuration

```json
{
  "offline": {
    "enabled": false,
    "cachePath": ".dotnet-harness/cache",
    "maxCacheAge": "7d",
    "fallbackEnabled": true
  }
}
```

## Commands

- `dotnet-harness:sync-cache` - Sync cache from remote
- `dotnet-harness:clear-cache` - Clear local cache
- `dotnet-harness:cache-status` - Show cache statistics

## MCP Fallback

When offline:

- Serena: Uses cached symbol data
- Context7: Returns cached library docs
- Microsoft Learn: Serves from local cache

## Best Practices

1. Sync before travel/air-gapped work
2. Keep cache under 500MB
3. Clear stale cache weekly
4. Use selective caching for large projects

## Troubleshooting

**Cache Miss**: Run `dotnet-harness:sync-cache` **Stale Data**: Clear and re-sync **Large Cache**: Exclude heavy MCP
servers
