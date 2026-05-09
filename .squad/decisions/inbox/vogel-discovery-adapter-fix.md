# Decision: test-discovery counter rewritten for multi-shape adapter output

**Date:** 2026-05-07
**By:** Vogel (CI/CD)
**Requested by:** Jasper

## Context

Run 25490696770 of `test-discovery.yml` reported `tests_universe=0 status=ok`
for ~200 of ~311 enumerated test projects across 6 repos. The actual coverage
run from the same SHAs (run 25468601840) executes thousands of tests in EF
Core / Roslyn / ABP — so the discovery numbers were obviously wrong, not a
filter signal.

## Root cause (evidenced)

`_count_listed_tests` in `.github/scripts/list_tests.sh` only matched the
single output shape produced by VSTest's MSBuild logger
(`"The following Tests are available:"` header + indented FQNs). The
`build_status=ok` flag was set whenever that header appeared in stdout,
which the .NET 10 SDK's `dotnet test` driver appears to emit even when the
*actual* test list is rendered in a different format (MTP `Test Name:` lines,
xunit.v3 `Total tests: N` summaries) or stripped by MSBuild's default
`minimal` verbosity. Header found → status=ok; awk saw no indented lines →
reported 0. Raw stdout was captured into a shell variable but never
persisted, so the failure was undebuggable from the workflow logs alone.

## Decision

Keep the script + workflow architecture unchanged (still per-project
`dotnet test --no-build --list-tests`, still per-repo CSV artifact). Three
surgical changes:

1. **Counter is now max-of-three heuristics** — VSTest indented FQNs,
   `Test Name:` prefixed lines, and `Total tests: N` summary value.
   The largest count wins.
2. **Verbosity bumped to `-v normal`** so the MSBuild logger doesn't strip
   adapter-emitted enumeration lines.
3. **Per-project raw output is now persisted** under `./_discovery_raw/`
   and uploaded as a debug artifact (`test-discovery-<repo>-raw`, 7-day
   retention) — so the *next* run gives us evidence of the actual output
   shape if the new counter still misses something.

## Files touched

- `.github/scripts/list_tests.sh` (rewrite)
- `.github/workflows/test-discovery.yml` (added 6 raw-log upload steps)

Did NOT touch: Orleans test step / FILTER (Beck's domain), coverage
orchestrator workflow, `tools/test_discovery/aggregate.py` (CSV schema
unchanged so aggregator keeps working).

## Verification

- `python3 -c 'import yaml; yaml.safe_load(...)'` ✅
- `bash -n .github/scripts/list_tests.sh` ✅
- All four synthetic input shapes (VSTest, MTP, summary, empty-discovery)
  produce correct counts locally (3, 3, 42, 0).
- **End-to-end NOT yet verified** — needs a workflow dispatch with
  `repo=all`. If counts are still wrong, the raw-log artifact will tell us
  the exact format the adapter emits, and we add a fourth heuristic.

## Non-goals

- Not switching to `dotnet vstest <DLL> --ListTests`. That would need
  per-repo logic to locate built DLLs (artifact paths differ across abp /
  aspnetcore / efcore / orleans / roslyn / semantic-kernel) and is a much
  bigger surgery. We try the cheap fix first.

## Gotcha (for future agents)

`mcr.microsoft.com/dotnet/sdk:10.0-noble` runs **mawk**, not gawk. The
gawk-only 3-argument `match($0, /re/, arr)` form silently degrades to the
2-arg form on mawk and your captures will be empty. Use grep+sed for any
regex-with-captures inside this container, or pipe through gawk explicitly.
