# Claude Code Plugin Install

```bash
REPO_BASE_URL="${REPO_BASE_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit/tree/main}"
claude plugin marketplace add "$REPO_BASE_URL/plugins/claudecode"
claude plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace
claude agents
```
