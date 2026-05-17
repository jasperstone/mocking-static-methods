# tools/viz/

R + `ggplot2` figures for the project. Renders into `assets/figures/`.

## Devcontainer

R and all required packages (`readr`, `dplyr`, `tidyr`, `ggplot2`, `viridis`,
`scales`, `patchwork`) are installed in the **main** devcontainer. The old
separate "R viz (ggplot2)" devcontainer is no longer required.

## Layout

```
tools/viz/
  render_all.R                   top-level runner: sources every plots/*.R
  render_phase3.R                back-compat shim → render_all.R
  aggregate_phase_results.py     produces data/per_model_phase.csv from jsonl
  lib/
    load.R                       repo_root(), figures_dir(), load_* helpers
    theme.R                      theme_paper() + colour palettes
  plots/
    heatmap_runok.R              Fig 1 — phase 3 heatmap
    compile_vs_run.R             Fig 2 — phase 3 per-model bars
    repo_lollipop.R              Fig 3 — phase 3 per-repo lollipop
    successful_tests_progression.R   slopegraph: run_ok across phases
    coverage_baseline.R          per-repo baseline coverage dumbbell
    cost_efficiency.R            scatter: cost vs run_ok, Pareto frontier
    cost_per_passing_test.R      $/passing-test by model, faceted by phase
  data/
    per_model_repo.csv           per-(model, repo) phase 3 totals (committed)
    per_model_phase.csv          per-(phase, model) totals (derived, regenerable)
```

## Running

Always run from the repo root:

```bash
# all plots
Rscript tools/viz/render_all.R

# one plot
Rscript -e 'source("tools/viz/plots/cost_efficiency.R")'
```

Each plot file is self-contained — it sources `lib/load.R` + `lib/theme.R`,
loads its own data, and `ggsave()`s to `assets/figures/`.

## Refreshing derived data

After new phase results land, regenerate `data/per_model_phase.csv`:

```bash
python3 tools/viz/aggregate_phase_results.py
Rscript tools/viz/render_all.R
```

The aggregator walks `phases/phase2-agentic/results*/<model>/run_*/attempts.jsonl`
(and the parallel `evaluation.jsonl`), reuses the `PRICES` table from
`tools/cost/estimate.py`, and synthesises phase 3 totals from
`data/per_model_repo.csv` (phase 3 raw attempts are not committed; cost is
emitted as blank for phase 3).

## Outputs in `assets/figures/`

- `phase3-heatmap-runok.png`
- `phase3-by-model-compile-vs-run.png`
- `phase3-by-repo-runok.png`
- `progression-runok.png`
- `coverage-baseline.png`
- `cost-efficiency.png`
- `cost-per-passing-test.png`

## Inputs read in place (not duplicated)

- `baseline_coverage.csv` (repo root) — phase 1 per-repo coverage.

Only **derived** artifacts (aggregations that don't exist anywhere else) live
under `tools/viz/data/`. Existing CSVs are read directly from their canonical
locations.

## Why R

`ggplot2` polish for inclusion in a paper. Other charting in this repo can
stay in Python; this directory is the R island.
