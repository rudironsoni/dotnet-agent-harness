#!/usr/bin/env bash
# Validates subagent frontmatter against the conventions defined in
# .rulesync/rules/10-conventions.md:
#   - Required/banned top-level fields
#   - Platform blocks (claudecode, opencode, copilot) present on every agent
#   - Tool profile consistency across platforms
#   - Codex CLI sandbox_mode alignment with read-only profile

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=../lib/common.sh
source "$SCRIPT_DIR/../lib/common.sh"

# Check required dependency
if ! command -v yq >/dev/null 2>&1; then
  fail "'yq' is required but not installed. Install with: brew install yq"
fi

SUBAGENTS_DIR="$REPO_ROOT/.rulesync/subagents"
ERRORS=0
CHECKED=0

err() {
  printf '  FAIL [%s]: %s\n' "$1" "$2" >&2
  ERRORS=$((ERRORS + 1))
}

extract_frontmatter() {
  local file="$1"
  sed -n '2,/^---$/p' "$file" | sed '$d'
}

yq_val() {
  local fm="$1" path="$2"
  printf '%s' "$fm" | yq -r "$path" 2>/dev/null
}

# --- Validate a single subagent file ---
validate_subagent() {
  local file="$1"
  local basename
  basename="$(basename "$file")"
  local fm
  fm="$(extract_frontmatter "$file")"

  if [[ -z "$fm" ]]; then
    err "$basename" "No YAML frontmatter found"
    return
  fi

  CHECKED=$((CHECKED + 1))

  # ---- Required top-level fields ----
  local val
  for field in name description targets; do
    val="$(yq_val "$fm" ".$field")"
    if [[ "$val" == "null" || -z "$val" ]]; then
      err "$basename" "Missing required field: $field"
    fi
  done

  # ---- Banned top-level fields ----
  for field in tools model user-invocable capabilities context; do
    val="$(yq_val "$fm" ".\"$field\"")"
    if [[ "$val" != "null" ]]; then
      err "$basename" "Banned top-level field present: $field"
    fi
  done

  # ---- Platform blocks must exist ----
  for platform in claudecode opencode copilot; do
    val="$(yq_val "$fm" ".$platform")"
    if [[ "$val" == "null" ]]; then
      err "$basename" "Missing platform block: $platform"
    fi
  done

  # ---- Claude Code validation ----
  local cc_tools
  cc_tools="$(yq_val "$fm" '.claudecode."allowed-tools"')"
  if [[ "$cc_tools" != "null" ]]; then
    local cc_tool_list
    cc_tool_list="$(printf '%s' "$fm" | yq -r '.claudecode."allowed-tools"[]' 2>/dev/null)" || true
    if [[ -n "$cc_tool_list" ]]; then
      while IFS= read -r tool; do
        [[ -z "$tool" ]] && continue
        case "$tool" in
          Read|Grep|Glob|Bash|Edit|Write) ;;
          *) err "$basename" "claudecode.allowed-tools: invalid tool name '$tool' (valid: Read, Grep, Glob, Bash, Edit, Write)" ;;
        esac
      done <<< "$cc_tool_list"
    fi
  else
    err "$basename" "claudecode.allowed-tools is missing"
  fi

  # ---- OpenCode validation ----
  local oc_mode
  oc_mode="$(yq_val "$fm" '.opencode.mode')"
  if [[ "$oc_mode" == "null" || -z "$oc_mode" ]]; then
    err "$basename" "opencode.mode is missing (must be 'primary' or 'subagent')"
  elif [[ "$oc_mode" != "primary" && "$oc_mode" != "subagent" ]]; then
    err "$basename" "opencode.mode='$oc_mode' is invalid (must be 'primary' or 'subagent')"
  fi

  local oc_tool_val
  for tool_key in bash edit write; do
    oc_tool_val="$(yq_val "$fm" ".opencode.tools.$tool_key")"
    if [[ "$oc_tool_val" == "null" ]]; then
      err "$basename" "opencode.tools.$tool_key is missing (must be true or false)"
    elif [[ "$oc_tool_val" != "true" && "$oc_tool_val" != "false" ]]; then
      err "$basename" "opencode.tools.$tool_key='$oc_tool_val' is invalid (must be true or false)"
    fi
  done

  # ---- Copilot validation ----
  local cp_tools
  cp_tools="$(yq_val "$fm" '.copilot.tools')"
  if [[ "$cp_tools" != "null" ]]; then
    local cp_tool_list
    cp_tool_list="$(printf '%s' "$fm" | yq -r '.copilot.tools[]' 2>/dev/null)" || true
    if [[ -n "$cp_tool_list" ]]; then
      while IFS= read -r tool; do
        [[ -z "$tool" ]] && continue
        case "$tool" in
          read|search|execute|edit) ;;
          *) err "$basename" "copilot.tools: invalid tool name '$tool' (valid: read, search, execute, edit)" ;;
        esac
      done <<< "$cp_tool_list"
    fi
  else
    err "$basename" "copilot.tools is missing"
  fi

  # ---- Cross-platform profile consistency ----
  # Determine the profile from OpenCode (source of truth for booleans)
  local oc_bash oc_edit oc_write
  oc_bash="$(yq_val "$fm" '.opencode.tools.bash')"
  oc_edit="$(yq_val "$fm" '.opencode.tools.edit')"
  oc_write="$(yq_val "$fm" '.opencode.tools.write')"

  local profile="unknown"
  if [[ "$oc_bash" == "false" && "$oc_edit" == "false" && "$oc_write" == "false" ]]; then
    profile="read-only"
  elif [[ "$oc_bash" == "true" && "$oc_edit" == "false" && "$oc_write" == "false" ]]; then
    profile="standard"
  elif [[ "$oc_bash" == "true" && "$oc_edit" == "true" && "$oc_write" == "true" ]]; then
    profile="full"
  fi

  if [[ "$profile" == "unknown" && "$oc_bash" != "null" ]]; then
    err "$basename" "OpenCode tools do not match any known profile (read-only|standard|full): bash=$oc_bash edit=$oc_edit write=$oc_write"
  fi

  # Check Claude Code tools match the profile
  if [[ "$cc_tools" != "null" && "$profile" != "unknown" ]]; then
    local has_cc_bash has_cc_edit has_cc_write
    has_cc_bash="$(printf '%s' "$fm" | yq -r '.claudecode."allowed-tools"[] | select(. == "Bash")' 2>/dev/null)" || true
    has_cc_edit="$(printf '%s' "$fm" | yq -r '.claudecode."allowed-tools"[] | select(. == "Edit")' 2>/dev/null)" || true
    has_cc_write="$(printf '%s' "$fm" | yq -r '.claudecode."allowed-tools"[] | select(. == "Write")' 2>/dev/null)" || true

    case "$profile" in
      read-only)
        [[ -n "$has_cc_bash" ]] && err "$basename" "Profile mismatch: read-only profile but claudecode has Bash"
        [[ -n "$has_cc_edit" ]] && err "$basename" "Profile mismatch: read-only profile but claudecode has Edit"
        [[ -n "$has_cc_write" ]] && err "$basename" "Profile mismatch: read-only profile but claudecode has Write"
        ;;
      standard)
        [[ -z "$has_cc_bash" ]] && err "$basename" "Profile mismatch: standard profile but claudecode missing Bash"
        [[ -n "$has_cc_edit" ]] && err "$basename" "Profile mismatch: standard profile but claudecode has Edit"
        [[ -n "$has_cc_write" ]] && err "$basename" "Profile mismatch: standard profile but claudecode has Write"
        ;;
      full)
        [[ -z "$has_cc_bash" ]] && err "$basename" "Profile mismatch: full profile but claudecode missing Bash"
        [[ -z "$has_cc_edit" ]] && err "$basename" "Profile mismatch: full profile but claudecode missing Edit"
        [[ -z "$has_cc_write" ]] && err "$basename" "Profile mismatch: full profile but claudecode missing Write"
        ;;
    esac
  fi

  # Check Copilot tools match the profile
  if [[ "$cp_tools" != "null" && "$profile" != "unknown" ]]; then
    local has_cp_execute has_cp_edit
    has_cp_execute="$(printf '%s' "$fm" | yq -r '.copilot.tools[] | select(. == "execute")' 2>/dev/null)" || true
    has_cp_edit="$(printf '%s' "$fm" | yq -r '.copilot.tools[] | select(. == "edit")' 2>/dev/null)" || true

    case "$profile" in
      read-only)
        [[ -n "$has_cp_execute" ]] && err "$basename" "Profile mismatch: read-only profile but copilot has execute"
        [[ -n "$has_cp_edit" ]] && err "$basename" "Profile mismatch: read-only profile but copilot has edit"
        ;;
      standard)
        [[ -z "$has_cp_execute" ]] && err "$basename" "Profile mismatch: standard profile but copilot missing execute"
        [[ -n "$has_cp_edit" ]] && err "$basename" "Profile mismatch: standard profile but copilot has edit"
        ;;
      full)
        [[ -z "$has_cp_execute" ]] && err "$basename" "Profile mismatch: full profile but copilot missing execute"
        [[ -z "$has_cp_edit" ]] && err "$basename" "Profile mismatch: full profile but copilot missing edit"
        ;;
    esac
  fi

  # Check Codex CLI sandbox_mode alignment
  local codex_sandbox
  codex_sandbox="$(yq_val "$fm" '.codexcli.sandbox_mode')"
  if [[ "$profile" == "read-only" && "$codex_sandbox" != "read-only" ]]; then
    err "$basename" "Profile mismatch: read-only profile but codexcli.sandbox_mode is not 'read-only' (got: $codex_sandbox)"
  elif [[ "$profile" != "read-only" && "$codex_sandbox" == "read-only" ]]; then
    err "$basename" "Profile mismatch: $profile profile but codexcli.sandbox_mode is 'read-only'"
  fi
}

# ---- Main ----

if [[ ! -d "$SUBAGENTS_DIR" ]]; then
  fail "Subagents directory not found: $SUBAGENTS_DIR"
fi

log "Validating subagent frontmatter in $SUBAGENTS_DIR"

for file in "$SUBAGENTS_DIR"/*.md; do
  [[ -f "$file" ]] || continue
  validate_subagent "$file"
done

if [[ "$CHECKED" -eq 0 ]]; then
  fail "No subagent files found"
fi

if [[ "$ERRORS" -gt 0 ]]; then
  printf '\n'
  fail "Subagent validation failed with $ERRORS error(s) across $CHECKED file(s)"
fi

log "Subagent validation passed: $CHECKED file(s), 0 errors"
