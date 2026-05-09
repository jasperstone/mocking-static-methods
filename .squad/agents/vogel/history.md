# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper (Brady is also active 2026-05-08+)
- **Created:** 2026-04-30
- **Goal:** Parallel coverage workflow across 7 .NET OSS repos on GitHub-hosted runners (expanded to 15 in Phase 2).
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Parallel coverage workflow across 7 .NET OSS repos on GitHub-hosted runners.

## Core Context

CI/CD agent. `.github/` directory exists in repo. User wants parallelization via Actions.

## Learnings

- Pin commit SHAs in workflow inputs to lock the experiment.
- Use `actions/checkout@<sha>` with `ref: <commit-sha>` and `submodules: recursive` for aspnetcore.
- Cache `~/.nuget/packages` keyed on `**/packages.lock.json` or `global.json`.
- Default workflow dispatch is `repo=all` (parallel jobs, no time savings from per-repo, lose drift/timing/infra signal).
- VSTest "Code Coverage" data collector adapter only attaches against project/sln targets — never built `.dll`. Always iterate `*.Tests.csproj`.
- Container-bundled SDKs may not satisfy `global.json` pins. Install pinned SDK via `dotnet-install.sh --jsonfile global.json`. **Conditional**, not universal — MAUI's global.json declares only workload manifests (no `sdk.version`), which makes `--jsonfile` exit nonzero. Always check shape before applying.
- `|| true` on test steps + no validation = silent green. Always include the cobertura validation gate (file present, ≥ 5 KB, ≥ 1 `<class>` element) BEFORE Generate HTML report.
- Always upload raw `TestResults/` for forensics (`coverage-raw-<repo>` artifact).
- `.squad/decisions/inbox/` is gitignored — drop files there for Scribe.
- MTP + `--collect "Code Coverage;Format=cobertura"` is silent-no-op. When global.json sets MTP runner OR `MSTest.Sdk`/exe-style hosts are in play, reach for coverlet.console (mono.cecil, runner-agnostic). Canonical form: `coverlet "$asm" --target dotnet --targetargs "test <proj>"`.
- `dotnet workload install maui` is Linux-incompatible (umbrella manifest declares iOS+Mac SDKs). Use `maui-android` for cross-platform MAUI tests on Linux.
- NEVER `dotnet restore <whole.sln>` when the solution mixes test/server/utility/mobile/wasm projects. Workload manifests evaluate the WHOLE graph at restore time, not lazily. Per-csproj restore of just the test projects is the safe default.
- Local-clone inspection (`grep -l ... cloned_repos/<repo>/**/*.csproj`) beats CI iteration for scope questions.
- `mcr.microsoft.com/dotnet/sdk:10.0-noble` runs **mawk**, not gawk. The 3-arg `match($0, /re/, arr)` form silently degrades. Use grep+sed for regex-with-captures inside this container.
- StaticCallAnalyzer is containerized (`StaticCallAnalyzer/Dockerfile` + `run.sh`) — `aggregate_baseline.py` invokes the wrapper. Host needs only python3 + gh + docker.
- Pin-by-API pattern: for repos NOT in `cloned_repos/`, use `gh api /repos/<owner>/<name>/commits/<branch> --jq .sha` to get HEAD without cloning.
- For test counts in xunit.v3 repos, `--list-tests` is broken (returns 0/3/46). Authoritative source is `tools/test_counts/from_coverage_logs.py` parsing `Passed!  - Failed: N, Passed: N, ... - Foo.dll` summary lines from coverage workflow logs.

## Recent Updates

(Entries before 2026-05-08 archived to `history-archive.md`. One-line index below.)

- **2026-05-06 commit 7885485** — Silent empty-cobertura fix: VSTest data collector against csproj only; container SDK vs global.json pin; validation gate; raw forensics upload.
- **2026-05-07** — Containerized StaticCallAnalyzer; `aggregate_baseline.py` host-deps reduced to python3 + gh + docker.
- **2026-05-07** — Test discovery counter rewritten for multi-shape adapter output (max-of-three heuristics; `-v normal`; raw artifact upload). mawk-vs-gawk gotcha.
- **2026-05-07 commit 0318b56** — Orchestrator expanded 7→14 repos (Avalonia, duplicati, eShop, garnet, jellyfin, maui, server). Per-repo build patterns + risks documented.
- **2026-05-07 commit 05b60b4 (round-1)** — 4 of 5 failures fixed from run 25527102157: Avalonia + eShop per-csproj restore (workload-bound graph), MAUI dotnet-install removed, Server NuGetAudit=false. Garnet was transient (MCR rate-limit).
- **2026-05-07 (round-2)** — 4 of 4 remaining: Avalonia + eShop → coverlet.console (MTP silent-no-op); Server → per-csproj test build (sidesteps RustSdk in Seeder.csproj); MAUI → `maui-android` workload.

### 2026-05-08 — MAUI removed; OpenRA + StockSharp added (Phase 2 baseline) — commit d3689e0

Removed the deferred `coverage-maui` job entirely (workflow + choices + summary + README row). 4 rounds of remediation hit increasingly internal MS-CI assumptions; per Brady, the data didn't justify the drag.

Added 2 new jobs:

- **OpenRA** (SHA `8f2138c7`, bleed HEAD) — `net8.0`, NUnit 4 + NUnit3TestAdapter, no coverlet in `OpenRA.Test/OpenRA.Test.csproj`. Strategy: data-collector path (`--collect "Code Coverage;Format=cobertura"` + `dotnet-coverage merge`), same as abp/efcore/roslyn/runtime. Side-installs .NET 8 SDK via `dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet` because the noble container ships only .NET 10 and OpenRA has no `global.json` to pin against. Per-csproj restore+build+test of the single test project — no slnx-level invocation.

- **StockSharp** (SHA `a26ce597`, master HEAD) — `net10.0` (resolved via `common_target_tests.props` → `common_target_net.props` → `NetVer=10` in `common_versions.props`), MSTest 4.x, no coverlet. Strategy: same data-collector path. Per-csproj restore+build of `Tests/Tests.csproj` only (the `StockSharp_Tests.slnx` pulls 22 projects; targeted restore is safer). No `NuGet.config` so default nuget.org feed pulls Ecng.* packages. Risk: `Tests/Tests.csproj` references `Microsoft.Data.SqlClient` + `Ecng.Data.SqlServer` — individual test cases hitting SQL will fail in isolation (filterable by Category=Integration); discover/build unaffected.

**Skipped PowerToys + Files:** both Windows-only at the SDK/TFM level, not just framework. Files mandates `net10.0-windows10.0.26100.0` in root `Directory.Build.props`; PowerToys' UnitTest projects all hang off `src/modules/<windows-only>/` chains. Documented in README Phase 2 rows with concrete blockers.

**Validation:** YAML safe-load OK. Active matrix: **15 repos**. Triggered runs: OpenRA=25552129165, StockSharp=25552132370.

**Pin-by-API-not-clone pattern:** First time pinning SHAs for repos that don't exist in `cloned_repos/`. `gh api /repos/<owner>/<name>/commits/<default_branch> --jq .sha` is the right tool — generalizes cleanly to any future Phase N additions.

### 2026-05-08 — StockSharp coverlet.console fix — commit d3c765d

Run 25552132370 confirmed the flagged risk: 178-byte cobertura stub while 4239 of 4263 tests actually passed. **MSTest 4.x = MSTest.Sdk = routes through MTP**, even when the project doesn't reference coverlet.collector. The data-collector silent-no-op is not just an MTP-runner-explicit issue (Avalonia/eShop): it's also triggered by the SDK choice. Heuristic update: `MSTest.Sdk` in `<Project Sdk="MSTest.Sdk">` OR `MSTest 4.x` package version → assume MTP routing → use coverlet.console. Don't trust `--collect` for any MSTest-4.x-or-newer repo.

Swapped to canonical coverlet.console wrap (same shape as Avalonia/eShop). Single test project so no per-csproj loop — direct `coverlet "$asm" --target dotnet --targetargs "test Tests/Tests.csproj ..."`. Built assembly is `StockSharp.Tests.dll` (not `Tests.dll` — `RootNamespace` from `common_target_tests.props` overrides). New run: 25556051863. The 13 failing tests (Paths/Report/InMemory/AsyncExtensions/ConnectorBasket) are env-dependent assertions — coverlet doesn't care; instrumentation runs regardless.

### 2026-05-08 — StockSharp flaky-test filter (partial); coverlet+MTP empty-modules blocker — commits 4d07fc1, a89d57e, f82c22d

User asked for `FullyQualifiedName!~ClassName` exclusions targeting the timing/event-count flaky classes. Pulled the FQNs from run 25556051863 with `gh run view --log-failed | grep -oE "StockSharp\.Tests\.[A-Za-z_.]+"`. Five classes covered the failure set: `AsyncExtensionsTests`, `ConnectorBasketTests`, `PathsTests`, `ReportTests`, `TransactionIdStorageTests`.

**Filter worked perfectly** — run 25562087626 went from ~50 failures to **0 failures / 4096 passed / 11 skipped / 4107 total** in 4m26s. **But validate-cobertura still failed: 231-byte report, empty Module table.** Coverlet ran, tests ran under it, but it instrumented zero modules. Despite ~80 DLLs (StockSharp.*, Ecng.*) sitting in `Tests/bin/Release/net10.0/`, the report has no `<class>` elements.

**Round 2** (run 25562607958) added explicit `--include "[StockSharp*]*"` + `--include "[Ecng*]*"`. Self-inflicted bug: a diagnostic `ls -la | head -40` line caused **SIGPIPE → exit 141** under `bash -e -o pipefail`, killing the step before coverlet started. Reverted the `ls|head` line in `f82c22d`. The `--include` patterns are still in the workflow but unproven.

**Stopped at 2 attempts per task instruction.** Full diagnosis in `.squad/decisions/inbox/vogel-stocksharp-flaky-filter.md`.

**Learnings:**
- `FullyQualifiedName!~ClassName` exclusion: confirmed working syntax for coverlet's `--filter` (it pipes through to `dotnet test`'s `--filter`). `&` is the AND separator. Works for any MSTest class name suffix.
- Pulling failing class names: `gh run view <id> --log-failed 2>&1 | grep -oE "<Repo>\.Tests\.[A-Za-z_.]+" | sort -u` is the cleanest way — avoids OOM from full-log fetches and groups failures by class.
- **`bash -e -o pipefail` + `ls | head` = step death (exit 141 SIGPIPE)** in GitHub Actions. Never use `head`/`tail` on output you might want short for diagnostic logging — use `awk 'NR<=40'` or `2>&1 | sed -n '1,40p'` instead. Or just drop `pipefail` for that line.
- **Coverlet.console under MSTest.Sdk + MTP can pass tests but emit an empty Module table.** New failure mode separate from the 178-byte MTP-no-op shape. Tests run successfully against the instrumented test assembly, but dependency assemblies aren't instrumented despite being present and loaded. Likely cause: MTP child-process probe path differs from coverlet's pre-instrumented path. `--include-test-assembly` alone is insufficient; explicit `--include "[<asm>*]*"` patterns may be needed (untested due to attempt cap). Future fix paths: explicit include patterns, direct MTP exec via `dotnet exec $asm --treenode-filter`, or `dotnet-coverage collect` (in-proc, MTP-aware).
- Filter exclusions and instrumentation success are **orthogonal problems**. Solving test failures does not guarantee `<class>` elements appear. Always verify the cobertura Module table, not just the test pass count.

- 2026-05-08: R6 pattern (XPlat Code Coverage runsettings + coverlet.collector injection for StockSharp).

## Recent Updates

- 2026-04-30 — Workflow audit findings: All actions tag-ref'd (`@v4`), need SHA pins. `ubuntu-latest` should pin to `ubuntu-24.04`. ZERO caching for NuGet/tools. No `timeout-minutes` on any job (default 6h is loose; aspnetcore/runtime should be 120, others 60-90). No `concurrency:` block — duplicate pushes spawn parallel runs. No `permissions:` block (default-permissive). 7 hand-rolled jobs duplicate same 4 setup steps — candidate for composite action `.github/actions/setup-coverage/`.
- 2026-05-01 — Default workflow dispatch is repo=all (parallel jobs, no time savings from per-repo, but lose drift/timing/infra signal).

### 2026-05-06 — Silent empty-cobertura fix (commit 7885485)
- **VSTest "Code Coverage" data collector adapter only attaches against project/sln targets.** Invoking `dotnet test <foo.dll> --collect "Code Coverage"` runs the tests but silently produces zero `Attachments:`. Always iterate `*.Tests.csproj` (not built .dll) for the per-assembly loop pattern.
- **Container-bundled SDKs may not satisfy `global.json` pins.** Roslyn pins `10.0.100-rc.2.25502.107`; the `mcr.microsoft.com/dotnet/sdk:10.0-noble` image carries `10.0.203`. Install pinned SDK via `dotnet-install.sh --jsonfile global.json --install-dir $GITHUB_WORKSPACE/.dotnet` for repos that lack their own restore.sh.
- **`|| true` on test steps + no validation = silent green.** The "real coverage" gate (file present, ≥ 5 KB, ≥ 1 `<class>` element) catches all three failure modes from run 25451789359 unambiguously. Place it BEFORE Generate HTML report; HTML can succeed against an empty stub and mask the real problem.
- **Always upload raw `TestResults/` for forensics.** `coverage-raw-<repo>` artifact lets us diagnose collector failures (missing attachments, profiler errors) post-hoc without re-running a 6-hour build.
- **`.squad/decisions/inbox/` is gitignored in this repo.** Drop files there for Scribe; they don't get committed.

## Recent Updates

### 2026-05-07 — Containerized StaticCallAnalyzer

**Problem:** `aggregate_baseline.py` invoked `dotnet StaticCallAnalyzer.dll` directly. Jasper's system `dotnet` (snap install) lacks .NET 8 runtime, breaking the local aggregation step. Same hit any collaborator without .NET 8 SDK.

**What I shipped:**
- `StaticCallAnalyzer/Dockerfile` — multi-stage SDK 8.0 build → runtime 8.0; ENTRYPOINT `dotnet /app/StaticCallAnalyzer.dll`, default CMD `/src`.
- `StaticCallAnalyzer/run.sh` — wrapper that auto-builds image `static-call-analyzer:local` if missing, then `docker run --rm -v <abs>:/src:ro …`. Build output redirected to stderr so stdout stays JSON-clean.
- `StaticCallAnalyzer/.dockerignore` — keep `bin/` and `obj/` out of the build context.

**aggregate_baseline.py fixes:**
- `run_static_analyzer()` now invokes the bash wrapper (no `dotnet` on host required).
- `main()` precheck swapped: `shutil.which("docker")` + wrapper existence; removed ANALYZER_DLL check.
- Path stripping in `aggregate_static()` now handles `/src/` mount prefix (with legacy host-path fallback).
- Headline + gap #3 are now CONDITIONAL — derived from `rows_for_md` (`Lines (total)` < 100). All 7 real this run → green headline, gap omitted.
- CI URL: derived from `git remote get-url origin`, cached. `RUN_ID` → `RUN_IDS` list, one link per run.
- Branch HEAD: `git rev-parse HEAD` at report time.
- Reproducing section updated: docker is the only host requirement besides python3 + gh.

**Gotcha:** Docker emits file paths as `/src/<...>` instead of host paths, so `aggregate_static`'s prefix-strip logic needed both prefixes. Verified all 7 repos report static-call counts > 0 (abp 126, aspnetcore 155, efcore 39, orleans 91, roslyn 117, runtime 613, semantic-kernel 38).

### 2026-05-07 — Test discovery: adapter + parser fragility

**Symptom (Run 25490696770):** test-discovery workflow reported `universe=0
filter=0 status=ok` for nearly every project. EF Core: 0/17 had any tests.
Roslyn: 1/49 (3 tests). ABP: 1/78 (46 tests). The actual coverage runs from
the same SHAs execute thousands of tests in those repos — so discovery is
silently under-reporting, not the filter.

**Root cause (best-supported by evidence, not yet bottom-confirmed):**
The legacy `_count_listed_tests` in `list_tests.sh` only matched indented
FQN lines under the VSTest header `"The following Tests are available:"`.
The `status=ok` flag is set whenever that header appears in stdout, which
is true for many shapes that do *not* follow the "indented per-line FQN"
format the awk expected (e.g., MTP / xunit.v3 enumerators, MSBuild minimal
verbosity stripping nested logger output, multi-TFM projects where one TFM
errors on `net481` under Linux). Header found → status=ok; counter sees
zero indented lines → reports 0. The script captured raw output silently,
so the actual emitted format was un-debuggable from the workflow logs.

**What I shipped:**
- `.github/scripts/list_tests.sh` — three counting heuristics (max wins):
  (a) indented FQNs under VSTest header (legacy);
  (b) `Test Name:` prefixed lines (vstest direct mode / MTP);
  (c) `Total tests: N` summary value (xunit.v3 / MTP discovery summary).
  Added `_discovery_ran` predicate that recognises empty-discovery markers
  ("No test is available", "Found 0 tests") so they classify as
  `status=ok, count=0` instead of `<error>`.
- `dotnet test` calls now include `-v normal`, so the MSBuild logger
  doesn't drop adapter-emitted enumeration lines on the default `minimal`.
- Per-project raw stdout/stderr saved to `./_discovery_raw/` (relative to
  each repo's working directory). New artifact `test-discovery-<repo>-raw`
  uploaded with 7-day retention so the next run gives us actual evidence.
- `_count_total_summary` rewritten with grep+sed (mawk-portable; the SDK
  container is Ubuntu Noble with mawk, which lacks gawk's 3-arg `match()`).

**Did NOT touch:** Orleans test step or its FILTER (Beck's territory), the
coverage orchestrator workflow, or the aggregate.py CSV schema (columns
unchanged).

**Verified:** YAML parses; bash syntax-checks; all four counter shapes pass
on synthetic inputs locally (3, 3, 42, 0).

**Not verified:** Does the new counter actually produce non-zero universe
values for EF Core / Roslyn? That requires Jasper to dispatch the workflow
with `repo=all` and inspect the new raw-log artifact. If the new run still
reports 0s, the raw logs will tell us the *actual* output shape and we
can add a fourth heuristic.

**Gotcha for future agents:**
- `mcr.microsoft.com/dotnet/sdk:10.0-noble` ships mawk, not gawk. Do NOT
  use `match($0, /re/, arr)` (gawk-only 3-arg form) in scripts that run
  inside this container — use grep+sed or `match() == 0` style.
- The `status=ok` heuristic in this script only proves "discovery emitted
  *some* recognised marker", not "discovery succeeded". If you see
  `status=ok` with `universe=0`, check the raw-log artifact before
  blaming the filter.
