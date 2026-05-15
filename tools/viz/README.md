# tools/viz/

R + `ggplot2` figures for the Phase 3 calibration sweep.

## Quick start (R viz devcontainer)

1. In VS Code: **Dev Containers: Reopen in Container** → pick **"R viz (ggplot2)"**.
   - First build pulls `rocker/tidyverse:4.4` and installs `viridis`, `scales`, `patchwork`, `ggtext`. Takes a few minutes once.
2. Open a terminal in the container and run:

   ```bash
   Rscript tools/viz/render_phase3.R
   ```

3. PNGs land in `assets/figures/`:
   - `phase3-heatmap-runok.png` — repos × models heatmap, fill = run-OK%
   - `phase3-by-model-compile-vs-run.png` — one panel per model, compile vs run-OK bars per repo
   - `phase3-by-repo-runok.png` — per-repo lollipop, compile% and run-OK% side by side

To iterate, run lines interactively from `render_phase3.R` with the VS Code R extension
(Ctrl-Enter sends to the R terminal).

## Inputs

- `data/per_model_repo.csv` — one row per (model, repo) shard with columns:
  `model, repo, submitted, compile_ok, run_ok, compile_pct, run_pct`.

To refresh after runs 2+3 land, re-aggregate eval outputs into the same schema
and overwrite the CSV.

## Why R and not Python here

The user wanted `ggplot2` polish for inclusion in a paper. Other charting in this
repo can stay in Python; this directory is the R island.
