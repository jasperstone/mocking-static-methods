#!/usr/bin/env python3
"""Aggregate per-(phase, model) totals from raw phase result jsonl into a CSV
for the R viz layer.

Re-run this whenever new phase results land:

    python3 tools/viz/aggregate_phase_results.py

Outputs:
    tools/viz/data/per_model_phase.csv

Columns: phase, model, attempts, submitted, compile_ok, run_ok,
         prompt_tokens, completion_tokens, cost_usd

Sources:
  * `phases/phase2-agentic/results/<model>/run_*/attempts.jsonl`  (token counts,
    submitted flag)
  * `phases/phase2-agentic/results/<model>/run_*/evaluation.jsonl` (compile_ok,
    run_ok). Joined on (target_id, run_index, model_id).
  * `phases/phase2-singleshot/results/...` — same shape if/when it ever fills.
  * `phases/phase3-agentic-loop/results/...` — same shape as phase 2;
    attempts.jsonl carries `total_prompt_tokens` / `total_completion_tokens`
    so cost_usd is computed the same way.
"""
from __future__ import annotations

import csv
import json
import math
from collections import defaultdict
from pathlib import Path

# Reuse the canonical price table — do NOT duplicate.
import sys

REPO_ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO_ROOT))
from tools.cost.estimate import PRICES  # noqa: E402

OUT_CSV = REPO_ROOT / "tools" / "viz" / "data" / "per_model_phase.csv"
PER_MODEL_REPO = REPO_ROOT / "tools" / "viz" / "data" / "per_model_repo.csv"

# Phases that have raw attempts/evaluation jsonl under
# `phases/<phase>/results/<model>/run_*/`.
RAW_PHASES = ["phase2-agentic", "phase2-singleshot", "phase3-agentic-loop"]


def aggregate_raw_phase(phase: str) -> dict[str, dict]:
    """Walk attempts.jsonl + evaluation.jsonl under phases/<phase>/results/."""
    phase_dir = REPO_ROOT / "phases" / phase / "results"
    if not phase_dir.is_dir():
        return {}

    per_model: dict[str, dict] = defaultdict(
        lambda: {
            "attempts": 0,
            "submitted": 0,
            "compile_ok": 0,
            "run_ok": 0,
            "prompt_tokens": 0,
            "completion_tokens": 0,
        }
    )

    # Attempts: tokens + submitted flag.
    # NB: glob matches sibling results dirs too (e.g. `results_v1_oldprompt/`)
    # so totals match `tools/cost/estimate.py --phase phase2-agentic` and the
    # published table in `phases/phase2-agentic/COSTS.md`.
    for path in sorted(phase_dir.parent.glob("results*/*/run_*/attempts.jsonl")):
        for line in path.open():
            r = json.loads(line)
            m = r.get("model_id", "?")
            t = per_model[m]
            t["attempts"] += 1
            t["submitted"] += int(bool(r.get("submitted")))
            t["prompt_tokens"] += int(r.get("total_prompt_tokens") or 0)
            t["completion_tokens"] += int(r.get("total_completion_tokens") or 0)

    # Evaluations: compile_ok / run_ok. Keyed on (target_id, run_index, model_id)
    # to avoid double-counting if a target appears more than once. The model
    # totals are simply sums across all unique eval rows for that model.
    seen_eval: set[tuple[str, int, str]] = set()
    for path in sorted(phase_dir.parent.glob("results*/*/run_*/evaluation.jsonl")):
        for line in path.open():
            r = json.loads(line)
            key = (
                r.get("target_id", "?"),
                int(r.get("run_index") or 0),
                r.get("model_id", "?"),
            )
            if key in seen_eval:
                continue
            seen_eval.add(key)
            m = r.get("model_id", "?")
            t = per_model[m]
            t["compile_ok"] += int(bool(r.get("compile_ok")))
            t["run_ok"] += int(bool(r.get("run_ok")))

    # Cost.
    for m, t in per_model.items():
        price = PRICES.get(m)
        if price is None:
            t["cost_usd"] = math.nan
        else:
            t["cost_usd"] = (
                t["prompt_tokens"] / 1_000_000 * price["in"]
                + t["completion_tokens"] / 1_000_000 * price["out"]
            )

    return per_model


def aggregate_phase3() -> dict[str, dict]:
    """Phase 3 raw attempts/eval are not committed; synthesise model totals
    from the already-aggregated per-repo CSV. Cost is NaN — there is no
    per-attempt token data available for phase 3 in this repo snapshot.
    """
    if not PER_MODEL_REPO.is_file():
        return {}
    per_model: dict[str, dict] = defaultdict(
        lambda: {
            "attempts": 0,  # phase3 raw attempt count not available; use submitted as a floor
            "submitted": 0,
            "compile_ok": 0,
            "run_ok": 0,
            "prompt_tokens": 0,
            "completion_tokens": 0,
            "cost_usd": math.nan,
        }
    )
    with PER_MODEL_REPO.open() as f:
        rdr = csv.DictReader(f)
        for row in rdr:
            m = row["model"]
            t = per_model[m]
            sub = int(row["submitted"])
            t["submitted"] += sub
            t["attempts"] += sub  # floor; raw attempt count not preserved
            t["compile_ok"] += int(row["compile_ok"])
            t["run_ok"] += int(row["run_ok"])
    return per_model


def main() -> int:
    OUT_CSV.parent.mkdir(parents=True, exist_ok=True)

    rows: list[dict] = []
    for phase in RAW_PHASES:
        per_model = aggregate_raw_phase(phase)
        for m, t in sorted(per_model.items()):
            rows.append({"phase": phase, "model": m, **t})

    # Phase 3 fallback path (synthesise from per_model_repo.csv) only fires if
    # the raw aggregation found nothing.
    if not any(r["phase"] == "phase3-agentic-loop" for r in rows):
        p3 = aggregate_phase3()
        for m, t in sorted(p3.items()):
            rows.append({"phase": "phase3-agentic-loop", "model": m, **t})

    fields = [
        "phase",
        "model",
        "attempts",
        "submitted",
        "compile_ok",
        "run_ok",
        "prompt_tokens",
        "completion_tokens",
        "cost_usd",
    ]
    with OUT_CSV.open("w", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields)
        w.writeheader()
        for r in rows:
            out = dict(r)
            c = out.get("cost_usd")
            if c is None or (isinstance(c, float) and math.isnan(c)):
                out["cost_usd"] = ""
            else:
                out["cost_usd"] = f"{c:.4f}"
            w.writerow(out)

    # Echo a quick summary per phase.
    print(f"wrote {OUT_CSV.relative_to(REPO_ROOT)} ({len(rows)} rows)")
    by_phase: dict[str, dict] = defaultdict(
        lambda: {"attempts": 0, "submitted": 0, "compile_ok": 0, "run_ok": 0, "cost_usd": 0.0, "has_cost": False}
    )
    for r in rows:
        p = by_phase[r["phase"]]
        for k in ("attempts", "submitted", "compile_ok", "run_ok"):
            p[k] += r[k]
        c = r["cost_usd"]
        if isinstance(c, float) and not math.isnan(c):
            p["cost_usd"] += c
            p["has_cost"] = True
    for phase, t in by_phase.items():
        cost_str = f"${t['cost_usd']:.2f}" if t["has_cost"] else "n/a"
        print(
            f"  {phase}: attempts={t['attempts']} submitted={t['submitted']} "
            f"compile_ok={t['compile_ok']} run_ok={t['run_ok']} cost={cost_str}"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
