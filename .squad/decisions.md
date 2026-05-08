# Squad Decisions

## Active Decisions

### 2026-04-30: Coverage workflow architecture

**By:** Lewis (approved), proposed by Vogel + Watney + Beck

**Decision:**
- Each repo's coverage job runs inside `mcr.microsoft.com/dotnet/sdk:10.0-bookworm-slim` container.
- All target-repo refs are pinned to commit SHAs (resolved 2026-04-30 from local clones).
- All GitHub Actions and the runner image (`ubuntu-24.04`) are SHA-pinned.
- Coverage collection is dual-mode: native `--collect:"XPlat Code Coverage"` for repos that already include coverlet.collector (aspnetcore, efcore, orleans, semantic-kernel), and external `dotnet-coverage` tool for repos that don't (abp, roslyn, runtime). Zero modifications to cloned repos.
- Runtime job installs native deps (cmake, clang, llvm, lld, libicu-dev, etc.) inside the container.
- Test filters skip integration / E2E / quarantined only — no .csproj changes.
- `continue-on-error: true` on test steps so reportgenerator and artifact upload always run.
- Both HTML coverage reports and raw cobertura XML are uploaded as artifacts (90 days retention for XML, 30 for HTML).
- `push:` trigger removed; workflow_dispatch only (avoids surprise CI bills during the experiment).
- `prepare-disk` job NOT used: each container job runs on its own VM, so a host-level cleanup job can't free disk for downstream jobs.

**Pinned target SHAs:**
- abp: `ea4bbb8b517869a9fb735ea5bc05c819c209d0b5` (tag 10.0.2)
- aspnetcore: `ecb199c29cbefb6fcb6aa789436de36e44427a78`
- efcore: `45e3af0273b71919189367bc152a335b69f443c6`
- orleans: `8024faf860549cb960b4b573c1571b379e283daa` (tag v10.0.0)
- roslyn: `02d301627ed5016a4c18acd1a35e5bbc20ff03f0` (release/dev18.3 tip; replaces stale `3f2819f9...`)
- runtime: `9ffface2f3fa6fbbb427793c3230b1626a1fdd84` (tag v10.0.2)
- semantic-kernel: `0c898161a355b0a845aea48de79cb43e2e9435d2` (tag dotnet-1.70.0)

**Pinned action SHAs:**
- actions/checkout: `11bd71901bbe5b1630ceea73d27597364c9af683` (v4.2.2)
- actions/upload-artifact: `b4b15b8c7c6ac21ea08fcf65892d2ee8f75cf882` (v4.4.3)
- actions/cache: `1bd1e32a3bdc45362d1e726936510720a7c30a57` (v4.2.0)

### 2026-04-30: Methodology — finding buildable SDK commits

**By:** Lewis

When a target repo's tags reference internal RC/servicing SDKs that aren't publicly available, run `git log -p --all -- global.json | grep -E "(^commit|version.*10\.0\.10[1-9])"` to locate commits where `global.json` updates to a publicly released SDK (e.g., 10.0.101) rather than an internal `-rc.X` or `-servicing.X` version. This is how the aspnetcore pin `ecb199c29cbefb6fcb6aa789436de36e44427a78` was discovered. Reusable for any dotnet repo whose tags lag public SDK availability.

**Source:** Preserved from pre-Squad scratch note `aspnetcore_build_results.md` (deleted 2026-04-30 with Jasper's authorization).

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

### 2026-04-30: Tracked root-level .md purge

**By:** Lewis (Lead) — authorized by Jasper

**What:** Deleted 13 tracked root-level .md files via `git rm`. All described the superseded `test_orchestrator.py` / ABP-only PoC architecture and predate the current `coverage-orchestrator.yml` + `StaticCallAnalyzer/` design. None contained unique operational facts worth preserving — patterns, commands, and workflow are now captured in README.md, the StaticCallAnalyzer source, and the coverage-orchestrator workflow.

**Deleted:** 00_START_HERE.md, ABP_WORKFLOW.md, AGENT_TOOLS_EXAMPLES.md, ANALYSIS_REPORT.md, DELIVERY_SUMMARY.md, DOCUMENTATION_INDEX.md, DOCUMENTATION_MANIFEST.md, QUICK_REFERENCE.md, QUICK_START.md, TEST_ORCHESTRATOR_INDEX.md, TEST_ORCHESTRATOR_OVERVIEW.md, TEST_ORCHESTRATOR_README.md, TEST_ORCHESTRATOR_REFINEMENT.md.

**Kept:** README.md, LICENSE, csharptune/README.md (unique component-level doc).

**Why:** Default-discard policy. Comprehensive documentation pass deferred until CI is stable.

### 2026-04-30: Container image switched to `10.0-noble`
**By:** Vogel (CI/CD), requested by Jasper
**What:** Replaced all 8 occurrences of `mcr.microsoft.com/dotnet/sdk:10.0-bookworm-slim` with `mcr.microsoft.com/dotnet/sdk:10.0-noble` in `.github/workflows/coverage-orchestrator.yml` (1 env var + 7 job `container.image` fields).
**Why:** First CI run (25187422035) failed all 7 jobs at "Initialize containers" — the `10.0-bookworm-slim` tag is not published. Microsoft only publishes the .NET 10 SDK image on Ubuntu noble.
**Impact:** All jobs now run on Ubuntu 24.04 (noble). Package names used by the .NET Runtime job's apt-get install step (cmake, clang, llvm, lld, lldb, libicu-dev, liblttng-ust-dev, libssl-dev, libkrb5-dev, libunwind8-dev, build-essential, pigz, cpio, python3) are identical on noble — no further changes required.
**Commit:** c2e0e46 on `jasper/squad`. New run: 25187497983.

### 2026-04-30: Bumped dotnet-coverage and forced bash shell on activate.sh steps
**By:** Vogel
**What:** `DOTNET_COVERAGE_VERSION` 17.12.0 → 18.6.2 (17.12.0 not on NuGet). Added `shell: bash` to "Restore SDK & dependencies (./restore.sh + activate.sh)" steps in aspnetcore and efcore jobs. Default container shell on `mcr.microsoft.com/dotnet/sdk:10.0-noble` is `sh`, which lacks `source`. Runtime job already had `shell: bash` on shopt/mapfile steps — confirmed still present.
**Why:** Run 25187497983 failed with `source: not found, exit code 127` (efcore/aspnetcore) and NU1102 for dotnet-coverage 17.12.0 (abp/roslyn/runtime). Triggered run 25188011117.

### 2026-04-30: ASP.NET Core build skips Java SignalR client
**By:** Jasper (via Watney)
**What:** aspnetcore job uses `./eng/build.sh --all --no-build-java --no-test` instead of `--all --no-test`.
**Why:** Run 25188011117 failed at `Java.Common.targets` invoking `gradlew compileJava` — the SDK container `mcr.microsoft.com/dotnet/sdk:10.0-noble` has no JDK. The build.sh `--no-build-java` flag sets `-p:BuildJava=false`, skipping the SignalR Java client cleanly. Preferred over installing JDK in the container (least invasive, fastest, no apt-get round-trip) and over modifying the cloned repo (per directive: keep cloned repos untouched).

### 2026-04-30: ASP.NET Core needs Node.js for JS-dependent projects
**By:** Watney
**What:** Install Node.js 22 LTS via NodeSource in the aspnetcore coverage job (Option D). New "Install Node.js" step in `.github/workflows/coverage-orchestrator.yml` aspnetcore job, between "Restore SDK & dependencies" and "Build ASP.NET Core".
**Why:** `eng/build.sh --all` builds JS-dependent projects like `Microsoft.AspNetCore.Components.WebView`, which reference `Web.JS/dist/Debug/blazor.webview.js`. `build.sh` auto-enables `-build-nodejs` when `node` is on PATH; when absent, it sets `-p:BuildNodeJSUnlessSourcebuild=false` and managed projects fall back to nonexistent prebuilt JS, causing "blazor.webview.js does not exist". `package.json` requires `node >=20.9.0`. No `.nvmrc` exists. Per directive: no edits to cloned repos — install Node in the container before the build step.
**Alternatives considered:** (B) Drop `--all` and target specific projects → fragile; project set varies across release branches. (C) Build only test binaries → dotnet test still triggers JS deps transitively. (A/D merged) Adding Node is the most "honest" path — uses the team's full pipeline as designed.

### 2026-04-30: EF Core uses external dotnet-coverage, not coverlet.collector
**By:** Beck (requested by Jasper)
**What:** Switched the `efcore` job in `.github/workflows/coverage-orchestrator.yml` from `dotnet test --collect:"XPlat Code Coverage"` to `dotnet-coverage collect "<test cmd>" -f cobertura -o ...` (same pattern as abp/roslyn/runtime).
**Why:** Run 25188011117 produced hundreds of `Could not find data collector 'XPlat Code Coverage'` errors. EF Core's test projects do NOT reference `coverlet.collector`, contrary to the prior assumption recorded in decisions.md. Tests ran for ~10 min but no cobertura XML was emitted.
**Impact:** Updated header comment classification: efcore now in the dotnet-coverage group (abp, efcore, roslyn, runtime). Coverlet group is now (aspnetcore, orleans, semantic-kernel). Report/upload paths changed from glob `target/TestResults/*/coverage.cobertura.xml` to single file `target/TestResults/coverage.cobertura.xml`. `activate.sh`-set DOTNET_ROOT is preserved via the existing `Restore SDK & dependencies` step which persists it to GITHUB_ENV/GITHUB_PATH.

### 2026-04-30: Phase 2 plan — targeted coverage analysis
**By:** Jasper (via Squad coordinator)
**What:** Once all 7 coverage jobs are green, the team will:
1. Pull cobertura artifacts from each run
2. Run `StaticCallAnalyzer/` against each repo to identify classes containing static method calls
3. Cross-reference: extract per-class coverage % (from cobertura) for ONLY the classes flagged by the analyzer
4. Produce a metrics summary: static-call class count + coverage of those specific classes per repo
5. Phase 3 (later): generate tests targeting those flagged classes
**Why:** Foundation for the experiment's core question — can we improve coverage of code that exercises static method calls (the hard-to-test surface).

### 2026-05-01: Unit-only test filters across all coverage jobs
**By:** Beck (requested by Jasper)
**What:** Applied an aggressive shared exclusion bundle to every coverage-orchestrator.yml job that uses `dotnet test --filter`:

```
FullyQualifiedName!~FunctionalTests
FullyQualifiedName!~IntegrationTests
FullyQualifiedName!~E2E
FullyQualifiedName!~EndToEnd
FullyQualifiedName!~Stress
Category!=Integration
Category!=IntegrationTest
Category!=IntegrationTests
Category!=E2E
Category!=EndToEnd
Category!=Functional
Category!=FunctionalTests
Category!=Stress
Category!=Performance
Category!=Quarantined
Category!=Flaky
```

Per-repo additions retained:
- abp: `&FullyQualifiedName!~SkiaSharp`
- roslyn: `&FullyQualifiedName!~LanguageServer&TargetFrameworkIdentifier!=.NETFramework`
- orleans: `&Category!=BVT&Category!=SlowBVT&Category!=LoadShedding&Category!=CorePerf` (Orleans-specific slow-suite traits)
- semantic-kernel: `&FullyQualifiedName!~ConformanceTests` (their cross-store conformance suites are integration-shaped)

**semantic-kernel filter swap:** the previous inclusive filter `FullyQualifiedName~UnitTests|FullyQualifiedName~.Tests.` is replaced with an exclusion-style filter for consistency with the other jobs.

**Timeout reductions:** aspnetcore 180→90, efcore 180→60. Others unchanged.

**runtime job left alone:** uses `./build.sh -subset libs+libs.tests -test`, not `dotnet test --filter`. Runtime already passed within its existing timeout in run 25197704510. Filter injection through build.sh is non-trivial (would need `XUnitMethodFilter`/`XUnitClassFilter` MSBuild props) and risky to change while it's green. Revisit only if a future run regresses.

**Why:** Run 25197704510 hit the 180-min job timeout on aspnetcore because their existing `Category!=Integration&Category!=E2E&Category!=Quarantined` filter doesn't exclude FunctionalTests projects (Kestrel `*.FunctionalTests`, Middleware `*.FunctionalTests`, etc.) which are the bulk of slow tests. Jasper's directive: unit-only across the board, no integration/e2e/functional/perf/stress.

### 2026-05-01: Always run all 7 jobs (no per-repo dispatch as default)
**By:** Jasper (via Squad coordinator)
**What:** Default workflow dispatch is `repo=all`. The 7 coverage jobs run in parallel, so per-repo dispatch saves no wall time and costs us drift detection, shared-infra signals, comparable timing, and a unified artifact bundle.
**When per-repo IS appropriate:** Active debugging of a single broken repo where the other 6 are stable AND we want a faster log iteration. Treat per-repo as a temporary diagnostic tool, not a normal mode.
**Implication:** Coordinator and agents should always default to `gh workflow run coverage-orchestrator.yml --ref <branch> -f repo=all` unless explicitly told otherwise.

### 2026-05-01: Phase 1 baseline shipped — 4 of 7 repos produced empty cobertura
**By:** Beck
**What:** Generated `BASELINE_COVERAGE.md` + `baseline_coverage.csv` + per-repo `baseline_artifacts/<repo>/static_call_classes.json` from CI run 25215078473 (jasper/squad, HEAD 188cb4a9). Aggregator script committed as `aggregate_baseline.py` (commit 835dcf2, not pushed).

**Headline finding:** Four of seven repos (`abp`, `aspnetcore`, `efcore`, `roslyn`) uploaded 178-byte stub cobertura files containing `<coverage line-rate="1" ...><packages /></coverage>`. CI jobs reported success but no assemblies were instrumented. Likely causes:
- `aspnetcore` uses `coverlet.collector` natively but the test projects under `--all` don't reference the collector package — coverlet silently does nothing.
- `abp`/`efcore`/`roslyn` use external `dotnet-coverage collect` — wrapped `dotnet test` likely matches no test projects under the unit-only filter, or `dotnet-coverage` is writing to a different path than the upload glob.

**Usable Phase 2 starting points:** `orleans` (6.08% lines, 5.43% branches), `runtime` (10.18% / 12.46%), `semantic-kernel` (12.12% / 9.78%).

**Phase 2 prereqs filed in BASELINE_COVERAGE.md "Next steps" — not blocking for this commit:**
1. Extend `StaticCallAnalyzer` to emit fully-qualified class names (currently simple `Identifier.Text` only) — owner Watney.
2. Build per-class coverage extractor that joins `static_call_classes.json` against cobertura `<class>` entries — owner Beck.
3. Diagnose the 4-repo empty-cobertura issue — owner Vogel/Beck.
4. Multi-file repo totals (orleans 49 files, semantic-kernel 43 files) are summed and may double-count code shared across test sessions — fine for class-level joins later, not de-duplicated at the totals row.
5. The 5-pattern `StaticCallConfig` may need extension (e.g. `Path.Combine`, `Environment.*`) before Phase 2 — would re-baseline static-call counts.

**Operational note for the team:** `StaticCallAnalyzer.Program.Main` appends to `./analysis_results.json` in its CWD on every run. Always invoke from a clean temp directory (the aggregator does this) or the file accumulates across runs and corrupts results.

### 2026-05-01: Empty cobertura on abp/aspnetcore/efcore/roslyn — root cause was profiler-not-attached, not the filter
**By:** Vogel
**What:** Pulled `Run tests with coverage` log for ABP from run 25215078473 (job 73933589003). Tests ran fine (hundreds passed across `Volo.Abp.AI.Tests`, `Volo.Abp.MongoDB.Tests`, `Volo.Abp.EntityFrameworkCore.Tests`, etc.). Immediately before the upload step, dotnet-coverage logged:

> `No code coverage data available. Profiler was not initialized. Verify that glibc (>=2.27), libxml2 and all .NET dependencies are installed.`
> `Code coverage results: TestResults/coverage.cobertura.xml.`

So `dotnet-coverage collect "dotnet test ..."` exited successfully but never instrumented the spawned testhost processes — it wrote the 178-byte stub `<coverage line-rate="1"><packages /></coverage>`. This is the well-known CLR profiler-attach failure inside the `mcr.microsoft.com/dotnet/sdk:10.0-noble` container (the wrapped-process model relies on `CORECLR_ENABLE_PROFILING` injection that does not propagate cleanly through `dotnet test → testhost`).

The filter is fine. `Category!=` works on tests with no Category trait (vstest treats missing trait as not-equal). The `No test matches` lines in the log are only for assemblies that legitimately contain no tests (e.g. `AbpTestBase.dll`); other DLLs reported `Passed!`.

**Fix shipped (commit pending push):** Switched all 4 affected jobs (abp, aspnetcore, efcore, roslyn) from
```
dotnet-coverage collect "dotnet test ... --filter ..." -f cobertura -o ...
```
to the in-process data-collector path:
```
dotnet test ... --filter ... --collect "Code Coverage;Format=cobertura" --results-directory TestResults
dotnet-coverage merge -r -f cobertura -o TestResults/coverage.cobertura.xml "TestResults/*.cobertura.xml"
```
The `--collect "Code Coverage;Format=cobertura"` data collector runs in-process via vstest (same engine as dotnet-coverage but invoked correctly from inside testhost), produces per-test-project XMLs under `TestResults/<guid>/coverage.cobertura.xml`, and we merge them with `dotnet-coverage merge`. Confirmed working in a local SDK-container smoke test before applying.

Runtime job left untouched (it uses `./build.sh -test` and Beck's baseline already showed 10.18% line coverage from it). Orleans / semantic-kernel unchanged (already use `coverlet.collector`).

**Why:** Get real Phase 1 baseline for the four largest repos so Phase 2 class-level coverage joins are not blocked.

**Next step (Jasper):** Push `jasper/squad`, dispatch coverage workflow with default `repo=all`, then re-run `aggregate_baseline.py` once green.

### 2026-05-06: Composite actions for coverage orchestrator
**By:** Vogel (via Copilot, requested by jastone)
**What:** Factored 350+ lines of duplicated step bodies across 7 jobs into 5 composite actions under .github/actions/. Workflow shrunk from 1102 lines to 727.
**Why:** Maintainability — fixes to the validator/uploader/cache previously required 5–7 simultaneous edits. Composite actions centralize the canonical implementation.
**Scope:** No behavior changes. Roslyn and ASP.NET Core failures from run 25458463158 are unaddressed and will be tackled in follow-up commits.

### 2026-05-06: EF Core coverage runs per-assembly with FunctionalTests DLLs excluded
**By:** Vogel (CI/CD)
**What:** Replaced the EF Core job's single `dotnet test EFCore.sln` invocation with an explicit `dotnet build EFCore.sln -c Debug --no-restore` step followed by a bash loop that discovers test DLLs under `target/artifacts/bin/**/Debug/` and runs `dotnet test <dll> --no-build` per assembly. The find filter excludes `*FunctionalTests*`, `*IntegrationTests*`, `*E2E*`, `*EndToEnd*`, `*Stress*`, `*Performance*`, `*Benchmarks*`, and `*Specification.Tests*` DLLs. Each per-assembly invocation is wrapped in `|| echo "::warning::..."` so one assembly's failure (or runner pressure) does not abort the loop.
**Why:** Run 25450510625 reproduced run 25218204007's failure — exit code 137 (SIGKILL/OOM) while loading `EFCore.SqlServer.FunctionalTests`. `--filter` only excludes test selection inside a DLL; VSTest still loads every assembly's testhost, and the FunctionalTests DLLs need external infra (SQL Server) that doesn't exist in the SDK container. Loading them was the OOM trigger. Per-assembly execution with FunctionalTests DLLs never loaded means each non-functional assembly persists its `.cobertura.xml` to disk before the next runs, so partial coverage survives even if a later assembly crashes the runner.
**Notes:** No partial-upload safety-net step was added — `if: always()` does not run when the runner host is killed, so the only real protection is preventing the OOM in the first place. `continue-on-error: true`, `timeout-minutes: 360`, and all SHA pins preserved. Other jobs untouched.

### 2026-05-06: Coverage workflow hardened against silent empty-cobertura failures
**By:** Vogel (CI/CD)
**Requested by:** jastone
**Context:** Run 25451789359 reported 7/7 jobs green but Roslyn, EF Core, and ASP.NET Core uploaded 289-byte empty cobertura stubs. Three independent root causes; common failure mode was that `dotnet test ... || true` masked the silent absence of `Attachments:`.

**What changed in `.github/workflows/coverage-orchestrator.yml`:**
1. **Roslyn** — Added `Install pinned .NET SDK from global.json` step using canonical `dotnet-install.sh --jsonfile global.json` between NuGet cache restore and `dotnet-coverage` install. Container's bundled SDK (10.0.203) does not satisfy Roslyn's pin (10.0.100-rc.2.25502.107).
2. **EF Core** — Switched per-assembly loop from discovering built `*.Tests.dll` under `artifacts/bin/**/Debug/` to discovering `*.Tests.csproj` (and `.fsproj`/`.vbproj`) under `test/`. VSTest's "Code Coverage" data collector adapter only attaches against project/sln targets — invoking it against a built `.dll` runs tests but produces zero attachments. Exclusions are now path-based (matches nested project dirs).
3. **ASP.NET Core** — Replaced slnx-wide `dotnet test AspNetCore.slnx --collect "Code Coverage"` with the same per-csproj loop pattern as EF Core, scanning `src/**/*.Tests.csproj` with broad path exclusions (FunctionalTests/IntegrationTests/E2E/E2ETest/E2E.Tests/EndToEnd/Stress/Performance/Benchmarks/HelixTests/NonHelixTests). Build step (`./eng/build.sh --all --no-build-java --no-test`) preserved unchanged.
4. **Validation gate** — New `Validate cobertura has real data` step in abp/aspnetcore/efcore/orleans/roslyn/semantic-kernel jobs (NOT runtime). Runs `if: always()`, NOT continue-on-error, hard-fails the job if no cobertura is produced, the largest cobertura is < 5 KB, or class count is zero. Placed before "Generate HTML report" so a missing report can't mask the failure. Searches both `TestResults/coverage.cobertura.xml` (merged) and `TestResults/**/coverage.cobertura.xml` (XPlat per-run subdirs) so it works for both collector strategies.
5. **Forensics upload** — New `Upload raw TestResults tree (forensics)` step in all six jobs above; uploads the entire `TestResults/` directory as `coverage-raw-<repo>` (30-day retention). Lets us diagnose collector failures without re-running.

**Preserved unchanged:**
- All SHA pins (actions, target repos, container image)
- `timeout-minutes: 360`
- `continue-on-error: true` on test steps (validation gate has none — must fail loudly)
- `--no-build-java` flag on ASP.NET Core build (despite directive note suggesting drop — removing it would require JDK in container, which would break the build; keeping it preserves working build behavior)
- Runtime job left fully untouched (separate `./build.sh -test` pipeline)

**Commit & push:** Single commit on `jasper/squad`, pushed to origin. Workflow not triggered — coordinator will dispatch after review.

**Open questions / follow-ups:**
- If ASP.NET Core's per-csproj loop still produces empty cobertura (e.g., if test SDK chain in `src/**/*.Tests.csproj` doesn't include the data collector adapter), fall back to writing a `coverage.runsettings` with `<DataCollector friendlyName="XPlat Code Coverage">` and using `--collect:"XPlat Code Coverage" --settings`. Validation gate will catch this case unambiguously.
- Class-count threshold (≥ 1) is intentionally minimal; can tighten later per-repo if needed.

### 2026-05-06: Roslyn slnx→sln, ASP.NET Core via coverlet.console
**By:** Vogel (via Copilot, requested by jastone)
**What:** Roslyn — Roslyn.slnx doesn't exist on the pinned SHA; renamed to Roslyn.sln. ASP.NET Core — neither --collect "Code Coverage" nor `dotnet-coverage collect` produces real cobertura because test projects don't reference any data collector. Switched to coverlet.console (standalone tool, managed instrumentation via mono.cecil — independent of project package references).
**Why:** First fix was wrong assumption from looking at error message rather than `ls`. Second is the only path that doesn't require modifying cloned repo source.
**Coverlet.console version:** 6.0.2.
**Risks:** coverlet.console might miss some projects (e.g. .fsproj/.vbproj) — keeping continue-on-error so failures don't block green; baseline measures whatever DOES instrument cleanly. If <80% of projects produce real cobertura, we'll need to investigate per-project.

### 2026-05-06: Roslyn per-csproj + ASP.NET Core dotnet-coverage wrapper
**By:** Vogel (requested by jastone)
**What:** Two final fixes to coverage-orchestrator.yml after run 25455447205 validation gate caught remaining empties.

- **Roslyn:** `dotnet test Roslyn.slnx` rejected by rc.2 SDK with MSB1009. Added explicit `dotnet restore Roslyn.slnx` + `dotnet build Roslyn.slnx --no-restore -c Debug` build step (these accept slnx); replaced test step with per-csproj loop matching EF Core shape, iterating `*.UnitTests.csproj` and `*.Tests.csproj` under `src/` with the exclusion list (LanguageServer, Specification.Tests, FunctionalTests, etc.) and the existing Roslyn FILTER. Inline `--collect "Code Coverage;Format=cobertura"` retained because Roslyn's test project asset paths ship the adapter.
- **ASP.NET Core:** Per-csproj loop ran 100+ projects with zero `Attachments:` because ASP test projects don't ship the inline data collector adapter. Switched to external instrumentation: each `dotnet test` invocation is now wrapped with `dotnet-coverage collect --output ... --output-format cobertura --session-id ... -- dotnet test ...`. Per-csproj sequential testhosts is a much simpler attach scenario than the slnx-wide concurrent testhosts that failed for us before. Each project gets a unique numbered output filename to prevent overwrites.

**Why:** Run 25455447205 validation gate working as designed — caught both remaining failures unambiguously. Two precise, narrowly-scoped fixes preserve all SHA pins, validation gate, raw uploads, and continue-on-error semantics.

### 2026-05-07: Always run all 7 coverage jobs together — never single-repo
**By:** Jasper (via Copilot)
**What:** When triggering `coverage-orchestrator.yml`, always use `repo=all`. Never trigger a single repo (e.g. `repo=aspnetcore`) to validate a fix.
**Why:** All 7 jobs run in parallel on independent runners — single-repo triggers do NOT save wall-clock time. They DO produce a partial result set, which means the next aggregation step is missing data and we have to re-run everything anyway. Net effect: single-repo runs waste a full cycle.
**Applies to:** All future workflow_dispatch invocations of coverage-orchestrator.yml.

### 2026-05-07: Containerize StaticCallAnalyzer (eliminate host .NET 8 dependency)
**By:** Vogel
**What:** Containerized `StaticCallAnalyzer` via `StaticCallAnalyzer/Dockerfile` (multi-stage `mcr.microsoft.com/dotnet/sdk:8.0` → `runtime:8.0`) and a `StaticCallAnalyzer/run.sh` wrapper. `aggregate_baseline.py` now invokes the wrapper, with a `docker`-on-PATH precheck replacing the prior `ANALYZER_DLL.exists()` check. Mount convention: target source at `/src` (read-only); image tag `static-call-analyzer:local`.
**Why:** Reproducibility. Any collaborator can now run `python3 aggregate_baseline.py` with only `python3 + gh + docker` on the host — no .NET 8 SDK install. Removed Jasper's blocker (snap-installed dotnet without the .NET 8 runtime).
**Side fixes:** Conditional headline + Phase-2 gap (driven by `Lines (total) < 100`), CI URL derived from `git remote get-url origin` and cached, multi-run support via `RUN_IDS` list, `Branch HEAD` from `git rev-parse HEAD`. Reproducing section updated to drop the analyzer build step.

### 2026-05-07: Test discovery diagnostic — built and pushed
**By:** Beck (requested by Jasper)
**What:** Added `.github/workflows/test-discovery.yml` (6 parallel jobs: abp/aspnetcore/efcore/orleans/roslyn/semantic-kernel) reusing the coverage orchestrator's build phases but replacing test execution with `dotnet test --no-build --list-tests` per project (with FILTER and without). Shared `.github/scripts/list_tests.sh` provides `run_discovery` + `_count_listed_tests`. Per-repo CSV artifacts `test-discovery-<repo>.csv` (columns: repo, project, tests_universe, tests_in_filter, tests_excluded, build_status). Local aggregator `tools/test_discovery/aggregate.py` → `TEST_DISCOVERY.md` + `test_discovery_summary.csv`.
**Why:** Capture filter-vs-universe deltas in CI to distinguish "filter is too aggressive" from "tests are genuinely shallow." Independent of coverage orchestrator (separate workflow/dispatch/artifacts). runtime intentionally skipped (uses `build.sh -subset libs+libs.tests -test`, not enumerable per-csproj).
**Status:** Pushed to `jasper/squad`. Awaiting Jasper review then `repo=all` dispatch.

### 2026-05-07: Coverage orchestrator expanded from 7 to 14 repos
**By:** Vogel (CI/CD), requested by Jasper via Coordinator
**What:** `.github/workflows/coverage-orchestrator.yml` now runs 14 parallel jobs. Added: Avalonia, duplicati, eShop, garnet, jellyfin, maui, server (bitwarden). Commit `0318b56` on `jasper/phase2`.
**Selection:** Mode #1 footprint × Linux-buildability. The 7 new repos cover ~6,800 of 6,879 detected Mode #1 sites (99%+) on top of original 7. Top contributors: jellyfin (1,206), garnet (745), maui (329).
**Pinned SHAs (resolved 2026-05-07 from local depth-1 clones):** Avalonia `dcb60717`, duplicati `4afa374d`, eShop `9b4f9434`, garnet `38cac99c`, jellyfin `e1e18e8d`, maui `b71adea6`, server `ea233580` (bitwarden/server, NOT microsoft/sqltoolsservice).
**Coverage strategy:** Coverlet group (XPlat) — jellyfin, maui (UnitTests subset), server, duplicati. External data-collector group — Avalonia, eShop, garnet.
**Risks called out:** MTP + `--collect "Code Coverage;Format=cobertura"` untested combo (eShop, Avalonia) — fall back to coverlet.console if empty; MAUI workload install ~10min / ~2GB; Bitwarden's SDK 8.0 install path is new.

### 2026-05-07: Coverage orchestrator round-1 fixes (run 25527102157)
**By:** Vogel (CI/CD)
**Context:** Run 25527102157 ran 14 jobs. Original 7 succeeded, 2 of 7 new succeeded (jellyfin, duplicati), 5 failed. Surgical fixes to 4; Garnet was transient.
**Fixes:**
1. **Garnet** — TRANSIENT, no fix. MCR rate-limit on docker pull; needs only re-run.
2. **Avalonia** — `dotnet restore Avalonia.slnx` pulled workload-bound projects (Android, Browser/iOS samples → NETSDK1147). Dropped slnx-level Restore+Build; added per-csproj loop using `find tests -name "*.UnitTests.csproj"`.
3. **Server (Bitwarden)** — NU1902 MailKit moderate audit promoted to error (TreatWarningsAsErrors=true + audit). Added `-p:NuGetAudit=false -p:WarningsNotAsErrors=NU1902` to Restore + Build.
4. **MAUI** — `dotnet-install.sh --jsonfile global.json` aborted ("Unable to find SDK:version node"). MAUI's global.json declares only workload manifests. Removed the install step; container's preinstalled 10.0.203 is sufficient.
5. **eShop** — `dotnet restore eShop.slnx` pulled `tests/ClientApp.UnitTests` (MAUI client → maui-tizen workload). Dropped slnx-level restore; per-csproj for Ordering.UnitTests + Basket.UnitTests only.
**Cross-cutting lesson:** `dotnet restore <whole.sln>` is unsafe when solutions mix server/desktop with mobile/wasm/MAUI projects — workload manifests are read at restore for the whole graph, not lazily.

### 2026-05-07: Coverage orchestrator round-2 fixes (4 of 4 remaining)
**By:** Vogel (CI/CD)
**What:** Surgical edits to the 4 jobs still failing after round-1 (commit 05b60b4):
- **Avalonia & eShop** — switched to coverlet.console pattern (mirrors aspnetcore). Both drive tests through Microsoft.Testing.Platform; under MTP the legacy `--collect "Code Coverage;Format=cobertura"` is silently ignored (tests pass, exit 0, 169-byte stub cobertura). Install `coverlet.console 6.0.2` global tool, find built test DLL under `<proj>/bin/Debug/<tfm>/`, invoke `coverlet "$asm" --target dotnet --targetargs "test ..."` per project. mono.cecil instrumentation is runner-agnostic.
- **Server (Bitwarden) — Option A (per-csproj test build).** RustSdk PreBuild target invokes `cargo build --release`; SDK container has no Rust. Local-clone grep showed RustSdk is referenced by exactly one csproj: `util/Seeder/Seeder.csproj` (not a test project). Replaced sln-level restore/build/test with per-csproj loop over `test/*.Test.csproj` excluding `*.IntegrationTest.csproj`, `Common.csproj`, `IntegrationTestCommon.csproj`. Sidesteps RustSdk graph entirely without installing Rust (~3min savings) and without modifying the cloned repo.
- **MAUI** — switched workload from `maui` to `maui-android`. Umbrella `maui` declares iOS+Mac SDKs unsupported on Linux ("Workload ID maui isn't supported on this platform"). `maui-android` is the Linux-supported subset; ships same MAUI cross-platform manifests (`$(_MauiDotNetTfm)` resolves once any maui-* workload is installed). Disk drops ~2GB → ~1.5GB.
**Patterns captured:**
- MTP + `--collect "Code Coverage;Format=cobertura"` is silent-no-op. Use coverlet.console when global.json sets MTP runner OR `MSTest.Sdk`/exe-style hosts are in play.
- `dotnet workload install maui` is Linux-incompatible. Use `maui-android`.
- Local-clone inspection beats workflow trial-and-error.
- NEVER `dotnet restore <whole.sln>` when the solution mixes test/server/utility projects.

### 2026-05-07: Re-include Orleans BVT category in coverage filter
**By:** Beck (Test & Coverage Engineer), requested by Jasper
**Context:** Test-discovery showed Orleans `ServiceBus.Tests` had 108 universe tests but only 14 passing FILTER (87% excluded). Orleans coverage was 6.07% — lowest of 7 repos. Traced to four Orleans-specific category exclusions added 2026-05-01: `Category!=BVT&Category!=SlowBVT&Category!=LoadShedding&Category!=CorePerf`.
**Verdicts after evidence review (Orleans pinned SHA `8024faf8`):** **BVT (54 occurrences across 159 files) — RE-INCLUDE** (classic unit tests; Orleans tradition: BVT = Build Verification = broad correctness). **SlowBVT (40 occurrences) — KEEP EXCLUDING** (heterogeneous-silo upgrade, integration-flavored). **LoadShedding (2) — KEEP EXCLUDING** (stress). **CorePerf (6) — KEEP EXCLUDING** (perf benchmarks).
**Cross-check:** Orleans defines `[TestCategory(string)]` in `test/TestInfrastructure/TestExtensions/TestCategory.cs` which emits `Category=<name>` xunit trait. So `Category!=BVT` filter does match `[TestCategory("BVT")]` — exclusion was working as intended, just over-broad.
**Project-glob gap:** Diagnostic's `find test -name "*.Tests.csproj"` missed `Orleans.Serialization.UnitTests` and `Orleans.Dashboard.UnitTests` (`.UnitTests.csproj` suffix). Coverage workflow uses `dotnet test Orleans.slnx` so those DID run during coverage — gap was only in per-project diagnostic CSV. Fixed by extending glob to `\( -name "*.Tests.csproj" -o -name "*.UnitTests.csproj" \)`.
**Files changed:** `.github/workflows/coverage-orchestrator.yml` (orleans `--filter`: removed `&Category!=BVT`); `.github/workflows/test-discovery.yml` (same FILTER + glob expansion).
**Expected impact:** Orleans coverage rises substantially from 6.07%; ServiceBus.Tests 14/108 → grows.

### 2026-05-07: Test counts derived from coverage workflow logs
**By:** Beck
**Context:** `tools/test_discovery/` (the `--list-tests` path) returns garbage for xunit.v3 repos (EF Core 0, Roslyn 3, ABP 46). The xunit.v3 VSTest adapter emits "No test is available" under `--list-tests` even when same projects execute thousands at runtime.
**Decision:** Authoritative per-project test counts now come from parsing the **Coverage Orchestrator workflow logs**. Every per-project `dotnet test` run emits `Passed!  - Failed: N, Passed: N, ... - Foo.dll (framework)` — unambiguous, not dependent on discovery API.
**New tool:** `tools/test_counts/from_coverage_logs.py` → `test_counts.csv` + `TEST_COUNTS.md`.
**Verified results (runs 25468601840 + 25472048463):** abp 74/1,358; aspnetcore 96/31,603; efcore 14/13,724; orleans 28/1,692; roslyn 33/155,993; semantic-kernel 44/6,263. EF Core's 14/13,724 matches hand-count exactly.
**Still missing:** runtime (uses `build.sh -subset libs+libs.tests -test`; summary lines not parseable shape — documented).
**Implication:** `tools/test_discovery/` still useful for non-xunit.v3 repos but NOT authoritative. `tools/test_counts/` is source of truth going forward.

### 2026-05-07: test-discovery counter rewritten for multi-shape adapter output
**By:** Vogel (CI/CD), requested by Jasper
**Context:** Run 25490696770 reported `tests_universe=0 status=ok` for ~200 of ~311 enumerated test projects across 6 repos. Coverage runs from same SHAs (run 25468601840) execute thousands — discovery numbers were obviously wrong, not a filter signal.
**Root cause:** `_count_listed_tests` only matched VSTest's MSBuild-logger shape (`"The following Tests are available:"` header + indented FQNs). `build_status=ok` was set whenever that header appeared, which the .NET 10 SDK driver emits even when the actual list is rendered differently (MTP `Test Name:` lines, xunit.v3 `Total tests: N` summaries) or stripped by MSBuild's default `minimal` verbosity. Header found → status=ok; awk saw zero indented lines → reported 0. Raw stdout never persisted, so failure was undebuggable from workflow logs.
**Fix:** Three surgical changes (architecture preserved):
1. Counter is now max-of-three heuristics: VSTest indented FQNs, `Test Name:` prefixed, `Total tests: N` summary.
2. Verbosity bumped to `-v normal` so MSBuild logger doesn't strip enumeration lines.
3. Per-project raw output persisted to `./_discovery_raw/`, uploaded as `test-discovery-<repo>-raw` (7-day retention).
**Did NOT touch:** Orleans test step / FILTER (Beck's domain), coverage orchestrator, `tools/test_discovery/aggregate.py` (CSV schema unchanged).
**Gotcha:** `mcr.microsoft.com/dotnet/sdk:10.0-noble` runs **mawk**, not gawk. The gawk-only 3-arg `match($0, /re/, arr)` form silently degrades on mawk and captures are empty. Use grep+sed for regex-with-captures inside this container.

### 2026-05-07T13:00Z: Phase 1 baseline refreshed against run 25495265941
**By:** Watney (requested by Jasper)
**What:** `BASELINE_COVERAGE.md`, `baseline_coverage.csv`, and per-repo `baseline_artifacts/<repo>/static_call_classes.json` regenerated from CI run 25495265941 (commit `99c79c9`, all 7 jobs green, includes Orleans BVT fix). `aggregate_baseline.py` `RUN_IDS` constant updated to `["25495265941"]`.
**Why:** Prior baseline (runs 25468601840 + 25472048463) had a known issue in Orleans coverage collection. New run reflects corrected pipeline: Orleans line coverage rose from 6.07% to **9.98%**. Other repos within rounding: abp 41.92%, aspnetcore 60.63%, efcore 27.06%, roslyn 76.21%, runtime 10.18%, semantic-kernel 12.12%. **TOTAL: 40.46% lines / 17.79% branches across 11.4M lines / 2.4M branches.** No headline warning — every repo emitted real cobertura.
**Authoritative source run for Phase 1 going forward:** `25495265941`. Earlier run IDs are superseded.

### 2026-05-07T13:00Z: Authoritative test-counts source advanced to run 25495265941
**By:** Beck (requested by Jasper)
**What:** `TEST_COUNTS.md` and `test_counts.csv` regenerated from the all-green run 25495265941 (commit 99c79c9, includes Orleans BVT fix). Previous source (runs 25468601840 + 25472048463) is superseded. Pushed as commit 349981e on `jasper/squad`.
**Headline deltas vs previous publication:** abp 74/1,358 → 73/1,346 (-1/-12); aspnetcore unchanged (96/31,603); efcore unchanged (14/13,724); **orleans 28/1,692 → 36/10,951 (+8/+9,259, BVT inclusion)**; roslyn unchanged (33/155,993); semantic-kernel unchanged (44/6,263); runtime n/a (coverlet log shape).

### 2026-05-08: MAUI removed from coverage matrix
**By:** Vogel (CI/CD), authorized by Brady
**What:** Deleted `coverage-maui` job from `.github/workflows/coverage-orchestrator.yml`. Removed `maui` from `repo` input choices. README Phase 2 row updated to ❌ Removed (was deferred).
**Why:** After 4 rounds of remediation (runs 25527102157, 25531117370, 25532666443, 25549163113), each fix surfaced another layer of MS-internal-CI assumption — custom global.json shape, `maui-android` workload subset, in-tree `Microsoft.Maui.BuildTasks` MSBuild prerequisite, then `net10.0-android36.0` TFMs in `Resizetizer/Core/Controls.Core` requiring the Android SDK on top of the workload. Carrying these workarounds was net-negative against MAUI's 329 Mode #1 sites.

### 2026-05-08: Phase 2 baseline additions — OpenRA + StockSharp
**By:** Vogel (CI/CD), requested by Brady
**What:** Added 2 jobs to `.github/workflows/coverage-orchestrator.yml`:
- `OpenRA` pinned at `8f2138c71106aaffd8a288b56d1c9ef6013700ba` (bleed HEAD, OpenRA/OpenRA)
- `StockSharp` pinned at `a26ce597cab73fe07949e0cc3845216342d33484` (master HEAD, StockSharp/StockSharp)
Both use the external `dotnet-coverage` data-collector path (`--collect "Code Coverage;Format=cobertura"` + `dotnet-coverage merge`) — neither references `coverlet.collector`. OpenRA additionally installs .NET 8 SDK alongside the noble container's .NET 10 (its tree pins `net8.0` in `Directory.Build.props`); StockSharp resolves to `net10.0` via `common_target_*.props` so the container SDK is sufficient.
Active matrix: **15 repos** (was 14 with MAUI deferred; -1 MAUI, +2 new = 15). Triggered runs: OpenRA=25552129165, StockSharp=25552132370. Commit d3689e0 pushed to `jasper/phase2`.

### 2026-05-08: Skipped Phase 2 candidates — PowerToys, Files
**By:** Vogel (CI/CD)
**What:** Did NOT add `microsoft/PowerToys` or `files-community/Files` to the workflow. README Phase 2 rows updated with concrete blockers.
**Why:**
- **Files** — `Directory.Build.props` mandates `WindowsTargetFramework=net10.0-windows10.0.26100.0`; `tests/Files.App.UITests/` carries `Package.appxmanifest` and `MainWindow.xaml`. Pure UWP/WinUI 3 stack, Windows-only TFMs throughout.
- **PowerToys** — All `*UnitTests*.csproj` discovered live under `src/modules/<windows-only-module>/` (colorPicker, fancyzones, registrypreview, AdvancedPaste, launcher plugins, previewpane handlers) and pull WinUI 3/WPF references transitively. None buildable in noble Linux container.

A coverage job that can't run isn't useful baseline data. Excluded.
