# Failure categorization diagnostics — phase2-agentic

Companion diagnostics only. This view does not change compile/run quality metric definitions.

## Totals

- attempts_total: 6300
- non_submitted_total: 2435
- infra_non_submitted_total: 1796
- infra_share_of_attempts: 28.51%
- infra_share_of_non_submitted: 73.76%

## Per-phase category totals

| phase | category | count | share_of_non_submitted | share_of_attempts |
|---|---|---:|---:|---:|
| phase2-agentic | timeout/connection | 0 | 0.00% | 0.00% |
| phase2-agentic | auth/access | 0 | 0.00% | 0.00% |
| phase2-agentic | rate-limit | 1471 | 60.41% | 23.35% |
| phase2-agentic | server-5xx | 325 | 13.35% | 5.16% |
| phase2-agentic | api-version-unsupported | 0 | 0.00% | 0.00% |
| phase2-agentic | baseline_compile_failed | 0 | 0.00% | 0.00% |
| phase2-agentic | baseline_no_owning_csproj | 0 | 0.00% | 0.00% |
| phase2-agentic | max-turns-exhausted | 625 | 25.67% | 9.92% |
| phase2-agentic | context-length | 4 | 0.16% | 0.06% |
| phase2-agentic | content-filter | 5 | 0.21% | 0.08% |
| phase2-agentic | invalid-prompt | 1 | 0.04% | 0.02% |
| phase2-agentic | adapter-parse-error | 4 | 0.16% | 0.06% |
| phase2-agentic | other | 0 | 0.00% | 0.00% |

## Rerun-needed signal thresholds

- red: infra_non_submitted >= 10 OR infra_share_of_attempts >= 3% OR infra_share_of_non_submitted >= 20%
- yellow: infra_non_submitted >= 3 OR infra_share_of_attempts >= 1% OR infra_share_of_non_submitted >= 10%
- green: otherwise

## Model/run quick check

| model | run | attempts_total | non_submitted_total | infra_non_submitted | infra_share_of_attempts | infra_share_of_non_submitted | rerun_signal |
|---|---:|---:|---:|---:|---:|---:|---|
| gpt-4.1-mini | 1 | 300 | 34 | 32 | 10.67% | 94.12% | red |
| gpt-4.1-mini | 2 | 300 | 47 | 44 | 14.67% | 93.62% | red |
| gpt-4.1-mini | 3 | 300 | 56 | 52 | 17.33% | 92.86% | red |
| gpt-4.1-nano | 1 | 300 | 136 | 64 | 21.33% | 47.06% | red |
| gpt-4.1-nano | 2 | 300 | 138 | 52 | 17.33% | 37.68% | red |
| gpt-4.1-nano | 3 | 300 | 131 | 70 | 23.33% | 53.44% | red |
| gpt-5-codex | 1 | 300 | 245 | 190 | 63.33% | 77.55% | red |
| gpt-5-codex | 2 | 300 | 249 | 193 | 64.33% | 77.51% | red |
| gpt-5-codex | 3 | 300 | 245 | 190 | 63.33% | 77.55% | red |
| grok-4-1-fast | 1 | 300 | 258 | 258 | 86.00% | 100.00% | red |
| grok-4-1-fast | 2 | 300 | 261 | 261 | 87.00% | 100.00% | red |
| grok-4-1-fast | 3 | 300 | 263 | 263 | 87.67% | 100.00% | red |
| llama-3.3-70b-instruct | 1 | 300 | 45 | 42 | 14.00% | 93.33% | red |
| llama-3.3-70b-instruct | 2 | 300 | 43 | 42 | 14.00% | 97.67% | red |
| llama-3.3-70b-instruct | 3 | 300 | 44 | 43 | 14.33% | 97.73% | red |
| codestral-2501 | 1 | 300 | 58 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 2 | 300 | 64 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 3 | 300 | 65 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| phi-4 | 2 | 300 | 19 | 0 | 0.00% | 0.00% | green |
| phi-4 | 3 | 300 | 17 | 0 | 0.00% | 0.00% | green |

## Output files

- tools/viz/data/phase2-agentic_failure_categories_by_model_run.csv
- tools/viz/data/phase2-agentic_failure_categories_totals.csv
- tools/viz/data/phase2-agentic_failure_rerun_signal_by_model_run.csv
- tools/viz/data/phase2-agentic_failure_categories_summary.json
