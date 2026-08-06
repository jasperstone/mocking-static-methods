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
    * `phases/phase4-refactoring/results/...` — may have attempts without
        evaluation.jsonl; compile/run fall back to `final_compile_ok` /
        `final_run_ok` when evaluator rows are absent.
"""
from __future__ import annotations

import csv
import json
import math
import re
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
RAW_PHASES = [
    "phase2-agentic",
    "phase2-singleshot",
    "phase3-agentic-loop",
    "phase4-refactoring",
]

# Non-evaluable tooling failures: these are execution-surface problems
# (auth, rate-limit, transport, service) and must not be interpreted as
# model-quality failures.
TOOLING_FAILURE_RE = re.compile(
    r"(\b401\b|\b403\b|\b408\b|\b429\b|\b500\b|\b502\b|\b503\b|\b504\b|"
    r"timeout|timed out|rate.?limit|access denied|invalid subscription key|"
    r"network error|connection)",
    re.IGNORECASE,
)


def is_tooling_failure(rec: dict) -> bool:
    if rec.get("submitted"):
        return False
    text = " ".join(
        str(rec.get(k, ""))
        for k in ("error", "halt_reason", "error_type", "final_error_type")
    )
    return bool(TOOLING_FAILURE_RE.search(text))


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
            "tooling_excluded": 0,
            "prompt_tokens": 0,
            "completion_tokens": 0,
        }
    )

    # Attempts: tokens + submitted flag (+ fallback compile/run where evaluator
    # rows are missing, e.g. some phase-4 snapshots).
    attempt_outcomes: dict[tuple[str, int, str], tuple[str, int, int]] = {}
    # NB: glob matches sibling results dirs too (e.g. `results_v1_oldprompt/`)
    # so totals match `tools/cost/estimate.py --phase phase2-agentic` and the
    # published table in `phases/phase2-agentic/COSTS.md`.
    for path in sorted(phase_dir.parent.glob("results*/*/run_*/attempts.jsonl")):
        for line in path.open():
            r = json.loads(line)
            m = r.get("model_id", "?")
            t = per_model[m]
            if is_tooling_failure(r):
                t["tooling_excluded"] += 1
                continue
            t["attempts"] += 1
            t["submitted"] += int(bool(r.get("submitted")))
            t["prompt_tokens"] += int(r.get("total_prompt_tokens") or 0)
            t["completion_tokens"] += int(r.get("total_completion_tokens") or 0)

            # Fallback outcomes are evaluative only for submitted rows.
            if r.get("submitted"):
                key = (
                    r.get("target_id", "?"),
                    int(r.get("run_index") or 0),
                    m,
                )
                attempt_outcomes[key] = (
                    m,
                    int(bool(r.get("final_compile_ok"))),
                    int(bool(r.get("final_run_ok"))),
                )

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
            # Guardrail: only score evaluator rows for keys that produced a
            # submitted candidate in attempts.jsonl. This prevents tooling-only
            # adapter failures (e.g., 429/auth/timeout) from leaking in as
            # compile/run failures.
            # Phase 3 archives can contain evaluator rows where attempts.jsonl
            # does not preserve submitted=true metadata; in that case evaluator
            # rows are the authoritative signal.
            if phase != "phase3-agentic-loop" and key not in attempt_outcomes:
                continue
            if key in seen_eval:
                continue
            seen_eval.add(key)
            m = r.get("model_id", "?")
            t = per_model[m]
            if phase == "phase3-agentic-loop" and key not in attempt_outcomes:
                # Preserve a conservative attempts/submitted floor from unique
                # evaluator keys when attempts metadata is incomplete.
                t["submitted"] += 1
                t["attempts"] += 1
            t["compile_ok"] += int(bool(r.get("compile_ok")))
            t["run_ok"] += int(bool(r.get("run_ok")))

    # Fallback for runs that have attempts rows but no evaluator row.
    for key, (m, c_ok, r_ok) in attempt_outcomes.items():
        if key in seen_eval:
            continue
        t = per_model[m]
        t["compile_ok"] += c_ok
        t["run_ok"] += r_ok

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


def aggregate_raw_phase_per_repo(phase: str) -> dict[tuple[str, str], dict]:
    """Walk raw JSONL and key totals by (model, repo). Repo is parsed from
    `target_id` (format: `<repo>:<NNNN>`). Returns sums; percentages are
    computed at write time.
    """
    phase_dir = REPO_ROOT / "phases" / phase / "results"
    if not phase_dir.is_dir():
        return {}

    per: dict[tuple[str, str], dict] = defaultdict(
        lambda: {"submitted": 0, "compile_ok": 0, "run_ok": 0}
    )

    for path in sorted(phase_dir.parent.glob("results*/*/run_*/attempts.jsonl")):
        for line in path.open():
            r = json.loads(line)
            m = r.get("model_id", "?")
            tid = r.get("target_id", "?")
            repo = tid.split(":", 1)[0] if ":" in tid else tid.split("_", 1)[0]
            per[(m, repo)]["submitted"] += int(bool(r.get("submitted")))

    # Keep fallback outcomes from attempts for phases where evaluation rows are
    # absent; evaluator rows take precedence when present.
    attempt_outcomes: dict[tuple[str, int, str], tuple[str, str, int, int]] = {}
    for path in sorted(phase_dir.parent.glob("results*/*/run_*/attempts.jsonl")):
        for line in path.open():
            r = json.loads(line)
            if is_tooling_failure(r) or not r.get("submitted"):
                continue
            m = r.get("model_id", "?")
            tid = r.get("target_id", "?")
            repo = tid.split(":", 1)[0] if ":" in tid else tid.split("_", 1)[0]
            key = (tid, int(r.get("run_index") or 0), m)
            attempt_outcomes[key] = (
                m,
                repo,
                int(bool(r.get("final_compile_ok"))),
                int(bool(r.get("final_run_ok"))),
            )

    seen: set[tuple[str, int, str]] = set()
    for path in sorted(phase_dir.parent.glob("results*/*/run_*/evaluation.jsonl")):
        for line in path.open():
            r = json.loads(line)
            key = (r.get("target_id", "?"), int(r.get("run_index") or 0), r.get("model_id", "?"))
            # Guardrail: only score evaluator rows for keys with submitted
            # attempt candidates; tooling-only attempts should be excluded.
            if phase != "phase3-agentic-loop" and key not in attempt_outcomes:
                continue
            if key in seen:
                continue
            seen.add(key)
            m = r.get("model_id", "?")
            tid = r.get("target_id", "?")
            repo = tid.split(":", 1)[0] if ":" in tid else tid.split("_", 1)[0]
            if phase == "phase3-agentic-loop" and key not in attempt_outcomes:
                per[(m, repo)]["submitted"] += 1
            per[(m, repo)]["compile_ok"] += int(bool(r.get("compile_ok")))
            per[(m, repo)]["run_ok"] += int(bool(r.get("run_ok")))

    for key, (m, repo, c_ok, r_ok) in attempt_outcomes.items():
        if key in seen:
            continue
        per[(m, repo)]["compile_ok"] += c_ok
        per[(m, repo)]["run_ok"] += r_ok

    return per


def write_per_model_repo(phase: str) -> int:
    """Regenerate per_model_repo.csv from raw JSONL of the given phase."""
    per = aggregate_raw_phase_per_repo(phase)
    if not per:
        return 0
    PER_MODEL_REPO.parent.mkdir(parents=True, exist_ok=True)
    fields = ["model", "repo", "submitted", "compile_ok", "run_ok", "compile_pct", "run_pct"]
    with PER_MODEL_REPO.open("w", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields, lineterminator="\n")
        w.writeheader()
        for (m, repo) in sorted(per.keys()):
            t = per[(m, repo)]
            sub = t["submitted"]
            cp = (100.0 * t["compile_ok"] / sub) if sub else 0.0
            rp = (100.0 * t["run_ok"] / sub) if sub else 0.0
            w.writerow({
                "model": m,
                "repo": repo,
                "submitted": sub,
                "compile_ok": t["compile_ok"],
                "run_ok": t["run_ok"],
                "compile_pct": f"{cp:.1f}",
                "run_pct": f"{rp:.1f}",
            })
    return len(per)


def aggregate_phase3() -> dict[str, dict]:
    """Synthesize phase-3 model totals from phase-3 raw per-repo aggregates.

    This avoids cross-phase contamination from `per_model_repo.csv`, which is
    refreshed from the latest available phase (typically phase 4).
    Cost is NaN — there is no per-attempt token data available here.
    """
    per_repo = aggregate_raw_phase_per_repo("phase3-agentic-loop")
    if not per_repo:
        return {}
    per_model: dict[str, dict] = defaultdict(
        lambda: {
            "attempts": 0,  # phase3 raw attempt count not available; use submitted as a floor
            "submitted": 0,
            "compile_ok": 0,
            "run_ok": 0,
            "tooling_excluded": 0,
            "prompt_tokens": 0,
            "completion_tokens": 0,
            "cost_usd": math.nan,
        }
    )
    for (m, _repo), row in per_repo.items():
        t = per_model[m]
        sub = int(row.get("submitted") or 0)
        t["submitted"] += sub
        t["attempts"] += sub  # floor; raw attempt count not preserved
        t["compile_ok"] += int(row.get("compile_ok") or 0)
        t["run_ok"] += int(row.get("run_ok") or 0)
    return per_model


def load_existing_phase_rows() -> dict[tuple[str, str], dict]:
    """Load existing per-model phase rows for selective carry-forward fallback.

    Used to avoid overwriting previously evaluable rows with zero-submitted
    tooling-outage reruns.
    """
    if not OUT_CSV.is_file():
        return {}
    out: dict[tuple[str, str], dict] = {}
    with OUT_CSV.open() as f:
        rdr = csv.DictReader(f)
        for row in rdr:
            key = (row.get("phase", ""), row.get("model", ""))
            if not all(key):
                continue
            try:
                out[key] = {
                    "phase": key[0],
                    "model": key[1],
                    "attempts": int(row.get("attempts") or 0),
                    "submitted": int(row.get("submitted") or 0),
                    "compile_ok": int(row.get("compile_ok") or 0),
                    "run_ok": int(row.get("run_ok") or 0),
                    "tooling_excluded": int(row.get("tooling_excluded") or 0),
                    "prompt_tokens": int(row.get("prompt_tokens") or 0),
                    "completion_tokens": int(row.get("completion_tokens") or 0),
                    "cost_usd": float(row["cost_usd"]) if row.get("cost_usd") else math.nan,
                }
            except ValueError:
                # Skip malformed legacy rows rather than failing aggregation.
                continue
    return out


def main() -> int:
    OUT_CSV.parent.mkdir(parents=True, exist_ok=True)
    existing_rows = load_existing_phase_rows()
    phase3_synth = aggregate_phase3()

    rows: list[dict] = []
    for phase in RAW_PHASES:
        per_model = aggregate_raw_phase(phase)
        for m, t in sorted(per_model.items()):
            row = {"phase": phase, "model": m, **t}
            rows.append(row)

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
        "tooling_excluded",
        "prompt_tokens",
        "completion_tokens",
        "cost_usd",
    ]
    with OUT_CSV.open("w", newline="") as f:
        w = csv.DictWriter(f, fieldnames=fields, lineterminator="\n")
        w.writeheader()
        for r in rows:
            out = dict(r)
            c = out.get("cost_usd")
            if c is None or (isinstance(c, float) and math.isnan(c)):
                out["cost_usd"] = ""
            else:
                out["cost_usd"] = f"{c:.4f}"
            w.writerow(out)

    # Refresh per_model_repo.csv from the latest applicable phase raw JSONL.
    # Prefer phase 4 when present so phase-3 visuals/stats can compare against
    # the newest corrected run outputs.
    n_repo_rows = write_per_model_repo("phase4-refactoring")
    if not n_repo_rows:
        n_repo_rows = write_per_model_repo("phase3-agentic-loop")
    if n_repo_rows:
        print(f"wrote {PER_MODEL_REPO.relative_to(REPO_ROOT)} ({n_repo_rows} rows)")

    # Echo a quick summary per phase.
    print(f"wrote {OUT_CSV.relative_to(REPO_ROOT)} ({len(rows)} rows)")
    by_phase: dict[str, dict] = defaultdict(
        lambda: {"attempts": 0, "submitted": 0, "compile_ok": 0, "run_ok": 0, "tooling_excluded": 0, "cost_usd": 0.0, "has_cost": False}
    )
    for r in rows:
        p = by_phase[r["phase"]]
        for k in ("attempts", "submitted", "compile_ok", "run_ok", "tooling_excluded"):
            p[k] += r[k]
        c = r["cost_usd"]
        if isinstance(c, float) and not math.isnan(c):
            p["cost_usd"] += c
            p["has_cost"] = True
    for phase, t in by_phase.items():
        cost_str = f"${t['cost_usd']:.2f}" if t["has_cost"] else "n/a"
        print(
            f"  {phase}: attempts={t['attempts']} submitted={t['submitted']} "
            f"compile_ok={t['compile_ok']} run_ok={t['run_ok']} tooling_excluded={t['tooling_excluded']} cost={cost_str}"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
