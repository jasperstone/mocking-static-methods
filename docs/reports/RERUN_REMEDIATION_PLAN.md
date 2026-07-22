# Rerun Remediation Plan

Date: 2026-07-21  
Owner: Lewis (Lead)  
Source diagnostics: `docs/reports/ALL_PHASES_FAILURE_DIAGNOSTICS.md`, `tools/viz/data/all_phases_failure_rerun_signal_by_model_run.csv`

## 1. Objective And Policy Constraints

Objective: clear infrastructure-driven non-submissions (rate-limit, timeout/connection, server-5xx, auth/access) from headline comparisons by running targeted reruns on flagged phase/model/run cells.

Policy constraints:
- Infra failures are rerun-required and are not treated as model-quality failures.
- Baseline compile failures (`baseline_compile_failed`, `baseline_no_owning_csproj`) are not infra rerun candidates unless baseline state changes.
- Proceed in waves from highest publish-blocking impact to backlog cleanup.
- Use current rerun-signal thresholds from diagnostics:
  - Red: `infra_non_submitted >= 10` OR `infra_share_of_attempts >= 3%` OR `infra_share_of_non_submitted >= 20%`
  - Yellow: `infra_non_submitted >= 3` OR `infra_share_of_attempts >= 1%` OR `infra_share_of_non_submitted >= 10%`

## 2. Prioritized Waves

## Wave 1 (Blockers): Phase 4 Refactoring

Priority target:
- `phase4-refactoring`: `phi-4` run 1/2/3 (all red)

Secondary hygiene target in same wave:
- `phase4-refactoring`: low-count llama rate-limit entries (run 1 and run 2) for closure on known residual infra incidents

Execution intent:
- Dispatch exact-cell reruns for `phi-4` run 1/2/3 first.
- Recompute diagnostics; if any `phi-4` run remains red, rerun only remaining red run(s) once more.
- Then dispatch llama targeted backfill only for affected run windows/cells.

## Wave 2 (High Infra): Phase 3 Agentic Loop

Priority targets:
- Red: `gpt-4.1-mini`, `gpt-4.1-nano`
- Red low-count: `grok-4-1-fast`, `llama-3.3-70b-instruct`
- Yellow cleanup: `phi-4`

Execution intent:
- First pass: run red models only.
- Second pass: if any run remains red, rerun only unresolved model/run combinations.
- Final cleanup: run yellow `phi-4` only if still yellow or red after red-pass consolidation.

## Wave 3 (Backlog): Phase 2 Agentic

Targets (all red):
- `gpt-4.1-mini`, `gpt-4.1-nano`, `gpt-5-codex`, `grok-4-1-fast`, `llama-3.3-70b-instruct`

Execution intent:
- Run model-by-model dispatches (not `models=all`) to isolate retries and avoid broad queue contention.
- Prioritize smaller/high-value slices first (`gpt-4.1-mini`, `gpt-4.1-nano`), then the heavier throttled models.

## 3. Dispatch Command Templates (Exact Workflows)

Prereq:
- `gh auth status`
- Repo root: `/home/jastone/src/mocking-static-methods`

### Phase 4 workflow (`.github/workflows/phase4-refactoring.yml`)

Template:

```bash
gh workflow run "Phase 4 — generate (agentic loop + refactoring tool)" \
  -f mode=foundry \
  -f confirm_spend=yes-after-2026-06-08-freeze \
  -f target_set=v2 \
  -f run_window=RUN_START:RUN_COUNT \
  -f models=MODEL_ID \
  -f repos=all \
  -f shard_spec="target_ids=TARGET_ID_1,TARGET_ID_2;chunk_size=3" \
  -f limit_per_repo=none \
  -f max_compile_attempts=4 \
  -f max_parallel=1 \
  -f run_timeout_s=60
```

Wave 1 baseline examples:

```bash
# phi-4 runs 1..3
gh workflow run "Phase 4 — generate (agentic loop + refactoring tool)" \
  -f mode=foundry -f confirm_spend=yes-after-2026-06-08-freeze \
  -f target_set=v2 -f run_window=1:3 -f models=phi-4 -f repos=all \
  -f shard_spec=chunk_size=3 -f limit_per_repo=none \
  -f max_compile_attempts=4 -f max_parallel=1 -f run_timeout_s=60

# llama follow-up window (example: runs 1..2)
gh workflow run "Phase 4 — generate (agentic loop + refactoring tool)" \
  -f mode=foundry -f confirm_spend=yes-after-2026-06-08-freeze \
  -f target_set=v2 -f run_window=1:2 -f models=llama-3.3-70b-instruct -f repos=all \
  -f shard_spec=chunk_size=3 -f limit_per_repo=none \
  -f max_compile_attempts=4 -f max_parallel=1 -f run_timeout_s=60
```

### Phase 3 workflow (`.github/workflows/phase3-generate.yml`)

Template:

```bash
gh workflow run "Phase 3 — generate" \
  -f target_set=v2 \
  -f runs_per_cell=3 \
  -f run_index_start=1 \
  -f models=MODEL_ID \
  -f repos=all \
  -f limit_per_repo= \
  -f target_ids= \
  -f max_compile_attempts=4 \
  -f run_timeout_s=60
```

Wave 2 examples:

```bash
for m in gpt-4.1-mini gpt-4.1-nano grok-4-1-fast llama-3.3-70b-instruct; do
  gh workflow run "Phase 3 — generate" \
    -f target_set=v2 -f runs_per_cell=3 -f run_index_start=1 \
    -f models="$m" -f repos=all -f limit_per_repo= -f target_ids= \
    -f max_compile_attempts=4 -f run_timeout_s=60
done

# yellow cleanup
for m in phi-4; do
  gh workflow run "Phase 3 — generate" \
    -f target_set=v2 -f runs_per_cell=3 -f run_index_start=1 \
    -f models="$m" -f repos=all -f limit_per_repo= -f target_ids= \
    -f max_compile_attempts=4 -f run_timeout_s=60
done
```

## Active Wave Execution (2026-07-21)

Current launched remediation runs:
- `phase4-refactoring` run `29846768297` (`in_progress`) - phi-4 rerun (Wave 1 blocker clearance)
- `phase4-refactoring` run `29846770599` (`queued`) - llama cleanup (Wave 1 residual infra hygiene)
- `phase3-generate` run `29846772787` (`queued`) - phase3 gpt-4.1-mini rerun (Wave 2 red model)
- `phase3-generate` run `29846774981` (`queued`) - phase3 gpt-4.1-nano rerun (Wave 2 red model)

Immediate monitoring checklist:

```bash
# quick status snapshot
gh run view 29846768297 --json databaseId,status,conclusion,createdAt,updatedAt,url
gh run view 29846770599 --json databaseId,status,conclusion,createdAt,updatedAt,url
gh run view 29846772787 --json databaseId,status,conclusion,createdAt,updatedAt,url
gh run view 29846774981 --json databaseId,status,conclusion,createdAt,updatedAt,url

# stream progression until terminal state
gh run watch 29846768297
gh run watch 29846770599
gh run watch 29846772787
gh run watch 29846774981

# if needed, inspect logs per run attempt
gh run view 29846768297 --log-failed
gh run view 29846770599 --log-failed
gh run view 29846772787 --log-failed
gh run view 29846774981 --log-failed
```

Post-completion regeneration commands (diagnostics + visualizations):

```bash
python3 tools/analysis/phase4_failure_categorization.py \
  --phases phase2-agentic,phase3-agentic-loop,phase4-refactoring

# regenerate aggregated phase tables consumed by report visuals
python3 aggregate_baseline.py

# refresh all report plots from current data snapshots
Rscript tools/viz/render_all.R
```

### Phase 2 workflow (`.github/workflows/phase2-generate.yml`)

Template:

```bash
gh workflow run "Phase 2 — generate" \
  -f target_set=v2 \
  -f runs_per_cell=3 \
  -f models=MODEL_ID \
  -f repos=all \
  -f limit_per_repo= \
  -f target_ids=
```

Wave 3 example:

```bash
for m in gpt-4.1-mini gpt-4.1-nano gpt-5-codex grok-4-1-fast llama-3.3-70b-instruct; do
  gh workflow run "Phase 2 — generate" \
    -f target_set=v2 -f runs_per_cell=3 -f models="$m" -f repos=all \
    -f limit_per_repo= -f target_ids=
done
```

## 4. Acceptance Criteria And Stop/Go Checks

After each wave:

1. Regenerate diagnostics:

```bash
python3 tools/analysis/phase4_failure_categorization.py \
  --phases phase2-agentic,phase3-agentic-loop,phase4-refactoring
```

2. Read gate files:
- `docs/reports/ALL_PHASES_FAILURE_DIAGNOSTICS.md`
- `tools/viz/data/all_phases_failure_rerun_signal_by_model_run.csv`
- `tools/viz/data/all_phases_failure_categories_summary.json`

Stop/go criteria by wave:
- Wave 1 go to Wave 2 only when `phase4-refactoring` `phi-4` runs are no longer red.
- Wave 2 go to Wave 3 only when phase-3 red rows are cleared or reduced to explicitly accepted residual low-count rows.
- Wave 3 complete when phase-2 targeted model/run rows are no longer red, or remaining red rows are documented as non-rerunnable due to policy constraints.

Global publish gate:
- No unresolved infra-red rows for the publication slice.
- Any remaining yellow rows must be documented in release notes with counts and rationale.

## 5. Post-Rerun Regeneration Checklist

1. Run phase-level evaluation/aggregation workflows for rerun phases:
- Phase 2: `.github/workflows/phase2-evaluate.yml`, `.github/workflows/phase2-aggregate.yml`
- Phase 3: `.github/workflows/phase3-evaluate.yml`, `.github/workflows/phase3-aggregate.yml`
- Phase 4: phase4 downstream evaluation/aggregation path used by current reporting pipeline

2. Regenerate diagnostics and visuals:
- `python3 tools/analysis/phase4_failure_categorization.py --phases phase2-agentic,phase3-agentic-loop,phase4-refactoring`
- Rebuild plots that consume all-phases failure data (under `tools/viz/plots/`)

3. Refresh report artifacts that depend on phase metrics/costs:
- `RESULTS.md`
- `phases/phase4-refactoring/HEADLINE.md`
- `phases/phase4-refactoring/COSTS_AUTOGEN.md`
- `tools/viz/data/per_model_phase.csv` and any derived tables used in docs

4. Final sanity checks before publish:
- Model/run row counts align across markdown and CSV outputs.
- Cost and run-OK summaries reflect latest successful complete reruns.
- Residual infra/yellow rows (if any) are explicitly called out.
