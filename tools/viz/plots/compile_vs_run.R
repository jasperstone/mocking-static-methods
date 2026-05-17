# plots/compile_vs_run.R — Per-model faceted bars: compile-OK vs run-OK per repo.
# Output: assets/figures/phase3-by-model-compile-vs-run.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(tidyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_repo() |>
  mutate(model = factor(model), repo = factor(repo))

long <- df |>
  select(model, repo, compile_pct, run_pct) |>
  pivot_longer(c(compile_pct, run_pct), names_to = "metric", values_to = "pct") |>
  mutate(metric = recode(metric,
                         compile_pct = "compile-OK",
                         run_pct     = "run-OK"))

p <- ggplot(long, aes(x = repo, y = pct, fill = metric)) +
  geom_col(position = position_dodge(width = 0.75), width = 0.7) +
  facet_wrap(~ model, ncol = 2) +
  scale_fill_manual(values = pal_compile_run) +
  scale_y_continuous(labels = label_percent(scale = 1), limits = c(0, 100)) +
  labs(
    title    = "Compile vs run-OK by repo, per model",
    subtitle = "Gap between bars = tests that compile but fail at runtime.",
    x = NULL, y = NULL, fill = NULL
  ) +
  theme_paper() +
  theme(
    axis.text.x     = element_text(angle = 45, hjust = 1),
    legend.position = "top"
  )

out <- file.path(figures_dir(), "phase3-by-model-compile-vs-run.png")
ggsave(out, p, width = 11, height = 8, dpi = 150, bg = "white")
cat("wrote", out, "\n")
