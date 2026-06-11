#!/usr/bin/env python3
"""Multi-agent runner (writer / reviewer / fixer) for phase 4.

Mirrors the CLI shape of `agentic_runner_feedback.py` so downstream tooling
(evaluate.py, aggregator, build_report.py) keeps working. Each cell records
the per-agent forensics in `attempts.jsonl`.

Adapter selection
=================

`--mock-llm` switches the runner from real Foundry calls to the
fixture-driven `adapters/mock_llm.py`. This is the default at install
time, because no real Foundry calls may be made before the Azure freeze
ends (~2026-06-08).

Output layout
=============

    phases/<phase-id>/results/{model_safe}/run_{i}/
        attempts.jsonl
        generated_tests/{repo}/{target_id}/test.cs
        turns/{repo}/{target_id}.jsonl       # writer + fixer turns
        reviews/{repo}/{target_id}.jsonl     # reviewer verdicts per cycle

Usage (smoke):
    python3 tools/generation/multi_agent_runner.py \
        --phase phase5-multiagent \
        --model mock-llm \
        --run-index 0 \
        --target-set v2 \
        --target-ids duplicati:0014 \
        --mock-llm \
        --mock-fixtures-dir tools/generation/tests/fixtures/multi_agent

Usage (real Foundry — DO NOT RUN BEFORE 2026-06-08):
    python3 tools/generation/multi_agent_runner.py \
        --phase phase5-multiagent \
        --model gpt-4.1-mini \
        --run-index 0 \
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

from tools.generation.prompt_render import render  # noqa: E402
from tools.generation.source_window import read_window  # noqa: E402
from tools.generation.strategies import multi_agent_writer_reviewer_fixer as mawrf  # noqa: E402


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
    }


def _resolve_generators(args):
    """Return (writer_gen, reviewer_gen, fixer_gen, check_fn_factory).

    `check_fn_factory(repo_dir, target_file)` returns a check_fn for that
    cell, so each cell points at the right production project.
    """
    if args.mock_llm:
        from tools.generation.adapters import mock_llm
        fix_dir = Path(args.mock_fixtures_dir)
        if not fix_dir.is_dir():
            raise SystemExit(f"--mock-fixtures-dir not found: {fix_dir}")
        # We expect one fixtures.json per target_id; the runner picks per cell.
        # For the smoke test we share a single file across all cells.
        fixture_file = fix_dir / "default.json"
        if not fixture_file.is_file():
            raise SystemExit(f"missing fixture file: {fixture_file}")
        writer_gen = mock_llm.make_role_generate(fixture_file, "writer")
        reviewer_gen = mock_llm.make_role_generate(fixture_file, "reviewer")
        fixer_gen = mock_llm.make_role_generate(fixture_file, "fixer")

        def mock_check_factory(_repo_dir, _target_file):
            # Mock check_fn: claim the test compiled and ran. The smoke test
            # validates the runner plumbing, not the dotnet toolchain.
            def _check(_candidate_text):
                class _R:
                    compile_ok = True
                    run_attempted = True
                    run_ok = True
                    build_ms = 1
                    run_ms = 1
                    errors: list = []
                    test_failures: list = []
                    tests_total = 1
                    tests_passed = 1
                    tests_failed = 0
                    tests_skipped = 0
                    error = None
                    timeout = None
                return _R()
            return _check

        return writer_gen, reviewer_gen, fixer_gen, mock_check_factory

    # Real Foundry path — wired but gated.
    if not args.i_understand_this_will_spend_money:
        raise SystemExit(
            "Refusing to use real Foundry adapter without "
            "--i-understand-this-will-spend-money. Azure freeze ends ~2026-06-08."
        )
    from tools.generation.adapters import foundry
    from tools.evaluation import compile_only as _compile_only

    def real_check_factory(repo_dir, target_file):
        def _check(candidate_text):
            return _compile_only.compile_and_run_check(
                candidate_text, repo_dir, target_file,
                build_timeout_s=args.compile_timeout_s,
                run_timeout_s=args.run_timeout_s,
            )
        return _check

    return foundry.generate, foundry.generate, foundry.generate, real_check_factory


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True)
    ap.add_argument("--model", required=True)
    ap.add_argument("--run-index", type=int, required=True)
    ap.add_argument("--target-set", required=True)
    ap.add_argument("--writer-max-turns", type=int, default=6)
    ap.add_argument("--writer-max-reads", type=int, default=4)
    ap.add_argument("--fixer-max-turns", type=int, default=4)
    ap.add_argument("--fixer-max-reads", type=int, default=2)
    ap.add_argument("--max-review-cycles", type=int, default=3)
    ap.add_argument("--agent-max-attempts", type=int, default=4)
    ap.add_argument("--temperature", type=float, default=0.0)
    ap.add_argument("--top-p", type=float, default=1.0)
    ap.add_argument("--seed", type=int, default=42)
    ap.add_argument("--max-output-tokens", type=int, default=4096)
    ap.add_argument("--timeout-s", type=int, default=180)
    ap.add_argument("--compile-timeout-s", type=int, default=240)
    ap.add_argument("--run-timeout-s", type=int, default=60)
    ap.add_argument("--limit", type=int, default=None)
    ap.add_argument("--target-ids", default=None)
    ap.add_argument("--repo-filter", default=None)
    ap.add_argument("--cloned-repos", default=str(REPO_ROOT / "cloned_repos"))
    # Mock-mode flags
    ap.add_argument("--mock-llm", action="store_true",
                    help="Use the fixture-driven mock LLM adapter. NO Azure calls.")
    ap.add_argument("--mock-fixtures-dir", default=None,
                    help="Required when --mock-llm: path to a directory containing default.json fixtures.")
    # Real-Foundry safety gate
    ap.add_argument("--i-understand-this-will-spend-money", action="store_true",
                    help="Required to use the real Foundry adapter. Azure freeze ends ~2026-06-08.")
    # Output dir override (smoke test)
    ap.add_argument("--out-dir", default=None,
                    help="Override the default phases/<phase>/results/... output location.")
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    if not phase_dir.is_dir():
        print(f"error: phase dir not found: {phase_dir}", file=sys.stderr)
        return 2

    targets_csv = REPO_ROOT / "targets" / args.target_set / "targets.csv"
    if not args.mock_llm and not targets_csv.is_file():
        print(f"error: targets file not found: {targets_csv}", file=sys.stderr)
        return 2

    writer_sys = (phase_dir / "prompt" / "writer-system.md").read_text(encoding="utf-8")
    reviewer_sys = (phase_dir / "prompt" / "reviewer-system.md").read_text(encoding="utf-8")
    fixer_sys = (phase_dir / "prompt" / "fixer-system.md").read_text(encoding="utf-8")
    user_template = (phase_dir / "prompt" / "user-template.md").read_text(encoding="utf-8")
    writer_sha = sha256_hex(writer_sys)
    reviewer_sha = sha256_hex(reviewer_sys)
    fixer_sha = sha256_hex(fixer_sys)
    tmpl_sha = sha256_hex(user_template)

    if args.out_dir:
        out_dir = Path(args.out_dir)
    else:
        out_dir = phase_dir / "results" / slug(args.model) / f"run_{args.run_index}"
    out_dir.mkdir(parents=True, exist_ok=True)
    tests_dir = out_dir / "generated_tests"
    tests_dir.mkdir(exist_ok=True)
    turns_dir = out_dir / "turns"
    turns_dir.mkdir(exist_ok=True)
    reviews_dir = out_dir / "reviews"
    reviews_dir.mkdir(exist_ok=True)
    attempts_path = out_dir / "attempts.jsonl"

    cloned_root = Path(args.cloned_repos)

    writer_gen, reviewer_gen, fixer_gen, check_factory = _resolve_generators(args)

    target_whitelist = None
    if args.target_ids:
        target_whitelist = {t.strip() for t in args.target_ids.split(",") if t.strip()}

    repo_filter = None
    if args.repo_filter:
        repo_filter = {r.strip() for r in args.repo_filter.split(",") if r.strip()}

    # Load rows
    if args.mock_llm:
        rows = [{
            "target_id": "mock:0001",
            "repo": "mock-repo",
            "file": "mock.cs",
            "line": "1",
            "method": "DoSomething",
            "receiver_type": "MockService",
            "kind": "instance",
            "containing_type": "MockService",
        }]
        # User prompt is rendered directly from a placeholder source window.
        user_msg_override = (
            "Smoke-test cell. Writer fixture returns a canned test; "
            "reviewer fixture APPROVES; no fixer call expected."
        )
    else:
        with targets_csv.open() as fh:
            rows = list(csv.DictReader(fh))
        user_msg_override = None

    if target_whitelist:
        rows = [r for r in rows if r["target_id"] in target_whitelist]
    if repo_filter:
        rows = [r for r in rows if r["repo"] in repo_filter]
    if not target_whitelist and args.limit:
        rows = rows[: args.limit]

    n_ok = n_fail = n_compile_ok = n_run_ok = 0
    with attempts_path.open("w") as out:
        for row in rows:
            target_id = row["target_id"]
            repo_dir = cloned_root / row["repo"]

            if user_msg_override is not None:
                user_msg = user_msg_override
            else:
                try:
                    values = build_user_values(row, repo_dir)
                    user_msg = render(user_template, values)
                except (FileNotFoundError, KeyError) as e:
                    _record(out, target_id, args,
                            writer_sha, reviewer_sha, fixer_sha, tmpl_sha,
                            error=f"prompt-render: {e}")
                    n_fail += 1
                    continue

            check_fn = check_factory(repo_dir, row["file"])

            t0 = time.monotonic()
            result = mawrf.run(
                writer_generate=writer_gen,
                reviewer_generate=reviewer_gen,
                fixer_generate=fixer_gen,
                check_fn=check_fn,
                model_id=args.model,
                writer_system_prompt=writer_sys,
                reviewer_system_prompt=reviewer_sys,
                fixer_system_prompt=fixer_sys,
                user_prompt=user_msg,
                repo_root=repo_dir,
                writer_max_turns=args.writer_max_turns,
                writer_max_reads=args.writer_max_reads,
                fixer_max_turns=args.fixer_max_turns,
                fixer_max_reads=args.fixer_max_reads,
                agent_max_attempts=args.agent_max_attempts,
                max_review_cycles=args.max_review_cycles,
                max_output_tokens=args.max_output_tokens,
                temperature=args.temperature,
                top_p=args.top_p,
                seed=args.seed,
                timeout_s=args.timeout_s,
            )
            wall_ms = int((time.monotonic() - t0) * 1000)

            # Persist writer + fixer turn logs.
            tdir = turns_dir / row["repo"]
            tdir.mkdir(parents=True, exist_ok=True)
            tpath = tdir / f"{target_id.replace(':', '_')}.jsonl"
            with tpath.open("w") as tfh:
                if result.writer_loop:
                    for turn in result.writer_loop.turns:
                        d = dataclasses.asdict(turn)
                        d["role_phase"] = "writer"
                        tfh.write(json.dumps(d) + "\n")
                for fi, fl in enumerate(result.fixer_loops, start=1):
                    for turn in fl.turns:
                        d = dataclasses.asdict(turn)
                        d["role_phase"] = f"fixer_cycle_{fi}"
                        tfh.write(json.dumps(d) + "\n")

            # Persist reviewer verdicts.
            rdir = reviews_dir / row["repo"]
            rdir.mkdir(parents=True, exist_ok=True)
            rpath = rdir / f"{target_id.replace(':', '_')}.jsonl"
            with rpath.open("w") as rfh:
                for rc in result.review_cycles:
                    rfh.write(json.dumps(dataclasses.asdict(rc)) + "\n")

            test_path = None
            if result.submitted and result.final_code:
                base = tests_dir / row["repo"] / target_id.replace(":", "_")
                base.mkdir(parents=True, exist_ok=True)
                fp = base / "test.cs"
                fp.write_text(result.final_code, encoding="utf-8")
                test_path = str(fp.relative_to(out_dir))
                n_ok += 1
                if result.final_compile_ok:
                    n_compile_ok += 1
                if result.final_run_ok:
                    n_run_ok += 1
            else:
                n_fail += 1

            submission_iters = [
                {
                    "attempt_index": a.attempt_index,
                    "turn_index": a.turn_index,
                    "compile_ok": a.compile_ok,
                    "run_attempted": a.run_attempted,
                    "run_ok": a.run_ok,
                    "build_ms": a.build_ms,
                    "run_ms": a.run_ms,
                    "tests_total": a.tests_total,
                    "tests_passed": a.tests_passed,
                    "tests_failed": a.tests_failed,
                    "tests_skipped": a.tests_skipped,
                    "n_compile_errors": len(a.errors),
                    "n_test_failures": len(a.test_failures),
                    "code_sha256": a.code_sha256,
                    "timeout": a.timeout,
                }
                for a in result.attempts
            ]
            review_summary = [
                {"cycle_index": rc.cycle_index, "verdict": rc.verdict,
                 "comment_len": len(rc.comment)}
                for rc in result.review_cycles
            ]

            _record(
                out, target_id, args,
                writer_sha, reviewer_sha, fixer_sha, tmpl_sha,
                test_path=test_path,
                submitted=result.submitted,
                halt_reason=result.halt_reason,
                final_role=result.final_role,
                multi_agent_cycles=len(result.review_cycles),
                review_summary=review_summary,
                final_compile_ok=result.final_compile_ok,
                final_run_ok=result.final_run_ok,
                submission_attempts_n=len(result.attempts),
                submission_iterations=submission_iters,
                total_prompt_tokens=result.total_prompt_tokens,
                total_completion_tokens=result.total_completion_tokens,
                total_latency_ms=result.total_latency_ms,
                wall_ms=wall_ms,
                rendered_user_prompt_sha256=sha256_hex(user_msg),
                final_code_sha256=sha256_hex(result.final_code) if result.final_code else None,
                turns_log=str(tpath.relative_to(out_dir)),
                reviews_log=str(rpath.relative_to(out_dir)),
                error=None if result.submitted else result.halt_reason,
            )

            print(
                f"{target_id:<25} {args.model:<20} "
                f"submitted={result.submitted} compile_ok={result.final_compile_ok} "
                f"run_ok={result.final_run_ok} cycles={len(result.review_cycles)} "
                f"final_role={result.final_role} halt={result.halt_reason} "
                f"wall={wall_ms}ms"
            )

    print(
        f"\ndone: {n_ok} submitted ({n_compile_ok} compile_ok, "
        f"{n_run_ok} run_ok), {n_fail} failures, "
        f"attempts.jsonl at {attempts_path}"
    )
    return 0


def _record(out, target_id, args,
            writer_sha, reviewer_sha, fixer_sha, tmpl_sha, **fields):
    rec = {
        "target_id": target_id,
        "phase": args.phase,
        "model_id": args.model,
        "run_index": args.run_index,
        "target_set": args.target_set,
        "strategy": "multi_agent_writer_reviewer_fixer",
        "writer_max_turns": args.writer_max_turns,
        "writer_max_reads": args.writer_max_reads,
        "fixer_max_turns": args.fixer_max_turns,
        "fixer_max_reads": args.fixer_max_reads,
        "max_review_cycles": args.max_review_cycles,
        "agent_max_attempts": args.agent_max_attempts,
        "temperature": args.temperature,
        "top_p": args.top_p,
        "seed": args.seed,
        "max_output_tokens": args.max_output_tokens,
        "mock_llm": bool(args.mock_llm),
        "prompt_writer_system_sha256": writer_sha,
        "prompt_reviewer_system_sha256": reviewer_sha,
        "prompt_fixer_system_sha256": fixer_sha,
        "prompt_user_template_sha256": tmpl_sha,
    }
    rec.update(fields)
    out.write(json.dumps(rec) + "\n")
    out.flush()


if __name__ == "__main__":
    sys.exit(main())
