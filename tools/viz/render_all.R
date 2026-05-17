#!/usr/bin/env Rscript
# render_all.R — top-level runner. Sources every plot file in plots/ in
# alphabetical order and prints which PNGs were produced.
#
# Usage (from repo root):
#   Rscript tools/viz/render_all.R
#
# To refresh derived aggregations first:
#   python3 tools/viz/aggregate_phase_results.py
#   Rscript tools/viz/render_all.R

plots_dir <- file.path("tools", "viz", "plots")
files <- sort(list.files(plots_dir, pattern = "\\.R$", full.names = TRUE))

if (length(files) == 0) {
  stop("no plot files found under ", plots_dir)
}

cat("rendering", length(files), "plot file(s):\n")
for (f in files) cat("  -", f, "\n")
cat("\n")

for (f in files) {
  cat("=== ", basename(f), " ===\n", sep = "")
  # Each plot file is self-contained: it sources lib/load.R + lib/theme.R,
  # loads its own data, and ggsave()s into assets/figures/.
  source(f, local = new.env(), echo = FALSE)
}

cat("\nfigures written to: assets/figures/\n")
