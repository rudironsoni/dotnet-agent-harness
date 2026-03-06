# RuleSync Authoring Guide

## Overview

RuleSync is the source format for dotnet-agent-harness. This guide covers authoring content in the `.rulesync/` directory.

## Directory Structure

```
.rulesync/
├── rules/
│   ├── 00-overview.md
│   ├── 10-skills.md
│   └── 20-commands.md
├── agents/
│   ├── dotnet-advisor.md
│   └── dotnet-architect.md
├── subagents/
│   ├── dotnet-coder.md
│   └── dotnet-tester.md
├── skills/
│   ├── dotnet-efcore-patterns.md
│   └── dotnet-testing.md
└── templates/
    ├── claude/
    │   └── bundle.json
    └── opencode/
        └── bundle.json
```

## Rule Files

Rules define global behavior and constraints:

```markdown
---
name: dotnet-skills
version: "2.0"
applies_to: ["*.cs", "*.csproj"]
---

# Skill Usage Guidelines

When implementing [skill:dotnet-efcore-patterns]:
1. Always use async/await for database operations
2. Prefer IQueryable over IEnumerable for deferred execution
3. Use AsNoTracking for read-only scenarios
```

## Agent Definitions

Agents provide specialized expertise:

```markdown
---
name: dotnet-advisor
role: architecture_advisor
triggers:
  - "what framework"
  - "recommend approach"
  - "architecture review"
---

# dotnet-advisor

Architecture advisor subagent for .NET projects...
```

## Subagent Definitions

Subagents handle specific tasks:

```markdown
---
name: dotnet-coder
parent: dotnet-advisor
capabilities: [code_generation, refactoring]
---

# dotnet-coder

Implements code changes with .NET best practices...
```

## Skill Definitions

Skills are self-contained guidance documents:

```markdown
---
name: dotnet-efcore-patterns
version: "1.0"
description: EF Core query optimization patterns
tags: [efcore, database, performance]
---

# dotnet-efcore-patterns

## Query Optimization

### AsNoTracking

Use for read-only scenarios...

## Code Example

```csharp
// Good: AsNoTracking for read-only
var users = await dbContext.Users
    .AsNoTracking()
    .ToListAsync();
```
```

## Best Practices

1. **Version your content**: Always include version in frontmatter
2. **Use semantic triggers**: Make triggers specific and unique
3. **Include examples**: Code examples improve AI understanding
4. **Test locally**: Run `npm run ci:rulesync` before committing
5. **Document dependencies**: Note related skills explicitly

## Validation

Validate your changes:

```bash
# Validate all .rulesync/ content
npm run ci:rulesync

# Validate specific file
rulesync validate --file .rulesync/skills/my-skill.md
```

## Common Issues

### Multiple root rules

Error: `Multiple root rulesync rules found`

Fix: Ensure only one file in `.rulesync/rules/` has `is_root: true`

### Invalid frontmatter

Error: `YAML parsing failed`

Fix: Check YAML syntax in frontmatter (between `---` markers)
