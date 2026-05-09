#!/usr/bin/env python3
"""Cross-reference Mode #1 sites against cobertura coverage XML.

For each Mode #1 site (file, line), determine whether the line is
covered by the test suite. Produces a CSV that joins both signals.

Inputs:
  - Mode1Analyzer/results/mode1_sites.csv  (from Mode1Analyzer)
  - /tmp/cov_25495265941/{repo}/**/coverage.cobertura.xml  (from CI run)

Output:
  - tools/coverage_xref/mode1_coverage.csv
  - tools/coverage_xref/MODE1_COVERAGE_SUMMARY.md
"""
from __future__ import annotations
import csv
import glob
import os
import xml.etree.ElementTree as ET
from collections import defaultdict, Counter
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
SITES_CSV = REPO_ROOT / "Mode1Analyzer" / "results" / "mode1_sites.csv"
COVERAGE_BASE = Path("/tmp/cov_25495265941")
OUT_CSV = REPO_ROOT / "tools" / "coverage_xref" / "mode1_coverage.csv"
OUT_MD = REPO_ROOT / "tools" / "coverage_xref" / "MODE1_COVERAGE_SUMMARY.md"

# Repos that have coverage data on disk
COVERED_REPOS = {"abp", "aspnetcore", "efcore", "orleans", "roslyn", "runtime", "semantic-kernel"}


def load_coverage(repo: str) -> dict[str, dict[int, int]]:
    """Return {filename_lower: {line_number: hits}} for the repo.

    Cobertura's <class filename="..."> is the path the test runner saw,
    typically relative to the repo's source root. Multiple cobertura
    files may exist; we union them.
    """
    coverage: dict[str, dict[int, int]] = defaultdict(dict)
    pattern = COVERAGE_BASE / repo
    files = list(pattern.rglob("coverage.cobertura.xml"))
    if not files:
        return {}
    for f in files:
        try:
            root = ET.parse(f).getroot()
        except ET.ParseError:
            continue
        for cls in root.iter("class"):
            fname = cls.get("filename", "")
            if not fname:
                continue
            # Normalize: strip leading slashes, lowercase, '/' only.
            key = fname.replace("\\", "/").lstrip("/").lower()
            for line in cls.iter("line"):
                num = int(line.get("number", "0"))
                hits = int(line.get("hits", "0"))
                if num <= 0:
                    continue
                cur = coverage[key].get(num, 0)
                coverage[key][num] = max(cur, hits)
    return coverage


def normalize_site_path(site_file: str) -> str:
    """Normalize a Mode1Analyzer 'file' field for matching against cobertura."""
    return site_file.replace("\\", "/").lstrip("/").lower()


def find_coverage(coverage: dict[str, dict[int, int]], site_file: str, line: int) -> tuple[str, int | None]:
    """Try several path-suffix matches. Return (status, hits_or_None).

    status ∈ {'covered', 'uncovered', 'unknown_file', 'unknown_line'}
    """
    site_norm = normalize_site_path(site_file)
    # Direct file match
    if site_norm in coverage:
        line_map = coverage[site_norm]
        if line in line_map:
            return ("covered" if line_map[line] > 0 else "uncovered", line_map[line])
        return ("unknown_line", None)
    # Suffix match: cobertura paths might be relative to a project subfolder
    # while site_file is relative to the repo root. Try matching by basename
    # with directory tail.
    site_parts = site_norm.split("/")
    for n in (5, 4, 3, 2):
        if len(site_parts) < n:
            continue
        suffix = "/".join(site_parts[-n:])
        for cov_path, line_map in coverage.items():
            if cov_path.endswith(suffix):
                if line in line_map:
                    return ("covered" if line_map[line] > 0 else "uncovered", line_map[line])
                return ("unknown_line", None)
    return ("unknown_file", None)


def main():
    OUT_CSV.parent.mkdir(parents=True, exist_ok=True)

    # Load all coverage maps
    coverage_by_repo: dict[str, dict[str, dict[int, int]]] = {}
    for repo in COVERED_REPOS:
        cov = load_coverage(repo)
        coverage_by_repo[repo] = cov
        n_files = len(cov)
        n_lines = sum(len(m) for m in cov.values())
        print(f"  {repo:18s}  {n_files:>6d} files, {n_lines:>10d} lines in coverage map")

    # Load sites
    with SITES_CSV.open() as fh:
        sites = list(csv.DictReader(fh))
    print(f"\nLoaded {len(sites)} Mode #1 sites")

    # Cross-reference
    rows = []
    repo_summary: dict[str, Counter] = defaultdict(Counter)
    for s in sites:
        repo = s["repo"]
        if repo not in COVERED_REPOS:
            status, hits = "no_coverage_data", None
        else:
            status, hits = find_coverage(coverage_by_repo[repo], s["file"], int(s["line"]))
        rows.append({
            **s,
            "coverage_status": status,
            "hits": "" if hits is None else hits,
        })
        repo_summary[repo][status] += 1

    # Write CSV
    with OUT_CSV.open("w", newline="") as fh:
        w = csv.DictWriter(fh, fieldnames=["repo", "file", "line", "receiver_type", "method", "kind", "containing_type", "coverage_status", "hits"])
        w.writeheader()
        w.writerows(rows)
    print(f"\nWrote {OUT_CSV}")

    # Markdown summary
    md = ["# Mode #1 sites × coverage cross-reference",
          "",
          f"Source: `Mode1Analyzer/results/mode1_sites.csv` ({len(sites)} sites)",
          f"Coverage: CI run 25495265941 (cobertura XML)",
          "",
          "## Per-repo breakdown",
          "",
          "| Repo | Sites | Covered | Uncovered | Unknown line | Unknown file | No coverage |",
          "|---|---:|---:|---:|---:|---:|---:|"]
    for repo in sorted(repo_summary):
        c = repo_summary[repo]
        total = sum(c.values())
        md.append(f"| {repo} | {total} | {c['covered']} | {c['uncovered']} | {c['unknown_line']} | {c['unknown_file']} | {c['no_coverage_data']} |")
    md.append("")
    md.append("**Status meanings:**")
    md.append("- `covered` — Mode #1 call site executed by tests at least once")
    md.append("- `uncovered` — site exists, line is in coverage map, hits = 0")
    md.append("- `unknown_line` — file in coverage map but the specific line isn't (likely whitespace/comment offset)")
    md.append("- `unknown_file` — file not in cobertura output (likely production code excluded by test filter, or path mismatch)")
    md.append("- `no_coverage_data` — repo not yet wired into coverage CI")
    md.append("")
    # Headline numbers
    grand = Counter()
    for c in repo_summary.values():
        grand.update(c)
    total = sum(grand.values())
    md.append(f"## Headline\n\nOf {total} Mode #1 sites with coverage data attempted:")
    for k in ("covered", "uncovered", "unknown_line", "unknown_file", "no_coverage_data"):
        md.append(f"- **{grand[k]:,}** {k} ({100*grand[k]/total:.1f}%)")
    OUT_MD.write_text("\n".join(md) + "\n")
    print(f"Wrote {OUT_MD}")


if __name__ == "__main__":
    main()
