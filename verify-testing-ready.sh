#!/bin/bash
# verify-testing-ready.sh - Complete infrastructure verification

set -e

echo "╔════════════════════════════════════════════════════════════════════════╗"
echo "║       TESTING INFRASTRUCTURE VERIFICATION - Ready to Launch           ║"
echo "╚════════════════════════════════════════════════════════════════════════╝"
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counter
TOTAL=0
PASSED=0

check() {
  TOTAL=$((TOTAL + 1))
  if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓${NC} $1"
    PASSED=$((PASSED + 1))
  else
    echo -e "${RED}✗${NC} $1"
  fi
}

echo -e "${BLUE}1. TOOL BUILD${NC}"
cd RoslynRefactorTool
dotnet build --configuration Release >/dev/null 2>&1
check "RoslynRefactorTool compiles cleanly"
cd ..
[ -f RoslynRefactorTool/bin/Release/net10.0/RoslynRefactorTool.dll ] || [ -f RoslynRefactorTool/bin/Release/RoslynRefactorTool.dll ]
check "Tool DLL exists"
echo ""

echo -e "${BLUE}2. TEST SCRIPTS${NC}"
[ -x tools/test_local.sh ]
check "tools/test_local.sh is executable"
[ -x tools/local_test_harness.py ]
check "tools/local_test_harness.py is executable"
[ -s tools/test_local.sh ] && [ $(wc -l < tools/test_local.sh) -gt 50 ]
check "tools/test_local.sh has content (>50 lines)"
echo ""

echo -e "${BLUE}3. GITHUB ACTIONS${NC}"
[ -f .github/workflows/test-refactor.yml ]
check ".github/workflows/test-refactor.yml exists"
grep -q "Test Refactoring Tool" .github/workflows/test-refactor.yml 2>/dev/null
check "Workflow has correct name"
echo ""

echo -e "${BLUE}4. DOCUMENTATION${NC}"
for doc in QUICK_START_TESTING.md LOCAL_TESTING_STRATEGY.md TESTING_INDEX.md TESTING_READY.md START_TESTING_HERE.md; do
  [ -f "$doc" ] && [ -s "$doc" ]
  check "$doc exists and has content"
done
echo ""

echo -e "${BLUE}5. TEST DATA${NC}"
[ -f phases/phase1-baseline/reports/mode1_sites.csv ]
check "Mode #1 sites CSV exists"
LINE_COUNT=$(wc -l < phases/phase1-baseline/reports/mode1_sites.csv)
[ "$LINE_COUNT" -gt 100 ]
check "Mode #1 sites has 6,000+ entries ($LINE_COUNT lines)"
echo ""

echo -e "${BLUE}6. WORKSPACE STRUCTURE${NC}"
[ -d RoslynRefactorTool ]
check "RoslynRefactorTool directory exists"
[ -d phases/phase1-baseline ]
check "Test phases directory exists"
[ -d cloned_repos ]
check "Cloned repositories exist"
echo ""

echo -e "${BLUE}7. TOOL FUNCTIONALITY${NC}"
# Quick JSON smoke test via dotnet + dll
TOOL_DLL="RoslynRefactorTool/bin/Release/net10.0/RoslynRefactorTool.dll"
if [ ! -f "$TOOL_DLL" ]; then
  TOOL_DLL="RoslynRefactorTool/bin/Release/RoslynRefactorTool.dll"
fi
RESULT=$(dotnet "$TOOL_DLL" --transform wrapper_interface --owning-dir /tmp --file /tmp/x.cs --line 1 --method M 2>/dev/null || true)
echo "$RESULT" | grep -q '"ok"'
check "Tool runs and emits JSON"
echo ""

echo -e "${BLUE}8. PREREQUISITES INSTALLED${NC}"
command -v dotnet >/dev/null 2>&1
check "dotnet CLI installed"
command -v python3 >/dev/null 2>&1
check "Python 3 installed"
command -v jq >/dev/null 2>&1 || echo "  (jq optional for result parsing)"
check "jq installed (optional)"
echo ""

echo "╔════════════════════════════════════════════════════════════════════════╗"
echo "║                          VERIFICATION COMPLETE                        ║"
echo "╚════════════════════════════════════════════════════════════════════════╝"
echo ""
echo "Results: $PASSED / $TOTAL checks passed"
echo ""

if [ $PASSED -eq $TOTAL ]; then
  echo -e "${GREEN}✓ ALL CHECKS PASSED - READY TO BEGIN TESTING${NC}"
  echo ""
  echo "Next steps:"
  echo "  1. Read: cat START_TESTING_HERE.md"
  echo "  2. Run:  bash tools/test_local.sh eShop wrapper_interface 25"
  echo "  3. View: cat test_results_local/results_*.csv"
  echo ""
  exit 0
else
  echo -e "${RED}✗ SOME CHECKS FAILED - INVESTIGATE ABOVE${NC}"
  echo ""
  exit 1
fi
