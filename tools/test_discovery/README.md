# Test Discovery

Diagnostic for: *"is the CI `--filter` excluding too many tests, or are the
included tests just not exercising production code?"*

## How it runs

1. CI workflow `.github/workflows/test-discovery.yml` is dispatched with
   `repo=all`. Six per-repo jobs build the target repo at the same pinned
   SHA the coverage workflow uses, then for each test project run
   `dotnet test --no-build --list-tests` twice — once with the same
   `--filter` the coverage workflow uses, once without.
2. Each job uploads `test-discovery-<repo>.csv`:
   `repo,project,tests_universe,tests_in_filter,tests_excluded,build_status`.
3. Locally, download all six artifacts and aggregate:

   ```bash
   gh run download <run-id> -p 'test-discovery-*' -D test_discovery_artifacts
   python3 tools/test_discovery/aggregate.py
   # → docs/TEST_DISCOVERY.md + test_discovery_summary.csv
   ```

## What's not in here

- **runtime** — uses `build.sh -subset libs+libs.tests -test`, not enumerable
  per-csproj. Diagnostic skipped on purpose; reverse-engineering the subset
  driver isn't worth it for a diagnostic.
- **No local Docker tool** — the cloned repos under `cloned_repos/` aren't
  built locally (multi-GB), and `--no-build --list-tests` requires the
  output assemblies. CI is the only place a build exists, so the diagnostic
  lives there.

## Files

- `.github/workflows/test-discovery.yml` — the workflow
- `.github/scripts/list_tests.sh` — shared helper sourced by each job
- `tools/test_discovery/aggregate.py` — markdown + summary CSV generator
