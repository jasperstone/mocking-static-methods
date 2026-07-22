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

### 2026-06-11: Phase-4 prompts held identical to phase 3 — `apply_refactor` is the sole manipulated variable

**By:** Lewis (Lead), requested by Jasper (reviewing PR #30)

**What:** Rewrote the phase-4 prompts so they are phase 3's prompts held constant
(the control), with the availability of the `apply_refactor` tool as the ONLY
manipulated independent variable.

- `phases/phase4-refactoring/prompt/writer-system.md` is now phase 3's
  `system.md` **verbatim**, with **exactly one addition**: a single factual line in
  the tool menu declaring `apply_refactor` (same terse style as the
  `read_file` / `list_dir` lines, no coaching).
- `phases/phase4-refactoring/prompt/user-template.md` is now **byte-for-byte**
  phase 3's `user-template.md`. The "This is a Mode #1 site … apply_refactor"
  sentence and the `{{TEST_FRAMEWORK}}` / `{{TARGET_TFM}}` variables (which phase 3
  does not have) were removed.

**Removed confounds (all previously in the phase-4 prompts, all deleted):** the
Mode #1 / seam explanation, the EXT-vs-NonVirtual taxonomy, the coached transform
menu (incl. "`make_virtual` is the cheapest when it applies"), the anti-gaming
essay, the transient-seam paragraph, the 5-point self-check checklist, the
12-turn budget (phase 3 has no turns budget), and the user-template seam-coaching
sentence. One factual correction: phase 3's intro phrase "read-only tool access"
was changed to "tool access" because `apply_refactor` writes production source —
leaving "read-only" would bias the agent AGAINST using the treatment tool.

**Why (the design rationale):** the phase-4 contribution under test is a
*capability/tooling augmentation*, NOT prompt engineering. If the prompt also
explains Mode #1, coaches transform selection, and pre-warns about gaming, then
any run-OK% delta vs phase 3 is un-attributable — it could be the tool or the
prose. Holding the prompts identical isolates **tool-availability** as the single
independent variable, so any delta in run-OK% vs phase 3's 7.1% on the identical
frozen v2 300-cell set (same 6-model panel, same harness) is attributable to the
tool. Anti-gaming and behavior-preservation are enforced by the **harness** and
surfaced to the agent **only through tool feedback** (`refactor_rejected`),
exactly the way compile/run errors are surfaced in phase 3 — never pre-coached.

### 2026-06-11: Phase-4 boundary refined — tool documentation ≠ task coaching (richer apply_refactor spec)

**By:** Lewis (Lead), requested by Jasper

**Refines** the 2026-06-11 decision "Phase-4 prompts held IDENTICAL to phase 3
(single-variable control)". That decision stands; this one sharpens where the line sits.

**The distinction Jasper drew:** the earlier pass stripped the `apply_refactor` entry to
ONE terse menu line to avoid a prompt-engineering confound. That over-corrected by
conflating two different things. **Documenting what a tool does and how to call it is
standard tool-calling practice, NOT experiment coaching.** What must stay out is *task
framing* — telling the agent it faces a Mode #1 site, that it "will need" a seam, or a
transform-selection strategy / cost ranking.

**What changed (only the `apply_refactor` tool description in
`phases/phase4-refactoring/prompt/writer-system.md`):**
- Menu entry enriched from a terse one-liner to a richer (still parallel) line pointing to
  a dedicated block.
- Added a dedicated `apply_refactor` block after "Tool-call rules:" / before "Submitting:",
  mirroring how phase 3 gives `submit_test` and compile+run feedback their own blocks. The
  block documents, factually and neutrally:
  - the three transforms — `make_virtual` (adds `virtual` to a non-virtual instance method
    so a test can subclass-and-override); `wrapper_interface` (adapter interface + thin
    wrapper + ctor injection so a test can mock the interface); `parameterize_dependency`
    (NEW defaulted overload preserving the public API so a test can pass a fake);
  - all three accepted calling forms (`apply_refactor(transform=make_virtual)`,
    `apply_refactor(make_virtual)`, `apply_refactor(transform=make_virtual, method=Foo)`) —
    verified against `parse_refactor_args` (accepts JSON / key=value / bare);
  - the mechanics/contract — edits confined to the owning project; rebuild after the edit;
    auto-revert + `refactor_rejected` if it no longer builds; applied change live for the
    next submit_test; change is transient (reverted after the task);
  - honest implementation status — only `make_virtual` is wired end-to-end;
    `wrapper_interface` / `parameterize_dependency` may report not-yet-available (verified:
    they raise `NotImplementedError`, surfaced as a neutral "not implemented in this pass /
    NOT applied" tool result). Stated as a tool limitation, not a hint to use make_virtual.

**Boundary held (NOT added back):** no Mode #1 label, no EXT/NonVirtual taxonomy, no
"you'll need a seam", no transform cost/ease ranking, no anti-gaming essay, no self-check
checklist, no motivational transient-seam lecture. The auto-revert/transient mechanic is
stated factually as tool behavior (needed to interpret `refactor_rejected`), not as a
legitimacy lecture.

**Held constant vs phase 3 = the TASK FRAMING, not the tool inventory.**
`user-template.md` left byte-for-byte phase-3's (untouched). The **sole manipulated
variable remains the availability of `apply_refactor`**; the agent must still DISCOVER that
the tool helps and which transform fits.

**Docs updated:** PLAN.md "Prompts held identical to phase 3 (the control)" section and
REPLICATION.md single-variable blockquote reworded to state the boundary precisely —
"the apply_refactor TOOL is documented like any other tool (capability + calling contract),
consistent with how phase 3 documents submit_test and the compile/run loop; what is held
constant is the task framing; tool documentation is not task coaching."

**Methodological rule worth keeping:** "hold the prompt constant" means **hold the task
framing constant** — documenting a new capability's *interface* is part of giving the agent
the tool, not a confound. The confound is *strategy/situation coaching*, not *interface
documentation*.

**Safety:** code parses the tool by regex (`APPLY_REFACTOR_RE`) + `parse_refactor_args`,
never the prose, so enriching the description cannot change parsing. Smoke test green:
`python -m pytest tools/generation/tests/test_refactor_smoke.py -q` → 1 passed in 0.09s.
Committed by coordinator as d2bfb2d3 (code/docs).
The agent must DISCOVER on its own that the tool helps.

**Methodological rule (team-relevant):** when a phase adds a capability, hold
prompts/panel/harness/params constant vs the prior phase and push all
enforcement/coaching into the harness + tool feedback. Before dispatch, diff the
new phase's prompt against the control — it must differ by only the one declared
variable.

**Verified:** the runner (`tools/generation/agentic_refactor_runner.py`) and the
strategy (`tools/generation/strategies/agentic_loop_refactor.py`) parse
`<tool>apply_refactor(...)</tool>` by **regex** (`APPLY_REFACTOR_RE`), not by
prompt prose — confirmed by grep — so removing the prose is safe. Smoke test
green: `python -m pytest tools/generation/tests/test_refactor_smoke.py -q`
→ **1 passed in 0.10s**.

**Docs updated:** `PLAN.md` and `REPLICATION.md` now state the single-variable
design explicitly. `REPORT.md` already said "the writer prompt stays generic"
(consistent — left untouched).

**Not committed** (coordinator commits this round). **Phase-3 files untouched.**
No Azure spend; no workflow dispatched.

### 2026-06-11: Phase-4 Transform Contract (wrapper_interface + parameterize_dependency)

**By:** Lewis (Lead), requested by Jasper. Artifact: `phases/phase4-refactoring/TRANSFORM_CONTRACT.md`. Implements against Watney (builder).

**Key contract decisions:**
1. **Fully general Roslyn rewriter (not family-scoped).** Both stubbed transforms implemented in a new C# tool `RoslynRefactorTool` (net10.0, Microsoft.CodeAnalysis.CSharp 4.14.0, mirroring `Mode1Analyzer`'s ref-assembly fast-path build). Binds any receiver/method in the owning project via the semantic model. The known families (ILogger/IServiceProvider/IConfiguration/HttpClient) are the *validation set*, not the capability bound. `make_virtual` stays in the menu, unchanged.
2. **Pure tool, Python owns mutation.** `RoslynRefactorTool` reads source and returns JSON `{ok, applicable, reason, files{path:text}, seam{}}` on stdout; NEVER writes the repo. Python (`RefactorEngine`) writes via the existing snapshot/`_write` + `_safe_prod_path` guard + behavior-preservation build + `restore_all()` auto-revert. That lifecycle is untouched.
3. **Distinct code paths over a shared core.** `SeamCore` (shared) does reference loading, compilation, invocation binding, signature reconstruction from `IMethodSymbol`, wrapper emission, naming/defaults, seam-descriptor build. `WrapperInterfaceRewriter` (type-level ctor-field injection, rewrites all same-receiver sites) and `ParameterizeDependencyRewriter` (method-level overload-delegation, one site + delegator) are distinct rewriters — identical front half, structurally different rewrites with divergent applicability rules.
4. **Defaults so a bare call works.** Infers `interface_name = I{Recv}Wrapper`, `wrapper_name = {Recv}Wrapper`, `param_name = camelCase(wrapper_name)` from `receiver_type` (strip one leading `I`; collision ⇒ numeric suffix). For parameterize, `param_type` defaults to the generated `interface_name`.
5. **Anti-gaming verification mechanism.** A transform is "legitimately applied" only if the production call site, post-rewrite, invokes ONLY the injected interface (call-site exclusivity) with the default path constructing the real forwarder. Legitimacy decided post-hoc by a verifier step in `agentic_refactor_runner.py` cross-referencing the seam descriptor against the final submitted test: (1) seam interface mocked, (2) mock injected at the recorded injection point, (3) containing method/overload driven, (4) non-trivial assertion. Gates the refactor-attributable metric in PLAN.md.
6. **`via_seam` field.** Add `seam: dict` to `RefactorResult` (apply-time, `{}` for make_virtual, surfaced in `to_dict()`). Add `via_seam: bool|None` to the per-cell attempts row — `None` until the post-`submit_test` verifier sets it. Persist `seam` next to it so attribution is auditable from `attempts.jsonl` alone.
7. **Edge cases: handle-or-reject-cleanly, never corrupt.** Exact `reason` tokens pinned (`multiple_ctors`, `ctor_chaining`, `primary_ctor`, `record_type`, `struct_type`, `static_method_no_instance`, `partial_split`, `receiver_not_in_method_scope`, `no_receiver_source`, `receiver_is_this`, `site_not_found`, `unbound_receiver`). Static containing methods rejected by wrapper_interface but handled by parameterize_dependency. The owning-project build is the backstop.

Contract complete and prescriptive. Watney builds the tool + wires the subprocess call + adds the `seam` field and `via_seam` verifier, and lands the §9 C#/Python/smoke tests before run_1 dispatch.

### 2026-06-11: bundler must exclude its own output dir (self-inclusion fix)

**By:** Watney (Build/Infra), requested by Jasper.

**What:** `tools/bundle_dissertation_context.py` concatenates every narrative `.md` in the repo into `dissertation_bundle/dissertation_context.md`. Its `EXCLUDE_DIRS` set omitted the output directory, so each run after the first folded the PREVIOUS bundle (and `schedule.md`) back into the new one — self-inclusion. Added `"dissertation_bundle"` to `EXCLUDE_DIRS` (first entry); one line fixes both the `os.walk` in-place prune and `is_excluded()` (same set). **Why:** a tool that writes into the tree it scans must never ingest its own output.

**Verification (regenerated clean):** `grep "=== dissertation_bundle/"` → none; `multi-team` count = 0; phase4-refactoring + phase5-multiagent docs present; README ladder confirmed (4 = agentic loop + testability refactoring, 5 = multi-agent). Bundle = 50 files, 232,483 bytes; MANIFEST updated. Did NOT commit — coordinator commits this round.

### 2026-06-11: Fix `IServiceProvider` Case-B `unbound_receiver` false-negative

**By:** Watney (Build/Infra). Context: Beck flagged that `RoslynRefactorTool` returned `applicable=false / reason=unbound_receiver` for the framework generic extension `IServiceProvider.GetRequiredService<T>()`, contradicting `TRANSFORM_CONTRACT.md` §2.2 (Case B must be applicable). Impact: `System.IServiceProvider` 77 sites + `IServiceScopeFactory` 6 → ~83/300 targets (~28%). Changes confined to `RoslynRefactorTool/**` + two now-passing xfail markers.

**Root cause (corrected from Beck's hypothesis):** NOT a net9/net10 reference-identity split. Instrumenting `SeamCore.Locate` showed `IServiceProvider` resolved as `ErrorTypeSymbol` / `CS0246` (type not found). The fast-path `BuildCompilation` parses every `*.cs` under the owning project but deliberately skips `obj/` (§0.2), which is exactly where the SDK emits `*.GlobalUsings.g.cs` for `<ImplicitUsings>enable</ImplicitUsings>` projects. Target files relying on implicit `global using System;` never bind `System.IServiceProvider`, so the genuine `unbound_receiver` guard fires as a false-negative. (ILogger/HttpClient fixtures import explicitly, so they never tripped it.)

**Decision (Beck's option (a), refined):** re-supply the SDK default implicit usings in the analysis compilation rather than relaxing the receiver-identity check. `BuildCompilation` now prepends a synthetic `__ImplicitGlobalUsings.g.cs` carrying the default `Microsoft.NET.Sdk` set (`System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading`, `System.Threading.Tasks`). Global usings are additive + lowest-precedence — never break files with explicit usings. Repairs **every** implicit-usings target, not just IServiceProvider. **Defensive hardening:** extension receiver type now taken from the declared `this` parameter (`methodDef.ReducedFrom.Parameters[0].Type`) before falling back — a concrete always-bound symbol.

**Guard integrity:** the genuine `unbound_receiver` reject is unchanged (a truly-unbindable receiver still yields a null method symbol). **Verification (hermetic):** tool build clean; both transforms on `tests/cases/isp/Site.cs` → `applicable=true`, seam `T GetRequiredService<T>()` with `where T : notnull` preserved, rewritten output compiles. `pytest tools/generation/tests/ -q` → **25 passed** (was 23 + 2 xfailed); the 2 Case-B xfails promoted to full positive rows. No Azure/Foundry.

### 2026-06-12: RoslynRefactorTool — build & architecture choices

**By:** Watney (Build/Infra). Implementing the two stubbed transforms per Lewis's TRANSFORM_CONTRACT. Records implementation-level decisions where the contract left freedom; none change externally observable behaviour — flagged for Lewis sign-off.

1. **Generated wrapper file uses `global::`-qualified type names; extension members forward via the fully-qualified STATIC form.** A fully general rewriter cannot assume the right `using`s are importable in a generated file; a statically-resolved extension method is not callable as `_inner.LogInformation(...)` without the using. Decision: emit all types with `global::` FQNs (no usings, never collides) and forward reduced-extension members through their unreduced static method. All 5 contract cases compile; the rewritten call-site file still uses clean short names.
2. **`param_name` for parameterize derives from `wrapper_name` (`camelCase(wrapper_name)` → `loggerWrapper`)**, not `camelCase(param_type)` (which gives the awkward `iLoggerWrapper`). Prioritised the concrete §3.1 AFTER example over the §1.2 table prose; matches §3.1/§3.2 verbatim. Easy to flip.
3. **Release dll preferred, Debug fallback** for `ROSLYN_REFACTOR_TOOL_DLL`; missing both → clean `roslyn_tool_missing` (no crash).
4. **Tool emits ABSOLUTE paths in `files{}` and `seam.call_site`** — the argv (§0.3) doesn't pass the repo root so it cannot compute relative. Python already accepts absolute in `_safe_prod_path` + converts via `_rel()`; the `via_seam` verifier works off simple-name seam fields. Add `--repo-root` if a future verifier needs relative.
5. **Signature reconstruction from `IMethodSymbol.OriginalDefinition`; delegator targets the enclosing method, not the seam member** — correctness invariants for generics + overload-delegation; any future edit must preserve them.

**Status:** builds clean, added to the .sln. All 5 §2/§3 cases compile end-to-end; §5 reject tokens verified; `make_virtual` smoke still GREEN. C#/Python test files (§9) intentionally NOT added — Beck's per the confinement constraint. No Azure/Foundry.

### 2026-06-12: §4.3 via_seam verifier + §9.1/§9.2 refactor test artifacts

**By:** Beck (Test & Coverage). Implements phase-4 §9 test obligations + the §4.3 anti-gaming `via_seam` verifier. Changes confined to `agentic_refactor_runner.py`, `tools/generation/tests/**` (+ fixtures), `RoslynRefactorTool/tests/cases/**`. No edits to `apply_refactor.py`, the C# tool source, or prompts.

**Built:**
1. **§4.3 `via_seam` verifier** — `verify_via_seam(seam, test_source) -> (bool, checks)` in `agentic_refactor_runner.py`. Runs AFTER `submit_test` returns run-OK for a cell whose refactor applied with a non-empty seam. Four checks over the final submitted test source: `seam_type_referenced` (Moq/NSubstitute/FakeItEasy/hand-rolled mock construction), `injected_at_injection_point` (ctor arg list or overload call contains a mock token / named arg), `target_method_driven`, `non_trivial_assertion` (`.Verify(`/`.Received(`/fluent or non-trivial `Assert.*`; rejects `Assert.True(true)` stubs). `via_seam = all(checks)`. Persisted on the attempts row (`via_seam`, `via_seam_checks`, `seam`) AND as a `{"verification": true, ...}` line on the per-cell refactors log (§4.4 auditable). Stays `None` for make_virtual / non-passing cells.
2. **§9.1 hermetic C# checks** — `tools/generation/tests/test_roslyn_tool.py` (manifest-driven) + fixtures. 4 positive cases assert `applicable=true` + seam matches + rewritten output COMPILES against bundled refs (no NuGet restore); 7 reject cases assert `applicable=false` with the exact §5 reason token.
3. **§9.2 Python integration smoke** — extended `test_refactor_smoke.py` (+`--mock-cell-json` to target a real ILogger/HttpClient temp repo). Three end-to-end cases drive both transforms through the runner; LEGIT fixture (injects the mock → `via_seam=true`) + GAMED fixture (builds a mock but never injects → `via_seam=false`) prove discrimination. Plus 7 pure-Python verifier unit tests.

**Verification:** `pytest tools/generation/tests/ -q` → 23 passed, 2 xfailed (the ISP bug, since fixed by Watney). All hermetic.

**Decisions:** (a) verifier is regex-based over test source text, not semantic — §4.3 specifies the conditions not the mechanism; conservative + dependency-free + fast; sufficient to discriminate realistic gaming. (b) overload-call match uses `(?<!\w)enclosing\s*\(` so the common member-access form `client.FetchAsync(...)` matches while longer identifiers don't.

**Flagged for Watney (since fixed):** IServiceProvider generic extension `GetRequiredService<T>()` returned `unbound_receiver` contradicting §2.2 — marked `xfail(strict=False)`. (Watney root-caused to skipped `obj/` implicit-usings and fixed.) Deferred low-value §5 reject rows (`static_method_no_instance` flaky, `ctor_chaining`, `partial_split`, `receiver_is_this`) until stable fixtures exist.

### 2026-06-12: Phase-4 transform applicability sweep — coverage stats + 2 real-repo bugs (since fixed)

**By:** Watney (Build/Infra). Deterministic (NO LLM, NO Azure) validation that the two new AST transforms fire on messy real code. New harness `tools/generation/refactor_applicability_sweep.py` runs `RefactorEngine.apply(...)` directly against the real cloned repos for all 300 `targets/v2/targets.csv` rows, restoring each repo after every target (`restore_all()` in a `finally`). Artifacts: `applicability_all.csv` (900 rows, no build), `build_sample_{parameterize,wrapper}.csv` (`--verify-build`).

**FAST applicability pass (all 300, no build):** parameterize 190/300 (63.3%), wrapper 120/300 (40.0%), make_virtual 6/300 (2.0%). make_virtual near-zero by design (282/300 targets are extension-method sites → no instance method to mark virtual). Per-family: ILogger (192) param 130 / wrapper 113; IServiceProvider (77) param 48 / wrapper 4; HttpClient (17) param 7 / wrapper 1. Reject tokens are all clean §5 rejections — the tool never corrupts, it declines (`receiver_is_this`, `unbound_receiver`, `no_receiver_source` dominate; `no_owning_csproj` ×3 is a target-set data issue).

**BUILD-VERIFIED sample (`--verify-build`):** 9/9 produced a seam; 5/9 passed the owning-project build. All 4 build failures were auto-reverted by the build backstop — no repo corrupted. Caveat: the 190/120 "applicable" counts are an upper bound on *behaviour-preserving* applicability.

**NON-DESTRUCTIVE — VERIFIED CLEAN:** after the full 300-row pass + build sample + error probe, 0 tracked `.cs` left modified, 0 leftover `I*Wrapper.cs`; `restore_all()` reverted every edit to byte-pristine. ⚠️ Cleanliness-check false positive: the harness's built-in `git status --porcelain` flags several clones as DIRTY, but every flagged path is pre-existing clutter from earlier agents (untracked `*.Tests.cs`, `CoverageReport/`, mtimes Dec-2025/Jan-2026) — none production `.cs` or our generated files. Recommend diffing against a pre-sweep snapshot or scoping to `*.cs` + `I*Wrapper.cs`.

**BUGS FOUND (flagged, fixed in the analyzer-hardening drop below):** (1) parameterize emits CS1737 when the enclosing method already ends in an optional/`params` parameter (appends the injected param to the END). (2) wrapper emits CS1503 when sibling same-receiver call sites use other unmodeled extension overloads (rewrites ALL same-receiver sites to a wrapper declaring only the target shape). Both safe — the build backstop reverts them.

### 2026-06-12: Analyzer-hardening of generated/injected refactor edits + overload-candidate arity fix

**By:** Watney (Build/Infra), coordinated by Squad — requested by Jasper. Status: implemented, tests green, build-sample re-verified.

**Why:** the general transforms emit logically-correct C#, but strict target repos (aspnetcore, efcore, abp, orleans, jellyfin) enable `TreatWarningsAsErrors` + StyleCop + XML doc generation — turning correct refactors into build failures for reasons unrelated to our logic. Concrete jellyfin:0006 failures: CS1591 (missing XML docs), CS1573 (injected ctor param missing `<param>` tag), SA1137/SA1505 (wrong indentation / blank line after `{`), CS8632 (nullable `?` in a `#nullable disable` region).

**Fixes (SeamCore.cs / WrapperInterfaceRewriter.cs / ParameterizeDependencyRewriter.cs):** (1) generated wrapper file now starts `// <auto-generated/>` + `#nullable enable`. (2) injected members in the existing prod file: emit minimal `<param>`/`///` when siblings carry docs (+ `#pragma warning disable CS1591, CS1573` fallback); detect file newline + insertion-site indent and match; detect effective nullable context and emit WITHOUT `?` if disabled. (3) behavior preserved — overload precision, required-before-optional insertion, seam descriptor, JSON-only stdout, via_seam markers unchanged.

**Applicability (post-bugfix, deterministic, 300 targets):** parameterize 158/300 (52.7%), wrapper 118/300 (39.3%), make_virtual 6/300, union 163/300 (54.3%). Lower than the pre-fix 190/120 — correct: false-accepts that didn't build are now clean rejects (a truthful build-safe upper bound).

**Build-sample build_ok (strict-repo slice):** wrapper 4/5 built OK (jellyfin:0006 was failing CS1591/CS1573/SA/CS8632 → now clean); parameterize 7/8 built OK. Other rows are honest rejects.

**Follow-up: overload-candidate arity fix.** Root-caused orleans:0116. When overload resolution doesn't fully bind (`symInfo.Symbol == null` — net9-Abstractions vs net10-runtime reference-identity split on the `ILogger` receiver), `SeamCore.Locate` fell back to `CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault()` — an ARBITRARY overload. For `logger.LogDebug("…", message, diag)` it grabbed the 4-param `LogDebug(EventId, Exception?, string?, params object?[])` instead of the reduced `LogDebug(string?, params object?[])`, so the wrapper exposed the wrong signature → CS7036/CS1503 → owning build failed. Fix: a new arity-aware `SeamCore.PickBestCandidate(...)` selects the candidate whose extension-reduced parameter list is arity-compatible with the call (accounting for `this` reduction, optional params, `params` arrays, explicit generic arity) and best matches the positional arg types; falls back to first-candidate only when nothing is compatible. `SameReceiverCallRewriter` uses the same helper so the call-site overload key matches `BoundMethod`. orleans:0116 build_ok was False/False → now True/True (both transforms); seam now reports the 2-param reduced overload. Added regression fixture `RoslynRefactorTool/tests/cases/overload_candidate_arity/`.

**Tests/cleanliness:** full pytest **36 passed** (was 31, +5 robustness/regression fixtures). All touched repos left clean (only pre-existing local artifacts remain). ZERO Azure/Foundry spend — deterministic local Roslyn + dotnet builds. Open item `aspnetcore:0669` = `baseline_build_failed` (repo doesn't build pre-edit — not our bug).

### 2026-06-12: CI input-limit fix + writer-prompt transform-status correction

**By:** Lewis (Lead). Branch `jasper/phase4-refactoring` (PR #30), commit 441a8418.

**(a) workflow_dispatch input consolidation (11 → 10).** actionlint failed PR #30: `workflow_dispatch` declared 11 inputs; GitHub's hard maximum is 10. Merged the two foundry safety confirmations (`i_understand_this_will_spend_money` + `confirm_after_freeze`) into a SINGLE required-token input `confirm_spend` (type string, default `no`); to dispatch foundry mode the operator must type exactly `yes-after-2026-06-08-freeze`. Rationale: two identical low-entropy `yes` boxes were weaker than they looked; one compound token naming BOTH the spend acknowledgement AND the freeze date is higher-friction — defensibly stronger. The independent runtime date check (`date >= 2026-06-08`) is retained as a second non-bypassable gate; `mode` default stays `mock`. All downstream references updated (inputs block, `guard-foundry` step, header comment, `mode` description). Rejected folding `run_index_start` into `runs_per_cell` (distinct semantics). Verification: local actionlint v1.7.7 clean, inputs == 10, CI actionlint green on 441a8418.

**(b) writer-system prompt transform-status correction.** phase4 `writer-system.md` line 33 still said only `make_virtual` was wired and the other two "may report back as not-yet-available." All three are now production-wired via the local Roslyn tool. This prompt is FED TO THE MODEL during the run, so a stale line would bias the writer away from two of the three transforms — and transform choice is the manipulated variable in phase 4, so it would confound the comparison. Replaced with an accurate statement: all three are wired end-to-end; applicability varies per target so any may return `refactor_rejected` when it doesn't apply or doesn't compile — pick a different transform or submit as-is. (phase5 `writer-system.md` lacks the stale line; no change.)

### 2026-06-12: Mock end-to-end validation of the phase-4 agentic loop on REAL targets

**By:** Beck (Test/Coverage), requested by Jasper. Spend: $0 — MOCK mode only (`--mock-llm` + `--mock-cell-json`); no Foundry/Azure call, no workflow dispatch.

**Why:** before the ~$214 real Foundry run, de-risk the full chain end-to-end (`read_file/list_dir → apply_refactor → seam descriptor → submit_test → compile+run feedback → via_seam verification`). Until now the now-fixed `wrapper_interface`/`parameterize_dependency` transforms had never been driven through the runner against genuine cloned-repo targets.

**What ran:** per case a fixture-scripted writer turn calls `apply_refactor(transform=…)` for the real target then submits a C# test. `apply_refactor` invoked the **real** `RoslynRefactorTool.dll` against real production source (real seam descriptor); `verify_via_seam` ran the four §4.3 checks. In MOCK mode the in-loop compile/run is stubbed (`run_ok=True`) and the behaviour-preservation build is skipped (`build_ok` = n/a mock — confirmed True separately in the build-sample CSVs).

**Results:** LEGIT cases all via_seam=True with all four checks true — jellyfin:0006 (wrapper), abp:0147 (wrapper), server:0053 (parameterize), semantic-kernel:0125 (parameterize); real seams emitted (e.g. `ILoggerWrapper` ctor-injected, `UpdateTaxInformation(ISubscriber, TaxInformation, ILoggerWrapper)` overload). make_virtual on real non-virtual `OAuthHttpClient.SendAsync` → via_seam correctly `None` (subclass-and-override path). **GAMED case** (jellyfin:0006): identical seam but the test constructed `Mock<ILoggerWrapper>` and never injected it (`new ApplicationHost()` no-arg) + trivial `Assert.NotNull` → via_seam=**False** with exactly one failing check (`injected_at_injection_point=False`) — the anti-gaming check discriminates on a real target, not just fixtures.

**Safety/cleanliness:** zero Azure/Foundry spend; no workflow dispatched. All five touched repos clean after `restore_all()` (jellyfin/semantic-kernel/duplicati fully clean; abp/server show only pre-existing unrelated artifacts; none of the refactored files leaked). Comment-only change: `apply_refactor.py` module docstring corrected the stale `wrapper_interface`/`parameterize_dependency` "STUB" labels to IMPLEMENTED (delegate to `RoslynRefactorTool` via `_invoke_roslyn_tool`). `pytest tools/generation/tests/ -q` → 36 passed.

**Decision: end-to-end loop validated against real targets — YES.** The full phase-4 chain drives real Roslyn seam transforms on genuine targets, legit/gamed via_seam discrimination holds on real seams, the working tree stays pristine. Caveat: in MOCK mode the in-loop compile+run and behaviour-preservation build are stubbed/skipped — those signals come live only under the real Foundry run (build_ok already independently confirmed True for these targets). Cleared to proceed to the funded run.

### 2026-06-16: site_not_found locator hardening retained despite zero gain in targeted recheck

**By:** Watney (Build/Infra), requested by Jasper.

**What:** Hardened `SeamCore.Locate(...)` candidate resolution for duplicate-name + line-drift scenarios using deterministic scoring (line proximity, enclosing context, containing-type hint, receiver-kind/type hints) with hard compatibility gates and ambiguity-safe tie reject.

**Validation:** `dotnet build RoslynRefactorTool/RoslynRefactorTool.csproj -c Release` succeeded; `pytest tools/generation/tests/test_roslyn_tool.py -q` passed (38).

**Observed outcome on targeted deterministic recheck:**
- `wrapper_interface` site_not_found set (96 rows): applicable `0 -> 0`; after reasons split to `site_not_found=50`, `baseline_build_failed=46`.
- `parameterize_dependency` site_not_found set (98 rows): applicable `0 -> 0`; after reasons split to `site_not_found=52`, `baseline_build_failed=46`.

**Decision:** Keep the locator hardening as a correctness/ambiguity-safety improvement; next gains require separate work on true unresolved-site rows and baseline build exclusions.

### 2026-06-17: verify-build now warms restore first and classifies restore failures explicitly

**By:** Watney (Build/Infra), requested by Jasper.

**What:** `tools/generation/apply_refactor.py` now runs one-time `dotnet restore <owning.csproj>` before `dotnet build` in verify-build flow using the same sandbox env knobs and timeout class.

**Failure classification:** restore timeout/exec/non-zero now returns `ok=False` with `errors[0].code = RESTORE_FAIL` and parsed diagnostics when available. Existing revert semantics are unchanged (`reason=refactor_rejected` on verify-build failure).

**Validation:**
- `pytest -q tools/generation/tests/test_roslyn_tool.py` -> 38 passed.
- `pytest -q tools/generation/tests/test_refactor_smoke.py -k "refactor_smoke or wrapper_interface_via_seam_legit"` -> 11 passed.

**Decision:** Treat restore failures as first-class triage signals (`RESTORE_FAIL`) while preserving behavior-preservation gating and auto-revert guarantees.

### 2026-06-17: serialize verify-build sweep families on shared cloned_repos roots

**By:** Beck (Test/Coverage), requested by Jasper.

**What:** Establish run-order convention for verify-build sweeps: run one sweep family at a time per shared `cloned_repos` root (wrapper targeted/full, parameterize targeted/full), and wait for CSV output before starting another sweep family.

**Why:** Concurrent sweep processes against the same repo tree create cross-process contention and ambiguous run ownership/progress.

**Decision:** Parallelize within one process (`--jobs N`), not by launching multiple concurrent sweep processes on the same shared repos root.

### 2026-06-17: baselinefix recheck artifacts refreshed with deterministic counts unchanged

**By:** Beck (Test/Coverage), requested by Jasper.

**Artifacts refreshed:**
- `tools/generation/results/baselinefix_recheck_wrapper.csv` (113 rows)
- `tools/generation/results/baselinefix_recheck_parameterize.csv` (108 rows)

**Reference full snapshots (unchanged):**
- `build_verified_wrapper.csv`: applicable true `53`, build_ok true `41`.
- `build_verified_parameterize.csv`: applicable true `75`, build_ok true `44`.

**Reason-token deltas (before -> after):** unchanged for both transforms; no RESTORE_FAIL markers observed in this pass.

**Decision:** Accept the refreshed baselinefix recheck artifacts as canonical for this batch; deterministic schema/distribution remained stable.

### 2026-07-21: Phase-4 latest-run source map for report regeneration

**By:** Watney (requested by Jasper)

**What:** For phase4 report refresh, use model-specific latest completed runs rather than a single workflow run. Selected latest full run sets: `grok-4-1-fast` -> `29612308097`, `llama-3.3-70b-instruct` -> `29612257530`. Restore remaining model baselines from cached `backfill-28522182860` before applying those reruns.

**Why:** Recent runs are model-sharded/chunked and not every workflow run contains all six canonical models. Some older GitHub artifacts are retention-expired, so local cached backfills are required for reproducible regeneration.

### 2026-07-21: Phase4 refresh verification found COSTS_AUTOGEN row mismatch

**By:** Jasper (via Copilot/Beck)

**What:** Post-refresh verification confirmed phase4 run directories and `tools/viz/data/per_model_phase.csv` and `phases/phase4-refactoring/HEADLINE.md` coherence across all six canonical models, but `phases/phase4-refactoring/COSTS_AUTOGEN.md` was inconsistent for `llama-3.3-70b-instruct` and `phi-4` (calls/token-list values differed from CSV).

**Why:** Avoid publishing mixed-state reporting; cost table must be regenerated from the same refreshed source snapshot as phase4 CSV/headline.

### 2026-07-21: Phase-4 cost/report coherence uses tooling-excluded aggregation

**By:** Vogel (requested by Jasper)

**What:** Standardized phase4 cost/report aggregation semantics so `tools/cost/estimate.py --phase phase4-refactoring` excludes non-submitted tooling failures (auth/rate-limit/timeout/network/service signatures), matching `tools/viz/aggregate_phase_results.py` and `HEADLINE.md` logic. Regenerated `phases/phase4-refactoring/COSTS_AUTOGEN.md`, `phases/phase4-refactoring/HEADLINE.md`, `tools/viz/data/per_model_phase.csv`, and `tools/viz/data/per_model_repo.csv` from the same canonical in-repo phase4 results.

**Why:** Prevent model-row drift (notably `llama-3.3-70b-instruct` and `phi-4`) where estimator call/token totals previously included tooling-failure rows that evaluator/viz rows intentionally exclude from quality metrics.

### 2026-07-21: Lead gate approval for phase4 latest-run report refresh

**By:** Lewis (requested by Jasper)

**Decision:** APPROVE

**What was verified:**
1. Latest successful complete run sets used for refreshed models: `grok-4-1-fast` run `29612308097` and `llama-3.3-70b-instruct` run `29612257530`; canonical retained inputs were used for the other panel models when newer full shards were unavailable.
2. Report artifacts were regenerated from current in-repo phase4 results (`phases/phase4-refactoring/HEADLINE.md`, `phases/phase4-refactoring/COSTS_AUTOGEN.md`, `tools/viz/data/per_model_phase.csv`, `tools/viz/data/per_model_repo.csv`).
3. Beck's mismatch finding was resolved by Vogel: `COSTS_AUTOGEN` calls/token-list align with `per_model_phase.csv` for all six phase4 models.

**Residual risk:** Data coherence blocker is closed for headline/cost tables. Remaining risk is local figure re-render environment variability (Docker/R runtime availability), not report-data correctness.
