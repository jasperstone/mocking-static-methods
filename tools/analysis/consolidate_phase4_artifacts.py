#!/usr/bin/env python3
"""Consolidate per-(model, repo, run) phase4 gen artifacts into canonical results.

Source artifacts are expected under a local download root such as:
  /home/jastone/gha-downloads/backfill-<RUN_ID>/gen-<model>-<repo>-run<N>/

Each artifact contains a subtree at:
  <artifact>/<model>/run_<N>/

This script rebuilds:
  phases/phase4-refactoring/results/<model>/run_<N>/

Re-runnable: clears destination run dirs before writing so partial state never
leaks into aggregates.
"""
from __future__ import annotations

import argparse
import shutil
import sys
from pathlib import Path

MODELS = [
    "codestral-2501",
    "gpt-4.1-mini",
    "gpt-4.1-nano",
    "grok-4-1-fast",
    "llama-3.3-70b-instruct",
    "phi-4",
]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("src", type=Path, help="Artifact download root")
    parser.add_argument(
        "--dst",
        type=Path,
        default=Path("phases/phase4-refactoring/results"),
        help="Canonical phase4 results root",
    )
    return parser.parse_args()


def detect_artifact(artifact: Path) -> tuple[str, int] | None:
    for model in MODELS:
        stem = f"gen-{model}-"
        if not artifact.name.startswith(stem):
            continue
        tail = artifact.name.rsplit("-run", 1)
        if len(tail) != 2:
            return None
        try:
            run = int(tail[1])
        except ValueError:
            return None
        return model, run
    return None


def consolidate(src: Path, dst_root: Path) -> int:
    if not src.exists():
        print(f"SRC not found: {src}", file=sys.stderr)
        return 1

    by_key: dict[tuple[str, int], list[Path]] = {}
    for artifact in sorted(src.iterdir()):
        if not artifact.is_dir():
            continue
        parsed = detect_artifact(artifact)
        if not parsed:
            print(f"  SKIP (unparseable): {artifact.name}", file=sys.stderr)
            continue
        by_key.setdefault(parsed, []).append(artifact)

    summary: list[tuple[str, int, int, int, int, int]] = []
    for (model, run), artifacts in sorted(by_key.items()):
        dst = dst_root / model / f"run_{run}"
        if dst.exists():
            shutil.rmtree(dst)
        (dst / "generated_tests").mkdir(parents=True, exist_ok=True)
        (dst / "turns").mkdir(parents=True, exist_ok=True)
        (dst / "refactors").mkdir(parents=True, exist_ok=True)

        attempts_path = dst / "attempts.jsonl"
        n_attempts = 0
        n_tests = 0
        n_turns = 0
        n_refactors = 0

        with attempts_path.open("w") as attempts_out:
            for artifact in sorted(artifacts):
                base = artifact / model / f"run_{run}"
                if not base.exists():
                    print(f"  WARN: expected base {base} missing", file=sys.stderr)
                    continue

                src_attempts = base / "attempts.jsonl"
                if src_attempts.exists():
                    text = src_attempts.read_text()
                    if text and not text.endswith("\n"):
                        text += "\n"
                    attempts_out.write(text)
                    n_attempts += sum(1 for _ in text.splitlines())

                for folder, counter_name in (("generated_tests", "test"), ("turns", "turn"), ("refactors", "refactor")):
                    src_dir = base / folder
                    if not src_dir.exists():
                        continue
                    for path in src_dir.rglob("*"):
                        if not path.is_file():
                            continue
                        rel = path.relative_to(src_dir)
                        target = dst / folder / rel
                        target.parent.mkdir(parents=True, exist_ok=True)
                        shutil.copy2(path, target)
                        if counter_name == "test":
                            n_tests += 1
                        elif counter_name == "turn":
                            n_turns += 1
                        else:
                            n_refactors += 1

        summary.append((model, run, n_attempts, n_tests, n_turns, n_refactors))
        print(
            f"  {model:<25} run_{run}  attempts={n_attempts:>3}  "
            f"tests={n_tests:>3}  turns={n_turns:>3}  refactors={n_refactors:>3}  "
            f"(from {len(artifacts)} artifacts)"
        )

    print("\nTotals:")
    print(f"  attempts={sum(s[2] for s in summary)}")
    print(f"  tests={sum(s[3] for s in summary)}")
    print(f"  turns={sum(s[4] for s in summary)}")
    print(f"  refactors={sum(s[5] for s in summary)}")
    return 0


def main() -> int:
    args = parse_args()
    return consolidate(args.src, args.dst)


if __name__ == "__main__":
    raise SystemExit(main())