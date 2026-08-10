# tools/viz/

R + `ggplot2` figures for the project. Renders into `assets/figures/`.

## Devcontainer

R work happens in the dedicated **R viz (ggplot2)** devcontainer
([.devcontainer/r-viz/devcontainer.json](../../.devcontainer/r-viz/devcontainer.json)),
not the main .NET devcontainer. The main container has no R toolchain.

To render figures:

1. In VS Code: **Dev Containers: Reopen in Container** → choose **R viz (ggplot2)**.
2. First build pulls `rocker/tidyverse:4.4` and installs `viridis`, `scales`,
   `patchwork`, `ggtext`, `ggrepel`, `languageserver` (takes a few minutes once).
3. R 4.4 + tidyverse are preinstalled in the base image.

The workspace-level [.vscode/settings.json](../../.vscode/settings.json) pins
the R extension to the in-container R (`/usr/local/bin/R`) and enables the
session watcher so the Workspace panel auto-attaches.

## Layout

```
tools/viz/
  render_all.R                   top-level runner: sources every plots/*.R
  render_phase3.R                back-compat shim → render_all.R
  aggregate_phase_results.py     produces data/per_model_phase.csv from jsonl
  ../analysis/phase4_failure_categorization.py
                                produces phase-4 non-submitted failure taxonomy
  lib/
    load.R                       repo_root(), figures_dir(), load_* helpers
    theme.R                      theme_paper() + colour palettes
  plots/
    all_phases_failure_categories.R
                  all-phases stacked share mix by model (faceted)
    all_phases_infrastructure_failures.R
                  absolute infrastructure failure matrix by phase/category
    heatmap_runok.R              Fig 1 — phase 3 heatmap
    compile_vs_run.R             Fig 2 — phase 3 per-model bars
    repo_lollipop.R              Fig 3 — phase 3 per-repo lollipop
    successful_tests_progression.R   slopegraph: run_ok across phases
    phase4_compile_vs_run.R       phase-4 per-model compile/run bars
    phase4_failure_buckets.R     phase-4 non-submitted failure categories (aggregated across runs)
    coverage_baseline.R          per-repo baseline coverage dumbbell
    cost_efficiency.R            scatter: cost vs run_ok, Pareto frontier
    cost_per_passing_test.R      $/passing-test by model, faceted by phase
  data/
    per_model_repo.csv           per-(model, repo) phase 3 totals (committed)
    per_model_phase.csv          per-(phase, model) totals (derived, regenerable)
    phase4-refactoring_failure_categories_by_model_run.csv
                                phase-4 taxonomy by (phase, model, run)
    phase4-refactoring_failure_categories_totals.csv
                                phase-4 taxonomy totals by category
    phase4-refactoring_failure_rerun_signal_by_model_run.csv
                                phase-4 rerun-needed signal by model/run
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

### Non-negotiable reporting policy

Reports, CSVs, and figures must include every recorded attempt and every failure
category. Infrastructure failures (authentication, rate limits, timeouts,
server errors, unsupported API versions, and similar execution failures) are
first-class results because they identify work that still needs correction.
Never filter them from charts, drop them from aggregate totals, or remove them
from metric denominators. If a model-quality-only view is useful, add it as a
separate view while preserving an all-results view as the default.

After new phase results land, regenerate phase-level and failure-taxonomy data:

```bash
python3 tools/viz/aggregate_phase_results.py
python3 tools/analysis/phase4_failure_categorization.py
python3 tools/analysis/phase4_failure_categorization.py --phases phase2-agentic,phase3-agentic-loop,phase4-refactoring
python3 tools/viz/validate_reporting.py
Rscript tools/viz/render_all.R
```

The validator fails if phase/model attempt counts differ from the raw JSONL,
failure-category counts do not conserve all non-submitted attempts, an
infrastructure category is missing, or infrastructure totals differ between
the taxonomy and aggregate CSV.

The aggregator walks each phase's
`results*/<model>/run_*/attempts.jsonl` files (and parallel
`evaluation.jsonl` files), reuses the `PRICES` table from
`tools/cost/estimate.py`, and derives both summary CSVs from those raw records.

## Outputs in `assets/figures/`

- `phase3-heatmap-runok.png`
- `phase3-by-model-compile-vs-run.png`
- `phase3-by-repo-runok.png`
- `progression-runok.png`
- `phase2-vs-phase3-vs-phase4.png`
- `phase4-by-model-compile-vs-run.png`
- `phase4-failure-buckets.png`
- `phase4-all-failure-buckets.png`
- `coverage-baseline.png`
- `cost-efficiency.png`
- `cost-per-passing-test.png`
- `all-phases-failure-category-shares-by-model-faceted.png`
- `all-phases-infrastructure-failures.png`

### `all-phases-failure-category-shares-by-model-faceted.png`

Compares non-submitted failure-category shares across `phase2-agentic`,
`phase3-agentic-loop`, and `phase4-refactoring` with one panel per model so you
can quickly see composition shifts per model. Infra categories (`timeout/connection`,
`auth/access`, `rate-limit`, `server-5xx`, `api-version-unsupported`) are visually separated from baseline
categories (`baseline_compile_failed`, `baseline_no_owning_csproj`) via distinct
color families.

### `all-phases-infrastructure-failures.png`

Shows absolute infrastructure-failure counts for every phase and category,
including explicit zeroes. Use this as the at-a-glance remediation view; the
faceted share chart remains useful for model-level failure composition.

`phase4-all-failure-buckets.png` includes every non-submitted category,
including infrastructure failures.

### Failure Taxonomy Note

`tools/analysis/phase4_failure_categorization.py` emits a dedicated
`api-version-unsupported` bucket for adapter/API version mismatch failures (for
example HTTP 400 `API version not supported`). This bucket is treated as infra
for rerun diagnostics and appears in both phase-4 and all-phases failure charts.

## Inputs read in place (not duplicated)

- `baseline_coverage.csv` (repo root) — phase 1 per-repo coverage.

Only **derived** artifacts (aggregations that don't exist anywhere else) live
under `tools/viz/data/`. Existing CSVs are read directly from their canonical
locations.

## Why R

`ggplot2` polish for inclusion in a paper. Other charting in this repo can
stay in Python; this directory is the R island.
