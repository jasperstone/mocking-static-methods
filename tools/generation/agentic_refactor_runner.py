#!/usr/bin/env python3
"""Agentic-loop runner WITH compile+run feedback AND the apply_refactor tool
(phase 4: agentic loop + testability refactoring).

Mirrors `tools/generation/agentic_runner_feedback.py` (same I/O shape, so
evaluate.py / the aggregator / build_report.py keep working) and borrows the
mock-LLM + out-dir + spend-gate flags from the phase-5 multi-agent runner.

Per cell it:
  1. locates the owning .csproj for the target file,
  2. instantiates a RefactorEngine confined to that project's subtree,
  3. runs the phase-4 refactor strategy (read_file/list_dir/apply_refactor/
     submit_test), where submit_test compiles+runs against the owning project
     (which already reflects any applied seam), and
  4. calls engine.restore_all() in a `finally` so the production tree is
     returned to pristine after every cell (cells never contaminate each other
     and the git working tree stays clean).

Output layout (phase-3 layout PLUS a per-cell refactor log):

    <out-dir or phases/<phase>/results/{model_safe}/run_{i}>/
        attempts.jsonl
        generated_tests/{repo}/{target_id}/test.cs
        turns/{repo}/{target_id}.jsonl
        refactors/{repo}/{target_id}.jsonl     # applied/rejected transforms

Usage (mock smoke — NO Azure spend):
    python3 tools/generation/agentic_refactor_runner.py \
        --model mock-llm --run-index 0 --target-set v2 \
        --target-ids OpenRA:0003 \
        --mock-llm --mock-fixtures-dir tools/generation/tests/fixtures/refactor \
        --out-dir /tmp/p4-smoke

Usage (real Foundry — DO NOT RUN BEFORE THE FREEZE LIFTS / gated):
    python3 tools/generation/agentic_refactor_runner.py \
        --model gpt-4.1-mini --run-index 1 --target-set v2 --limit 1 \
        --i-understand-this-will-spend-money
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
from tools.generation.strategies import agentic_loop_refactor as alr  # noqa: E402
from tools.generation.apply_refactor import RefactorEngine  # noqa: E402
from tools.evaluation import compile_only as _compile_only  # noqa: E402

DEFAULT_PHASE = "phase4-refactoring"


def slug(model_id: str) -> str:
    return model_id.replace("/", "__")


def sha256_hex(text: str) -> str:
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def build_user_values(target: dict, repo_root: Path) -> dict[str, str]:
    """Identical to the phase-3 runner's value set (keeps prompts compatible)."""
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


def _resolve_generate_and_check(args):
    """Return (generate_fn, check_factory).

    `check_factory(repo_dir, target_file)` returns a check_fn for that cell.
    Mock mode swaps in fixture-driven generation and a stub check (no dotnet),
    so the runner plumbing can be exercised without Azure spend.
    """
    if args.mock_llm:
        from tools.generation.adapters import mock_llm
        fix_dir = Path(args.mock_fixtures_dir) if args.mock_fixtures_dir else None
        if not fix_dir or not fix_dir.is_dir():
            raise SystemExit(f"--mock-fixtures-dir not found: {args.mock_fixtures_dir}")
        fixture_file = fix_dir / "default.json"
        if not fixture_file.is_file():
            raise SystemExit(f"missing fixture file: {fixture_file}")
        generate_fn = mock_llm.make_role_generate(fixture_file, "writer")

        def mock_check_factory(_repo_dir, _target_file):
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

        return generate_fn, mock_check_factory

    # Real Foundry path — gated behind the spend acknowledgement.
    if not args.i_understand_this_will_spend_money:
        raise SystemExit(
            "Refusing to use the real Foundry adapter without "
            "--i-understand-this-will-spend-money."
        )
    from tools.generation.adapters import foundry

    def real_check_factory(repo_dir, target_file):
        def _check(candidate_text):
            return _compile_only.compile_and_run_check(
                candidate_text, repo_dir, target_file,
                build_timeout_s=args.compile_timeout_s,
                run_timeout_s=args.run_timeout_s,
            )
        return _check

    return foundry.generate, real_check_factory


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", default=DEFAULT_PHASE)
    ap.add_argument("--model", required=True)
    ap.add_argument("--run-index", type=int, required=True)
    ap.add_argument("--target-set", default="v2")
    ap.add_argument("--max-turns", type=int, default=14)
    ap.add_argument("--max-reads", type=int, default=8)
    ap.add_argument("--max-attempts", type=int, default=4,
                    help="how many submit_test → check cycles per cell")
    ap.add_argument("--max-refactors", type=int, default=3,
                    help="how many apply_refactor calls allowed per cell")
    ap.add_argument("--temperature", type=float, default=0.0)
    ap.add_argument("--top-p", type=float, default=1.0)
    ap.add_argument("--seed", type=int, default=42)
    ap.add_argument("--max-output-tokens", type=int, default=4096)
    ap.add_argument("--timeout-s", type=int, default=180)
    ap.add_argument("--compile-timeout-s", type=int, default=240)
    ap.add_argument("--run-timeout-s", type=int, default=60)
    ap.add_argument("--refactor-build-timeout-s", type=int, default=240,
                    help="behaviour-preservation `dotnet build` timeout per refactor")
    ap.add_argument("--limit", type=int, default=None)
    ap.add_argument("--target-ids", default=None)
    ap.add_argument("--repo-filter", default=None)
    ap.add_argument("--cloned-repos", default=str(REPO_ROOT / "cloned_repos"))
    # Mock-mode flags (mirror the phase-5 multi-agent runner)
    ap.add_argument("--mock-llm", action="store_true",
                    help="Use the fixture-driven mock LLM adapter. NO Azure calls.")
    ap.add_argument("--mock-fixtures-dir", default=None,
                    help="Required with --mock-llm: dir containing default.json fixtures.")
    # Real-Foundry safety gate
    ap.add_argument("--i-understand-this-will-spend-money", action="store_true",
                    help="Required to use the real Foundry adapter.")
    # Output dir override (smoke test)
    ap.add_argument("--out-dir", default=None,
                    help="Override phases/<phase>/results/... output location.")
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    if not phase_dir.is_dir():
        print(f"error: phase dir not found: {phase_dir}", file=sys.stderr)
        return 2

    targets_csv = REPO_ROOT / "targets" / args.target_set / "targets.csv"
    if not args.mock_llm and not targets_csv.is_file():
        print(f"error: targets file not found: {targets_csv}", file=sys.stderr)
        return 2

    # Phase-4 prompts (Lewis authors these in parallel under the same dir).
    sys_prompt_path = phase_dir / "prompt" / "writer-system.md"
    user_tmpl_path = phase_dir / "prompt" / "user-template.md"
    if not sys_prompt_path.is_file() or not user_tmpl_path.is_file():
        print(
            f"error: phase-4 prompts not found. Expected:\n"
            f"  {sys_prompt_path}\n  {user_tmpl_path}\n"
            f"(Lewis is authoring these in parallel.)",
            file=sys.stderr,
        )
        return 2
    sys_prompt = sys_prompt_path.read_text(encoding="utf-8")
    user_template = user_tmpl_path.read_text(encoding="utf-8")
    sys_sha = sha256_hex(sys_prompt)
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
    refactors_dir = out_dir / "refactors"
    refactors_dir.mkdir(exist_ok=True)
    attempts_path = out_dir / "attempts.jsonl"

    cloned_root = Path(args.cloned_repos)
    generate_fn, check_factory = _resolve_generate_and_check(args)

    target_whitelist = None
    if args.target_ids:
        target_whitelist = {t.strip() for t in args.target_ids.split(",") if t.strip()}
    repo_filter = None
    if args.repo_filter:
        repo_filter = {r.strip() for r in args.repo_filter.split(",") if r.strip()}

    # Load rows (mock mode synthesizes a single placeholder cell).
    if args.mock_llm:
        rows = [{
            "target_id": "mock:0001",
            "repo": "mock-repo",
            "file": "mock.cs",
            "line": "1",
            "method": "DoSomething",
            "receiver_type": "MockService",
            "kind": "NonVirtual",
            "containing_type": "MockService",
        }]
        user_msg_override = (
            "Smoke-test cell. Writer fixture exercises apply_refactor + submit_test."
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
    n_refactor_applied = n_refactor_rejected = 0
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
                    _record(out, target_id, args, sys_sha, tmpl_sha,
                            error=f"prompt-render: {e}")
                    n_fail += 1
                    continue

            check_fn = check_factory(repo_dir, row["file"])

            # One engine per cell, confined to the owning csproj subtree.
            engine = RefactorEngine(
                repo_root=repo_dir,
                target=row,
                verify_build=not args.mock_llm,   # skip dotnet in mock smoke
                build_timeout_s=args.refactor_build_timeout_s,
            )

            t0 = time.monotonic()
            try:
                loop = alr.run(
                    generate=generate_fn,
                    check_fn=check_fn,
                    engine=engine,
                    model_id=args.model,
                    system_prompt=sys_prompt,
                    user_prompt=user_msg,
                    repo_root=repo_dir,
                    max_turns=args.max_turns,
                    max_reads=args.max_reads,
                    max_attempts=args.max_attempts,
                    max_refactors=args.max_refactors,
                    max_output_tokens=args.max_output_tokens,
                    temperature=args.temperature,
                    top_p=args.top_p,
                    seed=args.seed,
                    timeout_s=args.timeout_s,
                )
            finally:
                # CRITICAL: revert all production edits so the next cell starts
                # from a pristine tree.
                restored = engine.restore_all()
            wall_ms = int((time.monotonic() - t0) * 1000)

            # Per-turn forensics.
            tdir = turns_dir / row["repo"]
            tdir.mkdir(parents=True, exist_ok=True)
            tpath = tdir / f"{target_id.replace(':', '_')}.jsonl"
            with tpath.open("w") as tfh:
                for turn in loop.turns:
                    tfh.write(json.dumps(dataclasses.asdict(turn)) + "\n")

            # Per-cell refactor log.
            rdir = refactors_dir / row["repo"]
            rdir.mkdir(parents=True, exist_ok=True)
            rpath = rdir / f"{target_id.replace(':', '_')}.jsonl"
            with rpath.open("w") as rfh:
                for ra in loop.refactor_attempts:
                    rfh.write(json.dumps(ra) + "\n")
            for ra in loop.refactor_attempts:
                if ra.get("applied"):
                    n_refactor_applied += 1
                else:
                    n_refactor_rejected += 1

            tool_calls = {"read_file": 0, "list_dir": 0, "apply_refactor": 0,
                          "submit_test": 0, "no_tool": 0}
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
                if loop.final_run_ok:
                    n_run_ok += 1
            else:
                n_fail += 1

            model_snap = None
            for turn in reversed(loop.turns):
                if turn.model_snapshot:
                    model_snap = turn.model_snapshot
                    break

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
                    "first_compile_errors": a.errors[:5],
                    "n_test_failures": len(a.test_failures),
                    "first_test_failures": a.test_failures[:3],
                    "code_sha256": a.code_sha256,
                    "timeout": a.timeout,
                }
                for a in loop.attempts
            ]

            _record(
                out, target_id, args, sys_sha, tmpl_sha,
                repo=row["repo"],
                test_path=test_path,
                submitted=loop.submitted,
                halt_reason=loop.halt_reason,
                turns_used=len(loop.turns),
                tool_calls=tool_calls,
                reads_done=loop.reads_done,
                submission_attempts_n=len(loop.attempts),
                final_compile_ok=loop.final_compile_ok,
                final_run_ok=loop.final_run_ok,
                submission_iterations=submission_iters,
                refactor_attempts_n=len(loop.refactor_attempts),
                refactor_attempts=loop.refactor_attempts,
                files_restored=restored,
                total_prompt_tokens=loop.total_prompt_tokens,
                total_completion_tokens=loop.total_completion_tokens,
                total_latency_ms=loop.total_latency_ms,
                wall_ms=wall_ms,
                model_snapshot=model_snap,
                rendered_user_prompt_sha256=sha256_hex(user_msg),
                final_code_sha256=sha256_hex(loop.final_code) if loop.final_code else None,
                turns_log=str(tpath.relative_to(out_dir)),
                refactors_log=str(rpath.relative_to(out_dir)),
                error=None if loop.submitted else loop.halt_reason,
            )
            print(
                f"{target_id:<30} {args.model:<22} "
                f"turns={len(loop.turns)} reads={loop.reads_done} "
                f"refactors={len(loop.refactor_attempts)} "
                f"submitted={loop.submitted} compile_ok={loop.final_compile_ok} "
                f"run_ok={loop.final_run_ok} attempts={len(loop.attempts)} "
                f"halt={loop.halt_reason} wall={wall_ms}ms"
            )

    print(
        f"\ndone: {n_ok} submitted ({n_compile_ok} compile_ok, {n_run_ok} run_ok), "
        f"{n_fail} failures, refactors[applied={n_refactor_applied} "
        f"rejected={n_refactor_rejected}], attempts.jsonl at {attempts_path}"
    )
    return 0


def _record(out, target_id: str, args, sys_sha: str, tmpl_sha: str, **fields) -> None:
    rec = {
        "target_id": target_id,
        "phase": args.phase,
        "model_id": args.model,
        "run_index": args.run_index,
        "target_set": args.target_set,
        "strategy": "agentic_loop_refactor",
        "max_turns": args.max_turns,
        "max_reads": args.max_reads,
        "max_attempts": args.max_attempts,
        "max_refactors": args.max_refactors,
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
