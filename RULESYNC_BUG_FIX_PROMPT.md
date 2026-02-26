# Fix RuleSync Bug: OpenCode Agent Mode Hardcoded to "subagent"

## Bug Description

RuleSync has a bug in `src/features/subagents/opencode-subagent.ts` where the OpenCode agent mode is hardcoded to `"subagent"`, ignoring the `mode` value specified in the Rulesync subagent frontmatter.

## Location

File: `src/features/subagents/opencode-subagent.ts`
Line: ~63 (in the `fromRulesyncSubagent` method)

## Current Buggy Code

```typescript
static fromRulesyncSubagent({
  baseDir = process.cwd(),
  rulesyncSubagent,
  validate = true,
  global = false,
}: ToolSubagentFromRulesyncSubagentParams): ToolSubagent {
  const rulesyncFrontmatter = rulesyncSubagent.getFrontmatter();
  const opencodeSection = rulesyncFrontmatter.opencode ?? {};

  const opencodeFrontmatter: OpenCodeSubagentFrontmatter = {
    ...opencodeSection,
    description: rulesyncFrontmatter.description,
    mode: "subagent",  // <-- BUG: This ignores opencodeSection.mode!
    ...(rulesyncFrontmatter.name && { name: rulesyncFrontmatter.name }),
  };
  // ... rest of method
}
```

## The Problem

When a Rulesync subagent specifies:
```yaml
---
name: dotnet-architect
description: "Analyzes .NET project..."
opencode:
  mode: "primary"  # User wants primary agent
  model: anthropic/claude-sonnet-4-20250514
---
```

RuleSync ignores `mode: "primary"` and always outputs `mode: "subagent"` in the generated OpenCode agent file.

## Required Fix

Change line 63 from:
```typescript
mode: "subagent",
```

To:
```typescript
mode: opencodeSection.mode ?? "subagent",
```

This preserves the user's specified mode while defaulting to "subagent" if not specified.

## Full Context

The fix should be in the `fromRulesyncSubagent` method. Here's the corrected version of that section:

```typescript
const opencodeFrontmatter: OpenCodeSubagentFrontmatter = {
  ...opencodeSection,
  description: rulesyncFrontmatter.description,
  mode: opencodeSection.mode ?? "subagent",  // FIXED: Respect user's mode
  ...(rulesyncFrontmatter.name && { name: rulesyncFrontmatter.name }),
};
```

## Why This Matters

OpenCode has two agent modes:
- **primary**: Appears in Tab cycling (main agent rotation)
- **subagent**: Invoked via @mention only

Users need to set `mode: "primary"` in their Rulesync subagents to make them appear as main agents in OpenCode's Tab interface. Currently, this is impossible due to the hardcoded value.

## Testing

1. Create a test subagent with `opencode.mode: "primary"`:
```yaml
---
name: test-primary-agent
description: "A test primary agent"
opencode:
  mode: "primary"
  model: anthropic/claude-sonnet-4-20250514
---

# Test Agent

This is a test agent.
```

2. Save it in your Rulesync project at `subagents/test-primary-agent.md`

3. Run: `rulesync generate --targets opencode`

4. Check the generated file at `.opencode/agent/test-primary-agent.md`

5. **Expected result**: Frontmatter should contain `mode: "primary"`
   **Bug behavior**: Frontmatter contains `mode: "subagent"` (wrong!)

## Additional Considerations

- The fix should maintain backward compatibility (default to "subagent" if not specified)
- Check if there are TypeScript type constraints that need updating
- Verify the `OpenCodeSubagentFrontmatterSchema` allows "primary" as a valid mode value

## Files to Modify

- `src/features/subagents/opencode-subagent.ts` (line ~63)

## Success Criteria

- [ ] User-specified `opencode.mode` in Rulesync frontmatter is preserved in generated OpenCode agent
- [ ] Default remains `"subagent"` when mode is not specified
- [ ] TypeScript compiles without errors
- [ ] Existing tests pass (or are updated if needed)
