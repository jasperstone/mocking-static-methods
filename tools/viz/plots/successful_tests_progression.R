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
  c("phase2-singleshot", "phase2-agentic", "phase3-agentic-loop", "phase4-refactoring"),
  unique(df$phase)
)
phase_levels <- c(phase_levels, setdiff(unique(df$phase), phase_levels))
df <- df |>
  mutate(
    phase = factor(phase, levels = phase_levels),
    # Tooling-only failures can yield submitted=0; treat as non-evaluable.
    run_ok_pct = ifelse(submitted > 0, 100 * run_ok / submitted, NA_real_)
  )

# Highlight the biggest mover by run-OK percentage.
movers <- df |>
  group_by(model) |>
  filter(sum(!is.na(run_ok_pct)) >= 2) |>
  summarise(
    delta = {
      vals <- run_ok_pct[!is.na(run_ok_pct)]
      vals[length(vals)] - vals[1]
    },
    .groups = "drop"
  ) |>
  arrange(desc(abs(delta)))

top_mover <- if (nrow(movers) > 0) movers$model[[1]] else NA_character_

model_levels <- unique(df$model)
model_palette <- setNames(scales::hue_pal()(length(model_levels)), model_levels)
wrapped_subtitle <- paste(
  strwrap(
    sprintf(
      "Run-OK percentage (run_ok/submitted) per model; non-evaluable phases (submitted=0) are omitted. Highlighted: %s (biggest %s).",
      if (is.na(top_mover)) "(none)" else top_mover,
      "movement between first and last evaluable phase"
    ),
    width = 96
  ),
  collapse = "\n"
)

last_phase <- tail(phase_levels, 1)
first_phase <- head(phase_levels, 1)

  # Label each model at its rightmost present phase (so models that drop out
  # earlier still get labelled).
  endpoint <- df |>
    group_by(model) |>
    slice_max(as.integer(phase), n = 1, with_ties = FALSE) |>
    ungroup()

endpoint <- endpoint |>
  mutate(
    label_group = ifelse(row_number() %% 2 == 0, "high", "low"),
    label_y = run_ok_pct + ifelse(label_group == "high", 3.0, -3.0)
  )

start_labels <- df |>
  filter(phase == first_phase) |>
  mutate(
    label_group = ifelse(row_number() %% 2 == 0, "high", "low"),
    label_y = run_ok_pct + ifelse(label_group == "high", 2.0, -2.0)
  )

p <- ggplot(df, aes(x = phase, y = run_ok_pct, group = model, colour = model)) +
  geom_line(linewidth = 1.1, alpha = 0.85) +
  geom_point(size = 2.5) +
  geom_text(
    data = endpoint,
    aes(y = label_y, label = model, colour = model),
    hjust = 0, nudge_x = 0.10, size = 3.0, fontface = "bold",
    check_overlap = TRUE
  ) +
  geom_text(
    data = start_labels,
    aes(y = label_y, label = sprintf("%.1f%%", run_ok_pct), colour = model),
    hjust = 1, nudge_x = -0.10, size = 2.9,
    check_overlap = TRUE
  ) +
  scale_colour_manual(values = model_palette,
                      guide = "none") +
  scale_y_continuous(labels = label_percent(scale = 1)) +
  scale_x_discrete(expand = expansion(mult = c(0.18, 0.42))) +
  labs(
    title    = "Successful test progression across phases",
    subtitle = wrapped_subtitle,
    x = NULL, y = "run_ok / submitted"
  ) +
  theme_paper() +
  theme(
    panel.grid.major.x = element_blank(),
    plot.margin = margin(14, 28, 12, 28),
    plot.subtitle = element_text(lineheight = 1.05)
  )

out <- file.path(figures_dir(), "progression-runok.png")
ggsave(out, p, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote", out, "\n")
