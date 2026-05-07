# Phase 1 Coverage Baseline

**Date:** 2026-05-07  
**CI runs:** [25468601840](https://github.com/jasperstone/mocking-static-methods/actions/runs/25468601840), [25472048463](https://github.com/jasperstone/mocking-static-methods/actions/runs/25472048463) (workflow: `coverage-orchestrator.yml`, branch `jasper/squad`)  
**Branch HEAD at report time:** `7b45d1f611fe7d08d02e1bd335963335d10160a7`

This is the pre-Phase 2 snapshot of test coverage and static-call surface area for the seven .NET OSS repos under study. Coverage data is cobertura XML produced by each repo's CI job (`actions/upload-artifact` → `coverage-xml-<repo>`). Static-call counts come from `StaticCallAnalyzer/` run via Docker against the pinned source tree of each repo.

> ✅ **Headline:** all seven repos produced real coverage data this run.

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
| abp | 129,052 | 54,097 | 41.92% | 42,237 | 7,379 | 17.47% | 126 | 61 |
| aspnetcore | 574,127 | 348,084 | 60.63% | 104,425 | 43,322 | 41.49% | 155 | 80 |
| efcore | 978,082 | 264,657 | 27.06% | 158,116 | 47,513 | 30.05% | 39 | 16 |
| orleans | 4,429,586 | 268,810 | 6.07% | 1,154,700 | 62,523 | 5.41% | 91 | 50 |
| roslyn | 4,494,731 | 3,425,509 | 76.21% | 641,993 | 198,158 | 30.87% | 117 | 68 |
| runtime | 230,646 | 23,491 | 10.18% | 92,141 | 11,494 | 12.47% | 613 | 245 |
| semantic-kernel | 611,211 | 74,074 | 12.12% | 218,717 | 21,386 | 9.78% | 38 | 25 |
| **TOTAL** | 11,447,435 | 4,458,722 | 38.95% | 2,412,329 | 391,775 | 16.24% | 1,179 | 545 |

Percentages on the TOTAL row are weighted by line/branch volume across all 7 repos.

## Methodology

- **Coverage:** parsed root `<coverage>` attributes (`lines-valid`, `lines-covered`, `branches-valid`, `branches-covered`) from each cobertura XML. For repos that emit one file per test session (multi-package coverlet runs), totals are summed across all files.
- **Static call sites:** sum of `PatternCount` across every row emitted by `StaticCallAnalyzer` (one row per `(file, class, method, pattern)` triple). The analyzer only counts calls inside methods with cyclomatic complexity > 2 and excludes paths matching `Tests`, `Samples`, or `Demo`. It tracks five patterns: `DateTime.Now`, `DateTime.UtcNow`, `File.Exists`, `Directory.Exists`, `Guid.NewGuid` (see `StaticCallAnalyzer/StaticCallConfig.cs`).
- **Classes with static calls:** distinct `(file, class)` pairs in the analyzer output.
- **Per-class breakdown:** see `baseline_artifacts/<repo>/static_call_classes.json` — list of `{class_name, class_fqn, file_path, static_call_count}` sorted by count descending.

## Data quality notes

- **orleans**: coverage was emitted as 49 separate cobertura files (one per test project / coverlet session). Totals are summed across all files; code shared across multiple test sessions may be double-counted.
- **semantic-kernel**: coverage was emitted as 43 separate cobertura files (one per test project / coverlet session). Totals are summed across all files; code shared across multiple test sessions may be double-counted.

## Phase 2 readiness — gaps to close

1. **StaticCallAnalyzer does NOT emit fully-qualified class names.** It records the simple `Identifier.Text` of the enclosing `ClassDeclarationSyntax` only. Phase 2 needs `Namespace.OuterClass.InnerClass` to join against cobertura's `<class name="...">` entries. The `class_fqn` field in `static_call_classes.json` is currently `null` for every entry. **Action:** extend `StaticCallAnalyzer/Program.cs` to walk `NamespaceDeclarationSyntax` / `FileScopedNamespaceDeclarationSyntax` and parent `ClassDeclarationSyntax` ancestors when assembling the FQN. Owner: Watney.
2. **Per-class coverage extraction not yet implemented.** Cobertura `<class>` entries hold `line-rate` / `branch-rate`. Phase 2 needs a step that, for each class in `static_call_classes.json`, looks up its coverage in the matching cobertura file and emits a joined record `{repo, class_fqn, file_path, line_rate, branch_rate, static_call_count}`. Owner: Beck (next session).
3. **Multi-file repos (orleans, semantic-kernel, ...) sum-double-count code shared between test sessions.** For Phase 2 class-level joins this isn't a problem — we'll merge per-class entries by FQN and take the union of covered lines. But the totals shown above are upper bounds, not de-duplicated unions.
4. **Analyzer pattern set is fixed at 5 patterns.** If Phase 2 wants broader coverage of static-method usage (e.g. `Path.Combine`, `Environment.*`, `Console.*`), `StaticCallConfig.Patterns` needs extending. This will inflate static-call counts and re-baseline values.

## Reproducing

Host requirements: `python3`, `gh` (GitHub CLI, authenticated), and `docker`. No local .NET install needed — the analyzer is containerized.

```bash
# 1. Download artifacts from the run(s) (90-day retention)
mkdir -p baseline_artifacts
for repo in abp aspnetcore efcore orleans roslyn runtime semantic-kernel; do
  gh run download 25468601840 -n coverage-xml-$repo -D baseline_artifacts/$repo/ 2>/dev/null || true
  gh run download 25472048463 -n coverage-xml-$repo -D baseline_artifacts/$repo/ 2>/dev/null || true
done

# 2. (No analyzer build needed — Docker handles it on first run.)

# 3. Aggregate
python3 aggregate_baseline.py
```
