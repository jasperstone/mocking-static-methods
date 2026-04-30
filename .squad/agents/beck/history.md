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
