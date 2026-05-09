# Replicating this phase

## What you need

- A GitHub account (for the GitHub Models token).
- A fork of this repo, or read access to dispatch workflows in this repo.
- The same target set this phase used (pinned by `targets_version` and `targets_sha256` in [`phase.lock.yaml`](phase.lock.yaml)).

## Steps

1. **Verify the target set hasn't drifted**

   ```bash
   sha256sum targets/$(grep -oP 'targets_version: "?\K[^"]+' phases/<phase-id>/phase.lock.yaml)/targets.csv
   ```

   The output must equal `inputs.targets_sha256` in `phase.lock.yaml`. If it doesn't, you're not running against the same input — stop and check out the git tag named in `phase.git_tag`.

2. **Set the GitHub Models token as a repo secret**

   `Settings → Secrets and variables → Actions → New repository secret`:
   - Name: `GITHUB_MODELS_TOKEN`
   - Value: a token with `models:read` scope

3. **Dispatch the phase workflow**

   ```bash
   gh workflow run .github/workflows/<phase-id>.yml \
     -F target_set=$(grep -oP 'targets_version: "?\K[^"]+' phases/<phase-id>/phase.lock.yaml) \
     -F replication=true
   ```

   `replication=true` writes outputs to `phases/<phase-id>-replica-<run-id>/` instead of overwriting the canonical results.

4. **Wait** — 5 models × 5 runs = 25 jobs. They run in parallel; a phase 2 dispatch typically completes in 30–60 minutes wall-clock.

5. **Compare**

   ```bash
   diff phases/<phase-id>/results/aggregate.csv phases/<phase-id>-replica-<run-id>/results/aggregate.csv
   ```

   Differences are expected — model snapshots can shift even at temperature 0 due to gateway-side load balancing and KV-cache batching effects. The headline metric to compare is the per-model mean ± stddev block in each run's `phase.lock.yaml`. If your replica's mean for any model is more than 2σ from the canonical mean, file an issue.

## What CAN'T be reproduced

- The exact tokens emitted by closed-weight models. Even at temperature 0 there is gateway-level non-determinism.
- The exact model snapshot if the canonical run pre-dates a snapshot retirement. The phase becomes a historical artifact at that point — `model_snapshots_observed` in `phase.lock.yaml` records what was used.

## What IS reproducible

- The harness (workflow + adapter + prompt files + target set).
- The aggregate distribution of outcomes (per-model mean ± stddev) within ~2σ.
- Every input that fed the experiment (every SHA, every prompt byte, every model id).
