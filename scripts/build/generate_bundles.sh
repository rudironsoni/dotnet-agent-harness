#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
BUNDLES_DIR="$ROOT_DIR/bundles"

echo "Generating bundles..."

# Ensure bundles directory exists
mkdir -p "$BUNDLES_DIR"

# Check for rulesync
if ! command -v rulesync &> /dev/null; then
    echo "Error: rulesync not found. Install from https://github.com/rudironsoni/rulesync"
    exit 1
fi

# Generate all targets
echo "Running rulesync generate..."
rulesync generate \
    --source "$ROOT_DIR" \
    --output "$BUNDLES_DIR" \
    --targets "*" \
    --features "*"

# Create tar.gz archives
echo "Creating bundle archives..."
cd "$BUNDLES_DIR"
for dir in */; do
    if [ -d "$dir" ]; then
        target="${dir%/}"
        echo "  Packaging $target..."
        tar -czf "${target}.tar.gz" "$target"
        rm -rf "$target"
    fi
done

echo "Bundles generated in $BUNDLES_DIR"
ls -la "$BUNDLES_DIR"/*.tar.gz
