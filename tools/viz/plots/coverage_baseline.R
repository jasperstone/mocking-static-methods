# plots/coverage_baseline.R — per-repo line coverage (dumbbell with branch overlay).
# Input: baseline_coverage.csv (repo root, read in place — not duplicated).
# Output: assets/figures/coverage-baseline.png
#
# TODO: When phase 2/3 coverage CSVs exist, extend this into a per-phase
# coverage-progression chart (one panel per phase, or a slopegraph by repo).

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

cov <- load_baseline_coverage()
total_row <- cov |> filter(repo == "TOTAL")
bars <- cov |>
  filter(repo != "TOTAL") |>
  arrange(line_pct) |>
  mutate(repo = factor(repo, levels = repo))

has_branch <- any(!is.na(bars$branch_pct))

p <- ggplot(bars, aes(y = repo))

if (has_branch) {
  p <- p +
    geom_segment(aes(x = branch_pct, xend = line_pct, yend = repo),
                 colour = "grey75", linewidth = 1.2) +
    geom_point(aes(x = branch_pct), colour = "#dd8452", size = 3.2) +
    geom_text(aes(x = branch_pct, label = sprintf("%.1f%%", branch_pct)),
              hjust = 1.25, size = 3, colour = "#a85a25")
}

p <- p +
  geom_col(aes(x = line_pct), fill = "#1f4e79", width = 0.55) +
  geom_text(aes(x = line_pct, label = sprintf("%.1f%%", line_pct)),
            hjust = -0.15, size = 3, colour = "#1f4e79") +
  geom_vline(xintercept = total_row$line_pct[[1]],
             linetype = "dashed", colour = "grey40") +
  annotate("text",
           x = total_row$line_pct[[1]],
           y = nrow(bars) + 0.4,
           label = sprintf("TOTAL line %.1f%%", total_row$line_pct[[1]]),
           hjust = -0.05, vjust = 0, size = 3.2, colour = "grey25") +
  scale_x_continuous(labels = label_percent(scale = 1),
                     limits = c(0, max(bars$line_pct) * 1.15),
                     expand = expansion(mult = c(0, 0.05))) +
  coord_cartesian(clip = "off") +
  labs(
    title    = "Baseline coverage by repo (phase 1)",
    subtitle = ifelse(
      has_branch,
      "Bar = line coverage %, orange dot = branch coverage %. Dashed line = pooled TOTAL line coverage.",
      "Bar = line coverage %. Dashed line = pooled TOTAL line coverage."
    ),
    x = NULL, y = NULL
  ) +
  theme_paper() +
  theme(
    panel.grid.major.y = element_blank(),
    plot.margin = margin(5, 30, 5, 5)
  )

out <- file.path(figures_dir(), "coverage-baseline.png")
ggsave(out, p, width = 9, height = 5.5, dpi = 150, bg = "white")
cat("wrote", out, "\n")
