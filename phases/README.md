# Phases

Each phase varies one thing — the test-generation strategy — against a fixed input set ([`targets/v2/targets.csv`](../targets/v2/targets.csv), 300 stratified cells across 12 repos) and a fixed model panel. The 300-cell v2 sample is frozen across phases 2+; only the strategy changes.

## Four overall phases

| Phase | Strategy | Status / artifacts |
|---|---|---|
| 1 — baseline | Measure existing test suites across the 15 screened repositories. | ✅ sealed · [`phase1-baseline/`](phase1-baseline/) |
| 2 — single agent, no feedback | One agent explores with `read_file` / `list_dir` and calls `submit_test`; it never sees compile or test output. | ✅ v2 sweep complete: 300 targets × 3 runs × 7 models = 6,300 attempts · [`phase2-agentic/`](phase2-agentic/) |
| 3 — single-agent feedback loop with refactoring | **3a control:** compile + run feedback without refactoring, up to 4 submissions. **3b:** the feedback loop with `apply_refactor` available. | 3a: ✅ 5,400-attempt sweep, **787 compile / 386 run-OK** · [`phase3-agentic-loop/`](phase3-agentic-loop/). 3b: implemented harness; checked-in report still predictive, no consolidated results committed · [`phase4-refactoring/`](phase4-refactoring/). |
| 4 — multi-agent | Writer / reviewer / fixer collaborate per target. | Scaffold · [`phase5-multiagent/PLAN.md`](phase5-multiagent/PLAN.md) |

### Historical IDs and result attribution

The four-phase presentation groups the existing feedback-only control and feedback-plus-refactoring condition under phase 3. This documentation regrouping was agreed on 2026-11-12; the June 11 decision used five phase numbers. It does not change the experiment, merge result sets, or imply that the feedback-only sweep used the tool.
Existing artifact IDs remain unchanged: `phase3-agentic-loop` = **3a**,
`phase4-refactoring` = **3b**, and `phase5-multiagent` = **4**.
Historical documents and workflow names use those legacy numbers. See the
[root mapping](../README.md#phase-numbering-and-historical-artifacts).

The completed 3a sweep did **not** have `apply_refactor`: its
[lock file](phase3-agentic-loop/phase.lock.yaml) permits only `read_file`,
`list_dir`, and `submit_test`. The refactoring arm has its own
[lock file](phase4-refactoring/phase.lock.yaml) and results namespace; grouping
the arms does not pool their results or establish a measured refactoring gain.

The 300-cell v2 sample is fixed for generation comparisons. Its **12 contributing
repositories** are the nonempty eligible subset of the **15-repository baseline
matrix**; see the [screening and contribution table](../README.md#screened-repositories-and-sampled-target-contributors).

> **The refactoring arm (3b) and multi-agent phase (4) are devcontainer-only.**
> Local/dev steps run inside the [devcontainer](../.devcontainer/devcontainer.json);
> production sweeps run via containerized GitHub Actions. See
> [refactoring replication](phase4-refactoring/REPLICATION.md).

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
  phase4-refactoring.yml            # phase 3b — legacy refactoring ID
  phase5-{generate,evaluate,aggregate}.yml   # phase 4 — legacy multi-agent ID, mock default
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

After the run completes, fill in `phase.lock.yaml` (especially `model_snapshots_observed` and `ci_runs.generator_run_ids`), write `REPORT.md` + `COSTS.md` + `REPLICATION.md`, append a row to [`../docs/RESULTS.md`](../docs/RESULTS.md), and tag `phase-N-name-final`.

## Reproducibility envelope

What's reproducible:
- ✅ The harness (workflow + adapter + prompt files + target set).
- ✅ The aggregate distribution of outcomes (per-model mean ± stddev) within ~2σ.
- ✅ Every input that fed the experiment (every SHA, every prompt byte, every model id).

What's not:
- ❌ The exact tokens emitted by closed-weight models. Even at temperature 0 there is gateway-level non-determinism (load balancing, KV-cache batching, GPU non-associativity).
- ❌ The exact model snapshot if the canonical run pre-dates a snapshot retirement. Mitigation: `model_snapshots_observed` records what was actually used at run time.
