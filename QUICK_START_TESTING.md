# Quick Start: Local Testing

Get the refactoring tool validated locally before model experiment. ~30 minutes to first results.

---

## 1. Verify Tool Builds

```bash
cd /home/jastone/src/mocking-static-methods
cd RoslynRefactorTool
dotnet build

# Should print: "Build succeeded"
```

---

## 2. Run Phase A (Verification) - 15 minutes

Test the tool against real Mode #1 sites:

```bash
cd /home/jastone/src/mocking-static-methods

# Test wrapper_interface on eShop (25 sites)
bash tools/test_local.sh eShop wrapper_interface 25

# Test wrapper_interface on OpenRA (static utility test)
bash tools/test_local.sh OpenRA wrapper_interface 13

# Test parameterize_dependency on eShop
bash tools/test_local.sh eShop parameterize_dependency 25
```

**Watch for:**
- ✅ Sites marked "APPLICABLE" (good!)
- ❌ Rejection reasons (will show top reasons)
- ⏱️ Speed (should be <1 sec per site)

**Expected output:**
```
======================================================================
TEST RESULTS
======================================================================
Sites tested: 25
Applicable: 10 (40%)
Build successful: 10/10 (100%)
Rejected: 15

Top rejection reasons:
  no_receiver_source: 8
  receiver_not_ctor_reachable: 5
  unbound_receiver: 2
```

---

## 3. Analyze Results - 10 minutes

Results saved to: `test_results_local/results_*.csv`

```bash
cd /home/jastone/src/mocking-static-methods

# View latest results
ls -lh test_results_local/
cat test_results_local/results_*.csv | column -t -s,

# Count rejections by reason
tail -n +2 test_results_local/results_*.csv | cut -d',' -f6 | sort | uniq -c | sort -rn
```

**Analyze:**
- Are applicability rates matching expectations?
- Are rejection reasons the ones we analyzed?
- Do applicable sites actually compile?

---

## 4. Deep Dive - Optional

Look at specific failure cases:

```bash
# Find a non-applicable site
grep "false" test_results_local/results_*.csv | head -3

# Manually check that site
# Compare against our blocker analysis
```

---

## 5. GitHub Actions (Continuous)

Automatic testing on every commit:

```bash
# Push changes to trigger workflow
git add .
git commit -m "Test: Enhanced wrapper pattern"
git push origin main

# View results
# GitHub > Actions > Test Refactoring Tool > Latest run
```

---

## Phase B Preview: Compilation Testing

Once we verify applicability rates are good, next step:

```bash
# This will copy refactored code to temp directories and compile
python3 tools/local_test_harness.py

# Measures: 
# - Can generated code compile?
# - Build time?
# - Any compilation errors?
```

---

## Interpretation Guide

### ✅ Good Signs
- Applicability rate >30% for wrapper_interface
- Applicability rate >50% for parameterize_dependency
- Most rejections are from known blockers (no_receiver_source, etc.)
- No unexpected error patterns

### ⚠️ Concerning Signs
- Applicability <20% (something wrong with tool?)
- Many `tool_error` rejections (crashes?)
- Unexpected rejection reasons
- Compilation failures for applicable sites

### ❌ Show Stoppers
- Tool crashes on valid sites
- >30% of applicable sites don't compile
- Generated code doesn't parse
- Major performance issues

---

## If You Find Issues

### Issue: Tool crashes
```bash
# Run with error output
/home/jastone/src/mocking-static-methods/RoslynRefactorTool/bin/Debug/RoslynRefactorTool \
  --transform wrapper_interface \
  --repo /path/to/repo \
  --site site_id 2>&1
```

### Issue: Generated code doesn't compile
```bash
# Save generated code to file
# Copy to project
# Run: dotnet build
# Look at first error

# Then check if it's a tool bug or edge case
```

### Issue: Low applicability rate
```bash
# Count rejection reasons
# See if top reasons are expected blockers
# Or if we have a regression
```

---

## Next Steps (Based on Results)

### If Phase A succeeds (applicability >40%):
→ Move to Phase B (Compilation testing)

### If Phase A is borderline (20-40%):
→ Investigate top rejections
→ Check for regressions
→ Run enhanced debugging

### If Phase A fails (<20%):
→ Check for tool crashes
→ Review recent changes
→ Rebuild tool
→ Check reference issues

---

## File Locations

| Item | Path |
|------|------|
| Tool executable | `RoslynRefactorTool/bin/Debug/RoslynRefactorTool` |
| Test script | `tools/test_local.sh` |
| Test data | `phases/phase1-baseline/reports/mode1_sites.csv` |
| Test results | `test_results_local/` |
| GitHub workflow | `.github/workflows/test-refactor.yml` |
| Strategy doc | `LOCAL_TESTING_STRATEGY.md` |

---

## Useful Commands

```bash
# Count total Mode #1 sites
wc -l phases/phase1-baseline/reports/mode1_sites.csv

# Find all sites for a repository
grep "^eShop:" phases/phase1-baseline/reports/mode1_sites.csv | wc -l

# Clean test results
rm -rf test_results_local/

# View CSV nicely
column -t -s, test_results_local/results_*.csv | less -S
```

---

## Timeline

| Phase | Task | Time | Status |
|-------|------|------|--------|
| A | Verification | 15 min | ← Start here |
| A+ | Analysis | 10 min | |
| B | Compilation | 30 min | After A passes |
| C | Fix issues | 30-120 min | As needed |
| D | Full repo | 2 hr | Final validation |
| **Total** | **Ready for models** | **1-2 days** | |

---

## Q&A

**Q: Do I need to modify the repos?**  
A: No! Tests use read-only access and create temp copies for compilation.

**Q: Will this break anything?**  
A: No, it's completely safe. All testing is local and non-destructive.

**Q: How long does each test run?**  
A: <1 sec per site. 25 sites ≈ 30 seconds total.

**Q: Can I run tests in parallel?**  
A: Yes, run multiple `test_local.sh` commands in different terminals.

**Q: What if I find bugs?**  
A: Document them, fix the tool, re-run tests. Iterate fast.

---

## Let's Go! 🚀

```bash
cd /home/jastone/src/mocking-static-methods
bash tools/test_local.sh eShop wrapper_interface 25
```

Watch the results and report back what you see!
