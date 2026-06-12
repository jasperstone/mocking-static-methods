#!/usr/bin/env python3
"""Deterministic phase-4 transform applicability sweep (NO LLM, NO Azure).

Runs the phase-4 refactor transforms (`wrapper_interface`,
`parameterize_dependency`, `make_virtual`) DIRECTLY against the real cloned
repos for the actual `targets/v2/targets.csv` rows and measures tool coverage:
for each transform, on how many of the 300 real targets can the Roslyn tool
even produce a testability seam, and what is the §5 reject-reason distribution
for the rest.

This is the deterministic validation that the two AST transforms are "done":
no model in the loop, just `RefactorEngine.apply(...)` on messy production code,
with a strict non-destructive guarantee (every touched repo is restored after
every target via `engine.restore_all()` in a `finally`).

Two passes:
  * FAST  (--no-verify-build, default): tool produces seam-or-rejection only;
    no owning-project build. Cheap, runs all 300.
  * BUILD (--verify-build): the post-rewrite owning project is rebuilt to
    confirm behaviour-preservation. Slow; use on a small sample. A pre-existing
    (baseline) build failure of the owning project — i.e. NOT caused by our
    edit — is recorded as build_ok=None / reason `baseline_build_failed` and
    is NOT counted against the transform.

Target dict construction MIRRORS agentic_refactor_runner exactly: the CSV
DictReader row is passed straight through as the `target` (the engine reads
file/line/method/receiver_type/containing_type/kind from it), and the engine
is constructed `RefactorEngine(repo_root, target, verify_build=...)` so the
owning .csproj is located the same way production does.

Usage (fast applicability pass, all transforms, all 300 rows):
    python tools/generation/refactor_applicability_sweep.py \
        --targets targets/v2/targets.csv --repos-root cloned_repos \
        --transform all --no-verify-build \
        --out tools/generation/results/applicability_all.csv

Usage (build-verified sample):
    python tools/generation/refactor_applicability_sweep.py \
        --targets targets/v2/targets.csv --repos-root cloned_repos \
        --transform wrapper_interface --verify-build \
        --target-ids abp:0001,OpenRA:0003 \
        --out tools/generation/results/applicability_sample.csv
"""
from __future__ import annotations

import argparse
import csv
import os
import re
import subprocess
import sys
import threading
import time
from collections import Counter, defaultdict
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO_ROOT))

from tools.generation.apply_refactor import RefactorEngine  # noqa: E402
from tools.evaluation.compile_only import DOTNET, NUGET_CACHE  # noqa: E402

TRANSFORM_CHOICES = ("wrapper_interface", "parameterize_dependency", "make_virtual", "all")
_REAL_TRANSFORMS = ("wrapper_interface", "parameterize_dependency", "make_virtual")


# ---------------------------------------------------------------------------
# Receiver-family classification (matches the task's family buckets).
# ---------------------------------------------------------------------------
def receiver_family(receiver_type: str) -> str:
    """Bucket a (possibly fully-qualified) receiver type into a family label."""
    simple = (receiver_type or "").split(".")[-1].split("<")[0].strip()
    if simple == "ILogger" or simple.startswith("ILogger"):
        return "ILogger"
    if simple == "IServiceProvider":
        return "IServiceProvider"
    if simple == "IServiceScopeFactory":
        return "IServiceScopeFactory"
    if simple == "IConfiguration":
        return "IConfiguration"
    if simple == "IConfigurationBuilder":
        return "IConfigurationBuilder"
    if simple in ("HttpClient", "HttpMessageInvoker"):
        return "HttpClient/HttpMessageInvoker"
    return f"other:{simple}" if simple else "other"


# ---------------------------------------------------------------------------
# Reason-token extraction (for the §5 reject distribution tally).
# ---------------------------------------------------------------------------
def reason_token(res, applicable: bool) -> str:
    """Collapse a RefactorResult.reason into a single bucket token.

    Applicable outcomes bucket as `applied` / `applied_then_reverted`; rejects
    map to their §5 token (or a clear synthetic token for engine-level
    failures and make_virtual's prose rejections).
    """
    if applicable:
        return "applied_then_reverted" if res.reverted else "applied"
    r = (res.reason or "").strip()
    low = r.lower()
    # make_virtual prose rejections (not Roslyn §5 tokens).
    if low.startswith("could not find a non-virtual"):
        return "decl_not_found"
    if low.startswith("no owning .csproj"):
        return "no_owning_csproj"
    if low.startswith("no method name available"):
        return "no_method_name"
    if low.startswith("write target") and "outside the owning" in low:
        return "outside_owning_subtree"
    if low.startswith("prod-write guard"):
        return "prod_write_guard"
    if low.startswith("unknown transform"):
        return "unknown_transform"
    # Roslyn §5 tokens / engine tokens: first whitespace/colon-delimited token.
    first = re.split(r"[\s:]+", r, maxsplit=1)[0].strip(".:'\"") if r else ""
    return first or "unknown"


# ---------------------------------------------------------------------------
# Per-target sweep.
# ---------------------------------------------------------------------------
def sweep_one(row: dict, repos_root: Path, transform: str,
              verify_build: bool, build_timeout_s: int) -> dict:
    """Run one transform against one target row. ALWAYS restores the repo.

    Returns a flat result dict ready for CSV. Never raises; engine/IO failures
    are captured into the result.
    """
    target_id = row.get("target_id", "")
    repo = row.get("repo", "")
    kind = row.get("kind", "")
    receiver_type = row.get("receiver_type", "")
    method = row.get("method", "")

    base = {
        "target_id": target_id,
        "repo": repo,
        "kind": kind,
        "receiver_type": receiver_type,
        "receiver_family": receiver_family(receiver_type),
        "method": method,
        "transform": transform,
        "applicable": False,
        "applied": False,
        "reverted": False,
        "build_ok": "",
        "reason": "",
        "reason_token": "",
        "n_files_changed": 0,
        "seam_member_signature": "",
    }

    repo_root = repos_root / repo
    if not repo_root.is_dir():
        base["reason"] = f"repo_missing: {repo_root}"
        base["reason_token"] = "repo_missing"
        return base

    # Build the target dict EXACTLY as the runner does: the CSV row is the
    # target (engine reads file/line/method/receiver_type/containing_type/kind).
    engine = RefactorEngine(
        repo_root=repo_root,
        target=row,
        verify_build=verify_build,
        build_timeout_s=build_timeout_s,
    )

    baseline_ok: bool | None = None
    try:
        # When verifying, first confirm the owning project builds BEFORE our
        # edit. A pre-existing failure is not the transform's fault.
        if verify_build:
            baseline_ok, _errs, _tail = engine._build_owning_project()

        res = engine.apply(transform)

        applicable = bool(res.applied or res.reverted)
        base["applicable"] = applicable
        base["applied"] = bool(res.applied)
        base["reverted"] = bool(res.reverted)
        base["reason"] = (res.reason or "")[:400]
        base["reason_token"] = reason_token(res, applicable)
        base["n_files_changed"] = len(res.files_changed or [])
        seam = res.seam or {}
        base["seam_member_signature"] = str(seam.get("member_signature", ""))

        if verify_build:
            if baseline_ok is False:
                # Owning project couldn't build even pristine: don't count the
                # build outcome against the transform.
                base["build_ok"] = ""
                base["reason"] = "baseline_build_failed"
                base["reason_token"] = "baseline_build_failed"
            else:
                base["build_ok"] = "" if res.build_ok is None else bool(res.build_ok)
    except Exception as e:  # never let one target abort the sweep
        base["reason"] = f"sweep_exception: {type(e).__name__}: {e}"[:400]
        base["reason_token"] = "sweep_exception"
    finally:
        # CRITICAL non-destructive guarantee: restore the pristine clone.
        try:
            engine.restore_all()
        except Exception as e:
            base["reason"] = (base["reason"] + f" | restore_failed: {e}")[:400]

    return base


# ---------------------------------------------------------------------------
# Aggregate reporting.
# ---------------------------------------------------------------------------
def _pct(num: int, den: int) -> str:
    return f"{(100.0 * num / den):.1f}%" if den else "n/a"


def print_aggregate(results: list[dict], verify_build: bool) -> None:
    by_transform: dict[str, list[dict]] = defaultdict(list)
    for r in results:
        by_transform[r["transform"]].append(r)

    print("\n" + "=" * 78)
    print("APPLICABILITY SWEEP — AGGREGATE")
    print("=" * 78)

    for transform in _REAL_TRANSFORMS:
        rows = by_transform.get(transform)
        if not rows:
            continue
        total = len(rows)
        applicable = sum(1 for r in rows if r["applicable"])
        print(f"\n## transform = {transform}   (n={total})")
        print(f"   applicable (seam produced): {applicable}/{total}  ({_pct(applicable, total)})")

        # Reason-token distribution (all outcomes).
        tokens = Counter(r["reason_token"] for r in rows)
        print("   reason tokens:")
        for tok, cnt in tokens.most_common():
            print(f"      {tok:<28} {cnt}")

        if verify_build:
            build_true = sum(1 for r in rows if r["build_ok"] is True or r["build_ok"] == "True")
            build_false = sum(1 for r in rows if r["build_ok"] is False or r["build_ok"] == "False")
            build_skipped = sum(1 for r in rows if r["build_ok"] == "" or r["build_ok"] is None)
            print(f"   build_ok: true={build_true} false={build_false} skipped/None={build_skipped}")

        # Per-receiver-family applicability.
        fam_total: Counter = Counter()
        fam_appl: Counter = Counter()
        for r in rows:
            fam = r["receiver_family"]
            fam_total[fam] += 1
            if r["applicable"]:
                fam_appl[fam] += 1
        print("   per receiver family (applicable / total):")
        for fam in sorted(fam_total, key=lambda f: -fam_total[f]):
            t = fam_total[fam]
            a = fam_appl[fam]
            print(f"      {fam:<32} {a}/{t}  ({_pct(a, t)})")

    print("\n" + "=" * 78)


# ---------------------------------------------------------------------------
# Cleanliness check (defence-in-depth on top of restore_all).
# ---------------------------------------------------------------------------
def check_repo_cleanliness(repos_root: Path, repos: set[str]) -> list[tuple[str, str]]:
    """Run `git status --porcelain` in each touched repo. Returns a list of
    (repo, porcelain_output) for any repo left dirty."""
    dirty: list[tuple[str, str]] = []
    for repo in sorted(repos):
        rr = repos_root / repo
        if not rr.is_dir():
            continue
        try:
            proc = subprocess.run(
                ["git", "status", "--porcelain"],
                cwd=str(rr), capture_output=True, text=True, timeout=60,
            )
        except (OSError, subprocess.TimeoutExpired) as e:
            dirty.append((repo, f"<git status failed: {e}>"))
            continue
        out = (proc.stdout or "").strip()
        if out:
            dirty.append((repo, out))
    return dirty


# ---------------------------------------------------------------------------
# Driver.
# ---------------------------------------------------------------------------
def load_rows(targets_csv: Path) -> list[dict]:
    with targets_csv.open(newline="") as fh:
        return list(csv.DictReader(fh))


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--targets", default="targets/v2/targets.csv",
                    help="targets CSV (default: targets/v2/targets.csv)")
    ap.add_argument("--repos-root", default="cloned_repos",
                    help="root holding cloned_repos/{repo} (default: cloned_repos)")
    ap.add_argument("--transform", choices=TRANSFORM_CHOICES, default="all",
                    help="transform to sweep, or 'all' (default: all)")
    ap.add_argument("--limit", type=int, default=0,
                    help="cap to the first N target rows (0 = no cap)")
    ap.add_argument("--target-ids", default="",
                    help="comma-separated target_id whitelist")
    ap.add_argument("--verify-build", dest="verify_build", action="store_true",
                    help="rebuild the owning project after each rewrite (slow)")
    ap.add_argument("--no-verify-build", dest="verify_build", action="store_false",
                    help="applicability only; no owning-project build (default)")
    ap.set_defaults(verify_build=False)
    ap.add_argument("--build-timeout-s", type=int, default=600,
                    help="per-build timeout in seconds (default: 600)")
    ap.add_argument("--out", required=True, help="output CSV path")
    ap.add_argument("--jobs", type=int, default=1,
                    help="parallel workers across repos (default: 1). Targets in "
                         "the SAME repo are always serialized to keep edits "
                         "non-destructive.")
    args = ap.parse_args(argv)

    targets_csv = (REPO_ROOT / args.targets) if not Path(args.targets).is_absolute() else Path(args.targets)
    repos_root = (REPO_ROOT / args.repos_root) if not Path(args.repos_root).is_absolute() else Path(args.repos_root)
    out_path = (REPO_ROOT / args.out) if not Path(args.out).is_absolute() else Path(args.out)

    if not targets_csv.is_file():
        print(f"error: targets file not found: {targets_csv}", file=sys.stderr)
        return 2
    if not repos_root.is_dir():
        print(f"error: repos root not found: {repos_root}", file=sys.stderr)
        return 2

    rows = load_rows(targets_csv)

    whitelist = {t.strip() for t in args.target_ids.split(",") if t.strip()}
    if whitelist:
        rows = [r for r in rows if r.get("target_id") in whitelist]
    elif args.limit:
        rows = rows[: args.limit]

    transforms = list(_REAL_TRANSFORMS) if args.transform == "all" else [args.transform]

    # Build the (row, transform) work list.
    work: list[tuple[dict, str]] = [(row, t) for row in rows for t in transforms]
    touched_repos = {r.get("repo", "") for r in rows}

    print(f"sweep: {len(rows)} targets x {len(transforms)} transform(s) = {len(work)} runs")
    print(f"       repos-root={repos_root}")
    print(f"       verify_build={args.verify_build}  jobs={args.jobs}  "
          f"build_timeout_s={args.build_timeout_s}")
    print(f"       out={out_path}")

    results: list[dict] = []
    t0 = time.monotonic()

    if args.jobs <= 1:
        for i, (row, transform) in enumerate(work, 1):
            res = sweep_one(row, repos_root, transform,
                            args.verify_build, args.build_timeout_s)
            results.append(res)
            tag = "OK " if res["applicable"] else "-- "
            print(f"  [{i}/{len(work)}] {tag}{res['target_id']:<16} {transform:<24} "
                  f"{res['reason_token']}")
    else:
        # Per-repo locks: targets in the same repo never edit concurrently.
        repo_locks: dict[str, threading.Lock] = defaultdict(threading.Lock)

        def _job(item: tuple[dict, str]) -> dict:
            row, transform = item
            lock = repo_locks[row.get("repo", "")]
            with lock:
                return sweep_one(row, repos_root, transform,
                                 args.verify_build, args.build_timeout_s)

        done = 0
        with ThreadPoolExecutor(max_workers=args.jobs) as ex:
            futs = {ex.submit(_job, item): item for item in work}
            for fut in as_completed(futs):
                res = fut.result()
                results.append(res)
                done += 1
                tag = "OK " if res["applicable"] else "-- "
                print(f"  [{done}/{len(work)}] {tag}{res['target_id']:<16} "
                      f"{res['transform']:<24} {res['reason_token']}")

    elapsed = time.monotonic() - t0
    print(f"\nsweep finished in {elapsed:.1f}s")

    # Write CSV.
    out_path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = [
        "target_id", "repo", "kind", "receiver_type", "receiver_family",
        "method", "transform", "applicable", "applied", "reverted",
        "build_ok", "reason", "reason_token", "n_files_changed",
        "seam_member_signature",
    ]
    # Keep deterministic ordering (work order) regardless of parallelism.
    order = {(row.get("target_id"), t): idx for idx, (row, t) in enumerate(work)}
    results.sort(key=lambda r: order.get((r["target_id"], r["transform"]), 1 << 30))
    with out_path.open("w", newline="") as fh:
        w = csv.DictWriter(fh, fieldnames=fieldnames)
        w.writeheader()
        for r in results:
            w.writerow(r)
    print(f"wrote {len(results)} rows -> {out_path}")

    # Aggregate summary (the headline statistic).
    print_aggregate(results, args.verify_build)

    # Cleanliness verification (defence-in-depth).
    print("CLEANLINESS CHECK (git status --porcelain per touched repo):")
    dirty = check_repo_cleanliness(repos_root, touched_repos)
    if not dirty:
        print(f"  ALL {len(touched_repos)} touched repos are CLEAN.")
    else:
        print(f"  WARNING: {len(dirty)} repo(s) left DIRTY:")
        for repo, porcelain in dirty:
            print(f"    --- {repo} ---")
            for ln in porcelain.splitlines()[:20]:
                print(f"      {ln}")
    print("=" * 78)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
