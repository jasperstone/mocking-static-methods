# plots/phase4_failure_buckets.R — Phase-4 non-submitted failure categories.
# Output: assets/figures/phase4-failure-buckets.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

# Policy: rerunnable infra throttling (rate-limit) is not a model-failure bucket
# for headline charts; it should be rerun until the model has a chance to answer.
excluded_rerun_only <- c("rate-limit")

df <- load_phase4_failure_by_model_run() |>
  filter(!(category %in% excluded_rerun_only)) |>
  mutate(
    category = factor(
      category,
      levels = c(
        "timeout/connection",
        "auth/access",
        "server-5xx",
        "api-version-unsupported",
        "max-turns-exhausted",
        "context-length",
        "content-filter",
        "invalid-prompt",
        "adapter-parse-error",
        "baseline_compile_failed",
        "baseline_no_owning_csproj",
        "other"
      )
    )
  ) |>
  group_by(model, category) |>
  summarise(count = sum(count), .groups = "drop") |>
  filter(count > 0)

if (nrow(df) == 0) {
  message("phase4_failure_buckets.R: no non-submitted failure rows after rerun-only exclusions; nothing to plot.")
} else {
  model_order <- df |>
    group_by(model) |>
    summarise(total = sum(count), .groups = "drop") |>
    arrange(desc(total)) |>
    pull(model)

  df <- df |>
    mutate(model = factor(model, levels = model_order))

  pal_failure <- c(
    "timeout/connection"     = "#577590",
    "auth/access"            = "#43aa8b",
    "server-5xx"             = "#f9844a",
    "api-version-unsupported" = "#277da1",
    "max-turns-exhausted"    = "#e9c46a",
    "context-length"         = "#f4a261",
    "content-filter"         = "#e76f51",
    "invalid-prompt"         = "#c77dff",
    "adapter-parse-error"    = "#b5838d",
    "baseline_compile_failed" = "#f94144",
    "baseline_no_owning_csproj" = "#9d4edd",
    "other"                  = "#adb5bd"
  )

  p <- ggplot(df, aes(x = model, y = count, fill = category)) +
    geom_col(width = 0.72, colour = "white", linewidth = 0.2) +
    scale_fill_manual(values = pal_failure) +
    scale_y_continuous(labels = label_number(big.mark = ","),
                       expand = expansion(mult = c(0, 0.08))) +
    labs(
      title = "Phase 4 non-submitted failures by category and model",
      subtitle = "Aggregated across run_1 through run_3; rerun-only rate-limit events are excluded.",
      x = NULL,
      y = "non-submitted failures",
      fill = "category",
      caption = "Source: tools/viz/data/phase4-refactoring_failure_categories_by_model_run.csv (aggregated by model across runs)"
    ) +
    theme_paper() +
    theme(
      axis.text.x = element_text(angle = 20, hjust = 1),
      legend.position = "bottom",
      panel.grid.major.x = element_blank()
    )

  out <- file.path(figures_dir(), "phase4-failure-buckets.png")
  ggsave(out, p, width = 12, height = 4.8, dpi = 150, bg = "white")
  cat("wrote", out, "\n")
}
