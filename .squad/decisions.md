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
