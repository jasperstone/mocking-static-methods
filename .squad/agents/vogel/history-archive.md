# Vogel — History Archive

Older detailed entries summarized out of `history.md` to keep it lean. Full text preserved here for reference.

---

### 2026-05-06 — Silent empty-cobertura fix (commit 7885485)

- **VSTest "Code Coverage" data collector adapter only attaches against project/sln targets.** Invoking `dotnet test <foo.dll> --collect "Code Coverage"` runs the tests but silently produces zero `Attachments:`. Always iterate `*.Tests.csproj` (not built .dll) for the per-assembly loop pattern.
- **Container-bundled SDKs may not satisfy `global.json` pins.** Roslyn pins `10.0.100-rc.2.25502.107`; the `mcr.microsoft.com/dotnet/sdk:10.0-noble` image carries `10.0.203`. Install pinned SDK via `dotnet-install.sh --jsonfile global.json --install-dir $GITHUB_WORKSPACE/.dotnet` for repos that lack their own restore.sh.
- **`|| true` on test steps + no validation = silent green.** The "real coverage" gate (file present, ≥ 5 KB, ≥ 1 `<class>` element) catches all three failure modes from run 25451789359 unambiguously. Place it BEFORE Generate HTML report; HTML can succeed against an empty stub and mask the real problem.
- **Always upload raw `TestResults/` for forensics.** `coverage-raw-<repo>` artifact lets us diagnose collector failures (missing attachments, profiler errors) post-hoc without re-running a 6-hour build.
- **`.squad/decisions/inbox/` is gitignored in this repo.** Drop files there for Scribe; they don't get committed.

### 2026-05-07 — Containerized StaticCallAnalyzer

**Problem:** `aggregate_baseline.py` invoked `dotnet StaticCallAnalyzer.dll` directly. Jasper's system `dotnet` (snap install) lacks .NET 8 runtime, breaking the local aggregation step. Same hit any collaborator without .NET 8 SDK.

**What I shipped:**
- `StaticCallAnalyzer/Dockerfile` — multi-stage SDK 8.0 build → runtime 8.0; ENTRYPOINT `dotnet /app/StaticCallAnalyzer.dll`, default CMD `/src`.
- `StaticCallAnalyzer/run.sh` — wrapper that auto-builds image `static-call-analyzer:local` if missing, then `docker run --rm -v <abs>:/src:ro …`. Build output redirected to stderr so stdout stays JSON-clean.
- `StaticCallAnalyzer/.dockerignore` — keep `bin/` and `obj/` out of the build context.

**aggregate_baseline.py fixes:**
- `run_static_analyzer()` now invokes the bash wrapper (no `dotnet` on host required).
- `main()` precheck swapped: `shutil.which("docker")` + wrapper existence; removed ANALYZER_DLL check.
- Path stripping in `aggregate_static()` now handles `/src/` mount prefix (with legacy host-path fallback).
- Headline + gap #3 are now CONDITIONAL — derived from `rows_for_md` (`Lines (total)` < 100). All 7 real this run → green headline, gap omitted.
- CI URL: derived from `git remote get-url origin`, cached. `RUN_ID` → `RUN_IDS` list, one link per run.
- Branch HEAD: `git rev-parse HEAD` at report time.
- Reproducing section updated: docker is the only host requirement besides python3 + gh.

**Gotcha:** Docker emits file paths as `/src/<...>` instead of host paths, so `aggregate_static`'s prefix-strip logic needed both prefixes. Verified all 7 repos report static-call counts > 0 (abp 126, aspnetcore 155, efcore 39, orleans 91, roslyn 117, runtime 613, semantic-kernel 38).

### 2026-05-07 — Test discovery: adapter + parser fragility

**Symptom (Run 25490696770):** test-discovery workflow reported `universe=0 filter=0 status=ok` for nearly every project. EF Core: 0/17 had any tests. Roslyn: 1/49 (3 tests). ABP: 1/78 (46 tests). Coverage runs from same SHAs execute thousands of tests in those repos — discovery is silently under-reporting, not the filter.

**Root cause (best-supported by evidence):** Legacy `_count_listed_tests` in `list_tests.sh` only matched indented FQN lines under VSTest header `"The following Tests are available:"`. The `status=ok` flag is set whenever that header appears in stdout, which is true for many shapes that do *not* follow the "indented per-line FQN" format (MTP / xunit.v3 enumerators, MSBuild minimal verbosity stripping nested logger output, multi-TFM projects where one TFM errors on `net481` under Linux). Header found → status=ok; counter sees zero indented lines → reports 0. Script captured raw output silently — emitted format un-debuggable from workflow logs.

**What I shipped:**
- `.github/scripts/list_tests.sh` — three counting heuristics (max wins): (a) indented FQNs under VSTest header (legacy); (b) `Test Name:` prefixed lines (vstest direct mode / MTP); (c) `Total tests: N` summary value (xunit.v3 / MTP discovery summary). Added `_discovery_ran` predicate that recognises empty-discovery markers ("No test is available", "Found 0 tests") so they classify as `status=ok, count=0` instead of `<error>`.
- `dotnet test` calls now include `-v normal` so MSBuild logger doesn't drop adapter-emitted enumeration lines on default `minimal`.
- Per-project raw stdout/stderr saved to `./_discovery_raw/`. New artifact `test-discovery-<repo>-raw` uploaded with 7-day retention so the next run gives us actual evidence.
- `_count_total_summary` rewritten with grep+sed (mawk-portable; the SDK container is Ubuntu Noble with mawk, which lacks gawk's 3-arg `match()`).

**Gotcha for future agents:**
- `mcr.microsoft.com/dotnet/sdk:10.0-noble` ships mawk, not gawk. Do NOT use `match($0, /re/, arr)` (gawk-only 3-arg form) in scripts that run inside this container — use grep+sed or `match() == 0` style.
- The `status=ok` heuristic in this script only proves "discovery emitted *some* recognised marker", not "discovery succeeded". If you see `status=ok` with `universe=0`, check the raw-log artifact before blaming the filter.

### 2026-05-07 — Orchestrator expansion 7→14 repos (commit 0318b56)

Added 7 jobs (Avalonia, duplicati, eShop, garnet, jellyfin, maui, server) to `.github/workflows/coverage-orchestrator.yml`. SHAs resolved from local `cloned_repos/<repo>` HEADs. Per-repo build patterns:

- **jellyfin** — `Jellyfin.sln`, all 16 test csprojs reference coverlet.collector → sln-level `dotnet test --collect:"XPlat Code Coverage"`. global.json pins `10.0.0` `latestMinor` → noble container's `10.0.203` satisfies it. Easiest of the seven.
- **garnet** — `Garnet.slnx`, NO coverlet anywhere. Tests don't follow `*.Tests.csproj` naming (`Garnet.test`, `Garnet.test.cluster`, `Garnet.fuzz`). Scoped to `test/Garnet.test/Garnet.test.csproj` only — cluster needs multi-node setup, fuzz is non-deterministic. Standard `--collect "Code Coverage;Format=cobertura"` + merge.
- **maui** — `Microsoft.Maui.sln`, only the Controls/tests/*.UnitTests subset has coverlet. Scoped find filter excludes TestCases.* (device tests). global.json pins RTM preview SDK `10.0.100-rtm.25523.113` → install via `dotnet-install.sh --jsonfile global.json` (Roslyn pattern). **Requires `dotnet workload install maui`** — pulls Android SDK ~2GB.
- **server (bitwarden)** — `bitwarden-server.sln`, all `*.Test` projects reference coverlet (21 csprojs found). global.json pins SDK `8.0.100` `latestFeature` → container's .NET 10 won't satisfy. Install .NET 8 SDK via `dotnet-install.sh --jsonfile global.json`. Local clone is bitwarden/server, NOT microsoft/sqltoolsservice.
- **eShop** — `eShop.slnx`, NO coverlet. Tests use `MSTest.Sdk` + Microsoft.Testing.Platform (exe-style entry points). Only 3 unit-test csprojs (Ordering / Basket / ClientApp). Scoped per-csproj. **Aspire is NuGet-only in .NET 10** — no workload install needed. Risk: MTP's interaction with `--collect "Code Coverage;Format=cobertura"` is untested in this stack.
- **duplicati** — `Duplicati.slnx`, only `Duplicati/UnitTest/` has coverlet (1 project). NO global.json. Build only that one csproj (Browser.Test needs Playwright; LiveTests/Backend.Tests need cloud creds — both excluded by name).
- **Avalonia** — `Avalonia.slnx`, NO coverlet. global.json sets `test.runner=Microsoft.Testing.Platform`. SDK `10.0.201` `latestFeature` → container OK. 12 `*.UnitTests.csproj` under tests/. Scoped per-csproj, excluding RenderTests/LeakTests/Designer/Browser. Same MTP-vs-collector risk as eShop.

**Gotchas captured:**
- Naming convention skew: Garnet uses `Garnet.test.csproj` (lowercase, no plural), not `*.Tests.csproj`. Always inspect test/ directory contents before applying generic `find -name "*.Tests.csproj"`.
- MTP runner + Code Coverage data collector is an untested interaction. eShop and Avalonia are the canaries. If both produce empty cobertura on first run, fix is coverlet.console (the ASP.NET Core path).
- MAUI workload install is the slowest step in the matrix (~10 min, ~2GB disk).
- Bitwarden/server is .NET 8, not .NET 10. First job in the matrix that pins to a different major SDK.

### 2026-05-07 — Orchestrator round-1 fixes (4 of 5 failures from run 25527102157)

Run 25527102157 expanded matrix to 14 repos. 9 succeeded (original 7 + jellyfin + duplicati). 5 failed; 4 had real bugs, 1 was transient.

**Surgical edits:**
- **Avalonia** (~+22/−9 lines): Replaced two slnx-level steps with one `Restore + build UnitTests projects` step doing per-csproj restore+build using same `find tests -name "*.UnitTests.csproj"` filter. The slnx graph includes `src/Android/Avalonia.Android` and `samples/ControlCatalog.{Browser,iOS}` triggering `NETSDK1147` (android / wasm-tools).
- **eShop** (~+15/−12): Dropped `Restore eShop.slnx` step; rewrote `Build unit-test projects only` to per-csproj restore+build of Ordering and Basket only. Removed `ClientApp.UnitTests` from both build and test loops (MAUI client, needs `maui-tizen` workload).
- **MAUI** (−12 lines): Deleted `Install pinned .NET SDK from global.json` step entirely. MAUI's `global.json` declares only workload manifests, no `sdk.version`, which makes `dotnet-install.sh --jsonfile` exit nonzero. Container's preinstalled 10.0.203 SDK is sufficient.
- **Server** (~+8/−2): Restore and Build commands now pass `-p:NuGetAudit=false -p:WarningsNotAsErrors=NU1902`. bitwarden-server has `TreatWarningsAsErrors=true` plus NuGet audit enabled, so MailKit 4.14.0's NU1902 moderate advisory was failing restore.
- **Garnet** (no change): Container init was blocked at MCR docker pull (sporadic egress rate-limit). No code-level remedy.

**Pattern captured:** `dotnet restore <whole.slnx>` is unsafe when the solution mixes server projects with mobile/MAUI/wasm/Tizen projects. Workload manifests are evaluated at restore time for the WHOLE graph — they are not lazy. Default to per-csproj restore + build for the specific test projects we plan to invoke.

**Bitwarden's strict warning posture is repo-policy, not bug:** They genuinely want to fail their CI on moderate CVEs. Disabling `NuGetAudit` for our job does not affect their pipeline.

**MAUI dotnet-install pattern is conditional, not universal:** Roslyn ships a real `sdk.version` and benefits from `--jsonfile` install. Server (bitwarden) ships SDK 8.0.100 latestFeature and benefits. MAUI ships only manifest declarations and breaks. Always check the `global.json` shape before assuming the Roslyn pattern transfers.

### 2026-05-07 — Orchestrator round-2 fixes (4 of 4 remaining failures)

After round-1 (commit 05b60b4): garnet recovered (transient); Avalonia, eShop, Server, MAUI still failing. Round-2 surgical edits (+135/−45 lines):

- **Avalonia** (~+50/−18): Switched to coverlet.console pattern (mirrors aspnetcore). Added `Install coverlet.console` step (`coverlet.console 6.0.2`). Test loop now invokes `coverlet "$asm" --target dotnet --targetargs "test \"$proj\" --no-build --filter ..."` per csproj, with the test DLL discovered under `<proj_dir>/bin/Debug/<tfm>/`. Restore+build step unchanged.
- **eShop** (~+30/−12): Same coverlet.console swap as Avalonia. Restore+build step unchanged (still Ordering + Basket only).
- **Server** (~+45/−22): Replaced sln-level `restore/build/test bitwarden-server.sln` with per-csproj loop over `test/*.Test.csproj` (excluding `*.IntegrationTest.csproj`, `Common.csproj`, `IntegrationTestCommon.csproj`). RustSdk is referenced ONLY by `util/Seeder/Seeder.csproj` (verified locally with `grep -l RustSdk cloned_repos/server/**/*.csproj`); scoping to test projects sidesteps it without installing Rust toolchain. Belt-and-suspenders `-p:NuGetAudit=false -p:WarningsNotAsErrors=NU1902` preserved. coverlet.collector is referenced transitively via `test/Common.csproj`, so XPlat Code Coverage works without coverlet.console here.
- **MAUI** (~+10/−8): Workload `maui` → `maui-android`. Umbrella `maui` workload manifest declares iOS+Mac SDKs ("Workload ID maui isn't supported on this platform" on Linux). `maui-android` is the Linux-supported subset that ships the same MAUI cross-platform manifests the 4 `Controls/*.UnitTests.csproj` projects need. Disk drops ~2GB → ~1.5GB.

**New patterns captured:**
- MTP + `--collect "Code Coverage;Format=cobertura"` is silent-no-op. When global.json sets MTP runner OR `MSTest.Sdk`/exe-style hosts are in play, reach for coverlet.console (mono.cecil instrumentation, runner-agnostic). The `--target dotnet --targetargs "test <proj>"` form is the canonical MTP-compatible invocation.
- `dotnet workload install maui` is Linux-incompatible. Use `maui-android` for cross-platform MAUI tests on Linux runners.
- Local-clone inspection beats CI iteration. RustSdk's tiny dependency footprint (1 csproj) was found in 2 grep commands.
- NEVER `dotnet restore <whole.sln>` for solutions that mix test/server/utility projects. Workload manifests evaluate the whole graph at restore time. Per-csproj restore of just the test projects is the safe default.

---

## Archived from history.md on 2026-06-10 (size-gate summarization)

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

**Superseded intermediate (cycles=2 exploration):** Before the cycles=1 freeze above,
the cut was framed as runs 3→1 + cycles 3→2 (Config A ≈ $304/122%, B ≈ $913, C ≈ $1,197).
The cycles=1 freeze replaced it (A run_1 ≈ $209). Per-cell multiplier model retained in
`estimate.py`: invocations/cell = `1 + reviewer×C + fixer×C`, realized `1 + 1.1·C`
(reviewer 0.6/cycle, fixer 0.5/cycle) → C1=2.1, C2=3.2, C3=4.3 (C3 anchor reproduces
$1,197); `runs_per_cell` scales writer calls + Foundry Tools overhead linearly. CLI:
`--project-phase4 --cap 250` (decomposed A/B/C table), `--runs N --review-cycles N`
(ad-hoc); `--phase` defaults to phase3-agentic-loop. Full historical detail in decisions.md
("Phase-4 cost cut via runs + review-cycles" — SUPERSEDED).

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

---

## Archived 2026-06-11T20:42:57Z (moved from history.md — size gate)

### 2026-06-10 — phase4-tripwire-250 budget created + PR #28 squad bookkeeping (full text)
- **Squad bookkeeping commit:** staged ONLY the 4 intended `.squad` paths (lewis +
  vogel history, decisions.md, deleted inbox `vogel-phase4-calibration-is-run1.md`),
  committed as **`9d07268`** ("squad: merge calibration-as-run_1 decision + Scribe
  bookkeeping") and pushed to `jasper/phase4-scaffold` (PR #28), range
  `aea5d165..9d072682`. Gotcha: `.squad/decisions/inbox/` is gitignored, so the
  deletion of the tracked inbox file was already staged from the working-tree delete
  (`D` in index) — `git add`/`git rm --cached`/`git add -u` all error on the ignored
  pathspec, but the deletion lands in the commit anyway. `.squad/log/` and
  `.squad/orchestration-log/` are **gitignored** (`git check-ignore` returns them) —
  Scribe's session/orchestration logs are NOT committed; that's the established repo
  behavior, leave them.
- **phase4-tripwire-250 created** (Azure budget = FREE control-plane op, no spend):
  scope = **subscription** (`/subscriptions/9490eefa-f2af-4485-983f-63397bfb5386`),
  same scope as phase3-tripwire-250 so it tracks total monthly spend = the combined
  $250 soft cap (marketplace + credit both count). Amount **$250 Monthly**,
  timePeriod 2026-06-01 → 2027-06-01 UTC. Notifications: **Actual 50% / 80% / 100%**
  + **Forecasted 100%**. Created via `az rest --method put` on the
  `Microsoft.Consumption/budgets` provider, api-version 2024-08-01. Verified with
  `az consumption budget show/list` (currentSpend $0).
- **Email handling:** reused the contactEmail already configured on
  **phase3-tripwire-250** (fetched via `az rest get`, 1 distinct address) — did NOT
  invent one or read `git config user.email`. Stashed to a temp file, used in the
  PUT body, then deleted the temp file. Never printed in the summary (PII).
- **Enforcement caveat:** Azure budgets **ALERT only** — they do NOT hard-stop spend.
  A true at-cap kill = wire the 100% alert → action group → webhook/runbook that
  cancels the dispatch (larger infra, NOT built here — flagged as a follow-up). The
  real hard stop remains the subscription **spending-limit toggle** (currently OFF
  for the soft-cap strategy).
- **No token/compute spend:** no generation/eval workflow dispatched, no Foundry
  model invoked. Budget creation + git only.

### 2026-06-11 — phase3-tripwire-250 budget DELETED (redundant after phase 3 sealed) (full text)
- Jasper confirmed phase 3 is done; `phase3-tripwire-250` became an exact redundant
  twin of `phase4-tripwire-250` (same subscription scope, same $250 Monthly amount,
  tracking the same total monthly spend). Deleted ONLY phase3.
- **Prior config (captured before delete, for the record):** amount **$250**,
  timeGrain **Monthly**, timePeriod **2026-05-01 → 2027-12-31 UTC**, currentSpend
  **$6.27**, notifications **Actual 50% / 75% / 90% + Forecasted 100%**. (Note: phase3
  used 50/75/90 thresholds; phase4 uses 50/80/100 — they were NOT identical on
  thresholds, only on scope/amount/spend-tracking, which is what made phase3
  redundant once phase 3 was sealed.)
- Deleted via `az consumption budget delete --budget-name phase3-tripwire-250`
  (subscription scope is the `az consumption budget` default; EXIT=0, no explicit
  scope or `az rest` fallback needed).
- **Remaining 3-budget set (verified via `az consumption budget list`):**
  `VS_Credit_Budget` ($150, BillingMonth), `budget-mockstatic-50` ($50, Monthly),
  `phase4-tripwire-250` ($250, Monthly). All intact, none touched.
- Did NOT alter phase4 thresholds (a possible tweak is pending Jasper, separate task).
- **No token/compute spend:** budget delete is a FREE control-plane op; no workflow
  dispatched, no Foundry model invoked.

### 2026-06-11 — Created .github/workflows/phase4-refactoring.yml (phase-4 generate workflow) [archived from history.md 2026-06-11]
- New phase-4 generate workflow, modeled on `phase5-generate.yml` (the most complete:
  mock|foundry mode switch, mock smoke job, foundry guard + spend gate + freeze
  confirmation) but adapted for phase 4 = **SINGLE-AGENT writer + LOCAL apply_refactor
  tool** (NOT multi-agent). `name:` = "Phase 4 — generate (agentic loop + refactoring
  tool)"; `env: PHASE: phase4-refactoring`.
- **Mode switch:** `mode` input (mock|foundry, default **mock** so a stray UI click
  can't spend). workflow_dispatch ONLY — no schedule/push/PR triggers.
- **`smoke` job (mode==mock):** sets up Python 3.12 + .NET 10 SDK, installs pytest, runs
  `pytest tools/generation/tests/test_refactor_smoke.py` (the test Beck builds in
  parallel), prints runner `--help`, then a **best-effort** mock-runner shakedown that
  invokes the runner with `--mock-llm --mock-fixtures-dir tools/generation/tests/fixtures/refactor`
  — guarded to skip cleanly if the fixtures dir isn't present yet (parallel dev).
- **`guard-foundry` job (mode==foundry):** mirrors phase5 — requires
  `i_understand_this_will_spend_money=yes` + `confirm_after_freeze=yes` + date ≥
  2026-06-08, and a spend-gate step that REUSES the EXISTING Azure budget
  **`phase4-tripwire-250`** ($250 Monthly, subscription scope) — did NOT invent a new
  budget noun. Prints the run_1 (~$214/85%) vs full 3-run (~$641/257%) framing.
- **`plan` + `generate` jobs (mode==foundry):** `plan` reuses `.github/scripts/plan_matrix.py`
  (same 6-model panel resolution as phase3/phase5) + target sha256 integrity gate;
  `generate` runs inside `mcr.microsoft.com/dotnet/sdk:10.0-noble` (refactor tool
  recompiles the owning csproj in-loop), clones the pinned-SHA repo, and invokes
  **`tools/generation/agentic_refactor_runner.py`** (NOT multi_agent_runner, NOT the
  phase-3 runner).
- **Runner flag cross-check (read its argparse):** input `max_compile_attempts` →
  `--max-attempts`; added `--max-refactors 3` (phase-4-specific), `--refactor-build-timeout-s 240`,
  `--repo-filter`, and the real-Foundry safety gate `--i-understand-this-will-spend-money`
  (the exact flag name in the runner — hyphenated, NOT `--spend-gate`). Mock flags:
  `--mock-llm`, `--mock-fixtures-dir`, `--out-dir`.
- **VERIFIED YAML valid:** `python3 -c "import yaml; yaml.safe_load(open('.github/workflows/phase4-refactoring.yml')); print('YAML OK')"` → `YAML OK`.
- Gotcha avoided: used `dotnet --info | sed -n '1,10p'` (NOT `| head`) inside the SDK
  container to dodge the known `bash -e -o pipefail` + `head` SIGPIPE step death.
- Decision dropped to `.squad/decisions/inbox/vogel-phase4-workflow.md`.

### 2026-06-11 — Phase-4 (agentic loop + refactoring tool) cost model added to estimate.py [archived from history.md 2026-06-11]
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
