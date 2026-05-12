# Phase 2 — Single Agent, No Feedback

> **Status: pilot complete.** 7-model panel, 5 cells, 1 run. One agent per cell, given exploration tools but **no compile or test feedback** — the agent submits once and the loop ends. Phase 3 closes that gap by feeding compile errors back as additional turns. This phase is the no-feedback baseline that phase 3 will be measured against.

See also:
- [COSTS.md](COSTS.md) — what this experiment cost on Azure (~$0.57 to date) and how to read it
- [REPLICATION.md](REPLICATION.md) — one-page recipe for outside readers
- [phase.lock.yaml](phase.lock.yaml) — exact inputs / hyperparameters
- [results/](results/) — per-model raw outputs (JSONL + generated tests + per-turn forensics)

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
