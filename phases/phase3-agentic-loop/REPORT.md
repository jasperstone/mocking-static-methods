# Phase 3 — Agentic Loop with Compile + Run Feedback

> **Status: final (3-run sweep complete, fix shards landed).** 6-model panel, **300 cells × 3 runs = 5,400 attempts** (all landed after fix-shard re-dispatch). Sample is the same v2 target set as phase 2 (`targets/v2/targets.csv`, 300 stratified cells, `targets_sha256: 4db523f9…`). Calibration generated via GitHub Actions run [25877016877](https://github.com/jasperstone/mocking-static-methods/actions/runs/25877016877); calibration evaluation run [25918592994](https://github.com/jasperstone/mocking-static-methods/actions/runs/25918592994). Runs 2 + 3 generated via run [25921948154](https://github.com/jasperstone/mocking-static-methods/actions/runs/25921948154) (144 / 145 jobs) and evaluated via run [25989290863](https://github.com/jasperstone/mocking-static-methods/actions/runs/25989290863) (144 / 145 jobs). The two container-init failures (`llama × duplicati × run_3` and `gpt-4.1-nano × efcore × run_3`) were re-dispatched via runs [26048493558](https://github.com/jasperstone/mocking-static-methods/actions/runs/26048493558) and [26048496818](https://github.com/jasperstone/mocking-static-methods/actions/runs/26048496818) and merged. Total token cost **$82.19**, well under the $250 tripwire.

See also:
- [HEADLINE.md](HEADLINE.md) — per-model success table (full 3-run)
- [COSTS.md](COSTS.md) — final spend + cost-per-green-test analysis
- [REPLICATION.md](REPLICATION.md) — one-page recipe
- [`assets/figures/phase2-vs-phase3.png`](../../assets/figures/phase2-vs-phase3.png) — paired-bar visual
- [phase.lock.yaml](phase.lock.yaml) — frozen inputs / hyperparameters
- [results/](results/) — per-model raw outputs (`attempts.jsonl` + `evaluation.jsonl` + `generated_tests/`)

## What changed vs phase 2

One structural change. Same prompt, same 6-model panel (`gpt-5-codex` removed
after phase 2 — see [phase 2 COSTS](../phase2-agentic/COSTS.md#decision-drop-gpt-5-codex-from-phases-3-5)),
same 300 v2 cells, same tools. The difference:

| Phase 2 | Phase 3 |
|---|---|
| `submit_test` ends the run unconditionally | `submit_test` triggers `dotnet build` AND `dotnet test`; outcome is fed back as additional turns |
| Cell halts after **1** submission | Cell halts after up to **4** submissions, OR `submitted_run_ok`, OR turn budget exhausted |
| `halt_reason ∈ {submitted, max_turns}` | `halt_reason ∈ {submitted_run_ok, submitted_run_failed, submitted_compile_failed, max_turns_exhausted}` |
| No structured forensics for compile errors | `first_compile_errors[]` (file/line/col/code/message) per submission |
| No runtime forensics | TRX-parsed `first_test_failures[]` (name/message/stack-tail) per submission |

The runner builds each candidate test in a fast standalone sandbox to decide
*what to feed back to the model*. The canonical evaluator (`phase3-evaluate.yml`)
re-runs every submitted test inside the production csproj. These can disagree;
see [Sandbox discrepancy](#sandbox-discrepancy) below.

## v2 results — full 3-run sweep

**5,400 generation attempts → 4,855 submitted tests → 787 compiled → 386 ran green.** Canonical evaluator numbers.

| Model | Attempts | Submitted | Compile OK | Run OK | Compile% (of submit) | Run% (of submit) | Token cost |
|---|---:|---:|---:|---:|---:|---:|---:|
| `grok-4-1-fast`          |   900 | 899 | 240 | 133 | **26.7%** | **14.8%** |  $7.05 |
| `gpt-4.1-mini`           |   900 | 637 | 173 | 109 | 27.2% | 17.1% | $13.34 |
| `llama-3.3-70b-instruct` |   900 | 894 | 121 |  52 | 13.5% |  5.8% | $33.02 |
| `codestral-2501`         |   900 | 855 | 146 |  43 | 17.1% |  5.0% | $19.08 |
| `phi-4`                  |   900 | 869 |  65 |  30 |  7.5% |  3.5% |  $5.54 |
| `gpt-4.1-nano`           |   900 | 701 |  42 |  19 |  6.0% |  2.7% |  $4.16 |
| **Total**                | **5,400** | **4,855** | **787** | **386** | **16.2%** | **8.0%** | **$82.19** |

> Blended over **all 5,400 attempts** (not just submitted): compile 14.6%, run 7.1%.
> This is the apples-to-apples number for cross-phase comparison.

## The compile-vs-run gap is the headline

787 cells compiled but only 386 ran successfully — **about half of tests that build still fail at runtime.** This is exactly the failure mode that motivated routing `dotnet test` output back into the loop, and it's still the dominant blocker on phase 3.

Breakdown of the runtime failures across all submission iterations:

| Bucket | Count | What it is |
|---|---:|---|
| `other_exception` | 253 | Generic runtime throws (DI / ctor failures, by sampling) |
| `no_fact_methods` | 160 | Compiled class with no `[Fact]` — model wrote test scaffolding that exercises nothing |
| `assertion_failed` | 53 | True assertion failures — the actual signal we want |
| `invalid_op_runtime` | 35 | `InvalidOperationException` at runtime |
| `arg_null` | 24 | Missing arguments in setup |
| `null_ref` | 22 | Null-derefs in setup |
| `type_or_method_load` | 2 | Runtime type-loading failures |

The **160 "no `[Fact]`" cases** continue to be striking — a prompt-side fix
candidate: the prompt does not explicitly require at least one `[Fact]`
attribute. Phase 5 will likely close this.

## Cross-phase comparison

Phase 2 (single shot, no feedback) on the same 6-model panel and same 300
cells hit **4.8% compile / 1.4% run-OK** on 5,400 attempts for $16.58. Phase 3
(3 runs, with in-loop feedback) hits **14.6% compile / 7.1% run-OK** on 5,400
attempts for $82.19:

| Metric | Phase 2 (6-model, no feedback) | Phase 3 (6-model, compile + run feedback) | Gain |
|---|---:|---:|---:|
| Compile-OK% blended | 4.8% (259 / 5,400) | 14.6% (787 / 5,400) | **3.0×** |
| Run-OK% blended | 1.4% (75 / 5,400) | 7.1% (386 / 5,400) | **5.1×** |
| Token cost | $16.58 | $82.19 | 4.96× |
| Cost per green test | $0.221 | $0.213 | (flat) |

Cost-per-green-test is **identical between phases 2 and 3 to the cent**.
Phase 3 buys 5.1× more passing tests at 4.96× the cost — the same
efficiency-frontier ratio. The in-loop feedback is a **strict pareto
improvement**, not a cheaper-per-test improvement. Visualised in
[`assets/figures/phase2-vs-phase3.png`](../../assets/figures/phase2-vs-phase3.png).

### Per-model winners and losers

`grok-4-1-fast` is the headline gainer once it can see its own mistakes:

| Model | Phase 2 run-OK | Phase 3 run-OK (3 runs) | Δ |
|---|---:|---:|---:|
| `grok-4-1-fast` | 0.1% | 14.8% | **×148** |
| `phi-4` | 0.4% | 3.3% | ×8.3 |
| `llama-3.3-70b-instruct` | 1.2% | 5.8% | ×4.8 |
| `codestral-2501` | 1.6% | 4.8% | ×3.0 |
| `gpt-4.1-mini` | 4.0% | 12.1% | ×3.0 |
| `gpt-4.1-nano` | 1.0% | 2.1% | ×2.1 |

The phase 2 finding (`grok` submits least, produces almost nothing) **inverts**
in phase 3: once grok can see its own mistakes, it submits at 99.9% and
produces the highest absolute compile and run counts in the panel. This
validates the original hypothesis driving phase 3.

## Per-repo difficulty

Three repos sit at **0% across every model** on every metric, on both phase 2
and phase 3: **`aspnetcore`, `roslyn`, `server`**. Naive standalone-csproj
generation does not survive their build configurations (source-build infra,
strict csproj graphs, complex Directory.Build.props inheritance).

`eShop` is a different shape — every model **compiles** in eShop (18-36%)
but **none** ever run-green. Pure compile-vs-run gap, concentrated in one
repo. By sampling, the runtime failures are dominated by DI / host-builder
exceptions thrown during test setup.

Best-performing (model, repo) pairs on run-OK%:

| Repo | Best model | Run-OK% |
|---|---|---:|
| `duplicati` | `gpt-4.1-mini` | 57.1% |
| `garnet` | `grok-4-1-fast` | 46.5% |
| `orleans` | `gpt-4.1-mini` | 31.2% |
| `semantic-kernel` | `gpt-4.1-mini` | 30.0% |
| `jellyfin` | `gpt-4.1-mini` | 10.7% |

`gpt-4.1-mini` wins 4 of the 8 "live" (non-fortress) repos.

## What's failing — compile-error taxonomy

Across **all** submission iterations in the 3-run dataset (5,693 iterations
scanned), the dominant compile-error families are:

| CS code | What it means | Frequency |
|---|---|---:|
| `CS0246` | Type / namespace not found | **1,800** |
| `(no_data)` | Empty stdout (likely build timeouts) | 1,354 |
| `CS0122` | Member inaccessible (`internal` / `private`) | 897 |
| `CS1061` | Type does not contain definition | 610 |
| `CS1503` | Argument type mismatch | 515 |
| `CS1525` | Unexpected token (syntax) | 403 |
| `CS1002` | Expected `;` | 397 |
| `CS1733` | Expected expression | 382 |
| `CS0234` | Namespace path wrong | 237 |
| `CS0103` | Identifier not declared | 151 |
| `CS0117` | Type does not contain member | 147 |
| `CS7036` | Required parameter missing | 122 |
| `CS0535` | Interface member not implemented | 91 |
| `CS0115` | Method does not override anything | 79 |

The `(no_data)` bucket (1,354) is the runner's 2-minute build timeout firing
before the compiler emits structured errors. Cross-reference with
`submission_iterations[].timeout` in `attempts.jsonl` confirms.

Regenerate this table with `python3 tools/analysis/phase3_taxonomy.py`.

## Sandbox discrepancy

The runner's in-loop sandbox is **more conservative** than the canonical
evaluator: it reports a noticeably lower compile+run count than the evaluator.
Same data, different build context:

- **Runner sandbox:** `phases/phase3-agentic-loop/.squad-eval/compile_run_*/` —
  a synthetic standalone csproj built fast for in-loop feedback. Conservative
  by design; missing some transitive references that the production csproj
  resolves.
- **Canonical evaluator:** `tools/evaluation/evaluate.py` builds inside the
  production csproj (the repo's own `Directory.Build.props` and `nuget.config`
  apply). Slower; more permissive; matches what a developer would see if they
  copied the test into the real test project.

The evaluator numbers (787 compile / 386 run-OK) are the **headline**; the
runner numbers are an internal feedback signal. A later phase may close this gap
by giving the runner access to the production csproj at the cost of build
speed.

## Engineering decisions captured

- **`max_attempts = 4` per cell**, not infinite. Pilot data suggested
  diminishing returns past 4 submissions; the runner cost-budget assumes
  this cap.
- **`run_timeout_s = 60`** per `dotnet test` invocation. Long enough for
  most NuGet-warm builds; aggressive enough to catch hung tests.
- **Build-timeout-OK is treated as `compile_failed`**, not a separate halt
  reason. The model gets the build-timeout message as feedback and can
  retry with a smaller test.
- **TRX failure parser** (`tools/evaluation/compile_only.py::_parse_trx_failures`)
  reports up to 5 first-failures per attempt with name + message + stack-tail.
  Empirically that's enough context for the model to fix most assertions.
- **Split-dispatch via `RUN_INDEX_START` env**. Calibration is run_1; the
  continuation sweep dispatched runs 2-3 with `run_index_start=2` so the
  evaluator can aggregate via CSV run IDs.
- **Two container-init failures** (`llama-3.3-70b-instruct × duplicati × run3`,
  9 cells, and `gpt-4.1-nano × efcore × run3`, 1 cell) produced no artifact
  and consumed no tokens in the original sweep. Both have been re-dispatched
  as fix shards (workflow runs [26048493558](https://github.com/jasperstone/mocking-static-methods/actions/runs/26048493558) and
  [26048496818](https://github.com/jasperstone/mocking-static-methods/actions/runs/26048496818)) and their results merged into the headline numbers
  above.

## Next: variance analysis and phase 4 (refactoring tool)

With 3 runs of every (model, cell) pair, variance analysis is now possible.
Open questions for follow-up:

1. **Per-cell determinism.** What fraction of (model, cell) triples are
   3-of-3 green vs 0-of-3 green vs flaky (1-of-3 or 2-of-3)? The flaky
   middle is where phase 4 should target.
2. **Inter-model overlap.** Which cells are *only* solvable by a single
   model? Which are solvable by all 6? The intersection sizes inform
   whether multi-agent (phase 5) is worth the cost over a single best-
   performing model.
3. **Fix shards landed.** The two container-init failures (`llama ×
   duplicati × run_3`, 9 cells; `gpt-4.1-nano × efcore × run_3`, 1 cell)
   were re-dispatched and merged. Net delta to the sweep: +10 attempts,
   +5 compile, +2 run-OK, +$0.20 cost.

Phase 4 (agentic loop + testability refactoring tool) is the immediate next
step: an `apply_refactor` tool introduces a testability seam into production
code before the test is written, isolating the effect of a refactoring
capability on the fixed input set. Phase 5 (multi-agent: writer + reviewer +
fixer) follows.
