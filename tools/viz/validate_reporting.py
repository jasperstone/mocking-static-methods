#!/usr/bin/env python3
"""Validate that reporting preserves every raw attempt and failure category."""

from __future__ import annotations

import csv
import json
from collections import defaultdict
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = REPO_ROOT / "tools" / "viz" / "data"
PHASES = ("phase2-agentic", "phase3-agentic-loop", "phase4-refactoring")
INFRA_CATEGORIES = {
    "timeout/connection",
    "auth/access",
    "rate-limit",
    "server-5xx",
    "api-version-unsupported",
}


def reported_outcome_counts() -> dict[tuple[str, str], int]:
    counts: dict[tuple[str, str], int] = defaultdict(int)
    submitted_keys: set[tuple[str, str, int, str]] = set()
    for phase in PHASES:
        phase_dir = REPO_ROOT / "phases" / phase
        for path in phase_dir.glob("results*/*/run_*/attempts.jsonl"):
            with path.open(encoding="utf-8") as stream:
                for line in stream:
                    if not line.strip():
                        continue
                    record = json.loads(line)
                    model = record.get("model_id") or path.parent.parent.name
                    counts[(phase, model)] += 1
                    if record.get("submitted"):
                        submitted_keys.add(
                            (
                                phase,
                                record.get("target_id", "?"),
                                int(record.get("run_index") or 0),
                                model,
                            )
                        )

        if phase == "phase3-agentic-loop":
            evaluator_keys: set[tuple[str, str, int, str]] = set()
            for path in phase_dir.glob("results*/*/run_*/evaluation.jsonl"):
                with path.open(encoding="utf-8") as stream:
                    for line in stream:
                        if not line.strip():
                            continue
                        record = json.loads(line)
                        model = record.get("model_id") or path.parent.parent.name
                        evaluator_keys.add(
                            (
                                phase,
                                record.get("target_id", "?"),
                                int(record.get("run_index") or 0),
                                model,
                            )
                        )
            for key in evaluator_keys - submitted_keys:
                counts[(phase, key[3])] += 1
    return counts


def read_phase_summary() -> dict[tuple[str, str], dict[str, str]]:
    path = DATA_DIR / "per_model_phase.csv"
    with path.open(newline="", encoding="utf-8") as stream:
        return {
            (row["phase"], row["model"]): row
            for row in csv.DictReader(stream)
        }


def validate_failure_categories(
    phase_summary: dict[tuple[str, str], dict[str, str]],
) -> None:
    path = DATA_DIR / "all_phases_failure_categories_by_model_run.csv"
    groups: dict[tuple[str, str, str], list[dict[str, str]]] = defaultdict(list)
    with path.open(newline="", encoding="utf-8") as stream:
        for row in csv.DictReader(stream):
            groups[(row["phase"], row["model"], row["run"])].append(row)

    infra_by_model: dict[tuple[str, str], int] = defaultdict(int)
    for key, rows in groups.items():
        categories = {row["category"] for row in rows}
        missing = INFRA_CATEGORIES - categories
        assert not missing, f"{key} omits infrastructure categories: {sorted(missing)}"

        expected = int(rows[0]["non_submitted_total"])
        actual = sum(int(row["count"]) for row in rows)
        assert actual == expected, (
            f"{key} failure categories total {actual}, expected {expected}"
        )

        for row in rows:
            if row["category"] in INFRA_CATEGORIES:
                infra_by_model[(row["phase"], row["model"])] += int(row["count"])

    for key, row in phase_summary.items():
        expected = int(row["infrastructure_failures"])
        actual = infra_by_model[key]
        assert actual == expected, (
            f"{key} infrastructure total {actual}, summary reports {expected}"
        )


def main() -> int:
    raw_counts = reported_outcome_counts()
    phase_summary = read_phase_summary()

    assert set(phase_summary) == set(raw_counts), (
        "phase/model summary keys do not match raw attempt keys"
    )
    for key, expected in raw_counts.items():
        actual = int(phase_summary[key]["attempts"])
        assert actual == expected, f"{key} reports {actual} attempts, expected {expected}"

    validate_failure_categories(phase_summary)
    print(
        f"validated {sum(raw_counts.values())} attempts across "
        f"{len(raw_counts)} phase/model rows; no failure categories were dropped"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
