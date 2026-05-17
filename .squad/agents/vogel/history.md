# Vogel — CI/CD (history)

## Project Context

- **Project:** mocking-static-methods
- **User:** Jasper (Brady also active 2026-05-08+)
- **Created:** 2026-04-30
- **Goal:** Parallel coverage workflow across 7 .NET OSS repos on GitHub-hosted runners (expanded to 15 in Phase 2).

## Core Context

CI/CD agent. Owns `.github/workflows/` (coverage-orchestrator, test-discovery), `.github/scripts/list_tests.sh`, `.github/actions/` (composite actions), StaticCallAnalyzer containerization.

## Learnings (durable)

### Workflow hygiene
- Pin commit SHAs in workflow inputs to lock the experiment.
- Use `actions/checkout@<sha>` with `ref: <commit-sha>` and `submodules: recursive` for aspnetcore.
- Cache `~/.nuget/packages` keyed on `**/packages.lock.json` or `global.json`.
- Default workflow dispatch is `repo=all` — parallel jobs, no time savings per-repo, but you lose drift/timing/infra signal.
- Always upload raw `TestResults/` for forensics (`coverage-raw-<repo>` artifact).
- `.squad/decisions/inbox/` is gitignored — drop files there for Scribe.

### Coverage collection
- **VSTest "Code Coverage" data collector adapter only attaches against project/sln targets** — never built `.dll`. Always iterate `*.Tests.csproj`.
- Container-bundled SDKs may not satisfy `global.json` pins. Install pinned SDK via `dotnet-install.sh --jsonfile global.json --install-dir $GITHUB_WORKSPACE/.dotnet` for repos lacking their own restore.sh. **Conditional** — MAUI's global.json declares only workload manifests (no `sdk.version`), `--jsonfile` exits nonzero. Check shape first.
- `|| true` on test steps + no validation = silent green. Always include the cobertura validation gate (file present, ≥ 5 KB, ≥ 1 `<class>` element) BEFORE Generate HTML report. HTML succeeds against empty stub and masks problems.
- MTP + `--collect "Code Coverage;Format=cobertura"` is silent-no-op. When global.json sets MTP runner OR `MSTest.Sdk`/exe-style hosts are in play, reach for coverlet.console (mono.cecil, runner-agnostic). Canonical: `coverlet "$asm" --target dotnet --targetargs "test <proj>"`.
- **MSTest 4.x = MSTest.Sdk = MTP routing**, even when project doesn't reference coverlet.collector. Don't trust `--collect` for any MSTest-4.x-or-newer repo. Heuristic: `<Project Sdk="MSTest.Sdk">` OR MSTest 4.x package version → use coverlet.console.
- **Coverlet.console under MSTest.Sdk + MTP can pass tests but emit empty Module table** (separate failure mode from 178-byte MTP-no-op). Tests run on instrumented test assembly, but dependency assemblies aren't instrumented despite being loaded. Likely cause: MTP child-process probe path differs from coverlet's pre-instrumented path. Possible fixes: explicit `--include "[<asm>*]*"` patterns (untested); direct MTP exec via `coverlet "$asm" --target dotnet --targetargs "exec $asm --treenode-filter ..."`; `dotnet-coverage collect` (in-proc, MTP-aware).
- Filter exclusions and instrumentation success are **orthogonal**. Solving test failures does NOT guarantee `<class>` elements appear. Always verify cobertura Module table, not just pass count.

### Build/restore
- NEVER `dotnet restore <whole.sln>` when solution mixes test/server/utility/mobile/wasm projects. Workload manifests evaluate WHOLE graph at restore time, not lazily. Per-csproj restore of just test projects is safe default.
- `dotnet workload install maui` is Linux-incompatible (umbrella manifest declares iOS+Mac SDKs). Use `maui-android` for cross-platform MAUI tests on Linux.
- Local-clone inspection (`grep -l ... cloned_repos/<repo>/**/*.csproj`) beats CI iteration for scope questions.

### Pinning without clones
- For repos NOT in `cloned_repos/`, use `gh api /repos/<owner>/<name>/commits/<branch> --jq .sha` to get HEAD without cloning. Generalizes to any future Phase N additions.

### Test discovery
- `--list-tests` is broken for xunit.v3 repos (returns 0/3/46). Authoritative source for test counts is `tools/test_counts/from_coverage_logs.py` parsing `Passed!  - Failed: N, Passed: N, ... - Foo.dll` summary lines.
- The current `_count_listed_tests` in `list_tests.sh` uses 3 heuristics (max wins): indented FQNs under VSTest header; `Test Name:` prefixed lines; `Total tests: N` summary value. Plus `_discovery_ran` predicate to classify empty-discovery as `status=ok, count=0`.
- Per-project raw stdout/stderr saved to `./_discovery_raw/`, uploaded as `test-discovery-<repo>-raw` (7-day retention) for next-round debugging.

### Container/shell gotchas
- `mcr.microsoft.com/dotnet/sdk:10.0-noble` runs **mawk**, not gawk. 3-arg `match($0, /re/, arr)` silently degrades — empty captures. Use grep+sed for regex-with-captures inside this container.
- **`bash -e -o pipefail` + `ls | head` = step death (exit 141 SIGPIPE)** in GitHub Actions. Never use `head`/`tail` on piped output. Use `awk 'NR<=40'` or `2>&1 | sed -n '1,40p'` instead. Or drop pipefail for that line.

### Coverlet filter syntax
- `FullyQualifiedName!~ClassName` works as exclusion in coverlet's `--filter` (pipes through to dotnet test `--filter`). `&` is the AND separator. Pulling failing class names: `gh run view <id> --log-failed 2>&1 | grep -oE "<Repo>\.Tests\.[A-Za-z_.]+" | sort -u`.

### StaticCallAnalyzer container
- Containerized (`StaticCallAnalyzer/Dockerfile` + `run.sh`). `aggregate_baseline.py` invokes wrapper. Host needs only python3 + gh + docker — no .NET 8 SDK install required.
- Docker emits file paths as `/src/<...>` instead of host paths — `aggregate_static` prefix-strip handles both prefixes.

## Recent Updates

### 2026-05-16T00:00:00Z — Team update (viz layout)
viz layout changed — see `tools/viz/README.md` and `.squad/decisions.md` (entry: 2026-05-16: tools/viz restructure). Per-plot files under `tools/viz/plots/`, shared helpers in `tools/viz/lib/`, new derived `tools/viz/data/per_model_phase.csv` from `aggregate_phase_results.py`. Four new plot families shipped.

### 2026-05-08 — MAUI removed; OpenRA + StockSharp added (Phase 2 baseline) — commit d3689e0
Removed deferred `coverage-maui` job entirely. 4 rounds of remediation hit increasingly internal MS-CI assumptions; per Brady, data didn't justify drag.

Added 2 new jobs:
- **OpenRA** (`8f2138c7`, bleed HEAD) — `net8.0`, NUnit 4 + NUnit3TestAdapter, no coverlet. Data-collector path (`--collect "Code Coverage;Format=cobertura"` + `dotnet-coverage merge`), same as abp/efcore/roslyn/runtime. Side-installs .NET 8 SDK because noble ships only .NET 10 and OpenRA has no `global.json`.
- **StockSharp** (`a26ce597`, master HEAD) — `net10.0` (via `common_target_*.props` → `NetVer=10`), MSTest 4.x, no coverlet. Per-csproj restore+build of `Tests/Tests.csproj` only. Risk flagged: references `Microsoft.Data.SqlClient` + `Ecng.Data.SqlServer` (SQL-dependent tests filterable by Category=Integration).

**Skipped PowerToys + Files** — both Windows-only at SDK/TFM level. Files mandates `net10.0-windows10.0.26100.0`; PowerToys UnitTests all in `src/modules/<windows-only>/` chains.

Active matrix: **15 repos**. Triggered runs: OpenRA=25552129165, StockSharp=25552132370.

### 2026-05-08 — StockSharp coverlet.console fix — commit d3c765d
Run 25552132370: 178-byte cobertura stub while 4239/4263 tests passed. MSTest 4.x SDK choice triggers MTP routing → data-collector silent-no-op. Swapped to canonical coverlet.console wrap. Built assembly is `StockSharp.Tests.dll` (RootNamespace from `common_target_tests.props` overrides). New run: 25556051863.

### 2026-05-08 — StockSharp flaky-filter (partial); coverlet+MTP empty-modules blocker — commits 4d07fc1, a89d57e, f82c22d
Added `FullyQualifiedName!~` exclusions for 5 flaky classes (`AsyncExtensionsTests`, `ConnectorBasketTests`, `PathsTests`, `ReportTests`, `TransactionIdStorageTests`). **Filter worked**: run 25562087626 went 0 failures / 4096 passed / 11 skipped. **But cobertura 231 bytes with empty Module table** — coverlet ran but instrumented zero modules despite ~80 dependency DLLs in `Tests/bin/Release/net10.0/`. Round 2 (run 25562607958) added `--include "[StockSharp*]*"` + `--include "[Ecng*]*"`. Self-inflicted SIGPIPE bug from `ls | head` killed step before coverlet started — reverted diag in `f82c22d`. `--include` patterns remain, unproven. Stopped at 2 attempts. Full diagnosis: decision `2026-05-08: StockSharp flaky-test filter — partial fix`.

### Earlier entries
- 2026-05-06 — Silent empty-cobertura fix (commit 7885485)
- 2026-05-07 — Containerized StaticCallAnalyzer
- 2026-05-07 — Test-discovery counter rewritten for multi-shape adapter output
- 2026-05-07 — Coverage orchestrator expanded 7→14 repos (Avalonia, duplicati, eShop, garnet, jellyfin, maui, server) + round-1/round-2 fix passes

Full text of all entries above is in `history-archive.md`.
