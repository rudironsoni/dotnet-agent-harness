# Antigravity Manual Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/antigravity
  BUNDLE_DIR="$TMP_DIR/dotnet-harness-toolkit/plugins/antigravity"
  cp -R "$BUNDLE_DIR/workspace/.agent" "./"
  mkdir -p ~/.gemini/antigravity/skills
  cp -R "$BUNDLE_DIR/global/.gemini/antigravity/skills/." ~/.gemini/antigravity/skills/
)
```
