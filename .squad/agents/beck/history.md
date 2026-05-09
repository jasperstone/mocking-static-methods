# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Reproducible test runs + coverage across 7 .NET OSS repos.

## Core Context

Test/coverage agent. Existing test_logs/ and coverage_logs/ directories track previous runs.

## Learnings

- EF Core release/10.0: 49,056 tests passed across multiple projects (EFCore.Tests 6,622, Sqlite.FunctionalTests 37,278, +12 others).
- aspnetcore: ~137 test projects in AspNetCore.slnx; coverlet.collector add-pass takes 15-30 min.
- reportgenerator command pattern: `reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./CoverageReport" -reporttypes:Html`.

## Recent Updates

- 2026-04-30 — Workflow audit findings: `--collect:"XPlat Code Coverage"` requires coverlet.collector in each test project; workflow adds it nowhere — most jobs will silently produce empty cobertura XML. efcore/orleans/runtime have NO test filters (efcore = 49k tests) — will exceed 6h runner cap. Only HTML reports uploaded; raw `coverage.cobertura.xml` is NOT preserved — reproducibility comparison impossible. Tests failing skips reportgenerator step — decide if `continue-on-error: true` is wanted. Missing `--blame-hang-timeout` and parallelism controls.
- 2026-05-01 — Default workflow dispatch is repo=all (parallel jobs, no time savings from per-repo, but lose drift/timing/infra signal).
- 2026-05-01 — Phase 1 baseline (run 25215078473): TOTAL 6.96% line / 6.52% branch coverage, 1,179 static call sites across 545 classes. **4 of 7 repos (abp/aspnetcore/efcore/roslyn) uploaded 178-byte empty cobertura — CI green but no instrumentation took effect.** Usable repos for Phase 2: orleans, runtime, semantic-kernel. Aggregator: `aggregate_baseline.py`. Outputs: `BASELINE_COVERAGE.md`, `baseline_coverage.csv`, per-repo `baseline_artifacts/<repo>/static_call_classes.json`. Committed 835dcf2, not pushed. StaticCallAnalyzer gotcha: appends to `./analysis_results.json` in CWD — always run from a clean temp dir.

- 2026-05-07 — Team update from Vogel: `StaticCallAnalyzer` is now containerized (multi-stage Dockerfile, SDK 8.0 → runtime 8.0). Use `StaticCallAnalyzer/run.sh` wrapper; `aggregate_baseline.py` invokes it automatically. Host requires only `python3 + gh + docker` — no .NET 8 SDK install needed. Image tag `static-call-analyzer:local`, target source mounted read-only at `/src`. Commit 3d53670 on `jasper/squad`.

## Recent Updates

### 2026-05-07 — Built test-discovery workflow + aggregator

Added a CI-only diagnostic to investigate why orleans/runtime/semantic-kernel/efcore have low coverage (6%, 10%, 12%, 27%): is the FILTER too aggressive, or are the included tests just shallow?

**What I built**
- `.github/workflows/test-discovery.yml` — 6 parallel jobs (skipping runtime; see below). Each job mirrors the coverage workflow's clone+restore+build phase verbatim (same SHAs, same composite actions, same SDK image), then for every discovered test csproj runs `dotnet test --no-build --list-tests` twice — once with the per-repo FILTER, once without. Counts the output and emits one CSV row per project: `repo,project,tests_universe,tests_in_filter,tests_excluded,build_status`. CSV uploaded as `test-discovery-<repo>` artifact.
- `.github/scripts/list_tests.sh` — shared helper sourced by every job. Provides `run_discovery <repo> <filter> <projects...>` and the awk-based `_count_listed_tests` parser. Distinguishes `<not-built>` from `<error>` by sniffing the dotnet output for known phrases.
- `tools/test_discovery/aggregate.py` — local post-processor. Reads downloaded artifacts, emits `TEST_DISCOVERY.md` (per-repo summary, top-10 highest exclusion ratio, bottom-10 lowest tests-in-filter, list of skipped projects) and `test_discovery_summary.csv`.

### 2026-05-07 — Orleans BVT re-included in coverage filter

Test-discovery showed Orleans `ServiceBus.Tests` at 14/108 filter-pass (87% excluded) → 6.07% coverage. Investigated the four Orleans-specific exclusions (`Category!=BVT&Category!=SlowBVT&Category!=LoadShedding&Category!=CorePerf`).

Inventory at pinned SHA `8024faf8`:
- **BVT**: 54 attribute occurrences across 159 files (`[TestCategory("BVT")]` 26 + `[Trait("Category", "BVT")]` 28). Sample `test/Extensions/ServiceBus.Tests/EvictionStrategyTests/EHPurgeLogicTests.cs` is pure unit-level (mocks `CachePressureInjectionMonitor`, `PurgeDecisionInjectionPredicate`, no silo). Orleans tradition: BVT = "Build Verification" = broad correctness suite, predominantly unit-level. **Re-included.**
- **SlowBVT**: 40 occurrences (e.g. `HeterogeneousSilosTests/UpgradeTests`, `ClientConnectionTests`). Slow integration. **Kept excluded.**
- **LoadShedding**: 2 occurrences in `TesterInternal/General/LoadSheddingTest.cs`. Stress. **Kept excluded.**
- **CorePerf**: 6 occurrences in `TesterInternal/StorageTests/PersistenceGrainTests.cs` and `GrainPersistenceTestRunner.cs`. Perf benchmarks. **Kept excluded.**

Mechanism: Orleans defines `[TestCategory(string)]` in `test/TestInfrastructure/TestExtensions/TestCategory.cs` which emits `Category=<name>` xunit traits via `CategoryDiscoverer`, so `Category!=BVT` correctly matched.

Also caught a project-glob gap in `test-discovery.yml` orleans job: `*.Tests.csproj` misses `Orleans.Serialization.UnitTests` and `Orleans.Dashboard.UnitTests` (named `*.UnitTests.csproj`). Coverage workflow runs `dotnet test Orleans.slnx` so those projects DID run during coverage — gap was diagnostic-only. Glob extended to `\( -name "*.Tests.csproj" -o -name "*.UnitTests.csproj" \)`. Did not add `Tester.AdoNet`/`Tester.AzureUtils`/`Tester.Cassandra`/`Tester.Cosmos`/`Tester.Redis`/`Tester.ZooKeeperUtils` — those spin up real infrastructure.

Files: `.github/workflows/coverage-orchestrator.yml` (orleans test step) + `.github/workflows/test-discovery.yml` (orleans list step). Both filters still match. Decision file: `.squad/decisions/inbox/beck-orleans-bvt-decision.md`. Workflow not triggered — Jasper will dispatch.
- `tools/test_discovery/README.md` — usage notes.

**What we learned about the per-repo FILTER logic**
- The "baseline" filter is identical across abp/aspnetcore/efcore/orleans/semantic-kernel: a long `FullyQualifiedName!~…&Category!=…` string excluding Functional/Integration/E2E/EndToEnd/Stress/Performance/Quarantined/Flaky.
- Per-repo additions: **abp** appends `FullyQualifiedName!~SkiaSharp`. **roslyn** appends `FullyQualifiedName!~LanguageServer&TargetFrameworkIdentifier!=.NETFramework`. **orleans** appends `Category!=BVT&Category!=SlowBVT&Category!=LoadShedding&Category!=CorePerf` — this is the smoking gun for orleans's 6%: BVT (Build Verification Tests) is the bulk of orleans's unit suite. **semantic-kernel** appends `FullyQualifiedName!~ConformanceTests`.
- Project-discovery globs differ a lot: aspnetcore needs the FunctionalTests/IntegrationTests/Helix path filters because its src tree contains those siblings; efcore's discovery is multi-language (csproj/fsproj/vbproj) and excludes Specification.Tests; orleans+SK run against slnx in the workflow but I enumerated per-csproj for diagnostic granularity.

**Gotchas hit**
- `--no-build --list-tests` requires built artifacts on disk → the diagnostic can't run locally because `cloned_repos/` is unbuilt. Pivoted to CI-only per the user's instruction. The `tools/test_discovery/` Docker idea is gone; only the CI workflow + local aggregator remain.
- Orleans's coverage workflow runs `dotnet test Orleans.slnx` once instead of per-csproj. For the diagnostic I `find test -name *.Tests.csproj` per-project so we get per-project counts. Same FILTER, same exclusions.
- Runtime is genuinely intractable for this diagnostic: it uses `build.sh -subset libs+libs.tests -test`, which doesn't have an enumerable per-csproj surface. Skipped intentionally; documented in the workflow header and README.
- Composite action `validate-cobertura` deliberately not used — there is no cobertura in this workflow.

**Locally validated**
- `python3 -c 'import yaml; yaml.safe_load(...)'` on the workflow → ok.
- `bash -n` on `list_tests.sh` → ok. `_count_listed_tests` smoke-tested on synthetic dotnet output → returns 3 for a 3-test sample.
- `python3 -m py_compile aggregate.py` → ok. Smoke-tested with 2 synthetic CSVs → produced expected summary csv + markdown.
- Find globs validated against `cloned_repos/`: abp 78, aspnetcore 117, efcore 20, orleans 13, roslyn 54, semantic-kernel 37 projects.

**Pushed** to `jasper/squad` as commit "tooling: add test-discovery workflow + aggregator". Not triggered yet — Jasper will dispatch with `repo=all`.

### 2026-05-07 — Built test-counts-from-coverage-logs parser

Bypassed the broken `--list-tests` path for xunit.v3 repos by reading the
authoritative per-project `dotnet test` summary lines that already exist in
Coverage Orchestrator job logs.

**What I built**
- `tools/test_counts/from_coverage_logs.py` — gh-CLI based; downloads each
  "Coverage: <repo>" job log, regex-matches `Passed!|Failed!  - Failed: N,
  Passed: N, Skipped: N, Total: N, Duration: ... - Foo.dll (framework)`,
  emits per-(repo,project,framework) row. Multi-run merge: most recent
  source_run_id wins per key. Multi-occurrence in same log: last wins
  (handles retries). Default behavior with no args: picks most recent
  successful run via `gh run list ... --status completed --limit 50` and
  filters for conclusion=success.
- `tools/test_counts/README.md`.
- Outputs: `test_counts.csv` + `TEST_COUNTS.md` at repo root.

**Verified results (runs 25468601840 + 25472048463)**

| repo            | projects | total |
|-----------------|---------:|------:|
| abp             | 74       | 1,358 |
| aspnetcore      | 96       | 31,603 |
| efcore          | 14       | 13,724 (matches hand-count exactly) |
| orleans         | 28       | 1,692 |
| roslyn          | 33       | 155,993 |
| semantic-kernel | 44       | 6,263 |

EF Core / Roslyn / ABP now real (was 0 / 3 / 46 from `--list-tests`). Runtime
is the only repo still missing — it uses `build.sh -subset libs+libs.tests
-test` which doesn't emit the parseable per-project line.

**Notes**
- Coverlet-wrapped runs (aspnetcore, orleans, SK) DO emit the summary line
  fine. The original task description's worry that they'd be unparseable was
  wrong — only runtime is genuinely opaque.
- Job-name → repo-slug map needed `.NET Runtime` → `runtime` (initial run
  produced `net-runtime` slug from naive lowercase strip).
- Logs cached at `/tmp/cov_<job_id>.log` so re-runs are free.

Branch: `jasper/squad`. Commit: "tooling: extract per-project test counts
from coverage logs". Decision file:
`.squad/decisions/inbox/beck-test-counts-from-logs.md`.

### 2026-05-07 — Test counts refreshed against run 25495265941 (post-BVT-fix)
Re-ran `tools/test_counts/from_coverage_logs.py 25495265941` after the all-green Orleans BVT run. Headline change: **Orleans 28 / 1,692 → 36 / 10,951** (+8 projects, +9,259 tests) — BVT inclusion landed. Side notes: abp slipped 74 / 1,358 → 73 / 1,346 (one project dropped from logs, likely a flaky-skip change); aspnetcore / efcore / roslyn / semantic-kernel unchanged. New authoritative source = single run 25495265941. Commit 349981e on jasper/squad.

### 2026-05-08 — Baseline matrix update (heads up)

Coverage matrix changed: MAUI removed (4 failed remediation rounds), OpenRA + StockSharp added, Files + PowerToys skipped (Windows-only). Once OpenRA (run 25552129165) and StockSharp (run 25552132370) finish, the next baseline + test-counts refresh covers 15 repos. Both new repos use the external `dotnet-coverage` data-collector path; expect potentially 1–2 rounds of remediation per established pattern. OpenRA targets `net8.0` (side-installs .NET 8 SDK in noble container); StockSharp resolves to `net10.0`.

### 2026-05-08 — Mode #1 attribution diagnosis (Avalonia/eShop/duplicati/runtime)

Brady asked why 4 repos in `tools/coverage_xref/UNIFIED_TABLE.md` show `Mode #1 covered = 0` despite having coverage data. Read-only diagnostic against locally-downloaded cobertura artifacts (gh run download from runs 25532664482 / 25532665179 / 25527102157). Decision drop at `.squad/decisions/inbox/beck-mode1-attribution.md` has the full breakdown.

Two distinct failure modes found:

1. **Empty instrumentation** (Avalonia, eShop) — coverlet.console emits structurally valid cobertura with thousands of classes/lines but **zero hit-lines anywhere in the file**. Path matching works fine; the suffix matcher in `xref_mode1_coverage.py` correctly resolves `tests/Avalonia.Base.UnitTests/...` to `__w/.../target/tests/avalonia.base.unittests/...`. Tests either didn't run under coverlet.console or the in-process collector didn't attach. Diagnostic signature: `lines-valid` is large, all `<line hits="0">`.

2. **Real test-scope gap** (duplicati, runtime) — duplicati has 34,597 hit-lines (real coverage), but the 21 matched Mode#1 sites are in `Backend/Jottacloud`, `RestAPI/Database`, `OneDrive`, `HttpClientExtensions` — code paths the unit tests don't reach. runtime is more severe: cobertura covers `FSharp.Compiler.Service`, `FSharp.Core`, `illink`, source generators (`Microsoft.Interop.*`, `System.Text.*.Generator`); all 33 Mode#1 sites are in `src/libraries/System.Net.Http/...` and `src/libraries/Microsoft.Extensions.*/...` which are not in the test scope at all.

**Useful path shape reference (cobertura `<class filename="...">`):**
- Avalonia: `__w/mocking-static-methods/mocking-static-methods/target/src/Avalonia.Dialogs/...` (workspace-absolute) and `_/src/Avalonia.Dialogs/...` (deterministic-build).
- eShop: `src/Basket.API/...` (project-relative, clean).
- duplicati: `Duplicati/Library/Encryption/...` (project-relative, clean — matches Mode#1 sites directly).
- runtime: `/_/src/arcade/src/...` (deterministic-build absolute prefix).

Suffix-matcher in `xref_mode1_coverage.py` handles all four shapes; no path-matching fix needed.

**Optional xref improvement** (not implemented, just suggested in decision drop): add a `matched_zero_hits_global` status to flag repos where EVERY hit value in the cobertura is 0 — distinguishes "instrumentation broken" from "this line not exercised".

### 2026-05-08 — Mode #1 attribution re-confirmed (duplicati / runtime)

Brady asked for a fresh diagnostic on the 0-covered Mode#1 cells in `tools/coverage_xref/UNIFIED_TABLE.md`. Re-ran the live `load_coverage_map` + `find_site` from `build_unified_table.py` against `/tmp/cov_phase2/coverage-xml-{duplicati,runtime}/`. Result is identical to my 2026-05-08 finding (decision: `beck-mode1-attribution.md`):

- **duplicati**: matcher works. 21/34 sites match directly into cobertura (`direct_in_map=True`) and report `uncovered` (hits=0 — Backend/OAuthHelper/RestAPI surface not driven by the 1,096-test unit suite). 13 sites are in `Duplicati/UnitTest/*.cs` test sources that cobertura correctly omits (prod-only instrumentation).
- **runtime**: matcher works. All 33 sites are `unknown_file`. Cobertura only contains F# compiler, HotReload generator tooling, illink, source generators, arcade — `grep filename=".*System\.Net\.Http"` and `grep filename=".*Microsoft\.Extensions"` both return zero. The libs targeted by the Mode#1 sites (`src/libraries/{System.Net.Http,Microsoft.Extensions.*}`) were never instrumented by the `build.sh -subset libs+libs.tests -test` run.

No fix needed in `find_site` — its 5/4/3/2 suffix match + lowercase forward-slash normalization handles all four cobertura path shapes (project-relative, `/_/src/...` deterministic, `/__w/1/s/...` workspace-absolute, plain `src/...`).

Recommendations sent in `beck-mode1-attribution-gap.md`:
1. runtime is a Vogel/orchestrator issue (artifact glob may be missing per-library cobertura, OR libs.tests didn't emit cobertura at all).
2. duplicati's 13 `unknown_file` sites are an analyzer hygiene issue — Mode1Analyzer should skip test source paths (`*/UnitTest/*`, `*Test*.cs`) so they don't pollute the denominator.
3. The 21 uncovered duplicati sites are a real "code not exercised" signal — keep as-is.
- 2026-05-08: R6 fixes were dispatched, awaiting run results.
