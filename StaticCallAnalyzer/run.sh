#!/usr/bin/env bash
# Wrapper: run StaticCallAnalyzer against a host directory via Docker.
# Usage: run.sh <path-to-source-tree>
# Stdout is the analyzer's JSON output (consumed by aggregate_baseline.py).
set -euo pipefail

if [[ $# -lt 1 ]]; then
    echo "Usage: $0 <path-to-source-tree>" >&2
    exit 64
fi

TARGET="$1"
if [[ ! -d "$TARGET" ]]; then
    echo "run.sh: target directory not found: $TARGET" >&2
    exit 66
fi

IMAGE_TAG="static-call-analyzer:local"
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Build if missing. Build output goes to stderr so stdout stays JSON-clean.
if ! docker image inspect "$IMAGE_TAG" >/dev/null 2>&1; then
    echo "run.sh: building $IMAGE_TAG (one-time)..." >&2
    docker build -t "$IMAGE_TAG" "$HERE" >&2
fi

ABS_TARGET="$(realpath "$TARGET")"
exec docker run --rm \
    -v "$ABS_TARGET":/src:ro \
    "$IMAGE_TAG" /src
