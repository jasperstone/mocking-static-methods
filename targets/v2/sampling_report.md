# Targets — v2 sampling report

- Source: `targets/v1/targets.csv` (sha256 `aca60f388a4c82c8...`)
- Target n: 300
- Actual n: 300
- Seed: 42
- Repo floor: 3

## Selection procedure

The sample is selected by [`stratified_sample.py`](../../tools/targets/stratified_sample.py)
using stratified random sampling. Repository and API-family quotas are
deterministic; only the choice of sites within each stratum is randomized.

1. **Start with the eligible population.** Read `targets/v1/targets.csv`, which contains
   3,147 eligible call sites. The v1 input retains sites classified as
   production code whose exact source lines are present but uncovered in the
   baseline coverage data; it excludes already-covered sites, non-production
   paths, and sites with unknown coverage files or lines.

2. **Group by repository and API family.** Assign every site to one
   `(repo, family)` stratum. The `family_of` function checks logging,
   dependency injection, HTTP, and configuration in that order, using
   containing-type substrings or explicit method-name sets. Unmatched sites
   are assigned to `other`. The first matching category wins.

3. **Allocate targets across repositories using square-root weighting.**
   For repository `r` with `N_r` eligible sites, the initial quota is
   `n * sqrt(N_r) / sum_j sqrt(N_j)`, where `n = 300`.
   Apply Python's `round`, enforce a minimum of `min(3, N_r)`,
   and cap the quota at `N_r`. Add one site at a time to the repository with
   the largest quota residual, or remove one from the repository
   with the smallest residual, respecting the bounds, until quotas total
   300. Square-root weighting increases smaller repositories'
   representation relative to sampling proportional to population size.

4. **Allocate each repository's quota across API families.** Within each
   repository, allocate proportionally to family population:
   `n_r * N_(r,f) / N_r`. Round and cap allocations at available sites,
   with a minimum of one site per nonempty family. Adjust allocations
   one site at a time by quota residual, respecting these bounds, to match
   the repository's quota.

5. **Select sites randomly within each stratum, without replacement.**
   Initialize one Python pseudorandom generator with
   `random.Random(42)`. Visit repositories in alphabetical order
   and nonempty families in the fixed order `logger`, `di`, `http`,
   `config`, `other`. Preserve input row order within each pool, shuffle
   the pool using the shared generator, and take its allocated number
   of sites. Retain original target IDs and sort the final rows by
   `target_id` before writing the CSV.

The resulting sample has unequal site inclusion probabilities. An unweighted
success rate describes this benchmark sample, rather than automatically
providing an unbiased estimate for the entire eligible population.

## Reproduction

Run from the repository root with the same ordered input CSV and sampling
implementation:

```bash
python3 tools/targets/stratified_sample.py --source targets/v1/targets.csv --out-version v2 --n 300 --seed 42 --repo-floor 3
```

This regenerates `targets.csv`, `sampling_report.md`, and
`targets.lock.yaml` under `targets/v2/`.
The lock file records the seed, allocation counts, and SHA-256 hashes of
the source and selected CSVs. The seed alone is insufficient if input
ordering, sampling code, or the Python random implementation changes.
Check the regenerated CSV hashes against the frozen lock file; byte-level
hashes also depend on CSV newline encoding.

## Allocation by (repo, family)

| Repo | Population | Sampled | logger | di | http | config | other |
|---|---|---|---|---|---|---|---|
| OpenRA | 13 | 7 | 0 | 0 | 7 | 0 | 0 |
| abp | 445 | 41 | 32 | 7 | 2 | 0 | 0 |
| aspnetcore | 403 | 39 | 13 | 24 | 1 | 1 | 0 |
| duplicati | 21 | 9 | 0 | 3 | 6 | 0 | 0 |
| eShop | 30 | 11 | 10 | 1 | 0 | 0 | 0 |
| efcore | 1 | 1 | 0 | 1 | 0 | 0 | 0 |
| garnet | 499 | 43 | 43 | 0 | 0 | 0 | 0 |
| jellyfin | 1046 | 62 | 59 | 1 | 1 | 1 | 0 |
| orleans | 98 | 19 | 6 | 9 | 0 | 4 | 0 |
| roslyn | 6 | 5 | 5 | 0 | 0 | 0 | 0 |
| semantic-kernel | 458 | 41 | 18 | 22 | 1 | 0 | 0 |
| server | 127 | 22 | 6 | 15 | 0 | 1 | 0 |
| **total** | 3147 | 300 | | | | | |

