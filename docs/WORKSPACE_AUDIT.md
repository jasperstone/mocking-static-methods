# Workspace audit — 2026-01-30

A pass over the repository root looking for stale files, broken references,
and gaps in `.gitignore`. **No deletions in this PR** — the only change is a
single `.gitignore` line. The rest is recorded here so the user can decide
what (if anything) to act on.

## Summary

The repo root is in good shape. Earlier session scratch files (the dozens of
`*_SUMMARY.md`, `orchestrator.py`, `test_orchestrator*` etc. that lived at
the root during the planning phase) have already been removed in prior PRs.
What remains at the root is the minimum set of artifacts the README and the
viz pipeline genuinely reference.

## Section A — `.gitignore` addition (applied in this PR)

Two empty files are sitting at the root from accidental `git diff > foo.diff`
redirections:

```
attempts.diff   0 bytes
full.diff       0 bytes
```

These are already untracked. Adding `*.diff` to `.gitignore` so they don't
keep showing up in `git status` and don't get accidentally committed in the
future.

`runtime_run.log` (185 bytes, Jan 30) is also at the root but is already
covered by the existing `*.log` rule — no action needed. Local file can be
deleted at the user's discretion.

`__pycache__/` is already covered by the existing `**/__pycache__/` rule.

## Section B — root-level files, verified KEEP

All four root markdown files and the root data files are actively referenced:

| File | Referenced by |
|---|---|
| `README.md` | (root) |
| `RESULTS.md` | linked from README, phase headlines |
| `TEST_COUNTS.md` | `tools/test_counts/`, decisions ledger |
| `TEST_DISCOVERY.md` | `tools/test_discovery/`, decisions ledger |
| `baseline_coverage.csv` | `tools/viz/lib/load.R::load_baseline_coverage()` |
| `test_counts.csv` | `tools/test_counts/from_coverage_logs.py` |
| `test_discovery_summary.csv` | `tools/test_discovery/aggregate.py` |
| `aggregate_baseline.py` | source of `baseline_coverage.csv` |
| `Mode1Analyzer/` | linked from README §"Repo selection"; consumed by `tools/coverage_xref/{build_unified_table,xref_mode1_coverage}.py` |

Nothing here is orphaned. (Earlier draft notes flagged `Mode1Analyzer/` and
`aggregate_baseline.py` as candidates for removal — that was wrong; both
have live references confirmed by grep.)

## Section C — known data gaps (NOT addressed in this PR)

These were noticed during the audit but are out of scope for a cleanup PR.
They're recorded here so a follow-up can pick them up:

1. **`tools/viz/data/per_model_repo.csv` is run-1-only.** Phase 3 raw data
   (runs 1 + 2 + 3) is available under
   `phases/phase3-agentic-loop/results/*/run_{1,2,3}/{attempts,evaluation}.jsonl`
   (landed in PR #22). `per_model_repo.csv` still totals 1,688 submitted /
   270 compile-ok / 132 run-ok for phase 3, where the canonical 3-run
   totals are 4,845 / 782 / 384. The aggregator
   `tools/viz/aggregate_phase_results.py` writes `per_model_phase.csv` from
   the raw JSONL but only *reads* `per_model_repo.csv` as a fallback — there
   is no per-repo writer in tree. A follow-up PR can extend the aggregator
   to emit per-repo rows directly from the raw JSONL.

2. **Untracked regeneration artifacts.** `baseline_artifacts/`,
   `discovery_artifacts/`, and `coverage_logs/` at the root are products
   of CI artifact downloads (`gh run download`). They are correctly
   untracked today. If they start sprawling, a `baseline_artifacts/` +
   `discovery_artifacts/` + `coverage_logs/` block can be added to
   `.gitignore`. Not adding now because (a) they're already untracked and
   (b) adding ignore rules makes future `git add path/` slightly more
   surprising.

## Change in this PR

- `.gitignore`: add `*.diff`.
- `WORKSPACE_AUDIT.md`: this file.

That's it. Open to discussion on Section C items.
