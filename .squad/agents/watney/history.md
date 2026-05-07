# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Reproducible builds for 7 .NET OSS repos.

## Core Context

Build/infra agent. `.devcontainer/` exists. Each repo in `cloned_repos/` has its own SDK requirements.

## Learnings

- aspnetcore needs `git submodule update --init --recursive` and `source ./activate.sh`.
- aspnetcore tests need coverlet.collector added to ~137 test projects.
- efcore uses local SDK via `activate.sh` (10.0.102).
- Containerized build pattern: `docker run --rm -v "$(pwd)/cloned_repos/<repo>:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:10.0 bash -c "..."`.

## Recent Updates

- 2026-04-30 — Workflow audit findings: 6/7 jobs set `dotnet-version: 9.0.x` while their `global.json` requires 10.0.x (aspnetcore 10.0.101, efcore 10.0.102, orleans 10.0.102, roslyn 10.0.100-rc.2, sk 10.0.100). Runtime job has NO `setup-dotnet` step. EF Core sources `activate.sh` but doesn't export `DOTNET_ROOT` to `$GITHUB_ENV` like aspnetcore does — fragile. coverlet.collector is never added to test projects (README requires it for aspnetcore's 137 test projects).

- 2026-05-07 — Team update from Vogel: `StaticCallAnalyzer` is now containerized (multi-stage Dockerfile, SDK 8.0 → runtime 8.0). Use `StaticCallAnalyzer/run.sh` wrapper; `aggregate_baseline.py` invokes it automatically. Eliminates host .NET 8 SDK dependency for the analyzer toolchain. Commit 3d53670 on `jasper/squad`.

## Recent Updates

### 2026-05-07 — Phase 1 baseline refresh
Re-ran `aggregate_baseline.py` against CI run 25495265941 (commit 99c79c9, all 7 jobs green, post-Orleans-BVT-fix). Updated `RUN_IDS` constant to `["25495265941"]`, refreshed `baseline_artifacts/` from the new run's coverage XML, regenerated `BASELINE_COVERAGE.md`, `baseline_coverage.csv`, and per-repo `static_call_classes.json`. Orleans line coverage 6.07% → **9.98%**; all other repos held: roslyn 76.21%, aspnetcore 60.63%, abp 41.92%, efcore 27.06%, sk 12.12%, runtime 10.18%. No headline warning (every repo emitted real cobertura). TOTAL 40.46% lines / 17.79% branches.
