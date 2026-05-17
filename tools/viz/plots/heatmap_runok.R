# plots/heatmap_runok.R — Phase 3 heatmap, run-OK% by (repo, model).
# Output: assets/figures/phase3-heatmap-runok.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(viridis)
  library(scales)
})

df <- load_per_model_repo() |>
  mutate(model = factor(model), repo = factor(repo))

model_order <- df |>
  group_by(model) |>
  summarise(run = sum(run_ok), sub = sum(submitted), .groups = "drop") |>
  mutate(pct = run / sub) |>
  arrange(desc(pct)) |>
  pull(model)

repo_order <- df |>
  group_by(repo) |>
  summarise(run = sum(run_ok), sub = sum(submitted), .groups = "drop") |>
  mutate(pct = run / sub) |>
  arrange(pct) |>
  pull(repo)

df <- df |>
  mutate(
    model = factor(model, levels = model_order),
    repo  = factor(repo,  levels = repo_order)
  )

p <- ggplot(df, aes(x = model, y = repo, fill = run_pct)) +
  geom_tile(colour = "white", linewidth = 0.4) +
  geom_text(aes(label = sprintf("%.0f%%", run_pct)),
            colour = "white", size = 3) +
  scale_fill_viridis(option = "C", limits = c(0, 60),
                     oob = scales::squish, name = "run-OK %") +
  labs(
    title    = "Phase 3 calibration \u2014 run-OK rate by repo x model",
    subtitle = "1,800 cells, 1 run; canonical evaluator. Brighter = more tests actually passed.",
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
