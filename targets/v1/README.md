# Targets — v1

Canonical input set for test-generation phases.

## What this is

Each row in [`targets.csv`](targets.csv) is one Mode #1 static-call site that:
- lives in **production code** (not tests/samples/benchmarks/playgrounds), and
- is **NOT currently line-covered** by the existing test suite at the SHAs pinned in [`phases/phase1-baseline/phase.lock.yaml`](../../phases/phase1-baseline/phase.lock.yaml), and
- has a cobertura entry for both the file and the exact line (so we can verify whether a generated test newly covers it).

A "Mode #1 site" is a call to a static method or extension method that the source code can't substitute (no seam). They're the targets we want to learn how to test through later phases.

## Why uncovered-only

If a line is already covered by an existing test, generating a new test for it can't move the line-coverage number. This experiment's headline metric is *new coverage produced by each generation strategy*; including already-covered sites would dilute that signal. Branch coverage of already-covered lines is a separate experiment — see `targets/v2/` (future) for that scope.

For the analysis of how the 1,087 covered sites are *currently* covered (the five buckets: deterministic statics, in-process test servers, default/null config paths, wrapper-mocked, and integration tests), see [`covered_sites_analysis.csv`](covered_sites_analysis.csv) and the rationale section in the root [`README.md`](../../README.md#why-we-target-only-uncovered-sites).

## How to use this in a phase

```python
import csv
with open("targets/v1/targets.csv") as fh:
    for row in csv.DictReader(fh):
        # row["target_id"]     stable across phases (e.g. "abp:0042")
        # row["repo"]          which repo to clone
        # row["file"]          repo-relative path
        # row["line"]          line of the static call
        # row["containing_type"]  the .NET type holding the static method
        # row["method"]        the static method name
        # row["receiver_type"] the type the call appears on (extension target)
        # row["kind"]          "Static" or "Extension"
        attempt_to_generate_test_for(row)
```

Pin in the phase's `phase.lock.yaml`:

```yaml
inputs:
  targets_version: v1
  targets_sha256: aca60f388a4c82c8af021a76ea47a443838b7edb6a7992fe21f9fe3afe8d5e10
  targets_count: 3147
```

A phase that runs against a different `targets_sha256` than the file currently on disk MUST fail validation.

## Schema

| column | type | meaning |
|---|---|---|
| `target_id` | string | `{repo}:{4-digit}` — stable across phases |
| `repo` | string | repo key matching `phases/*/phase.lock.yaml` `repos:` and the orchestrator matrix |
| `file` | string | repo-relative path to the .cs file |
| `line` | int | line number of the static call site |
| `containing_type` | string | fully-qualified .NET type defining the static method |
| `method` | string | static method name |
| `receiver_type` | string | the type the call appears on (matters for extension methods) |
| `kind` | string | `Static` or `Extension` |

## How it was built

Run `python3 tools/targets/build_targets.py --version v1`. Inputs:
- [`Mode1Analyzer/results/mode1_sites.csv`](../../Mode1Analyzer/results/mode1_sites.csv) — all 6,879 Mode #1 sites detected by the Roslyn analyzer.
- `/tmp/cov_phase2/coverage-xml-{repo}/` — cobertura output collected during the phase 1 baseline coverage run.

Output sizes (see [`targets.lock.yaml`](targets.lock.yaml) for full provenance):

| Bucket | Count |
|---|---:|
| **In `targets.csv` (the input set)** | **3,147** |
| Excluded — already covered | 1,087 |
| Excluded — non-production path | 1,167 |
| Excluded — `unknown_file` (no cobertura entry) | 901 |
| Excluded — `unknown_line` (multi-line expression) | 19 |
| Total Mode #1 sites detected | 6,321 (deduped by row) |

## Targets per repo

See `counts.targets_by_repo` in [`targets.lock.yaml`](targets.lock.yaml). Highlights:
- jellyfin (1,046), garnet (499), semantic-kernel (458), abp (445), aspnetcore (403) — the bulk of the work.
- Avalonia (0), runtime (0), StockSharp (0) — all production Mode #1 sites in these repos are either already covered or sit in files cobertura didn't load. Reconsider for v2 once we expand coverage scope.

## Versioning

Bump to `v2/` when:
- adding/removing repos changes the population,
- expanding scope (e.g. branch-uncovered sites, or `unknown_file` sites once we resolve the missing-assembly issue), or
- the analyzer's site-detection logic changes materially.

Old phases stay reproducible against the version they pinned.
