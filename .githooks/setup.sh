#!/bin/bash
#
# Setup script for git hooks
# Install hooks to .git/hooks/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GIT_HOOKS_DIR="${SCRIPT_DIR}/../.git/hooks"

echo "Setting up git hooks..."

# Check if we're in a git repo
if [[ ! -d "${SCRIPT_DIR}/../.git" ]]; then
    echo "Error: Not in a git repository"
    exit 1
fi

# Create hooks directory if it doesn't exist
mkdir -p "${GIT_HOOKS_DIR}"

# Install pre-commit hook
if [[ -f "${SCRIPT_DIR}/pre-commit" ]]; then
    cp "${SCRIPT_DIR}/pre-commit" "${GIT_HOOKS_DIR}/pre-commit"
    chmod +x "${GIT_HOOKS_DIR}/pre-commit"
    echo "Installed: pre-commit hook"
fi

# Install pre-push hook
if [[ -f "${SCRIPT_DIR}/pre-push" ]]; then
    cp "${SCRIPT_DIR}/pre-push" "${GIT_HOOKS_DIR}/pre-push"
    chmod +x "${GIT_HOOKS_DIR}/pre-push"
    echo "Installed: pre-push hook"
fi

echo "Git hooks installed successfully!"
echo ""
echo "Configuration options:"
echo "  DOTNET_AGENT_HARNESS_SKIP_ANALYZERS=true  - Skip analyzer checks"
echo "  git commit --no-verify                      - Bypass hooks (not recommended)"
