# Installation Matrix

This repository distributes agent bundles from `plugins/{agent}`.

| Target | Mode | Bundle path |
| --- | --- | --- |
| Claude Code | Plugin | `plugins/claudecode` |
| GitHub Copilot CLI | Plugin | `plugins/copilot` |
| Gemini CLI | Extension | `plugins/geminicli` |
| OpenCode | Plugin/config bundle | `plugins/opencode` |
| Codex CLI | Manual | `plugins/codexcli` |
| Antigravity | Manual | `plugins/antigravity` |

## Plugin installs

### Claude Code

```bash
claude plugin validate ./plugins/claudecode
claude plugin marketplace add ./plugins/claudecode
claude plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace
```

### GitHub Copilot CLI

```bash
npx -y @github/copilot plugin install ./plugins/copilot
```

### Gemini CLI

```bash
GEMINI_API_KEY=<your-key> gemini extensions validate ./plugins/geminicli
GEMINI_API_KEY=<your-key> gemini extensions install ./plugins/geminicli --consent
```

### OpenCode

```bash
cp -R ./plugins/opencode/.opencode ./.opencode
cp ./plugins/opencode/opencode.json ./opencode.json
cp ./plugins/opencode/AGENTS.md ./AGENTS.md
```

## Manual installs

### Codex CLI

```bash
cp ./plugins/codexcli/workspace/AGENTS.md ./AGENTS.md
cp -R ./plugins/codexcli/workspace/.codex ./
mkdir -p ~/.codex/prompts
cp -R ./plugins/codexcli/global/.codex/prompts/. ~/.codex/prompts/
```

### Antigravity

```bash
cp -R ./plugins/antigravity/workspace/.agent ./
mkdir -p ~/.gemini/antigravity/skills
cp -R ./plugins/antigravity/global/.gemini/antigravity/skills/. ~/.gemini/antigravity/skills/
```
