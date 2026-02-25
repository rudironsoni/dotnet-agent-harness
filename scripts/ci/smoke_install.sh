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

TMP_ROOT="$(mktemp -d)"

cleanup() {
  rm -rf "$TMP_ROOT"
}
trap cleanup EXIT

log "Smoke test: Claude plugin install"
CLAUDE_HOME="$TMP_ROOT/claude-home"
mkdir -p "$CLAUDE_HOME"

CLAUDE_MARKETPLACE_PATH="$(realpath "$PLUGINS_DIR/claudecode")"

HOME="$CLAUDE_HOME" run_claude plugin marketplace add "$CLAUDE_MARKETPLACE_PATH"
HOME="$CLAUDE_HOME" run_claude plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace

CLAUDE_PLUGIN_LIST="$(HOME="$CLAUDE_HOME" run_claude plugin list)"
if ! grep -q "dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace" <<<"$CLAUDE_PLUGIN_LIST"; then
  fail "Claude plugin not found in plugin list"
fi

CLAUDE_AGENTS_LIST="$(HOME="$CLAUDE_HOME" run_claude agents)"
if ! grep -q "dotnet-harness-toolkit:dotnet-architect" <<<"$CLAUDE_AGENTS_LIST"; then
  fail "Claude plugin agents are not available after installation"
fi

log "Smoke test: Copilot plugin install"
COPILOT_HOME="$TMP_ROOT/copilot-home"
mkdir -p "$COPILOT_HOME"

COPILOT_MARKETPLACE_PATH="$(realpath "$PLUGINS_DIR/copilot")"

HOME="$COPILOT_HOME" npx -y @github/copilot plugin marketplace add "$COPILOT_MARKETPLACE_PATH"
HOME="$COPILOT_HOME" npx -y @github/copilot plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace

COPILOT_PLUGIN_LIST="$(HOME="$COPILOT_HOME" npx -y @github/copilot plugin list)"
if ! grep -q "dotnet-harness-toolkit" <<<"$COPILOT_PLUGIN_LIST"; then
  fail "Copilot plugin not found in plugin list"
fi

log "Smoke test: Gemini extension install"
GEMINI_HOME="$TMP_ROOT/gemini-home"
mkdir -p "$GEMINI_HOME"

GEMINI_API_KEY="dummy-key" HOME="$GEMINI_HOME" run_gemini extensions install "$PLUGINS_DIR/geminicli" --consent

GEMINI_API_KEY="dummy-key" HOME="$GEMINI_HOME" run_gemini extensions list >/dev/null

if [[ ! -d "$GEMINI_HOME/.gemini/extensions/dotnet-harness-toolkit" ]]; then
  fail "Gemini extension directory was not created after installation"
fi

log "Smoke test: OpenCode configuration and agents"
OPENCODE_HOME="$TMP_ROOT/opencode-home"
OPENCODE_WORKSPACE="$TMP_ROOT/opencode-workspace"
mkdir -p "$OPENCODE_HOME" "$OPENCODE_WORKSPACE"

cp -R "$PLUGINS_DIR/opencode/.opencode" "$OPENCODE_WORKSPACE/.opencode"
cp "$PLUGINS_DIR/opencode/opencode.json" "$OPENCODE_WORKSPACE/opencode.json"
cp "$PLUGINS_DIR/opencode/AGENTS.md" "$OPENCODE_WORKSPACE/AGENTS.md"

OPENCODE_AGENTS_LIST="$(cd "$OPENCODE_WORKSPACE" && HOME="$OPENCODE_HOME" run_opencode agent list)"
if ! grep -q "dotnet-architect" <<<"$OPENCODE_AGENTS_LIST"; then
  fail "OpenCode agent list does not include dotnet-architect"
fi

log "Smoke test: Codex manual bundle"
CODEX_WORKSPACE="$TMP_ROOT/codex-workspace"
mkdir -p "$CODEX_WORKSPACE"
cp "$PLUGINS_DIR/codexcli/workspace/AGENTS.md" "$CODEX_WORKSPACE/AGENTS.md"
cp -R "$PLUGINS_DIR/codexcli/workspace/.codex" "$CODEX_WORKSPACE/.codex"

if [[ ! -f "$CODEX_WORKSPACE/.codex/config.toml" ]]; then
  fail "Codex manual workspace is missing .codex/config.toml"
fi

if command -v codex >/dev/null 2>&1; then
  codex --help >/dev/null
fi

log "Smoke test: Antigravity manual bundle"
ANTIGRAVITY_WORKSPACE="$TMP_ROOT/antigravity-workspace"
mkdir -p "$ANTIGRAVITY_WORKSPACE"
cp -R "$PLUGINS_DIR/antigravity/workspace/.agent" "$ANTIGRAVITY_WORKSPACE/.agent"

if [[ ! -d "$ANTIGRAVITY_WORKSPACE/.agent/workflows" ]]; then
  fail "Antigravity manual workspace is missing workflows"
fi

log "Smoke install checks completed successfully"
