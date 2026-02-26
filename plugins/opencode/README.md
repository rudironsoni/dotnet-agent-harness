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

  # Global installation (directory names must match SKILL.md 'name' frontmatter).
  mkdir -p "$HOME/.config/opencode/skills" "$HOME/.config/opencode/agents"

  if ! compgen -G "$BUNDLE_DIR/.opencode/skill/*/SKILL.md" > /dev/null; then
    echo "No OpenCode skills found in bundle: $BUNDLE_DIR/.opencode/skill" >&2
    exit 1
  fi

  for skill_file in "$BUNDLE_DIR"/.opencode/skill/*/SKILL.md; do
    skill_dir="$(basename "$(dirname "$skill_file")")"
    skill_name="$(grep -m1 "^name:" "$skill_file" | sed 's/^name:[[:space:]]*//' | tr -d '\r')"

    if [[ -z "$skill_name" ]]; then
      echo "Missing 'name' frontmatter in: $skill_file" >&2
      exit 1
    fi

    if [[ "$skill_name" != "$skill_dir" ]]; then
      echo "Skill directory must match SKILL.md name: $skill_dir != $skill_name" >&2
      exit 1
    fi

    mkdir -p "$HOME/.config/opencode/skills/$skill_name"
    cp "$skill_file" "$HOME/.config/opencode/skills/$skill_name/SKILL.md"
  done

  # Project installation for hooks, commands, memories, and config.
  cp "$BUNDLE_DIR"/.opencode/agent/*.md "$HOME/.config/opencode/agents/"
  mkdir -p ".opencode"
  cp -R "$BUNDLE_DIR/.opencode/plugins" ".opencode/plugins"
  cp -R "$BUNDLE_DIR/.opencode/command" ".opencode/command"
  cp -R "$BUNDLE_DIR/.opencode/memories" ".opencode/memories"
  cp "$BUNDLE_DIR/opencode.json" "opencode.json"
  cp "$BUNDLE_DIR/AGENTS.md" "AGENTS.md"

  opencode agent list
  opencode debug skill
)
```
