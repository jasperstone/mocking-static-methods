# Phase 2 Coverage Baseline — Final

**Branch:** `jasper/phase2`  ·  **As of:** 2026-05-09  ·  **Last commit:** dd7f9c3+

## Per-repo final coverage

| Repo | Tests | Mode #1 covered | Line cov % | Status |
|---|---:|---:|---:|---|
| abp | 1,358 | 192 / 1,012 | 42.04% | under target |
| **aspnetcore** | 31,603 | 314 / 923 | **64.36%** | ✅ over 50% |
| **Avalonia** | 6,860 | 0 / 0 | **73.25%** | ✅ over 50% |
| duplicati | 1,096 | 0 / 21 | 36.18% | scope-capped |
| efcore | 13,724 | 11 / 13 | 27.37% | scope-capped |
| eShop | — | 4 / 94 | 13.95% | scope-capped |
| **garnet** | 3,563 | 179 / 688 | **65.99%** | ✅ over 50% |
| **jellyfin** | 2,740 | 153 / 1,199 | **55.93%** | ✅ over 50% (newly crossed) |
| OpenRA | 473 | 0 / 13 | 5.84% | accept (headless) |
| orleans | 11,041 | 154 / 290 | 40.47% | under target |
| **roslyn** | 155,997 | 4 / 110 | **84.92%** | ✅ over 50% |
| runtime | 6,012 | 1 / 33 | 14.83% | scope-capped |
| semantic-kernel | 6,263 | 31 / 583 | 27.24% | under target |
| server | 5,118 | 44 / 172 | 3.44% | structural cap |
| StockSharp | 4,107 | 0 / 3 | 30.96% | scope-capped |
| **TOTAL** | **249,955** | **1,087 / 5,154** | **58.23%** |  |

## Repos at or above 50% (5 of 15)

- **aspnetcore** 64.36% (was 60.36%)
- **Avalonia** 73.25%
- **garnet** 65.99%
- **jellyfin** 55.93% — **newly crossed** tonight
- **roslyn** 84.92% (was 76.21%)

## Repos under 50% — give-up reasons

| Repo | Cov % | Reason capped |
|---|---:|---|
| abp | 42.04% | Already runs the whole solution with broad filter; remaining gap is integration / functional tests we deliberately exclude. Pushing past 50% would require relaxing the filter, which lets in DB-bound suites that fail without external infra. |
| duplicati | 36.18% | Only one viable unit-test project (`Duplicati.UnitTest`). `Browser.Test` requires Playwright browsers; `LiveTests/Backend.Tests` requires real cloud credentials. Both are deliberately excluded. |
| efcore | 27.37% | Scope is `EFCore.Tests` + `EFCore.Specification.Tests`. Provider FunctionalTests (Cosmos, Sqlite, SqlServer) are categorically excluded — they're integration-bound and add 30k+ tests that double the wall-clock without advancing Mode #1 coverage. |
| eShop | 13.95% | Only `Ordering.UnitTests` and `Basket.UnitTests` are unit-style. `ClientApp.UnitTests` requires the `maui-tizen` workload (NETSDK1147 on the .NET 10 noble container). `Ordering.FunctionalTests` requires Aspire host. `EventBus.UnitTests` does not exist in the repo. |
| OpenRA | 5.84% | Game engine. UI/SDL/rendering tests cannot run headless on a Linux container without a display and audio driver. The 13 Mode #1 sites are all in renderer paths. **Accepted as expected.** |
| orleans | 40.47% | Already runs the whole solution including BVT (re-enabled 2026-05-07). The remaining gap is `SlowBVT` / `LoadShedding` / `CorePerf` (heterogeneous-silo upgrades, stress, perf). Including them risks runner pressure and adds little to Mode #1 coverage. |
| runtime | 14.83% | Build is `-subset libs+libs.tests`. Most of the dotnet/runtime codebase is `clr` (CoreCLR) and `mono`, neither of which we build. `System.Net.Http` + `Microsoft.Extensions.*` already added manually. **Accepted as expected — the libs subset is the relevant scope.** |
| semantic-kernel | 27.24% | Whole `SK-dotnet.slnx` runs with broad filter. `ConformanceTests` are categorically excluded — they need real vector DB endpoints (Cosmos, Pinecone, Weaviate, Qdrant, etc). Process.IntegrationTests need Dapr. |
| server | 3.44% | Bitwarden is ~992k instrumented lines; only 5,118 unit tests across 18 unit-test csprojs cover ~34k lines. Real coverage requires `*.IntegrationTest` projects, which need EF + SQL Server + Identity infra. Structural cap of unit-only scope. |
| StockSharp | 30.96% | Single `Tests.csproj` is the only test project in the repo. 4,107 tests run via dotnet-coverage MTP wrapper. Adding more would require building tests that don't exist. |

## What changed tonight

1. **Per-source-file coverage dedup (commit `dd7f9c3`)** — biggest single fix. Per-csproj cobertura output from coverlet enumerates *every* instrumented assembly the test process loaded, so naively summing root `lines-valid` across N test projects double-counts shared production sources by N while `lines-covered` reflects only the one runner that hit them. Fixed `tools/coverage_xref/build_unified_table.py:line_coverage()` to build a `(file, line) → max-hits` map across all cobertura files and sum unique pairs once. Also tightened to direct `<class>/<lines>/<line>` children (cobertura repeats the same line elements under `<methods>/<method>/<lines>`).

   **Impact:**
   - jellyfin **11.24% → 55.93%** (16 cobertura files)
   - orleans **10.00% → 40.47%**
   - semantic-kernel **12.12% → 27.24%**
   - aspnetcore **60.36% → 64.36%**
   - roslyn **76.21% → 84.92%**
   - server **1.71% → 3.44%**
   - **TOTAL 33.04% → 58.23%**

2. **Test-count scraping** — confirmed totals for the four repos previously reported as `—`:
   - Avalonia: **6,860** (sum of lowercase `total: N` across 5 UnitTests assemblies, job 75121694042)
   - runtime: **6,012** (sum of `Total: N` across 12 libs.tests assemblies, job 75113219551)
   - StockSharp: **4,107** (StockSharp.Tests.dll MTP `Passed!` line, job 75121921818)
   - eShop: still `—` (the captured run crashed both unit suites under coverlet.console with 0% per-project coverage; no parseable test totals survive)

3. **Updated `_TEST_COUNTS` doc string** — documents the three patterns scraped (`Total:`, `total:`, `Passed!`) so future maintenance knows what to look for.

## What did NOT change

No CI runs were dispatched. The structural per-csproj-cobertura inflation was the dominant cause of the apparent low coverage on jellyfin / orleans / semantic-kernel; fixing the aggregation was higher-leverage than re-running CI for the same artifacts. Repos that remain under 50% are at structural caps (unit-only scope, missing integration infrastructure, or platform-specific test gates), not measurement artifacts.

## Confirmation

- ✅ `tools/coverage_xref/UNIFIED_TABLE.md` regenerated and reflects every successful run currently in `/tmp/cov_phase2/`.
- ✅ `tools/coverage_xref/build_unified_table.py` carries the dedup fix and updated test-count scraping notes.
- ✅ This file (`BASELINE_FINAL.md`) is the morning briefing.
- ✅ Branch `jasper/phase2` is up to date on origin (commit `dd7f9c3`+).

## Phase-2 readiness

The baseline is clean and ready for the test-generation phase. **5,154 production Mode #1 static-call sites** have been catalogued across 15 repos; **1,087 (21.1%)** are already covered by the existing test suites. The remaining 4,067 are the candidate set for new test generation.

Five repos are at or above the 50% line-coverage gate. The under-50% tail is dominated by structurally capped repos (unit-only scope vs integration-heavy codebases) — those are still useful Mode #1 site sources, but their line-coverage numbers reflect their existing test-suite shape, not a deficiency in the orchestrator.
