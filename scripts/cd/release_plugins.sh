#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "$SCRIPT_DIR/../lib/common.sh"

AGENTS=(claudecode copilot geminicli opencode codexcli antigravity)
PLUGINS_DIR="$REPO_ROOT/plugins"
OUTPUT_DIR="$REPO_ROOT/.tmp/release-assets"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --plugins-dir)
      PLUGINS_DIR="$2"
      shift 2
      ;;
    --output-dir)
      OUTPUT_DIR="$2"
      shift 2
      ;;
    *)
      fail "Unknown argument: $1"
      ;;
  esac
done

if [[ "$PLUGINS_DIR" != /* ]]; then
  PLUGINS_DIR="$REPO_ROOT/$PLUGINS_DIR"
fi

if [[ "$OUTPUT_DIR" != /* ]]; then
  OUTPUT_DIR="$REPO_ROOT/$OUTPUT_DIR"
fi

for command_name in git zip tar sha256sum; do
  if ! command -v "$command_name" >/dev/null 2>&1; then
    fail "Required command is missing: $command_name"
  fi
done

TMP_ROOT="$(mktemp -d)"
NOTES_FILE=""
TAG_FILE=""

cleanup_on_exit() {
  local exit_code="$1"

  rm -rf "$TMP_ROOT"
  popd >/dev/null || true
  exit "$exit_code"
}

pushd "$REPO_ROOT" >/dev/null
trap 'cleanup_on_exit $?' EXIT

for agent in "${AGENTS[@]}"; do
  if [[ ! -d "$PLUGINS_DIR/$agent" ]]; then
    fail "Missing plugin directory: $PLUGINS_DIR/$agent"
  fi
done

tag_on_head="$(git tag --points-at HEAD --list 'v*' --sort=-v:refname | head -n 1)"

if [[ -n "$tag_on_head" ]]; then
  if [[ ! "$tag_on_head" =~ ^v([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
    fail "Tag on HEAD has invalid semver format: $tag_on_head"
  fi

  next_tag="$tag_on_head"
  previous_tag="$(git tag --list 'v*' --sort=-v:refname | grep -Fxv "$next_tag" | head -n 1 || true)"
else
  latest_tag="$(git tag --list 'v*' --sort=-v:refname | head -n 1)"

  if [[ -z "$latest_tag" ]]; then
    next_tag="v0.1.0"
    previous_tag=""
  else
    if [[ ! "$latest_tag" =~ ^v([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
      fail "Latest release tag has invalid semver format: $latest_tag"
    fi

    major="${BASH_REMATCH[1]}"
    minor="${BASH_REMATCH[2]}"
    patch="${BASH_REMATCH[3]}"
    patch=$((patch + 1))
    next_tag="v${major}.${minor}.${patch}"
    previous_tag="$latest_tag"
  fi

  log "Using next release tag: $next_tag"
fi

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"
NOTES_FILE="$OUTPUT_DIR/release-notes.md"
TAG_FILE="$OUTPUT_DIR/release-tag.txt"

for agent in "${AGENTS[@]}"; do
  log "Packaging plugin bundle: $agent"

  tar -czf "$OUTPUT_DIR/dotnet-agent-harness-${agent}-${next_tag}.tar.gz" -C "$PLUGINS_DIR" "$agent"
  (
    cd "$PLUGINS_DIR"
    zip -rq "$OUTPUT_DIR/dotnet-agent-harness-${agent}-${next_tag}.zip" "$agent"
  )
done

log "Packaging rulesync source bundle"
RULESYNC_SOURCE_DIR="$TMP_ROOT/rulesync-source"
mkdir -p "$RULESYNC_SOURCE_DIR"

if [[ ! -d "$REPO_ROOT/.rulesync" ]]; then
  fail "Missing rulesync directory: $REPO_ROOT/.rulesync"
fi

if [[ ! -f "$REPO_ROOT/rulesync.jsonc" ]]; then
  fail "Missing rulesync config file: $REPO_ROOT/rulesync.jsonc"
fi

cp -R "$REPO_ROOT/.rulesync" "$RULESYNC_SOURCE_DIR/.rulesync"
cp "$REPO_ROOT/rulesync.jsonc" "$RULESYNC_SOURCE_DIR/rulesync.jsonc"

tar -czf "$OUTPUT_DIR/dotnet-agent-harness-rulesync-${next_tag}.tar.gz" -C "$RULESYNC_SOURCE_DIR" .
(
  cd "$RULESYNC_SOURCE_DIR"
  zip -rq "$OUTPUT_DIR/dotnet-agent-harness-rulesync-${next_tag}.zip" .
)

(
  cd "$OUTPUT_DIR"
  sha256sum ./*.tar.gz ./*.zip > "checksums-${next_tag}.txt"
)

scope_logs() {
  local scope_title="$1"
  shift
  local -a scope_paths=("$@")
  local logs=""

  if [[ -n "$previous_tag" ]]; then
    logs="$(git log --no-merges --pretty='- %h %s' "${previous_tag}..HEAD" -- "${scope_paths[@]}" || true)"
  else
    logs="$(git log --no-merges --pretty='- %h %s' -- "${scope_paths[@]}" || true)"
  fi

  {
    printf '## %s\n' "$scope_title"
    if [[ -n "$logs" ]]; then
      printf '%s\n' "$logs"
    else
      printf '%s\n' "- No changes in this release."
    fi
    printf '\n'
  } >> "$NOTES_FILE"
}

{
  printf '# %s\n\n' "$next_tag"
  if [[ -n "$previous_tag" ]]; then
    printf '_Changes since %s._\n\n' "$previous_tag"
  else
    printf '_Initial release._\n\n'
  fi
} > "$NOTES_FILE"

scope_logs "rulesync" ".rulesync" "rulesync.jsonc" "scripts/build/build_plugin_bundles.sh" "scripts/ci/validate_rulesync.sh"
for agent in "${AGENTS[@]}"; do
  scope_logs "$agent" "plugins/$agent"
done

printf '%s\n' "$next_tag" > "$TAG_FILE"

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  {
    echo "tag=$next_tag"
    echo "notes_file=$NOTES_FILE"
    echo "tag_file=$TAG_FILE"
  } >> "$GITHUB_OUTPUT"
fi

log "Prepared release assets for $next_tag in $OUTPUT_DIR"

trap - EXIT
cleanup_on_exit 0
