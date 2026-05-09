# Results

Cross-phase scoreboard. One row per sealed phase. Numbers are sourced from each phase's `phases/phaseN/phase.lock.yaml` `results:` block and must match. If a row disagrees with its lock file, the lock file wins and this table is wrong.

## Headline

| Phase | Strategy | Targets attempted | Newly covered | Pass | Compile-fail | Runtime-fail | Δ line cov vs phase 1 |
|---|---|---:|---:|---:|---:|---:|---:|
| [phase1-baseline](phases/phase1-baseline/) | no generation (existing suites only) | n/a | n/a | n/a | n/a | n/a | — (baseline = 58.23%) |

## Baseline anchors

Recorded once; later phases compare against these.

| Metric | Value | Source |
|---|---:|---|
| Repos in matrix | 15 | [phases/phase1-baseline/phase.lock.yaml](phases/phase1-baseline/phase.lock.yaml) |
| Existing tests aggregated | 249,955 | [phases/phase1-baseline/REPORT.md](phases/phase1-baseline/REPORT.md) |
| Total line coverage | 58.23% | [phases/phase1-baseline/reports/unified_table.csv](phases/phase1-baseline/reports/unified_table.csv) |
| Mode #1 production sites | 5,154 | [phases/phase1-baseline/reports/mode1_coverage.csv](phases/phase1-baseline/reports/mode1_coverage.csv) |
| — already covered | 1,087 | (excluded from `targets/v1/`) |
| — uncovered, in cobertura | 3,147 | [targets/v1/targets.csv](targets/v1/targets.csv) ← **the input set** |
| — uncovered, missing from cobertura | 901 | (deferred to `targets/v2/`) |
| — non-production / unknown-line | 1,186 | (out of scope) |

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
