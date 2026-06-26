#!/bin/bash
# Local testing entrypoint (Linux-friendly, current phase-4 tooling).

set -euo pipefail

WORKSPACE="/home/jastone/src/mocking-static-methods"
TEST_RESULTS="$WORKSPACE/test_results_local"
TARGETS_CSV="$WORKSPACE/targets/v2/targets.csv"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

REPO="${1:-eShop}"
TRANSFORM="${2:-wrapper_interface}"
LIMIT="${3:-20}"
MODE="${4:-noverify}"  # noverify|verify

if [[ "$MODE" != "noverify" && "$MODE" != "verify" ]]; then
    echo "Usage: bash tools/test_local.sh <repo> <transform> <limit> [noverify|verify]"
    exit 2
fi

mkdir -p "$TEST_RESULTS"

echo "======================================================================"
echo "RoslynRefactorTool - Local Testing Suite (Current Toolchain)"
echo "======================================================================"
echo "Timestamp: $(date)"
echo "Repository: $REPO"
echo "Transform: $TRANSFORM"
echo "Limit: $LIMIT"
echo "Mode: $MODE"
echo ""

echo "Building RoslynRefactorTool (Release)..."
dotnet build "$WORKSPACE/RoslynRefactorTool/RoslynRefactorTool.csproj" \
    -c Release --nologo -v quiet

if [[ ! -f "$TARGETS_CSV" ]]; then
    echo "ERROR: targets CSV not found: $TARGETS_CSV"
    exit 1
fi

TARGET_IDS=$(awk -F, -v repo="$REPO" 'NR==1 {next} $2==repo {print $1}' "$TARGETS_CSV" | head -n "$LIMIT" | paste -sd, -)
if [[ -z "${TARGET_IDS:-}" ]]; then
    echo "ERROR: no targets found for repo '$REPO' in $TARGETS_CSV"
    exit 1
fi

VERIFY_FLAG="--no-verify-build"
if [[ "$MODE" == "verify" ]]; then
    VERIFY_FLAG="--verify-build"
fi

RESULT_FILE="$TEST_RESULTS/results_${REPO}_${TRANSFORM}_${LIMIT}_${MODE}_${TIMESTAMP}.csv"

python3 "$WORKSPACE/tools/generation/refactor_applicability_sweep.py" \
    --targets "$TARGETS_CSV" \
    --repos-root "$WORKSPACE/cloned_repos" \
    --transform "$TRANSFORM" \
    --target-ids "$TARGET_IDS" \
    $VERIFY_FLAG \
    --jobs "$(nproc)" \
    --out "$RESULT_FILE"

TOTAL=$(($(wc -l < "$RESULT_FILE") - 1))
APPLICABLE=$(awk -F, 'NR>1 && $8=="True" {c++} END{print c+0}' "$RESULT_FILE")
REJECTED=$((TOTAL - APPLICABLE))
BUILD_TRUE=$(awk -F, 'NR>1 && $11=="True" {c++} END{print c+0}' "$RESULT_FILE")

echo ""
echo "======================================================================"
echo "TEST RESULTS"
echo "======================================================================"
echo "Sites tested: $TOTAL"
echo "Applicable: $APPLICABLE"
echo "Rejected: $REJECTED"
if [[ "$MODE" == "verify" ]]; then
    echo "Build ok (True): $BUILD_TRUE"
fi
echo "Results saved to: $RESULT_FILE"
echo "======================================================================"
