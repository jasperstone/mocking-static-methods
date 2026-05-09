#!/usr/bin/env python3
"""Single-shot generation runner for one (model × run_index) cell.

One CI matrix job invokes this script once. It reads the target set,
generates a test file per target via GitHub Models, and writes outputs
into:

    phases/<phase-id>/results/{model_id_safe}/run_{run_index}/
      attempts.jsonl
      generated_tests/{repo}/{target_id}.cs

The downstream coverage workflow consumes generated_tests/ and produces
compile/, runtime/, and cobertura XMLs. build_outcomes.py joins those
back into outcome.csv.

Phase 3+ strategies wrap this runner in a loop; see tools/generation/strategies/.

Usage:
    python3 tools/generation/runner.py \
        --phase phase2-singleshot \
        --model anthropic/claude-opus-4-5 \
        --run-index 1 \
        --target-set v1
"""
from __future__ import annotations
import argparse
import csv
import hashlib
import json
import os
import re
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO_ROOT))

from tools.generation.adapters import github_models
from tools.generation.prompt_render import render
from tools.generation.source_window import read_window

CSHARP_BLOCK = re.compile(r"```(?:csharp|cs|c#)?\s*\n(.*?)```", re.DOTALL | re.IGNORECASE)


def slug(model_id: str) -> str:
    return model_id.replace("/", "__")


def sha256_hex(text: str) -> str:
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def build_user_values(target: dict, repo_root: Path) -> dict[str, str]:
    win = read_window(repo_root, target["file"], int(target["line"]))
    return {
        "REPO": target["repo"],
        "REPO_SHA": os.environ.get("REPO_SHA", "unknown"),
        "TARGET_FILE": target["file"],
        "TARGET_LINE": target["line"],
        "RECEIVER_TYPE": target["receiver_type"],
        "METHOD": target["method"],
        "KIND": target["kind"],
        "CONTAINING_TYPE": target["containing_type"],
        "SOURCE_WINDOW_START": str(win.start_line),
        "SOURCE_WINDOW_END": str(win.end_line),
        "SOURCE_WINDOW": win.text,
        # Filled by per-repo conventions module (TODO phase 2 day 1):
        "TEST_FRAMEWORK": os.environ.get("TEST_FRAMEWORK", "xUnit"),
        "TARGET_TFM": os.environ.get("TARGET_TFM", "net10.0"),
        "TEST_PROJECT_PATH": os.environ.get("TEST_PROJECT_PATH", ""),
        "TEST_NAMESPACE_HINT": os.environ.get("TEST_NAMESPACE_HINT", ""),
        "EXAMPLE_TEST": os.environ.get("EXAMPLE_TEST", "// (no example available)"),
    }


def extract_csharp_blocks(text: str) -> list[str]:
    """Return every fenced code block in the response, in order.

    The single-shot prompt no longer constrains the model to one file or
    one block. A model that recognizes the static-call seam problem may
    legitimately emit multiple blocks (a wrapper interface, an
    implementation, the test class). We capture all of them; downstream
    compile/test logic decides which (if any) goes into the test project.
    """
    return [m.group(1).rstrip() + "\n" for m in CSHARP_BLOCK.finditer(text)]


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True, help="phase id, e.g. phase2-singleshot")
    ap.add_argument("--model", required=True, help="model id, e.g. anthropic/claude-opus-4-5")
    ap.add_argument("--run-index", type=int, required=True, help="1..runs_per_model")
    ap.add_argument("--target-set", required=True, help="targets version, e.g. v1")
    ap.add_argument("--temperature", type=float, default=0.0)
    ap.add_argument("--top-p", type=float, default=1.0)
    ap.add_argument("--seed", type=int, default=42)
    ap.add_argument("--max-output-tokens", type=int, default=4096)
    ap.add_argument("--limit", type=int, default=None,
                    help="dry-run cap on number of targets to process")
    ap.add_argument("--cloned-repos", default=str(REPO_ROOT / "cloned_repos"),
                    help="root of cloned repos at the pinned SHAs")
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    if not phase_dir.is_dir():
        print(f"error: phase dir not found: {phase_dir}", file=sys.stderr)
        return 2

    targets_csv = REPO_ROOT / "targets" / args.target_set / "targets.csv"
    if not targets_csv.is_file():
        print(f"error: targets file not found: {targets_csv}", file=sys.stderr)
        return 2

    sys_prompt = (phase_dir / "prompt" / "system.md").read_text(encoding="utf-8")
    user_template = (phase_dir / "prompt" / "user-template.md").read_text(encoding="utf-8")

    out_dir = phase_dir / "results" / slug(args.model) / f"run_{args.run_index}"
    out_dir.mkdir(parents=True, exist_ok=True)
    tests_dir = out_dir / "generated_tests"
    tests_dir.mkdir(exist_ok=True)
    attempts_path = out_dir / "attempts.jsonl"

    cloned_root = Path(args.cloned_repos)

    n_ok = n_fail = 0
    with targets_csv.open() as fh, attempts_path.open("w") as out:
        rows = list(csv.DictReader(fh))
        if args.limit:
            rows = rows[: args.limit]
        for row in rows:
            target_id = row["target_id"]
            repo_dir = cloned_root / row["repo"]
            try:
                values = build_user_values(row, repo_dir)
                user_msg = render(user_template, values)
            except (FileNotFoundError, KeyError) as e:
                _record(out, target_id, args, error=f"prompt-render: {e}")
                n_fail += 1
                continue

            try:
                t0 = time.monotonic()
                result = github_models.generate(
                    model_id=args.model,
                    system_prompt=sys_prompt,
                    user_prompt=user_msg,
                    temperature=args.temperature,
                    top_p=args.top_p,
                    seed=args.seed,
                    max_output_tokens=args.max_output_tokens,
                )
            except github_models.GitHubModelsError as e:
                _record(out, target_id, args, error=f"api: {e}", latency_ms=int((time.monotonic() - t0) * 1000))
                n_fail += 1
                continue

            blocks = extract_csharp_blocks(result.text)
            test_paths: list[str] = []
            if blocks:
                base = tests_dir / row["repo"] / target_id.replace(":", "_")
                base.mkdir(parents=True, exist_ok=True)
                for i, cs in enumerate(blocks, start=1):
                    p = base / f"block_{i:02d}.cs"
                    p.write_text(cs, encoding="utf-8")
                    test_paths.append(str(p.relative_to(out_dir)))
                n_ok += 1
            else:
                n_fail += 1

            _record(
                out, target_id, args,
                test_paths=test_paths,
                num_csharp_blocks=len(blocks),
                model_snapshot=result.model_snapshot,
                prompt_tokens=result.prompt_tokens,
                completion_tokens=result.completion_tokens,
                latency_ms=result.latency_ms,
                finish_reason=result.finish_reason,
                rendered_user_prompt=user_msg,
                rendered_user_prompt_sha256=sha256_hex(user_msg),
                response_sha256=sha256_hex(result.text),
                response_text=result.text,
                error=None if blocks else "no csharp block in response",
            )

    print(f"done: {n_ok} files written, {n_fail} failures, attempts.jsonl at {attempts_path}")
    return 0


def _record(out, target_id: str, args, **fields) -> None:
    rec = {
        "target_id": target_id,
        "phase": args.phase,
        "model_id": args.model,
        "run_index": args.run_index,
        "target_set": args.target_set,
        "temperature": args.temperature,
        "top_p": args.top_p,
        "seed": args.seed,
        "max_output_tokens": args.max_output_tokens,
        **fields,
    }
    out.write(json.dumps(rec, ensure_ascii=False) + "\n")


if __name__ == "__main__":
    sys.exit(main())
