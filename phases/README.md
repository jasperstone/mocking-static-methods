# Phases

Each phase varies one thing — the test-generation strategy — against a fixed input set ([`targets/v1/targets.csv`](../targets/v1/targets.csv)) and a fixed model panel.

## The pattern

| Phase | Strategy | Status |
|---|---|---|
| 1 — baseline | No generation. Measure existing test suites. | ✅ sealed ([`phase1-baseline/`](phase1-baseline/)) |
| 2 — single-shot | One LLM prompt → one test file. No feedback loop. | not started |
| 3 — single-agent loop | One agent: compile → fix → run → fix, ≤5 iterations. | not started |
| 4 — multi-agent | Specialist agents (writer / reviewer / fixer) per target. | not started |
| 5 — multi-team | Multiple multi-agent teams compete or partition the target set. | not started |

Every phase 2+ runs the **same panel** — 5 models × 5 runs each = 25 generation cells per phase — so cross-phase claims like "the loop helps GPT-5 more than it helps Claude" are valid.

### Model panel (frozen for the experiment)

| Slot | Model id | Why included |
|---|---|---|
| 1 | `anthropic/claude-opus-4-5` | Anthropic frontier (lineage 1) |
| 2 | `anthropic/claude-sonnet-4-5` | Anthropic mid-tier — controls capability axis within Anthropic |
| 3 | `openai/gpt-5` | OpenAI frontier (lineage 2) |
| 4 | `openai/o3` | OpenAI reasoning — controls training-paradigm axis within OpenAI |
| 5 | `google/gemini-2-5-pro` | Google frontier (lineage 3) |

All five are available via [GitHub Models](https://models.github.ai/inference) using the same OpenAI-compatible endpoint. Anyone with a GitHub account can re-run any phase.

### Per-cell parameters (frozen)

- `temperature: 0.0`, `top_p: 1.0`, `seed: 42`, `max_output_tokens: 4096`.
- `runs_per_model: 5` for noise estimation. Every reported number is mean ± stddev across the 5 runs of that cell.

## Per-phase workflow file

Each phase ships **its own** GitHub Actions workflow:

```
.github/workflows/
  coverage-orchestrator.yml         # always current — measures whatever's there
  phase2-singleshot.yml             # frozen at seal of phase 2
  phase3-singleagent.yml            # frozen at seal of phase 3
  phase4-multiagent.yml             # frozen at seal of phase 4
  phase5-multiteam.yml              # frozen at seal of phase 5
```

Why one workflow per phase, not one parameterized workflow:

- An older strategy can be re-run by checking out its git tag and dispatching its workflow. No `if:` branches, no flags, no chance of accidental cross-contamination.
- The workflow file is listed in `phase.lock.yaml.infrastructure.generator_workflow_sha` — re-running a phase always means dispatching the file at exactly that SHA.
- Future-you fixing a bug in `phase4-multiagent.yml` cannot accidentally change phase 2's behavior.

## To start a new phase

```bash
cp -r phases/_template phases/phase2-singleshot
cp .github/workflows/_phase-template.yml .github/workflows/phase2-singleshot.yml
# edit prompt/, phase.lock.yaml, and the workflow file
gh workflow run .github/workflows/phase2-singleshot.yml
```

After the run completes, fill in `phase.lock.yaml` (especially `model_snapshots_observed` and `ci_runs.generator_run_ids`), write `REPORT.md`, append a row to [`../RESULTS.md`](../RESULTS.md), and tag `phase-N-name-final`.

## Reproducibility envelope

What's reproducible:
- ✅ The harness (workflow + adapter + prompt files + target set).
- ✅ The aggregate distribution of outcomes (per-model mean ± stddev) within ~2σ.
- ✅ Every input that fed the experiment (every SHA, every prompt byte, every model id).

What's not:
- ❌ The exact tokens emitted by closed-weight models. Even at temperature 0 there is gateway-level non-determinism (load balancing, KV-cache batching, GPU non-associativity).
- ❌ The exact model snapshot if the canonical run pre-dates a snapshot retirement. Mitigation: `model_snapshots_observed` records what was actually used at run time.
