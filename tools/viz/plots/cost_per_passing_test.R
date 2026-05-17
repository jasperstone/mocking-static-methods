# plots/cost_per_passing_test.R — $/passing-test per model, faceted by phase.
# Output: assets/figures/cost-per-passing-test.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_phase() |>
  filter(!is.na(cost_usd))

if (nrow(df) == 0) {
  message("cost_per_passing_test.R: no rows with cost data; nothing to plot.")
} else {
  df <- df |>
    mutate(
      cost_per_pass = ifelse(run_ok > 0, cost_usd / run_ok, NA_real_),
      label_text    = ifelse(
        run_ok == 0,
        "no passing tests",
        sprintf("$%.2f", cost_per_pass)
      )
    )

  # Plot: for rows with run_ok == 0 we still show a tiny bar (epsilon) so the
  # model is visible, with an annotated "no passing tests" tag.
  plot_max <- suppressWarnings(max(df$cost_per_pass, na.rm = TRUE))
  if (!is.finite(plot_max)) plot_max <- 1
  epsilon <- plot_max * 0.01

  df <- df |>
    mutate(
      bar_value = ifelse(run_ok == 0, epsilon, cost_per_pass),
      missing   = run_ok == 0
    ) |>
    arrange(phase, desc(bar_value)) |>
    mutate(model = factor(model, levels = unique(model)))

  p <- ggplot(df, aes(y = model, x = bar_value, fill = missing)) +
    geom_col(width = 0.65) +
    geom_text(aes(label = label_text),
              hjust = -0.05, size = 3, colour = "grey20") +
    facet_wrap(~ phase, scales = "free_x") +
    scale_fill_manual(values = c(`FALSE` = "#1f4e79", `TRUE` = "grey75"),
                      guide = "none") +
    scale_x_continuous(labels = label_dollar(),
                       expand = expansion(mult = c(0, 0.25))) +
    labs(
      title    = "Cost per passing test, by model and phase",
      subtitle = "Lower is better. Bars with no fill = run_ok = 0 (cost/pass undefined).",
      x = "USD per run_ok test", y = NULL
    ) +
    theme_paper() +
    theme(panel.grid.major.y = element_blank())

  out <- file.path(figures_dir(), "cost-per-passing-test.png")
  ggsave(out, p, width = 10, height = 5.5, dpi = 150, bg = "white")
  cat("wrote", out, "\n")
}
