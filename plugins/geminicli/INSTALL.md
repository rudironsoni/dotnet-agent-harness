# Gemini CLI Extension Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/geminicli
  GEMINI_API_KEY=<your-key> gemini extensions install "$TMP_DIR/dotnet-harness-toolkit/plugins/geminicli" --consent
  GEMINI_API_KEY=<your-key> gemini extensions list
)
```
