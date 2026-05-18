# Phase `phase3-agentic-loop` — headline

Target set: **v2** (300 cells × 3 runs = 900 attempts per model)

> **Status: final.** All three runs landed (gen + canonical evaluator), and
> the 9-cell fix shard (`llama-3.3-70b-instruct × duplicati × run_3`)
> plus a 1-cell fix shard (`gpt-4.1-nano × efcore × run_3`) that
> failed at container init in the original sweep have been re-dispatched
> and merged. Numbers are **canonical evaluator** counts, not the in-loop
> runner self-reports (see [REPORT.md](REPORT.md#sandbox-discrepancy) for
> the difference).

## Per-model results (all 3 runs)

| Model | Attempts | Submitted | Compile OK | Run OK | Compile% (of attempts) | Run% (of attempts) |
|---|---:|---:|---:|---:|---:|---:|
| `grok-4-1-fast`          |   900 | 899 | 240 | 133 | **26.7%** | **14.8%** |
| `gpt-4.1-mini`           |   900 | 637 | 173 | 109 | 19.2% | 12.1% |
| `llama-3.3-70b-instruct` |   900 | 894 | 121 |  52 | 13.4% |  5.8% |
| `codestral-2501`         |   900 | 855 | 146 |  43 | 16.2% |  4.8% |
| `phi-4`                  |   900 | 869 |  65 |  30 |  7.2% |  3.3% |
| `gpt-4.1-nano`           |   900 | 701 |  42 |  19 |  4.7% |  2.1% |
| **TOTAL**                | **5,400** | **4,855** | **787** | **386** | **14.6%** |  **7.1%** |

Definitions are identical to [phase 2's HEADLINE](../phase2-agentic/HEADLINE.md).

## Phase 3 vs phase 2 same-panel comparison

| Metric | Phase 2 (no feedback) | Phase 3 (compile + run feedback) | Gain |
|---|---:|---:|---:|
| Compile-OK% (blended, 6-model panel) | 4.8% | 14.6% | **3.0×** |
| Run-OK% (blended, 6-model panel) | 1.4% | 7.1% | **5.1×** |
| Token cost | $16.58 (ex-codex) | $82.19 | 4.96× |
| Cost per green test | $0.221 | $0.213 | (flat) |

The in-loop feedback is a strict pareto improvement: **5.1× more passing tests
at 4.96× the cost** — same cost-per-green-test, but five times the green tests
landed. Compile gains are real; run gains are larger than compile gains (the
feedback loop helps with both syntax and runtime bugs).

See [`assets/figures/phase2-vs-phase3.png`](../../assets/figures/phase2-vs-phase3.png)
for the visual comparison.

## Phase 3 total spend

**$82.19** USD across 5,400 attempts. Well under the $250 tripwire.
`llama-3.3-70b-instruct` is the most expensive at $33.02 — 40% of phase 3
spend — driven by the 70B model's prompt-token-heavy multi-turn fix loop.
See [COSTS.md](COSTS.md) for the per-model cost breakdown and projections.
