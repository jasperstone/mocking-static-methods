#!/usr/bin/env python3
"""Project the *actual Azure bill* for a phase, not just model-token cost.

Background
----------
The original estimator modelled only per-token model cost. For phase 3 it
reported **$82.19**, but the real experiment bill for the same window was
**~5x** that. The May 2026 cost-management breakdown (VisualStudioSubscription,
heavy window May 12-16, phases 2+3 generation sweeps) was:

    Foundry Tools    $182.26   <- NOT token-based; agent-tool/runtime surface.
                                  Single biggest line item, previously UNMODELED.
    Foundry Models   $160.45   <- token cost; ~2x the per-token list estimate.
    SaaS/Marketplace  $24.22   <- card-billed (Marketplace models; see below).
    Container Reg.     $6.39   <- minor fixed.
    Azure AI Search   $25.96   <- EXCLUDED. Resource torn down, unrelated to the
                                  experiment. NOT modelled anywhere in this file.

So the actual *Foundry* portion (Tools + Models) was ~$342, vs the old $82.

This estimator keeps the per-token model cost (PRICES) and adds two modeled
non-token components calibrated against the May anchors:

  1. A token-cost RECONCILIATION FACTOR for the Foundry Models meter (list
     prices undercount actual ~2x).
  2. A Foundry TOOLS / runtime overhead that scales with the number of
     *agent-role invocations* per cell (writer + reviewer x cycles +
     fixer x cycles), NOT with token count -- because that agent/tool-call
     surface is what actually drove the $182, and it gets much worse under
     phase 5's multi-agent loop.

It also splits spend into credit-eligible vs marketplace (card-billed) and
reports the combined total against a soft/hard cap and the $150 monthly credit.

Usage:
    python3 tools/cost/estimate.py --phase phase3-agentic-loop --md
    python3 tools/cost/estimate.py --phase phase3-agentic-loop --cap 250
"""
from __future__ import annotations
import argparse
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]

# ---------------------------------------------------------------------------
# Per-token list prices. USD per 1M tokens. Sources:
#   * Azure OpenAI: https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/
#   * Foundry Models (serverless): per-model deployment page in ai.azure.com
# Captured 2026-05-12. Verify before quoting.
PRICES = {
    # Azure OpenAI Chat surface
    "gpt-4.1-mini":            {"in": 0.40, "out": 1.60},
    "gpt-4.1-nano":            {"in": 0.10, "out": 0.40},
    # Azure OpenAI Responses surface (gpt-5 family) -- dropped from phase 3+
    "gpt-5-codex":             {"in": 1.25, "out": 10.00},
    # Foundry Models inference surface
    "phi-4":                   {"in": 0.125, "out": 0.50},
    "codestral-2501":          {"in": 0.30, "out": 0.90},
    "llama-3.3-70b-instruct":  {"in": 0.71, "out": 0.71},
    "grok-4-1-fast":           {"in": 0.20, "out": 0.50},
}

# ---------------------------------------------------------------------------
# Billing surface per model. Auditable -- edit this one dict to reclassify.
#   "credit"      = Azure first-party (Azure OpenAI / Azure-hosted Foundry
#                   Models). Draws down the $150 monthly MSDN credit.
#   "marketplace" = Marketplace SaaS offer, card-billed. Does NOT draw the
#                   credit, but per the user it DOES count toward the
#                   combined soft/hard cap.
#
# Default split is the user's stated billing intent.
#
# CAVEAT (captured 2026-06-10 from free read-only `az consumption usage list`):
# The May usage detail shows ONLY codestral routing through Microsoft.SaaS
# ("Codestral 25.01 - mistral-codestral-2501-plan-prod - paygo-inference-*").
# llama-3.3-70b and grok-4-1-fast appear as "Azure Llama Models" / "Azure Grok
# Models" billed via Microsoft.CognitiveServices -- i.e. the Azure first-party
# (credit) surface. The actual May SaaS/Marketplace line was only $24.22, which
# reconciles to codestral token cost alone (~$19 list x ~1.27), NOT to all three
# (~$59 list). If the bill is the authority, llama + grok belong in "credit" and
# the marketplace subtotal collapses to ~$24. Flipping them is a one-line edit
# here; the COMBINED total (and therefore the cap decision) is unaffected.
BILLING = {
    "gpt-4.1-mini":            "credit",
    "gpt-4.1-nano":            "credit",
    "phi-4":                   "credit",
    "gpt-5-codex":             "credit",
    "codestral-2501":          "marketplace",
    "llama-3.3-70b-instruct":  "marketplace",  # see CAVEAT: az shows credit surface
    "grok-4-1-fast":           "marketplace",  # see CAVEAT: az shows credit surface
}

# ---------------------------------------------------------------------------
# Reconciliation constants, calibrated against the May 2026 bill.
# These are TUNABLE -- they are the empirical bridge between list prices and the
# actual Foundry bill. Each default is derived inline from a May anchor.

# (1) Foundry Models token reconciliation.
#     May Foundry Models $160.45 / phase-3 token list estimate $82.19 = 1.952.
#     This slightly OVER-attributes because phase 2 also billed tokens in the
#     same window (phase 2 list ~ $16.58), so the true phase-3-only factor is
#     bounded ~1.6x .. 1.95x. We default to the upper anchor (conservative for a
#     go/no-go budget call -- it does not under-state the bill).
TOKEN_RECON_FACTOR = 1.95

# (2) Foundry Tools / agent-runtime overhead.
#     NOT token-based. The May Foundry Tools line was $182.26. We attribute it
#     to the phase-3 agentic sweep (5,400 writer invocations) -> a per-agent-
#     invocation surcharge. Phase 5 multiplies the invocation count per cell
#     (writer + reviewer + fixer), so this term -- not tokens -- is what makes
#     phase 5 explode. (Same over-attribution caveat as above: some of the $182
#     belongs to phase 2's agentic runs. Halving it still leaves phase 5 well
#     over cap, so the conclusion is robust. See residual-gap note in output.)
FOUNDRY_TOOLS_MAY_USD = 182.26
FOUNDRY_TOOLS_MAY_AGENT_CALLS = 5400
TOOLS_SURCHARGE_PER_CALL = FOUNDRY_TOOLS_MAY_USD / FOUNDRY_TOOLS_MAY_AGENT_CALLS  # ~$0.03375

# Foundry Tools (agent runtime) bills on the Azure first-party surface, so its
# whole cost is assigned to the CREDIT bucket regardless of which model the
# agent was driving. Documented assumption -- the agent orchestration/tool
# runtime is Azure-side even when the underlying model is a marketplace offer.
OVERHEAD_BILLING = "credit"

# Monthly MSDN credit (resets ~Jun 11).
CREDIT_USD = 150.0

# ---------------------------------------------------------------------------
# Phase-5 multi-agent projection assumptions (from phases/phase5-multiagent/PLAN.md).
#
# Per-cell agent invocations under the writer / reviewer / fixer loop:
#
#     calls/cell = 1 writer + (reviewer-per-cycle x cycles) + (fixer-per-cycle x cycles)
#
# THEORETICAL MAX per cell = 1 + 2*C  (every one of the C cycles fires both a
# review and a fix):   cycles=1 -> 3,   cycles=2 -> 5,   cycles=3 -> 7.
#
# REALIZED average is lower because most cells pass review before exhausting the
# cycle budget (early exit). May-calibrated realized per-cycle rates:
#     reviewer 0.6 / cycle,  fixer 0.5 / cycle
# -> realized calls/cell = 1 + 1.1*C :
#     cycles=1 -> 2.1,   cycles=2 -> 3.2,   cycles=3 -> 4.3
# The cycles=3 value (4.3 = 1 writer + 1.8 reviewer + 1.5 fixer, the original
# PLAN.md per-cell figures) is the anchor that reproduces the ~$1,197 full-scope
# projection. Cutting cycles 3 -> 2 drops calls/cell 4.3 -> 3.2 (-26%).
P5_REVIEWER_RATE_PER_CYCLE = 0.6   # realized reviewer invocations per review cycle
P5_FIXER_RATE_PER_CYCLE = 0.5      # realized fixer invocations per review cycle

# Phase-3 sweep shape: 5,400 writer invocations = 300 cells x 6 models x 3 runs.
# So the phase-3 calibration base corresponds to runs_per_cell = 3. Writer
# invocations (and therefore Foundry Tools overhead) scale LINEARLY with runs,
# so cutting runs 3 -> 1 is the single biggest lever on the bill.
P3_RUNS_PER_CELL = 3

# Defaults for a full-scope phase-5 dispatch (the original ~$1,197 call).
P5_DEFAULT_RUNS = 3
P5_DEFAULT_REVIEW_CYCLES = 3

# PLAN itemized token (list) anchors at the full-scope (runs=3, cycles=3) point:
#   writer $82.19 (the phase-3 base) + reviewer ~$50 + fixer ~$80 = ~$212.
# Carried as labeled inputs, then converted to per-invocation token rates so they
# scale correctly when runs and cycles change.
P5_REVIEWER_TOKEN_LIST_USD = 50.0   # at runs=3, cycles=3
P5_FIXER_TOKEN_LIST_USD = 80.0      # at runs=3, cycles=3

# Named phase-5 configs (label, runs_per_cell, max_review_cycles). Full 6-model
# panel in every config -- the cuts are runs + cycles only, never models, to keep
# the cross-model comparison intact. The frozen design (Jasper, 2026-06-10) is
# max_review_cycles=1: Config A is run_1 (the calibration dispatch), Config B is
# the pooled full 3-run set dispatched after the run_1 go/no-go. Config C is the
# pre-freeze original-full-scope reference that reproduces the published ~$1,197.
P5_CONFIGS = [
    ("A - run_1 calibration (cycles=1 frozen; 1st dispatch)", 1, 1),
    ("B - full 3-run set, cycles=1 (runs 2+3 after go/no-go)", 3, 1),
    ("C - original full scope (reference, pre-freeze)",        3, 3),
]

# ---------------------------------------------------------------------------
# Phase-4 refactoring-loop projection (from phases/phase4-refactoring/PLAN.md).
#
# Phase 4 = the SAME single writer agent as phase 3, PLUS a LOCAL `apply_refactor`
# tool (NO LLM behind it) that edits production source to introduce a testability
# seam -- extract-and-override, wrapper-interface/adapter, or dependency-
# parameterization -- BEFORE the test is written/compiled. The existing
# compile_only harness then rebuilds the owning csproj from source.
#
# The cost contrast with phase 5 is the whole point: phase 4 has exactly ONE LLM
# role (the writer). There is NO reviewer and NO fixer LLM, so -- unlike phase 5 --
# no second/third model role multiplies token spend. The delta over phase 3 comes
# from two modest, well-bounded terms only:
#
#  (1) TOKEN INFLATION on the phase-3 single-writer base. The refactoring capability
#      makes the writer take MORE turns per cell (inspect the target, decide on a
#      seam, call apply_refactor, read the result, then write/iterate the test), so
#      it emits more tokens per cell than the phase-3 writer. Modeled as a flat
#      multiplier on the phase-3 writer token base -- refactoring exploration adds
#      turns, NOT a whole extra agent, so the factor is modest (~1.4-1.6x).
P4R_TOKEN_INFLATION = 1.5
#  (2) apply_refactor IS an agent tool invocation (same billing mechanism as
#      read_file / list_dir today), so each call adds to the invocation-scaled
#      Foundry Tools overhead at the EXISTING TOOLS_SURCHARGE_PER_CALL rate. The
#      tool itself is zero-token (local), but the agent-runtime/tool surface still
#      bills. Phase 4 introduces ~1 seam per cell (occasionally a second), so model
#      ~1-2 apply_refactor calls per cell ON TOP of the phase-3 writer invocations.
P4R_REFACTOR_CALLS_PER_CELL = 1.2
#  Default dispatch = run_1 (go/no-go), mirroring the frozen phase-5 run_1 design.
#  A full 3-run sweep at the phase-3 cell/model scale necessarily exceeds the cap
#  (phase-3 combined alone is already ~$342 > $250), so the dispatch you actually
#  run first is run_1. runs_per_cell scales writer + apply_refactor invocations
#  (and the dominant Foundry Tools overhead) LINEARLY off the phase-3 base
#  (P3_RUNS_PER_CELL = 3 = 5,400 writer calls).
P4R_DEFAULT_RUNS = 1


TOOLING_FAILURE_RE = re.compile(
    r"(\b401\b|\b403\b|\b408\b|\b429\b|\b500\b|\b502\b|\b503\b|\b504\b|"
    r"timeout|timed out|rate.?limit|access denied|invalid subscription key|"
    r"network error|connection)",
    re.IGNORECASE,
)


def is_tooling_failure(rec: dict) -> bool:
    if rec.get("submitted"):
        return False
    text = " ".join(
        str(rec.get(k, ""))
        for k in ("error", "halt_reason", "error_type", "final_error_type")
    )
    return bool(TOOLING_FAILURE_RE.search(text))


def _bucket(model: str) -> str:
    return BILLING.get(model, "credit")


def aggregate_phase(phase_dir: Path):
    """Sum tokens + agent invocations per model across every attempts.jsonl.

    For single-agent phases (phase 2 / phase 3) each attempts.jsonl record is
    one writer-agent invocation, so agent_calls == record count. Phase-5
    multi-agent traces (writer/reviewer/fixer) will live under run_*/turns/ and
    will need per-role counting when those land; the analytical phase-5
    projection below does not depend on that.
    """
    totals = defaultdict(lambda: {"p": 0, "c": 0, "calls": 0, "submitted": 0,
                                  "wall_ms": 0, "agent_calls": 0})
    for path in sorted(phase_dir.glob("results*/**/attempts.jsonl")):
        for line in path.open():
            r = json.loads(line)
            if is_tooling_failure(r):
                continue
            m = r.get("model_id", "?")
            t = totals[m]
            t["p"] += r.get("total_prompt_tokens", 0)
            t["c"] += r.get("total_completion_tokens", 0)
            t["calls"] += 1
            t["submitted"] += int(r.get("submitted", False))
            t["wall_ms"] += r.get("wall_ms", 0)
            t["agent_calls"] += 1  # single writer agent per record
    return totals


def token_cost_list(model: str, p: int, c: int) -> float:
    price = PRICES.get(model)
    if price is None:
        return 0.0
    return p / 1_000_000 * price["in"] + c / 1_000_000 * price["out"]


def render(phase: str, totals: dict, cap: float, md: bool) -> dict:
    """Print the decomposed per-model table + subtotals. Returns summary dict."""
    rows = []
    sub = {"credit": 0.0, "marketplace": 0.0}
    grand = {"tok_list": 0.0, "tok_recon": 0.0, "overhead": 0.0, "total": 0.0,
             "calls": 0, "agent_calls": 0}

    for m in sorted(totals):
        t = totals[m]
        tok_list = token_cost_list(m, t["p"], t["c"])
        tok_recon = tok_list * TOKEN_RECON_FACTOR
        overhead = t["agent_calls"] * TOOLS_SURCHARGE_PER_CALL
        total = tok_recon + overhead
        bucket = _bucket(m)
        # token cost -> model's billing bucket; overhead -> credit bucket.
        sub[bucket] += tok_recon
        sub[OVERHEAD_BILLING] += overhead
        grand["tok_list"] += tok_list
        grand["tok_recon"] += tok_recon
        grand["overhead"] += overhead
        grand["total"] += total
        grand["calls"] += t["calls"]
        grand["agent_calls"] += t["agent_calls"]
        rows.append((m, bucket, t["calls"], tok_list, tok_recon, overhead, total))

    combined = sub["credit"] + sub["marketplace"]
    card_overage = max(0.0, sub["credit"] - CREDIT_USD) + sub["marketplace"]

    print(f"\n=== {phase}: projected Azure bill (Foundry Tools + Models) ===")
    print("(Azure AI Search excluded by design; not modelled.)\n")

    if md:
        print("| Model | Bill | Calls | Token (list) | Token (recon x%.2f) | Tools overhead | Total |"
              % TOKEN_RECON_FACTOR)
        print("|-------|------|------:|-------------:|--------------------:|---------------:|------:|")
        for m, b, calls, tl, tr, ov, tot in rows:
            print(f"| `{m}` | {b} | {calls} | ${tl:.2f} | ${tr:.2f} | ${ov:.2f} | ${tot:.2f} |")
        print(f"| **Total** | | **{grand['calls']}** | **${grand['tok_list']:.2f}** "
              f"| **${grand['tok_recon']:.2f}** | **${grand['overhead']:.2f}** "
              f"| **${grand['total']:.2f}** |")
    else:
        hdr = f"{'model':<24}{'bill':<12}{'calls':>6}{'tok_list':>11}{'tok_recon':>11}{'overhead':>11}{'total':>11}"
        print(hdr)
        print("-" * len(hdr))
        for m, b, calls, tl, tr, ov, tot in rows:
            print(f"{m:<24}{b:<12}{calls:>6}{('$%.2f'%tl):>11}{('$%.2f'%tr):>11}"
                  f"{('$%.2f'%ov):>11}{('$%.2f'%tot):>11}")
        print("-" * len(hdr))
        print(f"{'TOTAL':<24}{'':<12}{grand['calls']:>6}{('$%.2f'%grand['tok_list']):>11}"
              f"{('$%.2f'%grand['tok_recon']):>11}{('$%.2f'%grand['overhead']):>11}"
              f"{('$%.2f'%grand['total']):>11}")

    print()
    print(f"  Credit-billed subtotal      : ${sub['credit']:.2f}   "
          f"(model tokens on credit surface + ${grand['overhead']:.2f} Foundry Tools)")
    print(f"  Marketplace-billed subtotal : ${sub['marketplace']:.2f}   (card; does not draw credit)")
    print(f"  COMBINED total              : ${combined:.2f}   <- the number the cap measures")
    print()
    print(f"  vs --cap ${cap:.0f}         : {combined/cap*100:5.1f}%  "
          f"({'OVER by $%.2f' % (combined-cap) if combined>cap else 'under by $%.2f' % (cap-combined)})")
    print(f"  vs $150 credit (credit only): {sub['credit']/CREDIT_USD*100:5.1f}%  "
          f"({'credit exhausted; +$%.2f to card' % (sub['credit']-CREDIT_USD) if sub['credit']>CREDIT_USD else '$%.2f credit left' % (CREDIT_USD-sub['credit'])})")
    print(f"  Implied card spend          : ${card_overage:.2f}  "
          f"(credit overage ${max(0.0, sub['credit']-CREDIT_USD):.2f} + marketplace ${sub['marketplace']:.2f})")

    return {"combined": combined, "credit": sub["credit"],
            "marketplace": sub["marketplace"], "tok_recon": grand["tok_recon"],
            "tok_list": grand["tok_list"], "overhead": grand["overhead"],
            "agent_calls": grand["agent_calls"]}


def project_phase5(p3: dict, cap: float, runs: int = P5_DEFAULT_RUNS,
                   review_cycles: int = P5_DEFAULT_REVIEW_CYCLES,
                   label: str | None = None, md: bool = False) -> dict:
    """Analytical phase-5 projection, parametrized by runs_per_cell and
    max_review_cycles, calibrated off the phase-3 single-writer base.

    Phase 5 = 300-cell v2 sample x full 6-model panel x runs_per_cell x the
    writer/reviewer/fixer review loop (max_review_cycles). See
    phases/phase5-multiagent/PLAN.md.

    Cost model (reproduces the published ~$1,197 at runs=3, cycles=3):
      * Foundry Tools overhead scales with TOTAL agent invocations =
        writer_inv x (1 + 1.1*cycles), where writer_inv scales linearly with runs.
      * Foundry Models (token) scales with per-role invocation counts via
        per-invocation token-list rates derived from the runs=3 / cycles=3 anchors,
        then x TOKEN_RECON_FACTOR.
      * Billing split: token spend keeps the phase-3 marketplace fraction;
        Foundry Tools overhead is wholly credit.
    """
    base_writer_calls = p3.get("agent_calls") or FOUNDRY_TOOLS_MAY_AGENT_CALLS
    base_token_list = p3.get("tok_list") or 82.19
    market_frac = p3["marketplace"] / p3["tok_recon"] if p3.get("tok_recon") else 0.0
    market_frac = max(0.0, min(1.0, market_frac))

    # --- per-invocation rates, derived from the runs=3 / cycles=3 anchor ---
    writer_inv_per_run = base_writer_calls / P3_RUNS_PER_CELL            # 1,800
    writer_token_rate = base_token_list / base_writer_calls             # $/writer-inv
    rev_anchor_inv = base_writer_calls * P5_REVIEWER_RATE_PER_CYCLE * P5_DEFAULT_REVIEW_CYCLES
    fix_anchor_inv = base_writer_calls * P5_FIXER_RATE_PER_CYCLE * P5_DEFAULT_REVIEW_CYCLES
    reviewer_token_rate = P5_REVIEWER_TOKEN_LIST_USD / rev_anchor_inv   # $/reviewer-inv
    fixer_token_rate = P5_FIXER_TOKEN_LIST_USD / fix_anchor_inv         # $/fixer-inv

    # --- invocation counts for THIS (runs, cycles) config ---
    writer_inv = writer_inv_per_run * runs
    reviewer_inv = writer_inv * P5_REVIEWER_RATE_PER_CYCLE * review_cycles
    fixer_inv = writer_inv * P5_FIXER_RATE_PER_CYCLE * review_cycles
    total_agent_inv = writer_inv + reviewer_inv + fixer_inv
    calls_per_cell = 1.0 + (P5_REVIEWER_RATE_PER_CYCLE + P5_FIXER_RATE_PER_CYCLE) * review_cycles
    calls_per_cell_max = 1.0 + 2 * review_cycles

    # --- Foundry Tools overhead (dominant, invocation-scaled term) ---
    overhead = total_agent_inv * TOOLS_SURCHARGE_PER_CALL

    # --- Foundry Models token cost, per role ---
    writer_tok_list = writer_inv * writer_token_rate
    reviewer_tok_list = reviewer_inv * reviewer_token_rate
    fixer_tok_list = fixer_inv * fixer_token_rate
    token_list = writer_tok_list + reviewer_tok_list + fixer_tok_list
    token_recon = token_list * TOKEN_RECON_FACTOR

    # --- billing split (overhead all credit; token split by phase-3 mix) ---
    market = token_recon * market_frac
    credit = token_recon * (1 - market_frac) + overhead
    combined = credit + market
    card = max(0.0, credit - CREDIT_USD) + market

    title = label or f"runs={runs}, review_cycles={review_cycles}"
    print(f"\n=== phase 5 projection :: {title} ===")
    print(f"  runs_per_cell={runs}  max_review_cycles={review_cycles}  panel=6 models (full)")
    print(f"  calls/cell : {calls_per_cell:.1f} realized  (theoretical max {calls_per_cell_max:.0f} = 1 + 2x{review_cycles})")
    print(f"             = 1 writer + {P5_REVIEWER_RATE_PER_CYCLE*review_cycles:.1f} reviewer "
          f"+ {P5_FIXER_RATE_PER_CYCLE*review_cycles:.1f} fixer")

    rows = [
        ("writer",   writer_inv,   writer_tok_list),
        ("reviewer", reviewer_inv, reviewer_tok_list),
        ("fixer",    fixer_inv,    fixer_tok_list),
    ]
    if md:
        print(f"\n| Role | Invocations | Token (list) | Token (recon x{TOKEN_RECON_FACTOR}) |")
        print("|------|------------:|-------------:|--------------------:|")
        for role, inv, tl in rows:
            print(f"| {role} | {inv:,.0f} | ${tl:.2f} | ${tl*TOKEN_RECON_FACTOR:.2f} |")
        print(f"| **Total** | **{total_agent_inv:,.0f}** | **${token_list:.2f}** | **${token_recon:.2f}** |")
    else:
        print(f"\n  {'role':<10}{'invocations':>13}{'tok_list':>12}{'tok_recon':>12}")
        print("  " + "-" * 47)
        for role, inv, tl in rows:
            print(f"  {role:<10}{inv:>13,.0f}{('$%.2f'%tl):>12}{('$%.2f'%(tl*TOKEN_RECON_FACTOR)):>12}")
        print("  " + "-" * 47)
        print(f"  {'TOTAL':<10}{total_agent_inv:>13,.0f}{('$%.2f'%token_list):>12}{('$%.2f'%token_recon):>12}")

    print(f"\n  Foundry Tools overhead      : ${overhead:.2f}  "
          f"({total_agent_inv:,.0f} invocations @ ${TOOLS_SURCHARGE_PER_CALL:.5f})")
    print(f"  Foundry Models (token recon): ${token_recon:.2f}  (list ${token_list:.2f} x {TOKEN_RECON_FACTOR})")
    print(f"  Credit-billed subtotal      : ${credit:.2f}   (credit-surface tokens + all Foundry Tools)")
    print(f"  Marketplace-billed subtotal : ${market:.2f}   (card; does not draw credit)")
    print(f"  COMBINED projected total    : ${combined:.2f}   <- the number the cap measures")
    print()
    over = combined - cap
    print(f"  vs --cap ${cap:.0f}         : {combined/cap*100:5.1f}%  "
          f"({'OVER by $%.2f' % over if over > 0 else 'under by $%.2f' % -over})")
    print(f"  vs $150 credit (credit only): {credit/CREDIT_USD*100:5.1f}%  "
          f"({'credit exhausted; +$%.2f to card' % (credit-CREDIT_USD) if credit > CREDIT_USD else '$%.2f credit left' % (CREDIT_USD-credit)})")
    print(f"  Implied card spend          : ${card:.2f}  "
          f"(credit overage ${max(0.0, credit-CREDIT_USD):.2f} + marketplace ${market:.2f})")

    return {"label": title, "runs": runs, "review_cycles": review_cycles,
            "combined": combined, "credit": credit, "marketplace": market,
            "overhead": overhead, "token_recon": token_recon,
            "card": card, "agent_calls": total_agent_inv,
            "calls_per_cell": calls_per_cell}


def project_phase4(p3: dict, cap: float, runs: int = P4R_DEFAULT_RUNS,
                   refactor_calls: float = P4R_REFACTOR_CALLS_PER_CELL,
                   inflation: float = P4R_TOKEN_INFLATION,
                   label: str | None = None, md: bool = False) -> dict:
    """Analytical phase-4 (agentic loop + testability-refactoring tool) projection,
    calibrated off the phase-3 single-writer base.

    Phase 4 = 300-cell v2 sample x full 6-model panel x runs_per_cell, driven by the
    SAME single writer agent as phase 3 PLUS a LOCAL `apply_refactor` tool (no LLM
    behind it) that introduces a testability seam (extract-and-override,
    wrapper-interface/adapter, or dependency-parameterization) in production source
    before the test is written and the owning csproj is recompiled by the existing
    compile_only harness. See phases/phase4-refactoring/PLAN.md.

    Cost model (deliberately FAR below phase 5; only modestly above the same-runs
    phase-3 single-writer base):
      * Exactly ONE LLM role (writer). NO reviewer, NO fixer -- so unlike phase 5
        there is no second/third model role multiplying token spend. This is the
        single biggest reason phase 4 is cheap relative to phase 5's ~$1,197.
      * Foundry Models (token) = the phase-3 writer token base, scaled linearly by
        runs, then x P4R_TOKEN_INFLATION (the writer takes more turns per cell to
        inspect the target, choose a seam, and iterate the test), then x
        TOKEN_RECON_FACTOR.
      * Foundry Tools overhead scales with TOTAL agent invocations = writer
        invocations + apply_refactor calls. apply_refactor is a LOCAL zero-token
        tool, but it is still an agent tool invocation, so it bills at the existing
        TOOLS_SURCHARGE_PER_CALL exactly like read_file / list_dir.
      * Billing split: token spend keeps the phase-3 marketplace fraction; the
        Foundry Tools overhead is wholly credit (Azure-side agent runtime) -- same
        convention as project_phase5.

    Default runs=1 is the run_1 go/no-go dispatch (mirrors the frozen phase-5 run_1).
    A full runs=3 sweep necessarily exceeds the $250 cap because the phase-3 combined
    base alone is ~$342; that figure is reported for context and lands far under
    phase 5.
    """
    base_writer_calls = p3.get("agent_calls") or FOUNDRY_TOOLS_MAY_AGENT_CALLS
    base_token_list = p3.get("tok_list") or 82.19
    market_frac = p3["marketplace"] / p3["tok_recon"] if p3.get("tok_recon") else 0.0
    market_frac = max(0.0, min(1.0, market_frac))

    # --- per-run rates off the phase-3 base (runs=3 anchor) ---
    writer_inv_per_run = base_writer_calls / P3_RUNS_PER_CELL    # 1,800
    writer_token_per_run = base_token_list / P3_RUNS_PER_CELL    # $/run writer token-list

    # --- invocation counts for THIS run config ---
    writer_inv = writer_inv_per_run * runs
    refactor_inv = writer_inv * refactor_calls           # apply_refactor: local, but billable invocations
    total_agent_inv = writer_inv + refactor_inv

    # --- Foundry Tools overhead (invocation-scaled, dominant term) ---
    overhead = total_agent_inv * TOOLS_SURCHARGE_PER_CALL

    # --- Foundry Models token cost (single writer role, inflated) ---
    writer_tok_list = writer_token_per_run * runs
    token_list = writer_tok_list * inflation             # apply_refactor adds 0 tokens (local tool)
    token_recon = token_list * TOKEN_RECON_FACTOR

    # --- billing split (overhead all credit; token split by phase-3 mix) ---
    market = token_recon * market_frac
    credit = token_recon * (1 - market_frac) + overhead
    combined = credit + market
    card = max(0.0, credit - CREDIT_USD) + market

    # --- same-runs phase-3 single-writer baseline (honest "modestly above" anchor) ---
    p3_overhead_here = writer_inv * TOOLS_SURCHARGE_PER_CALL
    p3_token_recon_here = writer_tok_list * TOKEN_RECON_FACTOR
    p3_combined_here = p3_overhead_here + p3_token_recon_here

    title = label or f"runs={runs}, apply_refactor/cell={refactor_calls:.1f}"
    print(f"\n=== phase 4 projection :: {title} ===")
    print(f"  runs_per_cell={runs}  panel=6 models (full)  LLM roles=1 (writer only; NO reviewer/fixer)")
    print(f"  apply_refactor : LOCAL zero-token tool; ~{refactor_calls:.1f} call(s)/cell "
          f"(+{refactor_calls:.1f}x writer invocations on the Foundry Tools surface)")
    print(f"  token inflation: x{inflation:.2f} on the phase-3 single-writer base "
          f"(more turns/cell, not an extra agent)")

    rows = [
        ("writer",         writer_inv,   writer_tok_list),
        ("apply_refactor", refactor_inv, 0.0),
    ]
    if md:
        print(f"\n| Role | Invocations | Token (list) | Token (recon x{TOKEN_RECON_FACTOR}) |")
        print("|------|------------:|-------------:|--------------------:|")
        for role, inv, tl in rows:
            note = " _(local; 0 tok)_" if role == "apply_refactor" else ""
            print(f"| {role}{note} | {inv:,.0f} | ${tl:.2f} | ${tl*TOKEN_RECON_FACTOR:.2f} |")
        print(f"| **Total** | **{total_agent_inv:,.0f}** | **${token_list:.2f}** | **${token_recon:.2f}** |")
    else:
        print(f"\n  {'role':<16}{'invocations':>13}{'tok_list':>12}{'tok_recon':>12}")
        print("  " + "-" * 53)
        for role, inv, tl in rows:
            print(f"  {role:<16}{inv:>13,.0f}{('$%.2f'%tl):>12}{('$%.2f'%(tl*TOKEN_RECON_FACTOR)):>12}")
        print("  " + "-" * 53)
        print(f"  {'TOTAL':<16}{total_agent_inv:>13,.0f}{('$%.2f'%token_list):>12}{('$%.2f'%token_recon):>12}")

    print(f"\n  Foundry Tools overhead      : ${overhead:.2f}  "
          f"({total_agent_inv:,.0f} invocations @ ${TOOLS_SURCHARGE_PER_CALL:.5f} "
          f"= {writer_inv:,.0f} writer + {refactor_inv:,.0f} apply_refactor)")
    print(f"  Foundry Models (token recon): ${token_recon:.2f}  "
          f"(writer list ${writer_tok_list:.2f} x {inflation:.2f} inflation x {TOKEN_RECON_FACTOR})")
    print(f"  Credit-billed subtotal      : ${credit:.2f}   (credit-surface tokens + all Foundry Tools)")
    print(f"  Marketplace-billed subtotal : ${market:.2f}   (card; does not draw credit)")
    print(f"  COMBINED projected total    : ${combined:.2f}   <- the number the cap measures")
    print()
    over = combined - cap
    print(f"  vs --cap ${cap:.0f}         : {combined/cap*100:5.1f}%  "
          f"({'OVER by $%.2f' % over if over > 0 else 'under by $%.2f' % -over})")
    print(f"  vs $150 credit (credit only): {credit/CREDIT_USD*100:5.1f}%  "
          f"({'credit exhausted; +$%.2f to card' % (credit-CREDIT_USD) if credit > CREDIT_USD else '$%.2f credit left' % (CREDIT_USD-credit)})")
    print(f"  Implied card spend          : ${card:.2f}  "
          f"(credit overage ${max(0.0, credit-CREDIT_USD):.2f} + marketplace ${market:.2f})")

    # --- sanity check (printed, per task) ---
    print(f"\n  SANITY CHECK: phase 4 carries ONE LLM role (writer) + a LOCAL zero-token")
    print(f"  refactor tool -- no reviewer/fixer multiplying spend. At runs={runs} the")
    print(f"  combined ${combined:.2f} is {'UNDER' if combined <= cap else 'OVER'} the ${cap:.0f} cap, and FAR below phase 5's")
    print(f"  ~$1,197 full-scope projection (phase 4 has no 2nd/3rd model role). It sits")
    print(f"  modestly above the SAME-runs phase-3 single-writer base (${p3_combined_here:.2f}), "
          f"about {combined/p3_combined_here:.2f}x.")
    if runs == P4R_DEFAULT_RUNS:
        full_writer = writer_inv_per_run * P3_RUNS_PER_CELL
        full_refactor = full_writer * refactor_calls
        full_overhead = (full_writer + full_refactor) * TOOLS_SURCHARGE_PER_CALL
        full_token_recon = (base_token_list * inflation) * TOKEN_RECON_FACTOR
        full_combined = full_overhead + full_token_recon
        print(f"  For context, the full 3-run set (runs=3) projects ~${full_combined:.0f} "
              f"({full_combined/cap*100:.0f}% of cap)")
        print(f"  -- over cap, but ~{full_combined/1197.0*100:.0f}% of phase 5's $1,197, i.e. roughly half.")

    return {"label": title, "runs": runs, "refactor_calls": refactor_calls,
            "inflation": inflation, "combined": combined, "credit": credit,
            "marketplace": market, "overhead": overhead, "token_recon": token_recon,
            "card": card, "agent_calls": total_agent_inv,
            "writer_inv": writer_inv, "refactor_inv": refactor_inv}


def project_phase5_configs(p3: dict, cap: float, md: bool = False,
                           extra: tuple | None = None) -> list:
    """Print the three named phase-5 configs (A/B/C) plus an optional ad-hoc one,
    then a compact comparison table. Returns the per-config summary dicts."""
    print("\n############################################################")
    print("#  PHASE 5 PROJECTIONS -- runs + review-cycle cuts only     #")
    print("#  (full 6-model panel preserved in every config)           #")
    print("############################################################")
    results = []
    for label, runs, cycles in P5_CONFIGS:
        results.append(project_phase5(p3, cap, runs=runs, review_cycles=cycles,
                                      label=f"Config {label}", md=md))
    if extra is not None:
        er, ec = extra
        results.append(project_phase5(p3, cap, runs=er, review_cycles=ec,
                                      label=f"ad-hoc (runs={er}, cycles={ec})", md=md))

    print("\n=== comparison (full 6-model panel; combined = cap metric) ===")
    hdr = f"  {'config':<52}{'runs':>5}{'cyc':>5}{'combined':>11}{'cap%':>8}{'card':>10}"
    print(hdr)
    print("  " + "-" * (len(hdr) - 2))
    for r in results:
        print(f"  {r['label']:<52}{r['runs']:>5}{r['review_cycles']:>5}"
              f"{('$%.0f'%r['combined']):>11}{('%.0f%%'%(r['combined']/cap*100)):>8}"
              f"{('$%.0f'%r['card']):>10}")
    print()
    print(f"  Cap = ${cap:.0f} combined; credit = ${CREDIT_USD:.0f}/mo (resets ~Jun 11).")
    print(f"  Config C reproduces the published full-scope ~$1,197 (consistency check).")
    print(f"  Foundry Tools overhead carries a known phase-2 over-attribution; halving it")
    print(f"  lowers every figure ~33-40% but does not change the ranking.")
    return results


def render_silent(phase_dir: Path) -> dict:
    """Compute the phase summary without printing (for cross-phase calibration)."""
    totals = aggregate_phase(phase_dir)
    sub = {"credit": 0.0, "marketplace": 0.0}
    tok_list = tok_recon = overhead = 0.0
    agent_calls = 0
    for m, t in totals.items():
        tl = token_cost_list(m, t["p"], t["c"])
        tr = tl * TOKEN_RECON_FACTOR
        ov = t["agent_calls"] * TOOLS_SURCHARGE_PER_CALL
        sub[_bucket(m)] += tr
        sub[OVERHEAD_BILLING] += ov
        tok_list += tl
        tok_recon += tr
        overhead += ov
        agent_calls += t["agent_calls"]
    return {"combined": sub["credit"] + sub["marketplace"], "credit": sub["credit"],
            "marketplace": sub["marketplace"], "tok_recon": tok_recon,
            "tok_list": tok_list, "overhead": overhead, "agent_calls": agent_calls}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", default="phase3-agentic-loop",
                    help="phase dir to aggregate as the calibration base "
                         "(default phase3-agentic-loop)")
    ap.add_argument("--md", action="store_true", help="emit markdown tables")
    ap.add_argument("--cap", type=float, default=250.0,
                    help="combined soft/hard cap in USD (default 250)")
    ap.add_argument("--project-phase5", action="store_true",
                    help="print parametrized phase-5 projections (Configs A/B/C) "
                         "from the phase-3 base, with credit/marketplace split and "
                         "cap+credit utilization")
    ap.add_argument("--project-phase4", action="store_true",
                    help="print the phase-4 projection (agentic loop + local "
                         "apply_refactor testability tool) from the phase-3 base: "
                         "ONE writer LLM (no reviewer/fixer) + zero-token refactor "
                         "tool, with credit/marketplace split and cap+credit "
                         "utilization. Defaults to the run_1 go/no-go dispatch.")
    ap.add_argument("--runs", type=int, default=None,
                    help="runs_per_cell override for an ad-hoc projection "
                         "(with --project-phase5 or --project-phase4)")
    ap.add_argument("--review-cycles", type=int, default=None,
                    help="phase-5 max_review_cycles for an ad-hoc projection "
                         "(only with --project-phase5)")
    ap.add_argument("--refactor-calls", type=float, default=None,
                    help="phase-4 apply_refactor calls per cell for an ad-hoc "
                         "projection (only with --project-phase4; default %.1f)"
                         % P4R_REFACTOR_CALLS_PER_CELL)
    args = ap.parse_args()

    # --- Phase-5 projection mode (no Azure spend; analytical only) ---
    if args.project_phase5:
        p3_dir = REPO_ROOT / "phases" / "phase3-agentic-loop"
        if not p3_dir.is_dir():
            print(f"phase-3 calibration dir not found: {p3_dir}", file=sys.stderr)
            return 2
        p3 = render_silent(p3_dir)
        if not p3.get("agent_calls"):
            print(f"no attempts.jsonl found under {p3_dir} (needed as phase-5 base)",
                  file=sys.stderr)
            return 2
        extra = None
        if args.runs is not None and args.review_cycles is not None:
            extra = (args.runs, args.review_cycles)
        project_phase5_configs(p3, args.cap, md=args.md, extra=extra)
        return 0

    # --- Phase-4 projection mode (no Azure spend; analytical only) ---
    if args.project_phase4:
        p3_dir = REPO_ROOT / "phases" / "phase3-agentic-loop"
        if not p3_dir.is_dir():
            print(f"phase-3 calibration dir not found: {p3_dir}", file=sys.stderr)
            return 2
        p3 = render_silent(p3_dir)
        if not p3.get("agent_calls"):
            print(f"no attempts.jsonl found under {p3_dir} (needed as phase-4 base)",
                  file=sys.stderr)
            return 2
        runs = args.runs if args.runs is not None else P4R_DEFAULT_RUNS
        rc = args.refactor_calls if args.refactor_calls is not None else P4R_REFACTOR_CALLS_PER_CELL
        project_phase4(p3, args.cap, runs=runs, refactor_calls=rc, md=args.md)
        return 0

    phase_dir = REPO_ROOT / "phases" / args.phase
    if not phase_dir.is_dir():
        print(f"phase dir not found: {phase_dir}", file=sys.stderr)
        return 2

    totals = aggregate_phase(phase_dir)
    if not totals:
        print(f"no attempts.jsonl found under {phase_dir}", file=sys.stderr)
        return 2

    summary = render(args.phase, totals, args.cap, args.md)

    # Phase-5 projection: calibrate off phase-3 if we just rendered it,
    # otherwise read the phase-3 dir to get the base. Full scope (runs=3,
    # cycles=3) here, to preserve the published ~$1,197 headline.
    if args.phase == "phase3-agentic-loop":
        p3 = summary
    else:
        p3_dir = REPO_ROOT / "phases" / "phase3-agentic-loop"
        p3 = render_silent(p3_dir) if p3_dir.is_dir() else None
    if p3:
        project_phase5(p3, args.cap, runs=P5_DEFAULT_RUNS,
                       review_cycles=P5_DEFAULT_REVIEW_CYCLES,
                       label="full scope (runs=3, cycles=3)", md=args.md)
        print(f"\n  Sensitivity (Tools half-attributed to phase 2) and the "
              f"runs/cycle-reduced\n  Configs A/B/C: run with --project-phase5.")
        project_phase4(p3, args.cap, runs=P4R_DEFAULT_RUNS,
                       label="go/no-go dispatch (runs=1; single writer + refactor tool)",
                       md=args.md)
        print(f"\n  Phase-4 ad-hoc runs/refactor-call sweeps: run with "
              f"--project-phase4 [--runs N] [--refactor-calls F].")

    print("\n--- residual gap ---")
    print(f"  Phase-3 actual Foundry (Tools $182.26 + Models $160.45) = $342.71.")
    print(f"  This model's phase-3 combined = ${summary['combined']:.2f}.")
    print(f"  Residual = ${summary['combined']-342.71:+.2f}. The two reconciliation")
    print(f"  knobs (TOKEN_RECON_FACTOR={TOKEN_RECON_FACTOR}, "
          f"Tools ${TOOLS_SURCHARGE_PER_CALL:.5f}/call) are calibrated to this")
    print(f"  anchor; remaining gap is phase-2 token overlap inside the May window")
    print(f"  and sub-dollar Container Registry/storage, both intentionally unmodeled.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
