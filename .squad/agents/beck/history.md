# Beck — Test & Coverage Engineer (history)

## Project Context

- **Project:** mocking-static-methods
- **User:** Jasper (Brady also active 2026-05-08+)
- **Created:** 2026-04-30
- **Goal:** Reproducible test runs + coverage across 7 .NET OSS repos (expanded to 15 in Phase 2).

## Core Context

Test/coverage agent. Owns `tools/test_counts/`, `tools/test_discovery/`, `tools/coverage_xref/`, `tools/viz/`, `tools/cost/`, `aggregate_baseline.py`, and Phase 2/3 results aggregation.

## Learnings (durable)

### Filters & test categories
- Orleans `BVT` ("Build Verification") is mostly unit-level despite the name — re-include in coverage filter. `SlowBVT`, `LoadShedding`, `CorePerf` stay excluded (integration/perf).
- Orleans defines `[TestCategory(s)]` in `test/TestInfrastructure/TestExtensions/TestCategory.cs` that emits xunit `Category=<name>` trait via `CategoryDiscoverer`, so `Category!=BVT` filter strings actually match.
- Project-discovery globs miss `*.UnitTests.csproj` if only `*.Tests.csproj` is matched (Orleans `Orleans.Serialization.UnitTests`, `Orleans.Dashboard.UnitTests`). Use `\( -name "*.Tests.csproj" -o -name "*.UnitTests.csproj" \)`.

### Test counts (authoritative source)
- `--list-tests` is broken for xunit.v3 repos (returns 0/3/46 for EF Core / Roslyn / ABP). DO NOT use as authoritative.
- Authoritative source: `tools/test_counts/from_coverage_logs.py` parsing per-project `Passed!  - Failed: N, Passed: N, Skipped: N, Total: N, Duration: ... - Foo.dll (framework)` summary lines from Coverage Orchestrator job logs.
- dotnet-coverage MTP wrapper emits **lowercase** `total: N` inside `Test run summary` blocks (distinct shape from classic uppercase `Total: N`). StockSharp MTP exe uses uppercase. Avalonia per-csproj loop: 5 assemblies, lowercase. runtime XPlat: 12 assemblies, uppercase.
- Job-name → repo-slug map: `.NET Runtime` → `runtime` (naive lowercase strip produces wrong slug `net-runtime`).

### Mode #1 attribution (UNIFIED_TABLE.md)
- `find_site` suffix-matcher in `tools/coverage_xref/build_unified_table.py` correctly handles 4 cobertura path shapes: project-relative, `/_/src/...` deterministic-build, `/__w/1/s/...` workspace-absolute, plain `src/...`. Lowercase + forward-slash normalize + 5/4/3/2 suffix match.
- Two distinct "Mode#1 covered=0" failure modes:
  1. **Empty instrumentation** (Avalonia, eShop): coverlet.console emits structurally valid XML but **0 hit-lines globally**. Tests passed; collector never attached. Suggested xref enhancement: `matched_zero_hits_global` status to flag this.
  2. **Real test-scope gap** (duplicati, runtime): cobertura has real hits but Mode#1 sites are in unreached code (duplicati: Backend/OAuth/RestAPI not exercised by 1,096-test unit suite) or scope-mismatched (runtime: only F#/illink/codegen instrumented; `System.Net.Http`/`Microsoft.Extensions.*` libs absent).
- Mode1Analyzer hygiene improvement (not yet shipped): skip test source paths (`*/UnitTest/*`, `*/Tests/*`, `*Test*.cs`) at ingest to remove test-source self-references that inflate denominator.

### Per-csproj cobertura inflation (THE dedup fix, 2026-05-09)
- **Root cause:** Each per-project `coverage.cobertura.xml` enumerates every loaded assembly, not just files owned by the test project. Summing root `lines-valid` across N cobertura files multiplies shared production sources N×; `lines-covered` reflects only the one runner that hit them. → synthetically deflated coverage.
- **Fix:** in `tools/coverage_xref/build_unified_table.py`, build per-`(file, line)` map across all cobertura files, take max hits, sum unique lines-valid + lines-covered once. Iterate ONLY direct `<class>/<lines>/<line>` children — cobertura repeats line elements under `<methods>/<method>/<lines>` (double-count trap).
- **Initial bug:** `cur = line_map.get(num, 0); if hits > cur: line_map[num] = hits` registers a line only when hit > 0 → every dict entry is hit → 100%. Fix: `cur = line_map.get(num, -1)` so zero-hit lines register as instrumented.
- **Impact:** TOTAL coverage 33.04% → 58.23%. jellyfin 11.24% → 55.93%. aspnetcore 60→64, roslyn 76→85, orleans 10→40.

### Methodology
- Most "low coverage" findings are measurement artifacts. Fix the math before dispatching CI to expand scope.
- Repos still <50% after dedup (server 3.4%, eShop 14%, runtime 15%, OpenRA 6%) are at structural caps: unit-only scope deliberately excludes integration infra (DBs, browsers, displays, platform workloads).
- StaticCallAnalyzer gotcha: appends to `./analysis_results.json` in CWD — always run from clean temp dir.
- `mcr.microsoft.com/dotnet/sdk:10.0-noble` runs **mawk**, not gawk. 3-arg `match($0, /re/, arr)` silently degrades. Use grep+sed for regex-with-captures inside this container.

### tools/viz (2026-05-16 restructure)
- `tools/cost/estimate.py::PRICES` is the canonical model price table. Reuse via `from tools.cost.estimate import PRICES`. Never duplicate.
- Phase 2 attempts live in TWO sibling result dirs: `phases/phase2-agentic/results/` AND `phases/phase2-agentic/results_v1_oldprompt/`. Inclusive glob `results*/**/attempts.jsonl` = 6,307 attempts / $89.98 (matches published COSTS.md). Filtering out v1 drops 7 attempts / $0.11 and silently disagrees.
- Phase 3 raw attempts/eval NOT committed (`phases/phase3-agentic-loop/results/` empty at top level). Per-model rows synthesised from `tools/viz/data/per_model_repo.csv`; `cost_usd` blank for phase 3.
- `baseline_coverage.csv` has 7 repos + TOTAL row. Column names have spaces/parens — rename in loader (`Lines (total)` → `lines_total` etc).
- Aggregator: one row per `(phase, model)`. Join `evaluation.jsonl` to `attempts.jsonl` by `(target_id, run_index, model_id)`. Dedup with `seen_eval` set so re-runs don't double-count.
- **ggrepel is NOT installed** in the main devcontainer. Use plain `geom_text` with `check_overlap=TRUE` or `hjust/vjust`.
- Each plot file sources `lib/load.R` + `lib/theme.R` at top with paths relative to repo root. `repo_root()` walks up looking for `mocking-static-methods.sln` sentinel.
- `render_phase3.R` retained as one-line shim sourcing `render_all.R` for back-compat.
- "No duplication" rule: only files that don't exist anywhere else go in `tools/viz/data/` (e.g. `per_model_phase.csv`). `baseline_coverage.csv` read in place from repo root.
- Output PNGs in `assets/figures/` use `bg = "white"` for dark-mode previews.
- Verification numbers (phase2-agentic): 6,307 attempts / 3,870 submitted / 326 compile_ok / 129 run_ok / $89.98. Phase3 (from per_model_repo.csv): 1,688 submitted / 270 compile_ok / 132 run_ok / cost n/a.

## Recent Updates

### 2026-05-16 — tools/viz restructure (commit pending)
Split `render_phase3.R` into `tools/viz/plots/*.R` + `tools/viz/lib/{load,theme}.R`. Added `aggregate_phase_results.py` → derived `tools/viz/data/per_model_phase.csv`. Four new plot families: `successful_tests_progression`, `coverage_baseline`, `cost_efficiency`, `cost_per_passing_test`. Phase2 totals reconcile to COSTS.md. Decision: `2026-05-16: tools/viz restructure`.

### 2026-05-09 — Cobertura dedup fix (see Learnings above for details)
Per-csproj cobertura inflation root-caused and fixed in `build_unified_table.py`. TOTAL 33.04→58.23%.

### 2026-05-08 — Mode #1 attribution diagnostics
Read-only investigation of 4 Mode#1=0 repos (Avalonia, eShop, duplicati, runtime). Two failure modes: empty-instrumentation (Avalonia/eShop) vs real test-scope gap (duplicati/runtime). No xref change needed. Decisions: `2026-05-08: Mode #1 attribution diagnosis` and `2026-05-08: Mode #1 attribution gap — not a path-matcher bug`.

### 2026-05-08 — Baseline matrix update
Matrix is now 15 repos (MAUI removed; OpenRA + StockSharp added; Files + PowerToys skipped Windows-only). Next baseline + test-counts refresh once new runs complete.

### Earlier entries
Pre-2026-05-08 entries (Phase 1 baseline, test-discovery workflow, Orleans BVT decision, test-counts-from-coverage-logs tool, refresh against run 25495265941) archived to `history-archive.md`.
