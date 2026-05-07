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
