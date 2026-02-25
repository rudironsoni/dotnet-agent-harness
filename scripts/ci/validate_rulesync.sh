#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "$SCRIPT_DIR/../lib/common.sh"

TMP_ROOT="$(mktemp -d)"
WORK_DIR="$TMP_ROOT/work"
GLOBAL_HOME="$TMP_ROOT/home"

cleanup() {
  rm -rf "$TMP_ROOT"
}
trap cleanup EXIT

reset_generated_workspace() {
  local target_root="$1"

  rm -rf \
    "$target_root/.agent" \
    "$target_root/.claude" \
    "$target_root/.codex" \
    "$target_root/.gemini" \
    "$target_root/.opencode" \
    "$target_root/.vscode" \
    "$target_root/.github/agents" \
    "$target_root/.github/instructions" \
    "$target_root/.github/prompts" \
    "$target_root/.github/skills"

  rm -f \
    "$target_root/.github/copilot-instructions.md" \
    "$target_root/.mcp.json" \
    "$target_root/AGENTS.md" \
    "$target_root/CLAUDE.md" \
    "$target_root/GEMINI.md" \
    "$target_root/opencode.json"
}

require_path() {
  local path="$1"
  if [[ ! -e "$path" ]]; then
    fail "Expected generated path missing: $path"
  fi
}

checksum_paths() {
  local root="$1"
  shift

  local -a existing=()
  local rel
  for rel in "$@"; do
    if [[ -e "$root/$rel" ]]; then
      existing+=("$rel")
    fi
  done

  if [[ ${#existing[@]} -eq 0 ]]; then
    printf 'EMPTY\n'
    return
  fi

  (
    cd "$root"
    tar --sort=name --mtime='UTC 1970-01-01' --owner=0 --group=0 --numeric-owner -cf - "${existing[@]}" | sha256sum | cut -d' ' -f1
  )
}

mkdir -p "$WORK_DIR" "$GLOBAL_HOME"

log "Creating isolated workspace for RuleSync validation"
mkdir -p "$WORK_DIR"
tar -cf - \
  --exclude ".git" \
  --exclude "node_modules" \
  --exclude "plugins" \
  -C "$REPO_ROOT" . | tar -xf - -C "$WORK_DIR"

reset_generated_workspace "$WORK_DIR"

pushd "$WORK_DIR" >/dev/null
log "Running rulesync install"
run_rulesync install --frozen --silent || run_rulesync install

log "Running rulesync generate"
run_rulesync generate --silent

project_checksum_before="$(checksum_paths "$WORK_DIR" \
  .agent \
  .claude \
  .codex \
  .gemini \
  .geminiignore \
  .opencode \
  .vscode \
  .github/agents \
  .github/instructions \
  .github/prompts \
  .github/skills \
  .github/copilot-instructions.md \
  .mcp.json \
  AGENTS.md \
  CLAUDE.md \
  GEMINI.md \
  opencode.json
)"

log "Running rulesync generate again for determinism check"
run_rulesync generate --silent

project_checksum_after="$(checksum_paths "$WORK_DIR" \
  .agent \
  .claude \
  .codex \
  .gemini \
  .geminiignore \
  .opencode \
  .vscode \
  .github/agents \
  .github/instructions \
  .github/prompts \
  .github/skills \
  .github/copilot-instructions.md \
  .mcp.json \
  AGENTS.md \
  CLAUDE.md \
  GEMINI.md \
  opencode.json
)"

if [[ "$project_checksum_before" != "$project_checksum_after" ]]; then
  fail "RuleSync project generation is not deterministic across consecutive runs"
fi

log "Verifying generated target outputs"
require_path "$WORK_DIR/CLAUDE.md"
require_path "$WORK_DIR/.claude/agents"
require_path "$WORK_DIR/.github/copilot-instructions.md"
require_path "$WORK_DIR/.github/agents"
require_path "$WORK_DIR/GEMINI.md"
require_path "$WORK_DIR/.gemini/skills"
require_path "$WORK_DIR/.opencode/agent"
require_path "$WORK_DIR/AGENTS.md"
require_path "$WORK_DIR/.codex/agents"
require_path "$WORK_DIR/.agent/rules"
require_path "$WORK_DIR/.agent/workflows"
require_path "$WORK_DIR/.agent/skills"

log "Running global codex command generation"
HOME="$GLOBAL_HOME" run_rulesync generate --targets codexcli --features commands --global --silent
require_path "$GLOBAL_HOME/.codex/prompts"

codex_checksum_before="$(checksum_paths "$GLOBAL_HOME" .codex/prompts)"
HOME="$GLOBAL_HOME" run_rulesync generate --targets codexcli --features commands --global --silent
codex_checksum_after="$(checksum_paths "$GLOBAL_HOME" .codex/prompts)"

if [[ "$codex_checksum_before" != "$codex_checksum_after" ]]; then
  fail "RuleSync global codex generation is not deterministic across consecutive runs"
fi

log "Running global antigravity skill generation"
HOME="$GLOBAL_HOME" run_rulesync generate --targets antigravity --features skills --global --silent

antigravity_root="$GLOBAL_HOME"
if [[ ! -d "$antigravity_root/.gemini/antigravity/skills" && -d "$WORK_DIR/.gemini/antigravity/skills" ]]; then
  antigravity_root="$WORK_DIR"
fi

require_path "$antigravity_root/.gemini/antigravity/skills"

antigravity_checksum_before="$(checksum_paths "$antigravity_root" .gemini/antigravity/skills)"
HOME="$GLOBAL_HOME" run_rulesync generate --targets antigravity --features skills --global --silent
antigravity_checksum_after="$(checksum_paths "$antigravity_root" .gemini/antigravity/skills)"

if [[ "$antigravity_checksum_before" != "$antigravity_checksum_after" ]]; then
  fail "RuleSync global antigravity generation is not deterministic across consecutive runs"
fi
popd >/dev/null

log "RuleSync validation completed successfully"
