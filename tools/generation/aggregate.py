#!/usr/bin/env python3
"""Aggregate per-cell outcome.csv files into phases/<phase-id>/results/aggregate.csv.

Run after build_outcomes.py has produced per-(model × run) outcome.csv files.

Output columns:
    target_id, model_id, run_index, compile_pass, test_pass, covers_target

Schema is stable across phases — RESULTS.md and downstream stats scripts
read it.
"""
from __future__ import annotations
import argparse
import csv
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
FIELDS = ["target_id", "model_id", "run_index", "compile_pass", "test_pass", "covers_target"]


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True)
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    results_dir = phase_dir / "results"
    if not results_dir.is_dir():
        print(f"error: {results_dir} does not exist", file=sys.stderr)
        return 2

    out = results_dir / "aggregate.csv"
    n = 0
    with out.open("w", newline="") as fh:
        w = csv.DictWriter(fh, fieldnames=FIELDS)
        w.writeheader()
        for outcome in sorted(results_dir.rglob("run_*/outcome.csv")):
            run_dir = outcome.parent
            run_index = int(run_dir.name.removeprefix("run_"))
            model_id = run_dir.parent.name.replace("__", "/")
            with outcome.open() as src:
                for row in csv.DictReader(src):
                    w.writerow({
                        "target_id": row["target_id"],
                        "model_id": model_id,
                        "run_index": run_index,
                        "compile_pass": row.get("compile_pass", ""),
                        "test_pass": row.get("test_pass", ""),
                        "covers_target": row.get("covers_target", ""),
                    })
                    n += 1
    print(f"wrote {out} ({n} rows)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
