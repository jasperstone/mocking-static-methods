#!/usr/bin/env python3
"""Plan the shard matrix for phase generation workflows.

Reads from env:
  TARGET_SET     - e.g. "v2"
  RUNS_PER_CELL  - integer string, e.g. "3"
    RUN_INDEX_START - first run index, e.g. "1"
  MODELS         - "all" or comma-separated model ids
  REPOS          - "all" or comma-separated repo names
    TARGET_IDS     - optional comma-separated exact target_ids to include
    CHUNK_SIZE     - optional positive integer; splits each repo's target_ids into
                                     stable slices of this size

Writes a single-line JSON object to stdout. Default output preserves the legacy
shape: {"include": [{model, repo, run_index}, ...]}. When TARGET_IDS and/or
CHUNK_SIZE are provided, rows gain additive chunk fields.
"""

from __future__ import annotations

import csv
import json
import os
import sys
from collections import defaultdict

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


def _parse_csv_env(name: str, default: str = "") -> list[str]:
    raw = os.environ.get(name, default).strip()
    if not raw or raw.lower() == "none":
        return []
    return [item.strip() for item in raw.split(",") if item.strip()]


def _parse_optional_positive_int(name: str) -> int | None:
    raw = os.environ.get(name, "").strip()
    if not raw or raw.lower() == "none":
        return None
    value = int(raw)
    if value <= 0:
        raise ValueError(f"{name} must be a positive integer; got {raw!r}")
    return value


def _chunked(values: list[str], size: int | None) -> list[list[str]]:
    if not values:
        return []
    if size is None:
        return [values]
    return [values[index:index + size] for index in range(0, len(values), size)]


def main() -> int:
    ts = os.environ["TARGET_SET"]
    runs = int(os.environ.get("RUNS_PER_CELL", "1"))
    # Optional: shift the run_index window so a follow-up sweep can produce
    # only run_2/run_3 without redoing run_1. Default 1 keeps prior behavior.
    start = int(os.environ.get("RUN_INDEX_START", "1"))

    want_models = os.environ.get("MODELS", "all").strip()
    models = FULL_PANEL if want_models == "all" else [m.strip() for m in want_models.split(",") if m.strip()]

    want_repos = os.environ.get("REPOS", "all").strip()
    target_ids = _parse_csv_env("TARGET_IDS")
    chunk_size = _parse_optional_positive_int("CHUNK_SIZE")

    with open(f"targets/{ts}/targets.csv", newline="") as fh:
        rows = list(csv.DictReader(fh))

    all_repos = sorted({row["repo"] for row in rows})
    if want_repos == "all":
        wanted_repos = None
    else:
        wanted_repos = {r.strip() for r in want_repos.split(",") if r.strip()}

    target_whitelist = set(target_ids) if target_ids else None
    if target_whitelist:
        known_target_ids = {row["target_id"] for row in rows}
        unknown = sorted(target_whitelist - known_target_ids)
        if unknown:
            raise ValueError(f"unknown target_ids requested: {', '.join(unknown[:10])}")

    grouped_target_ids: dict[str, list[str]] = defaultdict(list)
    for row in rows:
        repo = row["repo"]
        if wanted_repos is not None and repo not in wanted_repos:
            continue
        if target_whitelist is not None and row["target_id"] not in target_whitelist:
            continue
        grouped_target_ids[repo].append(row["target_id"])

    if wanted_repos is None:
        repos = sorted(grouped_target_ids) if target_whitelist else all_repos
    else:
        missing_repos = sorted(wanted_repos - set(all_repos))
        if missing_repos:
            raise ValueError(f"unknown repos requested: {', '.join(missing_repos)}")
        repos = [repo for repo in all_repos if repo in wanted_repos]

    if target_whitelist is not None:
        repos = [repo for repo in repos if grouped_target_ids.get(repo)]

    # Interleave shards by repo/run first, then model. This avoids launching
    # many same-model shards back-to-back at high parallelism, which can trip
    # model-specific provider rate limits (notably on inference-surface models).
    include = []
    for repo in repos:
        repo_target_ids = grouped_target_ids.get(repo, [])
        repo_chunks = _chunked(repo_target_ids, chunk_size)
        if target_whitelist is not None and not repo_chunks:
            continue
        if target_whitelist is None and chunk_size is None:
            repo_chunks = [[]]

        chunk_count = len(repo_chunks) if repo_chunks else 1
        for run_index in range(start, start + runs):
            for model in models:
                for chunk_index, chunk in enumerate(repo_chunks, start=1):
                    shard = {"model": model, "repo": repo, "run_index": run_index}
                    if chunk:
                        shard["target_ids"] = ",".join(chunk)
                    if chunk_size is not None:
                        shard["chunk_index"] = chunk_index
                        shard["chunk_count"] = chunk_count
                        shard["target_count"] = len(chunk)
                        shard["artifact_suffix"] = f"-chunk{chunk_index}of{chunk_count}"
                        shard["job_label_suffix"] = f" chunk {chunk_index}/{chunk_count}"
                    include.append(shard)

    json.dump({"include": include}, sys.stdout)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
