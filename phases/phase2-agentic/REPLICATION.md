# Phase 2 — Agentic Loop: Replication

To reproduce this phase end-to-end on your own Azure subscription:

## Prerequisites

- Azure subscription (any tier) with permission to create resources in a region with Azure OpenAI + Foundry Models. We used `eastus2`.
- Python 3.11+
- .NET SDK 10.0+ (the evaluator builds tests against the cloned production projects)
- ~3 GB free disk (NuGet cache + cloned repos)
- ~$1 of Azure credit (the full pilot ran for $0.57; see [COSTS.md](COSTS.md))

## 1. Provision the Foundry account

```bash
az group create -n rg-mocking-static-experiment -l eastus2

az cognitiveservices account create \
  -n foundry-mockstatic -g rg-mocking-static-experiment -l eastus2 \
  --kind AIServices --sku S0 --custom-domain foundry-mockstatic
```

Then deploy the seven panel models from the Azure AI Foundry portal (`ai.azure.com`):

| Deployment name | Model | Surface |
|---|---|---|
| `gpt-4.1-mini` | OpenAI gpt-4.1-mini | Chat Completions |
| `gpt-4.1-nano` | OpenAI gpt-4.1-nano | Chat Completions |
| `gpt-5-codex` | OpenAI gpt-5-codex | Responses |
| `phi-4` | Microsoft phi-4 | Foundry Inference |
| `codestral-2501` | Mistral Codestral 2501 | Foundry Inference |
| `llama-3.3-70b-instruct` | Meta Llama 3.3 70B | Foundry Inference |
| `grok-4-1-fast` | xAI Grok 4.1 Fast | Foundry Inference |

## 2. Save credentials

Create `.env.foundry` at the repo root (it is `.gitignore`d):

```
FOUNDRY_ENDPOINT=https://foundry-mockstatic.cognitiveservices.azure.com/
FOUNDRY_API_KEY=<your-key>
FOUNDRY_PANEL_OPENAI_CHAT=gpt-4.1-mini,gpt-4.1-nano
FOUNDRY_PANEL_OPENAI_RESPONSES=gpt-5-codex
FOUNDRY_PANEL_INFERENCE=phi-4,codestral-2501,llama-3.3-70b-instruct,grok-4-1-fast
```

## 3. Set a budget alert

```bash
SUB=<your-subscription-id>
RG=rg-mocking-static-experiment
EMAIL=<your-email>
START=$(date -u +%Y-%m-01T00:00:00Z)
END=$(date -u -d '+12 months' +%Y-%m-01T00:00:00Z)
az rest --method put \
  --uri "https://management.azure.com/subscriptions/$SUB/resourceGroups/$RG/providers/Microsoft.Consumption/budgets/budget-mockstatic-50?api-version=2024-08-01" \
  --body "{\"properties\":{\"category\":\"Cost\",\"amount\":50,\"timeGrain\":\"Monthly\",\"timePeriod\":{\"startDate\":\"$START\",\"endDate\":\"$END\"},\"notifications\":{\"actual50\":{\"enabled\":true,\"operator\":\"GreaterThan\",\"threshold\":50,\"contactEmails\":[\"$EMAIL\"],\"thresholdType\":\"Actual\"},\"actual80\":{\"enabled\":true,\"operator\":\"GreaterThan\",\"threshold\":80,\"contactEmails\":[\"$EMAIL\"],\"thresholdType\":\"Actual\"}}}}"
```

## 4. Clone the target repos

```bash
mkdir -p cloned_repos
cd cloned_repos
git clone --depth=1 https://github.com/dotnet/eShop.git
git clone --depth=1 https://github.com/duplicati/duplicati.git
git clone --depth=1 https://github.com/microsoft/semantic-kernel.git
git clone --depth=1 https://github.com/dotnet/orleans.git
git clone --depth=1 https://github.com/jellyfin/jellyfin.git
cd ..
```

## 5. Smoke test the panel

```bash
python3 tools/generation/foundry_smoke.py
```

Should print one csharp block per panel model.

## 6. Run the pilot (5 cells × 7 models)

```bash
TARGETS="eShop:0001,duplicati:0011,semantic-kernel:0001,orleans:0001,jellyfin:0001"
for m in gpt-4.1-mini gpt-4.1-nano gpt-5-codex phi-4 codestral-2501 grok-4-1-fast llama-3.3-70b-instruct; do
  python3 tools/generation/agentic_runner.py \
    --phase phase2-agentic --model "$m" --run-index 1 \
    --target-set v1 --target-ids "$TARGETS" \
    --max-turns 6 --max-reads 5 --max-output-tokens 4096 --timeout-s 240
done
```

## 7. Evaluate (compile + run + coverage)

```bash
for m in gpt-4.1-mini gpt-4.1-nano gpt-5-codex phi-4 codestral-2501 grok-4-1-fast llama-3.3-70b-instruct; do
  python3 tools/evaluation/evaluate.py \
    --phase phase2-agentic --model "$m" --run-index 1 \
    --target-set v1 --build-timeout 240 --test-timeout 180
done
```

## 8. Get the cost number

```bash
python3 tools/cost/estimate.py --phase phase2-agentic --md
```

## Expected runtime

End-to-end: ~30–45 minutes wall-clock. Generation is dominated by gpt-5-codex (long reasoning); evaluation is dominated by first-time NuGet restore in each repo.
