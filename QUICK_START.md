# Quick Start Guide - Test Orchestrator

## What You Need

Before running the test orchestrator, make sure you have:

### 1. Python 3.7 or higher
```bash
python3 --version
```

### 2. .NET SDK 6.0 or higher
```bash
dotnet --version
```
If not installed, download from: https://dotnet.microsoft.com/download

## Getting Started in 3 Steps

### Step 1: Install Python Dependencies
```bash
pip install -r requirements.txt
```

### Step 2: Run the Orchestrator
```bash
python3 test_orchestrator.py
```

The script will:
- Process all projects in `cloned_repos/` directory
- Generate test metrics in `test_metrics.csv`
- Create detailed logs in `test_logs/` directory
- Save metrics incrementally after each project

### Step 3: Review Results

After processing completes:

1. **View CSV Results**
   ```bash
   cat test_metrics.csv
   ```
   Shows aggregated metrics for all projects in a spreadsheet-friendly format

2. **View Detailed Logs**
   ```bash
   ls test_logs/
   cat test_logs/orchestrator_*.log
   ```
   Contains step-by-step processing information

3. **Check Project-Specific Errors**
   ```bash
   cat test_logs/{project_name}_build_errors.log
   cat test_logs/{project_name}_test_failures.log
   ```

## Understanding the Output

### CSV Columns
- **project_name**: Name of the analyzed project
- **files_with_static_calls**: How many files have static method calls (DateTime.Now, etc.)
- **initial_coverage**: Code coverage % before generating tests
- **final_coverage**: Code coverage % after generating tests
- **coverage_change**: Difference in coverage (positive = improvement)
- **unit_tests_generated**: Number of test files created
- **build_failures_before_correction**: Build errors found initially
- **test_failures_after_correction**: Test failures after fixes
- **build_error_correction_success**: Whether fixes worked (True/False)

### Static Methods Detected
The script finds files containing:
- `DateTime.Now` - Current local time
- `DateTime.UtcNow` - Current UTC time
- `File.Exists()` - File existence check
- `Directory.Exists()` - Directory existence check
- `Guid.NewGuid()` - GUID generation

## Example Workflow

```bash
# Terminal Session Example
$ python3 test_orchestrator.py

2025-12-03 21:35:00,123456 - INFO - Test Orchestrator started
2025-12-03 21:35:00,123456 - INFO - .NET SDK version: 8.0.100
2025-12-03 21:35:00,123456 - INFO - Found 10 projects to process
2025-12-03 21:35:00,123456 - INFO - Processing project 1/10: abp

# ... processing continues ...
# Press Ctrl+C to stop

$ cat test_metrics.csv
project_name,timestamp,files_with_static_calls,initial_coverage,final_coverage,...
abp,2025-12-03T21:35:00,345,62.5,65.3,...
aspnetcore,2025-12-03T21:36:15,442,71.2,72.1,...

$ ls test_logs/
orchestrator_20251203_213500.log
abp_build_errors.log
abp_test_failures.log
aspnetcore_build_errors.log
aspnetcore_test_failures.log
```

## Common Issues

### Problem: "dotnet: command not found"
**Solution**: Install .NET SDK (see https://dotnet.microsoft.com/download)

### Problem: Script runs but shows N/A for coverage
**Solution**: This is normal if projects don't have code coverage tools installed. The analysis still works for finding static methods.

### Problem: Some projects take very long to process
**Solution**: This is expected for large projects. The script runs tests which can take significant time. You can:
- Let it run in the background (use `screen` or `tmux`)
- Check logs while it runs: `tail -f test_logs/orchestrator_*.log`
- Stop with Ctrl+C (results saved so far are in CSV)

### Problem: "No solution files found" warning
**Solution**: Some directories might not be .NET projects. The script gracefully skips them.

## Tips and Tricks

### 1. Monitor Progress in Real-Time
```bash
# In a new terminal window
tail -f test_logs/orchestrator_*.log
```

### 2. View CSV in a Spreadsheet
```bash
# Open in Excel/LibreOffice
# Or use command-line viewer
column -t -s ',' test_metrics.csv
```

### 3. Analyze Specific Project Errors
```bash
grep -i "error" test_logs/projectname_build_errors.log
grep -i "failed" test_logs/projectname_test_failures.log
```

### 4. Count Static Method Occurrences
```bash
# Find projects with most DateTime.Now calls
grep -r "DateTime\.Now" cloned_repos/ --include="*.cs" | wc -l
```

## Next Steps

After gathering metrics:

1. **Review Coverage Changes**: Identify projects that need more test coverage
2. **Analyze Build Failures**: Check error logs to understand common issues
3. **Compare Projects**: Use CSV to compare metrics across different projects
4. **Plan Improvements**: Use insights to prioritize refactoring efforts

## Additional Resources

- **Full Documentation**: See `TEST_ORCHESTRATOR_README.md`
- **Script Source**: `test_orchestrator.py`
- **Original Orchestrator**: `orchestrator.py` (for repo discovery and analysis)

---

**Need help?** Check the detailed logs in `test_logs/` directory or review the full README.
