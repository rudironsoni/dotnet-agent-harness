#!/usr/bin/env bash
# UserPromptSubmit hook: silently inject XML reminder via additionalContext.
set -euo pipefail

# Check if current directory looks like a .NET repo.
SLN_COUNT="$(find . -maxdepth 3 \( -name '*.sln' -o -name '*.slnx' \) 2>/dev/null | wc -l | tr -d ' ')" || SLN_COUNT=0
CSPROJ_COUNT="$(find . -maxdepth 3 -name '*.csproj' 2>/dev/null | wc -l | tr -d ' ')" || CSPROJ_COUNT=0
CS_COUNT="$(find . -maxdepth 4 -name '*.cs' 2>/dev/null | wc -l | tr -d ' ')" || CS_COUNT=0
HAS_GLOBAL_JSON=false
if [ -f "global.json" ]; then
  HAS_GLOBAL_JSON=true
fi

MSG=""
if [ "$SLN_COUNT" -gt 0 ] || [ "$CSPROJ_COUNT" -gt 0 ] || [ "$CS_COUNT" -gt 0 ] || [ "$HAS_GLOBAL_JSON" = true ]; then
  read -r -d '' MSG <<'EOF' || true
<system-reminder>
<dotnet-agent-harness-routing>
1. Mandatory first action: invoke [skill:dotnet-advisor].
2. Do not plan, reason, design, or implement until [skill:dotnet-advisor] has been invoked.
3. Follow advisor routing and invoke the relevant downstream skills.
4. Apply standards defined by the routed skills.
</dotnet-agent-harness-routing>
</system-reminder>
EOF
fi

if command -v jq &>/dev/null; then
  jq -n --arg ctx "$MSG" '{
    hookSpecificOutput: {
      hookEventName: "UserPromptSubmit",
      additionalContext: $ctx
    }
  }'
else
  # Fallback: escape JSON special characters, then replace newlines with \n literal
  ESCAPED="$(printf '%s' "$MSG" | sed 's/\\/\\\\/g; s/"/\\"/g' | while IFS= read -r line; do printf '%s\\n' "$line"; done)"
  # Remove trailing \n
  ESCAPED="${ESCAPED%\\n}"
  printf '{"hookSpecificOutput":{"hookEventName":"UserPromptSubmit","additionalContext":"%s"}}\n' "$ESCAPED"
fi

exit 0
