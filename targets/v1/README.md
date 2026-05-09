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
- [`Mode1Analyzer/results/mode1_sites.csv`](../../Mode1Analyzer/results/mode1_sites.csv) — Mode #1 sites detected by the Roslyn analyzer.
- `/tmp/cov_phase2/coverage-xml-{repo}/` — cobertura output collected during the phase 1 baseline coverage run.

## Bucket partition (every detected site lands in exactly one bucket)

The analyzer detected **6,321 Mode #1 sites** across the 15 repos in the matrix. Each site falls into one — and only one — of these buckets:

| Bucket | Count | In `targets.csv`? | Why |
|---|---:|:---:|---|
| **uncovered, in cobertura** | **3,147** | ✅ **yes** | Production code, line is not currently covered, cobertura has the file/line. We can verify whether a generated test newly covers it. |
| already covered | 1,087 | no | Production code, line already hit by the existing suite. A new test here can't move line coverage. Catalogued in [`covered_sites_analysis.csv`](covered_sites_analysis.csv). |
| non-production path | 1,167 | no | Site lives under `test/`, `samples/`, `benchmarks/`, `playground/`. Out of scope by definition. |
| `unknown_file` | 901 | no — deferred to v2 | Production source exists but cobertura has no entry — the existing test suite's projects never reference the assembly. A generated test gives us no signal until we resolve the missing-assembly issue. |
| `unknown_line` | 19 | no | Cobertura has the file but not the exact line (multi-line expression cobertura collapsed). Data quality issue. |
| **TOTAL** | **6,321** | | `3,147 + 1,087 + 1,167 + 901 + 19` |

The full provenance (sha256, source SHAs, source CI run) is in [`targets.lock.yaml`](targets.lock.yaml).

## Per-repo bucket breakdown

| Repo | **target** | covered | non-prod | unknown_file | unknown_line | total |
|---|---:|---:|---:|---:|---:|---:|
| abp | **445** | 192 | 5 | 370 | 5 | 1,017 |
| aspnetcore | **403** | 314 | 13 | 203 | 3 | 936 |
| Avalonia | **0** | 0 | 9 | 0 | 0 | 9 |
| duplicati | **21** | 0 | 13 | 0 | 0 | 34 |
| efcore | **1** | 11 | 0 | 1 | 0 | 13 |
| eShop | **30** | 4 | 0 | 60 | 0 | 94 |
| garnet | **499** | 179 | 57 | 10 | 0 | 745 |
| jellyfin | **1,046** | 153 | 7 | 0 | 0 | 1,206 |
| OpenRA | **13** | 0 | 0 | 0 | 0 | 13 |
| orleans | **98** | 154 | 891 | 38 | 0 | 1,181 |
| roslyn | **6** | 4 | 4 | 100 | 0 | 114 |
| runtime | **0** | 1 | 0 | 32 | 0 | 33 |
| semantic-kernel | **458** | 31 | 159 | 83 | 11 | 742 |
| server | **127** | 44 | 9 | 1 | 0 | 181 |
| StockSharp | **0** | 0 | 0 | 3 | 0 | 3 |
| **TOTAL** | **3,147** | 1,087 | 1,167 | 901 | 19 | 6,321 |

Highlights:
- jellyfin (1,046), garnet (499), semantic-kernel (458), abp (445), aspnetcore (403) carry 79% of the targets.
- Avalonia, runtime, StockSharp contribute 0 targets. Their production Mode #1 sites are either already covered or sit in files cobertura didn't load (recoverable in v2).
- abp and aspnetcore alone account for 573 of the 901 `unknown_file` sites. Recovering those is the most valuable v2 expansion — see issue notes in v2 README when created.

## Versioning

Bump to `v2/` when:
- adding/removing repos changes the population,
- expanding scope (e.g. branch-uncovered sites, or `unknown_file` sites once we resolve the missing-assembly issue), or
- the analyzer's site-detection logic changes materially.

Old phases stay reproducible against the version they pinned.
