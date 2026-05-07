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
