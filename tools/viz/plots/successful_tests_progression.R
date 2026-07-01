# plots/successful_tests_progression.R — slopegraph of run-OK count per model
# across phases. X-axis is whichever phases are present in per_model_phase.csv.
# Output: assets/figures/progression-runok.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

df <- load_per_model_phase()

# Drop phases with zero data (e.g. phase2-singleshot until it has results).
df <- df |>
  group_by(phase) |>
  filter(sum(attempts, na.rm = TRUE) > 0) |>
  ungroup()

# Canonical phase ordering when present; unknown phases sort after these.
phase_levels <- intersect(
  c("phase2-singleshot", "phase2-agentic", "phase3-agentic-loop"),
  unique(df$phase)
)
phase_levels <- c(phase_levels, setdiff(unique(df$phase), phase_levels))
df <- df |>
  mutate(
    phase = factor(phase, levels = phase_levels),
    # Keep zero-submission models visible at 0% so phase endpoints show all models.
    run_ok_pct = ifelse(submitted > 0, 100 * run_ok / submitted, 0)
  )

# Highlight the biggest mover by run-OK percentage.
movers <- df |>
  group_by(model) |>
  filter(n() >= 2) |>
  summarise(
    delta = run_ok_pct[phase == tail(phase_levels, 1)][1] -
            run_ok_pct[phase == head(phase_levels, 1)][1],
    .groups = "drop"
  ) |>
  arrange(desc(abs(delta)))

top_mover <- if (nrow(movers) > 0) movers$model[[1]] else NA_character_

df <- df |>
  mutate(highlight = ifelse(model == top_mover, "top mover", "other"))

last_phase <- tail(phase_levels, 1)
first_phase <- head(phase_levels, 1)

  # Label each model at its rightmost present phase (so models that drop out
  # earlier still get labelled).
  endpoint <- df |>
    group_by(model) |>
    slice_max(as.integer(phase), n = 1, with_ties = FALSE) |>
    ungroup()

p <- ggplot(df, aes(x = phase, y = run_ok_pct, group = model, colour = highlight)) +
  geom_line(linewidth = 1.1, alpha = 0.85) +
  geom_point(size = 2.5) +
  geom_text(
    data = endpoint,
    aes(label = model),
    hjust = 0, nudge_x = 0.05, vjust = -0.6, size = 3.2
  ) +
  geom_text(
    data = df |> filter(phase == first_phase),
    aes(label = sprintf("%.1f%%", run_ok_pct)),
    hjust = 1, nudge_x = -0.05, size = 3, colour = "grey30"
  ) +
  scale_colour_manual(values = c("top mover" = "#c44e52", "other" = "grey55"),
                      guide = "none") +
  scale_y_continuous(labels = label_percent(scale = 1)) +
  scale_x_discrete(expand = expansion(mult = c(0.10, 0.30))) +
  labs(
    title    = "Successful test progression across phases",
    subtitle = sprintf(
      "Run-OK percentage (run_ok/submitted) per model. Highlighted: %s (biggest %s).",
      if (is.na(top_mover)) "(none)" else top_mover,
      "movement between first and last phase"
    ),
    x = NULL, y = "run_ok / submitted"
  ) +
  theme_paper() +
  theme(panel.grid.major.x = element_blank())

out <- file.path(figures_dir(), "progression-runok.png")
ggsave(out, p, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote", out, "\n")
