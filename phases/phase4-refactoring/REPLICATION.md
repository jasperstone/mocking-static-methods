# Phase 4 — Agentic loop + testability refactoring tool: Replication

> **Status: scaffold. The mock-adapter smoke test below works once the runner
> lands. Production replication blocks on the run_1 go/no-go.**

To reproduce phase 4 once it has run. Most of this is identical to
[phase 3 replication](../phase3-agentic-loop/REPLICATION.md); the new pieces are
the `apply_refactor` tool, the refactoring runner, and the snapshot/restore
lifecycle. Phase 4 is **single-agent** (one writer with a refactoring tool) — there
is no reviewer or fixer.

> **Single-variable design (the control).** The phase-4 **task framing** is held
> **identical to phase 3**: the system prompt is phase 3's verbatim, and the user
> template is byte-for-byte phase 3's (no Mode #1 / seam language). The one new tool,
> `apply_refactor`, is documented like any other tool — a one-line menu entry plus a
> dedicated block covering its transforms, calling syntax, and
> build-guard/auto-revert/transient contract — consistent with how phase 3 documents
> `submit_test` and the compile/run loop. **Tool documentation is not task coaching.**
> **The sole manipulated variable is the availability of the `apply_refactor` tool.**
> What is held constant is the task framing: the agent is never told it faces a Mode #1
> site, never told it needs a seam, and is never given a transform-selection strategy.
> Anti-gaming and behavior-preservation are enforced by the harness and surfaced to the
> agent only through tool feedback (`refactor_rejected`), **never pre-coached in the
> prompt** — so any delta in run-OK% vs phase 3 is attributable to the tool, not to
> prompt engineering.

## Prerequisites

Same as phase 3 plus:

- ~$214 of Azure credit for the run_1 calibration sweep (300 cells × 6 models × 1
  run; see [PLAN.md § Cost note](PLAN.md#cost-note)).
- The phase 2/3 Azure account and the six panel models must already exist — phase
  4 reuses them and tracks spend against the existing `phase4-tripwire-250` budget.

## 1. Reuse the phase 2/3 Foundry account

Skip to step 2 if you've already done [phase 3 replication](../phase3-agentic-loop/REPLICATION.md).
The same six panel models cover phase 4.

## 2. Confirm the phase 4 tripwire

Phase 4 reuses the **existing** combined soft-cap budget `phase4-tripwire-250` —
do NOT create a new budget noun. Confirm it is present:

```bash
az consumption budget list -o table | grep phase4-tripwire-250
```

It is a subscription-scoped, $250 Monthly budget, Actual 50/80/100% + Forecasted
100%, alert-only. (See decision `2026-06-10: phase4-tripwire-250 Azure budget
created` and `2026-06-11: Budget cleanup`.) The real hard stop remains the
subscription spending-limit toggle, not the budget.

## 3. Unit-test the refactoring tool

This step does NOT require Azure access. It exercises each transform in the
`apply_refactor` menu plus the behavior-preservation guard's auto-revert path:

```bash
python3 -m pytest tools/generation/tests/test_apply_refactor.py -v
```

Expected: `make_virtual`, `wrapper_interface`, and `parameterize_dependency` each
produce a compiling seam on a fixture project; a deliberately behavior-changing
edit triggers the guard and is auto-reverted (`refactor_rejected`); the fixture
project is byte-identical after each test.

## 4. Smoke test the runner against the mock adapter

Exercises the entire read → apply_refactor → submit_test loop end-to-end against
canned fixture responses, validating the runner code, prompt rendering,
snapshot/restore, and output JSONL shape before any tokens are spent:

```bash
python3 -m pytest tools/generation/tests/test_refactor_smoke.py -v
```

Expected outcome: one test passes, one cell of fake output lands under
`/tmp/phase4-smoke/results/mock-llm/run_1/` with a well-formed `attempts.jsonl`
(one row, including `applied_refactors` and `refactor_guard_ok`) and
`generated_tests/.../test.cs`. The fixture repo is restored to pristine state
after the cell.

## 5. Smoke test against real Foundry (one cell, one model)

Once the run_1 budget is cleared, do a single-cell paid smoke test to validate the
production wiring:

```bash
python3 tools/generation/agentic_refactor_runner.py \
    --phase phase4-refactoring \
    --model gpt-4.1-mini \
    --run-index 0 \
    --target-set v2 \
    --target-ids duplicati:0014
```

Should land one row in
`phases/phase4-refactoring/results/gpt-4.1-mini/run_0/attempts.jsonl` with a
non-empty `applied_refactors` list and `refactor_guard_ok = true`. Expected token
spend: < $0.10. Confirm the repo working tree is clean afterward (`git status`
shows no modified prod files — snapshot/restore worked).

## 6. Dispatch the run_1 sweep via GitHub Actions

```
gh workflow run phase4-refactoring.yml \
  -f target_set=v2 \
  -f runs_per_cell=1 \
  -f models=all \
  -f repos=all \
  -f mock_llm=false
```

⚠️ The default value of `mock_llm` in the workflow is **`true`** — leave it as
`true` to test the workflow shape without spending tokens, or explicitly set it to
`false` for the production sweep. run_1 (`runs_per_cell=1`) is the go/no-go; runs
2+3 are dispatched only after run_1's measured bill clears the cap check.

## 7. Evaluate via the canonical evaluator

Same evaluator as phase 3 — phase 4 tests run against the production csproj built
from the (transiently) refactored source:

```
gh workflow run phase4-evaluate.yml \
  -f target_set=v2 \
  -f models=all \
  -f repos=all
```

## 8. Aggregate and refresh the dashboard

```bash
python3 tools/viz/aggregate_phase_results.py
docker run --rm --user "$(id -u):$(id -g)" -v "$PWD":/work -w /work \
    rocker/tidyverse:4.4 Rscript tools/viz/render_all.R
```

The phase 2 → 3 → 4 progression chart and the cross-phase paired-bar will
auto-pick up phase 4 once `per_model_phase.csv` and `per_model_repo.csv` have
phase-4 rows.

## Output layout

```
phases/phase4-refactoring/
  results/{model}/run_{i}/
    attempts.jsonl              # one row per cell
    generated_tests/{repo}/{target_id}/test.cs
    turns/{repo}/{target_id}.jsonl   # full agent trace (incl. apply_refactor calls)
```

Each `attempts.jsonl` row adds these refactoring-specific fields to the phase 3
schema:

- `applied_refactors` — list of `{transform, target_file, touched_files}` applied
  in the cell (empty if the agent submitted without refactoring)
- `refactor_guard_ok` — did the behavior-preservation guard pass (false ⇒
  `refactor_rejected`, auto-reverted)
- `refactor_rejected` — true if a refactor was attempted and reverted by the guard
- `seam_kind` — which transform produced the seam the test asserts through
  (`make_virtual` | `wrapper_interface` | `parameterize_dependency` | `none`)
- `legitimate_pass` — did the submitted test exercise the target via the seam and
  assert on real behavior (the refactor-attributable filter)
- `restore_ok` — did snapshot/restore return the repo to pristine state after the
  cell
