# plots/all_phases_infrastructure_failures.R — Absolute infrastructure failures by phase.
# Output: assets/figures/all-phases-infrastructure-failures.png

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
  "tools", "viz", "data", "all_phases_failure_categories_totals.csv"
)

if (!file.exists(path)) {
  stop(
    "all-phases failure totals CSV not found. Run: ",
    "python3 tools/analysis/phase4_failure_categorization.py ",
    "--phases phase2-agentic,phase3-agentic-loop,phase4-refactoring"
  )
}

phase_order <- c("phase2-agentic", "phase3-agentic-loop", "phase4-refactoring")
phase_labels <- c(
  "phase2-agentic" = "Phase 2",
  "phase3-agentic-loop" = "Phase 3",
  "phase4-refactoring" = "Phase 4"
)
infra_order <- c(
  "timeout/connection",
  "auth/access",
  "rate-limit",
  "server-5xx",
  "api-version-unsupported"
)
category_labels <- c(
  "timeout/connection" = "Timeout /\nconnection",
  "auth/access" = "Authentication /\naccess",
  "rate-limit" = "Rate limit",
  "server-5xx" = "Server 5xx",
  "api-version-unsupported" = "Unsupported\nAPI version",
  "total" = "TOTAL INFRA"
)

infra <- read_csv(path, show_col_types = FALSE) |>
  filter(scope == "phase_total", phase %in% phase_order, category %in% infra_order) |>
  select(phase, category, count)

totals <- infra |>
  group_by(phase) |>
  summarise(category = "total", count = sum(count), .groups = "drop")

df <- bind_rows(infra, totals) |>
  mutate(
    phase = factor(phase, levels = rev(phase_order), labels = rev(unname(phase_labels[phase_order]))),
    category = factor(
      category,
      levels = c(infra_order, "total"),
      labels = unname(category_labels[c(infra_order, "total")])
    ),
    label = comma(count),
    light_text = count >= 100
  )

p <- ggplot(df, aes(x = category, y = phase, fill = count)) +
  geom_tile(colour = "white", linewidth = 1.5) +
  geom_text(aes(label = label, colour = light_text), fontface = "bold", size = 5) +
  scale_fill_gradient(
    low = "#fff5f0",
    high = "#a50f15",
    trans = "sqrt",
    breaks = c(0, 1, 10, 100, 1000),
    labels = label_comma(),
    name = "Failures"
  ) +
  scale_colour_manual(
    values = c("FALSE" = "#3b0a0a", "TRUE" = "white"),
    guide = "none"
  ) +
  labs(
    title = "Infrastructure failures requiring correction",
    subtitle = "Absolute canonical counts. Every infrastructure category is shown, including zeros.",
    x = NULL,
    y = NULL,
    caption = "Source: tools/viz/data/all_phases_failure_categories_totals.csv"
  ) +
  theme_paper(base_size = 12) +
  theme(
    axis.text.x = element_text(face = "bold"),
    axis.text.y = element_text(face = "bold", size = 12),
    panel.grid = element_blank(),
    legend.position = "right"
  )

out <- file.path(figures_dir(), "all-phases-infrastructure-failures.png")
ggsave(out, p, width = 12, height = 4.5, dpi = 180, bg = "white")
cat("wrote", out, "\n")
