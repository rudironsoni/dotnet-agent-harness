#!/usr/bin/env bash

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"

log() {
  printf '%s\n' "$*"
}

fail() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

run_rulesync() {
  if [[ -x "$REPO_ROOT/node_modules/.bin/rulesync" ]]; then
    "$REPO_ROOT/node_modules/.bin/rulesync" "$@"
    return
  fi
  npx rulesync "$@"
}

run_claude() {
  if command -v claude >/dev/null 2>&1; then
    claude "$@"
    return
  fi
  npx -y @anthropic-ai/claude-code "$@"
}

run_copilot() {
  if command -v copilot >/dev/null 2>&1; then
    copilot "$@"
    return
  fi
  npx -y @github/copilot "$@"
}

run_gemini() {
  if command -v gemini >/dev/null 2>&1; then
    gemini "$@"
    return
  fi
  npx -y @google/gemini-cli "$@"
}

run_opencode() {
  if command -v opencode >/dev/null 2>&1; then
    opencode "$@"
    return
  fi
  npx -y opencode-ai "$@"
}
