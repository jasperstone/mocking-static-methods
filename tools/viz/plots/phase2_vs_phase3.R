# plots/phase2_vs_phase3.R — Grouped-bar comparison of compile-OK% and run-OK%
# across phase 2 (single shot), phase 3 (agentic loop with compile + run
# feedback), and phase 4 (agentic + refactor seam tooling).
#
# Output: assets/figures/phase2-vs-phase3.png
#
# This view is intended to show cross-phase progression at a glance.

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(tidyr)
  library(ggplot2)
  library(scales)
  library(stringr)
  library(forcats)
  library(grid)
})

# Six-model panel that exists in BOTH phases. Excludes gpt-5-codex (only in
# phase 2) so the comparison is apples-to-apples.
PANEL <- c("codestral-2501", "gpt-4.1-mini", "gpt-4.1-nano",
           "grok-4-1-fast", "llama-3.3-70b-instruct", "phi-4")

df <- load_per_model_phase() |>
  filter(model %in% PANEL,
         phase %in% c("phase2-agentic", "phase3-agentic-loop", "phase4-refactoring")) |>
  mutate(
    # Tooling-only failures (timeout/access/rate-limit) can produce submitted=0.
    # Treat those rows as non-evaluable rather than 0%-quality outcomes.
    compile_pct = ifelse(submitted > 0, 100 * compile_ok / submitted, NA_real_),
    run_pct     = ifelse(submitted > 0, 100 * run_ok     / submitted, NA_real_),
    phase_label = recode(phase,
      "phase2-agentic"      = "Phase 2: single shot",
      "phase3-agentic-loop" = "Phase 3: + compile/run feedback",
      "phase4-refactoring"  = "Phase 4: + refactor seam tooling")
  )

# Order models by phase-4 run-OK% (best-first) so latest outcomes are easiest
# to scan.
model_order <- df |>
  filter(phase == "phase4-refactoring") |>
  mutate(run_pct_sort = ifelse(is.na(run_pct), -Inf, run_pct)) |>
  arrange(desc(run_pct_sort)) |>
  pull(model)

df <- df |>
  mutate(
    model       = factor(model, levels = model_order),
    model_short = recode(as.character(model),
      "codestral-2501" = "codestral",
      "gpt-4.1-mini" = "gpt-4.1 mini",
      "gpt-4.1-nano" = "gpt-4.1 nano",
      "grok-4-1-fast" = "grok-4.1 fast",
      "llama-3.3-70b-instruct" = "llama-3.3 70b",
      "phi-4" = "phi-4"
    ),
    model_short = str_wrap(model_short, width = 12),
    phase_label = factor(phase_label, levels = c(
      "Phase 2: single shot",
      "Phase 3: + compile/run feedback",
      "Phase 4: + refactor seam tooling"))
  )

# Long form for the paired bars (one bar per metric per phase).
long <- df |>
  select(model, model_short, phase_label,
         `Compile-OK%` = compile_pct, `Run-OK%` = run_pct) |>
  pivot_longer(cols = c(`Compile-OK%`, `Run-OK%`),
               names_to = "metric", values_to = "pct") |>
  mutate(
    metric = factor(metric, levels = c("Compile-OK%", "Run-OK%")),
    is_na = is.na(pct),
    pct_plot = ifelse(is_na, 0, pct),
    pct_label = ifelse(is_na, "n/a", sprintf("%.1f%%", pct)),
    label_y = ifelse(is_na, 3.2, pct_plot + 1.8)
  )

# Keep bar order sensible by phase-4 run performance while rendering shortened labels.
label_levels <- df |>
  arrange(model) |>
  distinct(model, model_short) |>
  pull(model_short)

long <- long |>
  mutate(model_short = fct_inorder(model_short))

p <- ggplot(long, aes(x = model_short, y = pct_plot, fill = phase_label)) +
  geom_col(position = position_dodge(width = 0.84), width = 0.74,
           colour = "white", linewidth = 0.25) +
  geom_linerange(
    data = subset(long, is_na),
    aes(ymin = 0, ymax = 2.6),
    position = position_dodge(width = 0.84),
    inherit.aes = TRUE,
    linewidth = 0.9,
    linetype = "22",
    colour = "#b91c1c"
  ) +
  geom_text(
    data = subset(long, !is_na),
    aes(y = label_y, label = pct_label),
    position = position_dodge(width = 0.84),
    vjust = 0,
    size = 3.15,
    fontface = "bold"
  ) +
  geom_text(
    data = subset(long, is_na),
    aes(y = label_y, label = "n/a"),
    position = position_dodge(width = 0.84),
    vjust = 0,
    size = 3.1,
    fontface = "bold",
    colour = "#b91c1c"
  ) +
  facet_wrap(~ metric, nrow = 1) +
  scale_fill_manual(values = c(
    "Phase 2: single shot"           = "#94a3b8",
    "Phase 3: + compile/run feedback" = "#2563eb",
    "Phase 4: + refactor seam tooling" = "#f59e0b"
  )) +
  scale_y_continuous(labels = function(x) paste0(x, "%"),
                     breaks = seq(0, 100, 10),
                     limits = c(0, 106),
                     expand = expansion(mult = c(0, 0.02))) +
  labs(
    title    = "Phase 2 vs Phase 3 vs Phase 4 \u2014 compile and run-OK rates",
    subtitle = "Rates use submitted outputs only. Dashed red markers + n/a labels indicate non-evaluable cells (submitted=0).",
    x = NULL, y = NULL, fill = NULL,
    caption = "Source: tools/viz/data/per_model_phase.csv \u2014 canonical evaluator."
  ) +
  theme_paper() +
  theme(
    axis.text.x     = element_text(angle = 28, hjust = 1, size = 10),
    axis.text.y     = element_text(size = 10),
    legend.position = "top",
    legend.text     = element_text(size = 10),
    legend.key.width = unit(1.3, "lines"),
    strip.text      = element_text(face = "bold", size = 12),
    panel.grid.major.x = element_blank(),
    panel.spacing.x = unit(1.4, "lines"),
    plot.title      = element_text(size = 16, face = "bold"),
    plot.subtitle   = element_text(size = 11),
    plot.caption    = element_text(size = 9)
  )

out <- file.path(figures_dir(), "phase2-vs-phase3.png")
ggsave(out, p, width = 13.5, height = 7.8, dpi = 170, bg = "white")
cat("wrote", out, "\n")
