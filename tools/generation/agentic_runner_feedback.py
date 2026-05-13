#!/usr/bin/env python3
"""Agentic-loop runner WITH compile feedback (phase 3).

Same I/O shape as tools/generation/agentic_runner.py so downstream tooling
(evaluate.py, aggregator, build_report.py) keeps working. New per-attempt
fields capture the compile-feedback loop forensics:

    compile_attempts_n       — how many submit→compile cycles fired
    final_compile_ok         — did the last submission compile?
    compile_iterations       — list of {turn, ok, build_ms, n_errors, code_sha}

Output layout (unchanged from phase 2 agentic):

    phases/<phase-id>/results/{model_safe}/run_{i}/
        attempts.jsonl
        generated_tests/{repo}/{target_id}/test.cs
        turns/{repo}/{target_id}.jsonl

Usage:
    python3 tools/generation/agentic_runner_feedback.py \
        --phase phase3-agentic-loop \
        --model gpt-4.1-mini \
        --run-index 1 \
        --target-set v2 \
        --limit 1
"""
from __future__ import annotations
import argparse
import csv
import dataclasses
import hashlib
import json
import os
import sys
import time
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO_ROOT))

from tools.generation.adapters import foundry  # noqa: E402
from tools.generation.prompt_render import render  # noqa: E402
from tools.generation.source_window import read_window  # noqa: E402
from tools.generation.strategies import agentic_loop_feedback  # noqa: E402
from tools.evaluation import compile_only as _compile_only  # noqa: E402


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
        "TEST_FRAMEWORK": os.environ.get("TEST_FRAMEWORK", "xUnit"),
        "TARGET_TFM": os.environ.get("TARGET_TFM", "net10.0"),
        "TEST_PROJECT_PATH": os.environ.get("TEST_PROJECT_PATH", ""),
        "TEST_NAMESPACE_HINT": os.environ.get("TEST_NAMESPACE_HINT", ""),
        "EXAMPLE_TEST": os.environ.get("EXAMPLE_TEST", "// (no example available)"),
    }


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True)
    ap.add_argument("--model", required=True)
    ap.add_argument("--run-index", type=int, required=True)
    ap.add_argument("--target-set", required=True)
    ap.add_argument("--max-turns", type=int, default=12)
    ap.add_argument("--max-reads", type=int, default=8)
    ap.add_argument("--max-compile-attempts", type=int, default=4,
                    help="how many submit_test → compile cycles per cell (1 = phase 2 behavior)")
    ap.add_argument("--temperature", type=float, default=0.0)
    ap.add_argument("--top-p", type=float, default=1.0)
    ap.add_argument("--seed", type=int, default=42)
    ap.add_argument("--max-output-tokens", type=int, default=4096)
    ap.add_argument("--timeout-s", type=int, default=180)
    ap.add_argument("--compile-timeout-s", type=int, default=240)
    ap.add_argument("--limit", type=int, default=None)
    ap.add_argument("--target-ids", default=None)
    ap.add_argument("--repo-filter", default=None)
    ap.add_argument("--cloned-repos", default=str(REPO_ROOT / "cloned_repos"))
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
    sys_sha = sha256_hex(sys_prompt)
    tmpl_sha = sha256_hex(user_template)

    out_dir = phase_dir / "results" / slug(args.model) / f"run_{args.run_index}"
    out_dir.mkdir(parents=True, exist_ok=True)
    tests_dir = out_dir / "generated_tests"
    tests_dir.mkdir(exist_ok=True)
    turns_dir = out_dir / "turns"
    turns_dir.mkdir(exist_ok=True)
    attempts_path = out_dir / "attempts.jsonl"

    cloned_root = Path(args.cloned_repos)

    target_whitelist = None
    if args.target_ids:
        target_whitelist = {t.strip() for t in args.target_ids.split(",") if t.strip()}

    repo_filter = None
    if args.repo_filter:
        repo_filter = {r.strip() for r in args.repo_filter.split(",") if r.strip()}

    n_ok = n_fail = n_compile_ok = 0
    with targets_csv.open() as fh, attempts_path.open("w") as out:
        rows = list(csv.DictReader(fh))
        if target_whitelist:
            rows = [r for r in rows if r["target_id"] in target_whitelist]
        if repo_filter:
            rows = [r for r in rows if r["repo"] in repo_filter]
        if not target_whitelist and args.limit:
            rows = rows[: args.limit]

        for row in rows:
            target_id = row["target_id"]
            repo_dir = cloned_root / row["repo"]

            try:
                values = build_user_values(row, repo_dir)
                user_msg = render(user_template, values)
            except (FileNotFoundError, KeyError) as e:
                _record(out, target_id, args, sys_sha, tmpl_sha,
                        error=f"prompt-render: {e}")
                n_fail += 1
                continue

            # Bind compile callback to this cell's repo + target file.
            def compile_fn(candidate_text: str, _repo_dir=repo_dir,
                           _target_file=row["file"],
                           _timeout=args.compile_timeout_s):
                return _compile_only.compile_check(
                    candidate_text, _repo_dir, _target_file, timeout_s=_timeout,
                )

            t0 = time.monotonic()
            loop = agentic_loop_feedback.run(
                generate=foundry.generate,
                compile_fn=compile_fn,
                model_id=args.model,
                system_prompt=sys_prompt,
                user_prompt=user_msg,
                repo_root=repo_dir,
                max_turns=args.max_turns,
                max_reads=args.max_reads,
                max_compile_attempts=args.max_compile_attempts,
                max_output_tokens=args.max_output_tokens,
                temperature=args.temperature,
                top_p=args.top_p,
                seed=args.seed,
                timeout_s=args.timeout_s,
            )
            wall_ms = int((time.monotonic() - t0) * 1000)

            # Persist per-turn forensics.
            tdir = turns_dir / row["repo"]
            tdir.mkdir(parents=True, exist_ok=True)
            tpath = tdir / f"{target_id.replace(':', '_')}.jsonl"
            with tpath.open("w") as tfh:
                for turn in loop.turns:
                    tfh.write(json.dumps(dataclasses.asdict(turn)) + "\n")

            # Tool-call tally.
            tool_calls = {"read_file": 0, "list_dir": 0, "submit_test": 0, "no_tool": 0}
            for turn in loop.turns:
                if turn.tool_name in tool_calls:
                    tool_calls[turn.tool_name] += 1
                elif turn.tool_name is None and turn.role == "assistant":
                    tool_calls["no_tool"] += 1

            test_path = None
            if loop.submitted and loop.final_code:
                base = tests_dir / row["repo"] / target_id.replace(":", "_")
                base.mkdir(parents=True, exist_ok=True)
                fp = base / "test.cs"
                fp.write_text(loop.final_code, encoding="utf-8")
                test_path = str(fp.relative_to(out_dir))
                n_ok += 1
                if loop.final_compile_ok:
                    n_compile_ok += 1
            else:
                n_fail += 1

            model_snap = None
            for turn in reversed(loop.turns):
                if turn.model_snapshot:
                    model_snap = turn.model_snapshot
                    break

            compile_iters = [
                {
                    "attempt_index": a.attempt_index,
                    "turn_index": a.turn_index,
                    "compile_ok": a.compile_ok,
                    "build_ms": a.build_ms,
                    "n_errors": len(a.errors),
                    "first_errors": a.errors[:5],
                    "code_sha256": a.code_sha256,
                }
                for a in loop.compile_attempts
            ]

            _record(
                out, target_id, args, sys_sha, tmpl_sha,
                test_path=test_path,
                submitted=loop.submitted,
                halt_reason=loop.halt_reason,
                turns_used=len(loop.turns),
                tool_calls=tool_calls,
                reads_done=loop.reads_done,
                compile_attempts_n=len(loop.compile_attempts),
                final_compile_ok=loop.final_compile_ok,
                compile_iterations=compile_iters,
                total_prompt_tokens=loop.total_prompt_tokens,
                total_completion_tokens=loop.total_completion_tokens,
                total_latency_ms=loop.total_latency_ms,
                wall_ms=wall_ms,
                model_snapshot=model_snap,
                rendered_user_prompt_sha256=sha256_hex(user_msg),
                final_code_sha256=sha256_hex(loop.final_code) if loop.final_code else None,
                turns_log=str(tpath.relative_to(out_dir)),
                error=None if loop.submitted else loop.halt_reason,
            )
            print(
                f"{target_id:<30} {args.model:<25} "
                f"turns={len(loop.turns)} reads={loop.reads_done} "
                f"submitted={loop.submitted} compile_ok={loop.final_compile_ok} "
                f"attempts={len(loop.compile_attempts)} halt={loop.halt_reason} "
                f"p_tok={loop.total_prompt_tokens} c_tok={loop.total_completion_tokens} "
                f"wall={wall_ms}ms"
            )

    print(
        f"\ndone: {n_ok} submitted ({n_compile_ok} compile_ok), "
        f"{n_fail} failures, attempts.jsonl at {attempts_path}"
    )
    return 0


def _record(out, target_id: str, args, sys_sha: str, tmpl_sha: str, **fields) -> None:
    rec = {
        "target_id": target_id,
        "phase": args.phase,
        "model_id": args.model,
        "run_index": args.run_index,
        "target_set": args.target_set,
        "strategy": "agentic_loop_feedback",
        "max_turns": args.max_turns,
        "max_reads": args.max_reads,
        "max_compile_attempts": args.max_compile_attempts,
        "temperature": args.temperature,
        "top_p": args.top_p,
        "seed": args.seed,
        "max_output_tokens": args.max_output_tokens,
        "prompt_system_sha256": sys_sha,
        "prompt_user_template_sha256": tmpl_sha,
    }
    rec.update(fields)
    out.write(json.dumps(rec) + "\n")
    out.flush()


if __name__ == "__main__":
    sys.exit(main())
