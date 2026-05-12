"""Build targets/v{N}/targets.csv via stratified sampling from v1.

Why stratified, not random:
  v1 is dominated by a few repos (jellyfin alone has 1,046/3,147 sites)
  and a few static-call-pattern families (logger 74%, DI 23%). A uniform
  random sample at n=300 would land ~100 jellyfin logger calls and almost
  nothing on the rare patterns. Stratification lets the report make claims
  per-repo and per-family rather than one global average dominated by the
  jellyfin-logger bucket.

Algorithm:
  1. Bucket each row by (repo, family). family is derived from
     containing_type + method via FAMILY_RULES below.
  2. Allocate per-repo budget by sqrt-proportional with floor=min(3, available)
     and total = TARGET_N. sqrt damps the long tail without zeroing
     small repos.
  3. Within each repo, allocate per-family budget proportional to that
     family's count in that repo, with floor=1 if available.
  4. Inside each (repo, family) bucket, deterministically sample
     SEED-shuffled rows.

Output:
  targets/v{N}/targets.csv          - sampled rows, same schema as v1
  targets/v{N}/sampling_report.md   - stratification table for the lockfile
  targets/v{N}/targets.lock.yaml    - source SHA, seed, allocation counts

Usage:
  python3 tools/targets/stratified_sample.py \\
    --source targets/v1/targets.csv --out-version v2 --n 300 --seed 42
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import math
import random
from collections import Counter, defaultdict
from pathlib import Path

# ---------------------------------------------------------------------------
# Pattern families. Derived from method + containing_type in v1.
# Keep in lockstep with the families called out in REPORT.md.
# ---------------------------------------------------------------------------
LOGGER_METHODS = {
    "Log", "LogTrace", "LogDebug", "LogInformation",
    "LogWarning", "LogError", "LogCritical",
}
DI_METHODS = {
    "GetService", "GetRequiredService", "GetServices",
    "CreateScope", "CreateAsyncScope",
}
HTTP_METHODS = {
    "GetAsync", "PostAsync", "PutAsync", "DeleteAsync", "PatchAsync",
    "SendAsync", "GetStringAsync", "GetStreamAsync", "GetByteArrayAsync",
}
CONFIG_METHODS = {"GetConnectionString", "GetValue", "GetSection"}


def family_of(row: dict) -> str:
    method = row["method"]
    ctype = row["containing_type"]
    if "Logging" in ctype or method in LOGGER_METHODS:
        return "logger"
    if "DependencyInjection" in ctype or method in DI_METHODS:
        return "di"
    if "Net.Http" in ctype or method in HTTP_METHODS:
        return "http"
    if "Configuration" in ctype or method in CONFIG_METHODS:
        return "config"
    return "other"


# ---------------------------------------------------------------------------
# Allocation
# ---------------------------------------------------------------------------
def allocate_repo_budgets(
    repo_counts: dict[str, int],
    target_n: int,
    floor: int = 3,
) -> dict[str, int]:
    """sqrt-proportional with floor; total == target_n."""
    repos = list(repo_counts.keys())
    weights = {r: math.sqrt(repo_counts[r]) for r in repos}
    total_w = sum(weights.values())
    raw = {r: target_n * weights[r] / total_w for r in repos}

    # apply floor (capped at availability)
    alloc = {r: max(min(floor, repo_counts[r]), int(round(raw[r]))) for r in repos}
    # cap each at availability
    alloc = {r: min(alloc[r], repo_counts[r]) for r in repos}

    # adjust to hit target_n exactly
    diff = target_n - sum(alloc.values())
    while diff != 0:
        # rank repos by (raw - alloc) so we add to under-allocated and
        # subtract from over-allocated
        order = sorted(repos, key=lambda r: (raw[r] - alloc[r]), reverse=(diff > 0))
        moved = False
        for r in order:
            if diff > 0 and alloc[r] < repo_counts[r]:
                alloc[r] += 1
                diff -= 1
                moved = True
                break
            if diff < 0 and alloc[r] > min(floor, repo_counts[r]):
                alloc[r] -= 1
                diff += 1
                moved = True
                break
        if not moved:
            break  # can't move further without violating floor/cap
    return alloc


def allocate_family_budgets(
    family_counts: dict[str, int],
    repo_budget: int,
    floor: int = 1,
) -> dict[str, int]:
    """Linear-proportional with floor=1 per non-empty family; total == repo_budget."""
    families = [f for f, c in family_counts.items() if c > 0]
    total = sum(family_counts[f] for f in families)
    raw = {f: repo_budget * family_counts[f] / total for f in families}

    alloc = {f: max(min(floor, family_counts[f]), int(round(raw[f]))) for f in families}
    alloc = {f: min(alloc[f], family_counts[f]) for f in families}

    diff = repo_budget - sum(alloc.values())
    while diff != 0:
        order = sorted(families, key=lambda f: (raw[f] - alloc[f]), reverse=(diff > 0))
        moved = False
        for f in order:
            if diff > 0 and alloc[f] < family_counts[f]:
                alloc[f] += 1
                diff -= 1
                moved = True
                break
            if diff < 0 and alloc[f] > min(floor, family_counts[f]):
                alloc[f] -= 1
                diff += 1
                moved = True
                break
        if not moved:
            break
    return alloc


# ---------------------------------------------------------------------------
# Driver
# ---------------------------------------------------------------------------
def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--source", required=True, help="path to source targets.csv")
    ap.add_argument("--out-version", required=True, help="e.g. v2")
    ap.add_argument("--n", type=int, default=300)
    ap.add_argument("--seed", type=int, default=42)
    ap.add_argument("--repo-floor", type=int, default=3,
                    help="min cells per repo (capped at availability)")
    args = ap.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    src = (repo_root / args.source).resolve()
    out_dir = repo_root / "targets" / args.out_version
    out_dir.mkdir(parents=True, exist_ok=True)

    rows = list(csv.DictReader(src.open()))
    src_sha = hashlib.sha256(src.read_bytes()).hexdigest()

    # Bucket by (repo, family)
    buckets: dict[tuple[str, str], list[dict]] = defaultdict(list)
    for r in rows:
        buckets[(r["repo"], family_of(r))].append(r)

    repo_counts = Counter(r["repo"] for r in rows)
    repo_alloc = allocate_repo_budgets(dict(repo_counts), args.n, args.repo_floor)

    rng = random.Random(args.seed)

    sampled: list[dict] = []
    family_breakdown: dict[str, dict[str, int]] = {}
    for repo in sorted(repo_alloc.keys()):
        budget = repo_alloc[repo]
        if budget == 0:
            continue
        fam_counts = {
            f: len(buckets.get((repo, f), []))
            for f in ("logger", "di", "http", "config", "other")
        }
        fam_alloc = allocate_family_budgets(fam_counts, budget)
        family_breakdown[repo] = fam_alloc

        for fam, take in fam_alloc.items():
            pool = list(buckets.get((repo, fam), []))
            rng.shuffle(pool)
            sampled.extend(pool[:take])

    sampled.sort(key=lambda r: r["target_id"])

    # Write targets.csv
    out_csv = out_dir / "targets.csv"
    fieldnames = list(rows[0].keys())
    with out_csv.open("w", newline="") as fh:
        w = csv.DictWriter(fh, fieldnames=fieldnames)
        w.writeheader()
        w.writerows(sampled)
    out_sha = hashlib.sha256(out_csv.read_bytes()).hexdigest()

    # Sampling report (markdown)
    report_lines = [
        f"# Targets — {args.out_version} sampling report",
        "",
        f"- Source: `{args.source}` (sha256 `{src_sha[:16]}...`)",
        f"- Target n: {args.n}",
        f"- Actual n: {len(sampled)}",
        f"- Seed: {args.seed}",
        f"- Repo floor: {args.repo_floor}",
        "",
        "## Allocation by (repo, family)",
        "",
        "| Repo | Population | Sampled | logger | di | http | config | other |",
        "|---|---|---|---|---|---|---|---|",
    ]
    for repo in sorted(family_breakdown.keys()):
        fa = family_breakdown[repo]
        report_lines.append(
            f"| {repo} | {repo_counts[repo]} | {repo_alloc[repo]} | "
            f"{fa.get('logger', 0)} | {fa.get('di', 0)} | "
            f"{fa.get('http', 0)} | {fa.get('config', 0)} | {fa.get('other', 0)} |"
        )
    report_lines.append(f"| **total** | {sum(repo_counts.values())} | {sum(repo_alloc.values())} | | | | | |")
    report_lines.append("")
    (out_dir / "sampling_report.md").write_text("\n".join(report_lines) + "\n")

    # Lockfile
    lock = {
        "source": args.source,
        "source_sha256": src_sha,
        "out_csv_sha256": out_sha,
        "target_n": args.n,
        "actual_n": len(sampled),
        "seed": args.seed,
        "repo_floor": args.repo_floor,
        "method": "sqrt-proportional per repo, linear-proportional per family within repo",
        "family_rules_module": "tools/targets/stratified_sample.py",
        "repo_alloc": dict(repo_alloc),
        "family_breakdown": family_breakdown,
    }
    (out_dir / "targets.lock.yaml").write_text(
        "# Auto-generated by tools/targets/stratified_sample.py — do not edit\n"
        + json.dumps(lock, indent=2, sort_keys=True) + "\n"
    )

    print(f"Wrote {out_csv} ({len(sampled)} rows)")
    print(f"Wrote {out_dir / 'sampling_report.md'}")
    print(f"Wrote {out_dir / 'targets.lock.yaml'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
