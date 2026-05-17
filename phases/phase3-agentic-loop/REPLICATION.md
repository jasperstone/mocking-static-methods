# Phase 3 — Agentic Loop with Compile + Run Feedback: Replication

To reproduce the phase 3 calibration sweep end-to-end on your own Azure subscription. Same Foundry account as phase 2 — if you've already done [phase 2 replication](../phase2-agentic/REPLICATION.md), skip to **§5**.

## Prerequisites

- Azure subscription (any tier) with permission to create AI Foundry resources in `eastus2` (or comparable region).
- Python 3.11+
- .NET SDK 10.0+ (the in-loop runner sandbox **and** the canonical evaluator both invoke `dotnet build`/`dotnet test`)
- ~3 GB free disk (NuGet cache + cloned repos)
- ~$30 of Azure credit for the calibration sweep (1,800 attempts; see [COSTS.md](COSTS.md))
- ~$90 of Azure credit for the full 3-run sweep (5,400 attempts)

## 1. Provision the Foundry account

```bash
az group create -n rg-mocking-static-experiment -l eastus2

az cognitiveservices account create \
  -n foundry-mockstatic -g rg-mocking-static-experiment -l eastus2 \
  --kind AIServices --sku S0 --custom-domain foundry-mockstatic
```

Deploy the six panel models from the Azure AI Foundry portal (`ai.azure.com`).
**`gpt-5-codex` is intentionally excluded** from phase 3 (see
[phase 2 COSTS §"Decision: drop gpt-5-codex"](../phase2-agentic/COSTS.md#decision-drop-gpt-5-codex-from-phases-3-5)):

| Deployment name | Model | Surface |
|---|---|---|
| `gpt-4.1-mini` | OpenAI gpt-4.1-mini | Chat Completions |
| `gpt-4.1-nano` | OpenAI gpt-4.1-nano | Chat Completions |
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

## 4. Clone the v2 target repos

```bash
mkdir -p cloned_repos && cd cloned_repos
for r in eShop duplicati semantic-kernel orleans jellyfin abp aspnetcore efcore garnet mono roslyn server; do
  git clone --depth=1 "https://github.com/$(./resolve_org.sh $r)/$r.git"
done
cd ..
```

The v2 sample is **300 cells stratified across these 12 repos** (`targets/v2/targets.csv`,
sha256 `4db523f966ff24a469895a105db6dd011fc22bd52f401c4b5fbe83d905bb2823`).

## 5. Smoke test the panel

```bash
python3 tools/generation/foundry_smoke.py
```

Should print one csharp block per panel model.

## 6. Run the calibration sweep (run_1)

The phase 3 runner is **`agentic_runner_feedback.py`** — different from phase 2.
It feeds compile errors and TRX test failures back into the conversation as
additional turns, up to `--max-attempts` submissions per cell:

```bash
for m in gpt-4.1-mini gpt-4.1-nano phi-4 codestral-2501 grok-4-1-fast llama-3.3-70b-instruct; do
  python3 tools/generation/agentic_runner_feedback.py \
    --phase phase3-agentic-loop --model "$m" --run-index 1 \
    --target-set v2 \
    --max-turns 6 --max-reads 5 --max-attempts 4 \
    --max-output-tokens 4096 --timeout-s 240 --run-timeout-s 60
done
```

**Or — preferred — dispatch the GitHub Actions matrix** (one job per
`(model, repo, run)` shard):

```bash
gh workflow run phase3-generate.yml \
  -f target_set=v2 -f runs_per_cell=1 -f run_index_start=1 \
  -f models=all -f repos=all
```

This produces `attempts.jsonl` + `generated_tests/` artifacts that the
evaluator (§7) consumes.

## 7. Evaluate (compile + run, canonical)

The evaluator builds each submitted test **inside the production csproj**,
not the runner's standalone sandbox. Run locally:

```bash
for m in gpt-4.1-mini gpt-4.1-nano phi-4 codestral-2501 grok-4-1-fast llama-3.3-70b-instruct; do
  python3 tools/evaluation/evaluate.py \
    --phase phase3-agentic-loop --model "$m" --run-index 1 \
    --target-set v2 --build-timeout 240 --test-timeout 180
done
```

Or via GitHub Actions (recommended — uses a `dotnet/sdk:10.0-noble` container):

```bash
gh workflow run phase3-evaluate.yml \
  -f generate_run_id=<phase3-generate run id> \
  -f models=all -f repos=all
```

## 8. Aggregate and price

```bash
gh workflow run phase3-aggregate.yml -f generate_run_id=<run id>

# Or locally:
python3 tools/cost/estimate.py --phase phase3-agentic-loop --md
python3 tools/viz/aggregate_phase_results.py
```

## 9. (Optional) Continuation: runs 2 + 3

Re-dispatch `phase3-generate.yml` with `run_index_start=2` and `runs_per_cell=2`
to add the second and third runs. The evaluator and aggregator handle multi-run
data via the run-index column; figures auto-update in the [r-viz devcontainer](../../.devcontainer/r-viz/devcontainer.json).

## Expected runtime

- **Generation, full panel, run_1:** ~3-4 h wall-clock via the matrix
  (72 parallel jobs). Llama and codestral dominate; their 70B / Mistral
  inference is the long pole.
- **Evaluation, full panel, run_1:** ~2-3 h wall-clock; cold NuGet restore
  on each repo is the long pole.
- **Aggregation + cost:** seconds.

## Hyperparameters frozen for phase 3

See [phase.lock.yaml](phase.lock.yaml) for the canonical values. Briefly:

| Knob | Value | Note |
|---|---|---|
| `max_turns` | 6 | Conversation turns per cell |
| `max_reads` | 5 | `read_source` tool calls per cell |
| `max_attempts` | 4 | Submissions per cell before giving up |
| `max_output_tokens` | 4096 | Per completion |
| `timeout_s` | 240 | Per cell wall-clock |
| `run_timeout_s` | 60 | Per `dotnet test` invocation in the runner sandbox |
| `target_set` | v2 | 300-cell stratified sample, sha `4db523f9…` |
