# GitHub Copilot Agent Skills Update Log

This document records important updates to VS Code / GitHub Copilot, and the impact and notes for using .NET Testing Agent Skills.

**Target Audience**: Developers using GitHub Copilot with this project's dotnet-testing Agent Skills

---

## 📋 Update Log

| Date | VS Code Version | Key Changes |
| ---- | --------------- | ----------- |
| 2026-02-04 | v1.109 | [Agent Skills GA, Flexible Search Paths, Multi-Tool Workspace Sharing](#vs-code-v1109-2026-02-04) |

---

## VS Code v1.109 (2026-02-04)

**Reference**: [VS Code v1.109 Release Notes - Agent Customization](https://code.visualstudio.com/updates/v1_109#_agent-customization)

---

### 🎯 Key Summary (TL;DR)

- ✅ **Agent Skills Officially GA**: Enabled by default, no longer need to manually enable `chat.useAgentSkills` setting
- ✅ **No Action Required for Existing Users**: Skills continue to work in `.github/skills/`, completely unaffected
- ✅ **Flexible Search Paths**: VS Code now searches `.github/skills/`, `.claude/skills/`, and other paths simultaneously
- ✅ **Multi-Tool Sharing**: When using multiple AI tools, skills only need to be in one location, no more duplicate copies or symlinks needed
- ✅ **Diagnostics Tool**: New Chat Customization Diagnostics for troubleshooting skills loading issues

> 💡 **For Pure GitHub Copilot Users**: If you only use GitHub Copilot, skills continue to work in `.github/skills/` (official recommended path), **no changes needed**. This update primarily simplifies scenarios where multiple AI tools are used.

---

### 📋 Before vs After Comparison Table

| Item | Before (< v1.109) | After (v1.109+) |
| ---- | ----------------- | ----------------- |
| Agent Skills Enablement | Need to manually enable `chat.useAgentSkills` | Enabled by default (GA) |
| Skills Search Paths | Only search `.github/skills/` | Automatically search `.github/skills/`, `.claude/skills/`, `~/.copilot/skills/`, etc. |
| Multi AI Tool Sharing | Need to copy skills or create symlinks for each tool | Single directory usable by multiple tools |
| Custom Search Paths | Not supported | Custom paths via `chat.agentSkillsLocations` setting |
| Debugging Tools | None | Chat Customization Diagnostics view |
| Extension Distribution | Not supported | Package and distribute via `chatSkills` contribution point |
| Organization-Level Instructions | Not supported | `github.copilot.chat.organizationInstructions.enabled` |

---

### 🚀 Agent Skills Officially Released (GA)

VS Code v1.109 upgrades Agent Skills from experimental to **Generally Available (GA)**, and **enabled by default**.

This means:

- **No longer need** to go to VS Code settings and manually enable `chat.useAgentSkills`
- After updating to v1.109, Agent Skills will work automatically
- Previously enabled `chat.useAgentSkills` users are unaffected, setting remains but no longer needs manual management

> ⚠️ **Note**: The three-step enablement process mentioned in the "VS Code Settings" section of [SKILLS_USAGE_GUIDE.md](SKILLS_USAGE_GUIDE.md) (Open Settings - Search `chat.useAgentSkills` - Check to enable) is no longer needed in v1.109+.

---

### 🔍 Flexible Skills Search Paths

Before v1.109, GitHub Copilot only searched for skills in the `.github/skills/` directory. After the update, VS Code automatically searches in multiple paths:

| Search Path | Scope | Description |
| ----------- | ----- | ----------- |
| **`.github/skills/`** | Workspace | **GitHub Copilot official recommended path** (unchanged) |
| `.claude/skills/` | Workspace | Claude Code default path, now Copilot also auto-searches |
| `~/.copilot/skills/` | User Home | User global Copilot skills |
| `~/.claude/skills/` | User Home | User global Claude skills, Copilot also searches |
| `chat.agentSkillsLocations` configured paths | Custom | Custom paths specified via VS Code settings |

> ✅ **For Pure Copilot Users**: `.github/skills/` remains the official recommended path for GitHub Copilot. If you only use GitHub Copilot, skills continue to work in `.github/skills/`, **no changes needed**.

The new search paths primarily benefit users of multiple AI tools - for example, Claude Code users who have already installed skills in `.claude/skills/` will now have GitHub Copilot automatically find and load these skills without additional copying.

#### Pure GitHub Copilot User Directory Structure (Recommended, same as before)

```plaintext
your-project/
├── .github/
│   └── skills/                          ← GitHub Copilot official recommended path
│       ├── dotnet-testing/
│       ├── dotnet-testing-advanced/
│       ├── dotnet-testing-unit-test-fundamentals/
│       ├── dotnet-testing-autofixture-basics/
│       └── ... (total 29 skills)
├── src/
│   └── MyProject/
└── tests/
    └── MyProject.Tests/
```

---

### 🌐 Multi AI Tool Workspace Sharing

> 💡 **This section applies to users using multiple AI tools (such as GitHub Copilot + Claude Code)**. If you only use GitHub Copilot, skills continue to work in `.github/skills/`, you can skip this section.

This is one of the most impactful changes in v1.109. Previously, when using multiple AI tools in the same workspace, you needed to prepare separate skills directories for each tool:

#### Previous Approach (before v1.109)

```plaintext
your-project/
├── .github/
│   └── skills/              ← GitHub Copilot uses
│       ├── dotnet-testing/
│       └── ... (29 skills)
├── .claude/
│   └── skills/              ← Claude Code uses
│       ├── dotnet-testing/
│       └── ... (29 skills, duplicate copy)
└── .cursor/
    └── skills/              ← Cursor uses
        ├── dotnet-testing/
        └── ... (29 skills, another duplicate copy)
```

Problems with this approach:

- ❌ Need to maintain multiple copies of the same skills
- ❌ Updates need to be synchronized across multiple directories
- ❌ Or need to create symlinks to avoid duplication, increasing maintenance complexity

#### Current Approach (v1.109+)

Since GitHub Copilot now automatically searches `.claude/skills/` and other paths, when using multiple AI tools you only need to place skills in **one location**:

```plaintext
your-project/
├── .claude/
│   └── skills/              ← One copy of skills shared by Copilot and Claude Code
│       ├── dotnet-testing/
│       └── ... (29 skills)
├── src/
└── tests/
```

> ✅ GitHub Copilot v1.109+ → Auto-searches `.claude/skills/` ✅
> ✅ Claude Code → Originally supported `.claude/skills/` ✅
> ✅ No symlinks needed, no duplicate copies needed ✅

---

#### Choose Skills Directory Location Based on Use Case

##### Scenario A: Only Using GitHub Copilot (Recommended to maintain status quo)

Use GitHub Copilot official recommended path `.github/skills/`, **exactly the same as before, no changes needed**.

**Linux / macOS (Bash)**

```bash
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Copy to GitHub Copilot official recommended path
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# Done!
```

**Windows (PowerShell)**

```powershell
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Copy to GitHub Copilot official recommended path
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# Done!
```

##### Scenario B: Using GitHub Copilot + Claude Code Together

Place skills in `.claude/skills/`, both tools can access directly:

**Linux / macOS (Bash)**

```bash
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Copy to one location, both tools can use
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/

# Done! GitHub Copilot v1.109+ and Claude Code can both use
```

**Windows (PowerShell)**

```powershell
# Clone repo
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Copy to one location, both tools can use
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.claude\" -Recurse

# Done! GitHub Copilot v1.109+ and Claude Code can both use
```

##### Scenario C: Using Custom Shared Directory (For multi-tool or team scenarios)

Specify an AI-tool-agnostic shared directory name via `chat.agentSkillsLocations` setting:

```bash
# Create shared skills directory
cp -r dotnet-testing-agent-skills/skills /your-project/shared-skills/
```

Set in `.vscode/settings.json`:

```json
{
  "chat.agentSkillsLocations": [
    "./shared-skills"
  ]
}
```

##### Scenario D: Using User Home Directory (Global install, across all projects)

Install skills to user home directory, all projects can use:

**Linux / macOS (Bash)**

```bash
# Install to global Copilot skills path
cp -r dotnet-testing-agent-skills/skills/* ~/.copilot/skills/

# Or install to global Claude skills path (Copilot v1.109+ also searches here)
cp -r dotnet-testing-agent-skills/skills/* ~/.claude/skills/
```

**Windows (PowerShell)**

```powershell
# Install to global Copilot skills path
Copy-Item -Path "dotnet-testing-agent-skills\skills\*" -Destination "$HOME\.copilot\skills\" -Recurse

# Or install to global Claude skills path (Copilot v1.109+ also searches here)
Copy-Item -Path "dotnet-testing-agent-skills\skills\*" -Destination "$HOME\.claude\skills\" -Recurse
```

---

#### AI Tool × Directory Mapping Table

The following table summarizes the skills directories read by each AI tool:

| Directory Location | GitHub Copilot (< v1.109) | GitHub Copilot (v1.109+) | Claude Code | Cursor |
| ------------------ | ------------------------- | ------------------------ | ----------- | ------ |
| `.github/skills/` | ✅ | ✅ | ❌ | ❌ |
| `.claude/skills/` | ❌ | ✅ | ✅ | ❌ |
| `.cursor/skills/` | ❌ | ❌ | ❌ | ✅ |
| `~/.copilot/skills/` | ❌ | ✅ | ❌ | ❌ |
| `~/.claude/skills/` | ❌ | ✅ | ✅ | ❌ |
| `chat.agentSkillsLocations` custom path | ❌ | ✅ | ❌ | ❌ |

> 💡 **Recommendation**: Pure GitHub Copilot users continue using `.github/skills/` (official recommended). If using GitHub Copilot + Claude Code together, placing skills in `.claude/skills/` is the simplest sharing method. If also supporting Cursor, you can use `chat.agentSkillsLocations` to add `.cursor/skills/`, or just copy an extra copy for Cursor.

---

### ⚙️ chat.agentSkillsLocations Custom Path Configuration

`chat.agentSkillsLocations` is a new VS Code setting in v1.109, allowing you to specify additional skills search paths.

#### Basic Usage

Add setting in `.vscode/settings.json`:

```json
{
  "chat.agentSkillsLocations": [
    "./shared-skills",
    "./vendor/dotnet-testing-agent-skills/skills"
  ]
}
```

#### With Git Submodule

If using Git Submodule to install skills, you can point directly to submodule path:

```bash
# Add submodule
git submodule add https://github.com/kevintsengtw/dotnet-testing-agent-skills vendor/dotnet-testing-agent-skills
```

```json
{
  "chat.agentSkillsLocations": [
    "./vendor/dotnet-testing-agent-skills/skills"
  ]
}
```

#### Team Sharing

Commit `.vscode/settings.json` to version control, team members will automatically apply skills search path settings after cloning, no individual setup needed.

---

### 🔧 Chat Customization Diagnostics Tool

v1.109 adds Chat Customization Diagnostics view, making it easy to confirm skills are loaded correctly.

#### How to Use

1. Open GitHub Copilot Chat panel
2. **Right-click** in Chat panel
3. Select **Chat Customization Diagnostics**
4. View loaded skills list, file paths, and error messages

#### Diagnostics Content

- Lists all loaded customization files
- Shows each skill's loading status (success / failure)
- If errors, shows specific error messages

> 💡 **Debugging Tip**: If your .NET Testing Skills are not auto-triggering, use Chat Customization Diagnostics to confirm VS Code loaded skills correctly. Common issues include: incorrect directory paths, `SKILL.md` file format issues, frontmatter syntax errors, etc.

---

### 📦 Other v1.109 Related Updates

#### Extension Distribution Skills

v1.109 adds `chatSkills` contribution point, VS Code Extensions can package and distribute skills. This means .NET Testing Agent Skills could potentially be released as a **VS Code Extension** in the future, allowing one-click installation.

#### Organization-Level Instructions

Via `github.copilot.chat.organizationInstructions.enabled` setting, organizations can configure Copilot instructions uniformly, ensuring team members use consistent AI behavior standards. If your team wants to uniformly adopt .NET Testing Skills, you can promote this at the organization level with this feature.

#### `/init` Command

The new `/init` slash command automatically analyzes project structure and generates workspace instructions. This feature **complements** Agent Skills:

- `/init` generates project-level general instructions (coding style, project conventions)
- Agent Skills provide domain expertise (.NET testing best practices, framework usage guides)
- Both can be used simultaneously without conflict

---

### 📝 Installation Method Update Recommendations

After updating to VS Code v1.109, installing .NET Testing Agent Skills is simplified:

#### Pure GitHub Copilot Users (Same as before)

**Linux / macOS (Bash)**

```bash
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: Copy to GitHub Copilot official recommended path
cp -r dotnet-testing-agent-skills/skills /your-project/.github/

# Done! No need to manually enable chat.useAgentSkills after v1.109
```

**Windows (PowerShell)**

```powershell
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: Copy to GitHub Copilot official recommended path
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.github\" -Recurse

# Done! No need to manually enable chat.useAgentSkills after v1.109
```

#### Using GitHub Copilot + Claude Code Together (New simplified process)

**Linux / macOS (Bash)**

```bash
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: Copy to one location (shared by both tools)
cp -r dotnet-testing-agent-skills/skills /your-project/.claude/

# Done! GitHub Copilot v1.109+ and Claude Code can both use
```

**Windows (PowerShell)**

```powershell
# Step 1: Clone
git clone https://github.com/kevintsengtw/dotnet-testing-agent-skills.git

# Step 2: Copy to one location (shared by both tools)
Copy-Item -Path "dotnet-testing-agent-skills\skills" -Destination "\your-project\.claude\" -Recurse

# Done! GitHub Copilot v1.109+ and Claude Code can both use
```

#### Differences from Previous Installation Process

| Step | Before (< v1.109) | Now (v1.109+) |
| ---- | ----------------- | ----------------- |
| 1. Clone repo | Same | Same |
| 2. Copy skills | Need to copy to each tool's directory separately | Pure Copilot: `.github/skills/` (unchanged); Multi-tool: one location |
| 3. Enable Agent Skills | Manually enable `chat.useAgentSkills` in settings | **Not needed**, enabled by default |
| 4. Verify loading | No tool available | Use Chat Customization Diagnostics |

---

### ❓ FAQ

#### Q1: I already have skills installed in `.github/skills/`, do I need to move them?

**A**: **No**. `.github/skills/` is the official recommended path for GitHub Copilot, and it remains fully supported after the v1.109 update. If you only use GitHub Copilot, maintain status quo, no changes needed. Only consider adjusting directory locations if you use other AI tools like Claude Code and want to reduce duplicate copies.

---

#### Q2: Do I need to remove the `chat.useAgentSkills` setting?

**A**: No. This setting still exists in v1.109, just enabled by default. Keeping or removing it doesn't affect functionality.

---

#### Q3: I also use Cursor, does this update help?

**A**: Partially. Cursor still reads skills from `.cursor/skills/`, but you can use `chat.agentSkillsLocations` to make GitHub Copilot also search `.cursor/skills/`, reducing one duplicate. The best approach for full three-tool sharing is to use Strategy 2 (custom shared directory).

---

#### Q4: Has the Skills auto-trigger (Keywords) mechanism changed?

**A**: No. The mechanism for Skills to auto-match and trigger via Keywords in `description` is completely unchanged. v1.109 only changed the **search paths** for skills, not the content format and trigger logic.

---

#### Q5: How to confirm skills are loaded correctly?

**A**: Use the new Chat Customization Diagnostics in v1.109:

1. Open Copilot Chat panel
2. Right-click → Select Chat Customization Diagnostics
3. Confirm all 29 dotnet-testing skills appear in loaded list

If skills not loaded, check:

- Directory path is correct
- Each skill folder has `SKILL.md` file
- `SKILL.md` frontmatter format is correct

---

### 📚 Related Documentation

#### Project Documentation

- [SKILLS_USAGE_GUIDE.md](SKILLS_USAGE_GUIDE.md) - Complete user manual (installation, skills list, usage scenarios, FAQ)
- [SKILLS_QUICK_REFERENCE.md](SKILLS_QUICK_REFERENCE.md) - Quick reference guide (keyword mapping, prompt templates)
- [OPTIMIZATION_SUMMARY.md](OPTIMIZATION_SUMMARY.md) - Skills optimization summary report

#### External Links

- [VS Code v1.109 Release Notes - Agent Customization](https://code.visualstudio.com/updates/v1_109#_agent-customization)
- [agentskills.io Official Specification](https://agentskills.io)
- [GitHub Copilot Agent Skills Documentation](https://docs.github.com/copilot/using-github-copilot/using-github-copilot-agent-skills)

---

**Last Updated**: 2026-02-09
