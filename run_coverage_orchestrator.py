#!/usr/bin/env python3
"""
Coverage Orchestrator - Executes all repository build/test/coverage commands
and generates a CSV report with coverage metrics.

Usage:
    python run_coverage_orchestrator.py                    # Run all repos
    python run_coverage_orchestrator.py orleans             # Run just Orleans
    python run_coverage_orchestrator.py abp efcore roslyn  # Run multiple specific repos
"""

import subprocess
import csv
import re
import sys
from datetime import datetime
from pathlib import Path
from bs4 import BeautifulSoup

# Define all repositories and their build commands
REPOSITORIES = [
    {
        "name": "abp",
        "description": "App Framework",
        "command": """
cd cloned_repos/abp && \
git checkout 10.0.2 && \
cd framework && \
dotnet restore && \
dotnet build && \
dotnet test --filter "FullyQualifiedName!~SkiaSharp" --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/abp/framework/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/abp/framework/CoverageReport/index.html",
    },
    {
        "name": "aspnetcore",
        "description": "Web Framework",
        "command": """
cd cloned_repos/aspnetcore && \
git checkout ecb199c29cbefb6fcb6aa789436de36e44427a78 && \
git submodule update --init --recursive && \
source ./activate.sh && \
dotnet test AspNetCore.slnx --filter "Category!=Integration&Category!=E2E&Category!=Quarantined" --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/aspnetcore/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/aspnetcore/CoverageReport/index.html",
    },
    {
        "name": "efcore",
        "description": "ORM",
        "command": """
cd cloned_repos/efcore && \
git checkout release/10.0 && \
source ./activate.sh && \
dotnet test EFCore.sln --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/efcore/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/efcore/CoverageReport/index.html",
    },
    {
        "name": "orleans",
        "description": "Distributed Actors",
        "command": """
cd cloned_repos/orleans && \
git checkout v10.0.0 && \
dotnet restore Orleans.slnx && \
dotnet test Orleans.slnx --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/orleans/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/orleans/CoverageReport/index.html",
    },
    {
        "name": "roslyn",
        "description": "Compiler",
        "command": """
cd cloned_repos/roslyn && \
git checkout release/dev18.3 && \
dotnet test Roslyn.sln --filter "FullyQualifiedName!~LanguageServer&TargetFrameworkIdentifier!=.NETFramework" --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/roslyn/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/roslyn/CoverageReport/index.html",
    },
    {
        "name": "runtime",
        "description": ".NET Runtime",
        "command": """
cd cloned_repos/runtime && \
git checkout v10.0.2 && \
./build.sh -subset libs+libs.tests -test && \
reportgenerator -reports:"./artifacts/TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/runtime/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/runtime/CoverageReport/index.html",
    },
    {
        "name": "semantic-kernel",
        "description": "AI SDK",
        "command": """
cd cloned_repos/semantic-kernel && \
git checkout dotnet-1.70.0 && \
cd dotnet && \
dotnet test SK-dotnet.slnx --filter 'FullyQualifiedName!~SemanticKernel.IntegrationTests' --collect:"XPlat Code Coverage" --results-directory ./TestResults && \
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html && \
echo "Coverage report: cloned_repos/semantic-kernel/dotnet/CoverageReport/index.html"
        """.strip(),
        "coverage_report": "cloned_repos/semantic-kernel/dotnet/CoverageReport/index.html",
    },
]


def run_command(repo_name, command, log_file):
    """Execute a command and tee output to a log file."""
    print(f"\n{'='*80}")
    print(f"Starting build/test/coverage for: {repo_name}")
    print(f"{'='*80}")
    
    with open(log_file, 'w') as log:
        log.write(f"Build/Test/Coverage Log for {repo_name}\n")
        log.write(f"Started: {datetime.now()}\n")
        log.write(f"{'='*80}\n\n")
        
        try:
            # Run command with bash shell
            process = subprocess.Popen(
                command,
                shell=True,
                executable='/bin/bash',
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True,
                bufsize=1
            )
            
            # Stream output to both console and log file
            for line in process.stdout:
                print(line, end='')
                log.write(line)
                log.flush()
            
            process.wait()
            
            log.write(f"\n{'='*80}\n")
            log.write(f"Finished: {datetime.now()}\n")
            log.write(f"Exit code: {process.returncode}\n")
            
            return process.returncode == 0
            
        except Exception as e:
            error_msg = f"Error executing command: {e}\n"
            print(error_msg)
            log.write(error_msg)
            return False


def parse_coverage_report(html_file):
    """Parse the ReportGenerator HTML index.html file to extract coverage metrics."""
    try:
        with open(html_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f, 'html.parser')
        
        # Extract metrics from the summary table
        metrics = {
            'assemblies': None,
            'classes': None,
            'files': None,
            'covered_lines': None,
            'uncovered_lines': None,
            'coverable_lines': None,
            'total_lines': None,
            'line_coverage': None,
            'covered_branches': None,
            'total_branches': None,
            'branch_coverage': None,
        }
        
        # Find all table rows in the summary section
        # ReportGenerator creates specific structure with class names
        
        # Try to find the summary table
        summary_divs = soup.find_all('div', class_='container')
        
        for div in summary_divs:
            # Look for the information section
            info_rows = div.find_all('tr')
            for row in info_rows:
                cells = row.find_all('td')
                if len(cells) >= 2:
                    label = cells[0].get_text(strip=True).lower()
                    value = cells[1].get_text(strip=True)
                    
                    if 'assemblies:' in label:
                        metrics['assemblies'] = value
                    elif 'classes:' in label:
                        metrics['classes'] = value
                    elif 'files:' in label:
                        metrics['files'] = value
        
        # Look for coverage percentages and numbers
        # These are typically in specific divs with coverage data
        coverage_divs = soup.find_all('div')
        for div in coverage_divs:
            text = div.get_text(strip=True)
            
            # Line coverage section
            if 'Covered lines:' in text:
                match = re.search(r'Covered lines:\s*(\d+)', text)
                if match:
                    metrics['covered_lines'] = match.group(1)
            if 'Uncovered lines:' in text:
                match = re.search(r'Uncovered lines:\s*(\d+)', text)
                if match:
                    metrics['uncovered_lines'] = match.group(1)
            if 'Coverable lines:' in text:
                match = re.search(r'Coverable lines:\s*(\d+)', text)
                if match:
                    metrics['coverable_lines'] = match.group(1)
            if 'Total lines:' in text:
                match = re.search(r'Total lines:\s*(\d+)', text)
                if match:
                    metrics['total_lines'] = match.group(1)
            if 'Line coverage:' in text:
                match = re.search(r'Line coverage:\s*([\d.]+)%', text)
                if match:
                    metrics['line_coverage'] = match.group(1)
            
            # Branch coverage section
            if 'Covered branches:' in text:
                match = re.search(r'Covered branches:\s*(\d+)', text)
                if match:
                    metrics['covered_branches'] = match.group(1)
            if 'Total branches:' in text:
                match = re.search(r'Total branches:\s*(\d+)', text)
                if match:
                    metrics['total_branches'] = match.group(1)
            if 'Branch coverage:' in text:
                match = re.search(r'Branch coverage:\s*([\d.]+)%', text)
                if match:
                    metrics['branch_coverage'] = match.group(1)
        
        return metrics
        
    except Exception as e:
        print(f"Error parsing coverage report {html_file}: {e}")
        return None


def main():
    """Main orchestrator function."""
    base_dir = Path(__file__).parent
    logs_dir = base_dir / "coverage_logs"
    logs_dir.mkdir(exist_ok=True)
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    csv_file = logs_dir / f"coverage_summary_{timestamp}.csv"
    
    # Filter repositories based on command-line arguments
    repos_to_run = REPOSITORIES
    if len(sys.argv) > 1:
        requested_repos = [arg.lower() for arg in sys.argv[1:]]
        repos_to_run = [r for r in REPOSITORIES if r['name'].lower() in requested_repos]
        
        if not repos_to_run:
            print(f"Error: No matching repositories found for: {', '.join(sys.argv[1:])}")
            print(f"\nAvailable repositories:")
            for repo in REPOSITORIES:
                print(f"  - {repo['name']}")
            sys.exit(1)
        
        print(f"\n{'='*80}")
        print(f"Running {len(repos_to_run)} of {len(REPOSITORIES)} repositories:")
        for repo in repos_to_run:
            print(f"  ✓ {repo['name']} ({repo['description']})")
        print(f"{'='*80}\n")
    
    # CSV headers
    csv_headers = [
        'date',
        'repo_name',
        'assemblies',
        'classes',
        'files',
        'covered_lines',
        'uncovered_lines',
        'coverable_lines',
        'total_lines',
        'line_coverage',
        'covered_branches',
        'total_branches',
        'branch_coverage',
        'build_status',
    ]
    
    results = []
    
    # Execute each repository's build/test/coverage command
    for repo in repos_to_run:
        repo_name = repo['name']
        log_file = logs_dir / f"{repo_name}_{timestamp}.log"
        
        print(f"\n\n{'#'*80}")
        print(f"# Processing: {repo['description']} ({repo_name})")
        print(f"# Log file: {log_file}")
        print(f"{'#'*80}\n")
        
        # Run the build/test/coverage command
        success = run_command(repo_name, repo['command'], log_file)
        
        # Parse the coverage report
        coverage_report_path = base_dir / repo['coverage_report']
        
        if coverage_report_path.exists():
            metrics = parse_coverage_report(coverage_report_path)
            if metrics:
                result = {
                    'date': datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                    'repo_name': repo_name,
                    'build_status': 'SUCCESS' if success else 'FAILED',
                }
                result.update(metrics)
                results.append(result)
            else:
                print(f"⚠️  Warning: Could not parse coverage report for {repo_name}")
                results.append({
                    'date': datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                    'repo_name': repo_name,
                    'build_status': 'SUCCESS' if success else 'FAILED',
                })
        else:
            print(f"⚠️  Warning: Coverage report not found for {repo_name} at {coverage_report_path}")
            results.append({
                'date': datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                'repo_name': repo_name,
                'build_status': 'FAILED',
            })
    
    # Write results to CSV
    print(f"\n\n{'='*80}")
    print(f"Writing coverage summary to: {csv_file}")
    print(f"{'='*80}\n")
    
    with open(csv_file, 'w', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=csv_headers)
        writer.writeheader()
        writer.writerows(results)
    
    # Print summary
    print("\n" + "="*80)
    print("COVERAGE ORCHESTRATOR SUMMARY")
    print("="*80)
    print(f"Total repositories processed: {len(results)}")
    print(f"Successful builds: {sum(1 for r in results if r.get('build_status') == 'SUCCESS')}")
    print(f"Failed builds: {sum(1 for r in results if r.get('build_status') == 'FAILED')}")
    print(f"\nCSV Report: {csv_file}")
    print(f"Log files: {logs_dir}/")
    print("="*80)
    
    # Print quick coverage summary table
    print("\nQuick Coverage Summary:")
    print("-" * 80)
    print(f"{'Repo':<20} {'Status':<10} {'Line Cov':<12} {'Branch Cov':<12}")
    print("-" * 80)
    for result in results:
        repo = result.get('repo_name', 'N/A')[:20]
        status = result.get('build_status', 'N/A')
        line_cov = result.get('line_coverage', 'N/A')
        if line_cov != 'N/A':
            line_cov = f"{line_cov}%"
        branch_cov = result.get('branch_coverage', 'N/A')
        if branch_cov != 'N/A':
            branch_cov = f"{branch_cov}%"
        print(f"{repo:<20} {status:<10} {line_cov:<12} {branch_cov:<12}")
    print("-" * 80)


if __name__ == "__main__":
    main()
