# Codex CLI Manual Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/codexcli
  BUNDLE_DIR="$TMP_DIR/dotnet-harness-toolkit/plugins/codexcli"
  cp "$BUNDLE_DIR/workspace/AGENTS.md" "AGENTS.md"
  cp -R "$BUNDLE_DIR/workspace/.codex" "./"
  mkdir -p ~/.codex/prompts
  cp -R "$BUNDLE_DIR/global/.codex/prompts/." ~/.codex/prompts/
)
```
