# Local Testing Strategy: Before Model Experiment

## Goal
Thoroughly test and improve the refactoring tool locally before running the full Phase 4 experiment with AI models. This saves tokens and ensures we have a solid foundation.

---

## Testing Phases

### Phase A: Verification (Today)
**Goal**: Confirm tool works on real code

1. **Build verification**
   - [x] Tool compiles cleanly
   - [ ] Run on sample sites from each repository
   - [ ] Verify JSON output format

2. **Basic functionality**
   - [ ] wrapper_interface produces valid C# syntax
   - [ ] parameterize_dependency produces valid C# syntax
   - [ ] make_virtual produces valid C# syntax
   - [ ] static_utility_wrapper produces valid C# syntax

3. **Applicability analysis**
   - [ ] Count sites by rejection reason
   - [ ] Measure applicability rate per transform
   - [ ] Identify most common blockers

**Expected output**: `test_results_local/results_*.csv`

---

### Phase B: Compilation Testing (Next)
**Goal**: Verify generated code actually compiles

1. **Sample compilation**
   - [ ] Pick 5-10 representative sites
   - [ ] Generate refactored code
   - [ ] Copy to temp directory with all dependencies
   - [ ] Run `dotnet build`
   - [ ] Record success/failure and build time

2. **Build error analysis**
   - [ ] Collect compilation errors
   - [ ] Identify patterns (CS0103, CS1573, etc.)
   - [ ] Fix tool if errors found

3. **Coverage measurement**
   - [ ] Before: Run cobertura on original
   - [ ] After: Run cobertura on refactored (if compiled)
   - [ ] Measure actual coverage change

---

### Phase C: Iterative Improvement (Following days)
**Goal**: Fix issues found and improve coverage

1. **Issue prioritization**
   - [ ] Analyze all failures
   - [ ] Group by root cause
   - [ ] Prioritize by impact (frequency × severity)

2. **Targeted fixes**
   - [ ] Fix high-impact issues
   - [ ] Re-test affected sites
   - [ ] Measure improvement

3. **Enhancement opportunities**
   - [ ] Identify patterns that barely miss applicability
   - [ ] Implement quick wins
   - [ ] Measure new recovery

---

### Phase D: Repository Testing (If needed)
**Goal**: Validate across all 15 repositories

1. **Systematic testing**
   - [ ] Run each transform on each repository
   - [ ] Collect applicability rates
   - [ ] Identify repository-specific patterns

2. **Bottleneck analysis**
   - [ ] Which transforms work best per repo?
   - [ ] Which repos have highest coverage potential?
   - [ ] Are there common patterns we're missing?

3. **Coverage projection**
   - [ ] Extrapolate from sample to full set
   - [ ] Estimate overall coverage gain
   - [ ] Identify next bottleneck to tackle

---

## Test Repositories (Priority Order)

### Tier 1 (Start here)
1. **eShop** - DI patterns, Service locator
   - Reason: High Mode #1 count, clear patterns
   - Sites: 94 total, focus on ServiceProvider

2. **OpenRA** - External types (HttpClient)
   - Reason: Tests our new static utility wrapper
   - Sites: 13 total, good for validation

3. **aspnetcore** - Mixed patterns
   - Reason: Large and diverse, good signal
   - Sites: 936 total, sample 50-100

### Tier 2 (If time)
4. **Orleans** - Complex DI
5. **efcore** - Framework integration

### Tier 3 (Full validation)
6-15: All remaining repositories

---

## Testing Infrastructure

### Local Test Harness
- **Script**: `tools/test_local.sh`
- **Usage**: `bash tools/test_local.sh <repo> <transform> <limit>`
- **Output**: CSV with results per site

### Example: Test wrapper_interface on eShop (25 sites)
```bash
bash tools/test_local.sh eShop wrapper_interface 25
```

### Example: Test new static_utility_wrapper on OpenRA
```bash
bash tools/test_local.sh OpenRA wrapper_interface 13
```

### Python Harness (Advanced)
- **Script**: `tools/local_test_harness.py`
- **Features**: Compilation testing, coverage measurement
- **Status**: Available for compilation testing phase

---

## GitHub Actions Workflow

Automated testing on every commit:

```
workflow: test-refactor.yml

On every push to main/develop:
1. Build tool (Debug + Release)
2. Test wrapper_interface on eShop (25 sites)
3. Test parameterize_dependency on eShop (25 sites)
4. Test make_virtual on aspnetcore (25 sites)
5. Test static_utility_wrapper on OpenRA (15 sites)
6. Upload results as artifacts
```

Trigger manually:
```
Actions > Test Refactoring Tool > Run workflow
```

---

## Success Criteria

### Phase A (Verification)
- ✅ Tool runs on real sites
- ✅ Produces valid JSON output
- ✅ Applicability rates match test data (or better)

### Phase B (Compilation)
- ✅ Sample sites: >80% compile
- ✅ Identify and fix remaining errors
- ✅ Measure actual coverage gain (+0.5-1%)

### Phase C (Improvement)
- ✅ Fix top 3 rejection reasons
- ✅ Improve coverage by +1-2%
- ✅ No new bugs introduced

### Phase D (Ready for Models)
- ✅ >85% of tested sites either applicable or intentionally rejected
- ✅ >80% compilation rate for applicable sites
- ✅ Coverage baseline confirmed
- ✅ Zero critical bugs

---

## Timeline Estimate

| Phase | Task | Time | Status |
|-------|------|------|--------|
| A | Verification | 1 hour | Today |
| B | Compilation testing | 2-3 hours | Tomorrow |
| C | Issue fixes (per issue) | 30 min - 2 hr | This week |
| D | Full repo testing | 2-3 hours | This week |
| **Total** | **Ready for models** | **1-2 days** | **By Wed** |

---

## Measurement Approach

### Applicability Metrics
```
For each site:
  - Run tool with transform
  - Record: applicable? yes/no
  - Record: reason if no

Per repository:
  - Total sites
  - Applicable count
  - Applicability rate
  - Top 5 rejection reasons
```

### Compilation Metrics
```
For each applicable site:
  - Copy to temp directory
  - Run dotnet build
  - Record: compile_ok? yes/no
  - Record: build_time
  - Record: first error if failed
```

### Coverage Metrics
```
For each compiled site:
  - Run cobertura on original (if available)
  - Run cobertura on refactored
  - Measure: line coverage change
  - Measure: new covered lines
  - Aggregate across sites
```

---

## Key Metrics to Track

1. **Applicability Rate** (target: >80%)
   - What % of sites can the tool refactor?
   - By transform, by repo

2. **Compilation Rate** (target: >85%)
   - Of applicable sites, what % compile?
   - By transform, by repo

3. **Coverage Gain** (target: +0.5-1%)
   - Average coverage improvement per site
   - Aggregate across repositories

4. **Build Time** (target: <5 sec avg)
   - How long does compilation take?
   - Identify slow spots

5. **Error Patterns** (target: <5 unique errors)
   - What CS errors occur?
   - Can we fix them in the tool?

---

## Decision Points

### After Phase A (Verification)
- ✅ Continue to Phase B? (if applicability rates reasonable)
- ❌ Stop and debug? (if applicability <50%)

### After Phase B (Compilation)
- ✅ Move to Phase C? (if compilation rate >80%)
- ⚠️ Debug compilation? (if rate 60-80%)
- ❌ Redesign? (if rate <60%)

### After Phase C (Improvements)
- ✅ Ready for models? (if metrics met)
- ⚠️ One more iteration? (if marginal)
- ❌ More work needed? (if major issues remain)

---

## Issue Triage Template

When issues found:

```
ISSUE: [Description]
Frequency: [X sites affected]
Severity: [Blocking / Major / Minor]
Root Cause: [Analysis]
Fix: [Proposed solution]
Effort: [Quick / Medium / Large]
Impact: [% improvement if fixed]
Priority: [High / Medium / Low]
```

---

## Pre-Model Checklist

Before running Phase 4 (models):
- [ ] Applicability rate >80%
- [ ] Compilation rate >85%
- [ ] Zero critical bugs
- [ ] Coverage baseline confirmed
- [ ] Tool runs in <5 sec per site
- [ ] Generated code is clean (no warnings)
- [ ] Documentation updated
- [ ] GitHub Actions passing
- [ ] Results reproducible
- [ ] Ready for model experiment ✓

---

## Notes

- **No model calls**: All testing is local compilation only
- **Fast feedback**: <1 sec per site assessment
- **Incremental**: Fix one issue at a time
- **Measurable**: Every change tracked
- **Reproducible**: GitHub Actions automates testing
- **Safe**: Never modifies source repos (always uses temp copies)

---

## Resources

- Tool: `RoslynRefactorTool/bin/Debug/RoslynRefactorTool`
- Test data: `phases/phase1-baseline/reports/mode1_sites.csv`
- Test harness: `tools/test_local.sh`
- Python harness: `tools/local_test_harness.py`
- Workflow: `.github/workflows/test-refactor.yml`
- Results: `test_results_local/results_*.csv`
