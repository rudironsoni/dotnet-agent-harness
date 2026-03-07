# Antigravity Verification Checklist

**Target ID:** `antigravity`  
**Runtime Shape:** Rules, commands, skills  
**Special Behavior:** Concise, globally safe rules; no MCP or subagents assumed  
**Verification Date:** ___________  
**Verified By:** ___________

---

## Expected Files Generated

### Primary Configuration
- [ ] `.antigravity/ANTIGRAVITY.md` or similar root rule
- [ ] Root rule is concise and portable

### Rules Directory
- [ ] `.antigravity/rules/*.md` - Individual rule files from `.rulesync/rules/`
- [ ] `.antigravity/rules/antigravity-overrides.md` - Target-specific overrides
- [ ] Rules are concise and globally safe

### Commands Directory
- [ ] `.antigravity/commands/*.md` - Command definitions
- [ ] Commands are simple and portable

### Skills Directory
- [ ] `.antigravity/skills/**/*.md` - Skills from `.rulesync/skills/`
- [ ] Skills are self-contained

### No MCP Configuration
- [ ] **NO** mcp.json file (MCP not assumed)
- [ ] MCP references removed from rules

### No Subagents
- [ ] **NO** agents/ directory
- [ ] Subagent content in rules or skills

### No Hooks
- [ ] **NO** hooks configuration
- [ ] Hook-like behavior in rules if needed

---

## Feature Support Matrix

| Feature | Status | Notes |
|---------|--------|-------|
| Rules | Native | **Primary surface** - concise, safe |
| Ignore Patterns | N/A | May use standard ignore |
| Commands | Native | Simple, portable commands |
| Subagents | N/A | Not supported |
| Skills | Native | Self-contained skills |
| Hooks | N/A | Not supported |
| MCP | N/A | Not assumed available |
| Focus | Portable | Works when injected automatically |

**Key Differences from Claude Code:**
- **Concise rules** - Brief, focused guidance
- **Globally safe** - No assumptions about environment
- **No MCP** - MCP servers not assumed available
- **No subagents** - All guidance in rules/skills
- **Automatic injection** - Rules may be auto-injected

---

## Rules Verification

### Root Rule Location
- [ ] Root rule exists in Antigravity format
- [ ] Content is concise and focused
- [ ] Globally safe (no environment assumptions)
- [ ] Suitable for automatic injection

### Concise Rule Characteristics
Verify rules are:
- [ ] Brief (not verbose)
- [ ] Focused on core concepts
- [ ] Safe to inject anywhere
- [ ] No file path assumptions
- [ ] No tool dependencies assumed

### Root Rule Content
The root rule should include:
- [ ] Brief overview of dotnet-agent-harness
- [ ] Essential guidance only
- [ ] Core principles summary
- [ ] Reference to skills for details

### Non-Root Rules
- [ ] Rules from `.rulesync/rules/` adapted for Antigravity
- [ ] Each rule is concise
- [ ] Rules have `targets` including `'antigravity'`
- [ ] `antigravity.trigger` specified where appropriate

### Trigger Configuration
```yaml
antigravity:
  trigger: always_on  # Or other appropriate trigger
```

---

## Commands Verification

### Location and Format
- [ ] Commands located in `.antigravity/commands/`
- [ ] Each command file named `{command-name}.md`
- [ ] Commands are simple and portable

### Command Characteristics
Verify commands are:
- [ ] Simple (not complex workflows)
- [ ] Self-contained
- [ ] No MCP dependencies
- [ ] No subagent invocations
- [ ] Work in any environment

### Command Content
- [ ] Commands include basic description
- [ ] Simple execution contract
- [ ] Minimal options
- [ ] Clear examples

---

## Skills Verification

### Location and Format
- [ ] Skills in `.antigravity/skills/{skill-name}/SKILL.md`
- [ ] Each skill is self-contained
- [ ] No external dependencies

### Skill Characteristics
Verify skills are:
- [ ] Self-contained (no MCP references)
- [ ] Focused on core knowledge
- [ ] Safe for any project type
- [ ] No file system assumptions

### Skill Content
- [ ] Scope clearly defined
- [ ] Core patterns documented
- [ ] Cross-references minimized
- [ ] Works without MCP

---

## MCP Verification

### Important: No MCP Assumed

Antigravity does NOT assume MCP availability:

- [ ] **NO** mcp.json file
- [ ] **NO** `[mcp:server]` references in rules
- [ ] **NO** MCP tool dependencies

### Fallback Patterns
When MCP would be used, provide alternatives:
```markdown
<!-- Instead of: -->
Use [mcp:serena] for code analysis.

<!-- Use: -->
For code analysis, examine the file structure or use available tools.
```

---

## Subagents Verification

### Important: No Subagents

Antigravity does NOT support subagents:

- [ ] **NO** agents/ directory
- [ ] **NO** `[subagent:name]` references
- [ ] **NO** subagent invocation patterns

### Subagent Content Migration
Specialist guidance should be:
- [ ] Embedded in rules
- [ ] Included in skills
- [ ] Available as general guidance

---

## Hooks Verification

### Important: No Hooks

Antigravity does NOT support hooks:

- [ ] **NO** hooks.json references
- [ ] **NO** automatic execution assumptions
- [ ] **NO** hook-based workflows

### Advisory Content
If hook-like behavior is needed:
- [ ] Include as reminders in rules
- [ ] Make advisory (not automatic)
- [ ] User-initiated only

---

## Common Issues and Fixes

### Issue: Rules too verbose
**Symptom:** Antigravity rules are lengthy

**Verification Steps:**
1. Check rule word count
2. Verify concise guidance
3. Check for unnecessary detail

**Fix:** Condense rules:
```markdown
<!-- Before (verbose) -->
## dotnet-agent-harness

This comprehensive toolkit provides...
[500 words of detail]

<!-- After (concise) -->
## dotnet-agent-harness

.NET development guidance for C#, ASP.NET Core, MAUI, Blazor.
Key principle: Use dotnet-advisor skill for routing.
See skills for detailed guidance on specific topics.
```

---

### Issue: MCP references present
**Symptom:** Rules reference MCP servers

**Verification Steps:**
1. Check for `[mcp:server]` syntax
2. Verify no mcp.json exists
3. Check for MCP tool dependencies

**Fix:** Remove MCP references:
```markdown
<!-- Before -->
Query [mcp:microsoftdocs-mcp] for official guidance.

<!-- After -->
Refer to official Microsoft documentation for authoritative guidance.
```

---

### Issue: Environment assumptions
**Symptom:** Rules assume specific tools or setup

**Verification Steps:**
1. Check for tool dependencies
2. Verify no path assumptions
3. Check for environment requirements

**Fix:** Make guidance portable:
```markdown
<!-- Before -->
Run `dotnet format` after editing C# files.

<!-- After -->
If available, run `dotnet format` after editing C# files.
```

---

### Issue: Subagent references
**Symptom:** Rules reference subagents

**Verification Steps:**
1. Check for `[subagent:name]` syntax
2. Verify no agents/ directory
3. Check for agent invocation

**Fix:** Replace with general guidance:
```markdown
<!-- Before -->
Delegate to [subagent:dotnet-architect] for architecture review.

<!-- After -->
For architecture review, analyze solution structure and apply patterns.
```

---

### Issue: Complex commands
**Symptom:** Commands have many dependencies

**Verification Steps:**
1. Check command complexity
2. Verify no MCP dependencies
3. Check for subagent calls

**Fix:** Simplify commands:
```markdown
<!-- Before -->
Uses MCP servers, subagents, and complex workflows.

<!-- After -->
Simple command with clear, self-contained execution.
```

---

## Post-Verification Summary

### Critical Checks Passed
- [ ] Root rule concise and focused
- [ ] No mcp.json file
- [ ] No agents/ directory
- [ ] No hooks configuration
- [ ] Rules globally safe
- [ ] Commands simple and portable

### Antigravity-Specific Verification
- [ ] Rules suitable for automatic injection
- [ ] No environment assumptions
- [ ] No MCP dependencies
- [ ] No subagent references
- [ ] Guidance works anywhere

### Issues Found
| Issue | Severity | Status |
|-------|----------|--------|
| | | |

### Notes
___________
___________
