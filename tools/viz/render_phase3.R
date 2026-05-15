#!/usr/bin/env Rscript
# render_phase3.R — render Phase 3 calibration figures from per-model x per-repo CSV.
#
# Inputs:  tools/viz/data/per_model_repo.csv
# Outputs: assets/figures/phase3-heatmap-runok.png
#          assets/figures/phase3-by-model-compile-vs-run.png
#          assets/figures/phase3-by-repo-runok.png
#
# Run from the repo root inside the R viz devcontainer:
#   Rscript tools/viz/render_phase3.R

suppressPackageStartupMessages({
  library(readr)
  library(dplyr)
  library(tidyr)
  library(ggplot2)
  library(viridis)
  library(scales)
  library(patchwork)
})

# ---- locate paths relative to repo root --------------------------------------
args     <- commandArgs(trailingOnly = TRUE)
repo_root <- if (length(args) >= 1) args[[1]] else getwd()
in_csv   <- file.path(repo_root, "tools", "viz", "data", "per_model_repo.csv")
out_dir  <- file.path(repo_root, "assets", "figures")
dir.create(out_dir, recursive = TRUE, showWarnings = FALSE)

# ---- load --------------------------------------------------------------------
df <- read_csv(in_csv, show_col_types = FALSE) |>
  mutate(
    model = factor(model),
    repo  = factor(repo)
  )

cat(sprintf("loaded %d rows from %s\n", nrow(df), in_csv))

# Order models by overall run-OK% (highest at top of the heatmap).
model_order <- df |>
  group_by(model) |>
  summarise(run = sum(run_ok), sub = sum(submitted), .groups = "drop") |>
  mutate(pct = run / sub) |>
  arrange(desc(pct)) |>
  pull(model)

# Order repos by overall run-OK% (hardest at the bottom).
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

# ---- Fig 1: heatmap of run-OK% (repos x models) ------------------------------
p1 <- ggplot(df, aes(x = model, y = repo, fill = run_pct)) +
  geom_tile(colour = "white", linewidth = 0.4) +
  geom_text(aes(label = sprintf("%.0f%%", run_pct)),
            colour = "white", size = 3) +
  scale_fill_viridis(option = "C", limits = c(0, 60),
                     oob = scales::squish, name = "run-OK %") +
  labs(
    title    = "Phase 3 calibration — run-OK rate by repo x model",
    subtitle = "1,800 cells, 1 run; canonical evaluator. Brighter = more tests actually passed.",
    x = NULL, y = NULL
  ) +
  theme_minimal(base_size = 11) +
  theme(
    axis.text.x      = element_text(angle = 30, hjust = 1),
    panel.grid       = element_blank(),
    plot.title       = element_text(face = "bold"),
    legend.position  = "right"
  )

ggsave(file.path(out_dir, "phase3-heatmap-runok.png"),
       p1, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote phase3-heatmap-runok.png\n")

# ---- Fig 2: per-model faceted bars, compile% vs run-OK% per repo -------------
long <- df |>
  select(model, repo, compile_pct, run_pct) |>
  pivot_longer(c(compile_pct, run_pct), names_to = "metric", values_to = "pct") |>
  mutate(metric = recode(metric,
                         compile_pct = "compile-OK",
                         run_pct     = "run-OK"))

p2 <- ggplot(long, aes(x = repo, y = pct, fill = metric)) +
  geom_col(position = position_dodge(width = 0.75), width = 0.7) +
  facet_wrap(~ model, ncol = 2) +
  scale_fill_manual(values = c("compile-OK" = "#7fb3d5", "run-OK" = "#1f4e79")) +
  scale_y_continuous(labels = label_percent(scale = 1), limits = c(0, 100)) +
  labs(
    title    = "Compile vs run-OK by repo, per model",
    subtitle = "Gap between bars = tests that compile but fail at runtime.",
    x = NULL, y = NULL, fill = NULL
  ) +
  theme_minimal(base_size = 11) +
  theme(
    axis.text.x      = element_text(angle = 45, hjust = 1),
    strip.text       = element_text(face = "bold"),
    legend.position  = "top",
    plot.title       = element_text(face = "bold")
  )

ggsave(file.path(out_dir, "phase3-by-model-compile-vs-run.png"),
       p2, width = 11, height = 8, dpi = 150, bg = "white")
cat("wrote phase3-by-model-compile-vs-run.png\n")

# ---- Fig 3: per-repo run-OK% across models (horizontal lollipop) -------------
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

p3 <- ggplot(repo_summary, aes(y = repo)) +
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
    title    = "Per-repo difficulty — compile-OK and run-OK (all models pooled)",
    subtitle = "Light blue = compile-OK%, dark blue = run-OK%. Distance between = compile-but-fail-to-run.",
    x = NULL, y = NULL
  ) +
  theme_minimal(base_size = 11) +
  theme(
    panel.grid.major.y = element_blank(),
    plot.title         = element_text(face = "bold")
  )

ggsave(file.path(out_dir, "phase3-by-repo-runok.png"),
       p3, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote phase3-by-repo-runok.png\n")

cat("\ndone. figures in: ", out_dir, "\n", sep = "")
