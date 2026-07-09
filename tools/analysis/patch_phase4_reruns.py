#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path

REPO_ROOT = Path('/home/jastone/src/mocking-static-methods')
RESULTS = REPO_ROOT / 'phases/phase4-refactoring/results'
MODELS = [
    'gpt-4.1-mini',
    'gpt-4.1-nano',
    'codestral-2501',
    'grok-4-1-fast',
    'llama-3.3-70b-instruct',
    'phi-4',
]


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description='Patch phase4 attempts from rerun artifacts by replacing repo rows per (model, run).')
    p.add_argument('--rerun-id', dest='rerun_ids', action='append', required=True, type=int, help='GitHub Actions run ID used for downloaded backfill-<ID> artifacts; repeat for multiple IDs')
    p.add_argument('--downloads-root', type=Path, default=Path('/home/jastone/gha-downloads'), help='Root containing backfill-<RUN_ID> directories')
    p.add_argument('--results-root', type=Path, default=RESULTS, help='Phase4 canonical results root')
    p.add_argument('--expected-targets', type=Path, default=REPO_ROOT / 'targets/v2/targets.csv', help='Expected target panel CSV (for --strict-expected validation)')
    p.add_argument('--strict-expected', action='store_true', help='Fail if any patched (model, run) does not have expected repo counts')
    return p.parse_args()


def parse_artifact_name(name: str):
    for model in MODELS:
        pref = f'gen-{model}-'
        if name.startswith(pref) and '-run' in name:
            rest = name[len(pref):]
            repo, runpart = rest.rsplit('-run', 1)
            chunk = None
            if '-chunk' in runpart:
                runpart, chunk = runpart.split('-chunk', 1)
            return model, repo, int(runpart), chunk
    return None


def merge_repo_artifacts(src_repo_dir: Path, dst_repo_dir: Path) -> None:
    if not src_repo_dir.exists():
        return
    dst_repo_dir.mkdir(parents=True, exist_ok=True)
    for child in src_repo_dir.iterdir():
        dst_child = dst_repo_dir / child.name
        if dst_child.exists():
            if dst_child.is_dir():
                shutil.rmtree(dst_child)
            else:
                dst_child.unlink()
        if child.is_dir():
            shutil.copytree(child, dst_child)
        else:
            shutil.copy2(child, dst_child)


def load_expected_repo_counts(targets_csv: Path) -> dict[str, int]:
    counts: dict[str, int] = {}
    with targets_csv.open() as f:
        header = f.readline()
        if 'repo' not in header:
            raise ValueError(f'unexpected targets header in {targets_csv}')
        for line in f:
            if not line.strip():
                continue
            repo = line.split(',', 2)[1].strip()
            counts[repo] = counts.get(repo, 0) + 1
    return counts


def count_repos(attempts_path: Path) -> dict[str, int]:
    repo_counts: dict[str, int] = {}
    if not attempts_path.exists():
        return repo_counts
    for ln in attempts_path.read_text(errors='ignore').splitlines():
        if not ln.strip():
            continue
        try:
            obj = json.loads(ln)
        except Exception:
            continue
        repo = str(obj.get('repo') or obj.get('target', {}).get('repo') or '').strip()
        if not repo:
            continue
        repo_counts[repo] = repo_counts.get(repo, 0) + 1
    return repo_counts


def row_key(row: dict) -> tuple[str, str]:
    repo = str(row.get('repo') or row.get('target', {}).get('repo') or '').strip()
    target_id = str(row.get('target_id') or row.get('target', {}).get('target_id') or '').strip()
    return repo, target_id


def patch_from_reruns(args: argparse.Namespace) -> int:
    patched = []
    touched_runs: set[tuple[str, int]] = set()

    for rid in args.rerun_ids:
        src_root = args.downloads_root / f'backfill-{rid}'
        if not src_root.exists():
            print(f'WARN missing download dir: {src_root}')
            continue
        for art in sorted([p for p in src_root.iterdir() if p.is_dir()]):
            parsed = parse_artifact_name(art.name)
            if not parsed:
                continue
            model, repo, run, _chunk = parsed
            base = art / model / f'run_{run}'
            src_attempts = base / 'attempts.jsonl'
            if not src_attempts.exists():
                continue

            dst_run = args.results_root / model / f'run_{run}'
            dst_run.mkdir(parents=True, exist_ok=True)
            dst_attempts = dst_run / 'attempts.jsonl'

            old = []
            if dst_attempts.exists():
                for ln in dst_attempts.read_text(errors='ignore').splitlines():
                    if not ln.strip():
                        continue
                    try:
                        old.append(json.loads(ln))
                    except Exception:
                        pass

            new = []
            for ln in src_attempts.read_text(errors='ignore').splitlines():
                if not ln.strip():
                    continue
                try:
                    new.append(json.loads(ln))
                except Exception:
                    pass

            # Replace only overlapping target rows so partial reruns do not
            # wipe out unrelated rows for the same repo.
            replace_keys = {row_key(r) for r in new}
            kept = [r for r in old if row_key(r) not in replace_keys]
            merged = kept + new
            with dst_attempts.open('w') as f:
                for r in merged:
                    f.write(json.dumps(r) + '\n')

            for folder in ('generated_tests', 'turns', 'refactors'):
                dst_repo_dir = dst_run / folder / repo
                src_repo_dir = base / folder / repo
                merge_repo_artifacts(src_repo_dir, dst_repo_dir)

            patched.append((rid, model, repo, run, len(new)))
            touched_runs.add((model, run))

    print('patched_shards', len(patched))
    for row in patched:
        print('PATCH', *row)

    if args.strict_expected:
        expected = load_expected_repo_counts(args.expected_targets)
        failures = 0
        for model, run in sorted(touched_runs):
            attempts = args.results_root / model / f'run_{run}' / 'attempts.jsonl'
            observed = count_repos(attempts)
            bad = []
            for repo, exp in sorted(expected.items()):
                got = observed.get(repo, 0)
                if got != exp:
                    bad.append((repo, got, exp))
            total = sum(observed.values())
            if bad:
                failures += 1
                print(f'CHECK_FAIL {model} run_{run} total={total} mismatches={len(bad)}')
                for repo, got, exp in bad[:20]:
                    print(f'  REPO_MISMATCH {repo} got={got} expected={exp}')
            else:
                print(f'CHECK_OK {model} run_{run} total={total}')
        if failures:
            return 2

    return 0


if __name__ == '__main__':
    raise SystemExit(patch_from_reruns(parse_args()))
