# Phase 2 — Agentic Loop: Results & Costs

This phase pivoted from naive single-shot test generation to an **agentic-loop** strategy: each model gets a multi-turn budget and tools to read the codebase before submitting one xUnit test file.

The full design and findings are in [REPORT.md](REPORT.md). This file focuses on **what it costs to reproduce on a small Azure budget**, so other students and researchers can plan accordingly.

---

## Cost summary — v2 sweep (final phase 2 dataset)

All seven panel models live in **one Azure AI Foundry account** (single resource group, single endpoint, single API key). Spend is measured from per-call token counts captured in `results/<model>/run_*/attempts.jsonl` and priced against published Azure list rates (captured 2026-05-12).

Scope: **300 cells × 3 runs × 7 models = 6,300 generation attempts** dispatched via GitHub Actions matrix (252 successful shards). Numbers below reflect the v2-clean committed data (a handful of v1-contaminated rows from the shakedown phase have been filtered out — they accounted for ~$0.34 of the total).

| Model | Calls | Submit rate | Prompt tokens | Completion tokens | Cost (USD) | % of spend |
|-------|------:|-------:|--------------:|------------------:|-----------:|-----------:|
| `gpt-5-codex` | 901 | 17.9% | 3,245,098 | 6,934,171 | **$73.40** | **82%** |
| `codestral-2501` | 901 | 79.2% | 14,738,360 | 724,628 | $5.07 | 6% |
| `llama-3.3-70b-instruct` | 901 | 85.4% | 6,237,905 | 849,586 | $5.03 | 6% |
| `gpt-4.1-mini` | 901 | 84.8% | 5,246,116 | 833,896 | $3.43 | 4% |
| `phi-4` | 901 | 94.1% | 6,347,714 | 1,416,975 | $1.50 | 2% |
| `gpt-4.1-nano` | 901 | 54.9% | 10,331,312 | 474,799 | $1.22 | 1% |
| `grok-4-1-fast` | 901 | 13.2% | 1,231,977 | 143,947 | $0.32 | <1% |
| **Total** | **6,307** | **61.4%** | **47,378,482** | **11,378,002** | **$89.98** | 100% |

> Regenerate this table any time with `python3 tools/cost/estimate.py --phase phase2-agentic --md`

### Headline reading

- **`gpt-5-codex` consumed 82% of phase 2 spend** while submitting fewer than 1 in 5 attempts. Its reasoning tier burned 6.96M completion tokens — more than the other six models combined (4.46M) — but most of those tokens were internal "thinking" that never produced a submittable test.
- **Removing codex from the panel cuts per-cell cost ~5×** ($89.98 → $16.58) while preserving full coverage of the 6-model panel.
- **Top three by submission rate:** `phi-4` (94.1%), `llama-3.3-70b-instruct` (85.4%), `gpt-4.1-mini` (84.9%).
- **Bottom three by submission rate:** `grok-4-1-fast` (13.6%), `gpt-5-codex` (17.8%), `gpt-4.1-nano` (54.8%). Grok's low rate is a tool-following failure; codex's is reasoning-budget exhaustion; nano runs out of context on large prompts.
- The "all-in-one Foundry account" approach matters: zero infra cost, no idle deployments to pay for, no separate marketplaces. You pay only for tokens.

### Reconciling with the Azure bill

Azure cost-management showed **$105.47 actual** for the `foundry-mockstatic` resource group over the same window. Breakdown:

| Source | Amount | Notes |
|---|---:|---|
| Foundry Models (token cost — measured) | $89.98 | This table, reconstructed from `attempts.jsonl` (v2-clean) |
| Foundry Models (v1 shakedown contamination) | ~$0.34 | First-run artifacts mixed v1 IDs in; filtered out of committed data |
| Foundry Tools / Cognitive Services overhead | ~$11 | Container registry, identity, monitoring, network egress |
| Storage / misc | ~$4 | Logs, blob, key vault |
| **Azure total** | **~$105** | Matches portal within rounding |

The ~$15 gap between token-cost and Azure bill is **non-token infrastructure** that scales with account presence, not with usage. It will be roughly constant across remaining phases.

---

## Decision: drop `gpt-5-codex` from phases 3-5

After phase 2 we are removing `gpt-5-codex` from `FULL_PANEL` in `.github/scripts/plan_matrix.py`. Justification:

1. **Cost asymmetry.** 82% of phase 2 spend went to one model with the worst submission rate of any reasoning-capable model in the panel.
2. **Diminishing return.** The phase-2 codex run is preserved in `results/gpt-5-codex/` so we already have its agentic-loop baseline. Phases 3-5 change *generation strategy*, not the model panel — codex's per-strategy delta can be inferred from the cheaper panel members.
3. **Budget headroom.** The remaining three phases (agentic loop with compile feedback, multi-agent, multi-team) chain longer per cell. Codex would multiply that chain by 5-10× and consume the entire remaining budget on one model.
4. **No resampling.** The 300-cell v2 target set is preserved exactly — the same cells run in every subsequent phase. Removing codex changes the model panel only, not the sample. Cross-phase deltas remain valid because every later phase tests the same (target, file, method) tuples the v2 sweep used.

If a future phase specifically tests reasoning-tier behavior, codex can be re-added for a single targeted run.

### What's NOT in this number

- **Azure infra (RG, AI Services account).** ~$11/month, see reconciliation above.
- **GitHub Actions compute.** $0 — public repo on free unlimited Actions minutes.
- **Storage, networking, egress.** Bundled into the $11/month infra overhead.
- **Smoke / ping tests run from `tools/generation/foundry_smoke.py`** — a few hundred extra tokens per model, ~$0.001, not double-counted above.

---

## Budget guardrails

- **$50/month budget alert** on the resource group (`budget-mockstatic-50`), with email alerts at 50% / 80% / 100% actual + 80% forecast → my outlook.com address.
- **Azure VS Subscription credit** (~$150/mo) covers infra and most pay-per-token spend.
- **Credit card backstop** for marketplace items (e.g. Anthropic / Mistral serverless) when the credit is exhausted.

Phase 2 exceeded the $50 alert because of codex. With codex removed and the same sample set preserved, projected per-phase cost is back inside the budget envelope (see projections below).

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

## Cost projections for phases 3-5

Holding the same 300-cell v2 sample set across all phases (no resampling). Codex removed from the panel.

Per-cell average for the **6-model panel** (codex excluded): $16.58 / (300 × 3) = **$0.0184 per attempt**.

Phases 3-5 chain more turns per attempt, so per-attempt cost scales with average chain length. Conservative multipliers below:

| Phase | Strategy | Chain mult. | Est. cost (6 models × 300 × 3) |
|---|---|---:|---:|
| Phase 2 (this phase) | Agentic loop, single agent | 1.0× | **$16.58 measured (ex-codex)** |
| Phase 3 | Agentic loop + compile feedback | 2-3× | ~$33-50 |
| Phase 4 | Multi-agent (writer + critic) | 3-4× | ~$50-67 |
| Phase 5 | Multi-team coordination | 4-6× | ~$67-100 |
| **Remaining phases total** | | | **~$150-217** |

Phase 2 retrospectively cost $89.98 with codex; the same sweep without codex would have cost $16.58. The remaining-phase projection is built on that lower base.
