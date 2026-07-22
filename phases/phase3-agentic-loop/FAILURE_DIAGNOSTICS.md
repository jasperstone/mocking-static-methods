# Failure categorization diagnostics — phase3-agentic-loop

Companion diagnostics only. This view does not change compile/run quality metric definitions.

## Totals

- attempts_total: 5400
- non_submitted_total: 554
- infra_non_submitted_total: 455
- infra_share_of_attempts: 8.43%
- infra_share_of_non_submitted: 82.13%

## Per-phase category totals

| phase | category | count | share_of_non_submitted | share_of_attempts |
|---|---|---:|---:|---:|
| phase3-agentic-loop | timeout/connection | 4 | 0.72% | 0.07% |
| phase3-agentic-loop | auth/access | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | rate-limit | 450 | 81.23% | 8.33% |
| phase3-agentic-loop | server-5xx | 1 | 0.18% | 0.02% |
| phase3-agentic-loop | baseline_compile_failed | 2 | 0.36% | 0.04% |
| phase3-agentic-loop | baseline_no_owning_csproj | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | other | 97 | 17.51% | 1.80% |

## Rerun-needed signal thresholds

- red: infra_non_submitted >= 10 OR infra_share_of_attempts >= 3% OR infra_share_of_non_submitted >= 20%
- yellow: infra_non_submitted >= 3 OR infra_share_of_attempts >= 1% OR infra_share_of_non_submitted >= 10%
- green: otherwise

## Model/run quick check

| model | run | attempts_total | non_submitted_total | infra_non_submitted | infra_share_of_attempts | infra_share_of_non_submitted | rerun_signal |
|---|---:|---:|---:|---:|---:|---:|---|
| gpt-4.1-mini | 1 | 300 | 66 | 66 | 22.00% | 100.00% | red |
| gpt-4.1-mini | 2 | 300 | 113 | 113 | 37.67% | 100.00% | red |
| gpt-4.1-mini | 3 | 300 | 84 | 83 | 27.67% | 98.81% | red |
| gpt-4.1-nano | 1 | 300 | 23 | 15 | 5.00% | 65.22% | red |
| gpt-4.1-nano | 2 | 300 | 84 | 79 | 26.33% | 94.05% | red |
| gpt-4.1-nano | 3 | 300 | 92 | 90 | 30.00% | 97.83% | red |
| grok-4-1-fast | 2 | 300 | 1 | 1 | 0.33% | 100.00% | red |
| grok-4-1-fast | 3 | 300 | 1 | 1 | 0.33% | 100.00% | red |
| llama-3.3-70b-instruct | 1 | 300 | 2 | 1 | 0.33% | 50.00% | red |
| llama-3.3-70b-instruct | 3 | 300 | 4 | 2 | 0.67% | 50.00% | red |
| phi-4 | 2 | 300 | 16 | 3 | 1.00% | 18.75% | yellow |
| codestral-2501 | 1 | 300 | 15 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 2 | 300 | 13 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 3 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 1 | 300 | 1 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 2 | 300 | 2 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 5 | 0 | 0.00% | 0.00% | green |
| phi-4 | 3 | 300 | 15 | 1 | 0.33% | 6.67% | green |

## Output files

- tools/viz/data/phase3-agentic-loop_failure_categories_by_model_run.csv
- tools/viz/data/phase3-agentic-loop_failure_categories_totals.csv
- tools/viz/data/phase3-agentic-loop_failure_rerun_signal_by_model_run.csv
- tools/viz/data/phase3-agentic-loop_failure_categories_summary.json
