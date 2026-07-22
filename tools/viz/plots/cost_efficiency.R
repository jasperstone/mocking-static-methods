# plots/cost_efficiency.R — scatter of cost vs run-OK count per (model, phase).
# Pareto frontier overlay shows the cheap-and-good models.
# Output: assets/figures/cost-efficiency.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_phase() |>
  filter(!is.na(cost_usd), cost_usd > 0)

if (nrow(df) == 0) {
  message("cost_efficiency.R: no rows with cost data; nothing to plot.")
} else {
  # Pareto frontier: maximise run_ok, minimise cost. Sort by cost ascending and
  # keep points whose run_ok exceeds the max seen so far.
  frontier <- df |>
    arrange(cost_usd) |>
    mutate(cum_best = cummax(run_ok)) |>
    filter(run_ok == cum_best) |>
    arrange(cost_usd)

  p <- ggplot(df, aes(x = cost_usd, y = run_ok)) +
    geom_line(data = frontier, aes(x = cost_usd, y = run_ok),
              inherit.aes = FALSE,
              linetype = "dashed", colour = "grey55", linewidth = 0.6) +
    geom_point(aes(colour = model, shape = phase), size = 4, alpha = 0.9) +
    geom_text(
      aes(label = model),
      size = 3, colour = "grey25",
      hjust = -0.15, vjust = -0.4, check_overlap = TRUE
    ) +
    scale_x_log10(labels = label_dollar(),
                  expand = expansion(mult = c(0.08, 0.18))) +
    scale_colour_manual(values = pal_models, name = "model") +
    scale_shape_manual(values = c(
      "phase2-singleshot"   = 15,
      "phase2-agentic"      = 16,
      "phase3-agentic-loop" = 17,
      "phase4-refactoring"  = 18,
      "phase5-multiagent"   = 8
    ), name = "phase") +
    labs(
      title    = "Cost efficiency \u2014 dollars spent vs tests that actually pass",
      subtitle = "One point per (model, phase). Dashed line = Pareto frontier (cheapest at each run-OK level). Log x-axis.",
      x = "cost (USD, log scale)", y = "tests passing (run_ok)"
    ) +
    theme_paper()

  out <- file.path(figures_dir(), "cost-efficiency.png")
  ggsave(out, p, width = 9, height = 6, dpi = 150, bg = "white")
  cat("wrote", out, "\n")
}
