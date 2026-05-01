# Phase 1 Coverage Baseline

**Date:** 2026-05-01  
**CI run:** [25215078473](https://github.com/bradygaster/mocking-static-methods/actions/runs/25215078473) (workflow: `coverage-orchestrator.yml`, branch `jasper/squad`)  
**Branch HEAD at run:** `188cb4a95c2162c6db6fdafe6aa3f04f485104aa`

This is the pre-Phase 2 snapshot of test coverage and static-call surface area for the seven .NET OSS repos under study. Coverage data is cobertura XML produced by each repo's CI job (`actions/upload-artifact` → `coverage-xml-<repo>`). Static-call counts come from `StaticCallAnalyzer/` run locally against the pinned source tree of each repo.

> ⚠️ **Headline finding:** four of seven repos (`abp`, `aspnetcore`, `efcore`, `roslyn`) uploaded 178-byte stub cobertura files with `<packages />` empty — the CI jobs reported success, but no instrumented assemblies were exercised. Only `orleans`, `runtime`, and `semantic-kernel` produced usable coverage data this run. The static-call analysis below is sound for all seven; the line/branch columns must be re-baselined for the four stub repos before Phase 2 can compare "before" and "after" coverage there.

## Pinned target SHAs

| Repo | SHA |
|------|-----|
| abp | `ea4bbb8b517869a9fb735ea5bc05c819c209d0b5` |
| aspnetcore | `ecb199c29cbefb6fcb6aa789436de36e44427a78` |
| efcore | `45e3af0273b71919189367bc152a335b69f443c6` |
| orleans | `8024faf860549cb960b4b573c1571b379e283daa` |
| roslyn | `02d301627ed5016a4c18acd1a35e5bbc20ff03f0` |
| runtime | `9ffface2f3fa6fbbb427793c3230b1626a1fdd84` |
| semantic-kernel | `0c898161a355b0a845aea48de79cb43e2e9435d2` |

## Baseline Table

| Repo | Lines (total) | Lines (covered) | Line coverage % | Branches (total) | Branches (covered) | Branch coverage % | Static call sites | Classes with static calls |
|------|---:|---:|---:|---:|---:|---:|---:|---:|
| abp | 0 | 0 | 0.00% | 0 | 0 | 0.00% | 126 | 61 |
| aspnetcore | 0 | 0 | 0.00% | 0 | 0 | 0.00% | 155 | 80 |
| efcore | 0 | 0 | 0.00% | 0 | 0 | 0.00% | 39 | 16 |
| orleans | 4,429,586 | 269,509 | 6.08% | 1,154,700 | 62,698 | 5.43% | 91 | 50 |
| roslyn | 0 | 0 | 0.00% | 0 | 0 | 0.00% | 117 | 68 |
| runtime | 230,646 | 23,477 | 10.18% | 92,141 | 11,479 | 12.46% | 613 | 245 |
| semantic-kernel | 611,211 | 74,073 | 12.12% | 218,717 | 21,385 | 9.78% | 38 | 25 |
| **TOTAL** | 5,271,443 | 367,059 | 6.96% | 1,465,558 | 95,562 | 6.52% | 1,179 | 545 |

Percentages on the TOTAL row are weighted by line/branch volume across all 7 repos.

## Methodology

- **Coverage:** parsed root `<coverage>` attributes (`lines-valid`, `lines-covered`, `branches-valid`, `branches-covered`) from each cobertura XML. For repos that emit one file per test session (multi-package coverlet runs), totals are summed across all files.
- **Static call sites:** sum of `PatternCount` across every row emitted by `StaticCallAnalyzer` (one row per `(file, class, method, pattern)` triple). The analyzer only counts calls inside methods with cyclomatic complexity > 2 and excludes paths matching `Tests`, `Samples`, or `Demo`. It tracks five patterns: `DateTime.Now`, `DateTime.UtcNow`, `File.Exists`, `Directory.Exists`, `Guid.NewGuid` (see `StaticCallAnalyzer/StaticCallConfig.cs`).
- **Classes with static calls:** distinct `(file, class)` pairs in the analyzer output.
- **Per-class breakdown:** see `baseline_artifacts/<repo>/static_call_classes.json` — list of `{class_name, class_fqn, file_path, static_call_count}` sorted by count descending.

## Data quality notes

- **abp**: cobertura XML present but `lines-valid=0` — the run produced empty coverage data (no instrumented assemblies were exercised).
- **abp**: no branch data emitted by the collector.
- **aspnetcore**: cobertura XML present but `lines-valid=0` — the run produced empty coverage data (no instrumented assemblies were exercised).
- **aspnetcore**: no branch data emitted by the collector.
- **efcore**: cobertura XML present but `lines-valid=0` — the run produced empty coverage data (no instrumented assemblies were exercised).
- **efcore**: no branch data emitted by the collector.
- **orleans**: coverage was emitted as 49 separate cobertura files (one per test project / coverlet session). Totals are summed across all files; code shared across multiple test sessions may be double-counted.
- **roslyn**: cobertura XML present but `lines-valid=0` — the run produced empty coverage data (no instrumented assemblies were exercised).
- **roslyn**: no branch data emitted by the collector.
- **semantic-kernel**: coverage was emitted as 43 separate cobertura files (one per test project / coverlet session). Totals are summed across all files; code shared across multiple test sessions may be double-counted.

## Phase 2 readiness — gaps to close

1. **StaticCallAnalyzer does NOT emit fully-qualified class names.** It records the simple `Identifier.Text` of the enclosing `ClassDeclarationSyntax` only. Phase 2 needs `Namespace.OuterClass.InnerClass` to join against cobertura's `<class name="...">` entries. The `class_fqn` field in `static_call_classes.json` is currently `null` for every entry. **Action:** extend `StaticCallAnalyzer/Program.cs` to walk `NamespaceDeclarationSyntax` / `FileScopedNamespaceDeclarationSyntax` and parent `ClassDeclarationSyntax` ancestors when assembling the FQN. Owner: Watney.
2. **Per-class coverage extraction not yet implemented.** Cobertura `<class>` entries hold `line-rate` / `branch-rate`. Phase 2 needs a step that, for each class in `static_call_classes.json`, looks up its coverage in the matching cobertura file and emits a joined record `{repo, class_fqn, file_path, line_rate, branch_rate, static_call_count}`. Owner: Beck (next session).
3. **Four repos (`abp`, `aspnetcore`, `efcore`, `roslyn`) produced empty cobertura XML.** Each uploaded a 178-byte stub `<coverage line-rate="1" ...><packages /></coverage>`. CI jobs reported success because tests passed and the report step had `continue-on-error: true`; the underlying issue is that no assemblies got instrumented. Likely causes by repo: (a) `abp`/`efcore`/`roslyn` use external `dotnet-coverage collect` — the wrapped `dotnet test` command may not be matching any test projects under the unit-only filter, or `dotnet-coverage` is writing to a different path than the one we upload; (b) `aspnetcore` uses `coverlet.collector` natively, but the test projects under `--all` likely don't reference the collector package — coverlet silently does nothing. **Action:** Vogel/Beck investigate per-repo before declaring any of these four a Phase 2 baseline. Until fixed, only `orleans` / `runtime` / `semantic-kernel` are usable Phase 2 starting points.
4. **Multi-file repos (orleans, semantic-kernel, ...) sum-double-count code shared between test sessions.** For Phase 2 class-level joins this isn't a problem — we'll merge per-class entries by FQN and take the union of covered lines. But the totals shown above are upper bounds, not de-duplicated unions.
5. **Analyzer pattern set is fixed at 5 patterns.** If Phase 2 wants broader coverage of static-method usage (e.g. `Path.Combine`, `Environment.*`, `Console.*`), `StaticCallConfig.Patterns` needs extending. This will inflate static-call counts and re-baseline values.

## Reproducing

```bash
# 1. Download artifacts from the run (90-day retention)
mkdir -p baseline_artifacts
for repo in abp aspnetcore efcore orleans roslyn runtime semantic-kernel; do
  gh run download 25215078473 -n coverage-xml-$repo -D baseline_artifacts/$repo/
done

# 2. Build the analyzer (one-time)
dotnet build StaticCallAnalyzer/StaticCallAnalyzer.csproj -c Release

# 3. Aggregate
python3 aggregate_baseline.py
```
