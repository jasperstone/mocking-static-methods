# Phase _template_

Copy this directory when starting a new phase:

```bash
cp -r phases/_template phases/phase3-singleagent
```

Then:

1. **Frame the strategy.** Edit `phase.strategy` in `phase.lock.yaml`. One sentence.
2. **Author the prompt.** Edit `prompt/system.md` and `prompt/user-template.md`. Phase 3+ also adds `prompt/loop-feedback-template.md`.
3. **Author the workflow.** `cp .github/workflows/_phase-template.yml .github/workflows/phaseN-name.yml` and fill in the phase-specific steps.
4. **Wire the adapter.** If you need a strategy hook beyond the default single-shot in `tools/generation/runner.py`, add a runner under `tools/generation/strategies/`.
5. **Dispatch.** `gh workflow run .github/workflows/phaseN-name.yml`. Captures 5 models × 5 runs = 25 generation jobs, then one coverage CI run on the union of generated tests.
6. **Fill `phase.lock.yaml`.** Every field. Especially `model_snapshots_observed` from the JSONL rows.
7. **Write `REPORT.md`.** Phase narrative — what was tried, what worked, what didn't, per-model headlines.
8. **Append to root `RESULTS.md`.** New row per model in the cross-phase table.
9. **Tag** `phase-N-name-final` and never edit this directory again. Bug fixes go in the next phase.

## Subdirectories

| Path | Contents |
|---|---|
| `phase.lock.yaml` | All inputs needed to reproduce this phase |
| `REPORT.md` | Phase narrative — what was tried, what worked, what didn't |
| `REPLICATION.md` | One-page replication recipe for outside readers |
| `prompt/system.md` | Frozen system prompt — DO NOT EDIT after seal |
| `prompt/user-template.md` | Frozen user-message template with `{{...}}` placeholders |
| `prompt/loop-feedback-template.md` | (phase 3+) feedback prompt fed back each loop iteration |
| `results/{model_id}/run_{1..5}/` | Per-cell generation outputs |
| `results/aggregate.csv` | Merged outcomes across all 25 cells |
| `coverage/{repo}/` | Cobertura XMLs from this phase's coverage run |
| `generated_tests/{repo}/` | Union of test files used in the coverage run (the best-of-N selection from `results/`) |
| `errors/{repo}/` | Compile-fail and runtime-fail logs aggregated from `results/` |

## Why this layout

- **Tooling** (analyzers, orchestrator workflow, target builder, adapters) lives **outside** `phases/` in `tools/` and `.github/`. It evolves freely; phase reproducibility is anchored by the SHAs recorded in `phase.lock.yaml`, not by frozen copies of the tooling.
- **Inputs** (which Mode#1 sites we attempt) live in versioned `targets/v{N}/`. Phases pin to a specific version.
- **Outputs** (tests, errors, coverage, per-model JSONL) live in this phase directory.
- **The comparison table** lives once at the repo root in [`RESULTS.md`](../../RESULTS.md).
