#!/usr/bin/env python3
"""Aggregate test-discovery CSVs into TEST_DISCOVERY.md + summary CSV.

Usage:
    # After: gh run download <run-id> -p 'test-discovery-*' -D test_discovery_artifacts
    python3 tools/test_discovery/aggregate.py [artifacts_dir] [--out-md TEST_DISCOVERY.md] [--out-csv test_discovery_summary.csv]

The artifacts dir is expected to contain one folder per uploaded artifact
(test-discovery-<repo>/test-discovery-<repo>.csv), which is what
`gh run download` produces. We accept any layout though — we just glob for
test-discovery-*.csv recursively.
"""
from __future__ import annotations

import argparse
import csv
import sys
from pathlib import Path
from typing import Iterable


REPO_ORDER = ["abp", "aspnetcore", "efcore", "orleans", "roslyn", "semantic-kernel"]


def find_csvs(root: Path) -> list[Path]:
    return sorted(root.rglob("test-discovery-*.csv"))


def load_rows(csv_paths: Iterable[Path]) -> list[dict]:
    rows: list[dict] = []
    for path in csv_paths:
        with path.open(newline="") as f:
            reader = csv.DictReader(f)
            for row in reader:
                # Coerce numerics; tolerate missing/empty.
                for k in ("tests_universe", "tests_in_filter", "tests_excluded"):
                    try:
                        row[k] = int(row.get(k, "") or 0)
                    except ValueError:
                        row[k] = 0
                rows.append(row)
    return rows


def per_repo_summary(rows: list[dict]) -> list[dict]:
    by_repo: dict[str, dict] = {}
    for r in rows:
        repo = r["repo"]
        d = by_repo.setdefault(repo, {
            "repo": repo,
            "projects": 0,
            "projects_built": 0,
            "projects_unbuilt": 0,
            "projects_error": 0,
            "tests_universe": 0,
            "tests_in_filter": 0,
        })
        d["projects"] += 1
        status = r.get("build_status", "")
        if status == "ok":
            d["projects_built"] += 1
        elif status == "<not-built>":
            d["projects_unbuilt"] += 1
        else:
            d["projects_error"] += 1
        d["tests_universe"] += r["tests_universe"]
        d["tests_in_filter"] += r["tests_in_filter"]

    summaries = []
    for repo in REPO_ORDER:
        if repo in by_repo:
            summaries.append(by_repo.pop(repo))
    summaries.extend(by_repo.values())  # any repos not in REPO_ORDER

    for d in summaries:
        d["tests_excluded"] = d["tests_universe"] - d["tests_in_filter"]
        d["exclusion_pct"] = (
            (d["tests_excluded"] / d["tests_universe"] * 100.0)
            if d["tests_universe"] > 0 else 0.0
        )
    return summaries


def write_summary_csv(summaries: list[dict], out_path: Path) -> None:
    fields = [
        "repo", "projects", "projects_built", "projects_unbuilt", "projects_error",
        "tests_universe", "tests_in_filter", "tests_excluded", "exclusion_pct",
    ]
    with out_path.open("w", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        for d in summaries:
            row = {k: d[k] for k in fields}
            row["exclusion_pct"] = f"{d['exclusion_pct']:.1f}"
            w.writerow(row)


def fmt_pct(num: int, denom: int) -> str:
    if denom <= 0:
        return "—"
    return f"{num / denom * 100:.1f}%"


def write_markdown(rows: list[dict], summaries: list[dict], out_path: Path) -> None:
    lines: list[str] = []
    lines.append("# Test Discovery Diagnostic")
    lines.append("")
    lines.append("Per-project counts of tests included by the CI `--filter` vs the")
    lines.append("unfiltered universe of tests `dotnet test --list-tests` would")
    lines.append("emit. Generated 2026-05-07.")
    lines.append("")
    lines.append("- **tests_universe** — count from `dotnet test --no-build --list-tests` (no filter)")
    lines.append("- **tests_in_filter** — count with the same FILTER the coverage workflow uses")
    lines.append("- **tests_excluded** = universe − in_filter")
    lines.append("- A project marked `<not-built>` could not be enumerated and is treated as 0/0/0.")
    lines.append("")

    # Per-repo summary table
    lines.append("## Per-repo summary")
    lines.append("")
    lines.append("| Repo | Projects | Built | Universe | In filter | Excluded | Excl % |")
    lines.append("|---|---:|---:|---:|---:|---:|---:|")
    for d in summaries:
        lines.append(
            f"| {d['repo']} | {d['projects']} | "
            f"{d['projects_built']} | {d['tests_universe']:,} | "
            f"{d['tests_in_filter']:,} | {d['tests_excluded']:,} | "
            f"{d['exclusion_pct']:.1f}% |"
        )
    lines.append("")

    # Top-10 highest exclusion ratio (only built projects with >0 universe)
    eligible = [r for r in rows if r.get("build_status") == "ok" and r["tests_universe"] > 0]
    by_excl_ratio = sorted(
        eligible,
        key=lambda r: (r["tests_excluded"] / r["tests_universe"]) if r["tests_universe"] else 0,
        reverse=True,
    )
    lines.append("## Top 10 projects: highest filter-exclusion ratio")
    lines.append("")
    lines.append("Projects where the FILTER drops the largest fraction of tests. Signal: filter is aggressive here — review whether legitimate unit tests are being excluded.")
    lines.append("")
    lines.append("| Repo | Project | Universe | In filter | Excluded | Excl % |")
    lines.append("|---|---|---:|---:|---:|---:|")
    for r in by_excl_ratio[:10]:
        ratio = r["tests_excluded"] / r["tests_universe"] * 100
        lines.append(
            f"| {r['repo']} | `{r['project']}` | {r['tests_universe']:,} | "
            f"{r['tests_in_filter']:,} | {r['tests_excluded']:,} | {ratio:.1f}% |"
        )
    lines.append("")

    # Bottom-10 lowest tests_in_filter (built, in_filter > 0 ignored — we want low including 0)
    by_low_filter = sorted(eligible, key=lambda r: r["tests_in_filter"])
    lines.append("## Bottom 10 projects: lowest tests-in-filter count")
    lines.append("")
    lines.append("Projects with the fewest tests actually executed by the CI filter (among built projects). Signal: the test inventory exists but isn't running — either narrowly scoped tests, or the filter excludes ~all of them.")
    lines.append("")
    lines.append("| Repo | Project | Universe | In filter | Excluded |")
    lines.append("|---|---|---:|---:|---:|")
    for r in by_low_filter[:10]:
        lines.append(
            f"| {r['repo']} | `{r['project']}` | {r['tests_universe']:,} | "
            f"{r['tests_in_filter']:,} | {r['tests_excluded']:,} |"
        )
    lines.append("")

    # Unbuilt / error list
    skipped = [r for r in rows if r.get("build_status") != "ok"]
    if skipped:
        lines.append("## Projects skipped (not built / error)")
        lines.append("")
        lines.append("| Repo | Project | Status |")
        lines.append("|---|---|---|")
        for r in skipped:
            lines.append(f"| {r['repo']} | `{r['project']}` | {r['build_status']} |")
        lines.append("")

    out_path.write_text("\n".join(lines))


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "artifacts_dir",
        nargs="?",
        default="test_discovery_artifacts",
        help="Directory containing test-discovery-*.csv (recursively). Default: test_discovery_artifacts",
    )
    parser.add_argument("--out-md", default="TEST_DISCOVERY.md")
    parser.add_argument("--out-csv", default="test_discovery_summary.csv")
    args = parser.parse_args(argv)

    root = Path(args.artifacts_dir)
    if not root.exists():
        print(f"error: {root} does not exist", file=sys.stderr)
        return 2

    csvs = find_csvs(root)
    if not csvs:
        print(f"error: no test-discovery-*.csv found under {root}", file=sys.stderr)
        return 2

    print(f"Found {len(csvs)} CSV file(s):", file=sys.stderr)
    for p in csvs:
        print(f"  {p}", file=sys.stderr)

    rows = load_rows(csvs)
    summaries = per_repo_summary(rows)

    write_summary_csv(summaries, Path(args.out_csv))
    write_markdown(rows, summaries, Path(args.out_md))
    print(f"Wrote {args.out_md} and {args.out_csv}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    sys.exit(main())
