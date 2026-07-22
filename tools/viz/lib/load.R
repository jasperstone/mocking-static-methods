# lib/load.R — shared data loaders for the viz pipeline.
#
# Every plot file sources this and theme.R at the top. Loaders read from canonical
# locations in the repo (NOT duplicated copies). Derived aggregations live under
# tools/viz/data/.

suppressPackageStartupMessages({
  library(readr)
  library(dplyr)
})

# Resolve the repo root by walking up from this file. Allows plot files to be
# `source()`d from any cwd.
repo_root <- function() {
  # this file is at <root>/tools/viz/lib/load.R
  here <- tryCatch(
    normalizePath(sys.frame(1)$ofile %||% NULL, mustWork = FALSE),
    error = function(e) NULL
  )
  # Fallback: walk up from getwd() looking for a sentinel.
  candidates <- c(here, getwd())
  for (start in candidates) {
    if (is.null(start) || !nzchar(start)) next
    dir <- if (file.info(start)$isdir %in% TRUE) start else dirname(start)
    for (i in 1:8) {
      if (file.exists(file.path(dir, "mocking-static-methods.sln"))) return(dir)
      parent <- dirname(dir)
      if (parent == dir) break
      dir <- parent
    }
  }
  # Last resort: assume cwd is the repo root.
  getwd()
}

`%||%` <- function(a, b) if (is.null(a)) b else a

figures_dir <- function() {
  d <- file.path(repo_root(), "assets", "figures")
  dir.create(d, recursive = TRUE, showWarnings = FALSE)
  d
}

# ---- loaders -----------------------------------------------------------------

load_per_model_repo <- function() {
  path <- file.path(repo_root(), "tools", "viz", "data", "per_model_repo.csv")
  read_csv(path, show_col_types = FALSE)
}

load_per_model_phase <- function() {
  path <- file.path(repo_root(), "tools", "viz", "data", "per_model_phase.csv")
  if (!file.exists(path)) {
    stop(
      "tools/viz/data/per_model_phase.csv not found. ",
      "Run: python3 tools/viz/aggregate_phase_results.py"
    )
  }
  read_csv(path, show_col_types = FALSE)
}

load_phase4_failure_by_model_run <- function() {
  path <- file.path(
    repo_root(),
    "tools", "viz", "data", "phase4-refactoring_failure_categories_by_model_run.csv"
  )
  if (!file.exists(path)) {
    stop(
      "phase4 failure taxonomy CSV not found. Run: ",
      "python3 tools/analysis/phase4_failure_categorization.py"
    )
  }
  read_csv(path, show_col_types = FALSE)
}

load_baseline_coverage <- function() {
  path <- file.path(repo_root(), "baseline_coverage.csv")
  cov <- read_csv(path, show_col_types = FALSE) |>
    rename(
      repo            = Repo,
      lines_total     = `Lines (total)`,
      lines_covered   = `Lines (covered)`,
      line_pct        = `Line coverage %`,
      branches_total  = `Branches (total)`,
      branches_covered = `Branches (covered)`,
      branch_pct      = `Branch coverage %`,
      static_sites    = `Static call sites`,
      classes_static  = `Classes with static calls`
    )

  # Some snapshots have a placeholder baseline_coverage.csv with all-zero
  # coverage values. Fall back to the canonical phase-1 unified table when
  # this happens so visuals stay truthful without manual file surgery.
  if (sum(cov$lines_total, na.rm = TRUE) > 0) {
    return(cov)
  }

  fallback <- file.path(repo_root(), "phases", "phase1-baseline", "reports", "unified_table.csv")
  if (!file.exists(fallback)) {
    return(cov)
  }

  unified <- read_csv(fallback, show_col_types = FALSE) |>
    transmute(
      repo = repo,
      lines_total = as.numeric(lines_valid),
      lines_covered = as.numeric(lines_covered),
      line_pct = as.numeric(line_coverage_pct),
      branches_total = NA_real_,
      branches_covered = NA_real_,
      branch_pct = NA_real_,
      static_sites = as.numeric(mode1_total),
      classes_static = NA_real_
    )

  total <- unified |>
    summarise(
      repo = "TOTAL",
      lines_total = sum(lines_total, na.rm = TRUE),
      lines_covered = sum(lines_covered, na.rm = TRUE),
      line_pct = ifelse(lines_total > 0, 100 * lines_covered / lines_total, 0),
      branches_total = NA_real_,
      branches_covered = NA_real_,
      branch_pct = NA_real_,
      static_sites = sum(static_sites, na.rm = TRUE),
      classes_static = NA_real_
    )

  bind_rows(unified, total)
}
