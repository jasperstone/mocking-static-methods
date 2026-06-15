# Beck — Test & Coverage Engineer (history)

## Project Context

- **Project:** mocking-static-methods
- **User:** Jasper (Brady also active 2026-05-08+)
- **Created:** 2026-04-30
- **Goal:** Reproducible test runs + coverage across 7 .NET OSS repos (expanded to 15 in Phase 2).

## Core Context

Test/coverage agent. Owns `tools/test_counts/`, `tools/test_discovery/`, `tools/coverage_xref/`, `tools/viz/`, `tools/cost/`, `aggregate_baseline.py`, and Phase 2/3 results aggregation.

## Learnings (durable)

### Phase-4 §4.3 via_seam verifier + §9.1/§9.2 refactor tests (2026-06-12)
- **dotnet trap:** ALWAYS use `~/.dotnet/dotnet` (has the net10.0 runtime), never the snap `/snap/bin/dotnet` (errors "No frameworks were found"). The codebase already defaults correctly via `tools/evaluation/compile_only.py:DOTNET = os.environ.get("DOTNET", ~/.dotnet/dotnet)`. Reuse that constant in tests instead of hardcoding.
- **Hermetic C# compile without NuGet restore:** `RoslynRefactorTool` compiles owning-dir `*.cs` in-memory against its OWN bundled refs at `bin/Release/net10.0/refs/` (Microsoft.Extensions.{Logging,Logging.Abstractions,DependencyInjection.Abstractions,...}). For §9.1 rewritten-output compile checks, build a throwaway csproj with `<Reference><HintPath>` pointing at those bundled DLLs — no `dotnet restore`, fully offline. Gate with an env flag (`BECK_SKIP_DOTNET_COMPILE=1`) for compile-free CI.
- **Runner mock mode runs the Roslyn tool for real:** `--mock-llm` only sets `verify_build=False` (skips the post-write `dotnet build`); `engine.apply()` STILL shells out to `RoslynRefactorTool.dll`, producing a genuine seam + `files_changed`. So an end-to-end integration test needs no Azure/Foundry but DOES need the dll+dotnet (skip cleanly if absent).
- **New `--mock-cell-json` flag** (added to `agentic_refactor_runner.py`): mock mode otherwise hardcodes the synthesized `mock:0001/mock-repo` cell. Pass `--mock-cell-json` to point the engine at a real ILogger/HttpClient fixture repo so `apply_refactor(wrapper_interface|parameterize_dependency)` exercises the tool. `find_owning_csproj` only needs a `.csproj` to EXIST (the tool never builds it), so a 6-line `net10.0` stub csproj + the site `.cs` is enough.
- **Interface-name convention (drives test fixtures):** `ResolveNames` strips a single leading `I` before an uppercase, then `ifaceBase = "I" + recv + "Wrapper"`. So `ILogger` → `ILoggerWrapper`/param `loggerWrapper`; `HttpClient` → `IHttpClientWrapper`/param `httpClientWrapper`. wrapper_interface → `injection="ctor"`, `injection_ref=<param>`. parameterize_dependency → `injection="overload"`, `injection_ref="<enclosingMethod>(origTypes..., IWrapper)"`. Generated file = `<InterfaceName>.cs`.
- **via_seam verifier design (4 regex checks over the FINAL submitted test):** (1) seam type referenced in a mock construction (`Mock<I>`, `Substitute.For<I>`, `Mock.Of<I>`, `A.Fake<I>`, or hand-rolled `class X : I`); (2) the mock is INJECTED at the injection point — for `ctor` match `new Containing(...)` args contain a mock token (`mock.Object`/sub var/`new Fake`) OR a named arg `paramName:`; for `overload` match `enclosing(...)` args contain a mock token; (3) target method driven — a method invoked on the constructed instance (ctor) or the overload call itself (overload); (4) non-trivial assertion — `.Verify(`/`.Received(`/fluent `.Should()`/any `Assert.*` that isn't `Assert.True(true)`-style. `via_seam = all(checks)`, persisted on the attempts row AND as a `{"verification":true,...}` line on the per-cell refactors log.
- **Regex gotcha:** the overload-call lookbehind must be `(?<!\w)` NOT `(?<![\w.])` — the latter wrongly blocks member-access calls `client.FetchAsync(` (preceded by `.`). `(?<!\w)` still rejects longer identifiers like a test method named `FetchAsync_DoesX` because the trailing `_` breaks `name\s*\(`.
- **Gaming pattern the verifier catches:** a test that constructs the mock (check 1 passes) but never injects it — `new Worker(logger)` with no `wrapper.Object`, or calling the ORIGINAL signature `FetchAsync(url)` instead of the overload — fails check 2 → `via_seam=False`. Proven by both an integration fixture (`refactor_wrapper_gamed`) and direct unit tests.

### Phase-4 mock end-to-end validation on REAL targets (2026-06-12)
- **First time the full loop ran against genuine cloned-repo targets** (prior runs were `make_virtual` fixtures only). Drove `read_file/list_dir → apply_refactor → seam → submit_test → compile+run → via_seam` for 6 cases in MOCK mode ($0): jellyfin:0006 + abp:0147 (wrapper_interface), server:0053 + semantic-kernel:0125 (parameterize_dependency), one gamed jellyfin:0006, one duplicati:0006 make_virtual.
- **Result:** 4 legit interface-injection cases → applied=True, via_seam=True (all 4 checks). Gamed case → via_seam=False with the *single* failing check `injected_at_injection_point` (constructed `Mock<ILoggerWrapper>` but called `new ApplicationHost()` with no inject + trivial assert). make_virtual → via_seam=None (no descriptor; subclass-override path). pytest 36 passed.
- **Authoring via_seam-passing tests requires the REAL seam first:** dump it by running `engine.apply()` with `verify_build=False` (fast, no dotnet build) and printing `res.seam` — gives exact `interface` simple-name, `containing_type`, `injection`, and `injection_ref` to template the test. Named-arg ctor injection (`new Containing(loggerWrapper: wrapper.Object)`) satisfies check 2 because the verifier matches `\b{injection_ref}\s*:` in the ctor args.
- **MOCK-mode caveat for the funded run:** `--mock-llm` stubs the in-loop compile/run (`run_ok` always True) AND sets `verify_build=False` (no behaviour-preservation `dotnet build`, so `build_ok` is None in the refactor log). The genuinely-exercised real signals are: Roslyn apply on real source + the real seam + via_seam discrimination. build_ok=True was confirmed separately in `build_sample_{wrapper,parameterize}.csv`.
- **Cleanliness:** runner `restore_all()` reverts every write; verified the specific touched files (`*Service.cs`, generated `ILoggerWrapper.cs`/`IHttpMessageInvokerWrapper.cs`) are gone. Pre-existing untracked artifacts in abp/server (`CoverageReport/`, `GeneratedTests/`, `*.Tests.cs`) are unrelated — always diff the *specific* refactor paths, not just `git status` line count.
- **Comment fix:** corrected stale "STUB" labels for wrapper_interface/parameterize_dependency in `apply_refactor.py` docstring (both now delegate to RoslynRefactorTool).

### Earlier durable learnings (May-era — full detail in `history-archive.md`)
- **Filters/categories:** Orleans `BVT` is unit-level (re-include); `SlowBVT`/`LoadShedding`/`CorePerf` excluded. Discovery globs must match `*.Tests.csproj` AND `*.UnitTests.csproj`.
- **Test counts:** `--list-tests` is broken for xunit.v3 — authoritative source is `tools/test_counts/from_coverage_logs.py` parsing `Passed!  - … Total: N` lines from Coverage Orchestrator logs (MTP wrapper emits lowercase `total: N`; classic uppercase `Total: N`). `.NET Runtime` → slug `runtime`.
- **Mode #1 attribution:** `find_site` in `build_unified_table.py` handles 4 cobertura path shapes (suffix-match 5/4/3/2). Two "covered=0" modes: empty instrumentation (Avalonia/eShop, 0 global hits) vs real test-scope gap (duplicati/runtime).
- **Per-csproj cobertura inflation (dedup fix, 2026-05-09):** each cobertura enumerates EVERY loaded assembly → summing `lines-valid` across N files multiplies shared sources N×. Fix in `build_unified_table.py`: per-`(file,line)` map, max hits, sum unique once; iterate only direct `<class>/<lines>/<line>`; `line_map.get(num, -1)` so zero-hit lines register. TOTAL 33.04% → 58.23%.
- **Methodology:** most "low coverage" is a measurement artifact — fix the math first. StaticCallAnalyzer appends to `./analysis_results.json` in CWD (run from clean temp dir). `sdk:10.0-noble` runs mawk not gawk (3-arg `match()` degrades — use grep+sed).
- **tools/viz (2026-05-16 restructure):** `tools/cost/estimate.py::PRICES` is canonical (never duplicate). Phase-2 inclusive glob `results*/**/attempts.jsonl` = 6,307 attempts / $89.98. Phase-3 raw not committed (synthesise from `per_model_repo.csv`, cost blank). ggrepel NOT installed. `repo_root()` walks to the `.sln` sentinel. Verification: phase2 6,307/3,870/326/129/$89.98; phase3 1,688/270/132/n-a.

## Recent Updates

### 2026-06-11 — Phase-4 mock-LLM smoke test (branch jasper/phase4-refactoring)
- **Test:** `tools/generation/tests/test_refactor_smoke.py` (mirrors `test_multi_agent_smoke.py`). Drives `agentic_refactor_runner.py` via subprocess with `--mock-llm --mock-fixtures-dir tools/generation/tests/fixtures/refactor --phase phase4-refactoring --model mock-llm --run-index 0 --target-set v2 --target-ids mock:0001 --out-dir {tmp} --cloned-repos {tmp}/cloned_repos`. **Fully hermetic** — passes in ~0.3s, NO dotnet/Foundry.
- **Fixture shape** (`tools/generation/tests/fixtures/refactor/default.json`): same JSON list-of-`{role,text}` as multi_agent, but the runner only consumes `role:"writer"` turns (`mock_llm.make_role_generate(file,"writer")`). One writer turn per loop turn. My 3 scripted writer turns: `read_file(mock.cs)` → `<tool>apply_refactor(transform=make_virtual)</tool>` → `<tool>submit_test(csharp)</tool>` + fenced ```csharp block (subclass-and-override the now-virtual method). Used Watney's EXACT `transform=make_virtual` syntax.
- **CHOSEN TARGET — important runner gotcha:** the phase-4 runner's `--mock-llm` mode does NOT read `targets/v2/targets.csv`. It **hardcodes** a synthesized cell: `target_id=mock:0001, repo=mock-repo, file=mock.cs, method=DoSomething, kind=NonVirtual`. The `--target-ids` filter runs against that synthesized row, so passing a real id (e.g. `OpenRA:0003`) filters to ZERO cells (silent no-op — the runner's own docstring example is wrong). Pass `--target-ids mock:0001` to keep the cell. The engine still operates on a genuine NonVirtual instance method.
- **Making make_virtual APPLY hermetically:** the engine is built with `repo_root=cloned_root/mock-repo`. Point `--cloned-repos` at a tmp dir holding a real `mock-repo/MockLib.csproj` + `mock.cs` declaring `public string DoSomething()`. Then `find_owning_csproj` resolves, `_inject_virtual` adds `virtual`, and since mock mode sets `verify_build=False` no `dotnet build` gates it → `applied=True` (not just recorded-rejected). Output layout: `attempts.jsonl`, `generated_tests/mock-repo/mock_0001/test.cs`, `turns/mock-repo/mock_0001.jsonl`, `refactors/mock-repo/mock_0001.jsonl` (`:` → `_` in tid).

### 2026-05-16 — tools/viz restructure (commit pending)
Split `render_phase3.R` into `tools/viz/plots/*.R` + `tools/viz/lib/{load,theme}.R`. Added `aggregate_phase_results.py` → derived `tools/viz/data/per_model_phase.csv`. Four new plot families: `successful_tests_progression`, `coverage_baseline`, `cost_efficiency`, `cost_per_passing_test`. Phase2 totals reconcile to COSTS.md. Decision: `2026-05-16: tools/viz restructure`.

### 2026-05-09 — Cobertura dedup fix (see Learnings above for details)
Per-csproj cobertura inflation root-caused and fixed in `build_unified_table.py`. TOTAL 33.04→58.23%.

### 2026-05-08 — Mode #1 attribution diagnostics
Read-only investigation of 4 Mode#1=0 repos (Avalonia, eShop, duplicati, runtime). Two failure modes: empty-instrumentation (Avalonia/eShop) vs real test-scope gap (duplicati/runtime). No xref change needed. Decisions: `2026-05-08: Mode #1 attribution diagnosis` and `2026-05-08: Mode #1 attribution gap — not a path-matcher bug`.

### 2026-05-08 — Baseline matrix update
Matrix is now 15 repos (MAUI removed; OpenRA + StockSharp added; Files + PowerToys skipped Windows-only). Next baseline + test-counts refresh once new runs complete.

### Earlier entries
Pre-2026-05-08 entries (Phase 1 baseline, test-discovery workflow, Orleans BVT decision, test-counts-from-coverage-logs tool, refresh against run 25495265941) archived to `history-archive.md`.
