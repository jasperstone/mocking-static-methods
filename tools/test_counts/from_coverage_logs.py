#!/usr/bin/env python3
"""Extract per-project test counts from Coverage Orchestrator workflow logs.

The `--list-tests` discovery path is broken for xunit.v3 repos (efcore, roslyn,
abp partially) because the v3 VSTest adapter emits "No test is available" under
that flag. But the actual coverage workflow runs `dotnet test` per-project and
the resulting summary lines are authoritative:

    Passed!  - Failed:  0, Passed:  6622, Skipped:  0, Total:  6622, Duration: 1 m 2 s - Microsoft.EntityFrameworkCore.Tests.dll (net10.0)

This script downloads completed Coverage Orchestrator job logs via `gh`, parses
those summary lines, and emits a per-project test-count CSV plus a markdown
summary.

Coverlet-based runs (ASP.NET Core, Orleans, Semantic Kernel) emit a different
shape — the per-project `Passed!` line is suppressed by coverlet's wrapper, so
those repos are reported as "no per-project counts available in this log shape".
"""
from __future__ import annotations

import argparse
import csv
import json
import os
import re
import subprocess
import sys
from collections import defaultdict
from dataclasses import dataclass, field, asdict
from pathlib import Path
from typing import Iterable

REPO = "jasperstone/mocking-static-methods"
WORKFLOW = "coverage-orchestrator.yml"
BRANCH = "jasper/squad"
LOG_CACHE = Path("/tmp")

# Pattern A: dotnet test summary line.
#   <iso-timestamp> <Passed|Failed>!  - Failed:  N, Passed:  N, Skipped:  N, Total:  N, Duration: ... - <Assembly>.dll (<framework>)
SUMMARY_RE = re.compile(
    r"^(?:\S+\s+)?(?P<status>Passed|Failed)!\s+-\s+"
    r"Failed:\s+(?P<failed>\d+),\s+"
    r"Passed:\s+(?P<passed>\d+),\s+"
    r"Skipped:\s+(?P<skipped>\d+),\s+"
    r"Total:\s+(?P<total>\d+),\s+"
    r"Duration:\s+.*?-\s+(?P<dll>\S+\.dll)\s+\((?P<framework>[^)]+)\)\s*$"
)

# Map "Coverage: <Display Name>" → canonical repo slug used in the workspace.
JOB_NAME_TO_REPO = {
    "abp": "abp",
    "asp.net core": "aspnetcore",
    "aspnetcore": "aspnetcore",
    "ef core": "efcore",
    "efcore": "efcore",
    "orleans": "orleans",
    "roslyn": "roslyn",
    "runtime": "runtime",
    ".net runtime": "runtime",
    "semantic kernel": "semantic-kernel",
    "semantic-kernel": "semantic-kernel",
}


@dataclass
class TestRow:
    repo: str
    project: str
    dll: str
    framework: str
    total: int
    passed: int
    failed: int
    skipped: int
    status: str
    source_run_id: str
    source_job_id: str


@dataclass
class JobInfo:
    job_id: str
    name: str
    repo: str | None  # None if we couldn't classify (not a "Coverage: …" job)


@dataclass
class RepoResult:
    repo: str
    rows: list[TestRow] = field(default_factory=list)
    note: str | None = None  # e.g., "<coverlet — no per-project test counts available in this log shape>"


def sh(cmd: list[str], *, check: bool = True, capture: bool = True) -> subprocess.CompletedProcess:
    """Thin wrapper so the script's gh invocations are easy to read."""
    return subprocess.run(cmd, check=check, capture_output=capture, text=True)


def latest_successful_run() -> str:
    """Pick the most recent successful Coverage Orchestrator run on the branch.

    Most recent runs have failed conclusions, so a generous --limit is required
    to find a green one.
    """
    out = sh([
        "gh", "run", "list",
        "--workflow", WORKFLOW,
        "--branch", BRANCH,
        "--status", "completed",
        "--limit", "50",
        "--json", "databaseId,conclusion",
    ]).stdout
    runs = json.loads(out)
    for r in runs:
        if r.get("conclusion") == "success":
            return str(r["databaseId"])
    raise SystemExit("No successful Coverage Orchestrator run found on branch.")


def list_jobs(run_id: str) -> list[JobInfo]:
    out = sh([
        "gh", "run", "view", run_id,
        "--json", "jobs",
        "-q", ".jobs[] | {id: .databaseId, name: .name}",
    ]).stdout
    jobs: list[JobInfo] = []
    for line in out.splitlines():
        line = line.strip()
        if not line:
            continue
        obj = json.loads(line)
        name = obj["name"]
        repo = None
        if name.startswith("Coverage:"):
            display = name.split(":", 1)[1].strip().lower()
            repo = JOB_NAME_TO_REPO.get(display)
            if repo is None:
                # Best-effort fallback: slugify.
                repo = display.replace(" ", "-").replace(".", "")
        jobs.append(JobInfo(job_id=str(obj["id"]), name=name, repo=repo))
    return jobs


def fetch_log(job_id: str) -> Path:
    """Cache job logs in /tmp so re-runs are cheap."""
    path = LOG_CACHE / f"cov_{job_id}.log"
    if path.exists() and path.stat().st_size > 0:
        return path
    proc = subprocess.run(
        ["gh", "api", f"/repos/{REPO}/actions/jobs/{job_id}/logs"],
        capture_output=True,
    )
    # gh prints binary-ish text on stdout; the log endpoint returns text.
    if proc.returncode != 0:
        sys.stderr.write(
            f"WARN: gh api returned {proc.returncode} for job {job_id}: "
            f"{proc.stderr.decode('utf-8', errors='replace')[:200]}\n"
        )
        path.write_bytes(b"")
        return path
    path.write_bytes(proc.stdout)
    return path


def parse_log(log_path: Path, repo: str, run_id: str, job_id: str) -> list[TestRow]:
    """Parse summary lines. Last occurrence per (dll, framework) wins (handles retries)."""
    by_key: dict[tuple[str, str], TestRow] = {}
    try:
        text = log_path.read_text(encoding="utf-8", errors="replace")
    except FileNotFoundError:
        return []
    for raw in text.splitlines():
        # GitHub Actions prefixes every line with an ISO timestamp; strip leading
        # whitespace before matching so the regex anchor works either way.
        m = SUMMARY_RE.match(raw.strip())
        if not m:
            continue
        dll = m.group("dll")
        framework = m.group("framework")
        project = dll[:-4] if dll.endswith(".dll") else dll
        row = TestRow(
            repo=repo,
            project=project,
            dll=dll,
            framework=framework,
            total=int(m.group("total")),
            passed=int(m.group("passed")),
            failed=int(m.group("failed")),
            skipped=int(m.group("skipped")),
            status=m.group("status"),
            source_run_id=run_id,
            source_job_id=job_id,
        )
        by_key[(dll, framework)] = row  # last-wins
    return list(by_key.values())


def merge_runs(per_run: list[tuple[str, list[RepoResult]]]) -> dict[str, RepoResult]:
    """Across multiple runs, take the most recent (run_id) row per (repo, dll, framework).

    `per_run` is ordered by run_id ascending so later iterations overwrite earlier ones.
    """
    merged: dict[str, RepoResult] = {}
    # Sort runs by numeric id ascending; later (larger) IDs overwrite earlier.
    per_run_sorted = sorted(per_run, key=lambda x: int(x[0]))
    for run_id, repo_results in per_run_sorted:
        for rr in repo_results:
            slot = merged.setdefault(rr.repo, RepoResult(repo=rr.repo))
            if rr.note and not slot.rows:
                slot.note = rr.note
            if rr.rows:
                # Replace any prior rows for the same key.
                existing = {(r.dll, r.framework): r for r in slot.rows}
                for r in rr.rows:
                    existing[(r.dll, r.framework)] = r
                slot.rows = list(existing.values())
                slot.note = None  # data found, drop the placeholder
    return merged


def write_csv(rows: list[TestRow], path: Path) -> None:
    rows_sorted = sorted(rows, key=lambda r: (r.repo, r.project, r.framework))
    with path.open("w", newline="") as f:
        w = csv.writer(f)
        w.writerow([
            "repo", "project", "dll", "framework",
            "total", "passed", "failed", "skipped", "status",
            "source_run_id", "source_job_id",
        ])
        for r in rows_sorted:
            w.writerow([
                r.repo, r.project, r.dll, r.framework,
                r.total, r.passed, r.failed, r.skipped, r.status,
                r.source_run_id, r.source_job_id,
            ])


def render_markdown(merged: dict[str, RepoResult], run_ids: list[str]) -> str:
    lines: list[str] = []
    lines.append("# Test Counts (from coverage workflow logs)")
    lines.append("")
    lines.append(f"_Generated: 2026-05-07 from runs {', '.join(run_ids)}_")
    lines.append("")
    lines.append(
        "Per-project test counts extracted from `dotnet test` summary lines in "
        "Coverage Orchestrator job logs. Bypasses the broken `--list-tests` path "
        "for xunit.v3 repos."
    )
    lines.append("")

    # Aggregate table
    lines.append("## Per-repo aggregate")
    lines.append("")
    lines.append("| repo | projects | total | passed | failed | skipped |")
    lines.append("|---|---:|---:|---:|---:|---:|")
    for repo in sorted(merged):
        rr = merged[repo]
        if not rr.rows:
            continue
        proj_count = len(rr.rows)
        total = sum(r.total for r in rr.rows)
        passed = sum(r.passed for r in rr.rows)
        failed = sum(r.failed for r in rr.rows)
        skipped = sum(r.skipped for r in rr.rows)
        lines.append(
            f"| {repo} | {proj_count} | {total} | {passed} | {failed} | {skipped} |"
        )
    lines.append("")

    # Per-repo top/bottom
    for repo in sorted(merged):
        rr = merged[repo]
        if not rr.rows:
            continue
        lines.append(f"### {repo}")
        lines.append("")
        rows_sorted = sorted(rr.rows, key=lambda r: r.total, reverse=True)
        lines.append("**Top 10 projects by test count**")
        lines.append("")
        lines.append("| project | framework | total | passed | failed | skipped |")
        lines.append("|---|---|---:|---:|---:|---:|")
        for r in rows_sorted[:10]:
            lines.append(
                f"| {r.project} | {r.framework} | {r.total} | {r.passed} | "
                f"{r.failed} | {r.skipped} |"
            )
        lines.append("")
        if len(rows_sorted) > 10:
            lines.append("**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)")
            lines.append("")
            lines.append("| project | framework | total | passed | failed | skipped |")
            lines.append("|---|---|---:|---:|---:|---:|")
            bottom = sorted(rr.rows, key=lambda r: r.total)[:10]
            for r in bottom:
                flag = "⚠️ " if r.total == 0 else ""
                lines.append(
                    f"| {flag}{r.project} | {r.framework} | {r.total} | {r.passed} | "
                    f"{r.failed} | {r.skipped} |"
                )
            lines.append("")

    # Missing-data section
    missing = [rr for rr in merged.values() if not rr.rows]
    lines.append("## Repos missing data")
    lines.append("")
    if not missing:
        lines.append("_None — every observed Coverage job emitted parseable summary lines._")
    else:
        for rr in sorted(missing, key=lambda x: x.repo):
            note = rr.note or "<no summary lines parsed>"
            lines.append(f"- **{rr.repo}** — {note}")
    lines.append("")
    return "\n".join(lines)


def process_run(run_id: str) -> list[RepoResult]:
    print(f"[run {run_id}] listing jobs…", file=sys.stderr)
    jobs = list_jobs(run_id)
    by_repo: dict[str, RepoResult] = {}
    for j in jobs:
        if not j.name.startswith("Coverage:") or j.repo is None:
            continue
        slot = by_repo.setdefault(j.repo, RepoResult(repo=j.repo))
        print(f"[run {run_id}] fetching log for job {j.job_id} ({j.name})…", file=sys.stderr)
        log_path = fetch_log(j.job_id)
        rows = parse_log(log_path, j.repo, run_id, j.job_id)
        if rows:
            existing = {(r.dll, r.framework): r for r in slot.rows}
            for r in rows:
                existing[(r.dll, r.framework)] = r
            slot.rows = list(existing.values())
            slot.note = None
        else:
            if not slot.rows:
                slot.note = (
                    "<coverlet — no per-project test counts available in this log shape>"
                )
        print(
            f"[run {run_id}] {j.repo}: parsed {len(rows)} project rows from job {j.job_id}",
            file=sys.stderr,
        )
    return list(by_repo.values())


def main(argv: list[str] | None = None) -> int:
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument(
        "run_ids", nargs="*",
        help="Coverage Orchestrator workflow run IDs. Default: most recent successful run.",
    )
    p.add_argument(
        "--run-id", action="append", default=[],
        help="Additional run IDs (repeatable, alternative to positional args).",
    )
    p.add_argument(
        "--csv", default="test_counts.csv", help="Output CSV path (default: test_counts.csv)",
    )
    p.add_argument(
        "--md", default="docs/TEST_COUNTS.md", help="Output markdown path (default: docs/TEST_COUNTS.md)",
    )
    args = p.parse_args(argv)

    run_ids = list(args.run_ids) + list(args.run_id)
    if not run_ids:
        run_ids = [latest_successful_run()]
    run_ids = sorted(set(run_ids), key=int)

    per_run: list[tuple[str, list[RepoResult]]] = []
    for rid in run_ids:
        per_run.append((rid, process_run(rid)))

    merged = merge_runs(per_run)
    rows = [r for rr in merged.values() for r in rr.rows]

    csv_path = Path(args.csv)
    md_path = Path(args.md)
    write_csv(rows, csv_path)
    md = render_markdown(merged, run_ids)
    md_path.parent.mkdir(parents=True, exist_ok=True)
    md_path.write_text(md)

    print(md)
    print(f"\nWrote {len(rows)} rows to {csv_path} and summary to {md_path}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
