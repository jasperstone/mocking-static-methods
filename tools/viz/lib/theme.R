# lib/theme.R — shared ggplot theme + palette.

suppressPackageStartupMessages({
  library(ggplot2)
})

theme_paper <- function(base_size = 11) {
  theme_minimal(base_size = base_size) +
    theme(
      plot.title       = element_text(face = "bold"),
      plot.subtitle    = element_text(colour = "grey30"),
      panel.grid.minor = element_blank(),
      strip.text       = element_text(face = "bold"),
      legend.position  = "right",
      plot.background  = element_rect(fill = "white", colour = NA),
      panel.background = element_rect(fill = "white", colour = NA)
    )
}

# Consistent palette for compile vs run-OK across plots.
pal_compile_run <- c("compile-OK" = "#7fb3d5", "run-OK" = "#1f4e79")

# Six model colours (matches phase 3 panel). Add entries as new models appear.
pal_models <- c(
  "codestral-2501"         = "#4c72b0",
  "gpt-4.1-mini"           = "#dd8452",
  "gpt-4.1-nano"           = "#937860",
  "gpt-5-codex"            = "#8172b3",
  "grok-4-1-fast"          = "#55a868",
  "llama-3.3-70b-instruct" = "#c44e52",
  "phi-4"                  = "#da8bc3"
)
