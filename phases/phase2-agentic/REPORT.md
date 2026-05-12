# Phase 2 — Single Agent, No Feedback

> **Status: pilot complete, full run pending.** 7-model panel, 5 cells of 3,147, 1 run. The pilot establishes per-model unit costs, validates the harness, and surfaces the qualitative finding that compile is the bottleneck. It is **not** a publishable result — the full run (target ~150 stratified cells, 3 runs each) is what gets compared against phase 3's compile-feedback delta. See [Sample size](#sample-size-why-5-cells-and-whats-next) below.

See also:
- [COSTS.md](COSTS.md) — what this experiment cost on Azure (~$0.57 to date) and how to read it
- [REPLICATION.md](REPLICATION.md) — one-page recipe for outside readers
- [phase.lock.yaml](phase.lock.yaml) — exact inputs / hyperparameters
- [results/](results/) — per-model raw outputs (JSONL + generated tests + per-turn forensics)

## Vocabulary

These terms get used loosely in the agent literature — here's what they mean in *this* phase specifically:

- **Cell** — one (model, target) pair to be evaluated. The pilot has 35 cells (7 models × 5 targets).
- **Run** — one independent execution of a cell. Same model, same target, same prompt, same temperature — fired separately. Repeated because the same model can produce different outputs on the same input: GPU floating-point nondeterminism (batched matmul reduction order shifts with concurrent traffic), reasoning-model sampling (gpt-5-codex's internal reasoning trace varies even at temperature 0), and provider routing (Foundry can route the same deployment ID to different snapshots or replicas). With multiple runs you can report mean ± stddev rather than a single number that might be luck. The pilot uses 1 run; the full run will use 3 (the conference-paper convention).
- **Turn** — one round-trip to the model *within a single run*. The agent emits text, we parse a `<tool>...</tool>` call out of it, run the tool (read a file, list a dir, or accept the submitted test), append the tool result to the transcript, and ask the model again. A turn is bounded compute, not bounded thinking; the model may emit lots of reasoning per turn.
- **Read** — one successful `read_file` call within a turn. Bounded separately from turns (max 5) so a model can't burn its whole budget re-reading the same file.
- **Submission** — one `submit_test` call. There is exactly one per run. The instant it fires, the loop ends and that run is done.
- **Iteration** — what an *agentic loop* (phase 3+) will add and what this phase deliberately does NOT have: submit → compile → feed errors back → revise → resubmit. Phase 2 has zero iterations by design. Every "compile failed" outcome below is final.

Hierarchy: phase contains many **cells**; each cell has multiple **runs**; each run has multiple **turns**; some turns contain a **read**; exactly one turn per run contains the **submission**. Phase 3 will add **iterations** as a new level between submission and end-of-run.

## Sample size: why 5 cells, and what's next

The full target set is **3,147 uncovered Mode#1 sites** across the 15 cloned repos (frozen in [`targets/v1/targets.csv`](../../targets/v1/targets.csv)). This phase ran 5 of them. That's a deliberate pilot, not the experiment.

**Why a 5-cell pilot first:**

1. **Cost reconnaissance.** Per-cell cost wasn't known a priori — gpt-5-codex turned out to spend ~$0.10/cell on internal reasoning before submitting. A naive full sweep (3,147 × 7 models × 5 runs ≈ 110k attempts) would have run ~$2,200 just for codex, $300+ for the rest. Pilot first, learn the unit costs, then scope the full sweep.
2. **Harness shakedown.** Most of this phase's elapsed time was in finding out the evaluator must build *inside* the cloned repo (otherwise NU1100/NU1605 mask real CS errors), that `/responses` needs ≥16k output budget for reasoning headroom, that codestral wraps fences differently than the OpenAI surfaces, that aspnetcore's source-build refuses external `ProjectReference`s. Running 110k cells against a broken harness would have burned the budget without producing valid signal.
3. **Cells span 5 of the 15 repos.** Not a random sample, but they hit different concrete static-method patterns: an `ILogger` extension, an `IServiceProvider` extension, a `LogError` extension, a `GetConnectionString` extension, and an `AzureAIAgent.LogInformation`. Enough variety to expose the most common failure modes (CS0246 missing using, CS7036 wrong constructor) at very low cost.

**This is not a publishable result yet.** The 0/21 compile rate is a strong qualitative signal — it tells us compile is the bottleneck and motivates the phase 3 (compile-feedback) experiment — but n=5 is far too small for any per-model claim. With one cell per model per repo there is no within-condition variance to estimate, no stratified comparison across repos, and no way to separate model-skill from cell-difficulty.

### Sample sizes in comparable empirical SE work

Rough targets in the LLM-test-generation literature:

| Bar | Cells / classes | Examples |
|---|---|---|
| Minimum credible (non-parametric tests viable) | **30–50** | small workshop papers |
| Conference-paper baseline | **100–300** | most ICSE/FSE/ASE LLM-testgen submissions; SF110-derived subsets |
| Strong | **300–500** with 3–5 runs each for variance | CodaMosa, ChatUniTest, A3Test |
| Comprehensive benchmark | full population | Defects4J (835 bugs), SF110 (~24k classes) |

Convention is also to report a **stratified sample** rather than a uniform one — partition by repo, by API-surface family (logger / DI / config / database), and by cyclomatic complexity bucket of the type-under-test, then sample proportionally so no one bucket dominates the headline.

### What phase 2 needs before it can be sealed

- **Bump cells to ~150** (conservative conference-paper baseline). Stratified sample of the 3,147: ~10 cells per repo × 15 repos. Estimated cost: ~$30 for the cheap models, plus $50–80 if codex stays in the panel — roughly within the $50/mo budget if codex is sampled at ~20 cells instead of all 150.
- **Bump runs per cell to 3** for variance estimation. Temperature is currently 0.0 so per-call variance is limited to provider-side nondeterminism, but 3 runs lets us at least bound it.
- **Drop or sample codex aggressively.** It contributed 83% of phase spend ($0.47 of $0.57) and submitted nothing. Reasonable choices: include it on a 20-cell sub-sample only, or hold it for phase 3 once compile-feedback is in (where its reasoning budget might actually pay off).
- **Re-include phi-4 once Foundry capacity stabilizes** (it timed out on every call this run after working in smoke).

That's "phase 2 — full run" before moving to phase 3. The pilot tells us what to spend; the full run produces the result that's safe to compare against phase 3's compile-feedback delta.

## Strategy

Each model gets a multi-turn **exploration** budget with three tools:

  - `<tool>read_file(path)</tool>` — repo-relative read, sandboxed to the target repo root
  - `<tool>list_dir(path)</tool>` — list children
  - `<tool>submit_test(csharp)</tool>` — emit final fenced code block; loop ends

Bounded: **6 turns max, 5 reads max** per cell. The agent can read the type-under-test, sibling files, or just submit immediately based on the source window provided. Crucially, **no compile or test feedback is fed back into the loop** — once `submit_test` fires, the cell is done. That makes this a "single-agent, no-feedback" baseline; phase 3 will close the loop by feeding compile errors back as additional turns.

Identical prompt for all seven models. The same prompt is shipped to whatever the natural API surface is for that deployment:

- Azure OpenAI Chat Completions (`gpt-4.1-mini`, `gpt-4.1-nano`)
- Azure OpenAI Responses (`gpt-5-codex`, with reasoning-token headroom)
- Foundry Models Inference (`phi-4`, `codestral-2501`, `llama-3.3-70b-instruct`, `grok-4-1-fast`)

All seven deployments live in **one** Foundry account (`foundry-mockstatic`, `eastus2`), one resource group, one API key, one $50/mo budget alert.

## Pilot results

5 cells × 7 models = 35 attempts. **6 of 7 models** produced submissions; **0 of 21 evaluated tests** compile.

| Model | Submit | Compile | Run | Pass | Avg turns | Cost (USD) |
|---|---|---|---|---|---|---|
| `gpt-4.1-mini` | 5/5 | 0/5 | 0/5 | 0 | 2.6 | $0.0188 |
| `gpt-4.1-nano` | 2/5 | 0/5 | 0/5 | 0 | 4.8 | $0.0055 |
| `gpt-5-codex` | 0/5 | 0/5 | 0/5 | 0 | 3.4 | $0.4743 |
| `codestral-2501` | 4/5 | 0/5 | 0/5 | 0 | 5.2 | $0.0192 |
| `grok-4-1-fast` | 5/5 | 0/5 | 0/5 | 0 | 3.4 | $0.0108 |
| `llama-3.3-70b-instruct` | 5/5 | 0/5 | 0/5 | 0 | 3.4 | $0.0388 |
| `phi-4` | timeouts | — | — | — | — | $0.0027 |

The cells:

- `eShop:0001` — `LogInformation` extension on `ILogger` inside `RedisBasketRepository`
- `duplicati:0011` — `GetRequiredService` extension in `Duplicati.Library.RestAPI`
- `semantic-kernel:0001` — `LogInformation` in `AzureAIAgent`
- `orleans:0001` — `GetConnectionString` in `AdoNetClusteringProvider`
- `jellyfin:0001` — `LogError` in `Emby.Photos.PhotoProvider`

## What's failing

Compile-error histogram across the 21 evaluated submissions:

| CS code | What it means | Frequency |
|---|---|---|
| `CS0246` / `CS0234` | Type or namespace not found (missing `using` / wrong assembly) | most common |
| `CS1503` / `CS1501` / `CS1729` | Wrong overload / argument type mismatch | second most common |
| `CS7036` | Required constructor parameter missing | several |
| `CS0509` | Cannot derive from sealed/abstract type | several |
| `CS0122` | Member inaccessible (internal/private) | several |
| `CS0308` / `CS8917` | Generic type args / nullable mismatch | several |

The "compiles-but-doesn't-run" bucket is empty because **compile is the bottleneck**. Across six models with 5-read tool budgets, every submitted test hallucinates *something*: an extension method that doesn't exist, a constructor signature off by one parameter, a missing `using StackExchange.Redis;`, an attempt to inherit from a sealed type. Coverage delta is therefore zero — no tests ever execute.

## Per-model behavioral signal (the actual finding)

Even on identical prompts, the models behave very differently:

- **`gpt-4.1-mini`** is fastest (2.6 turns avg) and most decisive — but doesn't read enough to get the API right.
- **`codestral-2501`** uses the most turns (5.2) and most reads — yet still doesn't compile.
- **`gpt-5-codex`** is currently unusable on this scaffold: 0/5 submitted (a mix of safety-classifier refusals on real OSS source and reasoning-token exhaustion). Worth revisiting once the harness is harder to refuse.
- **`gpt-4.1-nano`** is the cheapest model that submits anything (~$0.001 per submitted test) but only 2/5.
- **`phi-4`** hit Foundry-side read timeouts on every call this run; reliably worked in smoke. Possibly capacity flakiness in eastus2.
- **`grok-4-1-fast`** and **`llama-3.3-70b-instruct`** are middle-of-the-pack on every metric but reliable on submission rate (5/5 each).

## Engineering decisions captured along the way

- **Text-based tool protocol** (`<tool>NAME(args)</tool>`) instead of native function calling — necessary for portability across the four non-OpenAI Foundry deployments which have inconsistent function-calling support.
- **Tolerant tool regex** — accepts `read_file(x)`, `read_file("x")`, `read_file(path="x")`, and bare `read_file x`. Several models drift off the strict syntax.
- **Conversation memory in the runner, not the adapter** — adapter is single-message-pair; the loop concatenates the transcript into each turn's user prompt.
- **`/responses` API needs a much larger `max_output_tokens`** — gpt-5 reasoning models burn the whole budget on internal reasoning before emitting visible text. Adapter bumps this to ≥16k for that surface only.
- **HTTP 429 retry with exponential backoff** in the adapter (Foundry throttles aggressively under fan-out).
- **Build the test project INSIDE the cloned repo, not in `/tmp`** — otherwise the repo's nuget.config and Directory.Build.props don't apply, and you get NU1100 / NU1605 restore failures masking real CS errors.
- **aspnetcore is not evaluable from outside** — its source-build infrastructure rejects external `ProjectReference`s. Substituted `jellyfin:0001` for the pilot.

## Forensics layout

For every (model, cell) pair, three artifact streams survive in `results/<model>/run_<i>/`:

| File | Schema | Use |
|------|--------|-----|
| `attempts.jsonl` | one line per cell | headline metrics: `turns_used`, `tool_calls{}`, `reads_done`, `submitted`, `halt_reason`, `total_prompt_tokens`, `total_completion_tokens`, `wall_ms` |
| `evaluation.jsonl` | one line per evaluated test | `compile_ok`, `run_ok`, `tests_passed/failed/skipped`, `compile_errors[]`, `coverage_target_file{lines_covered,lines_total,line_rate}` |
| `turns/<repo>/<target_id>.jsonl` | one line per turn | full per-turn forensics: assistant text, tool name + arg + ok flag, latency, tokens, finish reason |
| `generated_tests/<repo>/<target_id>/test.cs` | the submitted test file | the model's actual output, ready to feed into the next tier |

## Next tiers

The 0/21 compile rate is the headline. It strongly motivates the next phases:

1. **Phase 3 — agentic loop.** Same single agent, but compile errors are fed back as additional turns so the agent can fix its own output. Plausibly a large lift; cost roughly 3× this tier (more turns).
2. **Phase 4 — multi-agent (writer + reviewer).** A reviewer reads the candidate test and flags missing usings / wrong types before submission.
3. **Phase 5 — multi-team.** One team writes the spec; a separate team writes the test from that spec.

Each tier will land in its own `phases/phaseN-*` directory with its own COSTS.md so we can show how cost scales with sophistication.
