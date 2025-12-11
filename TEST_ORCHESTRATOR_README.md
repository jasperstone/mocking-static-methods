# Test Orchestrator - Static Method Mocking Analysis

## Overview

The `test_orchestrator.py` script automates the analysis and testing workflow for projects containing static method calls. It performs the following steps for each project in the `cloned_repos/` directory:

1. **Run unit tests** and capture initial code coverage percentage
2. **Find all files** containing static method calls:
   - `DateTime.Now`
   - `DateTime.UtcNow`
   - `File.Exists`
   - `Directory.Exists`
   - `Guid.NewGuid`
3. **Generate unit tests** for files containing static method calls
4. **Build the solution** and record build failures with detailed error messages
5. **Correct build errors** (automatic package restoration)
6. **Run tests** after corrections and record test failures
7. **Get final code coverage** and compare with initial coverage
8. **Record all metrics** to a CSV file and log messages to files

## Requirements

### Prerequisites
- **Python 3.7+**
- **.NET SDK** (required for building and testing C# projects)
  - Download from: https://dotnet.microsoft.com/download
  - Verify installation: `dotnet --version`

### Python Dependencies
```bash
pip install -r requirements.txt
```

## Usage

### Basic Usage
```bash
python3 test_orchestrator.py
```

The script will process all projects in the `cloned_repos/` directory.

### Output Files

#### 1. **test_metrics.csv**
Main metrics file containing aggregated results for all projects:

| Column | Description |
|--------|-------------|
| `project_name` | Name of the project |
| `timestamp` | ISO format timestamp when processing occurred |
| `files_with_static_calls` | Total number of files with static method calls |
| `initial_coverage` | Code coverage % before modifications (or N/A if unavailable) |
| `final_coverage` | Code coverage % after modifications (or N/A if unavailable) |
| `coverage_change` | Difference between final and initial coverage |
| `unit_tests_generated` | Number of unit tests generated |
| `build_failures_before_correction` | Number of build errors before correction |
| `test_failures_after_correction` | Number of test failures after correction |
| `build_error_correction_success` | Whether build error correction succeeded (True/False) |

#### 2. **test_logs/** Directory
Contains detailed log files:

- **orchestrator_YYYYMMDD_HHMMSS.log**: Main orchestrator log with all processing steps
- **{project_name}_build_errors.log**: Detailed build output and errors
- **{project_name}_test_failures.log**: Detailed test output and failures

### Example CSV Output

```csv
project_name,timestamp,files_with_static_calls,initial_coverage,final_coverage,coverage_change,unit_tests_generated,build_failures_before_correction,test_failures_after_correction,build_error_correction_success
abp,2025-12-03T21:35:00.123456,345,62.5,65.3,2.8,5,2,0,True
aspnetcore,2025-12-03T21:36:15.456789,442,71.2,72.1,0.9,8,0,1,True
efcore,2025-12-03T21:37:30.789012,287,68.9,70.5,1.6,4,3,0,True
```

### Example Log Output

```
2025-12-03 21:35:00,123456 - INFO - Test Orchestrator started
2025-12-03 21:35:00,123456 - INFO - Found 10 projects to process
2025-12-03 21:35:00,123456 - INFO - .NET SDK version: 8.0.100
2025-12-03 21:35:00,123456 - INFO - Processing project 1/10: abp
================================================================
================
2025-12-03 21:35:00,123456 - INFO - STEP 1: Getting initial code coverage
2025-12-03 21:35:00,123456 - INFO - Getting code coverage for ./cloned_repos/abp
2025-12-03 21:35:00,234567 - INFO - Found 166 test project(s)
2025-12-03 21:35:12,345678 - INFO - Code coverage: 62.5%
...
2025-12-03 21:35:15,456789 - INFO - STEP 2: Finding static method calls
2025-12-03 21:35:15,456789 - INFO - Finding static method calls in ./cloned_repos/abp
2025-12-03 21:35:16,567890 - INFO - Found 45 files with DateTime.Now
2025-12-03 21:35:16,567890 - INFO - Found 32 files with DateTime.UtcNow
...
```

## Features

### Static Method Pattern Detection

The script searches for 5 common static method patterns in C# source files:

1. **DateTime.Now**: Access to current local time
2. **DateTime.UtcNow**: Access to current UTC time
3. **File.Exists**: File system existence checks
4. **Directory.Exists**: Directory existence checks
5. **Guid.NewGuid**: GUID generation

These patterns are detected using regex and reported with file counts.

### Automatic Test Generation

For each file containing static method calls, the script generates skeleton unit tests with:
- Proper class naming (e.g., `ClassName.Tests.cs`)
- XUnit test framework structure
- Mock-ready patterns
- TODO comments for implementation

### Build Error Correction

The script attempts automatic corrections including:
- NuGet package restoration (`dotnet restore`)
- Dependency updates
- Error logging for manual review

### Code Coverage Tracking

Captures code coverage metrics before and after modifications to measure:
- Impact of generated tests
- Coverage changes from mocking implementation
- Overall code quality improvements

## Error Handling

The script handles various error conditions gracefully:

- **Missing .NET SDK**: Exits with clear error message directing to installation
- **Missing solution files**: Logs warning and skips project
- **Build failures**: Logs detailed error messages to project-specific log file
- **Test execution failures**: Captures and reports all failure details
- **File encoding issues**: Skips problematic files with warning
- **Timeout conditions**: Gracefully handles long-running operations

## Architecture

### Class: TestOrchestrator

Main orchestrator class with methods for:
- **Logging setup** and configuration
- **Project discovery** in cloned_repos directory
- **Static method call detection** using regex patterns
- **Code coverage extraction** from test output
- **Unit test generation** for identified patterns
- **Solution building** and error capture
- **Test execution** and failure reporting
- **Metrics aggregation** and CSV export

### Key Methods

- `get_projects()`: List all projects in cloned_repos
- `find_static_method_calls()`: Detect static method patterns
- `generate_unit_tests()`: Create test files for detected patterns
- `build_solution()`: Build and capture errors
- `get_code_coverage()`: Extract coverage percentage
- `run_tests()`: Execute tests and capture failures
- `process_project()`: Execute full workflow for one project
- `save_metrics_to_csv()`: Export metrics to CSV

## Workflow Diagram

```
For each project:
  ├─ Step 1: Get Initial Code Coverage
  ├─ Step 2: Find Static Method Calls (DateTime.Now, DateTime.UtcNow, etc.)
  ├─ Step 3: Generate Unit Tests
  ├─ Step 4: Build Solution & Record Failures
  ├─ Step 5: Correct Build Errors
  ├─ Step 6: Run Tests & Record Failures
  ├─ Step 7: Get Final Code Coverage
  └─ Save metrics to test_metrics.csv
```

## Limitations and Future Enhancements

### Current Limitations

1. **Test Generation**: Currently generates skeleton tests; actual mocking logic requires manual implementation
2. **Coverage Parsing**: Basic regex-based parsing; may not work with all test frameworks
3. **Error Correction**: Limited to package restoration; doesn't fix code-level issues
4. **Project Discovery**: Processes all directories; may include non-C# projects

### Potential Enhancements

1. **Advanced Test Generation**: 
   - Implement Moq/NSubstitute mocking patterns
   - Generate meaningful test assertions
   - Parse source code for dependency injection

2. **Coverage Integration**:
   - Support for Coverlet
   - Integration with OpenCover
   - Multi-framework coverage reporting

3. **Intelligent Error Correction**:
   - Analyze error patterns
   - Apply targeted fixes
   - Suggest manual interventions

4. **Metrics Aggregation**:
   - Generate HTML reports
   - Compare project metrics
   - Trend analysis over time

## Troubleshooting

### Issue: "Command not found: dotnet"

**Solution**: Install .NET SDK
```bash
# Windows
choco install dotnetcore-sdk

# macOS
brew install dotnet-sdk

# Linux (Ubuntu/Debian)
sudo apt-get install dotnet-sdk-8.0
```

### Issue: Tests not found for projects

**Solution**: Script looks for projects with "test" in the `.csproj` filename. Ensure test projects follow this naming convention.

### Issue: No code coverage data

**Solution**: Ensure projects have test projects and depend on a code coverage tool like Coverlet. The script attempts basic parsing of coverage output.

### Issue: Build failures not showing

**Solution**: Check the project-specific build log file in `test_logs/` directory for detailed error messages.

## Performance Considerations

- First run processes all projects sequentially (can take significant time for large projects)
- Each project: ~5-60 minutes depending on project size and test count
- Results are saved incrementally after each project
- Logs are created for each project for parallel analysis

## License

This script is part of the mocking-static-methods project.

## Support

For issues or enhancements, please refer to the project README.md or check the detailed logs in the `test_logs/` directory.
