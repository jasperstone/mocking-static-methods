# Watney — history archive

Older / verbose entries moved out of `history.md` during summarization (2026-06-12).
Durable build learnings and the condensed recent timeline remain in `history.md`.

## Recent Updates (archived detail)

- 2026-04-30 — Workflow audit findings: 6/7 jobs set `dotnet-version: 9.0.x` while their `global.json` requires 10.0.x (aspnetcore 10.0.101, efcore 10.0.102, orleans 10.0.102, roslyn 10.0.100-rc.2, sk 10.0.100). Runtime job has NO `setup-dotnet` step. EF Core sources `activate.sh` but doesn't export `DOTNET_ROOT` to `$GITHUB_ENV` like aspnetcore does — fragile. coverlet.collector is never added to test projects (README requires it for aspnetcore's 137 test projects).

- 2026-05-07 — Team update from Vogel: `StaticCallAnalyzer` is now containerized (multi-stage Dockerfile, SDK 8.0 → runtime 8.0). Use `StaticCallAnalyzer/run.sh` wrapper; `aggregate_baseline.py` invokes it automatically. Eliminates host .NET 8 SDK dependency for the analyzer toolchain. Commit 3d53670 on `jasper/squad`.

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
1. `make_virtual` — IMPLEMENTED end-to-end. Line-anchored: finds a non-virtual instance method declaration of the target `method` and inserts `virtual` after the access modifier. Works only when the method is declared in-repo (framework types → graceful rejection suggesting wrapper_interface).
2. `wrapper_interface` — STUB (at this date): raises `NotImplementedError` with the contract (emit `I{Receiver}Wrapper` + concrete wrapper, inject via constructor defaulted to concrete).
3. `parameterize_dependency` — STUB (at this date): `NotImplementedError` with contract (add defaulted overload taking the dependency; original delegates; public API preserved).
Strategy catches `NotImplementedError`/`TypeError` and turns them into a `<tool-result>` so the model reacts instead of crashing.

**Safety rails (all implemented):** `_safe_prod_path(repo_root, owning_csproj_dir, raw)` allows writes ONLY inside the owning .csproj subtree (owner via `compile_only.find_owning_csproj`); snapshot-on-write + `restore_all()` (None marker = file didn't exist → deleted on restore), runner calls `restore_all()` in a `finally` after EVERY cell; behaviour-preservation build via `dotnet build` of the owning csproj, AUTO-REVERT + `refactor_rejected` on fail, ctor `verify_build` flag (default True; runner sets False only in `--mock-llm`).

**apply_refactor tool-call syntax (prompts MUST match):** primary `apply_refactor(transform=make_virtual)`; bare `apply_refactor(make_virtual)`; extra kw `apply_refactor(transform=make_virtual, method=GetAsync)`; json `apply_refactor({"transform": "wrapper_interface", "interface_name": "IFoo"})`. Parsed by `parse_refactor_args(raw) -> (transform, kwargs)`. Other three tools keep the phase-3 `TOOL_RE`; apply_refactor has its own `APPLY_REFACTOR_RE`, preferred when first. Per-cell budget `--max-refactors` (default 3). `RefactorResult` schema: `{transform, applied, reverted, reason, files_changed, build_ok, errors}` (errors truncated to 5).

### 2026-06-11 — bundler self-inclusion bug fixed + context regenerated for Prism
`tools/bundle_dissertation_context.py` walks the whole repo concatenating every narrative `.md` into `dissertation_bundle/dissertation_context.md`, but its `EXCLUDE_DIRS` set did NOT include the output directory `dissertation_bundle`, so any run after the first folded the PREVIOUS bundle back in (self-inclusion). Fix: added `"dissertation_bundle"` to `EXCLUDE_DIRS` (first entry). One line covers both the `os.walk` prune and `is_excluded()`. Lesson: a tool that writes into the tree it scans must always exclude its own output dir. Regenerated clean: 50 files, 232,483 bytes; `grep "=== dissertation_bundle/"` returns nothing; `multi-team` count = 0. Did NOT commit — coordinator commits.

### 2026-06-12 — RoslynRefactorTool built (wrapper_interface + parameterize_dependency)
Implemented the two stubbed phase-4 transforms as a FULLY GENERAL pure C# Roslyn rewriter per `phases/phase4-refactoring/TRANSFORM_CONTRACT.md`. Confined to `RoslynRefactorTool/`, the `.sln`, and `apply_refactor.py`.

**Project layout (§7):** `RoslynRefactorTool/{RoslynRefactorTool.csproj, Program.cs, SeamCore.cs, WrapperInterfaceRewriter.cs, ParameterizeDependencyRewriter.cs}`. csproj is a byte-for-byte mirror of `Mode1Analyzer.csproj` (net10.0 Exe, `Microsoft.CodeAnalysis.CSharp` 4.14.0, same 5 ref-pack `PackageReference`s, same `CopyRefAssemblies AfterTargets=Build` copying `lib/net9.0/*.dll` → `$(OutDir)refs/`). Reference loading mirrors `Program.LoadReferences()`. No per-project `dotnet restore` — fast path.

**Build invocation:** `~/.dotnet/dotnet build RoslynRefactorTool/RoslynRefactorTool.csproj -c Release -v quiet --nologo` (dotnet 10.0.203); `dotnet sln mocking-static-methods.sln add ...`. Output dll: `RoslynRefactorTool/bin/Release/net10.0/RoslynRefactorTool.dll` (+ `refs/`). Python resolves via `ROSLYN_REFACTOR_TOOL_DLL` (Release preferred, Debug fallback).

**Tool contract:** pure — reads owning project source, emits ONLY JSON on stdout (`{ok,applicable,reason,files{},seam{}}`), diagnostics to stderr. Python (`_invoke_roslyn_tool`) owns ALL writes, re-checks every returned path through `_safe_prod_path`, `_write`s post-state text, then `apply()` runs `_build_owning_project()` + auto-revert. Added `seam: dict` to `RefactorResult` (+ `to_dict()`); `make_virtual` seam stays `{}`.

**Roslyn gotchas (durable — see history.md Learnings for the condensed list):**
1. Extension-method forwarding needs the static FQN form (`global::...LoggerExtensions.LogInformation(_inner, args)`) because the generated file has no `using`; plain instance members keep `_inner.M(...)`. Generated types use a `global::`-qualified `SymbolDisplayFormat`.
2. Generic methods: reconstruct from `IMethodSymbol.OriginalDefinition`, NOT the constructed bound symbol (else `T` gets substituted → CS0266/CS0266). Call site keeps its explicit `<T>`.
3. parameterize delegator must call the ENCLOSING method (`method.Identifier.Text`), not the seam member; the body rewriter uses the seam member name only to retarget the receiver.
4. Inserted nodes have no trivia — `ParseMemberDeclaration`/`ParseStatement` + `NormalizeWhitespace()` jam members together; add explicit `ElasticCarriageReturnLineFeed` + 4-space `Whitespace` leading trivia and preserve the replaced node's leading trivia.
5. `ReplaceNode(old, IEnumerable<SyntaxNode>)` swaps one method for [delegator, overload].
6. No `Microsoft.CodeAnalysis.Workspaces`/`Formatter` (only `.CSharp`) — use node-level `NormalizeWhitespace()`.

**Verification (hermetic):** all 5 §2/§3 cases emit `applicable=true` + full seam AND rewritten output COMPILES in a real net10.0 project (NUGET_PACKAGES=.nuget-cache). All §5 reject rows verified with exact tokens. `test_refactor_smoke.py` GREEN. 20/20 tool checks. §5 rows without dedicated fixtures (code paths exist; fixtures pending Beck): `ctor_chaining`, `primary_ctor`, `partial_split`, `no_receiver_source`, `receiver_is_this`, `unbound_receiver`.

### 2026-06-11 — ISP `unbound_receiver` false-negative (real cause: missing implicit usings)
Beck found `IServiceProvider.GetRequiredService<T>()` returned `applicable=false / reason=unbound_receiver`, contradicting §2.2 Case B (~83/300 targets). Root cause (NOT a reference-identity split): `BuildCompilation` parses every `*.cs` under the owning dir but skips `obj/`, where the SDK drops `*.GlobalUsings.g.cs` for `<ImplicitUsings>enable</ImplicitUsings>` — so files relying on implicit `global using System;` resolve `IServiceProvider` to `ErrorTypeSymbol` (CS0246) and the extension call can't bind. Fix: `BuildCompilation` prepends a synthetic `__ImplicitGlobalUsings.g.cs` carrying the default `Microsoft.NET.Sdk` implicit-usings set; additive + lowest-priority so explicit-using files are unaffected. Also hardened receiver-type derivation to prefer the declared `this` param (`ReducedFrom.Parameters[0].Type`). Genuine unbindable receiver still returns `unbound_receiver` (verified with `NonExistentService`). `pytest` 25 passed; the 2 Case-B xfails promoted to positive rows. **Gotcha:** any `unbound_receiver` on a common BCL type is almost certainly a missing-implicit-using — check `GetDiagnostics()` for `CS0246` first.

### 2026-06-12 — Deterministic applicability sweep over all 300 real targets (NO LLM/Azure)
Built `tools/generation/refactor_applicability_sweep.py`: runs phase-4 transforms via `RefactorEngine.apply()` against the real cloned repos for every `targets/v2/targets.csv` row, restoring each repo after every target (`restore_all()` in `finally`). FAST pass (900 runs, no build, ~21 min, jobs=6): parameterize 190/300 (63.3%), wrapper 120/300 (40.0%), make_virtual 6/300 (expected). BUILD-verified sample (9 targets): 9/9 seam, 5/9 build-pass; all 4 failures auto-reverted. Two real-repo bugs surfaced (later fixed in analyzer-hardening): CS1737 (parameterize appends optional after a trailing optional/`params`) and CS1503 (wrapper rewrites ALL same-receiver sites to a wrapper modeling only the target overload). Safety verified: 0 modified tracked `.cs`, 0 leftover `I*Wrapper.cs`. **Operational gotchas:** (1) NEVER run a long sweep as a FOREGROUND command in the shared persistent terminal — subsequent `run_in_terminal` injects into the same bash and Ctrl-Cs the foreground job; launch with `nohup … &`. (2) the git-status cleanliness check yields false positives from pre-existing clutter (mtimes Dec-2025/Jan-2026) — check specifically for modified tracked `.cs` + untracked `I*Wrapper.cs`.
