# Phase 3 — Agentic Loop with Compile + Run Feedback: Costs

> **Status: final.** This document covers the full 5,400-attempt phase 3 sweep
> (3 runs × 1,800 attempts). The 9-cell `llama × duplicati × run_3` and
> 1-cell `gpt-4.1-nano × efcore × run_3` shards that failed at container
> init in the original sweep have been re-dispatched as fix shards and
> their results merged.

The full design and findings are in [REPORT.md](REPORT.md). This file focuses on **what it costs to reproduce on a small Azure budget** so other students and researchers can plan accordingly.

---

## Cost summary — v2, all 3 runs

All six panel models live in **one Azure AI Foundry account** — same setup as
phase 2 ([phase 2 COSTS](../phase2-agentic/COSTS.md)). Token counts are
captured in `results/<model>/run_{1,2,3}/attempts.jsonl` and priced against
published Azure list rates (captured 2026-05-12, identical to phase 2).

Scope: **300 cells × 3 runs × 6 models = 5,400 generation attempts** (all
landed after the two fix shards). Dispatched via GitHub Actions matrix
across calibration (run [25877016877](https://github.com/jasperstone/mocking-static-methods/actions/runs/25877016877)) and the runs 2+3 sweep (run [25921948154](https://github.com/jasperstone/mocking-static-methods/actions/runs/25921948154)), with fix shards landed via runs [26048493558](https://github.com/jasperstone/mocking-static-methods/actions/runs/26048493558) and [26048496818](https://github.com/jasperstone/mocking-static-methods/actions/runs/26048496818). The 6-model panel is
the phase 2 panel minus `gpt-5-codex` ([phase 2 COSTS §"Decision: drop gpt-5-codex"](../phase2-agentic/COSTS.md#decision-drop-gpt-5-codex-from-phases-3-5)).

| Model | Calls | Submit rate | Prompt tokens | Completion tokens | Cost (USD) | % of spend |
|-------|------:|-------:|--------------:|------------------:|-----------:|-----------:|
| `llama-3.3-70b-instruct` |   900 | 99.3% | 42,846,465 | 3,655,273 | **$33.02** | **40%** |
| `codestral-2501`         |   900 | 95.0% | 50,986,768 | 4,206,602 | $19.08 | 23% |
| `gpt-4.1-mini`           |   900 | 70.8% | 27,069,565 | 1,570,369 | $13.34 | 16% |
| `grok-4-1-fast`          |   900 | 99.9% | 27,610,167 | 3,048,918 |  $7.05 |  9% |
| `phi-4`                  |   900 | 96.6% | 25,191,436 | 4,784,526 |  $5.54 |  7% |
| `gpt-4.1-nano`           |   900 | 77.9% | 36,464,180 | 1,290,688 |  $4.16 |  5% |
| **Total**                | **5,400** | **89.9%** | **210,168,581** | **18,556,376** | **$82.19** | 100% |

> Regenerate this table with `python3 tools/cost/estimate.py --phase phase3-agentic-loop --md`,
> or refresh the cross-phase CSV with `python3 tools/viz/aggregate_phase_results.py`.

### Headline reading

- **`llama-3.3-70b-instruct` consumed 40% of phase 3 spend** — $33.02 for
  52 green tests, the worst $/green ratio in the panel. The reason is
  structural: the in-loop feedback turns add compile-error and TRX-failure
  context to every retry, and on a 70B model the prompt tokens for those
  turns are not cheap. Llama's submit rate climbed to 99.3% (vs phase 2's
  85.4%), so it spent more attempts in the multi-turn fix loop instead of
  refusing early.
- **Compile-vs-run improvement is real:** phase 2 6-model panel hit 4.8% / 1.4%
  on 5,400 cells for $16.58; phase 3 hits 14.6% / 7.1% on 5,400 cells for
  $82.19. That's **3.0× compile and 5.1× run-OK for 4.96× cost.**
- **Submit rate convergence:** phase 2 submit rates ranged from 13.6%
  (grok) to 94.1% (phi-4). In phase 3, every model except `gpt-4.1-mini`
  submits above 77%, and four of six are above 95%. The feedback loop
  converts almost every model into a submission-happy one — but only the
  better models translate that into compile-OK.
- **Cost per *passing* test:**
  - `grok-4-1-fast`: $7.05 / 133 = **$0.053 per green test** ← cheapest
  - `gpt-4.1-mini`: $13.34 / 109 = $0.122 per green test
  - `phi-4`: $5.54 / 30 = $0.185 per green test
  - `gpt-4.1-nano`: $4.16 / 19 = $0.219 per green test
  - `codestral-2501`: $19.08 / 43 = $0.444 per green test
  - `llama-3.3-70b-instruct`: $33.02 / 52 = $0.635 per green test
  - **Panel average: $82.19 / 386 = $0.213 per green test.**

Compare to phase 2 (ex-codex): $16.58 / 75 = $0.221 per green test. **The
cost-per-green-test is identical between phase 2 and phase 3 to the cent.**
Phase 3 buys 5.1× more passing tests at 4.96× the cost — exactly the same
efficiency-frontier ratio. The in-loop feedback is a **strict pareto
improvement**, not a cheaper-per-test improvement.

### Reconciling with the Azure bill

Cost-management for the phase 3 window has not fully posted yet (24-48h
lag). Expected breakdown (using phase 2 ratios):

| Source | Amount | Notes |
|---|---:|---|
| Foundry Models (token cost — measured) | $82.19 | Above table |
| Foundry / Cognitive Services overhead | ~$8-12 | Pro-rated; phase 2 ran ~$11 for a 3-month window |
| Storage / misc | ~$2 | Logs, blob, key vault |
| **Estimated Azure total** | **~$92-96** | Will reconcile when the bill posts |

---

## Decisions captured during phase 3

1. **Calibration before commit.** Run_1 was dispatched standalone so we could
   inspect the run-OK delta before paying for the full 3-run sweep. The 5.3×
   run-OK gain (calibration vs phase 2) justified continuation; runs 2+3 were
   dispatched immediately after eval cleared.
2. **`max_attempts = 4`.** Pilot data on 50 cells showed diminishing returns
   past attempt 3; we kept 4 for safety margin. The mean number of attempts
   per submitted cell across phase 3 is ~2.1.
3. **`run_timeout_s = 60`.** Caught all the runaway tests in the smoke test
   without false-positive-timing-out any real test. Longer would add prompt
   tokens (more attempts hit the timeout, more feedback turns) and inflate
   llama's cost share further.
4. **`gpt-5-codex` stays out.** The 82%-of-budget consumption pattern in
   phase 2 would have multiplied through the feedback loop; preserving codex
   under chain-of-feedback is not what this phase tests. The reasoning-tier
   comparison can be re-opened in a targeted later phase.
5. **Two container-init failures** (9-cell `llama × duplicati × run_3` plus
   1-cell `gpt-4.1-nano × efcore × run_3`) consumed $0.00 in tokens in the
   original sweep. Re-dispatched as fix shards; combined budget impact <$0.25.

---

## Cost projections — looking ahead to phase 5

Phase 3 final = $82.19 across 5,400 attempts. **That $82.19 is the
Foundry *Models* (token) line only.** The actual May Azure bill for the
phase-3 window was **~$342.71** (Foundry Tools $182.26 + Foundry Models
$160.45) — ~4.2× the token figure, because the agent-runtime / tool
surface (Foundry Tools) is billed per agent invocation and was previously
unmodelled. `tools/cost/estimate.py` is now calibrated against that bill;
its phase-3 combined lands at $342.53 (residual −$0.18). This is the base
the phase-5 (multi-agent) projection scales from — see
[phase 5 PLAN §Cost projection](../phase5-multiagent/PLAN.md#cost-projection).

Because Foundry Tools scales with **agent invocations**, the multi-agent
loop (writer + reviewer×cycles + fixer×cycles) makes phase 5 far more
expensive than the old token-only math implied. The per-cell realized call
count is `1 + 1.1·C` (C = `max_review_cycles`): 2.1 at C=1, 3.2 at C=2,
4.3 at C=3. `runs_per_cell` (R) scales the writer base linearly.

| Config | runs | cycles | Combined (bill-calibrated) | % of $250 cap | To card |
|---|---:|---:|---:|---:|---:|
| **A — calibration** (recommended next dispatch) | 1 | 2 | **~$304** | 122% | ~$154 |
| **B — full sweep, reduced cycles** | 3 | 2 | **~$913** | 365% | ~$763 |
| **C — original full scope** | 3 | 3 | **~$1,197** | 479% | ~$1,047 |

Decision (2026-06-10, Jasper): **keep the full 6-model panel** — the
cross-model comparison is the point — and cut cost via **runs (3→1) and
review cycles (3→2)** instead of dropping models. That takes the full-scope
projection from ~$1,197 down to ~$304 for the calibration dispatch (Config A),
whose implied **card** spend is only ~$154 (≈ the monthly $150 credit). Even
so, the **combined** total (the cap metric) overshoots the $250 cap at every
config; Config A is the closest and the recommended first dispatch.
Reproduce with `python3 tools/cost/estimate.py --project-phase5 --cap 250`.

Phase 5 also remains the decision point on whether the panel composition
holds. `gpt-4.1-nano` produced 19 green tests for $4.16 — fewer absolute
wins than every other model except phi-4. `llama` is the opposite question —
52 green tests but 40% of token spend; phase 5 will reveal whether the extra
cost buys differentiated coverage or duplicates the cheaper models. (Neither
is being dropped for phase 5 — the comparison stays intact.)

---

## Budget guardrails (unchanged from phase 2)

- **$50/month budget alert** on `rg-mocking-static-experiment`. Phase 3 in
  total ($82) exceeded it across the calibration + runs-2+3 dispatch window,
  which was expected and acknowledged.
- **$250 tripwire** at the experiment level (the value the user explicitly
  set; see session memory).
- **Spending limit `CurrentPeriodOff`** — overage hits the credit card
  rather than killing access. Required for multi-day GitHub Actions matrix
  jobs that can't tolerate mid-run Azure suspension.
- **Three-week token-spend pause** starts now. All remaining number-crunching,
  documentation, and analysis runs on free GitHub Actions runners or locally
  during the pause.
