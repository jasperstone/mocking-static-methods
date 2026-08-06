# Failure categorization diagnostics — all_phases

Companion diagnostics only. This view does not change compile/run quality metric definitions.

## Totals

- attempts_total: 16938
- non_submitted_total: 4282
- infra_non_submitted_total: 2251
- infra_share_of_attempts: 13.29%
- infra_share_of_non_submitted: 52.57%

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
| phase3-agentic-loop | timeout/connection | 4 | 0.72% | 0.07% |
| phase3-agentic-loop | auth/access | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | rate-limit | 450 | 81.23% | 8.33% |
| phase3-agentic-loop | server-5xx | 1 | 0.18% | 0.02% |
| phase3-agentic-loop | api-version-unsupported | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | baseline_compile_failed | 2 | 0.36% | 0.04% |
| phase3-agentic-loop | baseline_no_owning_csproj | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | max-turns-exhausted | 83 | 14.98% | 1.54% |
| phase3-agentic-loop | context-length | 5 | 0.90% | 0.09% |
| phase3-agentic-loop | content-filter | 5 | 0.90% | 0.09% |
| phase3-agentic-loop | invalid-prompt | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | adapter-parse-error | 4 | 0.72% | 0.07% |
| phase3-agentic-loop | other | 0 | 0.00% | 0.00% |
| phase4-refactoring | timeout/connection | 0 | 0.00% | 0.00% |
| phase4-refactoring | auth/access | 0 | 0.00% | 0.00% |
| phase4-refactoring | rate-limit | 0 | 0.00% | 0.00% |
| phase4-refactoring | server-5xx | 0 | 0.00% | 0.00% |
| phase4-refactoring | api-version-unsupported | 0 | 0.00% | 0.00% |
| phase4-refactoring | baseline_compile_failed | 1170 | 90.49% | 22.34% |
| phase4-refactoring | baseline_no_owning_csproj | 54 | 4.18% | 1.03% |
| phase4-refactoring | max-turns-exhausted | 66 | 5.10% | 1.26% |
| phase4-refactoring | context-length | 0 | 0.00% | 0.00% |
| phase4-refactoring | content-filter | 3 | 0.23% | 0.06% |
| phase4-refactoring | invalid-prompt | 0 | 0.00% | 0.00% |
| phase4-refactoring | adapter-parse-error | 0 | 0.00% | 0.00% |
| phase4-refactoring | other | 0 | 0.00% | 0.00% |

## All-phases category totals

| category | count | share_of_non_submitted | share_of_attempts |
|---|---:|---:|---:|
| timeout/connection | 4 | 0.09% | 0.02% |
| auth/access | 0 | 0.00% | 0.00% |
| rate-limit | 1921 | 44.86% | 11.34% |
| server-5xx | 326 | 7.61% | 1.92% |
| api-version-unsupported | 0 | 0.00% | 0.00% |
| baseline_compile_failed | 1172 | 27.37% | 6.92% |
| baseline_no_owning_csproj | 54 | 1.26% | 0.32% |
| max-turns-exhausted | 774 | 18.08% | 4.57% |
| context-length | 9 | 0.21% | 0.05% |
| content-filter | 13 | 0.30% | 0.08% |
| invalid-prompt | 1 | 0.02% | 0.01% |
| adapter-parse-error | 8 | 0.19% | 0.05% |
| other | 0 | 0.00% | 0.00% |

## Rerun-needed signal thresholds

- red: infra_non_submitted >= 10 OR infra_share_of_attempts >= 3% OR infra_share_of_non_submitted >= 20%
- yellow: infra_non_submitted >= 3 OR infra_share_of_attempts >= 1% OR infra_share_of_non_submitted >= 10%
- green: otherwise

## Model/run quick check

| model | run | attempts_total | non_submitted_total | infra_non_submitted | infra_share_of_attempts | infra_share_of_non_submitted | rerun_signal |
|---|---:|---:|---:|---:|---:|---:|---|
| gpt-4.1-mini | 1 | 300 | 34 | 32 | 10.67% | 94.12% | red |
| gpt-4.1-mini | 1 | 300 | 66 | 66 | 22.00% | 100.00% | red |
| gpt-4.1-mini | 2 | 300 | 47 | 44 | 14.67% | 93.62% | red |
| gpt-4.1-mini | 2 | 300 | 113 | 113 | 37.67% | 100.00% | red |
| gpt-4.1-mini | 3 | 300 | 56 | 52 | 17.33% | 92.86% | red |
| gpt-4.1-mini | 3 | 300 | 84 | 83 | 27.67% | 98.81% | red |
| gpt-4.1-nano | 1 | 300 | 136 | 64 | 21.33% | 47.06% | red |
| gpt-4.1-nano | 1 | 300 | 23 | 15 | 5.00% | 65.22% | red |
| gpt-4.1-nano | 2 | 300 | 138 | 52 | 17.33% | 37.68% | red |
| gpt-4.1-nano | 2 | 300 | 84 | 79 | 26.33% | 94.05% | red |
| gpt-4.1-nano | 3 | 300 | 131 | 70 | 23.33% | 53.44% | red |
| gpt-4.1-nano | 3 | 300 | 92 | 90 | 30.00% | 97.83% | red |
| gpt-5-codex | 1 | 300 | 245 | 190 | 63.33% | 77.55% | red |
| gpt-5-codex | 2 | 300 | 249 | 193 | 64.33% | 77.51% | red |
| gpt-5-codex | 3 | 300 | 245 | 190 | 63.33% | 77.55% | red |
| grok-4-1-fast | 1 | 300 | 258 | 258 | 86.00% | 100.00% | red |
| grok-4-1-fast | 2 | 300 | 261 | 261 | 87.00% | 100.00% | red |
| grok-4-1-fast | 2 | 300 | 1 | 1 | 0.33% | 100.00% | red |
| grok-4-1-fast | 3 | 300 | 263 | 263 | 87.67% | 100.00% | red |
| grok-4-1-fast | 3 | 300 | 1 | 1 | 0.33% | 100.00% | red |
| llama-3.3-70b-instruct | 1 | 300 | 45 | 42 | 14.00% | 93.33% | red |
| llama-3.3-70b-instruct | 1 | 300 | 2 | 1 | 0.33% | 50.00% | red |
| llama-3.3-70b-instruct | 2 | 300 | 43 | 42 | 14.00% | 97.67% | red |
| llama-3.3-70b-instruct | 3 | 300 | 44 | 43 | 14.33% | 97.73% | red |
| llama-3.3-70b-instruct | 3 | 300 | 4 | 2 | 0.67% | 50.00% | red |
| phi-4 | 2 | 300 | 16 | 3 | 1.00% | 18.75% | yellow |
| codestral-2501 | 1 | 300 | 58 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 1 | 300 | 15 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 1 | 300 | 73 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 2 | 300 | 64 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 2 | 300 | 13 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 2 | 300 | 71 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 3 | 300 | 65 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 3 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| codestral-2501 | 3 | 300 | 73 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-mini | 1 | 277 | 68 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-mini | 2 | 286 | 68 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-mini | 3 | 283 | 68 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-nano | 1 | 268 | 80 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-nano | 2 | 266 | 76 | 0 | 0.00% | 0.00% | green |
| gpt-4.1-nano | 3 | 258 | 83 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 1 | 300 | 1 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 1 | 300 | 69 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 2 | 300 | 69 | 0 | 0.00% | 0.00% | green |
| grok-4-1-fast | 3 | 300 | 70 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 1 | 300 | 68 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 2 | 300 | 2 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 2 | 300 | 68 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 3 | 300 | 68 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 5 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 74 | 0 | 0.00% | 0.00% | green |
| phi-4 | 2 | 300 | 19 | 0 | 0.00% | 0.00% | green |
| phi-4 | 2 | 300 | 75 | 0 | 0.00% | 0.00% | green |
| phi-4 | 3 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| phi-4 | 3 | 300 | 15 | 1 | 0.33% | 6.67% | green |
| phi-4 | 3 | 300 | 72 | 0 | 0.00% | 0.00% | green |

## Output files

- tools/viz/data/all_phases_failure_categories_by_model_run.csv
- tools/viz/data/all_phases_failure_categories_totals.csv
- tools/viz/data/all_phases_failure_rerun_signal_by_model_run.csv
- tools/viz/data/all_phases_failure_categories_summary.json
