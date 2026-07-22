# Phase 4 — Agentic loop + testability refactoring tool: REPORT

> **Status: scaffold. No production runs yet — every number below is a
> PREDICTION.** Design lives in [PLAN.md](PLAN.md); replication recipe lives in
> [REPLICATION.md](REPLICATION.md); Azure dispatch is gated on the run_1 go/no-go.

## Per-model results

_Empty until the first production dispatch lands._

| Model | Attempts | Submitted | Compile OK | Run OK | Compile% | Run% | Refactored% | Token cost |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| _tbd_ | — | — | — | — | — | — | — | — |

## Cross-phase comparison (predicted — NOT yet run)

Will be filled once phase 4 has N runs against the same v2 300-cell target set.
The headline is **run-OK% A/B vs phase 3's 7.1% on the identical cells**. The only
difference between the arms is the `apply_refactor` capability.

| Metric | Phase 3 (measured) | Phase 4 (predicted) |
|---|---:|---:|
| Compile-OK% (blended) | 14.6% | 18–24% |
| Run-OK% (blended) | 7.1% | 12–18% |
| Refactor-attributable run-OK | — | the lift above 7.1% |
| `refactor_rejected` rate | — | < 25% (target) |
| Token cost (run_1) | — | ~$214 |

> These are predictions, not results. They will be replaced with measured values
> once phase 4 has dispatched.

## What we're testing

The hypothesis under examination is that **giving the proven phase-3 single agent
a constrained refactoring capability converts unmockable Mode #1 sites into
mockable ones**, lifting run-OK% above the 7.1% phase-3 ceiling. The lift, if any,
is attributable to the tool — the writer prompt stays generic, the model panel and
the harness are unchanged.

## Failure-bucket prediction

Phase 3's run failures split into those that are about **mockability** (a refactor
should convert them) and those that are not (a refactor should not move them). For
each phase-3 failure bucket, the predicted phase-4 outcome:

| Bucket | Phase 3 | Phase 4 prediction | Why a refactor does / doesn't help |
|---|---:|---:|---|
| unmockable Mode #1 (no seam) | large | **converts** | This is the bucket the tool exists for — `make_virtual` / `wrapper_interface` gives the test a substitution point |
| `other_exception` (DI / ctor) | 253 | partial | `parameterize_dependency` / `wrapper_interface` can inject a fake dependency; many still need code the agent can't see |
| `no_fact_methods` | 160 | small | Not a mockability problem — a seam doesn't make the agent write a `[Fact]`; out of scope for phase 4 (that's phase 5's reviewer) |
| `assertion_failed` | 53 | flat | Real failures — a seam doesn't conjure passing assertions |
| `null_ref` / `arg_null` | 22 / 24 | partial | If the null was an un-injectable dependency, the seam helps; otherwise flat |
| `invalid_op_runtime` | 35 | partial | Some are isolation-fixable, some environmental |

If the prediction holds, the run-OK lift is dominated by the **unmockable Mode #1**
conversion plus partial gains on the DI/ctor (`other_exception`) bucket where a
dependency can now be parameterized or wrapped. The `no_fact_methods` bucket is
explicitly **not** a phase-4 target — that structural problem is what phase 5
(reviewer) addresses.

## Reliability classification policy

Model failures are counted only for evaluative attempts where the model had a real
chance to generate and submit a candidate. Infra/tooling failures (auth,
rate-limit, timeout, 5xx/service unavailable, network/provider incidents) are
classified as rerun-required reliability events and excluded from model failure
rates.

## Operational checklist for publishable runs

1. Detect infra-heavy buckets per model/run (auth/rate-limit/timeout/5xx/network).
2. Rerun only targeted affected IDs or affected model-run shards.
3. Re-aggregate all phase outputs after reruns land.
4. Publish only the re-aggregated report; mark interim outputs provisional.

## What this report will contain post-dispatch

1. **Per-model success table** (same shape as phase 3 HEADLINE, plus a
   `Refactored%` column).
2. **Cross-phase paired-bar chart** — phase 2 vs 3 vs 4 run-OK per model.
3. **Refactor-attributable breakdown** — cells that pass ONLY when a refactor was
   applied (run-fail in phase 3, legitimate run-OK in phase 4 through a seam).
4. **Transform-type success** — `make_virtual` vs `wrapper_interface` vs
   `parameterize_dependency`, by Mode #1 kind (EXT / NonVirtual).
5. **`refactor_rejected` analysis** — guard auto-revert rate and what kinds of
   edits tripped it.
6. **Cost reconciliation** — actual vs the ~$214 run_1 projection.
7. **Decisions captured** — anything we changed mid-flight, with justification.

## Legitimacy audit

Because phase 4 lets the agent edit production code, the report will include a
**legitimacy audit**: the fraction of compiling "passes" that were excluded from
the refactor-attributable metric for bypassing the target site or asserting
trivially. A healthy filter excludes some passes — if it excludes none, the filter
is suspect.

## Sandbox discrepancy

The runner rebuilds the single owning csproj from the (transiently) refactored
source; the canonical evaluator (production csproj) remains the headline metric, as
in phase 3.

## See also

- [PLAN.md](PLAN.md) — design + anti-gaming + lifecycle
- [REPLICATION.md](REPLICATION.md) — one-page recipe
- [phase 3 REPORT](../phase3-agentic-loop/REPORT.md) — the 7.1% run-OK baseline we're trying to beat
- [phase 5 PLAN](../phase5-multiagent/PLAN.md) — the multi-agent phase that follows
