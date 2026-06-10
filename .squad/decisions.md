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
