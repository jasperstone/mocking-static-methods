# plots/phase4_compile_vs_run.R - Phase 4 compile/run outcomes by model.
# Output: assets/figures/phase4-by-model-compile-vs-run.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(tidyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_phase() |>
  filter(phase == "phase4-refactoring") |>
  mutate(
    compile_pct = 100 * compile_ok / attempts,
    run_pct = 100 * run_ok / attempts
  ) |>
  arrange(desc(run_pct)) |>
  mutate(model = factor(model, levels = model))

long <- df |>
  select(model, `Compile OK` = compile_pct, `Run OK` = run_pct) |>
  pivot_longer(
    cols = c(`Compile OK`, `Run OK`),
    names_to = "metric",
    values_to = "pct"
  ) |>
  mutate(metric = factor(metric, levels = c("Compile OK", "Run OK")))

p <- ggplot(long, aes(x = model, y = pct, fill = metric)) +
  geom_col(position = position_dodge(width = 0.76), width = 0.68) +
  geom_text(
    aes(label = sprintf("%.1f%%", pct)),
    position = position_dodge(width = 0.76),
    vjust = -0.4,
    size = 3.4,
    fontface = "bold"
  ) +
  scale_fill_manual(values = c("Compile OK" = "#2563eb", "Run OK" = "#f59e0b")) +
  scale_y_continuous(
    labels = label_percent(scale = 1),
    expand = expansion(mult = c(0, 0.14))
  ) +
  labs(
    title = "Phase 4 compile and run-OK rates by model",
    subtitle = "Reconciled production results: 5,238 unique attempts; infrastructure failures excluded from quality metrics.",
    x = NULL,
    y = "share of available attempts",
    fill = NULL,
    caption = "Source: tools/viz/data/per_model_phase.csv"
  ) +
  theme_paper() +
  theme(
    axis.text.x = element_text(angle = 22, hjust = 1),
    legend.position = "top",
    panel.grid.major.x = element_blank()
  )

out <- file.path(figures_dir(), "phase4-by-model-compile-vs-run.png")
ggsave(out, p, width = 11, height = 6, dpi = 170, bg = "white")
cat("wrote", out, "\n")
