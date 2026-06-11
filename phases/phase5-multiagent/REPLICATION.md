# Phase 5 — Multi-agent (writer / reviewer / fixer): Replication

> **Status: scaffold. The mock-adapter smoke test below works today.
> Production replication blocks on the Azure freeze ending ~2026-06-08.**

To reproduce phase 5 once it has run. Most of this is identical to
[phase 3 replication](../phase3-agentic-loop/REPLICATION.md); the new
pieces are the three-agent runner and its prompts.

## Prerequisites

Same as phase 3 plus:

- ~$210 of Azure credit for the full 3-run sweep (5,400 cells; see
  [PLAN.md § Cost projection](PLAN.md#cost-projection))
- The phase 3 Azure account, models, and tripwire (`phase3-tripwire-200`)
  must already exist — phase 5 uses the same Foundry account and adds
  a new tripwire `phase4-tripwire-250`.

## 1. Reuse the phase 2/3 Foundry account

Skip to step 2 if you've already done [phase 3 replication](../phase3-agentic-loop/REPLICATION.md).
The same six panel models cover phase 5.

## 2. Set the phase 5 tripwire

```bash
SUB=<your-subscription-id>
RG=rg-mocking-static-experiment
EMAIL=<your-email>
START=$(date -u +%Y-%m-01T00:00:00Z)
END=$(date -u -d '+12 months' +%Y-%m-01T00:00:00Z)
az rest --method put \
  --uri "https://management.azure.com/subscriptions/$SUB/resourceGroups/$RG/providers/Microsoft.Consumption/budgets/phase4-tripwire-250?api-version=2024-08-01" \
  --body "{\"properties\":{\"category\":\"Cost\",\"amount\":250,\"timeGrain\":\"Monthly\",\"timePeriod\":{\"startDate\":\"$START\",\"endDate\":\"$END\"},\"notifications\":{\"actual50\":{\"enabled\":true,\"operator\":\"GreaterThan\",\"threshold\":50,\"contactEmails\":[\"$EMAIL\"],\"thresholdType\":\"Actual\"},\"actual80\":{\"enabled\":true,\"operator\":\"GreaterThan\",\"threshold\":80,\"contactEmails\":[\"$EMAIL\"],\"thresholdType\":\"Actual\"}}}}"
```

## 3. Smoke test the runner against the mock adapter

This step does NOT require Azure access. It exercises the entire
writer / reviewer / fixer loop end-to-end against canned fixture
responses, validating that the runner code, prompt rendering, and
output JSONL shape all work before any tokens are spent.

```bash
python3 -m pytest tools/generation/tests/test_multi_agent_smoke.py -v
```

Expected outcome: one test passes, one cell of fake output lands in
`/tmp/phase5-smoke/results/mock-llm/run_1/` with a well-formed
`attempts.jsonl` (one row) and `generated_tests/.../test.cs`.

## 4. Smoke test against real Foundry (one cell, one model)

Once the freeze has elapsed and the phase 3 reconciliation is clean,
do a single-cell paid smoke test to validate the production wiring:

```bash
python3 tools/generation/multi_agent_runner.py \
    --phase phase5-multiagent \
    --model gpt-4.1-mini \
    --run-index 0 \
    --target-set v2 \
    --target-ids duplicati:0014 \
    --max-review-cycles 1
```

Should land one row in `phases/phase5-multiagent/results/gpt-4.1-mini/run_0/attempts.jsonl`
with `multi_agent_cycles = 1` and a non-empty `reviewer_verdict`.
Expected token spend: < $0.10.

## 5. Dispatch the full sweep via GitHub Actions

```
gh workflow run phase5-generate.yml \
  -f target_set=v2 \
  -f runs_per_cell=3 \
  -f models=all \
  -f repos=all \
  -f max_review_cycles=3 \
  -f mock_llm=false
```

⚠️ The default value of `mock_llm` in the workflow is **`true`** — leave
it as `true` to test the workflow shape without spending tokens, or
explicitly set it to `false` for the production sweep.

## 6. Evaluate via the canonical evaluator

Same as phase 3:

```
gh workflow run phase5-evaluate.yml \
  -f target_set=v2 \
  -f models=all \
  -f repos=all
```

## 7. Aggregate and refresh the dashboard

```bash
python3 tools/viz/aggregate_phase_results.py
docker run --rm --user "$(id -u):$(id -g)" -v "$PWD":/work -w /work \
    rocker/tidyverse:4.4 Rscript tools/viz/render_all.R
```

The phase 2 → 3 → 5 progression chart and the cross-phase paired-bar
will both auto-pick up phase 5 once `per_model_phase.csv` and
`per_model_repo.csv` have phase-5 rows.

## Output layout

```
phases/phase5-multiagent/
  results/{model}/run_{i}/
    attempts.jsonl              # one row per cell
    generated_tests/{repo}/{target_id}/test.cs
    turns/{repo}/{target_id}.jsonl   # full agent trace
      # role: writer | reviewer | fixer
      # role_call_index: 1, 2, ... within the cell
```

Each `attempts.jsonl` row adds these multi-agent-specific fields to the
phase 3 schema:

- `multi_agent_cycles` — count of (review, fix, check) triples that fired
- `reviewer_verdicts` — list of verdict strings (`APPROVE` | `REQUEST_CHANGES`)
- `final_role` — which agent produced the final submitted draft
  (`writer` if first draft passed review, otherwise `fixer`)
- `writer_turns`, `reviewer_turns`, `fixer_turns` — token-budget breakdown
