# Phase 4 — Multi-agent (writer / reviewer / fixer): PLAN

> **Status: scaffolding only. NO Azure dispatch before ~2026-06-08.**
>
> This document captures the design of phase 4 before any tokens are spent.
> The runner, prompts, and workflows exist on disk so that the next session
> can do production runs once the 3-week Azure freeze (started 2026-05-18)
> elapses. Every workflow in this scaffold is `workflow_dispatch`-only with
> defaults that point at the mock adapter, so an accidental dispatch from
> the GitHub UI cannot incur token spend.

## Hypothesis

Phase 3 closed the compile-vs-run gap from 5,400 attempts: **787 compile / 386 run-OK** ($82.19). The remaining 401-cell gap between "builds clean" and "actually runs green" is dominated by structural mistakes the single agent kept making even with compile + run feedback:

| Failure bucket | Count (phase 3) | What it looks like |
|---|---:|---|
| `other_exception` | 253 | DI / ctor exceptions in test setup |
| `no_fact_methods` | 160 | Test class compiles but contains no `[Fact]` |
| `assertion_failed` | 53 | Real assertion failures (the signal we want) |
| `invalid_op_runtime` | 35 | `InvalidOperationException` at runtime |
| `arg_null` | 24 | Missing arguments in setup |
| `null_ref` | 22 | Null-derefs in setup |

160 of the 401 (40%) are the `no_fact_methods` case — the agent wrote a
test *scaffold* that compiles but exercises nothing. The single agent has
no incentive to question its own scaffolding; the compiler doesn't catch
empty `[Fact]`-less classes, and the test runner happily reports "0 tests
run, 0 failed."

Phase 4 introduces a **reviewer** agent whose only job is to look at a
draft test and decide whether it actually exercises the target method
under realistic conditions. The reviewer can reject (with a written
reason) and a **fixer** agent then revises the draft. The cycle repeats
up to a budget, after which the latest draft is submitted.

## Architecture

```
target → Writer (1st draft) → check (build+test) → ┐
                                                   │
            ┌──────────────────────────────────────┘
            ▼
        Reviewer (verdict + comments)
            │
   ┌────────┴────────┐
   │                 │
APPROVE         REQUEST_CHANGES
   │                 │
   ▼                 ▼
submit         Fixer (revised draft) → check → Reviewer → ...
```

- **Writer**: same prompt as phase 3 plus an explicit "self-check"
  reminder. Produces a complete C# test file.
- **Reviewer**: a *new* agent with a different system prompt. Sees the
  writer's draft + the build/test result. Emits a structured verdict:
  `APPROVE` | `REQUEST_CHANGES` plus a free-text comment. Cannot edit
  the test itself — only describes what to change.
- **Fixer**: another agent role that takes the reviewer's comment +
  the draft and emits a revised C# test file. Same shape as the
  writer's output.

### Budgets

| Budget | Default | Rationale |
|---|---:|---|
| `max_review_cycles` | **1 (frozen)** | **Frozen at 1 by Jasper (2026-06-10).** Each cycle is one (review, fix, check) triple. The multi-agent agent-invocation overhead is the dominant cost driver; cycles=1 minimizes it while still exercising writer→reviewer→fixer once. This is sealed — it does not change after run_1 calibration. See [Cost projection](#cost-projection) and decision `2026-06-10: phase-4 calibration is run_1`. |
| `max_turns` (per agent) | 6 | Lower than phase 3's 12 because each agent has a narrower job. |
| `max_reads` (per agent) | 4 | Half of phase 3 — most cells in phase 3 used ≤4 reads in the writer phase. |
| `max_attempts` (submissions) | 4 | Same as phase 3. |
| `run_timeout_s` | 60 | Same as phase 3. |
| `runs_per_cell` | **3 (target), dispatched as run_1 → 2+3** | Writer invocations scale linearly with this. The 3-run target is dispatched incrementally: **run_1 = calibration** (300 cells × 6 models × 1 run = 1,800 writer calls) to get the first measured multi-agent cost + run-OK point, then a go/no-go, then **runs 2+3** with identical sealed config. run_1 is poolable with runs 2+3 only if nothing changes after calibration (see Reusability below). |

### Termination conditions

1. Reviewer emits `APPROVE` AND the latest build/test result is `run_ok` — submit + halt.
2. `max_review_cycles` exhausted — submit the latest draft regardless of reviewer verdict.
3. Either agent exceeds its turn budget — surface as `halt_reason = "turn_budget"`; the latest draft (writer or fixer) is submitted.

### Same model for all three roles?

For the first dispatch we run the **same model in all three roles**. This
is the cleanest A/B against phase 3 — any lift is attributable to the
multi-agent structure, not a model swap. Mixed-role panels (e.g. nano as
fixer, mini as reviewer) are a follow-up question for phase 5.

## Predicted lift

Based on phase 3 failure-bucket analysis:

- The 160 `no_fact_methods` cells should be near-zero in phase 4 — the
  reviewer's system prompt explicitly checks for `[Fact]` count and
  meaningful assertions.
- The 253 `other_exception` cells (DI / ctor problems) are partly
  addressable — the reviewer can spot obvious missing DI registrations,
  though many require code reads the reviewer doesn't have.
- The 22 `null_ref` and 24 `arg_null` cells should drop — these are
  setup-style mistakes the reviewer's verdict prompt explicitly flags.

Conservative estimate: **2.0–2.5× run-OK lift over phase 3** (i.e.
~770–960 green tests on 5,400 attempts). The "no `[Fact]`" alone is
~40% of the gap; closing half of it lands us at ~480 run-OK, which is
1.24× on its own.

## Cost projection

> **Updated 2026-06-10** after the May Azure bill posted and
> `tools/cost/estimate.py` was re-calibrated against it. The earlier
> token-only "~$210" figure was **5× too low** — it modelled only Foundry
> *Models* (token) cost and ignored the Foundry *Tools* / agent-runtime line,
> which was the single biggest item on the bill ($182.26) and scales with
> **agent invocations**, not tokens. The multi-agent loop multiplies
> invocations per cell, so that term — not tokens — dominates phase 4.
> Reproduce all numbers below with:
> `python3 tools/cost/estimate.py --project-phase4 --cap 250`.

### Per-cell agent invocation count (the multiplier)

Phase 3 = a single writer agent per cell. Phase 4 adds a reviewer and a fixer
inside a review loop bounded by `max_review_cycles` (C):

```
calls/cell = 1 writer + (reviewer per cycle × C) + (fixer per cycle × C)
```

**Theoretical max** is `1 + 2·C` (every cycle fires both a review and a fix):

| `max_review_cycles` | Theoretical max calls/cell (`1 + 2C`) |
|---:|---:|
| 1 | 3 |
| 2 | 5 |
| 3 | 7 |

**Realized average is lower** — most cells pass review before exhausting the
cycle budget (early exit). The May-calibrated realized per-cycle rates are
**0.6 reviewer / cycle** and **0.5 fixer / cycle**, giving `1 + 1.1·C`:

| `max_review_cycles` | Reviewer (`0.6C`) | Fixer (`0.5C`) | Realized calls/cell (`1 + 1.1C`) |
|---:|---:|---:|---:|
| 1 | 0.6 | 0.5 | **2.1** |
| 2 | 1.2 | 1.0 | **3.2** |
| 3 | 1.8 | 1.5 | **4.3** |

The `cycles=3` row (4.3 = 1 writer + 1.8 reviewer + 1.5 fixer) is the anchor
that reproduces the published full-scope ~$1,197 projection. **The frozen design
runs cycles=1 → 2.1 calls/cell (1 writer + 0.6 reviewer + 0.5 fixer), a −51% cut
on the dominant overhead term** vs the cycles=3 anchor.

`runs_per_cell` (R) is the other lever: writer invocations — and therefore the
whole agent-invocation count and the Foundry Tools overhead — scale **linearly**
with R. The phase-3 base of 5,400 writer invocations is `300 cells × 6 models ×
3 runs`, i.e. R=3. **Cutting runs 3 → 1 divides the overhead base by 3.**

### Bill-calibrated cost model

Per config, combined cost = Foundry Tools overhead + Foundry Models (token):

- **Foundry Tools overhead** = `total_agent_invocations × $0.03375` (May anchor:
  $182.26 / 5,400 phase-3 writer calls). `total_agent_invocations = 1,800·R ×
  (1 + 1.1·C)`.
- **Foundry Models (token)** = per-role token-list × `1.95` recon factor. Role
  token-list rates are derived from the runs=3/cycles=3 anchors (writer $82.19,
  reviewer $50, fixer $80) and scale with each role's invocation count.
- **Billing split:** token spend keeps the phase-3 marketplace fraction (~72%
  marketplace: codestral + llama + grok); the Foundry Tools overhead is wholly
  credit (Azure-side agent runtime). The **combined** total is what the cap
  measures and is split-independent.

### The decision: freeze cycles=1, dispatch calibration as run_1, keep the full 6-model panel

Jasper's directive (2026-06-10): preserve the cross-model comparison — **never
drop models** — and cut cost via review cycles, with the 3-run target dispatched
incrementally so the calibration spend is not repeated. **`max_review_cycles` is
frozen at 1**; the calibration is **run_1 of the real 3-run set**, not a throwaway.

| Config | runs | cycles | Combined | % of $250 cap | To card | Notes |
|---|---:|---:|---:|---:|---:|---|
| **A — run_1 calibration** (first dispatch) | 1 | 1 | **~$209** | **84% (under cap)** | ~$59 | first measured multi-agent point; poolable into B |
| **B — full 3-run set** (runs 2+3 after go/no-go) | 3 | 1 | **~$628** | 251% | ~$478 | run_1 + runs 2+3, pooled final result set |
| **C — original full scope** (reference, pre-freeze) | 3 | 3 | **~$1,197** | 479% | ~$1,047 | the pre-freeze number we cut from |

Freezing cycles at 1 takes the **calibration** projection from the old cycles=2
~$304 down to **~$209 — under the $250 combined cap** (implied card spend only
~$59, well inside the monthly $150 credit), making run_1 a clean go. The pooled
full 3-run set (Config B) lands at **~$628** (251% of cap, ~$478 to card); that
is the real go/no-go decision after run_1 reports its measured bill. Config C is
the pre-freeze reference only.

### Calibration is run_1 (reusability discipline)

The calibration dispatch **is** run_1 of the frozen 3-run design, so the spend
is not duplicated. run_1 is poolable with runs 2+3 **only if the harness,
prompts, and config are frozen at one SHA and nothing changes after
calibration.** Any prompt edit, cycle-count change, or model swap after
calibration invalidates run_1 as a member of the 3-run set and forces a re-run.

**Sequence:** smoke test (1 cell, correctness, <$0.10) → **run_1 = calibration**
on the sealed harness (real adapter, full 6-model panel, cycles=1) → measure the
actual bill across all meters → go/no-go vs the soft $150–250 combined cap →
dispatch **runs 2+3** with identical config. Calibration replaces the *derived*
Foundry Tools overhead factor in `tools/cost/estimate.py` with a *measured* one —
phases 2/3 were single-agent, so the reviewer+fixer tool traffic (the $182 May
Foundry Tools line) has never been directly measured and cannot be substituted
from prior-phase data.

> **Honest caveat:** the Foundry Tools overhead carries a known phase-2
> over-attribution (some of the May $182 belongs to phase-2 agentic runs).
> Halving it lowers every figure ~33–40% (Config A → ~$210) but does not change
> the ranking or the conclusion that full scope blows the cap. The headline
> figures above use full attribution (conservative for a go/no-go).

## Azure freeze

**No real Azure dispatch before 2026-06-08** (3 weeks from session date
2026-05-18). The freeze exists to:

1. Let the phase 3 final numbers settle in `main` and in the README so
   the cross-phase narrative is stable before adding phase 4.
2. Give the Azure cost-management reconciliation window (typically 7-14
   days) time to post the actual phase 3 bill so we can re-calibrate the
   phase 4 projection.
3. Avoid concurrent generation runs on the same Foundry account — the
   per-account rate limits in eastus2 are tight enough that two sweeps
   in flight at once would slow each other down.

Until 2026-06-08, only the **mock adapter** path is allowed. The smoke
test (`tools/generation/tests/test_multi_agent_smoke.py`) exercises the
entire writer / reviewer / fixer loop end-to-end against fixture
responses so the runner can be developed and reviewed without any
Foundry calls.

## Pre-flight checklist (run BEFORE 2026-06-08 dispatch)

- [ ] Phase 3 Azure bill posted and reconciled in [phase 3 COSTS](../phase3-agentic-loop/COSTS.md).
- [ ] Foundry credit balance >= projected phase 4 spend × 1.5.
- [ ] Tripwire alert "phase4-tripwire-250" created on the Foundry account
      (50 / 75 / 90 / 100% thresholds, email + webhook).
- [ ] Mock-adapter smoke test green on `main`.
- [ ] At least one **paid** smoke run (1 cell, 1 model, mock adapter
      replaced with real foundry adapter) completes end-to-end and lands
      a JSONL artifact in `phases/phase4-multiagent/results/`.
- [ ] Open a pre-flight PR with the dispatch plan (target count, model
      list, run count) and get explicit go/no-go review.

Once the above is green, the production dispatch goes through
`phase4-generate.yml` with `mock_llm: "false"` and the real model panel.
