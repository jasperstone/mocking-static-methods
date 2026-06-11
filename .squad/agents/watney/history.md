# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Reproducible builds for 7 .NET OSS repos.

## Core Context

Build/infra agent. `.devcontainer/` exists. Each repo in `cloned_repos/` has its own SDK requirements.

## Learnings

- aspnetcore needs `git submodule update --init --recursive` and `source ./activate.sh`.
- aspnetcore tests need coverlet.collector added to ~137 test projects.
- efcore uses local SDK via `activate.sh` (10.0.102).
- Containerized build pattern: `docker run --rm -v "$(pwd)/cloned_repos/<repo>:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:10.0 bash -c "..."`.

## Recent Updates

- 2026-04-30 — Workflow audit findings: 6/7 jobs set `dotnet-version: 9.0.x` while their `global.json` requires 10.0.x (aspnetcore 10.0.101, efcore 10.0.102, orleans 10.0.102, roslyn 10.0.100-rc.2, sk 10.0.100). Runtime job has NO `setup-dotnet` step. EF Core sources `activate.sh` but doesn't export `DOTNET_ROOT` to `$GITHUB_ENV` like aspnetcore does — fragile. coverlet.collector is never added to test projects (README requires it for aspnetcore's 137 test projects).

- 2026-05-07 — Team update from Vogel: `StaticCallAnalyzer` is now containerized (multi-stage Dockerfile, SDK 8.0 → runtime 8.0). Use `StaticCallAnalyzer/run.sh` wrapper; `aggregate_baseline.py` invokes it automatically. Eliminates host .NET 8 SDK dependency for the analyzer toolchain. Commit 3d53670 on `jasper/squad`.

## Recent Updates

### 2026-05-07 — Phase 1 baseline refresh
Re-ran `aggregate_baseline.py` against CI run 25495265941 (commit 99c79c9, all 7 jobs green, post-Orleans-BVT-fix). Updated `RUN_IDS` constant to `["25495265941"]`, refreshed `baseline_artifacts/` from the new run's coverage XML, regenerated `BASELINE_COVERAGE.md`, `baseline_coverage.csv`, and per-repo `static_call_classes.json`. Orleans line coverage 6.07% → **9.98%**; all other repos held: roslyn 76.21%, aspnetcore 60.63%, abp 41.92%, efcore 27.06%, sk 12.12%, runtime 10.18%. No headline warning (every repo emitted real cobertura). TOTAL 40.46% lines / 17.79% branches.

### 2026-05-16T00:00:00Z — Team update
viz layout changed — see `tools/viz/README.md` and `.squad/decisions.md` (entry: 2026-05-16: tools/viz restructure). Per-plot files under `tools/viz/plots/`, shared helpers in `tools/viz/lib/`, new derived `tools/viz/data/per_model_phase.csv` from `aggregate_phase_results.py`. Four new plot families shipped.

### 2026-06-11 — Phase 4 (agentic loop + testability refactoring) tool/strategy/runner built
Built the phase-4 stack on branch `jasper/phase4-refactoring`. Phase 4 = the phase-3 single-agent compile+run feedback loop PLUS an `apply_refactor` tool that edits PRODUCTION source to introduce a testability seam before the test is written; `compile_and_run_check` rebuilds the owning csproj from source so seam edits are picked up for free.

**New module paths:**
- `tools/generation/apply_refactor.py` — `RefactorEngine` + `RefactorResult`.
- `tools/generation/strategies/agentic_loop_refactor.py` — phase-4 strategy (extends phase-3 feedback loop), `RefactorLoopResult(FeedbackLoopResult)` + `refactor_attempts`, `parse_refactor_args()`.
- `tools/generation/agentic_refactor_runner.py` — phase-4 runner (mirrors `agentic_runner_feedback.py` + mock/out-dir/spend-gate flags from the phase-5 runner). Default `--phase phase4-refactoring`. NEW output: `refactors/{repo}/{target_id}.jsonl`.

**apply_refactor transform menu (the constraint IS the anti-gaming mechanism):**
1. `make_virtual` — IMPLEMENTED end-to-end. Line-anchored: finds a non-virtual instance method declaration of the target `method` (line starts with access modifier, names method before `(`, not already static/virtual/abstract/override/sealed/const) and inserts `virtual` after the access modifier. Works only when the method is declared in-repo (framework types like HttpClient cannot be made virtual → graceful rejection suggesting wrapper_interface).
2. `wrapper_interface` — STUB: raises `NotImplementedError` with the contract (emit `I{Receiver}Wrapper` + concrete wrapper, inject via constructor defaulted to concrete).
3. `parameterize_dependency` — STUB: `NotImplementedError` with contract (add defaulted overload taking the dependency; original delegates; public API preserved).
Strategy catches `NotImplementedError`/`TypeError` from stubs/bad args and turns them into a `<tool-result>` so the model can react instead of crashing. Roslyn (Mode1Analyzer infra, Microsoft.CodeAnalysis.CSharp 4.14.0) noted as the robust future path for the harder transforms; not built this pass.

**Safety rails (all implemented):**
- `_safe_prod_path(repo_root, owning_csproj_dir, raw)` — module-level guard; allows writes ONLY inside the owning .csproj subtree (owner located via `compile_only.find_owning_csproj`). Rejects escapes / out-of-subtree / empty.
- snapshot-on-write + `restore_all()` — original bytes captured before first edit (None marker = file didn't exist → deleted on restore). Runner calls `restore_all()` in a `finally` after EVERY cell so cells never contaminate each other and the git tree stays clean.
- behaviour-preservation build — after a successful edit, `dotnet build` the owning csproj (reuses `compile_only` DOTNET/NUGET_CACHE/env/`first_compile_errors`). On failure → AUTO-REVERT + `refactor_rejected` RefactorResult with build errors. Engine ctor `verify_build` flag (default True; runner sets False only in `--mock-llm` smoke so no dotnet/no money). Running the owning project's existing test suite left as TODO (build-preservation is the implemented minimum).

**apply_refactor tool-call syntax (Beck + Lewis's prompts MUST match):**
- primary: `<tool>apply_refactor(transform=make_virtual)</tool>`
- bare:    `<tool>apply_refactor(make_virtual)</tool>`
- extra kw:`<tool>apply_refactor(transform=make_virtual, method=GetAsync)</tool>`
- json:    `<tool>apply_refactor({"transform": "wrapper_interface", "interface_name": "IFoo"})</tool>`
Parsed by `parse_refactor_args(raw) -> (transform, kwargs)`. The other three tools (read_file/list_dir/submit_test) keep the EXACT phase-3 `TOOL_RE` protocol; apply_refactor has its own `APPLY_REFACTOR_RE` and is preferred when it appears first in the response. Per-cell budget `--max-refactors` (default 3).

**RefactorResult schema:** `{transform, applied, reverted, reason, files_changed, build_ok, errors}` (`.to_dict()` truncates errors to 5). Logged per cell to `refactors/{repo}/{target_id}.jsonl` and embedded in `attempts.jsonl` as `refactor_attempts`.

**Verify result:** all three modules import cleanly from repo root under `.venv`; arg parsing covers all four syntaxes; temp-snippet smoke confirmed make_virtual turns `public string GetAsync` → `public virtual string GetAsync`, the prod guard rejects `../../etc/passwd` and allows in-subtree paths, `restore_all()` reverts to byte-pristine, unknown transforms are rejected, and stubs raise NotImplementedError (caught by the strategy). No dotnet build / no Foundry spend. Full mock-LLM end-to-end run left to Beck (needs the `tools/generation/tests/fixtures/refactor/default.json` writer fixture matching the tool-call syntax above).
