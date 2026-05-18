# plots/heatmap_runok.R — Phase 3 heatmap, run-OK% by (repo, model).
# Output: assets/figures/phase3-heatmap-runok.png

source(file.path("tools", "viz", "lib", "load.R"))
source(file.path("tools", "viz", "lib", "theme.R"))

suppressPackageStartupMessages({
  library(dplyr)
  library(ggplot2)
  library(scales)
})

# Cells with very few submissions are noise (a single coin-flip can swing them
# to 0% or 100%). Mask them visually and label with N only so the eye doesn't
# anchor on a meaningless percentage. Threshold chosen so efcore (N=1 target
# in v2 -> ~3 attempts per model over 3 runs) and roslyn (N=5 targets -> ~4-15
# attempts per model) are excluded from the colour scale.
LOW_N_THRESHOLD <- 10

df <- load_per_model_repo() |>
  mutate(
    model = factor(model),
    repo = factor(repo),
    low_n = submitted < LOW_N_THRESHOLD,
    fill_pct = ifelse(low_n, NA_real_, run_pct),
    label = ifelse(
      low_n,
      sprintf("n=%d", submitted),
      sprintf("%.0f%%\n(n=%d)", run_pct, submitted)
    )
  )

model_order <- df |>
  filter(!low_n) |>
  group_by(model) |>
  summarise(run = sum(run_ok), sub = sum(submitted), .groups = "drop") |>
  mutate(pct = run / sub) |>
  arrange(desc(pct)) |>
  pull(model) |>
  as.character()

repo_order <- df |>
  filter(!low_n) |>
  group_by(repo) |>
  summarise(run = sum(run_ok), sub = sum(submitted), .groups = "drop") |>
  mutate(pct = run / sub) |>
  arrange(pct) |>
  pull(repo) |>
  as.character()

# Any repos that consist entirely of low-N cells (efcore in v2) still need a
# row position; append them at the bottom of the y axis.
all_repos <- as.character(levels(df$repo))
low_only_repos <- setdiff(all_repos, repo_order)
repo_order <- c(low_only_repos, repo_order)

df <- df |>
  mutate(
    model = factor(model, levels = model_order),
    repo  = factor(repo,  levels = repo_order)
  )

p <- ggplot(df, aes(x = model, y = repo, fill = fill_pct)) +
  geom_tile(colour = "white", linewidth = 0.4) +
  geom_text(aes(label = label, colour = low_n), size = 2.6, lineheight = 0.9) +
  scale_fill_viridis_c(
    option = "C", limits = c(0, 60), oob = scales::squish,
    name = "run-OK %", na.value = "grey85"
  ) +
  scale_colour_manual(values = c("FALSE" = "white", "TRUE" = "grey30"),
                      guide = "none") +
  labs(
    title    = "Phase 3 \u2014 run-OK rate by repo x model (3-run totals)",
    subtitle = paste0(
      "4,855 submissions across 6 models x 12 repos. Cells with n<",
      LOW_N_THRESHOLD,
      " submissions shown in grey (too few to be meaningful)."
    ),
    x = NULL, y = NULL
  ) +
  theme_paper() +
  theme(
    axis.text.x = element_text(angle = 30, hjust = 1),
    panel.grid  = element_blank()
  )

out <- file.path(figures_dir(), "phase3-heatmap-runok.png")
ggsave(out, p, width = 9, height = 6, dpi = 150, bg = "white")
cat("wrote", out, "\n")
