# plots/phase2_vs_phase3.R — Paired-bar comparison of compile-OK% and run-OK%
# between phase 2 (single shot) and phase 3 (agentic loop with compile + run
# feedback) for the 6-model panel that appears in both.
#
# Output: assets/figures/phase2-vs-phase3.png
#
# The headline visual: grok-4-1-fast goes from 0.8% to 14.7% run-OK once it
# can see its own compile errors. Every model gains, but the gain is widest
# on models that were submission-shy in phase 2.

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(tidyr)
  library(ggplot2)
  library(scales)
})

# Six-model panel that exists in BOTH phases. Excludes gpt-5-codex (only in
# phase 2) so the comparison is apples-to-apples.
PANEL <- c("codestral-2501", "gpt-4.1-mini", "gpt-4.1-nano",
           "grok-4-1-fast", "llama-3.3-70b-instruct", "phi-4")

df <- load_per_model_phase() |>
  filter(model %in% PANEL,
         phase %in% c("phase2-agentic", "phase3-agentic-loop")) |>
  mutate(
    compile_pct = 100 * compile_ok / attempts,
    run_pct     = 100 * run_ok     / attempts,
    phase_label = recode(phase,
      "phase2-agentic"      = "Phase 2: single shot",
      "phase3-agentic-loop" = "Phase 3: + compile/run feedback")
  )

# Order models by phase-3 run-OK% (best-first) so the strongest gainers cluster
# on the left.
model_order <- df |>
  filter(phase == "phase3-agentic-loop") |>
  arrange(desc(run_pct)) |>
  pull(model)

df <- df |>
  mutate(
    model       = factor(model, levels = model_order),
    phase_label = factor(phase_label, levels = c(
      "Phase 2: single shot",
      "Phase 3: + compile/run feedback"))
  )

# Long form for the paired bars (one bar per metric per phase).
long <- df |>
  select(model, phase_label, `Compile-OK%` = compile_pct, `Run-OK%` = run_pct) |>
  pivot_longer(cols = c(`Compile-OK%`, `Run-OK%`),
               names_to = "metric", values_to = "pct") |>
  mutate(metric = factor(metric, levels = c("Compile-OK%", "Run-OK%")))

p <- ggplot(long, aes(x = model, y = pct, fill = phase_label)) +
  geom_col(position = position_dodge(width = 0.78), width = 0.72,
           colour = "white", linewidth = 0.25) +
  geom_text(aes(label = sprintf("%.1f%%", pct)),
            position = position_dodge(width = 0.78),
            vjust = -0.4, size = 2.9) +
  facet_wrap(~ metric, nrow = 1) +
  scale_fill_manual(values = c(
    "Phase 2: single shot"           = "#94a3b8",
    "Phase 3: + compile/run feedback" = "#2563eb"
  )) +
  scale_y_continuous(labels = function(x) paste0(x, "%"),
                     expand = expansion(mult = c(0, 0.12))) +
  labs(
    title    = "Phase 2 vs Phase 3 \u2014 compile and run-OK rates",
    subtitle = "Same 6 models, same 300 v2 cells. Phase 3 adds an in-loop compile+run feedback budget. Bars ordered by phase-3 run-OK%.",
    x = NULL, y = NULL, fill = NULL,
    caption = "Source: tools/viz/data/per_model_phase.csv \u2014 canonical evaluator."
  ) +
  theme_paper() +
  theme(
    axis.text.x     = element_text(angle = 20, hjust = 1, size = 9),
    legend.position = "top",
    strip.text      = element_text(face = "bold", size = 11),
    panel.grid.major.x = element_blank()
  )

out <- file.path(figures_dir(), "phase2-vs-phase3.png")
ggsave(out, p, width = 10, height = 5.5, dpi = 150, bg = "white")
cat("wrote", out, "\n")
