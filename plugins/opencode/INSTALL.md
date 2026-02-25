# OpenCode Plugin Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/opencode
  BUNDLE_DIR="$TMP_DIR/dotnet-harness-toolkit/plugins/opencode"
  cp -R "$BUNDLE_DIR/.opencode" ".opencode"
  cp "$BUNDLE_DIR/opencode.json" "opencode.json"
  cp "$BUNDLE_DIR/AGENTS.md" "AGENTS.md"
  opencode agent list
)
```
