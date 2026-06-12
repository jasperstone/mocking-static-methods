# Phase 5 — Multi-agent (writer / reviewer / fixer): REPORT

> **Status: scaffold. No production runs yet.** Design lives in
> [PLAN.md](PLAN.md); replication recipe lives in [REPLICATION.md](REPLICATION.md);
> Azure dispatch is frozen until ~2026-06-08.

## Per-model results

_Empty until the first production dispatch lands._

| Model | Attempts | Submitted | Compile OK | Run OK | Compile% | Run% | Token cost |
|---|---:|---:|---:|---:|---:|---:|---:|
| _tbd_ | — | — | — | — | — | — | — |

## Cross-phase comparison

Will be filled once phase 5 has 3 full runs against the same v2 target set.
Predicted lift (from PLAN.md): **2.0–2.5× run-OK over phase 3** (i.e.
~770–960 green tests on 5,400 attempts) for ~4.6× the token cost.

| Metric | Phase 2 | Phase 3 | Phase 5 (predicted) |
|---|---:|---:|---:|
| Compile-OK% (blended) | 4.8% | 14.6% | 25–30% |
| Run-OK% (blended) | 1.4% | 7.1% | 14–18% |
| Token cost | $16.58 | $82.19 | ~$210 |
| Cost per green test | $0.221 | $0.213 | $0.22–0.27 |

## What we're testing

The hypothesis under examination is that **a second agent reviewing the
first agent's draft catches structural mistakes that compile + run
feedback cannot.** Specifically, the 160 "no `[Fact]`" cells from phase
3 — where the writer produced a syntactically valid test scaffold that
contained zero executable assertions — should drop to near zero, because
the reviewer's system prompt explicitly checks for `[Fact]` count and
asks "does this test actually exercise the target method?"

## Failure-bucket prediction

For each phase 3 failure bucket, the predicted phase 5 outcome:

| Bucket | Phase 3 | Phase 5 prediction | Why |
|---|---:|---:|---|
| `no_fact_methods` | 160 | < 20 | Reviewer explicitly counts `[Fact]` attributes |
| `other_exception` | 253 | ~150 | Reviewer can flag obvious DI / ctor issues |
| `assertion_failed` | 53 | 53–80 | Real failures; multi-agent cannot conjure passing assertions out of nothing |
| `null_ref` | 22 | < 10 | Setup-style; reviewer prompt explicitly flags |
| `arg_null` | 24 | < 10 | Same |
| `invalid_op_runtime` | 35 | ~20 | Partly addressable |
| `type_or_method_load` | 2 | ~2 | Not addressable at agent level |

If the prediction holds, run-OK lift is dominated by the `no_fact_methods`
collapse (~140 newly-green cells) plus partial gains on the runtime
failure buckets (~100 more) for a total of ~240 additional run-OK on top
of phase 3's 386 = ~626 run-OK (1.62×). The conservative 2.0× estimate
in PLAN.md assumes additional wins from cells that phase 3 didn't even
submit (4,855 / 5,400 = 89.9% submit rate; phase 5 should push this
toward 95%+).

## What this report will contain post-dispatch

1. **Per-model success table** (same shape as phase 3 HEADLINE).
2. **Cross-phase paired-bar chart** — phase 2 vs 3 vs 5 run-OK per model.
3. **Reviewer verdict distribution** — what fraction of cycles were
   `APPROVE` vs `REQUEST_CHANGES`, and what the average cycle count
   looked like per model.
4. **Failure-bucket comparison** — phase 3 vs phase 5, for the
   8 buckets in the table above.
5. **Cost reconciliation** — actual vs $210 projection, per-agent
   token breakdown (writer / reviewer / fixer).
6. **Decisions captured** — anything we changed mid-flight, with
   justification.

## Sandbox discrepancy

The runner's in-loop sandbox carries forward from phase 3 (synthetic
standalone csproj, fast). The canonical evaluator (production csproj)
remains the headline metric.

## See also

- [PLAN.md](PLAN.md) — design + Azure freeze
- [REPLICATION.md](REPLICATION.md) — one-page recipe
- [phase 3 REPORT](../phase3-agentic-loop/REPORT.md) — what we're trying to beat
- [phase 3 COSTS](../phase3-agentic-loop/COSTS.md) — cost baseline
