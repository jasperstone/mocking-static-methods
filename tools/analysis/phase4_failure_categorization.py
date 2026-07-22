#!/usr/bin/env python3
"""Non-submitted failure categorization and rerun diagnostics.

Scans attempts.jsonl under phases/<phase>/results/<model>/run_*/ and buckets
non-submitted rows into stable categories. Produces machine-readable CSV/JSON
plus a markdown companion report with rerun-needed signals.

Usage:
    python3 tools/analysis/phase4_failure_categorization.py
    python3 tools/analysis/phase4_failure_categorization.py --phase phase4-refactoring
    python3 tools/analysis/phase4_failure_categorization.py --phases phase2-agentic,phase3-agentic-loop,phase4-refactoring
"""

from __future__ import annotations

import argparse
import csv
import json
import re
from collections import Counter, defaultdict
from dataclasses import dataclass
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]

BUCKETS = [
    "timeout/connection",
    "auth/access",
    "rate-limit",
    "server-5xx",
    "api-version-unsupported",
    "baseline_compile_failed",
    "baseline_no_owning_csproj",
    "other",
]

INFRA_BUCKETS = {
    "timeout/connection",
    "auth/access",
    "rate-limit",
    "server-5xx",
    "api-version-unsupported",
}

AUTH_RE = re.compile(
    r"(\b401\b|\b403\b|unauthori[sz]ed|forbidden|access denied|invalid subscription key|auth)",
    re.IGNORECASE,
)
RATE_LIMIT_RE = re.compile(r"(\b429\b|rate.?limit|too many requests|quota)", re.IGNORECASE)
TIMEOUT_CONN_RE = re.compile(
    r"(timeout|timed out|read operation timed out|connection|network|socket|econn|reset by peer)",
    re.IGNORECASE,
)
SERVER_5XX_RE = re.compile(
    r"(\b5\d\d\b|internal server error|bad gateway|service unavailable|gateway timeout)",
    re.IGNORECASE,
)
API_VERSION_UNSUPPORTED_RE = re.compile(
    r"(api\s*version\s*not\s*supported|unsupported\s*api\s*version|invalid\s*api\s*version)",
    re.IGNORECASE,
)


@dataclass(frozen=True)
class Key:
    phase: str
    model: str
    run: int


def classify_non_submitted(rec: dict) -> str:
    text = " ".join(
        str(rec.get(k, ""))
        for k in ("halt_reason", "error", "error_type", "final_error_type")
        if rec.get(k) is not None
    ).lower()

    if "baseline_compile_failed" in text:
        return "baseline_compile_failed"
    if "baseline_no_owning_csproj" in text:
        return "baseline_no_owning_csproj"
    if API_VERSION_UNSUPPORTED_RE.search(text):
        return "api-version-unsupported"
    if RATE_LIMIT_RE.search(text):
        return "rate-limit"
    if AUTH_RE.search(text):
        return "auth/access"
    if SERVER_5XX_RE.search(text):
        return "server-5xx"
    if TIMEOUT_CONN_RE.search(text):
        return "timeout/connection"
    return "other"


def parse_run_index(run_dir_name: str) -> int:
    if run_dir_name.startswith("run_"):
        try:
            return int(run_dir_name.split("_", 1)[1])
        except ValueError:
            return -1
    return -1


def iter_attempt_paths(phase_dir: Path, include_results_glob: bool) -> list[Path]:
    # Canonical path is always included; optional glob adds non-canonical trees.
    patterns = ["results/*/run_*/attempts.jsonl"]
    if include_results_glob:
        patterns.append("results*/*/run_*/attempts.jsonl")

    seen: set[Path] = set()
    paths: list[Path] = []
    for pattern in patterns:
        for attempts_path in sorted(phase_dir.glob(pattern)):
            resolved = attempts_path.resolve()
            if resolved in seen:
                continue
            seen.add(resolved)
            paths.append(attempts_path)

    return paths


def collect(
    phase: str,
    include_results_glob: bool,
) -> tuple[dict[Key, Counter[str]], dict[Key, int], dict[Key, int]]:
    phase_dir = REPO_ROOT / "phases" / phase
    counters: dict[Key, Counter[str]] = defaultdict(Counter)
    attempts_total: dict[Key, int] = defaultdict(int)
    non_submitted_total: dict[Key, int] = defaultdict(int)

    for attempts_path in iter_attempt_paths(phase_dir=phase_dir, include_results_glob=include_results_glob):
        model = attempts_path.parent.parent.name
        run = parse_run_index(attempts_path.parent.name)
        key = Key(phase=phase, model=model, run=run)
        with attempts_path.open() as fh:
            for line in fh:
                line = line.strip()
                if not line:
                    continue
                rec = json.loads(line)
                attempts_total[key] += 1
                if rec.get("submitted"):
                    continue
                non_submitted_total[key] += 1
                bucket = classify_non_submitted(rec)
                counters[key][bucket] += 1

    return counters, attempts_total, non_submitted_total


def collect_many(
    phases: list[str],
    include_results_glob: bool,
) -> tuple[dict[Key, Counter[str]], dict[Key, int], dict[Key, int]]:
    merged_counters: dict[Key, Counter[str]] = defaultdict(Counter)
    merged_attempts: dict[Key, int] = defaultdict(int)
    merged_non_submitted: dict[Key, int] = defaultdict(int)

    for phase in phases:
        counters, attempts_total, non_submitted_total = collect(
            phase=phase,
            include_results_glob=include_results_glob,
        )
        for key, count_map in counters.items():
            merged_counters[key].update(count_map)
        for key, value in attempts_total.items():
            merged_attempts[key] += value
        for key, value in non_submitted_total.items():
            merged_non_submitted[key] += value

    return merged_counters, merged_attempts, merged_non_submitted


def rerun_signal(infra_count: int, attempts: int, non_submitted: int) -> str:
    if attempts <= 0 or non_submitted <= 0:
        return "green"
    infra_share_attempts = infra_count / attempts
    infra_share_non_sub = infra_count / non_submitted
    if infra_count >= 10 or infra_share_attempts >= 0.03 or infra_share_non_sub >= 0.20:
        return "red"
    if infra_count >= 3 or infra_share_attempts >= 0.01 or infra_share_non_sub >= 0.10:
        return "yellow"
    return "green"


def write_outputs(
    phases: list[str],
    counters: dict[Key, Counter[str]],
    attempts_total: dict[Key, int],
    non_submitted_total: dict[Key, int],
    out_dir: Path,
    markdown_path: Path,
    output_prefix: str,
) -> None:
    out_dir.mkdir(parents=True, exist_ok=True)

    by_model_run_csv = out_dir / f"{output_prefix}_failure_categories_by_model_run.csv"
    totals_csv = out_dir / f"{output_prefix}_failure_categories_totals.csv"
    signals_csv = out_dir / f"{output_prefix}_failure_rerun_signal_by_model_run.csv"
    summary_json = out_dir / f"{output_prefix}_failure_categories_summary.json"

    by_rows: list[dict] = []
    signal_rows: list[dict] = []

    all_keys = sorted(set(attempts_total.keys()) | set(counters.keys()), key=lambda k: (k.phase, k.model, k.run))

    for key in all_keys:
        counts = counters[key]
        attempts = attempts_total.get(key, 0)
        non_sub = non_submitted_total.get(key, 0)
        infra_count = sum(counts.get(b, 0) for b in INFRA_BUCKETS)
        signal = rerun_signal(infra_count=infra_count, attempts=attempts, non_submitted=non_sub)

        signal_rows.append(
            {
                "phase": key.phase,
                "model": key.model,
                "run": key.run,
                "attempts_total": attempts,
                "non_submitted_total": non_sub,
                "infra_non_submitted": infra_count,
                "infra_share_of_attempts": f"{(infra_count / attempts) if attempts else 0.0:.4f}",
                "infra_share_of_non_submitted": f"{(infra_count / non_sub) if non_sub else 0.0:.4f}",
                "rerun_signal": signal,
            }
        )

        for bucket in BUCKETS:
            count = counts.get(bucket, 0)
            by_rows.append(
                {
                    "phase": key.phase,
                    "model": key.model,
                    "run": key.run,
                    "attempts_total": attempts,
                    "non_submitted_total": non_sub,
                    "category": bucket,
                    "count": count,
                    "infra_bucket": int(bucket in INFRA_BUCKETS),
                }
            )

    totals = Counter()
    for key, count_map in counters.items():
        for bucket, count in count_map.items():
            totals[(key.phase, bucket)] += count

    phase_attempts = Counter()
    phase_non_submitted = Counter()
    for key, attempts in attempts_total.items():
        phase_attempts[key.phase] += attempts
    for key, non_sub in non_submitted_total.items():
        phase_non_submitted[key.phase] += non_sub

    grand_attempts = sum(attempts_total.values())
    grand_non_submitted = sum(non_submitted_total.values())

    total_rows: list[dict] = []
    for phase in phases:
        attempts = phase_attempts.get(phase, 0)
        non_sub = phase_non_submitted.get(phase, 0)
        for bucket in BUCKETS:
            cnt = totals.get((phase, bucket), 0)
            total_rows.append(
                {
                    "scope": "phase_total",
                    "phase": phase,
                    "category": bucket,
                    "count": cnt,
                    "share_of_non_submitted": f"{(cnt / non_sub) if non_sub else 0.0:.4f}",
                    "share_of_attempts": f"{(cnt / attempts) if attempts else 0.0:.4f}",
                }
            )

    if len(phases) > 1:
        for bucket in BUCKETS:
            cnt = sum(totals.get((phase, bucket), 0) for phase in phases)
            total_rows.append(
                {
                    "scope": "all_phases_total",
                    "phase": "all",
                    "category": bucket,
                    "count": cnt,
                    "share_of_non_submitted": f"{(cnt / grand_non_submitted) if grand_non_submitted else 0.0:.4f}",
                    "share_of_attempts": f"{(cnt / grand_attempts) if grand_attempts else 0.0:.4f}",
                }
            )

    with by_model_run_csv.open("w", newline="") as f:
        w = csv.DictWriter(
            f,
            fieldnames=[
                "phase",
                "model",
                "run",
                "attempts_total",
                "non_submitted_total",
                "category",
                "count",
                "infra_bucket",
            ],
        )
        w.writeheader()
        w.writerows(by_rows)

    with totals_csv.open("w", newline="") as f:
        w = csv.DictWriter(
            f,
            fieldnames=[
                "scope",
                "phase",
                "category",
                "count",
                "share_of_non_submitted",
                "share_of_attempts",
            ],
        )
        w.writeheader()
        w.writerows(total_rows)

    with signals_csv.open("w", newline="") as f:
        w = csv.DictWriter(
            f,
            fieldnames=[
                "phase",
                "model",
                "run",
                "attempts_total",
                "non_submitted_total",
                "infra_non_submitted",
                "infra_share_of_attempts",
                "infra_share_of_non_submitted",
                "rerun_signal",
            ],
        )
        w.writeheader()
        w.writerows(signal_rows)

    summary = {
        "phases": phases,
        "output_prefix": output_prefix,
        "bucket_order": BUCKETS,
        "infra_buckets": sorted(INFRA_BUCKETS),
        "thresholds": {
            "red": {
                "infra_non_submitted_min": 10,
                "infra_share_of_attempts_min": 0.03,
                "infra_share_of_non_submitted_min": 0.20,
            },
            "yellow": {
                "infra_non_submitted_min": 3,
                "infra_share_of_attempts_min": 0.01,
                "infra_share_of_non_submitted_min": 0.10,
            },
        },
        "grand_totals": {
            "attempts_total": grand_attempts,
            "non_submitted_total": grand_non_submitted,
            "categories": {b: sum(totals.get((phase, b), 0) for phase in phases) for b in BUCKETS},
            "by_phase": {
                phase: {
                    "attempts_total": phase_attempts.get(phase, 0),
                    "non_submitted_total": phase_non_submitted.get(phase, 0),
                    "categories": {b: totals.get((phase, b), 0) for b in BUCKETS},
                }
                for phase in phases
            },
        },
        "signals_by_model_run": signal_rows,
        "outputs": {
            "by_model_run_csv": str(by_model_run_csv.relative_to(REPO_ROOT)),
            "totals_csv": str(totals_csv.relative_to(REPO_ROOT)),
            "signals_csv": str(signals_csv.relative_to(REPO_ROOT)),
        },
    }
    summary_json.write_text(json.dumps(summary, indent=2) + "\n")

    infra_grand = sum(summary["grand_totals"]["categories"][b] for b in INFRA_BUCKETS)
    infra_share_all_attempts = (infra_grand / grand_attempts) if grand_attempts else 0.0
    infra_share_non_sub = (infra_grand / grand_non_submitted) if grand_non_submitted else 0.0

    signal_priority = {"red": 0, "yellow": 1, "green": 2}
    top_signals = sorted(
        signal_rows,
        key=lambda r: (signal_priority.get(r["rerun_signal"], 9), r["model"], int(r["run"])),
    )

    lines = []
    lines.append(f"# Failure categorization diagnostics — {output_prefix}")
    lines.append("")
    lines.append("Companion diagnostics only. This view does not change compile/run quality metric definitions.")
    lines.append("")
    lines.append("## Totals")
    lines.append("")
    lines.append(f"- attempts_total: {grand_attempts}")
    lines.append(f"- non_submitted_total: {grand_non_submitted}")
    lines.append(f"- infra_non_submitted_total: {infra_grand}")
    lines.append(f"- infra_share_of_attempts: {infra_share_all_attempts:.2%}")
    lines.append(f"- infra_share_of_non_submitted: {infra_share_non_sub:.2%}")
    lines.append("")
    lines.append("## Per-phase category totals")
    lines.append("")
    lines.append("| phase | category | count | share_of_non_submitted | share_of_attempts |")
    lines.append("|---|---|---:|---:|---:|")
    for row in total_rows:
        if row["scope"] != "phase_total":
            continue
        lines.append(
            f"| {row['phase']} | {row['category']} | {row['count']} | {float(row['share_of_non_submitted']):.2%} | {float(row['share_of_attempts']):.2%} |"
        )

    if len(phases) > 1:
        lines.append("")
        lines.append("## All-phases category totals")
        lines.append("")
        lines.append("| category | count | share_of_non_submitted | share_of_attempts |")
        lines.append("|---|---:|---:|---:|")
        for row in total_rows:
            if row["scope"] != "all_phases_total":
                continue
            lines.append(
                f"| {row['category']} | {row['count']} | {float(row['share_of_non_submitted']):.2%} | {float(row['share_of_attempts']):.2%} |"
            )

    lines.append("")
    lines.append("## Rerun-needed signal thresholds")
    lines.append("")
    lines.append("- red: infra_non_submitted >= 10 OR infra_share_of_attempts >= 3% OR infra_share_of_non_submitted >= 20%")
    lines.append("- yellow: infra_non_submitted >= 3 OR infra_share_of_attempts >= 1% OR infra_share_of_non_submitted >= 10%")
    lines.append("- green: otherwise")

    lines.append("")
    lines.append("## Model/run quick check")
    lines.append("")
    lines.append("| model | run | attempts_total | non_submitted_total | infra_non_submitted | infra_share_of_attempts | infra_share_of_non_submitted | rerun_signal |")
    lines.append("|---|---:|---:|---:|---:|---:|---:|---|")
    for row in top_signals:
        lines.append(
            "| {model} | {run} | {attempts_total} | {non_submitted_total} | {infra_non_submitted} | {infra_attempts:.2%} | {infra_non_sub:.2%} | {signal} |".format(
                model=row["model"],
                run=row["run"],
                attempts_total=row["attempts_total"],
                non_submitted_total=row["non_submitted_total"],
                infra_non_submitted=row["infra_non_submitted"],
                infra_attempts=float(row["infra_share_of_attempts"]),
                infra_non_sub=float(row["infra_share_of_non_submitted"]),
                signal=row["rerun_signal"],
            )
        )

    lines.append("")
    lines.append("## Output files")
    lines.append("")
    lines.append(f"- {by_model_run_csv.relative_to(REPO_ROOT)}")
    lines.append(f"- {totals_csv.relative_to(REPO_ROOT)}")
    lines.append(f"- {signals_csv.relative_to(REPO_ROOT)}")
    lines.append(f"- {summary_json.relative_to(REPO_ROOT)}")

    markdown_path.parent.mkdir(parents=True, exist_ok=True)
    markdown_path.write_text("\n".join(lines) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description="Categorize non-submitted failures.")
    parser.add_argument("--phase", default=None, help="Single phase folder under phases/.")
    parser.add_argument(
        "--phases",
        default=None,
        help="Comma-separated phase folders under phases/ for consolidated output.",
    )
    parser.add_argument(
        "--out-dir",
        default=str(REPO_ROOT / "tools" / "viz" / "data"),
        help="Directory for machine-readable outputs.",
    )
    parser.add_argument(
        "--markdown",
        default=None,
        help="Output markdown report path.",
    )
    parser.add_argument(
        "--output-prefix",
        default=None,
        help="Output filename prefix. Defaults to phase name for single phase, all_phases for multi-phase.",
    )
    parser.add_argument(
        "--include-results-glob",
        action="store_true",
        help="Also include non-canonical results* trees (default is canonical results/ only).",
    )
    args = parser.parse_args()

    if args.phases:
        phases = [p.strip() for p in args.phases.split(",") if p.strip()]
    else:
        phases = [args.phase or "phase4-refactoring"]

    if not phases:
        raise SystemExit("no phases supplied")

    if len(phases) == 1:
        default_prefix = phases[0]
        default_markdown = REPO_ROOT / "phases" / phases[0] / "FAILURE_DIAGNOSTICS.md"
    else:
        default_prefix = "all_phases"
        default_markdown = REPO_ROOT / "docs" / "reports" / "ALL_PHASES_FAILURE_DIAGNOSTICS.md"

    output_prefix = args.output_prefix or default_prefix
    out_dir = Path(args.out_dir).resolve()
    markdown_path = Path(args.markdown).resolve() if args.markdown else default_markdown.resolve()

    counters, attempts_total, non_submitted_total = collect_many(
        phases=phases,
        include_results_glob=args.include_results_glob,
    )
    if not counters:
        raise SystemExit(f"no attempts.jsonl data found for phases {phases}")

    write_outputs(
        phases=phases,
        counters=counters,
        attempts_total=attempts_total,
        non_submitted_total=non_submitted_total,
        out_dir=out_dir,
        markdown_path=markdown_path,
        output_prefix=output_prefix,
    )
    print(f"wrote outputs under {out_dir}")
    print(f"wrote markdown report {markdown_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())