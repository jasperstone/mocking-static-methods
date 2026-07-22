# Phase 4 Canonical Run Provenance

Date: 2026-07-21T15:48:22Z
Requested by: Jasper

This note records the source-of-truth model-to-run mapping used to refresh canonical Phase 4 results before regenerating diagnostics.

## Target panel models and mapped latest successful full-coverage runs

- codestral-2501 -> 28439893529 (latest successful run with run1..run3 artifacts present in metadata; artifacts currently expired in GitHub Actions)
- gpt-4.1-mini -> 28804908783 (latest successful run with run1..run3 artifacts present in metadata; artifacts currently expired in GitHub Actions)
- gpt-4.1-nano -> 28804913820 (latest successful run with run1..run3 artifacts present in metadata; artifacts currently expired in GitHub Actions)
- grok-4-1-fast -> 29612308097 (latest successful run with run1..run3 artifacts; artifacts live and downloaded)
- llama-3.3-70b-instruct -> 29612257530 (latest successful run with run1..run3 artifacts; artifacts live and downloaded)
- phi-4 -> 29846768297 (latest successful run with phi-focused reruns; artifacts downloaded and incorporated)

## Canonical refresh actions

- Rebuilt canonical attempts for grok-4-1-fast from run 29612308097 by aggregating all run chunk artifacts into:
  - phases/phase4-refactoring/results/grok-4-1-fast/run_1/attempts.jsonl
  - phases/phase4-refactoring/results/grok-4-1-fast/run_2/attempts.jsonl
  - phases/phase4-refactoring/results/grok-4-1-fast/run_3/attempts.jsonl
- Rebuilt canonical attempts for llama-3.3-70b-instruct from run 29612257530 by aggregating all run chunk artifacts into:
  - phases/phase4-refactoring/results/llama-3.3-70b-instruct/run_1/attempts.jsonl
  - phases/phase4-refactoring/results/llama-3.3-70b-instruct/run_2/attempts.jsonl
  - phases/phase4-refactoring/results/llama-3.3-70b-instruct/run_3/attempts.jsonl
- Patched canonical attempts for phi-4 from run 29846768297 by replacing overlapping target rows per repo for each run shard:
  - phases/phase4-refactoring/results/phi-4/run_1/attempts.jsonl
  - phases/phase4-refactoring/results/phi-4/run_2/attempts.jsonl
  - phases/phase4-refactoring/results/phi-4/run_3/attempts.jsonl
- For codestral-2501, gpt-4.1-mini, and gpt-4.1-nano, latest mapped artifacts remain expired in GitHub Actions and were retained from existing canonical local results.

## Related phase 3 canonical refresh (same diagnostics cycle)

- Applied latest successful combined rerun 29849541902 for:
  - gpt-4.1-mini
  - gpt-4.1-nano
- Canonical phase 3 attempts were patched in:
  - phases/phase3-agentic-loop/results/gpt-4.1-mini/run_1..run_3/attempts.jsonl
  - phases/phase3-agentic-loop/results/gpt-4.1-nano/run_1..run_3/attempts.jsonl

## Diagnostics regenerated after canonical refresh

- python3 tools/analysis/phase4_failure_categorization.py --phases phase2-agentic,phase3-agentic-loop,phase4-refactoring
- python3 tools/analysis/phase4_failure_categorization.py --phase phase4-refactoring

These diagnostics were regenerated from canonical data after applying the latest downloadable full-coverage reruns and retaining non-downloadable mapped models with explicit provenance.
