#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "$SCRIPT_DIR/../lib/common.sh"

OUTPUT_DIR="$REPO_ROOT/plugins"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --output-dir)
      OUTPUT_DIR="$2"
      shift 2
      ;;
    *)
      fail "Unknown argument: $1"
      ;;
  esac
done

TMP_ROOT="$(mktemp -d)"
WORK_DIR="$TMP_ROOT/work"
GLOBAL_HOME="$TMP_ROOT/home"

cleanup() {
  rm -rf "$TMP_ROOT"
}
trap cleanup EXIT

copy_file_if_exists() {
  local source_file="$1"
  local target_file="$2"

  if [[ -f "$source_file" ]]; then
    mkdir -p "$(dirname "$target_file")"
    cp "$source_file" "$target_file"
  fi
}

copy_dir_if_exists() {
  local source_dir="$1"
  local target_dir="$2"

  if [[ -d "$source_dir" ]]; then
    mkdir -p "$target_dir"
    cp -a "$source_dir/." "$target_dir/"
  fi
}

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

build_claudecode_bundle() {
  local bundle_dir="$OUTPUT_DIR/claudecode"

  mkdir -p "$bundle_dir/.claude-plugin" "$bundle_dir/hooks"

  jq -n '{
    name: "dotnet-harness-toolkit",
    description: "Comprehensive .NET development toolkit for Claude Code",
    version: "0.0.1"
  }' > "$bundle_dir/.claude-plugin/plugin.json"

  jq -n '{
    name: "dotnet-harness-toolkit-marketplace",
    description: "Marketplace for the dotnet-harness-toolkit Claude plugin",
    metadata: {
      description: "Marketplace for the dotnet-harness-toolkit Claude plugin"
    },
    owner: { name: "dotnet-harness-toolkit" },
    plugins: [
      {
        name: "dotnet-harness-toolkit",
        source: "./",
        description: "Comprehensive .NET development toolkit for Claude Code"
      }
    ]
  }' > "$bundle_dir/.claude-plugin/marketplace.json"

  copy_file_if_exists "$WORK_DIR/CLAUDE.md" "$bundle_dir/CLAUDE.md"
  copy_dir_if_exists "$WORK_DIR/.claude/rules" "$bundle_dir/rules"
  copy_dir_if_exists "$WORK_DIR/.claude/commands" "$bundle_dir/commands"
  copy_dir_if_exists "$WORK_DIR/.claude/agents" "$bundle_dir/agents"
  copy_dir_if_exists "$WORK_DIR/.claude/skills" "$bundle_dir/skills"
  copy_file_if_exists "$WORK_DIR/.mcp.json" "$bundle_dir/.mcp.json"

  if [[ -f "$WORK_DIR/.claude/settings.json" ]]; then
    jq '{ hooks: (.hooks // {}) }' "$WORK_DIR/.claude/settings.json" > "$bundle_dir/hooks/hooks.json"
  else
    jq -n '{ hooks: {} }' > "$bundle_dir/hooks/hooks.json"
  fi

  cat > "$bundle_dir/README.md" <<'EOF'
# Claude Code Plugin Install

```bash
REPO_BASE_URL="${REPO_BASE_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit/tree/main}"
claude plugin marketplace add "$REPO_BASE_URL/plugins/claudecode"
claude plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace
claude agents
```
EOF
}

build_copilot_bundle() {
  local bundle_dir="$OUTPUT_DIR/copilot"

  mkdir -p "$bundle_dir" "$bundle_dir/.claude-plugin"

  jq -n '{
    name: "dotnet-harness-toolkit",
    description: "Comprehensive .NET development toolkit for GitHub Copilot CLI",
    version: "0.0.1"
  }' > "$bundle_dir/plugin.json"

  jq -n '{
    name: "dotnet-harness-toolkit-marketplace",
    owner: { name: "dotnet-harness-toolkit" },
    plugins: [
      {
        name: "dotnet-harness-toolkit",
        source: "./",
        description: "Comprehensive .NET development toolkit for GitHub Copilot CLI"
      }
    ]
  }' > "$bundle_dir/.claude-plugin/marketplace.json"

  copy_file_if_exists "$WORK_DIR/.github/copilot-instructions.md" "$bundle_dir/copilot-instructions.md"
  copy_dir_if_exists "$WORK_DIR/.github/instructions" "$bundle_dir/instructions"
  copy_dir_if_exists "$WORK_DIR/.github/prompts" "$bundle_dir/commands"
  copy_dir_if_exists "$WORK_DIR/.github/agents" "$bundle_dir/agents"
  copy_dir_if_exists "$WORK_DIR/.github/skills" "$bundle_dir/skills"
  copy_file_if_exists "$WORK_DIR/.vscode/mcp.json" "$bundle_dir/.mcp.json"

  cat > "$bundle_dir/README.md" <<'EOF'
# Copilot CLI Plugin Install

```bash
REPO_BASE_URL="${REPO_BASE_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit/tree/main}"
npx -y @github/copilot plugin marketplace add "$REPO_BASE_URL/plugins/copilot"
npx -y @github/copilot plugin install dotnet-harness-toolkit@dotnet-harness-toolkit-marketplace
npx -y @github/copilot plugin list
```
EOF
}

build_geminicli_bundle() {
  local bundle_dir="$OUTPUT_DIR/geminicli"
  local mcp_json='{}'

  mkdir -p "$bundle_dir"

  if [[ -f "$WORK_DIR/.gemini/settings.json" ]]; then
    mcp_json="$(jq -c '.mcpServers // {}' "$WORK_DIR/.gemini/settings.json")"
  fi

  jq -n --argjson mcp "$mcp_json" '{
    name: "dotnet-harness-toolkit",
    version: "0.0.1",
    description: "Comprehensive .NET development toolkit for Gemini CLI",
    contextFileName: "GEMINI.md",
    mcpServers: $mcp
  }' > "$bundle_dir/gemini-extension.json"

  copy_file_if_exists "$WORK_DIR/GEMINI.md" "$bundle_dir/GEMINI.md"
  copy_dir_if_exists "$WORK_DIR/.gemini/memories" "$bundle_dir/.gemini/memories"
  copy_dir_if_exists "$WORK_DIR/.gemini/commands" "$bundle_dir/.gemini/commands"
  copy_dir_if_exists "$WORK_DIR/.gemini/skills" "$bundle_dir/.gemini/skills"
  copy_dir_if_exists "$WORK_DIR/.gemini/agents" "$bundle_dir/.gemini/agents"

  cat > "$bundle_dir/README.md" <<'EOF'
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
EOF
}

build_opencode_bundle() {
  local bundle_dir="$OUTPUT_DIR/opencode"

  mkdir -p "$bundle_dir"

  copy_file_if_exists "$WORK_DIR/AGENTS.md" "$bundle_dir/AGENTS.md"
  copy_file_if_exists "$WORK_DIR/opencode.json" "$bundle_dir/opencode.json"
  copy_dir_if_exists "$WORK_DIR/.opencode/memories" "$bundle_dir/.opencode/memories"
  copy_dir_if_exists "$WORK_DIR/.opencode/command" "$bundle_dir/.opencode/command"
  copy_dir_if_exists "$WORK_DIR/.opencode/agent" "$bundle_dir/.opencode/agent"
  copy_dir_if_exists "$WORK_DIR/.opencode/skill" "$bundle_dir/.opencode/skill"
  copy_dir_if_exists "$WORK_DIR/.opencode/plugins" "$bundle_dir/.opencode/plugins"

  cat > "$bundle_dir/README.md" <<'EOF'
# OpenCode Plugin Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/opencode
  BUNDLE_DIR="$TMP_DIR/dotnet-harness-toolkit/plugins/opencode"
  cp -R "$BUNDLE_DIR/.opencode" ".opencode"
  cp "$BUNDLE_DIR/opencode.json" "opencode.json"
  cp "$BUNDLE_DIR/AGENTS.md" "AGENTS.md"
  opencode agent list
)
```
EOF
}

build_codex_manual_bundle() {
  local bundle_dir="$OUTPUT_DIR/codexcli"

  mkdir -p "$bundle_dir/workspace" "$bundle_dir/global"

  copy_file_if_exists "$WORK_DIR/AGENTS.md" "$bundle_dir/workspace/AGENTS.md"
  copy_file_if_exists "$WORK_DIR/.codex/config.toml" "$bundle_dir/workspace/.codex/config.toml"
  copy_dir_if_exists "$WORK_DIR/.codex/memories" "$bundle_dir/workspace/.codex/memories"
  copy_dir_if_exists "$WORK_DIR/.codex/skills" "$bundle_dir/workspace/.codex/skills"
  copy_dir_if_exists "$WORK_DIR/.codex/agents" "$bundle_dir/workspace/.codex/agents"
  copy_dir_if_exists "$GLOBAL_HOME/.codex/prompts" "$bundle_dir/global/.codex/prompts"

  cat > "$bundle_dir/README.md" <<'EOF'
# Codex CLI Manual Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/codexcli
  BUNDLE_DIR="$TMP_DIR/dotnet-harness-toolkit/plugins/codexcli"
  cp "$BUNDLE_DIR/workspace/AGENTS.md" "AGENTS.md"
  cp -R "$BUNDLE_DIR/workspace/.codex" "./"
  mkdir -p ~/.codex/prompts
  cp -R "$BUNDLE_DIR/global/.codex/prompts/." ~/.codex/prompts/
)
```
EOF
}

build_antigravity_manual_bundle() {
  local bundle_dir="$OUTPUT_DIR/antigravity"
  local antigravity_global_skills_source="$GLOBAL_HOME/.gemini/antigravity/skills"

  mkdir -p "$bundle_dir/workspace" "$bundle_dir/global"

  if [[ ! -d "$antigravity_global_skills_source" && -d "$WORK_DIR/.gemini/antigravity/skills" ]]; then
    antigravity_global_skills_source="$WORK_DIR/.gemini/antigravity/skills"
  fi

  copy_dir_if_exists "$WORK_DIR/.agent/rules" "$bundle_dir/workspace/.agent/rules"
  copy_dir_if_exists "$WORK_DIR/.agent/workflows" "$bundle_dir/workspace/.agent/workflows"
  copy_dir_if_exists "$WORK_DIR/.agent/skills" "$bundle_dir/workspace/.agent/skills"
  copy_dir_if_exists "$antigravity_global_skills_source" "$bundle_dir/global/.gemini/antigravity/skills"

  cat > "$bundle_dir/README.md" <<'EOF'
# Antigravity Manual Install

```bash
(
  set -euo pipefail
  REPO_URL="${REPO_URL:-https://github.com/rudironsoni/dotnet-harness-toolkit.git}"
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "$TMP_DIR"' EXIT

  git clone --depth 1 --filter=blob:none --sparse "$REPO_URL" "$TMP_DIR/dotnet-harness-toolkit"
  git -C "$TMP_DIR/dotnet-harness-toolkit" sparse-checkout set plugins/antigravity
  BUNDLE_DIR="$TMP_DIR/dotnet-harness-toolkit/plugins/antigravity"
  cp -R "$BUNDLE_DIR/workspace/.agent" "./"
  mkdir -p ~/.gemini/antigravity/skills
  cp -R "$BUNDLE_DIR/global/.gemini/antigravity/skills/." ~/.gemini/antigravity/skills/
)
```
EOF
}

mkdir -p "$WORK_DIR" "$GLOBAL_HOME"

log "Preparing isolated build workspace"
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

log "Generating project-scoped outputs"
run_rulesync generate

log "Generating global Codex command prompts"
HOME="$GLOBAL_HOME" run_rulesync generate --targets codexcli --features commands --global

log "Generating global Antigravity skills"
HOME="$GLOBAL_HOME" run_rulesync generate --targets antigravity --features skills --global
popd >/dev/null

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

log "Building Claude Code plugin bundle"
build_claudecode_bundle

log "Building Copilot plugin bundle"
build_copilot_bundle

log "Building Gemini extension bundle"
build_geminicli_bundle

log "Building OpenCode plugin bundle"
build_opencode_bundle

log "Building Codex manual bundle"
build_codex_manual_bundle

log "Building Antigravity manual bundle"
build_antigravity_manual_bundle

log "Plugin bundles generated in: $OUTPUT_DIR"
