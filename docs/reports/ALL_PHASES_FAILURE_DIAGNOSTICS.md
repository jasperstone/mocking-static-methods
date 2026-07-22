# Failure categorization diagnostics — all_phases

Companion diagnostics only. This view does not change compile/run quality metric definitions.

## Totals

- attempts_total: 16938
- non_submitted_total: 9424
- infra_non_submitted_total: 6401
- infra_share_of_attempts: 37.79%
- infra_share_of_non_submitted: 67.92%

## Per-phase category totals

| phase | category | count | share_of_non_submitted | share_of_attempts |
|---|---|---:|---:|---:|
| phase2-agentic | timeout/connection | 0 | 0.00% | 0.00% |
| phase2-agentic | auth/access | 0 | 0.00% | 0.00% |
| phase2-agentic | rate-limit | 411 | 9.59% | 6.52% |
| phase2-agentic | server-5xx | 325 | 7.59% | 5.16% |
| phase2-agentic | api-version-unsupported | 2616 | 61.06% | 41.52% |
| phase2-agentic | baseline_compile_failed | 456 | 10.64% | 7.24% |
| phase2-agentic | baseline_no_owning_csproj | 36 | 0.84% | 0.57% |
| phase2-agentic | other | 440 | 10.27% | 6.98% |
| phase3-agentic-loop | timeout/connection | 1 | 0.03% | 0.02% |
| phase3-agentic-loop | auth/access | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | rate-limit | 2 | 0.06% | 0.04% |
| phase3-agentic-loop | server-5xx | 0 | 0.00% | 0.00% |
| phase3-agentic-loop | api-version-unsupported | 2347 | 74.15% | 43.46% |
| phase3-agentic-loop | baseline_compile_failed | 717 | 22.65% | 13.28% |
| phase3-agentic-loop | baseline_no_owning_csproj | 33 | 1.04% | 0.61% |
| phase3-agentic-loop | other | 65 | 2.05% | 1.20% |
| phase4-refactoring | timeout/connection | 0 | 0.00% | 0.00% |
| phase4-refactoring | auth/access | 0 | 0.00% | 0.00% |
| phase4-refactoring | rate-limit | 3 | 0.15% | 0.06% |
| phase4-refactoring | server-5xx | 0 | 0.00% | 0.00% |
| phase4-refactoring | api-version-unsupported | 696 | 35.24% | 13.29% |
| phase4-refactoring | baseline_compile_failed | 1170 | 59.24% | 22.34% |
| phase4-refactoring | baseline_no_owning_csproj | 54 | 2.73% | 1.03% |
| phase4-refactoring | other | 52 | 2.63% | 0.99% |

## All-phases category totals

| category | count | share_of_non_submitted | share_of_attempts |
|---|---:|---:|---:|
| timeout/connection | 1 | 0.01% | 0.01% |
| auth/access | 0 | 0.00% | 0.00% |
| rate-limit | 416 | 4.41% | 2.46% |
| server-5xx | 325 | 3.45% | 1.92% |
| api-version-unsupported | 5659 | 60.05% | 33.41% |
| baseline_compile_failed | 2343 | 24.86% | 13.83% |
| baseline_no_owning_csproj | 123 | 1.31% | 0.73% |
| other | 557 | 5.91% | 3.29% |

## Rerun-needed signal thresholds

- red: infra_non_submitted >= 10 OR infra_share_of_attempts >= 3% OR infra_share_of_non_submitted >= 20%
- yellow: infra_non_submitted >= 3 OR infra_share_of_attempts >= 1% OR infra_share_of_non_submitted >= 10%
- green: otherwise

## Model/run quick check

| model | run | attempts_total | non_submitted_total | infra_non_submitted | infra_share_of_attempts | infra_share_of_non_submitted | rerun_signal |
|---|---:|---:|---:|---:|---:|---:|---|
| gpt-4.1-mini | 1 | 300 | 265 | 224 | 74.67% | 84.53% | red |
| gpt-4.1-mini | 1 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| gpt-4.1-mini | 2 | 300 | 268 | 227 | 75.67% | 84.70% | red |
| gpt-4.1-mini | 2 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| gpt-4.1-mini | 3 | 300 | 271 | 230 | 76.67% | 84.87% | red |
| gpt-4.1-mini | 3 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| gpt-4.1-nano | 1 | 300 | 279 | 226 | 75.33% | 81.00% | red |
| gpt-4.1-nano | 1 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| gpt-4.1-nano | 2 | 300 | 281 | 223 | 74.33% | 79.36% | red |
| gpt-4.1-nano | 2 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| gpt-4.1-nano | 3 | 300 | 278 | 232 | 77.33% | 83.45% | red |
| gpt-4.1-nano | 3 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| gpt-5-codex | 1 | 300 | 245 | 190 | 63.33% | 77.55% | red |
| gpt-5-codex | 2 | 300 | 249 | 193 | 64.33% | 77.51% | red |
| gpt-5-codex | 3 | 300 | 245 | 190 | 63.33% | 77.55% | red |
| grok-4-1-fast | 1 | 300 | 295 | 254 | 84.67% | 86.10% | red |
| grok-4-1-fast | 2 | 300 | 293 | 252 | 84.00% | 86.01% | red |
| grok-4-1-fast | 2 | 300 | 260 | 192 | 64.00% | 73.85% | red |
| grok-4-1-fast | 3 | 300 | 289 | 248 | 82.67% | 85.81% | red |
| grok-4-1-fast | 3 | 300 | 259 | 191 | 63.67% | 73.75% | red |
| llama-3.3-70b-instruct | 1 | 300 | 264 | 223 | 74.33% | 84.47% | red |
| llama-3.3-70b-instruct | 1 | 300 | 260 | 192 | 64.00% | 73.85% | red |
| llama-3.3-70b-instruct | 2 | 300 | 260 | 219 | 73.00% | 84.23% | red |
| llama-3.3-70b-instruct | 3 | 300 | 262 | 221 | 73.67% | 84.35% | red |
| llama-3.3-70b-instruct | 3 | 300 | 259 | 191 | 63.67% | 73.75% | red |
| phi-4 | 1 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| phi-4 | 2 | 300 | 259 | 191 | 63.67% | 73.75% | red |
| phi-4 | 2 | 300 | 300 | 232 | 77.33% | 77.33% | red |
| phi-4 | 3 | 300 | 300 | 232 | 77.33% | 77.33% | red |
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
| llama-3.3-70b-instruct | 1 | 300 | 69 | 1 | 0.33% | 1.45% | green |
| llama-3.3-70b-instruct | 2 | 300 | 2 | 0 | 0.00% | 0.00% | green |
| llama-3.3-70b-instruct | 2 | 300 | 70 | 2 | 0.67% | 2.86% | green |
| llama-3.3-70b-instruct | 3 | 300 | 68 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| phi-4 | 1 | 300 | 5 | 0 | 0.00% | 0.00% | green |
| phi-4 | 2 | 300 | 19 | 0 | 0.00% | 0.00% | green |
| phi-4 | 3 | 300 | 17 | 0 | 0.00% | 0.00% | green |
| phi-4 | 3 | 300 | 15 | 1 | 0.33% | 6.67% | green |

## Output files

- tools/viz/data/all_phases_failure_categories_by_model_run.csv
- tools/viz/data/all_phases_failure_categories_totals.csv
- tools/viz/data/all_phases_failure_rerun_signal_by_model_run.csv
- tools/viz/data/all_phases_failure_categories_summary.json
