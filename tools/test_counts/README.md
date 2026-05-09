# tools/test_counts

Authoritative per-project test counts extracted from Coverage Orchestrator
workflow logs.

## Why this exists

`tools/test_discovery/` runs `dotnet test --list-tests` against every test
project and counts the listed names. That works for xunit.v2 / NUnit / MSTest
but **fails for xunit.v3** — the v3 VSTest adapter currently emits
`No test is available in <dll>` under `--list-tests` even when the same
project executes thousands of tests when actually run. EF Core, Roslyn, and
parts of ABP are xunit.v3 today, so their `--list-tests` numbers came back
as 0/3/46 — useless.

The Coverage Orchestrator workflow runs `dotnet test` for real, and every
per-project run prints a summary line:

```
Passed!  - Failed:  0, Passed:  6622, Skipped:  0, Total:  6622, Duration: 1 m 2 s - Microsoft.EntityFrameworkCore.Tests.dll (net10.0)
```

These are authoritative. This script downloads the logs and parses them.

## Usage

```bash
# Use the most recent successful run on jasper/squad (default)
python3 tools/test_counts/from_coverage_logs.py

# One or more specific run IDs
python3 tools/test_counts/from_coverage_logs.py 25468601840
python3 tools/test_counts/from_coverage_logs.py 25468601840 25472048463
python3 tools/test_counts/from_coverage_logs.py --run-id 25468601840 --run-id 25472048463

# Custom output paths
python3 tools/test_counts/from_coverage_logs.py --csv out/counts.csv --md out/counts.md
```

Outputs:

- `test_counts.csv` — one row per `(repo, project, framework)` with columns
  `repo, project, dll, framework, total, passed, failed, skipped, status,
  source_run_id, source_job_id`
- `TEST_COUNTS.md` — per-repo aggregate plus top/bottom-10 tables and a
  "missing data" section listing repos whose log shape didn't yield counts.

## Merge semantics

When multiple run IDs are passed, the script takes the **most recent**
(`source_run_id` ordered as integers) row per `(repo, project, framework)`.
Within a single log, retried runs are handled by **last occurrence wins**.
The CSV's `source_run_id` column tells you exactly which run produced each
number.

## Repos with no data

The .NET Runtime job runs `build.sh -subset libs+libs.tests -test` instead
of per-csproj `dotnet test`, so the per-project `Passed!` summary line
isn't emitted in a parseable shape. Runtime appears in the
"Repos missing data" section. Coverlet-wrapped runs (ASP.NET Core, Orleans,
Semantic Kernel) **do** still emit the summary line — they parse fine.

## Dependencies

Pure Python 3 + `gh` CLI. No network or `requirements.txt` install needed.
Logs are cached in `/tmp/cov_<job_id>.log` so re-runs are cheap.
