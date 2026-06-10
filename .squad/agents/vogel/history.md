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

### 2026-06-10 — Phase-4 calibration reframed as run_1; cycles=1 frozen; on PR #28
Jasper reframed the phase-4 calibration pass as **run_1 of the real 3-run experiment**
so calibration spend is not repeated. Captured the full frozen design + opened a PR.

- **Frozen phase-4 config (sealed before run_1, no changes after):**
  `max_review_cycles = 1` (down from 3 — multi-agent tool overhead is the dominant
  cost driver; cycles=1 minimizes it while still firing writer→reviewer→fixer once);
  `runs_per_cell = 3` target dispatched as **run_1 → go/no-go → runs 2+3**; full
  6-model panel (no drops); temp 0.0, top_p 1.0, seed 42, max_output_tokens 4096.
- **Calibration = run_1.** Not a throwaway — pooled into the final result set.
  **Reusability discipline:** run_1 is poolable with runs 2+3 ONLY if harness/prompts/
  config are frozen at one SHA; any prompt edit / cycle change / model swap after
  calibration invalidates run_1 and forces a re-run.
- **Bill-calibrated cycles=1 figures** (`estimate.py --project-phase4`):
  run_1 calibration (R=1,C=1) = **$209 / 84% of $250 cap — UNDER the cap**, ~$59 to
  card (inside the $150 credit) → clean go. Full 3-run set (R=3,C=1) = **$628 / 251%**,
  ~$478 to card → the real go/no-go after run_1's measured bill. Reference original
  (R=3,C=3) = $1,197 / 479%. Freezing cycles 2→1 dropped the calibration from the old
  ~$304 to $209.
- **Code/doc changes:** `tools/cost/estimate.py` — realigned named `P4_CONFIGS` to the
  frozen design (A=run_1 R1/C1, B=full set R3/C1, C=reference R3/C3); `P4_DEFAULT_*`
  left at R3/C3 so plain-run still reproduces the $1,197 consistency check.
  `phases/phase4-multiagent/PLAN.md` — budgets table (`max_review_cycles` = 1 frozen,
  `runs_per_cell` run_1 framing), cost projection table + new "Calibration is run_1"
  section. Decision recorded in inbox `vogel-phase4-calibration-is-run1.md`.
- **Git:** PLAN.md lives only on `jasper/phase4-scaffold` (open PR #28), not on
  `main` — a branch off main couldn't carry the PLAN.md edits coherently. Per
  Jasper's call (Option C), committed the cost-calibration work directly onto
  `jasper/phase4-scaffold` so it rides the existing **PR #28**
  (https://github.com/jasperstone/mocking-static-methods/pull/28). Pushed to
  origin. **NO Azure spend; no experiment workflow dispatched.**


Jasper chose to cut phase-4 cost via **runs + review cycles, NOT by dropping models**
(full 6-model panel preserved for the cross-model comparison). Extended
`tools/cost/estimate.py` with a parametrized projection so the configs are reproducible.

- **Per-cell agent call-count multiplier** (the dominant cost lever, drives Foundry
  Tools overhead). Theoretical max = `1 + 2·C`; realized = `1 + 1.1·C` using
  May-calibrated per-cycle rates (reviewer 0.6/cycle, fixer 0.5/cycle):
  - cycles=1 → 2.1 calls/cell (max 3)
  - cycles=2 → 3.2 calls/cell (max 5)
  - cycles=3 → 4.3 calls/cell (max 7) ← anchor that reproduces $1,197
  `runs_per_cell` scales writer invocations (and overhead) **linearly**; phase-3 base
  5,400 = 300 cells × 6 models × **3 runs**, so base corresponds to runs=3.
- **Projected configs (full 6-model panel, combined = cap metric):**
  - **Config A — calibration (R=1, C=2):** ~**$304** combined, 122% of $250 cap,
    ~$154 to card (≈ the $150 credit). Recommended next dispatch.
  - **Config B — full sweep reduced cycles (R=3, C=2):** ~**$913**, 365% of cap,
    ~$763 to card.
  - **Config C — original full scope (R=3, C=3):** ~**$1,197**, 479% of cap,
    ~$1,047 to card. Reproduces the published headline (consistency check ✓ — exact
    $1,197.49).
  - Cutting C 3→2 and R 3→1 = **$1,197 → $304 (3.9× reduction)** with no model drop.
    None fit *under* the $250 combined cap; A is closest. (Half-Tools sensitivity →
    A ≈ $210.)
- **New CLI knobs** (backward-compatible; `--phase`/`--md`/`--cap` unchanged):
  - `--project-phase4` → prints Configs A/B/C + comparison table (per-role decomposed
    invocations + token-list + token-recon, Foundry Tools overhead, credit/marketplace
    split, combined, cap% + credit utilization, implied card spend).
  - `--runs N --review-cycles N` → ad-hoc config (e.g. R=2,C=2 → $608/243%).
  - `--phase` now defaults to `phase3-agentic-loop` (was required); plain
    `python3 tools/cost/estimate.py` still renders phase-3 + auto full-scope projection.
  - Refactored `project_phase4(p3, cap, runs, review_cycles, label, md)` — derives
    per-invocation token rates from the runs=3/cycles=3 anchors so token cost scales
    correctly with both R and C; overhead = total_agent_inv × $0.03375.
- **Files:** `tools/cost/estimate.py` (extended), `phases/phase4-multiagent/PLAN.md`
  (rewrote Cost projection: 1+2C / 1+1.1C math, A/B/C table; budgets table notes
  C=2/R=1 first dispatch), `phases/phase3-agentic-loop/COSTS.md` (forward projection now
  bill-calibrated, $82.19 = token-only vs ~$342.71 actual). NO Azure spend; no dispatch.

### 2026-06-10 — Cost estimator rebuilt to project the ACTUAL Azure bill (not token-only)
`tools/cost/estimate.py` modelled only per-token cost ($82.19 phase 3) while the real
May Foundry bill was ~$342 (5×). Rebuilt it to reconcile against the May anchors.

- **Two reconciliation knobs (May-calibrated, tunable constants):**
  - `TOKEN_RECON_FACTOR = 1.95` — Foundry Models $160.45 / phase-3 token-list $82.19 = 1.952.
    Slight over-attribution (phase 2 tokens also in the May 12–16 window), so true
    phase-3-only factor is bounded ~1.6–1.95×; defaulted to the upper anchor (conservative
    for a go/no-go — never under-states the bill).
  - `TOOLS_SURCHARGE_PER_CALL = $182.26 / 5,400 = $0.03375` per **agent-role invocation**.
    Foundry Tools ($182, the biggest line, previously unmodeled) is NOT token-based — it's
    the agent/tool runtime surface. Modeled it to scale with agent invocations per cell,
    not tokens. Phase 3 = 1 writer invocation/record → reproduces $182 by construction.
- **Multi-agent overhead model (phase 4):** invocations/cell = 1 writer + 1.8 reviewer +
  1.5 fixer = **4.3×** (avg-cycle assumptions from `phase4-multiagent/PLAN.md`). Overhead
  scales on the invocation count, which is why phase 4 explodes: Foundry Tools alone →
  23,220 invocations × $0.03375 = **$783**. Token (list) base $212 (PLAN itemized:
  writer $82 + reviewer $50 + fixer $80) × 1.95 = $414.
- **Billing split** in a `BILLING` dict (auditable, one-line editable): credit =
  {gpt-4.1-mini, gpt-4.1-nano, phi-4, gpt-5-codex}; marketplace = {codestral, llama, grok}
  per the user's directive. Overhead assigned wholly to the **credit** bucket (Azure-side
  agent runtime). `--cap` default 250; reports credit vs marketplace subtotals + combined +
  cap/credit utilization + implied card spend. **Azure AI Search excluded entirely.**
- **az evidence captured (free read-only `az consumption usage list`, sub authenticated):**
  Only **codestral** routes through `Microsoft.SaaS` (Codestral 25.01 paygo-inference meters).
  **llama + grok bill as "Azure Llama/Grok Models" via `Microsoft.CognitiveServices`** — the
  first-party (credit) surface. The actual May SaaS line was only **$24.22**, reconciling to
  codestral-token alone (~$19×1.27), NOT all three (~$59). So the bill contradicts the stated
  split: llama+grok likely belong in `credit`. Left the dict at the user's directive (combined
  total — the cap number — is split-independent) but flagged it loudly. Dollar amounts are
  NOT queryable via `az` on this MSDN credit sub (`pretaxCost` returns "None"); Cost Mgmt
  portal remains the only dollar source. No Azure AI Search meters appeared in the May 12–16
  window → confirms it's cleanly excludable.
- **Residual gap:** phase-3 model combined = **$342.53** vs actual **$342.71** (−$0.18, by
  construction). Remaining unmodeled: phase-2 token overlap inside the May window and
  sub-$7 Container Registry/storage. What would close it: per-day per-model dollar data
  (unavailable on this sub) to disentangle phase-2 from phase-3 in the shared window.
- **Phase-4 projection: ~$1,197 combined (479% of $250 cap; credit side $900 = 6× the $150
  credit → ~$750 card overage + $298 marketplace = ~$1,047 to card).** Even halving Foundry
  Tools (phase-2 attribution) → ~$806, still 322% of cap. **Full-scope phase 4 blows the cap
  by a wide margin — this is the real go/no-go signal.**
- **Files:** `tools/cost/estimate.py` (rebuilt), `phases/phase3-agentic-loop/COSTS.md`
  (the $82-vs-bill discussion), `phases/phase4-multiagent/PLAN.md` (multiplier reasoning).
  NO Azure spend, NO workflow dispatched.

### 2026-05-16T00:00:00Z — Team update (viz layout)
viz layout changed — see `tools/viz/README.md` and `.squad/decisions.md` (entry: 2026-05-16: tools/viz restructure). Per-plot files under `tools/viz/plots/`, shared helpers in `tools/viz/lib/`, new derived `tools/viz/data/per_model_phase.csv` from `aggregate_phase_results.py`. Four new plot families shipped.

### 2026-05-08 — MAUI removed; OpenRA + StockSharp added (Phase 2 baseline) — commit d3689e0
Removed deferred `coverage-maui` job entirely. 4 rounds of remediation hit increasingly internal MS-CI assumptions; per Brady, data didn't justify drag.

Added 2 new jobs:
- **OpenRA** (`8f2138c7`, bleed HEAD) — `net8.0`, NUnit 4 + NUnit3TestAdapter, no coverlet. Data-collector path (`--collect "Code Coverage;Format=cobertura"` + `dotnet-coverage merge`), same as abp/efcore/roslyn/runtime. Side-installs .NET 8 SDK because noble ships only .NET 10 and OpenRA has no `global.json`.
- **StockSharp** (`a26ce597`, master HEAD) — `net10.0` (via `common_target_*.props` → `NetVer=10`), MSTest 4.x, no coverlet. Per-csproj restore+build of `Tests/Tests.csproj` only. Risk flagged: references `Microsoft.Data.SqlClient` + `Ecng.Data.SqlServer` (SQL-dependent tests filterable by Category=Integration).

**Skipped PowerToys + Files** — both Windows-only at SDK/TFM level. Files mandates `net10.0-windows10.0.26100.0`; PowerToys UnitTests all in `src/modules/<windows-only>/` chains.

Active matrix: **15 repos**. Triggered runs: OpenRA=25552129165, StockSharp=25552132370.

### 2026-05-08 — StockSharp coverlet.console fix — commit d3c765d
Run 25552132370: 178-byte cobertura stub while 4239/4263 tests passed. MSTest 4.x SDK choice triggers MTP routing → data-collector silent-no-op. Swapped to canonical coverlet.console wrap. Built assembly is `StockSharp.Tests.dll` (RootNamespace from `common_target_tests.props` overrides). New run: 25556051863.

### 2026-05-08 — StockSharp flaky-filter (partial); coverlet+MTP empty-modules blocker — commits 4d07fc1, a89d57e, f82c22d
Added `FullyQualifiedName!~` exclusions for 5 flaky classes (`AsyncExtensionsTests`, `ConnectorBasketTests`, `PathsTests`, `ReportTests`, `TransactionIdStorageTests`). **Filter worked**: run 25562087626 went 0 failures / 4096 passed / 11 skipped. **But cobertura 231 bytes with empty Module table** — coverlet ran but instrumented zero modules despite ~80 dependency DLLs in `Tests/bin/Release/net10.0/`. Round 2 (run 25562607958) added `--include "[StockSharp*]*"` + `--include "[Ecng*]*"`. Self-inflicted SIGPIPE bug from `ls | head` killed step before coverlet started — reverted diag in `f82c22d`. `--include` patterns remain, unproven. Stopped at 2 attempts. Full diagnosis: decision `2026-05-08: StockSharp flaky-test filter — partial fix`.

### Earlier entries
- 2026-05-06 — Silent empty-cobertura fix (commit 7885485)
- 2026-05-07 — Containerized StaticCallAnalyzer
- 2026-05-07 — Test-discovery counter rewritten for multi-shape adapter output
- 2026-05-07 — Coverage orchestrator expanded 7→14 repos (Avalonia, duplicati, eShop, garnet, jellyfin, maui, server) + round-1/round-2 fix passes

Full text of all entries above is in `history-archive.md`.
