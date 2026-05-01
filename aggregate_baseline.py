#!/usr/bin/env python3
"""
Aggregate Phase 1 baseline coverage.

Inputs:
  - baseline_artifacts/<repo>/**/coverage.cobertura.xml (downloaded from CI)
  - cloned_repos/<repo>/  (target source for StaticCallAnalyzer)

Outputs:
  - baseline_coverage.csv
  - BASELINE_COVERAGE.md
  - baseline_artifacts/<repo>/static_call_classes.json   (per-class breakdown)

Run:
  python3 aggregate_baseline.py
"""

from __future__ import annotations

import csv
import json
import os
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent
ARTIFACTS_DIR = REPO_ROOT / "baseline_artifacts"
CLONED_DIR = REPO_ROOT / "cloned_repos"
ANALYZER_DLL = REPO_ROOT / "StaticCallAnalyzer" / "bin" / "Release" / "net8.0" / "StaticCallAnalyzer.dll"

REPOS = ["abp", "aspnetcore", "efcore", "orleans", "roslyn", "runtime", "semantic-kernel"]

# Pinned SHAs (mirrored from .squad/decisions.md and .github/workflows/coverage-orchestrator.yml)
PINNED_SHAS = {
    "abp": "ea4bbb8b517869a9fb735ea5bc05c819c209d0b5",
    "aspnetcore": "ecb199c29cbefb6fcb6aa789436de36e44427a78",
    "efcore": "45e3af0273b71919189367bc152a335b69f443c6",
    "orleans": "8024faf860549cb960b4b573c1571b379e283daa",
    "roslyn": "02d301627ed5016a4c18acd1a35e5bbc20ff03f0",
    "runtime": "9ffface2f3fa6fbbb427793c3230b1626a1fdd84",
    "semantic-kernel": "0c898161a355b0a845aea48de79cb43e2e9435d2",
}

RUN_ID = "25215078473"
REPORT_DATE = "2026-05-01"


def _to_int(val: str | None) -> int:
    if val is None or val == "":
        return 0
    try:
        return int(float(val))
    except ValueError:
        return 0


def parse_cobertura_repo(repo: str) -> dict:
    """Sum coverage stats across every cobertura XML for the repo."""
    repo_dir = ARTIFACTS_DIR / repo
    xml_files = sorted(repo_dir.rglob("coverage.cobertura.xml"))

    lines_total = 0
    lines_covered = 0
    branches_total = 0
    branches_covered = 0
    files_seen = 0
    has_branch_data = False

    for xml in xml_files:
        try:
            root = ET.parse(xml).getroot()
        except ET.ParseError as exc:
            print(f"  ! parse error on {xml}: {exc}", file=sys.stderr)
            continue
        files_seen += 1
        lc = _to_int(root.get("lines-covered"))
        lv = _to_int(root.get("lines-valid"))
        bc = _to_int(root.get("branches-covered"))
        bv = _to_int(root.get("branches-valid"))
        lines_covered += lc
        lines_total += lv
        branches_covered += bc
        branches_total += bv
        if root.get("branches-valid") is not None:
            has_branch_data = True

    line_pct = (lines_covered / lines_total * 100.0) if lines_total else 0.0
    branch_pct = (branches_covered / branches_total * 100.0) if branches_total else 0.0

    return {
        "files_seen": files_seen,
        "lines_total": lines_total,
        "lines_covered": lines_covered,
        "line_pct": line_pct,
        "branches_total": branches_total,
        "branches_covered": branches_covered,
        "branch_pct": branch_pct,
        "has_branch_data": has_branch_data,
    }


def run_static_analyzer(repo: str) -> list[dict]:
    """Invoke StaticCallAnalyzer against cloned_repos/<repo>/. Returns parsed JSON list."""
    repo_path = CLONED_DIR / repo
    if not repo_path.is_dir():
        print(f"  ! cloned_repos/{repo} not found, skipping analyzer", file=sys.stderr)
        return []

    # Run from a clean tmp cwd because analyzer appends to ./analysis_results.json
    with tempfile.TemporaryDirectory() as tmp:
        proc = subprocess.run(
            ["dotnet", str(ANALYZER_DLL), str(repo_path)],
            cwd=tmp,
            capture_output=True,
            text=True,
            env={**os.environ, "DOTNET_NOLOGO": "1"},
        )
    if proc.returncode != 0:
        print(f"  ! analyzer failed for {repo}: {proc.stderr[:500]}", file=sys.stderr)
        return []
    out = proc.stdout.strip()
    # Analyzer prints help message if args missing — guard against non-JSON.
    if not out or not out.startswith("["):
        return []
    try:
        return json.loads(out)
    except json.JSONDecodeError as exc:
        print(f"  ! JSON decode failed for {repo}: {exc}", file=sys.stderr)
        return []


def aggregate_static(repo: str, rows: list[dict]) -> dict:
    """
    Aggregate analyzer rows into per-repo totals + per-class JSON.

    Each row is per-(file, class, method, pattern).
    PatternCount = occurrences of THAT pattern in THAT method.
    Sum PatternCount across all rows for total static call sites.
    Distinct (file, class) pairs = classes with static calls.
    """
    total_sites = 0
    by_class: dict[tuple[str, str], int] = defaultdict(int)

    for r in rows:
        pc = int(r.get("PatternCount", 0) or 0)
        total_sites += pc
        key = (r.get("File", ""), r.get("Class", ""))
        by_class[key] += pc

    # Per-class JSON for Phase 2
    repo_path = CLONED_DIR / repo
    repo_str = str(repo_path) + os.sep
    per_class = []
    for (file, cls), count in sorted(by_class.items(), key=lambda x: -x[1]):
        rel = file
        if file.startswith(repo_str):
            rel = file[len(repo_str):]
        per_class.append({
            "class_name": cls,        # simple name; FQN unavailable from current analyzer
            "class_fqn": None,        # Phase 2 prereq: extend analyzer to emit FQN
            "file_path": rel,
            "static_call_count": count,
        })

    out_path = ARTIFACTS_DIR / repo / "static_call_classes.json"
    out_path.parent.mkdir(parents=True, exist_ok=True)
    with open(out_path, "w") as f:
        json.dump(per_class, f, indent=2)

    return {
        "static_call_sites": total_sites,
        "classes_with_static_calls": len(by_class),
        "per_class_json": str(out_path.relative_to(REPO_ROOT)),
    }


def fmt_pct(p: float) -> str:
    return f"{p:.2f}%"


def main() -> int:
    if not ANALYZER_DLL.exists():
        print(f"Analyzer not built: {ANALYZER_DLL}", file=sys.stderr)
        print("Run: dotnet build StaticCallAnalyzer/StaticCallAnalyzer.csproj -c Release", file=sys.stderr)
        return 1

    rows_for_csv = []
    rows_for_md = []
    totals = {
        "lines_total": 0, "lines_covered": 0,
        "branches_total": 0, "branches_covered": 0,
        "static_call_sites": 0, "classes_with_static_calls": 0,
    }
    notes = []

    for repo in REPOS:
        print(f"=== {repo} ===")
        cov = parse_cobertura_repo(repo)
        print(f"  cobertura files: {cov['files_seen']}  lines: {cov['lines_covered']}/{cov['lines_total']}  branches: {cov['branches_covered']}/{cov['branches_total']}")

        analyzer_rows = run_static_analyzer(repo)
        stat = aggregate_static(repo, analyzer_rows)
        print(f"  static call sites: {stat['static_call_sites']}  classes: {stat['classes_with_static_calls']}")

        if cov["files_seen"] == 0:
            notes.append(f"- **{repo}**: no cobertura XML found.")
        elif cov["lines_total"] == 0:
            notes.append(f"- **{repo}**: cobertura XML present but `lines-valid=0` — the run produced empty coverage data (no instrumented assemblies were exercised).")
        if cov["files_seen"] > 1:
            notes.append(f"- **{repo}**: coverage was emitted as {cov['files_seen']} separate cobertura files (one per test project / coverlet session). Totals are summed across all files; code shared across multiple test sessions may be double-counted.")
        if cov["files_seen"] > 0 and not cov["has_branch_data"]:
            notes.append(f"- **{repo}**: no branch data emitted by the collector.")

        row = {
            "Repo": repo,
            "Lines (total)": cov["lines_total"],
            "Lines (covered)": cov["lines_covered"],
            "Line coverage %": cov["line_pct"],
            "Branches (total)": cov["branches_total"],
            "Branches (covered)": cov["branches_covered"],
            "Branch coverage %": cov["branch_pct"],
            "Static call sites": stat["static_call_sites"],
            "Classes with static calls": stat["classes_with_static_calls"],
        }
        rows_for_csv.append(row)
        rows_for_md.append(row)

        for k in ("lines_total", "lines_covered", "branches_total", "branches_covered"):
            totals[k] += cov[k.replace("_", "_")]
        totals["static_call_sites"] += stat["static_call_sites"]
        totals["classes_with_static_calls"] += stat["classes_with_static_calls"]

    total_line_pct = (totals["lines_covered"] / totals["lines_total"] * 100.0) if totals["lines_total"] else 0.0
    total_branch_pct = (totals["branches_covered"] / totals["branches_total"] * 100.0) if totals["branches_total"] else 0.0

    total_row = {
        "Repo": "**TOTAL**",
        "Lines (total)": totals["lines_total"],
        "Lines (covered)": totals["lines_covered"],
        "Line coverage %": total_line_pct,
        "Branches (total)": totals["branches_total"],
        "Branches (covered)": totals["branches_covered"],
        "Branch coverage %": total_branch_pct,
        "Static call sites": totals["static_call_sites"],
        "Classes with static calls": totals["classes_with_static_calls"],
    }

    # CSV
    csv_path = REPO_ROOT / "baseline_coverage.csv"
    with open(csv_path, "w", newline="") as f:
        w = csv.DictWriter(f, fieldnames=list(rows_for_csv[0].keys()))
        w.writeheader()
        for r in rows_for_csv:
            rr = dict(r)
            rr["Line coverage %"] = f"{r['Line coverage %']:.4f}"
            rr["Branch coverage %"] = f"{r['Branch coverage %']:.4f}"
            w.writerow(rr)
        tr = dict(total_row)
        tr["Repo"] = "TOTAL"
        tr["Line coverage %"] = f"{total_row['Line coverage %']:.4f}"
        tr["Branch coverage %"] = f"{total_row['Branch coverage %']:.4f}"
        w.writerow(tr)
    print(f"\nWrote {csv_path}")

    # Markdown
    md = []
    md.append("# Phase 1 Coverage Baseline")
    md.append("")
    md.append(f"**Date:** {REPORT_DATE}  ")
    md.append(f"**CI run:** [{RUN_ID}](https://github.com/bradygaster/mocking-static-methods/actions/runs/{RUN_ID}) (workflow: `coverage-orchestrator.yml`, branch `jasper/squad`)  ")
    md.append(f"**Branch HEAD at run:** `188cb4a95c2162c6db6fdafe6aa3f04f485104aa`")
    md.append("")
    md.append("This is the pre-Phase 2 snapshot of test coverage and static-call surface area for the seven .NET OSS repos under study. Coverage data is cobertura XML produced by each repo's CI job (`actions/upload-artifact` → `coverage-xml-<repo>`). Static-call counts come from `StaticCallAnalyzer/` run locally against the pinned source tree of each repo.")
    md.append("")
    md.append("> ⚠️ **Headline finding:** four of seven repos (`abp`, `aspnetcore`, `efcore`, `roslyn`) uploaded 178-byte stub cobertura files with `<packages />` empty — the CI jobs reported success, but no instrumented assemblies were exercised. Only `orleans`, `runtime`, and `semantic-kernel` produced usable coverage data this run. The static-call analysis below is sound for all seven; the line/branch columns must be re-baselined for the four stub repos before Phase 2 can compare \"before\" and \"after\" coverage there.")
    md.append("")
    md.append("## Pinned target SHAs")
    md.append("")
    md.append("| Repo | SHA |")
    md.append("|------|-----|")
    for repo in REPOS:
        md.append(f"| {repo} | `{PINNED_SHAS[repo]}` |")
    md.append("")
    md.append("## Baseline Table")
    md.append("")
    md.append("| Repo | Lines (total) | Lines (covered) | Line coverage % | Branches (total) | Branches (covered) | Branch coverage % | Static call sites | Classes with static calls |")
    md.append("|------|---:|---:|---:|---:|---:|---:|---:|---:|")
    for r in rows_for_md:
        md.append(
            f"| {r['Repo']} "
            f"| {r['Lines (total)']:,} "
            f"| {r['Lines (covered)']:,} "
            f"| {fmt_pct(r['Line coverage %'])} "
            f"| {r['Branches (total)']:,} "
            f"| {r['Branches (covered)']:,} "
            f"| {fmt_pct(r['Branch coverage %'])} "
            f"| {r['Static call sites']:,} "
            f"| {r['Classes with static calls']:,} |"
        )
    md.append(
        f"| {total_row['Repo']} "
        f"| {total_row['Lines (total)']:,} "
        f"| {total_row['Lines (covered)']:,} "
        f"| {fmt_pct(total_row['Line coverage %'])} "
        f"| {total_row['Branches (total)']:,} "
        f"| {total_row['Branches (covered)']:,} "
        f"| {fmt_pct(total_row['Branch coverage %'])} "
        f"| {total_row['Static call sites']:,} "
        f"| {total_row['Classes with static calls']:,} |"
    )
    md.append("")
    md.append("Percentages on the TOTAL row are weighted by line/branch volume across all 7 repos.")
    md.append("")
    md.append("## Methodology")
    md.append("")
    md.append("- **Coverage:** parsed root `<coverage>` attributes (`lines-valid`, `lines-covered`, `branches-valid`, `branches-covered`) from each cobertura XML. For repos that emit one file per test session (multi-package coverlet runs), totals are summed across all files.")
    md.append("- **Static call sites:** sum of `PatternCount` across every row emitted by `StaticCallAnalyzer` (one row per `(file, class, method, pattern)` triple). The analyzer only counts calls inside methods with cyclomatic complexity > 2 and excludes paths matching `Tests`, `Samples`, or `Demo`. It tracks five patterns: `DateTime.Now`, `DateTime.UtcNow`, `File.Exists`, `Directory.Exists`, `Guid.NewGuid` (see `StaticCallAnalyzer/StaticCallConfig.cs`).")
    md.append("- **Classes with static calls:** distinct `(file, class)` pairs in the analyzer output.")
    md.append("- **Per-class breakdown:** see `baseline_artifacts/<repo>/static_call_classes.json` — list of `{class_name, class_fqn, file_path, static_call_count}` sorted by count descending.")
    md.append("")
    md.append("## Data quality notes")
    md.append("")
    if notes:
        md.extend(notes)
    else:
        md.append("- No anomalies detected.")
    md.append("")
    md.append("## Phase 2 readiness — gaps to close")
    md.append("")
    md.append("1. **StaticCallAnalyzer does NOT emit fully-qualified class names.** It records the simple `Identifier.Text` of the enclosing `ClassDeclarationSyntax` only. Phase 2 needs `Namespace.OuterClass.InnerClass` to join against cobertura's `<class name=\"...\">` entries. The `class_fqn` field in `static_call_classes.json` is currently `null` for every entry. **Action:** extend `StaticCallAnalyzer/Program.cs` to walk `NamespaceDeclarationSyntax` / `FileScopedNamespaceDeclarationSyntax` and parent `ClassDeclarationSyntax` ancestors when assembling the FQN. Owner: Watney.")
    md.append("2. **Per-class coverage extraction not yet implemented.** Cobertura `<class>` entries hold `line-rate` / `branch-rate`. Phase 2 needs a step that, for each class in `static_call_classes.json`, looks up its coverage in the matching cobertura file and emits a joined record `{repo, class_fqn, file_path, line_rate, branch_rate, static_call_count}`. Owner: Beck (next session).")
    md.append("3. **Four repos (`abp`, `aspnetcore`, `efcore`, `roslyn`) produced empty cobertura XML.** Each uploaded a 178-byte stub `<coverage line-rate=\"1\" ...><packages /></coverage>`. CI jobs reported success because tests passed and the report step had `continue-on-error: true`; the underlying issue is that no assemblies got instrumented. Likely causes by repo: (a) `abp`/`efcore`/`roslyn` use external `dotnet-coverage collect` — the wrapped `dotnet test` command may not be matching any test projects under the unit-only filter, or `dotnet-coverage` is writing to a different path than the one we upload; (b) `aspnetcore` uses `coverlet.collector` natively, but the test projects under `--all` likely don't reference the collector package — coverlet silently does nothing. **Action:** Vogel/Beck investigate per-repo before declaring any of these four a Phase 2 baseline. Until fixed, only `orleans` / `runtime` / `semantic-kernel` are usable Phase 2 starting points.")
    md.append("4. **Multi-file repos (orleans, semantic-kernel, ...) sum-double-count code shared between test sessions.** For Phase 2 class-level joins this isn't a problem — we'll merge per-class entries by FQN and take the union of covered lines. But the totals shown above are upper bounds, not de-duplicated unions.")
    md.append("5. **Analyzer pattern set is fixed at 5 patterns.** If Phase 2 wants broader coverage of static-method usage (e.g. `Path.Combine`, `Environment.*`, `Console.*`), `StaticCallConfig.Patterns` needs extending. This will inflate static-call counts and re-baseline values.")
    md.append("")
    md.append("## Reproducing")
    md.append("")
    md.append("```bash")
    md.append("# 1. Download artifacts from the run (90-day retention)")
    md.append("mkdir -p baseline_artifacts")
    md.append("for repo in abp aspnetcore efcore orleans roslyn runtime semantic-kernel; do")
    md.append(f"  gh run download {RUN_ID} -n coverage-xml-$repo -D baseline_artifacts/$repo/")
    md.append("done")
    md.append("")
    md.append("# 2. Build the analyzer (one-time)")
    md.append("dotnet build StaticCallAnalyzer/StaticCallAnalyzer.csproj -c Release")
    md.append("")
    md.append("# 3. Aggregate")
    md.append("python3 aggregate_baseline.py")
    md.append("```")
    md.append("")

    md_path = REPO_ROOT / "BASELINE_COVERAGE.md"
    with open(md_path, "w") as f:
        f.write("\n".join(md))
    print(f"Wrote {md_path}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
