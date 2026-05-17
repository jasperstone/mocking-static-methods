#!/usr/bin/env Rscript
# render_phase3.R — back-compat shim. The viz pipeline is now split into
# per-plot files under plots/. This shim sources render_all.R so old commands
# (e.g. `Rscript tools/viz/render_phase3.R`) keep working.
#
# Prefer running:
#   Rscript tools/viz/render_all.R                                  # all plots
#   Rscript -e 'source("tools/viz/plots/heatmap_runok.R")'          # one plot

source(file.path("tools", "viz", "render_all.R"))
