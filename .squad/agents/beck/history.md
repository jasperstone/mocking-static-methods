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
