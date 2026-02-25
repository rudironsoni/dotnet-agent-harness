#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "$SCRIPT_DIR/../lib/common.sh"

if [[ ! -d "$REPO_ROOT/plugins" ]]; then
  fail "plugins directory does not exist: $REPO_ROOT/plugins"
fi

pushd "$REPO_ROOT" >/dev/null

git add plugins

if git diff --cached --quiet; then
  log "No plugin bundle changes to commit"
  popd >/dev/null
  exit 0
fi

GIT_AUTHOR_NAME="github-actions[bot]" \
GIT_AUTHOR_EMAIL="41898282+github-actions[bot]@users.noreply.github.com" \
GIT_COMMITTER_NAME="github-actions[bot]" \
GIT_COMMITTER_EMAIL="41898282+github-actions[bot]@users.noreply.github.com" \
git commit -m "chore: regenerate plugin bundles"
git push

popd >/dev/null
