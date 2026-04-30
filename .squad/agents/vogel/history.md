# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Parallel coverage workflow across 7 .NET OSS repos on GitHub-hosted runners.

## Core Context

CI/CD agent. `.github/` directory exists in repo. User wants parallelization via Actions.

## Learnings

- Pin commit SHAs in workflow inputs to lock the experiment.
- Use `actions/checkout@<sha>` with `ref: <commit-sha>` and `submodules: recursive` for aspnetcore.
- Cache `~/.nuget/packages` keyed on `**/packages.lock.json` or `global.json`.

## Recent Updates

- 2026-04-30 — Workflow audit findings: All actions tag-ref'd (`@v4`), need SHA pins. `ubuntu-latest` should pin to `ubuntu-24.04`. ZERO caching for NuGet/tools. No `timeout-minutes` on any job (default 6h is loose; aspnetcore/runtime should be 120, others 60-90). No `concurrency:` block — duplicate pushes spawn parallel runs. No `permissions:` block (default-permissive). 7 hand-rolled jobs duplicate same 4 setup steps — candidate for composite action `.github/actions/setup-coverage/`.
