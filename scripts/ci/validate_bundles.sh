#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "$SCRIPT_DIR/../lib/common.sh"

PLUGINS_DIR="$REPO_ROOT/plugins"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --plugins-dir)
      PLUGINS_DIR="$2"
      shift 2
      ;;
    *)
      fail "Unknown argument: $1"
      ;;
  esac
done

require_dir() {
  local dir_path="$1"
  if [[ ! -d "$dir_path" ]]; then
    fail "Required directory missing: $dir_path"
  fi
}

require_file() {
  local file_path="$1"
  if [[ ! -f "$file_path" ]]; then
    fail "Required file missing: $file_path"
  fi
}

log "Validating plugin bundle directory structure"
require_dir "$PLUGINS_DIR/claudecode"
require_dir "$PLUGINS_DIR/copilot"
require_dir "$PLUGINS_DIR/geminicli"
require_dir "$PLUGINS_DIR/opencode"
require_dir "$PLUGINS_DIR/codexcli"
require_dir "$PLUGINS_DIR/antigravity"

require_file "$PLUGINS_DIR/claudecode/.claude-plugin/plugin.json"
require_file "$PLUGINS_DIR/claudecode/.claude-plugin/marketplace.json"
require_file "$PLUGINS_DIR/copilot/plugin.json"
require_file "$PLUGINS_DIR/geminicli/gemini-extension.json"
require_file "$PLUGINS_DIR/opencode/.opencode/plugins/rulesync-hooks.js"
require_file "$PLUGINS_DIR/codexcli/INSTALL.md"
require_file "$PLUGINS_DIR/antigravity/INSTALL.md"

log "Validating bundle manifests"
jq -e '.name == "dotnet-harness-toolkit" and (.version | type == "string")' \
  "$PLUGINS_DIR/claudecode/.claude-plugin/plugin.json" >/dev/null

jq -e '.name == "dotnet-harness-toolkit-marketplace" and (.plugins | length > 0)' \
  "$PLUGINS_DIR/claudecode/.claude-plugin/marketplace.json" >/dev/null

jq -e '.name == "dotnet-harness-toolkit" and (.version | type == "string")' \
  "$PLUGINS_DIR/copilot/plugin.json" >/dev/null

jq -e '.name == "dotnet-harness-toolkit" and (.version | type == "string") and (.contextFileName == "GEMINI.md")' \
  "$PLUGINS_DIR/geminicli/gemini-extension.json" >/dev/null

log "Running Claude plugin validation"
run_claude plugin validate "$PLUGINS_DIR/claudecode"

log "Running Gemini extension validation"
GEMINI_API_KEY="dummy-key" run_gemini extensions validate "$PLUGINS_DIR/geminicli"

log "Checking OpenCode plugin JavaScript syntax"
node --check "$PLUGINS_DIR/opencode/.opencode/plugins/rulesync-hooks.js"

log "Verifying manual bundle contents"
require_file "$PLUGINS_DIR/codexcli/workspace/AGENTS.md"
require_dir "$PLUGINS_DIR/codexcli/workspace/.codex"
require_dir "$PLUGINS_DIR/antigravity/workspace/.agent/rules"
require_dir "$PLUGINS_DIR/antigravity/workspace/.agent/workflows"
require_dir "$PLUGINS_DIR/antigravity/workspace/.agent/skills"

log "Bundle validation completed successfully"
