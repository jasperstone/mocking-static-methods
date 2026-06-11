# Vogel — CI/CD (history)

## Project Context

- **Project:** mocking-static-methods
- **User:** Jasper (Brady also active 2026-05-08+)
- **Created:** 2026-04-30
- **Goal:** Parallel coverage workflow across 7 .NET OSS repos on GitHub-hosted runners (expanded to 15 in Phase 2).

## Core Context

CI/CD agent. Owns `.github/workflows/` (coverage-orchestrator, test-discovery), `.github/scripts/list_tests.sh`, `.github/actions/` (composite actions), StaticCallAnalyzer containerization.

## Learnings (durable)

### Workflow hygiene
- Pin commit SHAs in workflow inputs to lock the experiment.
- Use `actions/checkout@<sha>` with `ref: <commit-sha>` and `submodules: recursive` for aspnetcore.
- Cache `~/.nuget/packages` keyed on `**/packages.lock.json` or `global.json`.
- Default workflow dispatch is `repo=all` — parallel jobs, no time savings per-repo, but you lose drift/timing/infra signal.
- Always upload raw `TestResults/` for forensics (`coverage-raw-<repo>` artifact).
- `.squad/decisions/inbox/` is gitignored — drop files there for Scribe.

### Coverage collection
- **VSTest "Code Coverage" data collector adapter only attaches against project/sln targets** — never built `.dll`. Always iterate `*.Tests.csproj`.
- Container-bundled SDKs may not satisfy `global.json` pins. Install pinned SDK via `dotnet-install.sh --jsonfile global.json --install-dir $GITHUB_WORKSPACE/.dotnet` for repos lacking their own restore.sh. **Conditional** — MAUI's global.json declares only workload manifests (no `sdk.version`), `--jsonfile` exits nonzero. Check shape first.
- `|| true` on test steps + no validation = silent green. Always include the cobertura validation gate (file present, ≥ 5 KB, ≥ 1 `<class>` element) BEFORE Generate HTML report. HTML succeeds against empty stub and masks problems.
- MTP + `--collect "Code Coverage;Format=cobertura"` is silent-no-op. When global.json sets MTP runner OR `MSTest.Sdk`/exe-style hosts are in play, reach for coverlet.console (mono.cecil, runner-agnostic). Canonical: `coverlet "$asm" --target dotnet --targetargs "test <proj>"`.
- **MSTest 4.x = MSTest.Sdk = MTP routing**, even when project doesn't reference coverlet.collector. Don't trust `--collect` for any MSTest-4.x-or-newer repo. Heuristic: `<Project Sdk="MSTest.Sdk">` OR MSTest 4.x package version → use coverlet.console.
- **Coverlet.console under MSTest.Sdk + MTP can pass tests but emit empty Module table** (separate failure mode from 178-byte MTP-no-op). Tests run on instrumented test assembly, but dependency assemblies aren't instrumented despite being loaded. Likely cause: MTP child-process probe path differs from coverlet's pre-instrumented path. Possible fixes: explicit `--include "[<asm>*]*"` patterns (untested); direct MTP exec via `coverlet "$asm" --target dotnet --targetargs "exec $asm --treenode-filter ..."`; `dotnet-coverage collect` (in-proc, MTP-aware).
- Filter exclusions and instrumentation success are **orthogonal**. Solving test failures does NOT guarantee `<class>` elements appear. Always verify cobertura Module table, not just pass count.

### Build/restore
- NEVER `dotnet restore <whole.sln>` when solution mixes test/server/utility/mobile/wasm projects. Workload manifests evaluate WHOLE graph at restore time, not lazily. Per-csproj restore of just test projects is safe default.
- `dotnet workload install maui` is Linux-incompatible (umbrella manifest declares iOS+Mac SDKs). Use `maui-android` for cross-platform MAUI tests on Linux.
- Local-clone inspection (`grep -l ... cloned_repos/<repo>/**/*.csproj`) beats CI iteration for scope questions.

### Pinning without clones
- For repos NOT in `cloned_repos/`, use `gh api /repos/<owner>/<name>/commits/<branch> --jq .sha` to get HEAD without cloning. Generalizes to any future Phase N additions.

### Test discovery
- `--list-tests` is broken for xunit.v3 repos (returns 0/3/46). Authoritative source for test counts is `tools/test_counts/from_coverage_logs.py` parsing `Passed!  - Failed: N, Passed: N, ... - Foo.dll` summary lines.
- The current `_count_listed_tests` in `list_tests.sh` uses 3 heuristics (max wins): indented FQNs under VSTest header; `Test Name:` prefixed lines; `Total tests: N` summary value. Plus `_discovery_ran` predicate to classify empty-discovery as `status=ok, count=0`.
- Per-project raw stdout/stderr saved to `./_discovery_raw/`, uploaded as `test-discovery-<repo>-raw` (7-day retention) for next-round debugging.

### Container/shell gotchas
- `mcr.microsoft.com/dotnet/sdk:10.0-noble` runs **mawk**, not gawk. 3-arg `match($0, /re/, arr)` silently degrades — empty captures. Use grep+sed for regex-with-captures inside this container.
- **`bash -e -o pipefail` + `ls | head` = step death (exit 141 SIGPIPE)** in GitHub Actions. Never use `head`/`tail` on piped output. Use `awk 'NR<=40'` or `2>&1 | sed -n '1,40p'` instead. Or drop pipefail for that line.

### Coverlet filter syntax
- `FullyQualifiedName!~ClassName` works as exclusion in coverlet's `--filter` (pipes through to dotnet test `--filter`). `&` is the AND separator. Pulling failing class names: `gh run view <id> --log-failed 2>&1 | grep -oE "<Repo>\.Tests\.[A-Za-z_.]+" | sort -u`.

### StaticCallAnalyzer container
- Containerized (`StaticCallAnalyzer/Dockerfile` + `run.sh`). `aggregate_baseline.py` invokes wrapper. Host needs only python3 + gh + docker — no .NET 8 SDK install required.
- Docker emits file paths as `/src/<...>` instead of host paths — `aggregate_static` prefix-strip handles both prefixes.

## Recent Updates

### 2026-06-11 — Phase-4 (agentic loop + refactoring tool) cost model added to estimate.py
- The phase-4→phase-5 multi-agent rename freed the `--project-phase4` flag. Added a
  phase-4 projection modeling the NEW phase 4 = the SAME single writer agent as
  phase 3 PLUS a LOCAL `apply_refactor` tool (no LLM behind it) that introduces a
  testability seam before the test is written and the csproj recompiles.
- **Model assumptions (cheap by design vs phase 5):**
  - **ONE LLM role (writer). NO reviewer/fixer LLM** — so no 2nd/3rd model role
    multiplying token spend. This is the dominant reason phase 4 ≪ phase 5.
  - **Token inflation, not an extra agent:** `P4R_TOKEN_INFLATION = 1.5` flat
    multiplier on the phase-3 writer token base (writer takes more turns/cell:
    inspect → pick seam → call apply_refactor → read → iterate test). Modest range
    ~1.4–1.6, NOT a whole extra agent.
  - **apply_refactor = billable agent tool invocation:** `P4R_REFACTOR_CALLS_PER_CELL
    = 1.2` calls/cell (≈ one seam, occasional second), billed at the EXISTING
    `TOOLS_SURCHARGE_PER_CALL` ($0.03375) like read_file/list_dir. Local/zero-token,
    but the agent-runtime surface still bills → adds to invocation-scaled Foundry
    Tools overhead.
  - **Billing split convention reused from `project_phase5`:** token spend keeps the
    phase-3 marketplace fraction; Foundry Tools overhead wholly credit.
- **Default = run_1 (`P4R_DEFAULT_RUNS = 1`), the go/no-go dispatch.** GOTCHA worth
  remembering: the phase-3 *combined* base alone is ~$342 (already > the $250 cap),
  so a full 3-run phase-4 sweep can NEVER be under cap — it's strictly ≥ phase 3.
  Defaulting the printed projection to run_1 is the only self-consistent way to land
  "under cap" (and it's the dispatch you actually run first, mirroring frozen
  phase-5 run_1). The verify command prints run_1.
- **Projected combined total: phase-4 run_1 = $213.79 → 85.5% of the $250 cap, UNDER
  by $36.21** (credit $156.13 / marketplace $57.67; ~$63.79 to card; ~1.87× the
  same-runs phase-3 single-writer base $114.18). Full 3-run set (runs=3) ≈ $641
  (257% of cap) but ≈ 54% of phase 5's $1,197 — roughly half, because of the single
  LLM role.
- **Did NOT rename/break** `--project-phase5` / `project_phase5` / `P5_*` /
  `P3_RUNS_PER_CELL` / `FOUNDRY_*` / `TOKEN_RECON_FACTOR` / `CREDIT_USD`. `--runs` is
  now shared between phase-4 and phase-5 ad-hoc projections; added `--refactor-calls`.
  Normal runs auto-print the phase-4 go/no-go block beside the auto phase-5 block.
- Verified: `--project-phase4 --cap 250` EXIT 0 ($213.79); `--project-phase5 --cap
  250` EXIT 0 (Config C still $1,197.49); normal run EXIT 0 (phase-3 residual still
  −$0.18); no lint errors. **No Azure spend — estimator-only.** Decision dropped to
  `.squad/decisions/inbox/vogel-phase4-cost-model.md`.

### 2026-06-11 — phase3-tripwire-250 budget DELETED (redundant after phase 3 sealed) *(condensed)*
Deleted `phase3-tripwire-250` — once phase 3 sealed it was an exact redundant twin of
`phase4-tripwire-250` (same subscription scope, $250 Monthly, same spend-tracking). Via
`az consumption budget delete` (subscription is the default scope; EXIT 0). Prior config
snapshot before delete: $250 Monthly, 2026-05-01→2027-12-31 UTC, currentSpend $6.27, Actual
50/75/90 + Forecasted 100% (thresholds differed from phase4's 50/80/100; redundancy was on
scope/amount/spend-tracking). Surviving 3-budget set: `VS_Credit_Budget` ($150 BillingMonth),
`budget-mockstatic-50` ($50 Monthly), `phase4-tripwire-250` ($250 Monthly). phase4 thresholds
not altered (pending Jasper). Budget delete = FREE control-plane op, no spend. Full text →
`history-archive.md`.

### 2026-06-10 — phase4-tripwire-250 budget created + PR #28 squad bookkeeping *(condensed)*
Created Azure budget `phase4-tripwire-250` = the combined $250 soft cap: subscription scope
`9490eefa-f2af-4485-983f-63397bfb5386` (tracks total monthly spend, marketplace + credit),
$250 Monthly, 2026-06-01→2027-06-01 UTC, Actual 50/80/100 + Forecasted 100%. Via `az rest put`
on `Microsoft.Consumption/budgets` (api 2024-08-01); verified `currentSpend $0`. Reused phase3's
configured contactEmail (no invented address, never read `git config user.email`, PII never
printed). Enforcement caveat: Azure budgets ALERT only — no hard stop; a true at-cap kill =
100% alert → action group → webhook (not built, flagged); real hard stop = subscription
spending-limit toggle (OFF for soft-cap). Also committed `.squad` bookkeeping as `9d07268` on
`jasper/phase4-scaffold` (PR #28). Gotcha: `.squad/decisions/inbox/`, `.squad/log/`, and
`.squad/orchestration-log/` are gitignored — Scribe logs are NOT committed (established repo
behavior). FREE control-plane op + git only, no spend. Full text → `history-archive.md`.

### 2026-06-10 — Phase-4 calibration reframed as run_1; cycles=1 frozen; on PR #28 *(condensed)*
Jasper froze the phase-4 design and reframed the calibration pass as **run_1** of the real
3-run set (not a throwaway). Frozen config: `max_review_cycles = 1`, `runs_per_cell = 3`
(run_1 → go/no-go → runs 2+3), full 6-model panel (no drops), temp 0.0 / top_p 1.0 / seed 42 /
max_output_tokens 4096. Bill-calibrated: run_1 (R1,C1) ≈ **$209 / 84%** — under the $250 cap
(clean go, ~$59 to card); full 3-run set (R3,C1) ≈ **$628 / 251%** is the real go/no-go.
Reusability discipline: run_1 poolable ONLY if harness/prompts/config frozen at one SHA.
Code: `estimate.py` `P4_CONFIGS` realigned (A=R1/C1, B=R3/C1, C=R3/C3), `P4_DEFAULT_*` left
at R3/C3; `PLAN.md` budgets/projection tables. Committed onto `jasper/phase4-scaffold` (PR #28).
**NO Azure spend.** Full text → `history-archive.md`; canonical → decisions.md
("Phase-4 calibration is run_1 of the frozen design").

### 2026-06-10 — Cost estimator rebuilt to project the ACTUAL Azure bill *(condensed)*
Rebuilt `tools/cost/estimate.py` from token-only ($82.19 phase 3) to reconcile against the
real ~$342 May Foundry bill. Two May-calibrated knobs: `TOKEN_RECON_FACTOR = 1.95` (Foundry
Models $160.45 / token-list $82.19) and `TOOLS_SURCHARGE_PER_CALL = $0.03375` ($182.26 / 5,400
agent-role invocations — Foundry Tools is the biggest line, NOT token-based). Multi-agent
overhead (phase 4) = 4.3 invocations/cell. `BILLING` dict splits credit vs marketplace; AI
Search excluded. az evidence: only codestral routes via `Microsoft.SaaS`; llama+grok bill via
`Microsoft.CognitiveServices` (credit surface), so the May SaaS line ($24.22) suggests they
belong in credit — flagged but left per directive (combined/cap number is split-independent;
dollars not queryable via az on this MSDN sub). Phase-3 model = $342.53 vs actual $342.71
(−$0.18). Full-scope phase-4 projection ≈ **$1,197 / 479%** — the go/no-go signal. Files:
`estimate.py`, `phase3-agentic-loop/COSTS.md`, `phase4-multiagent/PLAN.md`. **NO Azure spend.**
Full text → `history-archive.md`; canonical → decisions.md ("Cost estimator models the actual
Azure bill").

### 2026-05-16T00:00:00Z — Team update (viz layout)
viz layout changed — see `tools/viz/README.md` and `.squad/decisions.md` (entry: 2026-05-16: tools/viz restructure). Per-plot files under `tools/viz/plots/`, shared helpers in `tools/viz/lib/`, new derived `tools/viz/data/per_model_phase.csv` from `aggregate_phase_results.py`. Four new plot families shipped.

### 2026-05-08 — MAUI removed; OpenRA + StockSharp added (Phase 2 baseline) — commit d3689e0
Removed deferred `coverage-maui` job entirely. 4 rounds of remediation hit increasingly internal MS-CI assumptions; per Brady, data didn't justify drag.

Added 2 new jobs:
- **OpenRA** (`8f2138c7`, bleed HEAD) — `net8.0`, NUnit 4 + NUnit3TestAdapter, no coverlet. Data-collector path (`--collect "Code Coverage;Format=cobertura"` + `dotnet-coverage merge`), same as abp/efcore/roslyn/runtime. Side-installs .NET 8 SDK because noble ships only .NET 10 and OpenRA has no `global.json`.
- **StockSharp** (`a26ce597`, master HEAD) — `net10.0` (via `common_target_*.props` → `NetVer=10`), MSTest 4.x, no coverlet. Per-csproj restore+build of `Tests/Tests.csproj` only. Risk flagged: references `Microsoft.Data.SqlClient` + `Ecng.Data.SqlServer` (SQL-dependent tests filterable by Category=Integration).

**Skipped PowerToys + Files** — both Windows-only at SDK/TFM level. Files mandates `net10.0-windows10.0.26100.0`; PowerToys UnitTests all in `src/modules/<windows-only>/` chains.

Active matrix: **15 repos**. Triggered runs: OpenRA=25552129165, StockSharp=25552132370.

### 2026-05-08 — StockSharp coverlet.console fix + MTP empty-modules blocker *(condensed)*
MSTest 4.x → MTP routing → data-collector silent-no-op (178-byte stub). Swapped to coverlet.console
wrap + `FullyQualifiedName!~` exclusions for 5 flaky classes → 0 failures / 4096 passed, but
cobertura had an empty Module table (zero modules instrumented despite ~80 dep DLLs). Round-2
`--include` patterns unproven; self-inflicted SIGPIPE (`ls | head`) killed the step (reverted).
Stopped at 2 attempts. Full diagnosis: decisions.md "2026-05-08: StockSharp flaky-test filter".

### Earlier entries
- 2026-05-06 — Silent empty-cobertura fix (commit 7885485)
- 2026-05-07 — Containerized StaticCallAnalyzer
- 2026-05-07 — Test-discovery counter rewritten for multi-shape adapter output
- 2026-05-07 — Coverage orchestrator expanded 7→14 repos (Avalonia, duplicati, eShop, garnet, jellyfin, maui, server) + round-1/round-2 fix passes

Full text of all entries above is in `history-archive.md`.
