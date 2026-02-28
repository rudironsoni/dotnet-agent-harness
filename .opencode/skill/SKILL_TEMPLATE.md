---
name: skill-name
suggested_name: <skill-name-descriptive>  # e.g., "dotnet-{topic}" or "{framework}-{action}"
description: 'One-line description of what this skill does'
triggered_on:  # When to invoke this skill (e.g., specific commands, patterns, topics)
  - "keyword"
  - "pattern"
allowed-tools:
  - Read
  - Glob
  - Grep
  - Write
  - Edit
  # Add specific tools as needed, e.g.:
  # - Bash(command-pattern)
---

# {Skill Name}

> One-sentence summary of the skill's purpose.

{Longer description explaining what problems this skill solves and when to use it. Reference related skills where appropriate.}

## Scope

- **In scope:** What this skill handles
- **Out of scope:** What this skill does NOT handle (with references to other skills)

---

## Quick Start

```bash
# Most common use case
<command>
```

### Expected Output
```
<example output>
```

---

## Core Workflows

### Workflow 1: {Descriptive Name}

**When to use:** Explanation of the scenario

```bash
# Step 1: Description
<command>

# Step 2: Description
<command>
```

**Expected result:** What success looks like

### Workflow 2: {Another Scenario}

**When to use:** Explanation

```bash
# Commands here
```

---

## Common Commands

| Command | Purpose | Example |
|---------|---------|---------|
| `<cmd>` | Description | `<cmd> --flag` |
| `<cmd>` | Description | `<cmd> <arg>` |

---

## Configuration & Options

### Required Setup

- Prerequisite 1
- Prerequisite 2

### Optional Configuration

```json
{
  "optional": "config"
}
```

---

## Edge Cases & Troubleshooting

### {Common Issue}

**Symptom:** Description

**Solution:**
```bash
<fix command>
```

### {Another Issue}

**Symptom:** Description

**Solution:** Steps to resolve

---

## Related Skills

- [skill:{related-skill-1}] - For {specific use case}
- [skill:{related-skill-2}] - For {specific use case}

---

## Best Practices

1. **Do this:** Explanation
2. **Avoid this:** Explanation with alternative
3. **Prefer:** When to choose one approach over another

---

## Technical Reference

### Supported Versions

| Version | Status | Notes |
|---------|--------|-------|
| X.Y | LTS/STS | Notes |

### Default Behavior

- Default setting 1
- Default setting 2

### Performance Considerations

- Performance tip 1
- Performance tip 2
