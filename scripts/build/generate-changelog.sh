#!/usr/bin/env bash
# Generate descriptive changelog from conventional commits
# Usage: generate-changelog.sh <version> [previous_tag]

set -euo pipefail

VERSION="${1:-}"
PREVIOUS_TAG="${2:-}"

if [[ -z "${VERSION}" ]]; then
  echo "Usage: ${0} <version> [previous_tag]"
  echo "  version: semver version (e.g., v1.0.0)"
  echo "  previous_tag: optional previous tag to compare against"
  exit 1
fi

# Remove 'v' prefix for comparison
VERSION_CLEAN="${VERSION#v}"

# Find previous tag if not provided
if [[ -z "${PREVIOUS_TAG}" ]]; then
  PREVIOUS_TAG=$(git tag --list 'v[0-9]*.[0-9]*.[0-9]*' --sort=-v:refname | grep -v "^${VERSION}$" | head -n1 || true)
fi

if [[ -n "${PREVIOUS_TAG}" ]]; then
  COMMIT_RANGE="${PREVIOUS_TAG}..HEAD"
  COMPARE_URL="https://github.com/${GITHUB_REPOSITORY:-rudironsoni/dotnet-harness}/compare/${PREVIOUS_TAG}...${VERSION}"
else
  COMMIT_RANGE="HEAD"
  COMPARE_URL=""
fi

# Function to categorize commits
categorize_commits() {
  local pattern="${1}"
  git log "${COMMIT_RANGE}" --pretty=format:"%s" --reverse | grep -E "^${pattern}:" || true
}

# Function to format commit message
format_commit() {
  local msg="${1}"
  # Extract the description after the type(scope): prefix
  local desc
  desc=$(echo "${msg}" | sed -E 's/^[a-z]+(\([^)]+\))?:\s*//')
  # Extract scope if present
  local scope
  scope=$(echo "${msg}" | grep -oE '^[a-z]+\(([^)]+)\)' | sed -E 's/^[a-z]+\(([^)]+)\)/\1/' || true)

  if [[ -n "${scope}" ]]; then
    echo "- **${scope}**: ${desc}"
  else
    echo "- ${desc}"
  fi
}

# Generate changelog
{
  echo "## What's Changed"
  echo ""

  if [[ -n "${COMPARE_URL}" ]]; then
    echo "**Full Changelog**: ${COMPARE_URL}"
    echo ""
  fi

  # Features
  FEATS=$(categorize_commits "feat")
  if [[ -n "${FEATS}" ]]; then
    echo "### Features"
    echo "${FEATS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Bug Fixes
  FIXES=$(categorize_commits "fix")
  if [[ -n "${FIXES}" ]]; then
    echo "### Bug Fixes"
    echo "${FIXES}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Performance
  PERFS=$(categorize_commits "perf")
  if [[ -n "${PERFS}" ]]; then
    echo "### Performance Improvements"
    echo "${PERFS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Documentation
  DOCS=$(categorize_commits "docs")
  if [[ -n "${DOCS}" ]]; then
    echo "### Documentation"
    echo "${DOCS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Refactoring
  REFACTORS=$(categorize_commits "refactor")
  if [[ -n "${REFACTORS}" ]]; then
    echo "### Code Refactoring"
    echo "${REFACTORS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Build System
  BUILDS=$(categorize_commits "build")
  if [[ -n "${BUILDS}" ]]; then
    echo "### Build System"
    echo "${BUILDS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # CI/CD
  CIS=$(categorize_commits "ci")
  if [[ -n "${CIS}" ]]; then
    echo "### CI/CD"
    echo "${CIS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Tests
  TESTS=$(categorize_commits "test")
  if [[ -n "${TESTS}" ]]; then
    echo "### Tests"
    echo "${TESTS}" | while read -r line; do
      format_commit "${line}"
    done
    echo ""
  fi

  # Other commits (excluding chore and merge)
  OTHERS=$(git log "${COMMIT_RANGE}" --pretty=format:"%s" --reverse | grep -v -E '^(feat|fix|perf|docs|refactor|build|ci|test|chore|Merge|merge):' || true)
  if [[ -n "${OTHERS}" ]]; then
    echo "### Other Changes"
    echo "${OTHERS}" | while read -r line; do
      echo "- ${line}"
    done
    echo ""
  fi

  # Contributors
  echo "### Contributors"
  echo ""
  CONTRIBUTORS=$(git log "${COMMIT_RANGE}" --pretty=format:"%an <%ae>" | sort | uniq)
  echo "${CONTRIBUTORS}" | while read -r contributor; do
    if [[ -n "${contributor}" ]]; then
      echo "- ${contributor}"
    fi
  done
  echo ""

  echo "---"
  echo ""
  echo "## Plugin Assets"
  echo ""
  echo "This release includes the following plugin bundles:"
  echo ""
  echo "| Plugin | Description |"
  echo "|--------|-------------|"
  echo "| dotnet-harness-claudecode.zip | Claude Code plugin bundle |"
  echo "| dotnet-harness-opencode.zip | OpenCode plugin bundle |"
  echo "| dotnet-harness-copilot.zip | GitHub Copilot plugin bundle |"
  echo "| dotnet-harness-codexcli.zip | Codex CLI plugin bundle |"
  echo "| dotnet-harness-geminicli.zip | Gemini CLI plugin bundle |"
  echo "| dotnet-harness-agentsmd.zip | AGENTS.md plugin bundle |"
  echo "| dotnet-harness-antigravity.zip | Antigravity plugin bundle |"
  echo ""

} > CHANGELOG.md

echo "Generated CHANGELOG.md for version ${VERSION}"
if [[ -n "${PREVIOUS_TAG}" ]]; then
  echo "Previous tag: ${PREVIOUS_TAG}"
fi
