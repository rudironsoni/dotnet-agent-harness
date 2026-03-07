# Verification Strategy for dotnet-agent-harness

This directory contains per-target verification checklists for the RuleSync-based multi-agent toolkit.

## Purpose

Verification ensures that generated agent configurations work correctly across all supported AI agent platforms:

- **Claude Code** - Full feature support with rules, skills, commands, subagents, hooks, MCP
- **OpenCode** - Tab-based primary agents with `@mention` subagent invocation
- **GitHub Copilot CLI** - Tool-based execution with implicit subagent support
- **Gemini CLI** - Rules, MCP, commands, skills, and hooks (no direct subagents)
- **Codex CLI** - Read-only sandbox with rules, MCP, subagents, skills
- **Antigravity** - Rules, commands, skills (no MCP or subagents)
- **Factory Droid** - Rules, MCP, hooks (via generated rules, not imported surfaces)

## How to Run Verification

### Automated Verification

Run RuleSync with the check flag:

```bash
rulesync generate --targets "claudecode,codexcli,opencode,geminicli,antigravity,copilot,factorydroid" --features "*" --check
```

This validates:
- File structure correctness
- Required files presence per target
- Cross-reference consistency
- Frontmatter validity

### Manual Verification

Use target-specific checklists to verify generated outputs:

1. **Generate outputs**:
   ```bash
   rulesync generate --targets "<target>" --features "*"
   ```

2. **Run target checklist**:
   - Open the corresponding checklist file
   - Verify each section systematically
   - Mark items as verified

3. **Test in target environment**:
   - Load generated configuration in target agent
   - Verify commands are invocable
   - Verify skills are loadable
   - Test hooks fire correctly

## Checklist Structure

Each checklist documents:

| Section | Purpose |
|---------|---------|
| **Expected Files** | What files should exist and where |
| **Feature Support Matrix** | What's natively supported vs simulated |
| **Rules Verification** | Root vs non-root rule locations and formats |
| **Commands Verification** | Command location, format, and invocability |
| **Subagents Verification** | Subagent registration and invocation paths |
| **Skills Verification** | Skill loading mechanisms and paths |
| **Hooks Verification** | Event types supported and hook execution |
| **MCP Verification** | Server registration and tool availability |
| **Ignore Verification** | Ignore patterns and exclusions |
| **Common Issues** | Known problems and verification fixes |

## Verification Coverage Matrix

| Target | Rules | Commands | Subagents | Skills | Hooks | MCP | Ignore |
|--------|-------|----------|-----------|--------|-------|-----|--------|
| claudecode | Full | Full | Full | Full | Full | Full | Full |
| opencode | Full | Full | `@mention` | Full | Full | Full | - |
| copilot | Full | Full | Implicit | Full | - | Full | - |
| geminicli | Full | Full | - | Full | Full | Full | Full |
| codexcli | Full | - | Read-only | Full | - | Full | Full |
| antigravity | Full | Full | - | Full | - | - | - |
| factorydroid | Full | - | - | - | Via rules | Full | - |

## Legend

- **Full** - Native support with full feature set
- **Partial** - Supported with limitations
- **Via rules** - Delivered through generated rules, not direct import
- **Implicit** - Available without explicit configuration
- **`-`** - Not applicable or not supported

## Quick Reference

| If you need to verify... | Use checklist... |
|--------------------------|------------------|
| Claude Code workspace | `claudecode-checklist.md` |
| OpenCode agent config | `opencode-checklist.md` |
| Copilot CLI integration | `copilot-checklist.md` |
| Gemini CLI setup | `geminicli-checklist.md` |
| Codex CLI sandbox | `codexcli-checklist.md` |
| Antigravity rules | `antigravity-checklist.md` |
| Factory Droid workspace | `factorydroid-checklist.md` |

## Troubleshooting

### RuleSync Generation Failures

If `rulesync generate --check` fails:

1. Check frontmatter validity in source files
2. Verify no duplicate root rules exist
3. Ensure all referenced skills exist in `.rulesync/skills/`
4. Validate JSON syntax in `.rulesync/hooks.json` and `.rulesync/mcp.json`

### Missing Generated Files

If expected files are not generated:

1. Check target is in the `--targets` list
2. Verify features are not excluded for that target
3. Review target-specific blocks in source files
4. Check file globs match intended patterns

### Hook Execution Issues

If hooks don't fire:

1. Verify hook JSON syntax is valid
2. Check command paths are correct relative to workspace
3. Ensure executable permissions on shell scripts
4. Review timeout values are appropriate

### MCP Connection Failures

If MCP servers don't connect:

1. Verify `mcp.json` syntax
2. Check required tools are installed (uvx, node, etc.)
3. Validate API keys are configured
4. Test server connectivity independently

## Updating Checklists

When adding new features or targets:

1. Update the source RuleSync files in `.rulesync/`
2. Regenerate outputs: `rulesync generate --targets "*" --features "*"`
3. Update the relevant checklist(s)
4. Run verification: `rulesync generate --check`
5. Test in target environment
6. Update this README if adding new targets or verification methods

## Related Resources

- [Target Surfaces](../rules/15-target-surfaces.md) - Runtime shape documentation
- [Workflow](../rules/20-workflow.md) - RuleSync workflow guidance
- [MCP Configuration](../mcp.json) - MCP server definitions
- [Hooks Configuration](../hooks.json) - Hook event definitions
