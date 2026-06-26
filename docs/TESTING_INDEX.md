# Testing Infrastructure Index

Complete guide to local testing framework for RoslynRefactorTool.

---

## 📚 Documentation

### Quick Reference
- **[QUICK_START_TESTING.md](QUICK_START_TESTING.md)** - 30-minute guide to first results
  - How to run tests
  - How to interpret results
  - What to do if issues found

### Strategic
- **[LOCAL_TESTING_STRATEGY.md](LOCAL_TESTING_STRATEGY.md)** - Complete testing plan
  - 4-phase approach (A: Verification → B: Compilation → C: Improvement → D: Full)
  - Repository priorities
  - Success criteria
  - Timeline estimates

---

## 🔧 Tools & Scripts

### Bash Testing Harness
**File**: `tools/test_local.sh`  
**Purpose**: Quick testing of applicability on real sites  
**Usage**:
```bash
bash tools/test_local.sh <repo> <transform> <limit>

# Examples:
bash tools/test_local.sh eShop wrapper_interface 25
bash tools/test_local.sh OpenRA wrapper_interface 13
bash tools/test_local.sh aspnetcore parameterize_dependency 50
```

**Output**: 
- CSV file in `test_results_local/results_*.csv`
- Console summary with applicability rate and rejection breakdown

**Speed**: ~1 sec per site

### Python Testing Harness
**File**: `tools/local_test_harness.py`  
**Purpose**: Advanced testing including compilation and coverage  
**Features**:
- Applicability analysis
- Compilation testing
- Build time measurement
- Coverage measurement (when available)
- CSV and JSON export

**Status**: Ready for Phase B

### GitHub Actions Workflow
**File**: `.github/workflows/test-refactor.yml`  
**Purpose**: Automated testing on every commit  
**Triggers**:
- Push to main/develop
- Pull requests
- Manual (Actions > Run workflow)

**Tests**:
- Builds tool (Debug + Release)
- Tests each transform on different repos
- Uploads results as artifacts

**View results**:
```
GitHub > Actions > Test Refactoring Tool > Latest run > Artifacts
```

---

## 📊 Understanding Results

### CSV Output Format
```
site_id,repo,kind,transform,applicable,reason,build_ok
eShop:0001,eShop,Extension,wrapper_interface,true,,true
OpenRA:0003,OpenRA,NonVirtual,wrapper_interface,false,no_receiver_source,false
```

### Key Metrics
| Metric | Formula | Good | Concern | Bad |
|--------|---------|------|---------|-----|
| Applicability | applicable / total | >40% | 20-40% | <20% |
| Compile rate | compiled / applicable | >85% | 70-85% | <70% |
| Coverage gain | (covered_after - covered_before) / total | >0.5% | 0.2-0.5% | <0.2% |

### Interpretation
- **High applicability + high compile = good** ✅
- **High applicability + low compile = tool bugs** ⚠️
- **Low applicability = expected blockers** (review against gap analysis)

---

## 🎯 Testing Phases

### Phase A: Verification (TODAY)
**Goal**: Confirm tool works on real code  
**Time**: 15-30 min  
**Commands**:
```bash
bash tools/test_local.sh eShop wrapper_interface 25
bash tools/test_local.sh OpenRA wrapper_interface 13
bash tools/test_local.sh aspnetcore parameterize_dependency 25
```
**Success criteria**: >40% applicability, expected rejection reasons  
**Next step**: Proceed to Phase B or debug

### Phase B: Compilation (TOMORROW)
**Goal**: Verify generated code actually compiles  
**Time**: 1-2 hours  
**Tool**: Python harness  
**Success criteria**: >80% compilation rate  
**Next step**: Phase C if issues found

### Phase C: Improvement (THIS WEEK)
**Goal**: Fix issues and improve coverage  
**Time**: Varies (30 min - 2 hr per issue)  
**Process**: Triage → Fix → Re-test → Measure  
**Success criteria**: Coverage gain +1-2%  
**Next step**: Phase D

### Phase D: Validation (END OF WEEK)
**Goal**: Full repo testing and ready for models  
**Time**: 2-3 hours  
**Success criteria**: All metrics meet targets  
**Result**: ✅ Ready for Phase 4 model experiment

---

## 📈 Measurement Approach

### Applicability Analysis
```bash
# Count by rejection reason
tail -n +2 test_results_local/results_*.csv | cut -d',' -f6 | sort | uniq -c | sort -rn

# Count by transform
tail -n +2 test_results_local/results_*.csv | cut -d',' -f5 | sort | uniq -c

# Applicability rate
APPLICABLE=$(grep ",true," test_results_local/results_*.csv | wc -l)
TOTAL=$(tail -n +2 test_results_local/results_*.csv | wc -l)
echo "Applicability: $APPLICABLE/$TOTAL = $(echo "scale=1; $APPLICABLE*100/$TOTAL" | bc)%"
```

### Compilation Testing (Phase B)
- For each applicable site:
  - Copy project to temp directory
  - Run `dotnet build`
  - Record: success/failure/build_time
  - Record: first error if failed

### Coverage Measurement (Phase B+)
- Run cobertura on original
- Run cobertura on refactored (if compiled)
- Compare: new covered lines / total refactored sites
- Aggregate across repositories

---

## 🚨 Common Issues & Solutions

### Issue: Tool not found
```bash
# Rebuild
cd RoslynRefactorTool
dotnet build
```

### Issue: "Site not found" or "File not found"
```bash
# Check paths in CSV
head -1 phases/phase1-baseline/reports/mode1_sites.csv
```

### Issue: Low applicability rate (<20%)
```bash
# Check for regressions
git log --oneline -5 RoslynRefactorTool/

# Verify against baseline
# (should match or improve from previous run)
```

### Issue: Compilation failures for applicable sites
```bash
# Extract generated code
# Check for CS errors
# Verify tool is generating valid syntax
```

---

## 📊 Data Storage

All test results stored in: `test_results_local/`

```
test_results_local/
├── results_20260622_140530.csv
├── results_20260622_141500.csv
└── ... (timestamped files, never deleted automatically)
```

To clean:
```bash
rm -rf test_results_local/
```

---

## 🔗 Integration with GitHub

### Automatic Testing
Every commit to `RoslynRefactorTool/`:
1. Builds tool (Debug + Release)
2. Tests on sample sites
3. Uploads results
4. Reports status

### Manual Trigger
```
GitHub > Actions > Test Refactoring Tool > Run workflow
```

### View Results
```
GitHub > Actions > Latest run > Artifacts > download
```

---

## 📋 Checklist: Ready for Models

Before running Phase 4 (AI experiment):

- [ ] Phase A (Verification) complete
- [ ] Phase B (Compilation) complete  
- [ ] Phase C (Improvements) complete if needed
- [ ] Applicability rate ≥80%
- [ ] Compilation rate ≥85%
- [ ] Zero critical bugs
- [ ] Coverage baseline confirmed
- [ ] Tool runs <5 sec per site
- [ ] GitHub Actions passing
- [ ] Documentation updated
- [ ] All results reproducible

---

## 🎓 Learning Path

1. **Start**: Read [QUICK_START_TESTING.md](QUICK_START_TESTING.md)
2. **Run**: `bash tools/test_local.sh eShop wrapper_interface 25`
3. **Analyze**: Review results in `test_results_local/`
4. **Learn**: Read [LOCAL_TESTING_STRATEGY.md](LOCAL_TESTING_STRATEGY.md)
5. **Plan**: Decide what to test next based on results
6. **Iterate**: Fix issues, re-test, measure improvement

---

## 📞 Support

For questions or issues:
- Check [LOCAL_TESTING_STRATEGY.md](LOCAL_TESTING_STRATEGY.md) section "Phase" for what to do
- Review [QUICK_START_TESTING.md](QUICK_START_TESTING.md) "If You Find Issues"
- Check tool output for specific error messages

---

## 🚀 Next Steps

1. **Right now**: Read docs/QUICK_START_TESTING.md
2. **Next 15 min**: Run first test
3. **Next 30 min**: Analyze results
4. **Next 1 hour**: Decide on Phase B
5. **By end of week**: All phases complete, ready for models

---

**Status**: Testing infrastructure ready, awaiting first run ✓
