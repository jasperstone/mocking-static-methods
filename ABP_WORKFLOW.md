# ABP Test Generation Workflow

## Quick Start

```bash
cd /home/jastone/src/mocking-static-methods

# Run the orchestrator
python test_orchestrator.py

# Check results
cat test_metrics.csv
tail -100 test_logs/orchestrator_*.log
```

## Workflow Steps

### 1. Detect Static Methods (scan `cloned_repos/abp/`)
```
Files scanned: All .cs files
Patterns searched:
  ✓ DateTime.Now
  ✓ DateTime.UtcNow
  ✓ File.Exists
  ✓ Directory.Exists
  ✓ Guid.NewGuid
```

### 2. Generate Tests per Static Method
```
Output: cloned_repos/abp/GeneratedTests/*.cs
Format: xUnit + Moq scaffolds
One test file per static method
```

### 3. Build All ABP Solutions
```
Script: cloned_repos/abp/build/build-all.ps1
Solutions: Defined in build/common.ps1
  - framework
  - modules/basic-theme
  - modules/users
  - modules/permission-management
  - (and more...)
```

### 4. Run Tests & Collect Coverage
```
Script: cloned_repos/abp/build/test-all.ps1
Output: XPlat Code Coverage reports
Location: framework/CoverageReport/index.html
```

### 5. Extract & Record Metrics
```
Coverage parsed from: framework/CoverageReport/index.html
Results saved to: test_metrics.csv
Columns:
  - timestamp
  - files_with_static_calls
  - unit_tests_generated
  - build_success
  - build_status
  - tests_success
  - test_status
  - final_coverage
  - pattern breakdowns
```

## Agent Framework Integration Points

### Current State (PoC)
- ✅ `agent_tools.py` created with test generation tools
- ✅ Tools compatible with Microsoft Agent Framework
- ✅ Generates xUnit/Moq test scaffolds
- ⚠️ Currently using programmatic generation (not agent-driven)

### For Production (with Azure OpenAI Agent)
Replace the test generation logic in `generate_unit_tests_with_agent()`:

```python
from agent_framework.azure import AzureOpenAIChatClient
from azure.identity import DefaultAzureCredential

# Initialize agent with tools
tools = TestGenerationTools()
agent = AzureOpenAIChatClient(
    credential=DefaultAzureCredential()
).create_agent(
    instructions="""
    You are an expert C# test engineer. 
    Generate comprehensive unit tests for static methods with proper mocking.
    Use xUnit and Moq frameworks.
    Each test should verify the static method behavior in isolation.
    """,
    tools=[
        tools.analyze_static_method,
        tools.generate_mock_test,
        tools.get_moq_setup_template
    ]
)

# For each static method, ask agent to generate test
for method_name in static_methods:
    result = await agent.run(
        f"Generate a comprehensive test for static method: {method_name}"
    )
    # Write result.text to test file
```

## CSV Output Format

```csv
timestamp,files_with_static_calls,unit_tests_generated,build_success,build_status,tests_success,test_status,final_coverage,files_with_DateTime.Now,...
2024-12-07T14:23:45.123456,42,85,True,✅ PASS,True,✅ PASS,72.5,12,...
```

## Logs

- `test_logs/orchestrator_YYYYMMDD_HHMMSS.log` - Main orchestrator log
- `test_logs/abp_build.log` - Build output
- `test_logs/abp_tests.log` - Test output

## Troubleshooting

### Build Fails
- Check `test_logs/abp_build.log` for errors
- Ensure all dependencies are restored: `dotnet restore` in build/ folder

### Tests Fail (Treat as Metric)
- Tests are still run and results recorded
- Failures count toward test_status metric
- Check `test_logs/abp_tests.log` for details

### Coverage Not Found
- Verify `framework/CoverageReport/index.html` exists
- Check HTML structure matches parsing patterns
- Add custom HTML parsing if report format changes

### GeneratedTests Directory Not Found
- Automatically created by orchestrator
- Should be picked up by build/test-all.ps1 scripts
- Verify solutions include GeneratedTests path in their discovery

## Key Metrics

| Metric | Purpose |
|--------|---------|
| files_with_static_calls | How many files need mocking |
| unit_tests_generated | Coverage of static methods |
| build_success | If generated tests don't break build |
| tests_success | If generated tests pass |
| final_coverage | Overall project test coverage % |

## Success Criteria (PoC)

- ✅ Scans ABP for static methods
- ✅ Generates one test per method
- ✅ Tests placed in GeneratedTests/
- ✅ Runs build-all.ps1 successfully
- ✅ Runs test-all.ps1 successfully
- ✅ Extracts coverage from HTML
- ✅ Records metrics to CSV
- ✅ Agent framework tools ready for integration
