# Phase 2 — Agentic Loop: Results & Costs

This phase pivoted from naive single-shot test generation to an **agentic-loop** strategy: each model gets a multi-turn budget and tools to read the codebase before submitting one xUnit test file.

The full design and findings are in [REPORT.md](REPORT.md). This file focuses on **what it costs to reproduce on a small Azure budget**, so other students and researchers can plan accordingly.

---

## Cost summary (this phase to date)

All seven models live in **one Azure AI Foundry account** (single resource group, single endpoint, single API key). Spend is measured from per-call token counts captured in `results/<model>/run_*/attempts.jsonl` and priced against published Azure list rates.

Pilot scope so far: **7 models × up to 5 cells × 1 run + smoke tests + prompt-iteration spikes** = 37 generation calls across the panel.

| Model | Calls | Submit rate | Prompt tokens | Completion tokens | Cost (USD) |
|-------|------:|-------:|--------------:|------------------:|-----------:|
| `codestral-2501` | 6 | 5/6 | 51,570 | 4,103 | $0.0192 |
| `gpt-4.1-mini` | 6 | 6/6 | 30,158 | 4,193 | $0.0188 |
| `gpt-4.1-nano` | 6 | 3/6 | 46,393 | 2,047 | $0.0055 |
| `gpt-5-codex` | 6 | 0/6 | 14,757 | 45,585 | $0.4743 |
| `grok-4-1-fast` | 6 | 6/6 | 39,588 | 5,671 | $0.0108 |
| `llama-3.3-70b-instruct` | 6 | 6/6 | 45,274 | 9,309 | $0.0388 |
| `phi-4` | 1 | 1/1 | 13,386 | 2,098 | $0.0027 |
| **Total** | **37** |  | **241,126** | **73,006** | **$0.5699** |

> Regenerate this table any time with `python3 tools/cost/estimate.py --phase phase2-agentic --md`

### Headline reading

- **The whole 7-model panel pilot fit under sixty cents** — including a handful of throwaway runs while we iterated the prompt.
- **`gpt-5-codex` accounts for 83% of the spend** despite submitting nothing. Its reasoning tier burns ~10× the output tokens of any other model and bills accordingly. If you're cost-constrained, drop reasoning models from the panel until you've validated the harness.
- **`gpt-4.1-nano` is ~$0.001 per submitted test file.** Cheapest panel member by a wide margin.
- The "all-in-one Foundry account" approach matters: zero infra cost, no idle deployments to pay for, no separate marketplaces. You pay only for tokens.

### What's NOT in this number

- **Azure infra (RG, AI Services account).** $0 — all panel models are pay-per-token deployments.
- **Storage, networking, egress.** $0 — everything runs locally and writes to the local repo.
- **Smoke / ping tests run from `tools/generation/foundry_smoke.py`** — a few hundred extra tokens per model, ~$0.001 not double-counted above.

---

## Budget guardrails I set up

- **$50/month budget alert** on the resource group (`budget-mockstatic-50`), with email alerts at 50% / 80% / 100% actual + 80% forecast → my outlook.com address.
- **Azure VS Subscription credit** (~$150/mo) covers infra and most pay-per-token spend.
- **Credit card backstop** for marketplace items (e.g. Anthropic / Mistral serverless) when the credit is exhausted.

If you have a different Azure subscription tier (Free, Student, Sponsored, Pay-as-you-go), the panel still works — only the credit headroom changes. The token rates are the same.

---

## Where to see "nice graphs" of the cost

Three options, ranked by usefulness for showing advisors:

1. **Azure Portal → Cost Management + Billing → Cost analysis**
   - URL: `https://portal.azure.com/#view/Microsoft_Azure_CostManagement/Menu/~/overview`
   - Scope it to the resource group (`rg-mocking-static-experiment`) for a per-day, per-resource breakdown
   - Has built-in line / area / donut charts grouped by Service / Resource / Meter
   - **Caveat:** Cost Management has a 24-48h lag, so today's runs won't show up immediately

2. **Azure AI Foundry portal → your project → Management Center → Quota & Usage**
   - URL: `https://ai.azure.com/`
   - Per-deployment token usage charts (input / output tokens over time)
   - Faster to update than Cost Management (usually within an hour)
   - The view I find most useful for monitoring an in-flight experiment

3. **Programmatic — `tools/cost/estimate.py`** (this repo)
   - Computes USD from the actual recorded token counts, no API lag, no portal needed
   - Reproducible — anyone who clones this repo and runs the pilot gets the same numbers
   - Use this in papers / write-ups; cite the portal screenshots only as a sanity check

---

## Rough cost projections for fuller experiments

If we hold per-cell average ≈ what we measured ($0.57 / 37 calls ≈ $0.015 per call, dominated by gpt-5-codex), here's roughly what the next tiers cost — assuming we **drop gpt-5-codex** to bring per-call cost down to ~$0.003:

| Scope | Calls | Estimated USD |
|-------|------:|--------------:|
| Current pilot (7 models × 5 cells × 1 run) | 35 | ~$0.50 |
| Per-model tight rates (6 models × 30 cells × 1 run) | 180 | ~$0.50 |
| Compile-feedback loop tier (6 models × 30 cells × 1 run × 3 turns avg) | ~540 | ~$1.50 |
| Full multi-team tier (3 teams × 4 agents × 30 cells × 5 runs) | ~1800 | ~$5.00 |
| Re-include gpt-5-codex everywhere | — | +5–10× the above |

These are estimates. The real numbers will go in this file after each tier runs.
