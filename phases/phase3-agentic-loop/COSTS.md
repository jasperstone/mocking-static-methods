# Phase 3 — Agentic Loop with Compile + Run Feedback: Costs

> **Status: calibration only (run_1).** This document covers the 1,800-attempt calibration sweep. Runs 2 + 3 are in flight and will roughly triple the numbers below when they land; projections are at the bottom.

The full design and findings are in [REPORT.md](REPORT.md). This file focuses on **what it costs to reproduce on a small Azure budget** so other students and researchers can plan accordingly.

---

## Cost summary — v2 calibration (run_1)

All six panel models live in **one Azure AI Foundry account** — same setup as
phase 2 ([phase 2 COSTS](../phase2-agentic/COSTS.md)). Token counts are
captured in `results/<model>/run_1/attempts.jsonl` and priced against
published Azure list rates (captured 2026-05-12, identical to phase 2).

Scope: **300 cells × 1 run × 6 models = 1,800 generation attempts**, dispatched
via GitHub Actions matrix (72 of 72 successful shards). The 6-model panel is
the phase 2 panel minus `gpt-5-codex` ([phase 2 COSTS §"Decision: drop gpt-5-codex"](../phase2-agentic/COSTS.md#decision-drop-gpt-5-codex-from-phases-3-5)).

| Model | Calls | Submit rate | Prompt tokens | Completion tokens | Cost (USD) | % of spend |
|-------|------:|-------:|--------------:|------------------:|-----------:|-----------:|
| `llama-3.3-70b-instruct` | 300 | 99.3% | 14,470,334 | 1,239,805 | **$11.15** | **38%** |
| `codestral-2501`         | 300 | 95.0% | 17,405,275 | 1,402,922 | $6.48 | 22% |
| `gpt-4.1-mini`           | 300 | 78.0% | 11,010,292 |   618,775 | $5.39 | 19% |
| `grok-4-1-fast`          | 300 | 99.7% |  9,588,679 | 1,030,090 | $2.43 |  8% |
| `phi-4`                  | 300 | 98.3% |  8,520,923 | 1,626,406 | $1.88 |  6% |
| `gpt-4.1-nano`           | 300 | 92.3% | 15,976,423 |   552,088 | $1.82 |  6% |
| **Total**                | **1,800** | **93.8%** | **76,971,926** | **6,470,086** | **$29.16** | 100% |

> Regenerate this table with `python3 tools/cost/estimate.py --phase phase3-agentic-loop --md`,
> or refresh the cross-phase CSV with `python3 tools/viz/aggregate_phase_results.py`.

### Headline reading

- **`llama-3.3-70b-instruct` consumed 38% of phase 3 calibration spend** — more
  than 4× phase 2 in per-attempt terms. The reason is structural: the
  in-loop feedback turns add compile-error and TRX-failure context to every
  retry, and on a 70B model the prompt tokens for those turns are not cheap.
  Llama's submit rate also climbed (99.3% vs phase 2's 85.4%), so it spent
  more attempts in the multi-turn fix loop instead of refusing early.
- **Compile-vs-run improvement is real:** phase 2 6-model panel hit 4.8% / 1.4%
  on 5,400 cells for $16.58; phase 3 hits 15.0% / 7.3% on 1,800 cells for
  $29.16. That's **3.1× compile and 5.3× run-OK for 1.76× cost**.
- **Submit rate convergence:** phase 2 submit rates ranged from 13.6%
  (grok) to 94.1% (phi-4). In phase 3, every model submits between 78%
  and 99.7%. The feedback loop converts every model into a submission-happy
  one — but only the better models translate that into compile-OK.
- **Cost per *passing* test:**
  - `gpt-4.1-mini`: $5.39 / 39 = **$0.138 per green test** ← winner
  - `grok-4-1-fast`: $2.43 / 44 = $0.055 per green test ← cheapest per green
  - `llama-3.3-70b-instruct`: $11.15 / 20 = $0.558 per green test
  - `codestral-2501`: $6.48 / 13 = $0.498 per green test
  - `gpt-4.1-nano`: $1.82 / 8 = $0.227 per green test
  - `phi-4`: $1.88 / 8 = $0.235 per green test
  - **Panel average: $29.16 / 132 = $0.221 per green test.**

Compare to phase 2 (ex-codex): $16.58 / 75 = $0.221 per green test. **The
cost-per-green-test is identical between phase 2 and phase 3 (to the cent).**
Phase 3 buys 5.3× more passing tests at 1.76× the cost — exactly the same
efficiency-frontier ratio. The in-loop feedback is a strict pareto improvement,
not a cheaper-per-test improvement.

### Reconciling with the Azure bill

Cost-management for the phase 3 calibration window has not posted yet (24-48h
lag). Expected breakdown (using phase 2 ratios):

| Source | Amount | Notes |
|---|---:|---|
| Foundry Models (token cost — measured) | $29.16 | Above table |
| Foundry / Cognitive Services overhead | ~$3-5 | Pro-rated; phase 2 ran ~$11 for a 3-month window |
| Storage / misc | ~$1 | Logs, blob, key vault |
| **Estimated Azure total** | **~$33-35** | Will reconcile when the bill posts |

---

## Decisions captured during calibration

1. **Calibration before commit.** Run_1 was dispatched standalone so we could
   inspect the run-OK delta before paying for the full 3-run sweep. The 5.3×
   run-OK gain justified continuation; runs 2+3 were dispatched immediately
   after eval cleared.
2. **`max_attempts = 4`.** Pilot data on 50 cells showed diminishing returns
   past attempt 3; we kept 4 for safety margin. The mean number of attempts
   per submitted cell across phase 3 calibration is ~2.1.
3. **`run_timeout_s = 60`.** Caught all the runaway tests in the smoke test
   without false-positive-timing-out any real test. Longer would add prompt
   tokens (more attempts hit the timeout, more feedback turns) and inflate
   llama's cost share further.
4. **`gpt-5-codex` stays out.** The 82%-of-budget consumption pattern in
   phase 2 would have multiplied through the feedback loop; preserving codex
   under chain-of-feedback is not what this phase tests. The reasoning-tier
   comparison can be re-opened in a targeted later phase.
5. **One container-init failure** (`llama × duplicati × run3`) consumed
   $0.00 in tokens; manual single-cell rerun deferred to post-aggregation.

---

## Cost projections — completing phase 3 and looking ahead

Calibration was 1 run × 1,800 attempts × $29.16. Runs 2+3 are dispatched
already; the linear projection is:

| Tier | Scope | Projected cost |
|---|---|---:|
| Phase 3 calibration (run_1) — actual | 1,800 attempts | **$29.16 (measured)** |
| Phase 3 runs 2+3 — dispatched | 3,600 attempts | ~$58 (linear) |
| **Phase 3 total — projected** | **5,400 attempts** | **~$87** |

Runs 2+3 generate completed successfully; the linear projection assumes
identical per-attempt distribution. The single failed shard
(llama × duplicati × run3, 25 cells) reduces the projection by ~$0.40.

**Phase 3 total stays under $100 — well under the $250 tripwire.**

Looking further out (same 300-cell v2 sample, no resampling):

| Phase | Strategy | Chain mult. vs phase 3 | Est. cost (5,400 attempts) |
|---|---|---:|---:|
| Phase 3 | Agentic loop + compile + run feedback | 1.0× | ~$87 (projected) |
| Phase 4 | Multi-agent (writer / reviewer / fixer) | 2-3× | ~$175-260 |
| Phase 5 | Multi-team coordination | 3-5× | ~$260-435 |

Phase 4 is the next decision point on whether to keep all six models or trim
the panel further. `gpt-4.1-nano` and `phi-4` produced the fewest absolute
green tests in phase 3; if phase 4 continues that pattern they're candidates
for removal in phase 5.

---

## Budget guardrails (unchanged from phase 2)

- **$50/month budget alert** on `rg-mocking-static-experiment`. Phase 3
  calibration ($29) sits comfortably under it; runs 2+3 will push the
  monthly total past it, which is expected and acknowledged.
- **$250 tripwire** at the experiment level (the value the user explicitly
  set; see session memory).
- **Spending limit `CurrentPeriodOff`** — overage hits the credit card
  rather than killing access. Required for multi-day GitHub Actions matrix
  jobs that can't tolerate mid-run Azure suspension.
- **Three-week token-spend pause** starting after runs 2+3 evaluate. All
  remaining number-crunching, documentation, and analysis must run on
  free GitHub Actions runners or locally during the pause.
