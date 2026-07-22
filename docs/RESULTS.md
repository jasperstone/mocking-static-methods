# Results

Cross-phase scoreboard. One row per sealed phase × model. Numbers are sourced from each phase's `phases/phaseN/phase.lock.yaml` `results:` block and `results/aggregate.csv`; the lock file wins on disagreement.

Phase 2+ rows show **mean ± stddev across 5 runs of that (phase × model) cell**.

## Headline

| Phase | Strategy | Model | Targets | Newly covered (mean ± σ) | Pass (mean ± σ) | Compile-fail | Runtime-fail | Δ line cov |
|---|---|---|---:|---:|---:|---:|---:|---:|
| [phase1-baseline](phases/phase1-baseline/) | no generation | n/a | n/a | n/a | n/a | n/a | n/a | — (baseline = 58.23%) |
<!-- Phase 2 will append five rows here, one per model in the panel. Phase 3, 4, 5 do the same. -->

## Model panel (frozen for phases 2+)

All five models route through GitHub Models (`https://models.github.ai/inference`). Generic ids; the actual snapshot served at run time is captured into each phase's `phase.lock.yaml.generation.model_snapshots_observed`.

| Slot | Model id | Lineage | Tier |
|---|---|---|---|
| 1 | `anthropic/claude-opus-4-5` | Anthropic | frontier |
| 2 | `anthropic/claude-sonnet-4-5` | Anthropic | mid |
| 3 | `openai/gpt-5` | OpenAI | frontier |
| 4 | `openai/o3` | OpenAI | reasoning |
| 5 | `google/gemini-2-5-pro` | Google | frontier |

Per-cell parameters (frozen): `temperature=0.0`, `top_p=1.0`, `seed=42`, `max_output_tokens=4096`, `runs_per_model=5`.

## Baseline anchors

Recorded once; later phases compare against these.

| Metric | Value | Source |
|---|---:|---|
| Repos in matrix | 15 | [phases/phase1-baseline/phase.lock.yaml](phases/phase1-baseline/phase.lock.yaml) |
| Existing tests aggregated | 249,955 | [phases/phase1-baseline/REPORT.md](phases/phase1-baseline/REPORT.md) |
| Total line coverage | 58.23% | [phases/phase1-baseline/reports/unified_table.csv](phases/phase1-baseline/reports/unified_table.csv) |
| Mode #1 sites detected (all) | 6,321 | [phases/phase1-baseline/reports/mode1_sites.csv](phases/phase1-baseline/reports/mode1_sites.csv) |

Partition of all 6,321 detected sites — every site lands in exactly one bucket:

| Bucket | Count | In `targets/v1/`? |
|---|---:|:---:|
| **production, uncovered, in cobertura** | **3,147** | ✅ **the input set** ([targets/v1/targets.csv](targets/v1/targets.csv)) |
| production, already covered | 1,087 | no — covered by existing suite |
| non-production path (test/sample/benchmark) | 1,167 | no — out of scope |
| production, `unknown_file` (no cobertura entry) | 901 | no — deferred to `targets/v2/` |
| production, `unknown_line` (multi-line expr) | 19 | no — data quality |
| **TOTAL** | **6,321** | |

Production Mode #1 sites = 3,147 + 1,087 + 901 + 19 = 5,154. The headline "21.1% covered" in phase 1 reports is `1,087 / 5,154`.

## Per-repo line coverage at baseline (phase 1)

| Repo | Tests | Mode #1 covered | Line cov % | Notes |
|---|---:|---:|---:|---|
| abp | 1,358 | 192 / 1,012 | 42.04% | broad-filter solution test, integration excluded |
| **aspnetcore** | 31,603 | 314 / 923 | **64.36%** | over 50% gate |
| **Avalonia** | 6,860 | 0 / 0 | **73.25%** | over 50% gate; 0 production Mode #1 sites in cobertura's loaded files |
| duplicati | 1,096 | 0 / 21 | 36.18% | only one viable unit-test project upstream |
| efcore | 13,724 | 11 / 13 | 27.37% | `EFCore.Tests` + `EFCore.Specification.Tests` only |
| eShop | — | 4 / 94 | 13.95% | only `Ordering.UnitTests` + `Basket.UnitTests` are unit-style |
| **garnet** | 3,563 | 179 / 688 | **65.99%** | over 50% gate |
| **jellyfin** | 2,740 | 153 / 1,199 | **55.93%** | over 50% gate; crossed at phase 1 seal |
| OpenRA | 473 | 0 / 13 | 5.84% | game engine, headless container can't drive renderer paths |
| orleans | 11,041 | 154 / 290 | 40.47% | BVT included; SlowBVT/perf excluded |
| **roslyn** | 155,997 | 4 / 110 | **84.92%** | over 50% gate |
| runtime | 6,012 | 1 / 33 | 14.83% | `libs+libs.tests` subset only (no CoreCLR/Mono build) |
| semantic-kernel | 6,263 | 31 / 583 | 27.24% | conformance/process-integration excluded (need real vector DB / Dapr) |
| server | 5,118 | 44 / 172 | 3.44% | bitwarden: 992k instrumented LOC, 5k unit tests; remainder integration-bound |
| StockSharp | 4,107 | 0 / 3 | 30.96% | one Tests.csproj exists upstream |
| **TOTAL** | **249,955** | **1,087 / 5,154** | **58.23%** | |
