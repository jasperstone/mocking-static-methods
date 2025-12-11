# Quick Reference Card - Test Orchestrator

## Quick Start (30 seconds)

```bash
cd /home/jastone/src/mocking-static-methods
python test_orchestrator.py
```

## Output Files

- **test_metrics.csv** - Results table
- **test_logs/orchestrator_*.log** - Execution log
- **cloned_repos/abp/GeneratedTests/** - Generated test files

## What It Does

| Step | Command | Output |
|------|---------|--------|
| 1. Scan | Find static methods in C# | files_with_static_calls |
| 2. Generate | Create xUnit+Moq tests | unit_tests_generated |
| 3. Build | Run build-all.ps1 | build_success |
| 4. Test | Run test-all.ps1 | tests_success |
| 5. Extract | Parse coverage HTML | final_coverage |

## Key Patterns Found

- `DateTime.Now`
- `DateTime.UtcNow`
- `File.Exists`
- `Directory.Exists`
- `Guid.NewGuid`

## CSV Output Example

```
timestamp,files_with_static_calls,unit_tests_generated,build_status,test_status,final_coverage
2024-12-07T14:30:45.123456,42,85,✅ PASS,✅ PASS,72.5
```

## Files to Know

| File | Purpose |
|------|---------|
| test_orchestrator.py | Main orchestrator |
| agent_tools.py | Agent framework tools |
| build-all.ps1 | Builds all ABP solutions |
| test-all.ps1 | Runs tests + coverage |
| common.ps1 | Lists all solutions |
| index.html | Coverage report (parsed) |

## Directories

```
cloned_repos/abp/
├── GeneratedTests/          ← Generated test files here
├── framework/CoverageReport/index.html  ← Coverage source
└── build/
    ├── build-all.ps1
    └── test-all.ps1
```

## Agent Framework Integration

Tools available in `agent_tools.py`:
1. `analyze_static_method()` - Analyze methods
2. `generate_mock_test()` - Create tests
3. `get_moq_setup_template()` - Moq patterns

Use with Azure OpenAI agent for autonomous test generation.

## Log Locations

- `test_logs/orchestrator_*.log` - Main log
- `test_logs/abp_build.log` - Build output
- `test_logs/abp_tests.log` - Test output

## Troubleshooting Checklist

- [ ] .NET SDK installed? (`dotnet --version`)
- [ ] PowerShell available? (`pwsh --version`)
- [ ] ABP directory exists? (`ls cloned_repos/abp`)
- [ ] Scripts executable? (`ls -l cloned_repos/abp/build/`)
- [ ] No syntax errors? Check `test_logs/orchestrator_*.log`

## Key Metrics

| Metric | Meaning |
|--------|---------|
| files_with_static_calls | How many files to mock |
| unit_tests_generated | Tests created |
| build_success | Build passed? |
| tests_success | Tests passed? |
| final_coverage | % coverage |

## Environment

- Python 3.12.3
- Virtual env: `.venv/`
- Command prefix: `/home/jastone/src/mocking-static-methods/.venv/bin/python`

## Key Configuration

```python
# In test_orchestrator.py
ABP_PROJECT_DIR = './cloned_repos/abp'
BUILD_SCRIPT = './cloned_repos/abp/build/build-all.ps1'
TEST_SCRIPT = './cloned_repos/abp/build/test-all.ps1'
COVERAGE_REPORT = './cloned_repos/abp/framework/CoverageReport/index.html'
GENERATED_TESTS_DIR = './cloned_repos/abp/GeneratedTests'
METRICS_CSV = 'test_metrics.csv'
```

## Testing Locally

```bash
# Check if orchestrator runs without errors
python test_orchestrator.py 2>&1 | head -50

# View metrics after run
head -2 test_metrics.csv

# Check generated tests
ls cloned_repos/abp/GeneratedTests/ | head -5

# View detailed logs
tail -100 test_logs/orchestrator_*.log
```

---

**Status**: ✅ Ready to use
**Last Updated**: December 7, 2025
