# Failure categorization diagnostics — phase4-refactoring

Companion diagnostics only. This view does not change compile/run quality metric definitions.

## Totals

- attempts_total: 5238
- non_submitted_total: 1975
- infra_non_submitted_total: 699
- infra_share_of_attempts: 13.34%
- infra_share_of_non_submitted: 35.39%

## Per-phase category totals

| phase | category | count | share_of_non_submitted | share_of_attempts |
|---|---|---:|---:|---:|
| phase4-refactoring | timeout/connection | 0 | 0.00% | 0.00% |
| phase4-refactoring | auth/access | 0 | 0.00% | 0.00% |
| phase4-refactoring | rate-limit | 3 | 0.15% | 0.06% |
| phase4-refactoring | server-5xx | 0 | 0.00% | 0.00% |
| phase4-refactoring | api-version-unsupported | 696 | 35.24% | 13.29% |
| phase4-refactoring | baseline_compile_failed | 1170 | 59.24% | 22.34% |
| phase4-refactoring | baseline_no_owning_csproj | 54 | 2.73% | 1.03% |
| phase4-refactoring | other | 52 | 2.63% | 0.99% |

## Rerun-needed signal thresholds

- red: infra_non_submitted >= 10 OR infra_share_of_attempts >= 3% OR infra_share_of_non_submitted >= 20%
- yellow: infra_non_submitted >= 3 OR infra_share_of_attempts >= 1% OR infra_share_of_non_submitted >= 10%
- green: otherwise

## Model/run quick check

| model | run | attempts_total | non_submitted_total | infra_non_submitted | infra_share_of_attempts | infra_share_of_non_submitted | rerun_signal |
|---|---:|---:|---:|---:|---:|---:|---|
| phi-4 | 1 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| phi-4 | 2 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| phi-4 | 3 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| codestral-2501 | 1 | 300 | 73 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 2 | 300 | 71 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 3 | 300 | 73 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-mini | 1 | 277 | 68 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-mini | 2 | 286 | 68 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-mini | 3 | 283 | 68 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-nano | 1 | 268 | 80 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-nano | 2 | 266 | 76 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-nano | 3 | 258 | 83 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 1 | 300 | 69 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 2 | 300 | 69 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 3 | 300 | 70 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 1 | 300 | 69 | 1 | 0.33% | 1.45% | green |
| llama-3.3-70b-instruct | 2 | 300 | 70 | 2 | 0.67% | 2.86% | green |
| llama-3.3-70b-instruct | 3 | 300 | 68 | 0 | 0.00% | 0.00% | green |

## Output files

- tools/viz/data/phase4-refactoring_failure_categories_by_model_run.csv
- tools/viz/data/phase4-refactoring_failure_categories_totals.csv
- tools/viz/data/phase4-refactoring_failure_rerun_signal_by_model_run.csv
- tools/viz/data/phase4-refactoring_failure_categories_summary.json
