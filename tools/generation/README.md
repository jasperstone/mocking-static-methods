# Generation runner + GitHub Models adapter

Reads `targets/v{N}/targets.csv`, dispatches each row to the GitHub Models API, writes outputs into `phases/<phase-id>/results/{model_id}/run_{i}/`.

## Files

| File | Role |
|---|---|
| [`runner.py`](runner.py) | Single-shot strategy. Iterates `targets.csv`, calls the adapter, writes outputs. Default for phase 2. |
| [`adapters/github_models.py`](adapters/github_models.py) | Thin OpenAI-compatible client for `https://models.github.ai/inference`. Returns `(test_source, model_snapshot, usage)`. |
| [`prompt_render.py`](prompt_render.py) | Renders `prompt/system.md` + `prompt/user-template.md` against a target row. Mustache-style `{{...}}` substitution. |
| [`source_window.py`](source_window.py) | Pulls `[line-30, line+30]` from the target file at the pinned repo SHA so the model has surrounding context. |
| [`build_outcomes.py`](build_outcomes.py) | After CI builds + tests the generated files, this script joins the build output with `attempts.jsonl` to produce `outcome.csv`. |
| [`aggregate.py`](aggregate.py) | Walks all `results/{model}/run_{i}/outcome.csv` and emits `results/aggregate.csv`. |

## How a single cell runs

One CI matrix job (one model × one run_index) does:

```
1. checkout repo @ phase-N-name-final tag
2. read targets/v{N}/targets.csv
3. for each target row:
     a. source_window.py -> pull surrounding code from cloned_repos/{repo}@{repo_sha}
     b. prompt_render.py -> render system + user prompt
     c. github_models.py -> call API; capture (text, snapshot, usage, latency)
     d. extract csharp ```block
     e. write generated_tests/{repo}/{target_id}.cs
     f. append to attempts.jsonl
4. upload phases/<phase-id>/results/{model_id}/run_{i}/ as artifact
```

A separate downstream coverage CI run (after all 25 cells finish) builds + tests the generated files and produces `compile/`, `runtime/`, and cobertura XMLs. `build_outcomes.py` joins those back in.

## Determinism notes

- `temperature=0`, `top_p=1`, `seed=42` are pinned.
- Even with these, GitHub Models gateway routing and KV-cache batching make exact-token reproducibility impossible. The phase's headline metric is the **mean ± stddev across 5 runs**, not a single run's number.
- The `model` field of every API response is captured into `attempts.jsonl` so we know which snapshot served each request.
