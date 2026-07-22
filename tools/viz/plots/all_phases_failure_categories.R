# plots/all_phases_failure_categories.R — Model-specific failure-category share mix across phases 2/3/4.
# Output: assets/figures/all-phases-failure-category-shares-by-model-faceted.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(readr)
  library(scales)
})

path <- file.path(
  repo_root(),
  "tools", "viz", "data", "all_phases_failure_categories_by_model_run.csv"
)

if (!file.exists(path)) {
  stop(
    "all-phases failure taxonomy by-model-run CSV not found. Run: ",
    "python3 tools/analysis/phase4_failure_categorization.py ",
    "--phases phase2-agentic,phase3-agentic-loop,phase4-refactoring"
  )
}

infra_categories <- c("timeout/connection", "auth/access", "rate-limit", "server-5xx", "api-version-unsupported")
baseline_categories <- c("baseline_compile_failed", "baseline_no_owning_csproj")

label_map <- c(
  "timeout/connection" = "Infra · timeout/connection",
  "auth/access" = "Infra · auth/access",
  "rate-limit" = "Infra · rate-limit",
  "server-5xx" = "Infra · server-5xx",
  "api-version-unsupported" = "Infra · API version unsupported",
  "baseline_compile_failed" = "Baseline · compile failed",
  "baseline_no_owning_csproj" = "Baseline · no owning csproj",
  "other" = "Other"
)

palette <- c(
  "Infra · timeout/connection" = "#4E79A7",
  "Infra · auth/access" = "#76B7B2",
  "Infra · rate-limit" = "#59A14F",
  "Infra · server-5xx" = "#499894",
  "Infra · API version unsupported" = "#277DA1",
  "Baseline · compile failed" = "#E15759",
  "Baseline · no owning csproj" = "#B07AA1",
  "Other" = "#ADB5BD"
)

phase_order <- c("phase2-agentic", "phase3-agentic-loop", "phase4-refactoring")
cat_order <- c(
  "timeout/connection",
  "auth/access",
  "rate-limit",
  "server-5xx",
  "api-version-unsupported",
  "baseline_compile_failed",
  "baseline_no_owning_csproj",
  "other"
)

df <- read_csv(path, show_col_types = FALSE) |>
  filter(phase %in% phase_order, category %in% cat_order) |>
  group_by(phase, model, category) |>
  summarise(
    count = sum(count, na.rm = TRUE),
    non_submitted_total = sum(non_submitted_total, na.rm = TRUE),
    .groups = "drop"
  ) |>
  mutate(
    share_of_non_submitted = if_else(non_submitted_total > 0, count / non_submitted_total, 0)
  ) |>
  mutate(
    phase = factor(phase, levels = phase_order),
    model = factor(model),
    category = factor(category, levels = cat_order),
    bucket = case_when(
      category %in% infra_categories ~ "infra",
      category %in% baseline_categories ~ "baseline",
      TRUE ~ "other"
    ),
    category_label = factor(unname(label_map[as.character(category)]), levels = unname(label_map[cat_order]))
  ) |>
  filter(!is.na(phase), !is.na(model), !is.na(category), share_of_non_submitted > 0)

if (nrow(df) == 0) {
  message("all_phases_failure_categories.R: no rows with non-zero shares; nothing to plot.")
} else {
  model_order <- df |>
    filter(category %in% infra_categories) |>
    group_by(model) |>
    summarise(infra_share = mean(share_of_non_submitted, na.rm = TRUE), .groups = "drop") |>
    arrange(desc(infra_share)) |>
    pull(model)

  if (length(model_order) == 0) {
    model_order <- df |>
      distinct(model) |>
      pull(model)
  }

  df <- df |>
    mutate(model = factor(as.character(model), levels = as.character(model_order)))

  p <- ggplot(df, aes(x = phase, y = share_of_non_submitted, fill = category_label)) +
    geom_col(width = 0.72, colour = "white", linewidth = 0.25) +
    facet_wrap(~ model, ncol = 4) +
    scale_fill_manual(values = palette, drop = FALSE) +
    scale_y_continuous(
      labels = percent_format(accuracy = 1),
      expand = expansion(mult = c(0, 0.03))
    ) +
    labs(
      title = "Failure-category mix by model across phase 2, phase 3, and phase 4",
      subtitle = "Each panel is one model. Stacked shares of non-submitted attempts by phase; infra and baseline buckets are color-separated.",
      x = NULL,
      y = "share of non-submitted attempts",
      fill = "category",
      caption = "Source: tools/viz/data/all_phases_failure_categories_by_model_run.csv (aggregated by model and phase across runs)"
    ) +
    theme_paper() +
    theme(
      axis.text.x = element_text(face = "bold"),
      panel.grid.major.x = element_blank(),
      strip.text = element_text(face = "bold", size = 9),
      legend.position = "bottom"
    )

  out <- file.path(figures_dir(), "all-phases-failure-category-shares-by-model-faceted.png")
  ggsave(out, p, width = 14, height = 8.5, dpi = 150, bg = "white")
  cat("wrote", out, "\n")
}
