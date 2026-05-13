#!/usr/bin/env python3
"""Plan the (model x repo x run_index) shard matrix for phase2-generate.yml.

Reads from env:
  TARGET_SET     - e.g. "v2"
  RUNS_PER_CELL  - integer string, e.g. "3"
  MODELS         - "all" or comma-separated model ids
  REPOS          - "all" or comma-separated repo names

Writes a single-line JSON object to stdout: {"include": [ {model, repo, run_index}, ... ]}
"""

from __future__ import annotations

import csv
import json
import os
import sys

FULL_PANEL = [
    # gpt-5-codex removed after phase 2: 82% of spend, 17.8% submission rate.
    # Baseline data preserved in phases/phase2-agentic/results/gpt-5-codex/.
    "gpt-4.1-mini",
    "gpt-4.1-nano",
    "phi-4",
    "codestral-2501",
    "grok-4-1-fast",
    "llama-3.3-70b-instruct",
]


def main() -> int:
    ts = os.environ["TARGET_SET"]
    runs = int(os.environ.get("RUNS_PER_CELL", "1"))

    want_models = os.environ.get("MODELS", "all").strip()
    models = FULL_PANEL if want_models == "all" else [m.strip() for m in want_models.split(",") if m.strip()]

    want_repos = os.environ.get("REPOS", "all").strip()
    all_repos = sorted({r["repo"] for r in csv.DictReader(open(f"targets/{ts}/targets.csv"))})
    repos = all_repos if want_repos == "all" else [r.strip() for r in want_repos.split(",") if r.strip()]

    include = [
        {"model": m, "repo": r, "run_index": i}
        for m in models
        for r in repos
        for i in range(1, runs + 1)
    ]
    json.dump({"include": include}, sys.stdout)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
