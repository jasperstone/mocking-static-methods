# plots/repo_lollipop.R — Per-repo difficulty: compile-OK and run-OK pooled across models.
# Output: assets/figures/phase3-by-repo-runok.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_repo() |>
  mutate(model = factor(model), repo = factor(repo))

repo_order <- df |>
  group_by(repo) |>
  summarise(run = sum(run_ok), sub = sum(submitted), .groups = "drop") |>
  mutate(pct = run / sub) |>
  arrange(pct) |>
  pull(repo)

repo_summary <- df |>
  group_by(repo) |>
  summarise(
    submitted = sum(submitted),
    compile   = sum(compile_ok),
    run       = sum(run_ok),
    .groups   = "drop"
  ) |>
  mutate(
    compile_pct = 100 * compile / submitted,
    run_pct     = 100 * run / submitted,
    repo        = factor(repo, levels = repo_order)
  )

p <- ggplot(repo_summary, aes(y = repo)) +
  geom_segment(aes(x = 0, xend = compile_pct, yend = repo),
               colour = "#7fb3d5", linewidth = 1.8) +
  geom_point(aes(x = compile_pct), colour = "#7fb3d5", size = 4) +
  geom_point(aes(x = run_pct), colour = "#1f4e79", size = 4) +
  geom_text(aes(x = compile_pct, label = sprintf("%.0f%%", compile_pct)),
            hjust = -0.4, size = 3, colour = "#3a6a8a") +
  geom_text(aes(x = run_pct, label = sprintf("%.0f%%", run_pct)),
            hjust = 1.4, size = 3, colour = "#1f4e79") +
  scale_x_continuous(labels = label_percent(scale = 1),
                     limits = c(0, max(repo_summary$compile_pct) * 1.15)) +
  labs(
    title    = "Per-repo difficulty \u2014 compile-OK and run-OK (all models pooled)",
    subtitle = "Light blue = compile-OK%, dark blue = run-OK%. Distance between = compile-but-fail-to-run.",
    x = NULL, y = NULL
  ) +
  theme_paper() +
  theme(panel.grid.major.y = element_blank())

out <- file.path(figures_dir(), "phase3-by-repo-runok.png")
ggsave(out, p, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote", out, "\n")
