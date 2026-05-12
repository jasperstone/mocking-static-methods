#!/usr/bin/env python3
"""Estimate USD spend for a phase from per-call token counts in attempts.jsonl.

Uses published Azure AI Foundry list prices (USD per 1M tokens). Rates change —
update PRICES below as needed. The script prints both per-model and totals so
the README can be regenerated whenever new runs land.

Usage:
    python3 tools/cost/estimate.py --phase phase2-agentic
"""
from __future__ import annotations
import argparse
import glob
import json
import sys
from collections import defaultdict
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]

# USD per 1M tokens. Sources:
#   * Azure OpenAI: https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/
#   * Foundry Models (serverless): per-model deployment page in ai.azure.com
# Captured 2026-05-12. Verify before quoting.
PRICES = {
    # Azure OpenAI Chat surface
    "gpt-4.1-mini":            {"in": 0.40, "out": 1.60},
    "gpt-4.1-nano":            {"in": 0.10, "out": 0.40},
    # Azure OpenAI Responses surface (gpt-5 family)
    "gpt-5-codex":             {"in": 1.25, "out": 10.00},
    # Foundry Models inference surface
    "phi-4":                   {"in": 0.125, "out": 0.50},
    "codestral-2501":          {"in": 0.30, "out": 0.90},
    "llama-3.3-70b-instruct":  {"in": 0.71, "out": 0.71},
    "grok-4-1-fast":           {"in": 0.20, "out": 0.50},
}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True)
    ap.add_argument("--md", action="store_true", help="emit a markdown table instead of plain text")
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    if not phase_dir.is_dir():
        print(f"phase dir not found: {phase_dir}", file=sys.stderr)
        return 2

    # Aggregate every attempts.jsonl in every run dir
    totals = defaultdict(lambda: {"p": 0, "c": 0, "calls": 0, "submitted": 0, "wall_ms": 0})
    for path in sorted(phase_dir.glob("results*/**/attempts.jsonl")):
        for line in path.open():
            r = json.loads(line)
            m = r.get("model_id", "?")
            totals[m]["p"] += r.get("total_prompt_tokens", 0)
            totals[m]["c"] += r.get("total_completion_tokens", 0)
            totals[m]["calls"] += 1
            totals[m]["submitted"] += int(r.get("submitted", False))
            totals[m]["wall_ms"] += r.get("wall_ms", 0)

    if args.md:
        print(f"| Model | Calls | Submit | Prompt tokens | Completion tokens | Cost (USD) |")
        print(f"|-------|------:|-------:|--------------:|------------------:|-----------:|")
    else:
        print(f"{'model':<26} {'calls':>5} {'sub':>4} {'p_tok':>10} {'c_tok':>10} {'cost_usd':>10}")
        print("-" * 75)

    grand_cost = 0.0
    grand_p = grand_c = grand_calls = 0
    for m in sorted(totals):
        t = totals[m]
        price = PRICES.get(m)
        if price is None:
            cost = 0.0
            cost_str = "(no rate)"
        else:
            cost = t["p"] / 1_000_000 * price["in"] + t["c"] / 1_000_000 * price["out"]
            cost_str = f"${cost:.4f}"
        grand_cost += cost
        grand_p += t["p"]; grand_c += t["c"]; grand_calls += t["calls"]
        if args.md:
            print(f"| `{m}` | {t['calls']} | {t['submitted']}/{t['calls']} | {t['p']:,} | {t['c']:,} | {cost_str} |")
        else:
            print(f"{m:<26} {t['calls']:>5} {t['submitted']:>4} {t['p']:>10,} {t['c']:>10,} {cost_str:>10}")

    if args.md:
        print(f"| **Total** | **{grand_calls}** |  | **{grand_p:,}** | **{grand_c:,}** | **${grand_cost:.4f}** |")
    else:
        print("-" * 75)
        print(f"{'TOTAL':<26} {grand_calls:>5}      {grand_p:>10,} {grand_c:>10,} ${grand_cost:>9.4f}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
