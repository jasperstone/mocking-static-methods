#!/usr/bin/env python3
"""Build per-cell outcome.csv from build/test logs + cobertura output.

This runs in CI AFTER the generated test files have been compiled and
tested by the per-repo coverage workflow. It joins:

    phases/<phase-id>/results/{model}/run_{i}/generated_tests/{repo}/{tid}.cs
    phases/<phase-id>/results/{model}/run_{i}/compile/{tid}.log    (iff failed)
    phases/<phase-id>/results/{model}/run_{i}/runtime/{tid}.log    (iff failed)
    phases/<phase-id>/coverage/{repo}/coverage.cobertura.xml

into outcome.csv: one row per attempted target with three boolean columns:

    compile_pass:   the generated test file compiled
    test_pass:      the generated test file's tests all passed
    covers_target:  the cobertura entry for (target.file, target.line) has hits>0
                    after this cell's tests ran

Phase 2's headline metric per cell is `sum(test_pass AND covers_target) /
len(targets.csv)`. The aggregate.py step joins all 25 cells' outcome.csv
into results/aggregate.csv for cross-cell analysis.
"""
from __future__ import annotations
import argparse
import csv
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]


def load_coverage_map(cov_dir: Path) -> dict[tuple[str, int], int]:
    """{(file_lower, line): max_hits} across all cobertura files in cov_dir."""
    out: dict[tuple[str, int], int] = {}
    for f in cov_dir.rglob("coverage.cobertura.xml"):
        try:
            root = ET.parse(f).getroot()
        except ET.ParseError:
            continue
        for cls in root.iter("class"):
            fname = cls.get("filename", "").replace("\\", "/").lstrip("/").lower()
            if not fname:
                continue
            child_lines = cls.find("lines")
            if child_lines is None:
                continue
            for line in child_lines.findall("line"):
                try:
                    num = int(line.get("number", "0"))
                    hits = int(line.get("hits", "0"))
                except ValueError:
                    continue
                if num <= 0:
                    continue
                key = (fname, num)
                if hits > out.get(key, -1):
                    out[key] = hits
    return out


def covers(cov: dict[tuple[str, int], int], file_path: str, line: int) -> bool:
    key = (file_path.replace("\\", "/").lstrip("/").lower(), line)
    return cov.get(key, 0) > 0


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True)
    ap.add_argument("--model", required=True, help="model id, slashes preserved")
    ap.add_argument("--run-index", type=int, required=True)
    ap.add_argument("--targets-csv", default=None,
                    help="path to targets.csv (defaults to targets/{set}/targets.csv)")
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    cell_dir = phase_dir / "results" / args.model.replace("/", "__") / f"run_{args.run_index}"
    if not cell_dir.is_dir():
        print(f"error: {cell_dir} does not exist", file=sys.stderr)
        return 2

    targets_csv = Path(args.targets_csv) if args.targets_csv else None
    if targets_csv is None:
        # Default to the version pinned by phase.lock.yaml — but loading YAML
        # without a dep is annoying, and the workflow always knows the version,
        # so we expect it to pass --targets-csv explicitly. Fall back to v1.
        targets_csv = REPO_ROOT / "targets" / "v1" / "targets.csv"
    if not targets_csv.is_file():
        print(f"error: targets.csv not found at {targets_csv}", file=sys.stderr)
        return 2

    cov = load_coverage_map(phase_dir / "coverage")

    compile_logs = {p.stem.replace("_", ":", 1): p for p in (cell_dir / "compile").rglob("*.log")} if (cell_dir / "compile").is_dir() else {}
    runtime_logs = {p.stem.replace("_", ":", 1): p for p in (cell_dir / "runtime").rglob("*.log")} if (cell_dir / "runtime").is_dir() else {}
    # generated_tests/{repo}/{target_id_safe}/block_NN.cs — presence of the
    # directory means the model produced at least one csharp block for that target.
    if (cell_dir / "generated_tests").is_dir():
        generated = {
            d.name.replace("_", ":", 1)
            for repo_dir in (cell_dir / "generated_tests").iterdir()
            if repo_dir.is_dir()
            for d in repo_dir.iterdir()
            if d.is_dir()
        }
    else:
        generated = set()

    out_path = cell_dir / "outcome.csv"
    n = 0
    with targets_csv.open() as ths, out_path.open("w", newline="") as outf:
        w = csv.DictWriter(outf, fieldnames=["target_id", "compile_pass", "test_pass", "covers_target"])
        w.writeheader()
        for row in csv.DictReader(ths):
            tid = row["target_id"]
            if tid not in generated:
                # No file produced — neither compile nor test ran.
                w.writerow({"target_id": tid, "compile_pass": "0", "test_pass": "0", "covers_target": "0"})
                n += 1
                continue
            compile_pass = "0" if tid in compile_logs else "1"
            test_pass = "0" if (tid in compile_logs or tid in runtime_logs) else "1"
            covers_target = "1" if covers(cov, row["file"], int(row["line"])) else "0"
            w.writerow({
                "target_id": tid,
                "compile_pass": compile_pass,
                "test_pass": test_pass,
                "covers_target": covers_target,
            })
            n += 1
    print(f"wrote {out_path} ({n} rows)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
