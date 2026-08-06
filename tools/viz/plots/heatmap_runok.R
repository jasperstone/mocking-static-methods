# plots/heatmap_runok.R — Phase 3 heatmap, run-OK% by (repo, model).
# Output: assets/figures/phase3-heatmap-runok.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_repo() |>
  mutate(
    model = factor(model),
    repo = factor(repo),
    label = sprintf("%.0f%%\n(n=%d)", run_pct, attempts)
  )

model_order <- df |>
  group_by(model) |>
  summarise(run = sum(run_ok), attempts = sum(attempts), .groups = "drop") |>
  mutate(pct = run / attempts) |>
  arrange(desc(pct)) |>
  pull(model) |>
  as.character()

repo_order <- df |>
  group_by(repo) |>
  summarise(run = sum(run_ok), attempts = sum(attempts), .groups = "drop") |>
  mutate(pct = run / attempts) |>
  arrange(pct) |>
  pull(repo) |>
  as.character()

df <- df |>
  mutate(
    model = factor(model, levels = model_order),
    repo  = factor(repo,  levels = repo_order)
  )

p <- ggplot(df, aes(x = model, y = repo, fill = run_pct)) +
  geom_tile(colour = "white", linewidth = 0.4) +
  geom_text(aes(label = label), colour = "white", size = 2.6, lineheight = 0.9) +
  scale_fill_viridis_c(
    option = "C", limits = c(0, 60), oob = scales::squish,
    name = "run-OK %"
  ) +
  labs(
    title    = "Phase 3 \u2014 run-OK rate by repo x model (3-run totals)",
    subtitle = "Rates use every attempt; cell labels disclose sample size without masking low-N outcomes.",
    x = NULL, y = NULL
  ) +
  theme_paper() +
  theme(
    axis.text.x = element_text(angle = 30, hjust = 1),
    panel.grid  = element_blank()
  )

out <- file.path(figures_dir(), "phase3-heatmap-runok.png")
ggsave(out, p, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote", out, "\n")
