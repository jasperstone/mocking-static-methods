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
