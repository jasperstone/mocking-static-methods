# Decision: Test counts derived from coverage workflow logs

**Date:** 2026-05-07
**Author:** Beck

## Context

`tools/test_discovery/` (the `--list-tests` path) returns garbage for xunit.v3
repos: EF Core 0, Roslyn 3, ABP 46. The xunit.v3 VSTest adapter emits
"No test is available" under `--list-tests` even when the same projects
execute thousands of tests at runtime. This blocked any per-project test
inventory for those repos.

## Decision

Authoritative per-project test counts now come from parsing the **Coverage
Orchestrator workflow logs** instead. Every per-project `dotnet test` run
emits a summary line (`Passed!  - Failed: N, Passed: N, ... - Foo.dll
(framework)`) which is unambiguous and not dependent on the discovery API.

New tool: `tools/test_counts/from_coverage_logs.py` → `test_counts.csv` +
`TEST_COUNTS.md`.

## Verified results (runs 25468601840 + 25472048463)

| repo            | projects | total tests |
|-----------------|---------:|------------:|
| abp             | 74       | 1,358       |
| aspnetcore      | 96       | 31,603      |
| efcore          | 14       | 13,724      |
| orleans         | 28       | 1,692       |
| roslyn          | 33       | 155,993     |
| semantic-kernel | 44       | 6,263       |

EF Core's 14 / 13,724 matches the hand-count exactly. Roslyn and ABP now
report real numbers instead of the broken 3 / 46.

## Repos still missing data

- **runtime** — uses `build.sh -subset libs+libs.tests -test`; per-project
  summary lines aren't emitted in a parseable shape. Documented in the
  tool README.

## Implications

- `tools/test_discovery/` is still useful for non-xunit.v3 repos
  (ASP.NET Core, Orleans, Semantic Kernel) where it agrees with these
  numbers, but it should NOT be treated as authoritative for any repo.
- `tools/test_counts/` is the source of truth going forward.
- No workflow changes; this is a pure post-hoc reader of existing logs.
