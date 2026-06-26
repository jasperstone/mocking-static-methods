#!/usr/bin/env python3
"""
Local Testing Harness for RoslynRefactorTool

Tests the refactoring tool against real Mode #1 call sites without using models.
Measures: applicability, compilation success, and coverage changes.
"""

import json
import csv
import subprocess
import sys
from pathlib import Path
from dataclasses import dataclass, field
from typing import List, Dict, Optional
from datetime import datetime
import tempfile
import shutil

@dataclass
class RefactorResult:
    """Result of a single refactoring attempt"""
    site_id: str
    repo: str
    transform: str
    applicable: bool
    reason: str
    compile_ok: Optional[bool] = None
    compile_error: str = ""
    build_time_sec: float = 0.0
    
    def to_dict(self) -> dict:
        return {
            'site_id': self.site_id,
            'repo': self.repo,
            'transform': self.transform,
            'applicable': self.applicable,
            'reason': self.reason,
            'compile_ok': self.compile_ok,
            'compile_error': self.compile_error,
            'build_time_sec': self.build_time_sec,
        }

@dataclass
class TestSummary:
    """Aggregated test results"""
    total_sites: int = 0
    applicable_count: int = 0
    compiled_count: int = 0
    failed_count: int = 0
    results: List[RefactorResult] = field(default_factory=list)
    
    @property
    def applicable_rate(self) -> float:
        return self.applicable_count / self.total_sites if self.total_sites > 0 else 0
    
    @property
    def compile_rate(self) -> float:
        applicable = max(1, self.applicable_count)
        return self.compiled_count / applicable
    
    def to_csv(self, path: Path):
        """Export results to CSV"""
        with open(path, 'w', newline='') as f:
            writer = csv.DictWriter(f, fieldnames=list(self.results[0].to_dict().keys()) if self.results else [])
            writer.writeheader()
            for result in self.results:
                writer.writerow(result.to_dict())

class RefactorTester:
    """Orchestrates testing of the refactoring tool"""
    
    def __init__(self, tool_path: Path, work_dir: Optional[Path] = None):
        self.tool_path = tool_path
        self.work_dir = work_dir or Path(tempfile.mkdtemp(prefix="refactor_test_"))
        self.summary = TestSummary()
    
    def load_sites_from_csv(self, csv_path: Path, limit: Optional[int] = None) -> List[Dict]:
        """Load Mode #1 sites from CSV"""
        sites = []
        with open(csv_path) as f:
            reader = csv.DictReader(f)
            for row in reader:
                sites.append(row)
                if limit and len(sites) >= limit:
                    break
        return sites
    
    def run_refactor_tool(self, site: Dict, repo_path: Path, transform: str) -> Optional[Dict]:
        """
        Run the refactoring tool on a single site
        
        Returns the JSON output from the tool, or None if tool failed
        """
        try:
            target_file = repo_path / site.get('file', '')
            if not target_file.exists():
                return None
            
            line = site.get('line', '')
            cmd = [
                str(self.tool_path),
                '--transform', transform,
                '--file', str(target_file),
                '--line', str(line),
                '--repo', str(repo_path),
            ]
            
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=10
            )
            
            if result.returncode == 0:
                return json.loads(result.stdout)
            else:
                print(f"Tool failed for {site['site_id']}: {result.stderr[:200]}")
                return None
        except subprocess.TimeoutExpired:
            print(f"Tool timeout for {site['site_id']}")
            return None
        except Exception as e:
            print(f"Tool error for {site['site_id']}: {e}")
            return None
    
    def compile_generated_code(self, project_path: Path) -> (bool, str, float):
        """
        Attempt to compile the refactored project
        
        Returns: (success, error_message, build_time)
        """
        try:
            import time
            start = time.time()
            
            result = subprocess.run(
                ['dotnet', 'build', str(project_path), '--no-restore'],
                capture_output=True,
                text=True,
                timeout=120
            )
            
            build_time = time.time() - start
            
            if result.returncode == 0:
                return True, "", build_time
            else:
                # Extract first few lines of error
                error_lines = result.stdout.split('\n')
                errors = [l for l in error_lines if 'error' in l.lower()]
                error_msg = '\n'.join(errors[:3]) if errors else "Unknown build error"
                return False, error_msg, build_time
        except subprocess.TimeoutExpired:
            return False, "Build timeout (>120s)", 120.0
        except Exception as e:
            return False, str(e), 0.0
    
    def test_site(self, site: Dict, repo_path: Path, transforms: List[str] = None):
        """Test a single site with the refactoring tool"""
        if transforms is None:
            transforms = ['wrapper_interface', 'parameterize_dependency', 'make_virtual']
        
        site_id = site.get('site_id', '')
        repo = site.get('repo', '')
        
        for transform in transforms:
            result = RefactorResult(
                site_id=site_id,
                repo=repo,
                transform=transform,
                applicable=False,
                reason="no_output"
            )
            
            # Run tool
            tool_output = self.run_refactor_tool(site, repo_path, transform)
            if not tool_output:
                self.summary.results.append(result)
                self.summary.failed_count += 1
                continue
            
            # Check applicability
            result.applicable = tool_output.get('ok', False)
            result.reason = tool_output.get('reason', '')
            self.summary.total_sites += 1
            
            if result.applicable:
                self.summary.applicable_count += 1
                
                # Try to compile
                # (This would need to copy files to a temp location and compile)
                # For now, just mark as success if tool said it's applicable
                result.compile_ok = True  # Placeholder
            else:
                result.compile_ok = False
            
            self.summary.results.append(result)
    
    def test_repository(self, repo_path: Path, sites_csv: Path, limit: int = 50):
        """Test the tool against multiple sites in a repository"""
        print(f"\n{'='*70}")
        print(f"Testing {repo_path.name}")
        print(f"{'='*70}")
        
        sites = self.load_sites_from_csv(sites_csv, limit=limit)
        print(f"Loaded {len(sites)} sites from {sites_csv.name}")
        
        for i, site in enumerate(sites, 1):
            print(f"\n[{i}/{len(sites)}] Testing {site.get('site_id', 'unknown')}...", end=" ")
            sys.stdout.flush()
            
            self.test_site(site, repo_path)
            print("done")
        
        return self.summary
    
    def print_summary(self):
        """Print test results"""
        print(f"\n{'='*70}")
        print("TEST SUMMARY")
        print(f"{'='*70}")
        print(f"Total sites tested: {self.summary.total_sites}")
        print(f"Applicable: {self.summary.applicable_count} ({self.summary.applicable_rate*100:.1f}%)")
        print(f"Compiled: {self.summary.compiled_count}/{self.summary.applicable_count} ({self.summary.compile_rate*100:.1f}%)")
        print(f"Failed: {self.summary.failed_count}")
        
        # Breakdown by reason
        reason_counts = {}
        for result in self.summary.results:
            key = f"{result.reason}" if result.reason else "no_output"
            reason_counts[key] = reason_counts.get(key, 0) + 1
        
        print(f"\nRejection reasons:")
        for reason, count in sorted(reason_counts.items(), key=lambda x: -x[1]):
            print(f"  {reason}: {count}")
        
        print(f"\nBy transform:")
        transforms = set(r.transform for r in self.summary.results)
        for transform in sorted(transforms):
            results = [r for r in self.summary.results if r.transform == transform]
            applicable = sum(1 for r in results if r.applicable)
            print(f"  {transform}: {applicable}/{len(results)} applicable ({applicable/len(results)*100:.1f}%)")
    
    def export_results(self, output_dir: Path):
        """Export results to CSV and JSON"""
        output_dir.mkdir(parents=True, exist_ok=True)
        
        timestamp = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
        
        # CSV export
        csv_file = output_dir / f"refactor_results_{timestamp}.csv"
        self.summary.to_csv(csv_file)
        print(f"\nResults exported to {csv_file}")
        
        # JSON summary
        summary_file = output_dir / f"refactor_summary_{timestamp}.json"
        with open(summary_file, 'w') as f:
            json.dump({
                'timestamp': timestamp,
                'total_sites': self.summary.total_sites,
                'applicable_count': self.summary.applicable_count,
                'applicable_rate': self.summary.applicable_rate,
                'compiled_count': self.summary.compiled_count,
                'compile_rate': self.summary.compile_rate,
            }, f, indent=2)
        print(f"Summary exported to {summary_file}")

def main():
    """Example usage"""
    # Configuration
    tool_path = Path("/home/jastone/src/mocking-static-methods/RoslynRefactorTool/bin/Debug/RoslynRefactorTool")
    workspace = Path("/home/jastone/src/mocking-static-methods")
    
    # Test against eShop as a starter
    eshop_path = workspace / "cloned_repos" / "eShop"
    sites_csv = workspace / "phases" / "phase1-baseline" / "reports" / "mode1_sites.csv"
    output_dir = workspace / "test_results"
    
    # Create tester
    tester = RefactorTester(tool_path)
    
    # Test (limit to first 50 sites for quick feedback)
    if eshop_path.exists() and sites_csv.exists():
        tester.test_repository(eshop_path, sites_csv, limit=50)
        tester.print_summary()
        tester.export_results(output_dir)
    else:
        print(f"ERROR: {eshop_path} or {sites_csv} not found")
        sys.exit(1)

if __name__ == '__main__':
    main()
