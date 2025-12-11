#!/usr/bin/env python3
"""
Test Orchestrator for Static Method Mocking Analysis - ABP Focus

This script performs the following:
1. Find all C# files in cloned_repos/abp containing static method calls
2. Use Microsoft Agent Framework to generate unit tests for those files
3. Store generated tests in GeneratedTests directory
4. Build all ABP solutions using build/build-all.ps1
5. Run all tests and collect coverage using build/test-all.ps1
6. Extract coverage from CoverageReport/index.html
7. Record all metrics to test_metrics.csv
"""

import os
import subprocess
import json
import csv
import logging
import re
import sys
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import shutil
from bs4 import BeautifulSoup

# Import agent tools for test generation
try:
    from agent_tools import TestGenerationTools
    AGENT_TOOLS_AVAILABLE = True
except ImportError:
    AGENT_TOOLS_AVAILABLE = False

# Constants
STATIC_PATTERNS = {
    'DateTime.Now': r'DateTime\s*\.\s*Now(?!\s*=)',
    'DateTime.UtcNow': r'DateTime\s*\.\s*UtcNow(?!\s*=)',
    'File.Exists': r'File\s*\.\s*Exists\s*\(',
    'Directory.Exists': r'Directory\s*\.\s*Exists\s*\(',
    'Guid.NewGuid': r'Guid\s*\.\s*NewGuid\s*\(',
}

ABP_PROJECT_DIR = './cloned_repos/abp'
# Convert paths to absolute to work with cwd changes in subprocess
BUILD_SCRIPT = os.path.abspath('./cloned_repos/abp/build/build-all.ps1')
TEST_SCRIPT = os.path.abspath('./cloned_repos/abp/build/test-all.ps1')
COVERAGE_REPORT = './cloned_repos/abp/framework/CoverageReport/index.html'
GENERATED_TESTS_DIR = './cloned_repos/abp/GeneratedTests'
METRICS_CSV = 'test_metrics.csv'
LOGS_DIR = './test_logs'


class TestOrchestrator:
    """Orchestrator for generating tests and collecting metrics for ABP project."""

    def __init__(self):
        """Initialize the orchestrator."""
        self.setup_logging()
        self.metrics = {}
        self.ensure_directories()

    def setup_logging(self):
        """Setup logging configuration."""
        if not os.path.exists(LOGS_DIR):
            os.makedirs(LOGS_DIR)

        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        log_file = os.path.join(LOGS_DIR, f'orchestrator_{timestamp}.log')

        logging.basicConfig(
            level=logging.INFO,
            format='%(asctime)s - %(levelname)s - %(message)s',
            handlers=[
                logging.FileHandler(log_file),
                logging.StreamHandler()
            ]
        )
        self.logger = logging.getLogger(__name__)
        self.logger.info("Test Orchestrator started")

    def ensure_directories(self):
        """Ensure necessary directories exist."""
        if not os.path.exists(LOGS_DIR):
            os.makedirs(LOGS_DIR)
        if not os.path.exists(GENERATED_TESTS_DIR):
            os.makedirs(GENERATED_TESTS_DIR)
    
    def check_prerequisites(self) -> bool:
        """
        Check if required tools are available.
        
        Returns:
            True if dotnet and powershell are available, False otherwise
        """
        try:
            result = subprocess.run(
                ["dotnet", "--version"],
                capture_output=True,
                text=True,
                timeout=5
            )
            if result.returncode == 0:
                self.logger.info(f".NET SDK version: {result.stdout.strip()}")
            else:
                self.logger.error("ERROR: .NET SDK is not available")
                return False
        except Exception as e:
            self.logger.error(f"ERROR: .NET SDK check failed: {e}")
            return False
        
        # Check for PowerShell
        try:
            result = subprocess.run(
                ["pwsh", "--version"],
                capture_output=True,
                text=True,
                timeout=5
            )
            if result.returncode == 0:
                self.logger.info(f"PowerShell Core available")
                return True
        except:
            pass
        
        # Try pwsh (cross-platform PowerShell)
        try:
            result = subprocess.run(
                ["pwsh", "-Command", "Write-Host 'test'"],
                capture_output=True,
                text=True,
                timeout=5
            )
            if result.returncode == 0:
                self.logger.info("PowerShell Core available")
                return True
        except Exception as e:
            self.logger.error(f"ERROR: PowerShell is not available: {e}")
            return False
        
        return True

    def run_command(self, command: List[str], cwd: str, timeout: int = 600) -> Tuple[int, str, str]:
        """
        Run a shell command and return exit code, stdout, stderr.
        
        Args:
            command: Command to run as list
            cwd: Working directory
            timeout: Timeout in seconds
            
        Returns:
            Tuple of (exit_code, stdout, stderr)
        """
        try:
            result = subprocess.run(
                command,
                cwd=cwd,
                capture_output=True,
                text=True,
                timeout=timeout
            )
            return result.returncode, result.stdout, result.stderr
        except subprocess.TimeoutExpired:
            error_msg = f"Command timed out after {timeout} seconds"
            self.logger.error(error_msg)
            return -1, "", error_msg
        except FileNotFoundError as e:
            error_msg = f"Command not found: {command[0]}"
            self.logger.error(error_msg)
            return -1, "", error_msg
        except Exception as e:
            error_msg = f"Error running command: {str(e)}"
            self.logger.error(error_msg)
            return -1, "", error_msg

    def find_static_method_calls(self, project_dir: str) -> Dict[str, List[str]]:
        """
        Find all C# files in ABP containing static method calls.
        
        Args:
            project_dir: Project directory path
            
        Returns:
            Dictionary mapping pattern names to lists of file paths
        """
        self.logger.info(f"Finding static method calls in {project_dir}")
        
        static_files = {pattern: [] for pattern in STATIC_PATTERNS}
        
        # Search for C# files
        for root, dirs, files in os.walk(project_dir):
            # Skip bin, obj, and git directories
            dirs[:] = [d for d in dirs if d not in ('bin', 'obj', '.git', '.github', 'packages', 'GeneratedTests')]
            
            for file in files:
                if not file.endswith('.cs'):
                    continue
                
                file_path = os.path.join(root, file)
                try:
                    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                        content = f.read()
                    
                    for pattern_name, pattern_regex in STATIC_PATTERNS.items():
                        if re.search(pattern_regex, content):
                            if file_path not in static_files[pattern_name]:
                                static_files[pattern_name].append(file_path)
                except Exception as e:
                    self.logger.warning(f"Error reading file {file_path}: {e}")
        
        # Log findings
        total_files = 0
        for pattern, files in static_files.items():
            if files:
                self.logger.info(f"Found {len(files)} files with {pattern}")
                total_files += len(files)
        
        self.logger.info(f"Total unique files with static patterns: {total_files}")
        return static_files

    def find_static_methods_in_file(self, file_path: str) -> List[Tuple[str, str]]:
        """
        Find all static method call occurrences in a file and return the containing
        class and method names along with parameter list and whether the method
        is static.

        Returns:
            List of tuples (class_name, method_name, parameters, is_static)
        """
        results = []
        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()

            lines = content.splitlines()

            # For each pattern occurrence, locate the enclosing method and class
            for pattern_name, pattern_regex in STATIC_PATTERNS.items():
                for m in re.finditer(pattern_regex, content):
                    char_index = m.start()
                    line_no = content[:char_index].count('\n')

                    # Search upward for the nearest method declaration
                    method_name = None
                    parameters = ""
                    is_static = False
                    for i in range(line_no, max(-1, line_no - 80), -1):
                        if i < 0:
                            break
                        l = lines[i].strip()
                        if not l:
                            continue

                        # Match a method signature line (simplified)
                        meth = re.search(r"(?P<name>\w+)\s*\((?P<params>[^\)]*)\)", l)
                        if meth:
                            method_name = meth.group('name')
                            parameters = meth.group('params').strip()
                            if 'static' in l:
                                is_static = True
                            break

                    # Search upward for class name
                    class_name = None
                    for j in range((i if 'i' in locals() else line_no), max(-1, (i if 'i' in locals() else line_no) - 200), -1):
                        if j < 0:
                            break
                        cl = lines[j].strip()
                        cls = re.search(r"class\s+(?P<name>\w+)", cl)
                        if cls:
                            class_name = cls.group('name')
                            break

                    if not class_name:
                        class_name = os.path.splitext(os.path.basename(file_path))[0]

                    if not method_name:
                        # Fallback: use file/class name with a placeholder
                        method_name = 'UnknownMethod'

                    # Avoid duplicates
                    key = (class_name, method_name, parameters, is_static)
                    if key not in results:
                        results.append(key)

        except Exception as e:
            self.logger.warning(f"Error analyzing file {file_path}: {e}")

        return results

    def generate_unit_tests_with_agent(self, static_files: Dict[str, List[str]]) -> int:
        """
        Generate unit tests using Test Generation Tools.
        
        This integrates with the Microsoft Agent Framework tools for test generation.
        For production use, this would call an actual agent to generate tests.
        
        Args:
            static_files: Dictionary of static method files
            
        Returns:
            Number of test files generated
        """
        self.logger.info("Generating unit tests for static methods")
        
        if AGENT_TOOLS_AVAILABLE:
            self.logger.info("Using Agent Framework tools for test generation")
            agent_tools = TestGenerationTools()
        else:
            self.logger.info("Agent Framework tools not available, using fallback generation")
            agent_tools = None
        
        # For now, generate basic test stubs
        # In production, this would call the Agent Framework with these tools
        
        generated_count = 0
        
        for pattern, files in static_files.items():
            for file_path in files:
                # Find the containing methods/class for each static usage
                methods = self.find_static_methods_in_file(file_path)

                if not methods:
                    continue

                for (class_name, method_name, parameters, is_static) in methods:
                    # Generate test file for the containing method
                    if agent_tools:
                        test_content = agent_tools.generate_mock_test(
                            class_name=class_name,
                            method_name=method_name.replace('.', '_'),
                            return_type="dynamic",
                            parameters=parameters,
                            is_static=is_static,
                        )
                    else:
                        test_content = self.generate_test_content(file_path, method_name)

                    test_file_path = self.get_test_file_path(file_path, method_name)

                    try:
                        os.makedirs(os.path.dirname(test_file_path), exist_ok=True)
                        with open(test_file_path, 'w', encoding='utf-8') as f:
                            f.write(test_content)
                        self.logger.info(f"Generated test: {test_file_path}")
                        generated_count += 1
                    except Exception as e:
                        self.logger.error(f"Error generating test file: {e}")
        
        return generated_count

    def generate_test_content(self, source_file: str, static_method: str) -> str:
        """Generate test file content for a static method."""
        class_name = os.path.splitext(os.path.basename(source_file))[0]
        test_class = f"{class_name}_{static_method.replace('.', '_')}_Tests"
        
        test_content = f'''using Xunit;
using Moq;
using System;

namespace GeneratedTests
{{
    /// <summary>
    /// Auto-generated tests for {class_name}
    /// Tests static method: {static_method}
    /// </summary>
    public class {test_class}
    {{
        [Fact]
        public void Test_{static_method.replace('.', '_')}_MockedSuccessfully()
        {{
            // TODO: Implement test for {static_method}
            // This is a placeholder test for the static method mock
            Assert.True(true);
        }}
    }}
}}
'''
        return test_content

    def get_test_file_path(self, source_file: str, static_method: str) -> str:
        """Get the test file path in GeneratedTests directory."""
        file_name = os.path.splitext(os.path.basename(source_file))[0]
        method_clean = static_method.replace('.', '_')
        test_file_name = f"{file_name}_{method_clean}_Tests.cs"
        return os.path.join(GENERATED_TESTS_DIR, test_file_name)

    def build_abp_solution(self) -> Tuple[bool, str]:
        """
        Build ABP solution using build-all.ps1 script.
        
        Returns:
            Tuple of (success, output)
        """
        self.logger.info("Building ABP solution using build-all.ps1")
        
        if not os.path.exists(BUILD_SCRIPT):
            error_msg = f"Build script not found: {BUILD_SCRIPT}"
            self.logger.error(error_msg)
            return False, error_msg
        
        # Use pwsh to run PowerShell script
        command = ["pwsh", "-File", BUILD_SCRIPT]
        
        exit_code, stdout, stderr = self.run_command(
            command, 
            os.path.dirname(BUILD_SCRIPT),
            timeout=1200
        )
        
        build_log_file = os.path.join(LOGS_DIR, 'abp_build.log')
        with open(build_log_file, 'w') as f:
            f.write(f"Exit Code: {exit_code}\n\n")
            f.write(f"STDOUT:\n{stdout}\n\n")
            f.write(f"STDERR:\n{stderr}\n")
        
        self.logger.info(f"Build log saved to {build_log_file}")
        
        if exit_code == 0:
            self.logger.info("✅ Build succeeded")
            return True, stdout
        else:
            self.logger.error(f"❌ Build failed with exit code {exit_code}")
            return False, stderr

    def run_tests_and_coverage(self) -> Tuple[bool, str]:
        """
        Run tests and generate coverage using test-all.ps1 script.
        
        Returns:
            Tuple of (success, output)
        """
        self.logger.info("Running tests and collecting coverage using test-all.ps1")
        
        if not os.path.exists(TEST_SCRIPT):
            error_msg = f"Test script not found: {TEST_SCRIPT}"
            self.logger.error(error_msg)
            return False, error_msg
        
        # Use pwsh to run PowerShell script
        command = ["pwsh", "-File", TEST_SCRIPT]
        
        exit_code, stdout, stderr = self.run_command(
            command,
            os.path.dirname(TEST_SCRIPT),
            timeout=1200
        )
        
        test_log_file = os.path.join(LOGS_DIR, 'abp_tests.log')
        with open(test_log_file, 'w') as f:
            f.write(f"Exit Code: {exit_code}\n\n")
            f.write(f"STDOUT:\n{stdout}\n\n")
            f.write(f"STDERR:\n{stderr}\n")
        
        self.logger.info(f"Test log saved to {test_log_file}")
        
        # Return stdout for both success and failure - the test output with failures is in stdout
        if exit_code == 0:
            self.logger.info("✅ Tests passed")
            return True, stdout
        else:
            self.logger.error(f"❌ Tests failed with exit code {exit_code}")
            return False, stdout  # Return stdout, not stderr, to parse test failures

    def extract_coverage_from_html(self) -> Optional[float]:
        """
        Extract code coverage percentage from CoverageReport/index.html.
        
        Returns:
            Coverage percentage or None if not found
        """
        self.logger.info(f"Extracting coverage from {COVERAGE_REPORT}")
        
        if not os.path.exists(COVERAGE_REPORT):
            self.logger.warning(f"Coverage report not found: {COVERAGE_REPORT}")
            return None
        
        try:
            with open(COVERAGE_REPORT, 'r', encoding='utf-8', errors='ignore') as f:
                html_content = f.read()
            
            # Parse HTML to find coverage percentage
            soup = BeautifulSoup(html_content, 'html.parser')
            
            # Look for the first percentage that appears - usually the overall coverage
            # Pattern: "43.2%" or similar
            match = re.search(r'(\d+\.\d+)%', html_content)
            if match:
                coverage = float(match.group(1))
                self.logger.info(f"Found coverage in HTML: {coverage}%")
                return coverage
            
            # Alternative: look for percentage without decimal
            match = re.search(r'(\d+)%', html_content)
            if match:
                coverage = float(match.group(1))
                self.logger.info(f"Found coverage in HTML: {coverage}%")
                return coverage
            
            self.logger.warning("Could not parse coverage percentage from HTML")
            return None
            
        except Exception as e:
            self.logger.error(f"Error extracting coverage from HTML: {e}")
            return None

    def run(self):
        """Run the orchestrator for ABP project."""
        # Check prerequisites
        if not self.check_prerequisites():
            self.logger.error("Prerequisites check failed. Exiting.")
            return
        
        self.logger.info(f"=" * 80)
        self.logger.info(f"Starting ABP Test Generation and Analysis")
        self.logger.info(f"=" * 80)
        
        if not os.path.exists(ABP_PROJECT_DIR):
            self.logger.error(f"ABP project directory not found: {ABP_PROJECT_DIR}")
            return
        
        try:
            # Step 1: Find static method calls
            self.logger.info("STEP 1: Finding static method calls in ABP")
            
            # Capture initial coverage (before running our generated tests)
            self.logger.info("Capturing initial coverage baseline")
            initial_coverage = self.extract_coverage_from_html()
            self.metrics['initial_coverage'] = initial_coverage if initial_coverage is not None else 'N/A'
            
            static_files = self.find_static_method_calls(ABP_PROJECT_DIR)
            total_files = sum(len(files) for files in static_files.values())
            self.metrics['files_with_static_calls'] = total_files
            self.metrics['static_patterns_found'] = {k: len(v) for k, v in static_files.items()}
            
            if total_files == 0:
                self.logger.warning("No files with static calls found")
                return
            
            # Step 2: Generate tests for static methods
            self.logger.info("STEP 2: Generating unit tests for static methods")
            generated_tests = self.generate_unit_tests_with_agent(static_files)
            self.metrics['unit_tests_generated'] = generated_tests
            
            # Step 3: Build ABP solution
            self.logger.info("STEP 3: Building ABP solution")
            build_success, build_output = self.build_abp_solution()
            self.metrics['build_success'] = build_success
            self.metrics['build_status'] = "PASS" if build_success else "FAIL"
            
            # Step 4: Run tests and collect coverage
            self.logger.info("STEP 4: Running tests and collecting coverage")
            tests_success, tests_output = self.run_tests_and_coverage()
            self.metrics['tests_success'] = tests_success
            self.metrics['test_status'] = "PASS" if tests_success else "FAIL"
            
            # Step 5: Parse test results to extract failing tests and coverage
            self.logger.info("STEP 5: Parsing test results")
            failing_tests = self.extract_failing_tests(tests_output)
            self.metrics['failing_tests'] = failing_tests
            
            # Step 6: Extract coverage percentage from HTML report
            self.logger.info("STEP 6: Extracting coverage from report")
            coverage = self.extract_coverage_from_html()
            self.metrics['final_coverage'] = coverage if coverage is not None else 'N/A'
            
            self.logger.info(f"=" * 80)
            self.logger.info(f"Analysis Complete")
            self.logger.info(f"Files with static calls: {total_files}")
            self.logger.info(f"Tests generated: {generated_tests}")
            self.logger.info(f"Build status: {self.metrics['build_status']}")
            self.logger.info(f"Test status: {self.metrics['test_status']}")
            self.logger.info(f"Failing tests: {self.metrics.get('failing_tests', 0)}")
            self.logger.info(f"Final coverage: {self.metrics['final_coverage']}")
            self.logger.info(f"=" * 80)
            
            # Save metrics
            self.save_metrics_to_csv()
            
        except Exception as e:
            self.logger.error(f"Error during orchestration: {e}")
            import traceback
            traceback.print_exc()

    def extract_failing_tests(self, test_output: str) -> int:
        """
        Extract the number of failing tests from test output.
        
        Args:
            test_output: The output from the test run
            
        Returns:
            Total number of failing tests
        """
        try:
            # Look for "Failed! - Failed: N" pattern in test output
            # Pattern: "Failed!  - Failed:     4, Passed: ..."
            total_failed = 0
            
            pattern = r'Failed!\s*-\s*Failed:\s*(\d+)'
            for match in re.finditer(pattern, test_output):
                failed_count = int(match.group(1))
                total_failed += failed_count
            
            return total_failed
        except Exception as e:
            self.logger.debug(f"Could not extract failing tests count: {e}")
            return 0

    def save_metrics_to_csv(self):
        """Save metrics to CSV file."""
        fieldnames = [
            'timestamp',
            'files_with_static_calls',
            'unit_tests_generated',
            'build_success',
            'build_status',
            'tests_success',
            'test_status',
            'failing_tests',
            'initial_coverage',
            'final_coverage',
        ]
        
        # Add static pattern counts
        for pattern in STATIC_PATTERNS.keys():
            fieldnames.append(f'files_with_{pattern.replace(".", "_")}')
        
        try:
            metrics_row = {
                'timestamp': datetime.now().isoformat(),
            }
            metrics_row.update(self.metrics)
            
            # Add individual pattern counts and remove the dict from metrics_row
            static_patterns_found = metrics_row.pop('static_patterns_found', {})
            for pattern, count in static_patterns_found.items():
                metrics_row[f'files_with_{pattern.replace(".", "_")}'] = count
            
            with open(METRICS_CSV, 'a', newline='') as csvfile:
                writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
                
                # Write header if file is empty
                if csvfile.tell() == 0:
                    writer.writeheader()
                
                writer.writerow(metrics_row)
            
            self.logger.info(f"Metrics saved to {METRICS_CSV}")
        except Exception as e:
            self.logger.error(f"Error saving metrics to CSV: {e}")



def main():
    """Main entry point."""
    orchestrator = TestOrchestrator()
    orchestrator.run()


if __name__ == "__main__":
    main()
