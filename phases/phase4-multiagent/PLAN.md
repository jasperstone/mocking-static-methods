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
| `max_review_cycles` | 3 | Each cycle is one (review, fix, check) triple. 3 is empirically the point where phase 3 fix-loop returns plateaued. |
| `max_turns` (per agent) | 6 | Lower than phase 3's 12 because each agent has a narrower job. |
| `max_reads` (per agent) | 4 | Half of phase 3 — most cells in phase 3 used ≤4 reads in the writer phase. |
| `max_attempts` (submissions) | 4 | Same as phase 3. |
| `run_timeout_s` | 60 | Same as phase 3. |

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

Phase 3 cost $82.19 for 5,400 attempts. Multi-agent multiplies tokens
roughly by **(1 + review_cycles × 2)**: each cycle adds a reviewer call
and a fixer call. With `max_review_cycles = 3` and typical cycle count
~1.8 (most cells will pass review on cycle 1 or 2), the effective
multiplier is ~4.6×.

| Source | Estimate | Notes |
|---|---:|---|
| Writer | $82.19 | Same as phase 3 single agent |
| Reviewer (avg 1.8 cycles) | $50 | Shorter prompts, fewer tokens than writer |
| Fixer (avg 1.5 cycles) | $80 | Similar to writer; produces full test file each time |
| **Phase 4 total** | **~$210** | Within $250 tripwire |

If the writer / reviewer / fixer turn counts hold to projection, phase 4
clears the tripwire with $40 headroom. If reviewer cycles average 2.5+
instead of 1.8, phase 4 lands at ~$260 and we cut runs from 3 to 2.

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
