# Copilot CLI Plugin Install

```bash
REPO_BASE_URL="${REPO_BASE_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit/tree/main}"
npx -y @github/copilot plugin marketplace add "$REPO_BASE_URL/plugins/copilot"
npx -y @github/copilot plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace
npx -y @github/copilot plugin list
```
