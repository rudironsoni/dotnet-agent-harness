#!/usr/bin/env bash
set -euo pipefail

# Build per-platform plugin bundles for distribution
# Creates zip files in dist/ directory
#
# Generated output directories per platform:
#   claudecode  -> .claude/          (agents, commands, rules, skills, settings.json)
#   opencode    -> .opencode/        (agent, command, memories, skill) + AGENTS.md
#   copilot     -> .github/          (agents, instructions, prompts, skills, copilot-instructions.md)
#   codexcli    -> .codex/           (agents, memories, skills, config.toml) + AGENTS.md
#   geminicli   -> .gemini/          (commands, memories, skills, settings.json) + GEMINI.md
#   agentsmd    -> .agents/          (memories, skills)
#   antigravity -> .agent/           (rules, skills, workflows)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
DIST_DIR="${PROJECT_ROOT}/dist"

echo "Building plugin bundles..."
echo "Project root: ${PROJECT_ROOT}"
echo "Output: ${DIST_DIR}"

# Clean and create dist directory
rm -rf "${DIST_DIR}"
mkdir -p "${DIST_DIR}"

# Generate all platform outputs first
echo ""
echo "=== Generating RuleSync outputs ==="
cd "${PROJECT_ROOT}"

# Generate for all targets
npx rulesync generate --targets "*" --features "*"

# Define platforms, their output directories, and any root-level files to include
# Format: platform|directory|root_files (comma-separated, empty if none)
PLATFORM_CONFIGS=(
    "claudecode|.claude|"
    "opencode|.opencode|AGENTS.md"
    "copilot|.github|"
    "codexcli|.codex|AGENTS.md"
    "geminicli|.gemini|GEMINI.md"
    "agentsmd|.agents|AGENTS.md"
    "antigravity|.agent|"
)

# Copilot content lives in .github/ alongside CI infrastructure (actions/, workflows/).
# Only these subdirectories and files are copilot-generated content:
COPILOT_SUBDIRS=("agents" "instructions" "prompts" "skills")
COPILOT_ROOT_FILES=("copilot-instructions.md")

# Build bundles for each platform
echo ""
echo "=== Building platform bundles ==="

for config in "${PLATFORM_CONFIGS[@]}"; do
    IFS='|' read -r platform dir_name root_files <<< "$config"
    src_dir="${PROJECT_ROOT}/${dir_name}"
    bundle_name="dotnet-agent-harness-${platform}.zip"
    bundle_path="${DIST_DIR}/${bundle_name}"

    if [[ ! -d "$src_dir" ]]; then
        echo "Skipping ${platform}: directory ${src_dir} does not exist"
        continue
    fi

    echo "Building ${bundle_name}..."
    cd "${PROJECT_ROOT}"

    if [[ "$platform" == "copilot" ]]; then
        # Copilot: selectively zip only copilot content from .github/
        # Excludes CI infrastructure (.github/actions/, .github/workflows/)
        local_args=()
        for subdir in "${COPILOT_SUBDIRS[@]}"; do
            if [[ -d ".github/${subdir}" ]]; then
                local_args+=(".github/${subdir}")
            fi
        done
        for rf in "${COPILOT_ROOT_FILES[@]}"; do
            if [[ -f ".github/${rf}" ]]; then
                local_args+=(".github/${rf}")
            fi
        done
        if [[ ${#local_args[@]} -gt 0 ]]; then
            zip -r "${bundle_path}" "${local_args[@]}" || {
                echo "Warning: Could not create zip for ${platform}"
                continue
            }
        else
            echo "Warning: No copilot content found in .github/"
            continue
        fi
    else
        # All other platforms: zip the entire directory
        zip -r "${bundle_path}" "${dir_name}" -x "*.git*" -x "*/node_modules/*" 2>/dev/null || {
            echo "Warning: Could not create zip for ${platform}"
            continue
        }
    fi

    # Add root-level files if specified
    if [[ -n "$root_files" ]]; then
        IFS=',' read -ra files <<< "$root_files"
        for rf in "${files[@]}"; do
            if [[ -f "${PROJECT_ROOT}/${rf}" ]]; then
                zip "${bundle_path}" "${rf}" 2>/dev/null || true
            fi
        done
    fi

    if [[ -f "$bundle_path" ]]; then
        size=$(du -h "$bundle_path" | cut -f1)
        echo "  Created ${bundle_name} (${size})"
    fi
done

# Summary
echo ""
echo "=== Build complete ==="
echo "Bundles in ${DIST_DIR}:"
ls -lh "${DIST_DIR}"/*.zip 2>/dev/null || echo "  (no bundles created)"
