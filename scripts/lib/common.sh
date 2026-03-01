#!/usr/bin/env bash

set -euo pipefail

# Ensure we're in a git repository
if ! git rev-parse --show-toplevel >/dev/null 2>&1; then
  echo "ERROR: Not inside a git repository" >&2
  exit 1
fi

readonly REPO_ROOT="$(git rev-parse --show-toplevel)"

log() {
  printf '%s\n' "$*"
}

fail() {
  printf 'ERROR: %s\n' "$*" >&2
  exit 1
}

run_rulesync() {
  if command -v rulesync >/dev/null 2>&1; then
    rulesync "$@"
    return
  fi
  fail "rulesync is not installed. Run scripts/ci/install_rulesync.sh to install the binary."
}

run_claude() {
  if command -v claude >/dev/null 2>&1; then
    claude "$@"
    return
  fi
  fail "claude-cli is not installed. Please install it and retry."
}

run_copilot() {
  if command -v copilot >/dev/null 2>&1; then
    copilot "$@"
    return
  fi
  fail "copilot CLI is not installed. Please install it and retry."
}

run_gemini() {
  if command -v gemini >/dev/null 2>&1; then
    gemini "$@"
    return
  fi
  fail "gemini CLI is not installed. Please install it and retry."
}

run_opencode() {
  if command -v opencode >/dev/null 2>&1; then
    opencode "$@"
    return
  fi
  fail "opencode CLI is not installed. Please install it and retry."
}
