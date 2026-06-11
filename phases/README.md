# Phases

Each phase varies one thing — the test-generation strategy — against a fixed input set ([`targets/v2/targets.csv`](../targets/v2/targets.csv), 300 stratified cells across 12 repos) and a fixed model panel. The 300-cell v2 sample is frozen across phases 2+; only the strategy changes.

## The pattern

| Phase | Strategy | Status |
|---|---|---|
| 1 — baseline | No generation. Measure existing test suites against detected Mode#1 sites. | ✅ sealed ([`phase1-baseline/`](phase1-baseline/)) |
| 2 — agentic, no feedback | One agent with `read_file` / `list_dir` / `submit_test` tools, ≤6 turns. Can explore the repo before submitting; never sees its own compile or test output. | ✅ v2 sweep complete — 300 cells × 3 runs × 7 models = 6,300 attempts ([`phase2-agentic/`](phase2-agentic/)) |
| 3 — agentic loop | Same single agent as phase 2, but compile errors **and `dotnet test` results** are fed back as additional turns so the agent can fix its own output, ≤4 submissions per cell. | ✅ v2 sweep complete — 300 cells × 3 runs × 6 models = 5,400 attempts ([`phase3-agentic-loop/`](phase3-agentic-loop/)) |
| 4 — agentic loop + testability refactoring | The phase-3 single agent, plus an `apply_refactor` tool that can introduce a testability seam into the production code (extract-and-override, wrapper interface, dependency parameterization) before testing it. Isolates the effect of a refactoring *capability*; prompts stay generic. | design in progress |
| 5 — multi-agent | Writer / reviewer / fixer specialist agents collaborate per target. | 🟡 scaffold only — design in [`phase5-multiagent/PLAN.md`](phase5-multiagent/PLAN.md); Azure dispatch frozen until ~2026-06-08 |

Every phase 2+ runs the **same 300-cell v2 sample** so cross-phase deltas reflect generation strategy, not target drift.

### Model panel (as run)

The panel has evolved with the experiment. Per-phase exact rosters live in each phase's `phase.lock.yaml`.

| Slot | Model id | Notes |
|---|---|---|
| 1 | `codestral-2501` | Mistral code model — phases 2 & 3 |
| 2 | `gpt-4.1-mini` | OpenAI mid-tier — phases 2 & 3 |
| 3 | `gpt-4.1-nano` | OpenAI small — phases 2 & 3 |
| 4 | `grok-4-1-fast` | xAI — phases 2 & 3 (phase 3 leader) |
| 5 | `llama-3.3-70b-instruct` | Meta — phases 2 & 3 |
| 6 | `phi-4` | Microsoft small — phases 2 & 3 |
| 7 | `gpt-5-codex` | OpenAI reasoning — phase 2 only. Dropped from phase 3+ on cost grounds (82% of phase 2 spend, since removed from Azure AI Foundry). |

All models served via Azure AI Foundry (OpenAI-compatible endpoint).

### Per-cell parameters (frozen)

- `temperature: 0.0`, `top_p: 1.0`, `seed: 42`, `max_output_tokens: 4096`.
- `runs_per_cell: 3` (conference-paper convention) — every reported number is across the 3 runs of that (model, target) cell.

## Per-phase workflow file

Each phase ships **its own** GitHub Actions workflow:

```
.github/workflows/
  coverage-orchestrator.yml         # always current — measures whatever's there
  phase2-agentic.yml                # frozen at seal of phase 2
  phase3-agentic-loop.yml           # frozen at seal of phase 3
  phase4-refactoring.yml            # planned — agentic loop + refactoring tool
  phase5-{generate,evaluate,aggregate}.yml   # scaffolded (multi-agent), defaults to mock adapter
```

Why one workflow per phase, not one parameterized workflow:

- An older strategy can be re-run by checking out its git tag and dispatching its workflow. No `if:` branches, no flags, no chance of accidental cross-contamination.
- The workflow file is listed in `phase.lock.yaml.infrastructure.generator_workflow_sha` — re-running a phase always means dispatching the file at exactly that SHA.
- Future-you fixing a bug in `phase5-generate.yml` cannot accidentally change phase 2's behavior.

## To start a new phase

```bash
cp -r phases/_template phases/phaseN-name
cp .github/workflow-templates/phase.yml .github/workflows/phaseN-name.yml
# edit prompt/, phase.lock.yaml, and the workflow file
gh workflow run .github/workflows/phaseN-name.yml
```

After the run completes, fill in `phase.lock.yaml` (especially `model_snapshots_observed` and `ci_runs.generator_run_ids`), write `REPORT.md` + `COSTS.md` + `REPLICATION.md`, append a row to [`../RESULTS.md`](../RESULTS.md), and tag `phase-N-name-final`.

## Reproducibility envelope

What's reproducible:
- ✅ The harness (workflow + adapter + prompt files + target set).
- ✅ The aggregate distribution of outcomes (per-model mean ± stddev) within ~2σ.
- ✅ Every input that fed the experiment (every SHA, every prompt byte, every model id).

What's not:
- ❌ The exact tokens emitted by closed-weight models. Even at temperature 0 there is gateway-level non-determinism (load balancing, KV-cache batching, GPU non-associativity).
- ❌ The exact model snapshot if the canonical run pre-dates a snapshot retirement. Mitigation: `model_snapshots_observed` records what was actually used at run time.
