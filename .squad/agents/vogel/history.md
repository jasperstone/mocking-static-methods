# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper (Brady is also active 2026-05-08+)
- **Created:** 2026-04-30
- **Goal:** Parallel coverage workflow across 7 .NET OSS repos on GitHub-hosted runners (expanded to 15 in Phase 2).

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
