# Squad Decisions

## Active Decisions

### 2026-05-16: tools/viz restructure — per-plot files + new chart families
**By:** Beck (requested by Brady)

**What:** Split `tools/viz/render_phase3.R` into one file per plot under `tools/viz/plots/`, with shared helpers in `tools/viz/lib/`. Added four new plot families (`successful_tests_progression`, `coverage_baseline`, `cost_efficiency`, `cost_per_passing_test`). Added `tools/viz/aggregate_phase_results.py` producing `tools/viz/data/per_model_phase.csv` from raw jsonl. `render_phase3.R` retained as a one-line back-compat shim.

**Why:** Make plot files individually editable without scrolling past unrelated charts. Add cost and progression views that didn't exist.

**Conventions established (team-relevant):**
1. **"Data duplication" defined:** copying a source-controlled file into `tools/viz/data/` is duplication and forbidden. Producing a NEW aggregated CSV (e.g. `per_model_phase.csv`) that doesn't exist anywhere else is NOT duplication — it's a derived artifact and is allowed.
2. **Inputs read in place:** `baseline_coverage.csv` is read directly from the repo root by `lib/load.R::load_baseline_coverage()`. Other root-level CSVs should follow this pattern.
3. **Single price table:** all cost calculations reuse `PRICES` from `tools/cost/estimate.py`. No fork. New scripts import via `from tools.cost.estimate import PRICES`.
4. **Phase 3 cost is currently unavailable** — `phases/phase3-agentic-loop/results/` top level is empty, so per-attempt token counts don't exist. Aggregator emits blank `cost_usd` for phase 3 rows; cost-based plots filter them out. When phase 3 jsonl lands, add `phase3-agentic-loop` to `RAW_PHASES`.
5. **Phase 2 totals come from inclusive `results*/**/attempts.jsonl` glob** to match published `phases/phase2-agentic/COSTS.md` (6,307 attempts, $89.98). Filtering `results_v1_oldprompt/` would drop 7 attempts / ~$0.11 and silently disagree.
6. **ggrepel is not installed** in the main devcontainer. Plot files must not depend on it.

**Files affected:**
- New: `tools/viz/render_all.R`, `tools/viz/aggregate_phase_results.py`, `tools/viz/lib/{load,theme}.R`, `tools/viz/plots/*.R` (7 plot files), `tools/viz/data/per_model_phase.csv`.
- Modified: `tools/viz/README.md` (new layout, devcontainer note).
- Replaced: `tools/viz/render_phase3.R` is now a back-compat shim that sources `render_all.R`.
- New PNGs: `progression-runok.png`, `coverage-baseline.png`, `cost-efficiency.png`, `cost-per-passing-test.png`. Existing phase-3 PNGs regenerate identical visual output.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

### 2026-06-10: Cost estimator models the actual Azure bill (not token-only)

**By:** Vogel (CI/CD), requested by Jasper

**What:** Rebuilt `tools/cost/estimate.py` so its output approximates the real Azure
Foundry bill instead of token-only model cost. The old estimator reported $82.19 for
phase 3; the actual May Foundry bill was ~$342 (≈5×). The new estimator lands phase 3 at
$342.53 (vs actual $342.71).

**What the new estimator models:**
- Keeps per-token list `PRICES`, but adds two May-calibrated, tunable reconciliation knobs:
  - `TOKEN_RECON_FACTOR = 1.95` — Foundry Models meter undercounts list prices ~2×
    (May Foundry Models $160.45 / phase-3 token-list $82.19). Conservative upper anchor;
    true phase-3-only factor is ~1.6–1.95× because phase-2 tokens also fell in the window.
  - **Foundry Tools / agent-runtime overhead** `$182.26 / 5,400 = $0.03375` per
    *agent-role invocation*. This is the biggest May line ($182) and is NOT token-based —
    it scales with agent/tool calls. Modeled to scale on invocations per cell so phase 4
    (multi-agent) projects correctly. Overhead is assigned wholly to the **credit** bucket
    (Azure-side agent runtime).
- Per-model + total decomposition: token (list), token (recon), Tools overhead, total.
- Two subtotals — **credit-billed vs marketplace-billed** — plus a **combined total**, with
  utilization against `--cap` (default 250) and the $150 monthly credit, and implied card spend.

**Billing split (the marketplace-counts-toward-cap rule):** Driven by an auditable `BILLING`
dict. Credit-eligible (draws the $150 credit): gpt-4.1-mini, gpt-4.1-nano, phi-4, gpt-5-codex.
Marketplace SaaS (card-billed, does NOT draw the credit but **DOES count toward the combined
cap** per Jasper): codestral-2501, llama-3.3-70b-instruct, grok-4-1-fast. The combined total —
the number the cap measures — is independent of where the credit/marketplace boundary sits.

**Azure AI Search EXCLUDED:** The $25.96 May Search line is not modeled anywhere. The resource
was torn down and is unrelated to the experiment. Confirmed via free read-only az query that no
Search meters appear in the May 12–16 experiment window.

**Caveat captured (az evidence, free read-only `az consumption usage list`):** Only codestral
actually routed to the card (`Microsoft.SaaS`, Codestral paygo-inference meters). llama and grok
billed as "Azure Llama/Grok Models" via `Microsoft.CognitiveServices` — the first-party (credit)
surface. The actual May SaaS line ($24.22) reconciles to codestral-token alone, not all three
(~$59). If the bill is the authority, llama+grok belong in `credit`; flipping them is a one-line
edit and does not change the combined/cap result. Dollar amounts are not queryable via az on this
MSDN credit subscription (`pretaxCost` = "None") — Cost Management portal remains the dollar source.

**Phase-4 projection (the go/no-go signal):** Same 300-cell v2 × 6-model panel × 3 runs ×
max_review_cycles=3. Agent invocations/cell = 1 writer + 1.8 reviewer + 1.5 fixer = 4.3×.
Projected combined ≈ **$1,197 (479% of the $250 cap)**; credit side ≈ $900 (6× the $150 credit →
~$750 card overage) + ~$298 marketplace ≈ **$1,047 to the card**. Even halving Foundry Tools
(phase-2 attribution) leaves ~$806 (322% of cap). **Full-scope phase 4 blows the cap; scope or
staging must be reconsidered before any dispatch.**

**No Azure spend incurred. No workflow dispatched.** This was a tooling/accuracy change only.

**Validation:** `python3 tools/cost/estimate.py --phase phase3-agentic-loop --md` runs clean,
prints the decomposed table + subtotals + cap utilization; phase-3 combined = $342.53
(residual −$0.18 vs actual). Plain output, `--cap`, and `--md` all preserved.

### 2026-06-10: Phase-4 cost cut via runs + review-cycles, NOT model drop  *(SUPERSEDED)*

> **Superseded by the 2026-06-10 "Phase-4 calibration is run_1 of the frozen design" entry
> below.** This entry's cycles=2 framing (Config A ≈ $304) was the intermediate exploration;
> the final frozen design uses `max_review_cycles = 1` (Config A run_1 ≈ $209). Retained for
> the audit trail of how the design converged. Numbers below are historical.

**By:** Vogel (CI/CD), requested by Jasper

**What:** Full-scope phase 4 (300-cell v2 × full 6-model panel × runs_per_cell=3 ×
max_review_cycles=3) projects ~**$1,197 combined = 479% of the $250 cap**. Jasper's
decision: preserve the cross-model comparison (**keep the full 6-model panel — never
drop models**) and cut cost via **runs (3→1) and review cycles (3→2)** instead. Extended
`tools/cost/estimate.py` with a reproducible, bill-calibrated phase-4 projection
parametrized by runs_per_cell and max_review_cycles.

**The multiplier math (documented inline in PLAN.md):** per-cell agent invocations =
`1 writer + reviewer×cycles + fixer×cycles`. Theoretical max = `1 + 2·C`; realized =
`1 + 1.1·C` using May-calibrated per-cycle rates (reviewer 0.6/cycle, fixer 0.5/cycle):
cycles=1 → 2.1, cycles=2 → 3.2, cycles=3 → 4.3 (the cycles=3 anchor reproduces $1,197).
`runs_per_cell` scales writer invocations — and the dominant Foundry Tools overhead —
**linearly** (phase-3 base 5,400 writer calls = 300×6×3 = runs=3).

**Projected configs at this stage (cycles=2 framing — historical):**

| Config | runs | cycles | Combined | % of $250 cap | To card |
|---|---:|---:|---:|---:|---:|
| A — calibration | 1 | 2 | ~$304 | 122% | ~$154 |
| B — full sweep, reduced cycles | 3 | 2 | ~$913 | 365% | ~$763 |
| C — original full scope (reference) | 3 | 3 | ~$1,197 | 479% | ~$1,047 |

Consistency check: the estimator's cycles=3 / runs=3 path reproduces $1,197.49 exactly.
**No Azure spend incurred. No workflow dispatched.** Estimator-only.

**Files:** `tools/cost/estimate.py`, `phases/phase4-multiagent/PLAN.md`,
`phases/phase3-agentic-loop/COSTS.md`.

### 2026-06-10: Phase-4 calibration is run_1 of the frozen design (not a throwaway)

**By:** Vogel (CI/CD), requested by Jasper

**What:** Reframe the phase-4 calibration pass as **run_1 of the real experiment**
so the calibration work and spend are not repeated. The phase-4 config is **frozen
now** (sealed at one SHA before run_1) and does not change after calibration.

**Frozen phase-4 config (decided now — do not touch after calibration):**
- `max_review_cycles = 1` — down from the original 3. Jasper's decision: the
  multi-agent tool overhead (Foundry Tools / agent-runtime invocations) is the
  dominant cost driver; cycles=1 minimizes it while still exercising the
  writer → reviewer → fixer loop once.
- `runs_per_cell = 3` total target, dispatched incrementally as **run_1
  (calibration) → go/no-go → runs 2+3**.
- **Full 6-model panel, no models dropped** (codestral-2501, gpt-4.1-mini,
  gpt-4.1-nano, grok-4-1-fast, llama-3.3-70b-instruct, phi-4). Keeping the panel
  full preserves the cross-model comparison.
- Per-cell params unchanged from phases 2/3: temperature 0.0, top_p 1.0, seed 42,
  max_output_tokens 4096.

**Calibration = run_1 (the framing change):** The calibration is dispatched early
to get the first real multi-agent cost + run-OK data point, then **pooled into the
final result set** as run_1 of the 3-run design. This supersedes the earlier
"Config A calibration is a separate ~$150 spend" framing — calibration is now
run_1, i.e. spend we would make anyway, with a go/no-go checkpoint attached.

**Reusability condition (the discipline):** run_1 is poolable with runs 2+3 **only
if the harness, prompts, and config are frozen at one SHA and nothing changes after
calibration.** Any prompt edit, cycle-count change, or model swap after calibration
invalidates run_1 as a member of the 3-run set and forces a re-run.

**Sequence:** smoke test (1 cell, correctness, <$0.10) → **run_1 = calibration** on
the sealed harness (real adapter, full panel, cycles=1) → measure actual bill across
all meters → go/no-go vs the soft $150–250 combined cap → dispatch **runs 2+3** with
identical config.

**Why prior-phase data can't substitute:** phases 2/3 were single-agent; phase 4's
reviewer+fixer tool traffic (the $182 May "Foundry Tools" line) has never been
directly measured. Calibration replaces the *derived* overhead factor in
`tools/cost/estimate.py` with a *measured* one.

**Bill-calibrated projections (full 6-model panel; combined = cap metric; reproduce
with `python3 tools/cost/estimate.py --project-phase4 --cap 250`):**

| Config | runs | cycles | Combined | % of $250 cap | To card |
|---|---:|---:|---:|---:|---:|
| **A — run_1 calibration** (first dispatch) | 1 | 1 | **~$209** | **84% (under cap)** | ~$59 |
| **B — full 3-run set** (runs 2+3 after go/no-go) | 3 | 1 | **~$628** | 251% | ~$478 |
| **C — original full scope** (reference, pre-freeze) | 3 | 3 | **~$1,197** | 479% | ~$1,047 |

Freezing cycles at 1 brings the **calibration** (run_1) under the $250 combined cap
(~$209, ~$59 to card — inside the $150 credit), a clean go. The pooled full 3-run
set (~$628) is the real go/no-go after run_1's measured bill lands.

**No Azure spend incurred. No workflow dispatched.** Estimator config and PLAN docs
only; dispatch is gated on the smoke test + run_1 go/no-go after the ~Jun 11 credit
reset.

**Files:** `tools/cost/estimate.py` (named configs A/B aligned to frozen cycles=1),
`phases/phase4-multiagent/PLAN.md` (budgets table cycles=1 frozen + runs_per_cell
run_1 framing; cost projection table + calibration-as-run_1 section).

**Git provenance:** Committed to branch `jasper/phase4-scaffold` as commit `aea5d165`,
pushed to origin, recorded on open **PR #28**
(https://github.com/jasperstone/mocking-static-methods/pull/28). PLAN.md exists only on
the scaffold branch, so the cost-calibration work legitimately rides PR #28 rather than a
fresh branch off `main`. (See orchestration log for the git-discipline process note.)

### 2026-06-10: phase4-tripwire-250 Azure budget created (combined soft cap = $250, alert-only)

**By:** Vogel (CI/CD), requested by Jasper

**What:** Created the Azure budget `phase4-tripwire-250` referenced by
`phases/phase4-multiagent/phase.lock.yaml` and `REPLICATION.md §2`. Budget creation is a
FREE control-plane operation — no token/compute spend.

- **Scope:** subscription (`/subscriptions/9490eefa-f2af-4485-983f-63397bfb5386`) — the
  same scope as `phase3-tripwire-250`. Subscription scope tracks **total monthly spend**,
  which is exactly the combined soft cap: both marketplace (card) and credit spend count
  toward it.
- **Amount / grain:** $250, Monthly. timePeriod 2026-06-01 → 2027-06-01 UTC.
- **Notifications:** Actual **50% ($125) / 80% ($200) / 100% ($250)** + a **Forecasted 100%** alert.
- **Email:** reused the contactEmail already configured on `phase3-tripwire-250` (retrieved
  via `az rest get`). No email invented; `git config user.email` not read.

**Why combined cap = $250:** per Jasper's decision, marketplace + credit spend both count
toward one soft cap. A single subscription-scoped budget at $250 measures the combined total
directly (split-independent), matching the cost estimator's "combined = cap metric" model.

**Enforcement caveat (important):** Azure budgets **ALERT only** — they do NOT hard-stop spend.
A true at-cap kill requires wiring the 100% alert → action group → webhook/runbook that cancels
the dispatch (larger infra, NOT built — proposed as a follow-up). The real hard stop remains the
subscription **spending-limit toggle**, currently OFF to allow the soft-cap (credit-overage) strategy.

**Git provenance:** also committed the outstanding `.squad` bookkeeping (calibration-as-run_1
decision merge + cross-agent history) as commit `9d07268` on `jasper/phase4-scaffold` (PR #28),
pushed to origin.

**No Azure spend. No generation/eval workflow dispatched. No Foundry model invoked.**

### 2026-06-11: Budget cleanup — phase3-tripwire-250 deleted; phase4-tripwire-250 held at $250 (no $150 card-begins threshold)

**By:** Vogel (CI/CD) + Coordinator, requested by Jasper

**This is an UPDATE/refinement of the 2026-06-10 `phase4-tripwire-250` creation entry above
(that entry stands as-is), plus the deletion of its redundant phase-3 twin.**

**1 — phase3-tripwire-250 deleted (redundant after phase 3 sealed).** Deleted the Azure budget
`phase3-tripwire-250` from subscription `VisualStudioSubscription`
(`9490eefa-f2af-4485-983f-63397bfb5386`). Budget deletion is a FREE control-plane operation —
no token/compute spend. With phase 3 done/sealed, `phase3-tripwire-250` became a redundant twin
of `phase4-tripwire-250` — same subscription scope, same $250 Monthly amount, tracking the same
total monthly spend. The combined soft-cap tripwire is now solely `phase4-tripwire-250`.
- **Prior config (captured before delete):** amount $250, timeGrain Monthly, timePeriod
  2026-05-01 → 2027-12-31 UTC, currentSpend $6.27 at delete time, notifications Actual
  50% / 75% / 90% + Forecasted 100%. (Thresholds differed from phase4's 50/80/100; the
  redundancy was on scope + amount + spend-tracking, not on alert thresholds.)
- **How:** `az consumption budget delete --budget-name phase3-tripwire-250` (subscription scope
  is the `az consumption budget` default; exit 0, no `az rest` fallback needed).

**2 — phase4-tripwire-250 held at $250; NO "$150 card-begins" threshold added.** The combined
soft cap stays $250 = $150 monthly credit + $100 card overage. We deliberately did **not** add a
"$150 card-begins" alert threshold, because marketplace models (codestral-2501,
llama-3.3-70b-instruct, grok-4-1-fast) **always bill the card and never draw the credit** — so
$150 is not a meaningful credit-exhausted / card-begins boundary. phase4-tripwire-250 thresholds
remain unchanged (Actual 50% / 80% / 100% + Forecasted 100%).

**Net budget state now — three budgets (verified via `az consumption budget list`):**
- `VS_Credit_Budget` — $150, BillingMonth — KEPT
- `budget-mockstatic-50` — $50, Monthly (RG scope) — KEPT, untouched
- `phase4-tripwire-250` — $250, Monthly — KEPT (surviving combined soft-cap tripwire)

**No Azure spend incurred. No workflow dispatched. No Foundry model invoked.**

### 2026-06-11: Phase ladder renumbered in the reports (forward-looking roadmap labels only)

**By:** Lewis (Lead), requested by Jasper (autopilot)

**What:** Brought the stale forward-looking roadmap narrative in the published reports into line with the renumbered canonical phase ladder. Only FORWARD-LOOKING labels/projections changed; measured phase-2/phase-3 RESULTS numbers (compile/run rates, costs) were left untouched.

**New canonical ladder:**
- Phase 1 = baseline coverage
- Phase 2 = agentic, no feedback
- Phase 3 = agentic loop (compile+run feedback) — shipped, 14.6% compile / 7.1% run-OK
- **Phase 4 = agentic loop + testability refactoring tool** (NEW; `apply_refactor` introduces a testability seam — extract-and-override / wrapper-interface-adapter / dependency-parameterization — into production code before testing; prompts stay generic; isolates the effect of a refactoring *capability* on the fixed input set)
- **Phase 5 = multi-agent (writer + reviewer + fixer)** — moved here from old phase 4
- **multi-team — DROPPED entirely**

**Files changed (reports only):**
- `phases/phase2-agentic/REPORT.md` — "## Next tiers" list rewritten (phase 4 = refactoring tool, phase 5 = multi-agent, multi-team removed); surrounding prose ("same 300 v2 cells, same 6-model panel, codex removed") preserved.
- `phases/phase2-agentic/COSTS.md` — item 3 "Budget headroom" parenthetical → "(agentic loop with compile feedback, refactoring tool, multi-agent)"; projection table relabeled — Phase 4 = Agentic loop + refactoring tool (2-3×, ~$33-50, kept modest: one LLM agent + a local tool), Phase 5 = Multi-agent (writer + reviewer + fixer) (4-6×, ~$67-100); "Remaining phases total" → ~$133-200 for internal consistency. Table left explicitly ROUGH (precise phase-5 model lives in `tools/cost/estimate.py`).
- `phases/phase3-agentic-loop/REPORT.md` — no-[Fact] closer "Phase 4" → "Phase 5" (reviewer behavior, not refactoring); runner/production-csproj parity "Phase 4 may close" → "A later phase may close" (sandbox parity, phase-agnostic); "## Next" heading + variance open-question phase labels updated (multi-agent worth-the-cost → phase 5; phase 4 reframed as immediate next refactoring-tool step, phase 5 multi-agent follows).

**Deliberately NOT touched (naming mismatch left in place on purpose):**
- Azure budget proper noun `phase4-tripwire-250` (preserve verbatim).
- `.squad/` decisions + history that reference old phase4=multi-agent (append-only; not retroactively edited).
- `phases/phase4-multiagent/` and `phases/phase5-multiagent/` directory names and their internal docs (out of scope for this edit). `phases/README.md` already reads multi-agent = phase 5, which happens to be consistent with the new ladder.

**Why:** Jasper renumbered the ladder ("update the report, it's not final yet"). The reports were the public-facing forward-looking narrative still describing the OLD roadmap.

**Follow-up flagged (not done):** internal `.squad` decision history and the `phases/phase4-multiagent/` directory still encode the OLD numbering (phase4=multi-agent). If/when the ladder is finalized, a future pass should reconcile directory names and the phase-4 cost-calibration decisions, keeping `phase4-tripwire-250` as a fixed proper noun.

**No Azure spend. No workflow dispatched. Documentation edits only.**

### 2026-06-11: Phase-4 (agentic loop + refactoring tool) cost model added to estimate.py

**By:** Vogel (CI/CD), requested by Jasper (autopilot)

**What:** Added a phase-4 cost projection to `tools/cost/estimate.py` and wired
`--project-phase4` (the flag freed up by the phase-4→phase-5 multi-agent rename).
NEW phase 4 = the SAME single writer agent as phase 3 PLUS a LOCAL `apply_refactor`
tool (no LLM behind it) that introduces a testability seam (extract-and-override,
wrapper-interface/adapter, dependency-parameterization) in production source before
the test is written and the owning csproj is recompiled by the existing compile_only
harness.

**Modeling choices (the defensible model, documented in the `project_phase4`
docstring; cites `phases/phase4-refactoring/PLAN.md`):**
- **Exactly ONE LLM role (writer).** No reviewer, no fixer LLM — so unlike phase 5
  there is no second/third model role multiplying token spend. This is the single
  biggest reason phase 4 is far cheaper than phase 5.
- **Token inflation, not an extra agent.** `P4R_TOKEN_INFLATION = 1.5` — a flat
  multiplier on the phase-3 writer token base. Refactoring makes the writer take
  more turns per cell (inspect target → choose seam → call apply_refactor → read
  result → write/iterate test), so it emits ~50% more tokens per cell. Modest
  (range ~1.4–1.6), not a whole extra agent.
- **apply_refactor adds to invocation-scaled Foundry Tools overhead.**
  `P4R_REFACTOR_CALLS_PER_CELL = 1.2` apply_refactor calls per cell (≈ one seam per
  cell, occasional second), billed at the EXISTING `TOOLS_SURCHARGE_PER_CALL`
  ($0.03375/invocation) exactly like read_file/list_dir. The tool is local
  (zero-token) but the agent-runtime/tool surface still bills.
- **Billing split unchanged in convention:** token spend keeps the phase-3
  marketplace fraction; Foundry Tools overhead is wholly credit (same as
  `project_phase5`).
- **Default dispatch = run_1 (`P4R_DEFAULT_RUNS = 1`), the go/no-go,** mirroring the
  frozen phase-5 run_1 design. NOTE the modeling tension: the phase-3 combined base
  alone is ~$342 (> the $250 cap), so a full 3-run phase-4 sweep CANNOT be under cap.
  Defaulting to run_1 is the honest resolution — it is the dispatch you actually run
  first, and it lands under cap.

**Projected numbers:**
- **Phase 4 run_1 (runs=1) = $213.79 combined → 85.5% of the $250 cap, UNDER by
  $36.21** (credit $156.13 / marketplace $57.67; ~$63.79 to card). Clean go.
- Same-runs phase-3 single-writer baseline = $114.18, so phase 4 run_1 is ~1.87× the
  phase-3 base — modestly above, as expected for added refactoring turns + tool calls.
- For context, the full 3-run set (runs=3) projects ~$641 (257% of cap) — over cap
  but ~54% of phase 5's $1,197, i.e. roughly half. The single-LLM-role design is the
  whole reason it is far below the multi-agent phase 5.

**Implementation:** `P4R_*` constants near the `P5_*` block; `def project_phase4(p3,
cap, runs, refactor_calls, inflation, label, md)` mirroring `project_phase5`'s
structure (per-role table, Foundry Tools/Models breakdown, credit/marketplace split,
cap + $150-credit utilization, printed sanity check). `--project-phase4` and
`--refactor-calls` wired in `main()`; `--runs` now shared across phase-4/phase-5
ad-hoc projections. Normal runs auto-print the phase-4 go/no-go block alongside the
existing auto phase-5 full-scope projection.

**Nothing existing was renamed or broken:** `--project-phase5` / `project_phase5` /
`P5_*` / `P3_RUNS_PER_CELL` / `FOUNDRY_*` / `TOKEN_RECON_FACTOR` / `CREDIT_USD` are
all untouched. Verified: `--project-phase4 --cap 250` (EXIT 0, $213.79),
`--project-phase5 --cap 250` (EXIT 0, Config C still $1,197.49), normal run (EXIT 0,
phase-3 residual still −$0.18).

**No Azure spend. Estimator-only change.**

### 2026-06-11: Phase-4 (agentic loop + refactoring tool) experiment directory scaffolded + design-of-record

**By:** Lewis (Lead), requested by Jasper (autopilot)

**What:** Created the NEW `phases/phase4-refactoring/` experiment directory on branch
`jasper/phase4-refactoring`, mirroring the phase-5 layout, and authored the authoritative
design-of-record. Watney built the `apply_refactor` tool + runner to this same spec in parallel.

**The authoritative design:**

- **Goal:** measure the effect of giving the proven phase-3 single writer agent a *refactoring
  capability*. The agent may introduce a testability seam into PRODUCTION code so a Mode #1 static
  call site becomes mockable (Moq/NSubstitute), then write a test exploiting the seam. Same frozen
  v2 300-cell target set, same 6-model panel, same compile/run harness as phases 2–3. **Headline =
  run-OK% A/B vs phase-3's 7.1% on the identical cells.** The contribution is capability/tooling
  augmentation, NOT prompt engineering — the writer prompt stays generic; the only new degree of
  freedom is `apply_refactor`.

- **Mode #1 sites** = (1) extension methods on interface receivers (EXT); (2) non-virtual instance
  methods on non-sealed concrete classes (NonVirtual). Neither is directly mockable — the refactor
  introduces the seam.

- **`apply_refactor` — constrained transform menu** (local tool, no LLM; the bounded menu IS the
  anti-gaming mechanism, NOT free-form prod editing):
  1. `make_virtual` — NonVirtual kind: mark the target method `virtual` (extract-and-override seam).
  2. `wrapper_interface` (extract-and-adapter) — generate an adapter interface + thin wrapper around
     the receiver; consumer depends on the interface (ctor-injected); tests substitute a mock.
  3. `parameterize_dependency` — inject the dependency via a NEW defaulted overload that preserves
     the existing public API (no breaking change).

- **Anti-gaming rules:** refactor must NOT delete/disable/no-op the target site and must not change
  observable behavior; `parameterize_dependency` keeps a default-preserving overload; all edits
  confined to the owning `.csproj` subtree. **Behavior-preservation guard:** after a refactor, the
  owning production project MUST still build, and if it has an associated existing test project those
  tests MUST still pass — else the refactor is auto-reverted and recorded `refactor_rejected`.
  **Legitimate pass** = the test exercises the target via the seam and asserts on real behavior (no
  trivial assert, no bypass of the site).

- **Per-cell lifecycle:** snapshot-on-write (lazy) of any prod file before its first edit → agent
  loop (read_file / list_dir / apply_refactor / submit_test; apply_refactor edits prod source and the
  next compile+run rebuilds the single owning csproj from source for free) → UNCONDITIONAL restore of
  all touched files to pristine state after the cell (cells never contaminate each other) → log
  applied refactors + guard outcomes per cell.

- **Metrics:** run-OK% vs phase 3; a **refactor-attributable** breakdown (cells that pass ONLY when a
  legitimate refactor was applied — run-fail in phase 3, run-OK in phase 4 through a seam); the
  `refactor_rejected` rate; and which transform types succeed by Mode #1 kind.

- **Cost:** one writer LLM + one local tool — no reviewer/fixer LLM, so far cheaper than phase 5.
  run_1 (runs=1) ≈ **$214 combined / ~85% of the $250 cap — clean go** (`tools/cost/estimate.py
  --project-phase4`). Full 3-run ≈ $641 (over cap, ~half of phase-5's $1,197). run_1 is the honest
  default dispatch + go/no-go.

**Files created:** `phases/phase4-refactoring/{PLAN.md, REPLICATION.md, REPORT.md, phase.lock.yaml,
prompt/writer-system.md, prompt/user-template.md, results/.gitkeep}`.

**Consistency choices:**
- `phase.lock.yaml` copies phase-5's EXACT `targets_sha256` (`4db523f966ff…2823`) + `targets_count`
  300, the same `infrastructure`, the same `repos` SHA pins, and the same 6-model panel. Adds a
  `refactoring:` section (transform menu + guard + anti-gaming + legitimate-pass + lifecycle) and
  `agent_topology: single_writer_refactor`; `runs_per_model: 1` (run_1). seal_date / git_tag /
  digests left blank as phase 5 does.
- REPLICATION reuses the EXISTING Azure budget noun `phase4-tripwire-250` (did NOT invent a new
  budget) and references the `phase4-refactoring.yml` workflow Vogel is stubbing.
- Single-agent → NO reviewer/fixer prompts (unlike phase 5). Only `writer-system.md` (writer who also
  wields apply_refactor) + `user-template.md` (phase-3 interpolation vars plus TEST_FRAMEWORK /
  TARGET_TFM).
- REPORT.md is explicitly marked predictions-only ("will be filled once phase 4 has N runs"), with a
  Phase 3 vs Phase 4 predicted table + per-bucket conversion prediction (unmockable Mode #1 converts;
  no_fact_methods is NOT a phase-4 target — that's phase 5's reviewer).

**Tool paths Watney owns (referenced from PLAN):** `tools/generation/apply_refactor.py`,
`tools/generation/strategies/agentic_loop_refactor.py`, `tools/generation/agentic_refactor_runner.py`.

**No Azure spend. No workflow dispatched. No Foundry model invoked.** Scaffold + docs only.

### 2026-06-11: Phase-4 apply_refactor tool + strategy + runner (the seam-introducing build)

**By:** Watney (Build/Infra), requested by Jasper (autopilot)

**What:** Built the phase-4 "agentic loop + testability refactoring" stack on branch
`jasper/phase4-refactoring`. Phase 4 = the phase-3 single-agent compile+run feedback loop
PLUS an `apply_refactor` tool that edits PRODUCTION source to introduce a testability seam,
so a Mode #1 static call site becomes mockable. `compile_and_run_check` already rebuilds the
owning csproj from source, so seam edits are picked up on the next `submit_test`.

**New files:**
- `tools/generation/apply_refactor.py` — `RefactorEngine` + `RefactorResult`.
- `tools/generation/strategies/agentic_loop_refactor.py` — phase-4 strategy; `RefactorLoopResult(FeedbackLoopResult)` adds `refactor_attempts`; `parse_refactor_args()`.
- `tools/generation/agentic_refactor_runner.py` — phase-4 runner (mirrors `agentic_runner_feedback.py`; adds `--mock-llm/--mock-fixtures-dir/--out-dir` + `--i-understand-this-will-spend-money` from the phase-5 runner). Default `--phase phase4-refactoring`, `--target-set v2`.

**apply_refactor tool-call syntax (PROMPTS + SMOKE-TEST FIXTURE MUST MATCH):**
```
<tool>apply_refactor(transform=make_virtual)</tool>          (primary)
<tool>apply_refactor(make_virtual)</tool>                    (bare)
<tool>apply_refactor(transform=make_virtual, method=GetAsync)</tool>   (extra kwargs)
<tool>apply_refactor({"transform": "wrapper_interface", "interface_name": "IFoo"})</tool>   (json for extra args)
```
Parsed by `parse_refactor_args(raw) -> (transform, kwargs)`. read_file/list_dir/submit_test keep
the EXACT phase-3 `TOOL_RE` protocol unchanged; apply_refactor uses its own `APPLY_REFACTOR_RE`
and is preferred when it appears first. Per-cell budget: `--max-refactors` (default 3).

**Transform menu (the constraint IS the anti-gaming mechanism):**
1. `make_virtual` — **IMPLEMENTED**. Line-anchored text edit: finds a non-virtual instance
   declaration of the target method and inserts `virtual` after the access modifier. Only works
   when the method is declared in-repo (framework types → graceful rejection).
2. `wrapper_interface` — STUB (`NotImplementedError` + contract: emit `I{Receiver}Wrapper` +
   concrete wrapper, constructor-inject defaulted to concrete).
3. `parameterize_dependency` — STUB (`NotImplementedError` + contract: defaulted overload taking
   the dependency; original delegates; public API preserved).
Roslyn (Mode1Analyzer infra, Microsoft.CodeAnalysis.CSharp 4.14.0) is the robust future path for
the harder transforms — not built this pass.

**RefactorResult schema (smoke test + prompts align to this):**
`{transform: str, applied: bool, reverted: bool, reason: str, files_changed: list[str],
build_ok: bool|None, errors: list[dict]}`. `.to_dict()` truncates `errors` to 5. Logged per cell
to `refactors/{repo}/{target_id}.jsonl` and embedded in `attempts.jsonl` as `refactor_attempts`.

**Safety rails (all implemented now):**
- **Prod-write guard** `_safe_prod_path(repo_root, owning_csproj_dir, raw)` — writes allowed ONLY
  inside the owning .csproj subtree (owner via `compile_only.find_owning_csproj`); rejects escapes.
- **Snapshot/restore** — originals snapshotted before first edit; `restore_all()` returns every
  touched file to byte-pristine (deletes engine-created files). Runner calls it in a `finally`
  after EVERY cell → cells never contaminate each other; git tree clean between cells.
- **Behaviour-preservation** — after a successful edit, `dotnet build` the owning csproj (reuses
  `compile_only` toolchain). Build fail → AUTO-REVERT + `refactor_rejected` with errors. Engine
  ctor `verify_build` flag (default True; runner sets False only under `--mock-llm`). Running the
  owning project's existing test suite is a documented TODO (build-preservation is the minimum).

**Verify (no Foundry / no money):** all three modules import cleanly under `.venv`; all four
arg-syntaxes parse; temp-snippet smoke confirmed make_virtual `public string GetAsync` →
`public virtual string GetAsync`, the guard rejects `../../etc/passwd` / allows in-subtree,
`restore_all()` reverts to pristine, unknown transforms rejected, stubs raise NotImplementedError
(caught by the strategy). Full mock-LLM end-to-end run handed to Beck — needs
`tools/generation/tests/fixtures/refactor/default.json` (writer-role fixture exercising the
tool-call syntax above).

**No Azure spend. No Foundry model invoked. No dotnet build run in verification.**

### 2026-06-11: Phase-4 mock-LLM smoke test + fixture (hermetic, green)

**By:** Beck (Test/Coverage), requested by Jasper (autopilot)

**What:** Added the phase-4 end-to-end smoke test Watney handed off:
- `tools/generation/tests/fixtures/refactor/default.json` — writer-role mock fixture exercising `read_file → apply_refactor(transform=make_virtual) → submit_test(csharp)` using Watney's exact tool-call syntax.
- `tools/generation/tests/test_refactor_smoke.py` — subprocess-drives `agentic_refactor_runner.py` in `--mock-llm` mode; asserts all four artifacts (`attempts.jsonl`, `generated_tests/{repo}/{tid}/test.cs`, `turns/{repo}/{tid}.jsonl`, `refactors/{repo}/{tid}.jsonl`) and that the refactor log records an APPLIED `make_virtual`. **1 passed in ~0.3s, fully hermetic — no dotnet, no Foundry, no Azure spend.**

**Notable runner finding (NOT a bug fix — documented behavior to be aware of) [FLAGGED]:**
The phase-4 runner's `--mock-llm` mode **synthesizes a single hardcoded cell**
(`target_id=mock:0001, repo=mock-repo, file=mock.cs, method=DoSomething, kind=NonVirtual`)
and ignores `targets/v2/targets.csv`. The `--target-ids` filter then runs against that
synthesized row. Consequence: passing a *real* targets.csv id (like the `OpenRA:0003`
in the runner's own docstring usage example) filters the cell set to EMPTY → the runner
exits 0 having written only an empty `attempts.jsonl`, silently producing no test/turns/
refactor artifacts. **The smoke test pins `--target-ids mock:0001`** to match the
synthesized cell. Did not change the runner — the hardcoded mock cell is the intended
plumbing-only contract; flagging so future callers of mock mode don't chase a phantom
"no output" failure. (If we later want mock mode to honor a real target, that's a
deliberate runner change, not a smoke-test concern.)

**Hermeticity mechanism:** mock mode constructs `RefactorEngine(verify_build=False)` and a
stub check (run_ok=True, no dotnet). To make `make_virtual` actually APPLY (vs. record
`applied=False` "no owning .csproj"), the test points `--cloned-repos` at a tmp dir with a
real `mock-repo/MockLib.csproj` + `mock.cs` (`public string DoSomething()`), so
`find_owning_csproj` resolves and `_inject_virtual` runs for real. No build is triggered.

**No Azure spend. No Foundry. No dotnet invoked.**

### 2026-06-11: Phase-4 generate workflow created (.github/workflows/phase4-refactoring.yml)

**By:** Vogel (CI/CD), requested by Jasper (autopilot)

**What:** Authored `.github/workflows/phase4-refactoring.yml`, the phase-4 generate
workflow. Modeled structurally on `phase5-generate.yml` (the most complete template:
mock|foundry mode switch, mock smoke job, foundry guard + spend gate + freeze
confirmation) but adapted for phase 4 = **single-agent writer + a LOCAL `apply_refactor`
tool** (NOT multi-agent). `name:` = "Phase 4 — generate (agentic loop + refactoring
tool)"; `env: PHASE: phase4-refactoring`.

**Design / key choices:**
- **Hard-gated foundry, default mock.** `mode` input defaults to `mock`; workflow_dispatch
  only (no schedule/push/PR). A stray "Run workflow" click runs the no-Azure smoke path.
- **Smoke job (mock):** Python 3.12 + .NET 10 SDK setup, runs
  `pytest tools/generation/tests/test_refactor_smoke.py` (Beck's test, built in parallel),
  prints runner `--help`, plus a **best-effort** mock-runner shakedown
  (`--mock-llm --mock-fixtures-dir tools/generation/tests/fixtures/refactor --limit 1`)
  guarded to skip if the fixtures dir doesn't exist yet (parallel authoring).
- **Foundry guard:** requires `i_understand_this_will_spend_money=yes` +
  `confirm_after_freeze=yes` + date ≥ 2026-06-08 (same freeze noun as phase5), and a
  spend-gate step that **reuses the EXISTING Azure budget `phase4-tripwire-250`**
  ($250 Monthly, subscription scope) — deliberately did NOT mint a new budget name.
- **Matrix reuse:** `plan` job uses the shared `.github/scripts/plan_matrix.py`, so the
  6-model panel resolves identically to phase3/phase5; target-set sha256 integrity gate
  carried over. `generate` job runs in the dotnet SDK container (the refactor tool
  recompiles the owning csproj in-loop) and invokes
  `tools/generation/agentic_refactor_runner.py` — NOT `multi_agent_runner.py`, NOT the
  phase-3 `agentic_runner_feedback.py`.

**Runner flag cross-check (verified against the runner's argparse):** `max_compile_attempts`
→ `--max-attempts`; phase-4-specific `--max-refactors 3` and `--refactor-build-timeout-s 240`;
`--repo-filter`; real-Foundry gate is `--i-understand-this-will-spend-money` (exact hyphenated
flag name); mock flags `--mock-llm` / `--mock-fixtures-dir` / `--out-dir`.

**Faithful-stub posture:** syntactically valid and runnable today; foundry path gated hard
and defaults to run_1 shakedown (limit_per_repo=1) since the phase isn't sealed.

**VERIFIED:** `python3 -c "import yaml; yaml.safe_load(open('.github/workflows/phase4-refactoring.yml')); print('YAML OK')"` → **YAML OK**.

**No Azure spend. No workflow dispatched. File authoring + YAML validation only.**

### 2026-06-11: Phase-4 refactoring PR (#30) open against main
**By:** Vogel (CI/CD), requested by Jasper

**What:** The phase-4 (agentic loop + testability refactoring tool) work is now an open PR
against `main`: **PR #30** — https://github.com/jasperstone/mocking-static-methods/pull/30
(branch `jasper/phase4-refactoring`, tip `f7b42ecd`).

**Git provenance / why a rebase happened:** PR #28 (phase-5 renumbering + report updates +
phase-4 cost model, from `jasper/phase4-scaffold` / `4dbc35e9`) was **SQUASH-merged** into
`main` as squash commit `8d9b0ada` (the original `4dbc35e9` is therefore NOT reachable from
main). `jasper/phase4-refactoring` had been branched from the scaffold, so it carried the
now-duplicated `4dbc35e9` plus the new phase-4 work. Rebased with
`git rebase --onto main 4dbc35e9 jasper/phase4-refactoring` to drop the duplicate and replay
only the phase-4 scaffold commit (new sha `f7b42ecd`); clean, zero conflicts. Force-pushed
with `--force-with-lease`. The PR diff cleanly shows ONLY phase-4 files.

**Scope of PR #30:** `phases/phase4-refactoring/` scaffold (PLAN/REPLICATION/REPORT/phase.lock
+ single-agent prompts), `tools/generation/apply_refactor.py` (RefactorEngine; `make_virtual`
end-to-end, `wrapper_interface`/`parameterize_dependency` stubbed),
`agentic_loop_refactor.py` strategy, `agentic_refactor_runner.py`, hermetic smoke test (green),
and `.github/workflows/phase4-refactoring.yml` (mock|foundry; foundry guard reuses the existing
`phase4-tripwire-250` budget).

**Status:** Scaffold only — no Azure spend, no foundry run. Phase-4 cost model projects
$213.79 (85.5% of the $250 cap) for run_1. **PR is OPEN, not merged; no branches deleted.**
