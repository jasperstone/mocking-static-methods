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
import re
import subprocess
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


# ---------------------------------------------------------------------------
# §4.3 anti-gaming verifier ("via_seam").
#
# A phase-4 pass is only attributable to the refactor if the submitted test
# actually exercises the target behaviour THROUGH the new seam. `via_seam` is
# not knowable at apply time (the test does not exist yet), so it is computed
# here AFTER submit_test returns run-OK, by cross-referencing the tool's `seam`
# descriptor against the FINAL submitted test source. All four checks below
# must hold for via_seam=True (TRANSFORM_CONTRACT §4.3). Regex over the test
# source is acceptable for v1 per the contract; because the production site is
# rewritten to invoke ONLY the injected interface (§4.1.1), "mock injected +
# method driven" is sufficient evidence that the mock is what runs at the site.
# ---------------------------------------------------------------------------

_TRIVIAL_ASSERT_RES = (
    re.compile(r"Assert\.True\s*\(\s*true\s*\)", re.IGNORECASE),
    re.compile(r"Assert\.False\s*\(\s*false\s*\)", re.IGNORECASE),
    re.compile(r"Assert\.Equal\s*\(\s*1\s*,\s*1\s*\)", re.IGNORECASE),
)


def _balanced_call_args(source: str, open_re: re.Pattern) -> list[str]:
    """For every match of `open_re` (whose match must END at the call's '('),
    return the argument substring inside the balanced parentheses."""
    out: list[str] = []
    n = len(source)
    for m in open_re.finditer(source):
        i = m.end() - 1  # position of '('
        if i < 0 or i >= n or source[i] != "(":
            continue
        depth = 0
        j = i
        while j < n:
            c = source[j]
            if c == "(":
                depth += 1
            elif c == ")":
                depth -= 1
                if depth == 0:
                    out.append(source[i + 1 : j])
                    break
            j += 1
    return out


def _mock_tokens(source: str, iface: str) -> set[str]:
    """Collect expressions/identifiers that denote a constructed mock/fake of
    `iface` (the seam interface, simple name). Membership of any of these inside
    an injection-point argument list proves the mock is what gets injected."""
    esc = re.escape(iface)
    tokens: set[str] = set()

    # NSubstitute: var sub = Substitute.For<iface>();  -> inject `sub`
    for m in re.finditer(rf"(?:var|{esc}\??)\s+(\w+)\s*=\s*Substitute\.For\s*<\s*{esc}\s*>", source):
        tokens.add(m.group(1))
    # FakeItEasy: var fake = A.Fake<iface>();  -> inject `fake`
    for m in re.finditer(rf"(?:var|{esc}\??)\s+(\w+)\s*=\s*A\.Fake\s*<\s*{esc}\s*>", source):
        tokens.add(m.group(1))
    # Moq: var mock = new Mock<iface>();  -> inject `mock.Object`
    for m in re.finditer(rf"(?:var|Mock\s*<\s*{esc}\s*>)\s+(\w+)\s*=\s*new\s+Mock\s*<\s*{esc}\s*>", source):
        tokens.add(m.group(1) + ".Object")
    # Hand-rolled fake: class Fake : ... iface ... { }  -> inject `new Fake(` / a var of it
    fake_classes = re.findall(rf"class\s+(\w+)\s*:\s*[^{{]*\b{esc}\b", source)
    for fc in fake_classes:
        fe = re.escape(fc)
        tokens.add(f"new {fc}")
        for m in re.finditer(rf"(?:var|{esc}\??|{fe})\s+(\w+)\s*=\s*new\s+{fe}\b", source):
            tokens.add(m.group(1))
    # Inline constructions usable directly as an argument.
    if re.search(rf"Substitute\.For\s*<\s*{esc}\s*>", source):
        tokens.add(f"Substitute.For<{iface}>")
    if re.search(rf"Mock\.Of\s*<\s*{esc}\s*>", source):
        tokens.add(f"Mock.Of<{iface}>")
    if re.search(rf"A\.Fake\s*<\s*{esc}\s*>", source):
        tokens.add(f"A.Fake<{iface}>")
    return tokens


def verify_via_seam(seam: dict, test_source: str) -> tuple[bool, dict]:
    """Run the four §4.3 checks of `seam` against `test_source`.

    Returns (via_seam, checks) where `checks` records each boolean so the
    attribution is auditable from saved artifacts alone.
    """
    src = test_source or ""
    iface = str(seam.get("interface", "")).split(".")[-1]
    containing = str(seam.get("containing_type", "")).split(".")[-1]
    injection = str(seam.get("injection", ""))
    injref = str(seam.get("injection_ref", ""))
    esc_if = re.escape(iface)
    esc_ct = re.escape(containing)

    # Check 1: seam interface referenced in a mock/fake construction context.
    check_seam_type = bool(iface) and bool(
        re.search(rf"Mock\s*<\s*{esc_if}\s*>", src)
        or re.search(rf"Substitute\.For\s*<\s*{esc_if}\s*>", src)
        or re.search(rf"Mock\.Of\s*<\s*{esc_if}\s*>", src)
        or re.search(rf"A\.Fake\s*<\s*{esc_if}\s*>", src)
        or re.search(rf"class\s+\w+\s*:\s*[^{{]*\b{esc_if}\b", src)
    )

    tokens = _mock_tokens(src, iface)

    def _args_have_mock(arglists: list[str]) -> bool:
        for args in arglists:
            if any(tok in args for tok in tokens):
                return True
        return False

    # Check 2: the mock is injected at the injection point.
    check_injected = False
    check_method_driven = False
    if injection == "ctor" and containing:
        ctor_open = re.compile(rf"\bnew\s+{esc_ct}\s*\(")
        ctor_args = _balanced_call_args(src, ctor_open)
        # named-arg injection (param_name:) also counts.
        param_name = injref.strip()
        named = bool(param_name) and any(
            re.search(rf"\b{re.escape(param_name)}\s*:", a) for a in ctor_args
        )
        check_injected = named or _args_have_mock(ctor_args)
        # Check 3: a method is invoked on the constructed instance.
        mvar = re.search(rf"(?:var|{esc_ct})\s+(\w+)\s*=\s*new\s+{esc_ct}\s*\(", src)
        if mvar:
            inst = re.escape(mvar.group(1))
            check_method_driven = bool(re.search(rf"\b{inst}\s*\.\s*\w+\s*\(", src))
        else:
            check_method_driven = bool(
                re.search(rf"new\s+{esc_ct}\s*\([^;]*\)\s*\.\s*\w+\s*\(", src)
            )
    elif injection == "overload":
        enclosing = injref.split("(", 1)[0].strip()
        if enclosing:
            # Match `enclosing(` whether called as `obj.M(...)` or `M(...)`, but
            # not a longer identifier ending in `enclosing` (e.g. a test method
            # named `M_DoesX`).
            call_open = re.compile(rf"(?<!\w){re.escape(enclosing)}\s*\(")
            overload_args = _balanced_call_args(src, call_open)
            check_injected = _args_have_mock(overload_args)
            # The overload call IS the method invocation that drives the seam.
            check_method_driven = check_injected

    # Check 4: a non-trivial assertion is present.
    has_verify = bool(re.search(r"\.\s*Verify\s*\(", src) or re.search(r"\.\s*Received\s*[(<]", src))
    asserts = re.findall(r"Assert\.\w+\s*\([^;]*\)", src)
    non_trivial_assert = any(
        not any(t.search(a) for t in _TRIVIAL_ASSERT_RES) for a in asserts
    )
    fluent = bool(re.search(r"\.\s*Should\s*\(\s*\)", src))
    check_assertion = has_verify or non_trivial_assert or fluent

    checks = {
        "seam_type_referenced": check_seam_type,
        "injected_at_injection_point": check_injected,
        "target_method_driven": check_method_driven,
        "non_trivial_assertion": check_assertion,
    }
    return all(checks.values()), checks


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
    ap.add_argument(
        "--require-baseline-compile",
        action=argparse.BooleanOptionalAction,
        default=True,
        help=(
            "Require owning project baseline `dotnet build` to pass before any "
            "generation/refactor work for a target (default: true)."
        ),
    )
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
    ap.add_argument("--mock-cell-json", default=None,
                    help="Optional with --mock-llm: JSON file overriding the synthesized "
                         "mock target row (target_id/repo/file/line/method/receiver_type/"
                         "kind/containing_type), e.g. to point at a real ILogger/HttpClient "
                         "fixture repo so apply_refactor exercises the Roslyn tool.")
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
        if args.mock_cell_json:
            cell_path = Path(args.mock_cell_json)
            if not cell_path.is_file():
                print(f"error: --mock-cell-json not found: {cell_path}", file=sys.stderr)
                return 2
            cell = json.loads(cell_path.read_text())
            # Required keys default sensibly so a partial cell still runs.
            rows = [{
                "target_id": cell.get("target_id", "mock:0001"),
                "repo": cell.get("repo", "mock-repo"),
                "file": cell.get("file", "mock.cs"),
                "line": str(cell.get("line", "1")),
                "method": cell.get("method", ""),
                "receiver_type": cell.get("receiver_type", ""),
                "kind": cell.get("kind", ""),
                "containing_type": cell.get("containing_type", ""),
            }]
        else:
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

            baseline_compile_ok = None
            baseline_build_ms = None
            baseline_csproj = None
            baseline_errors: list[dict] = []
            baseline_timeout = None
            baseline_halt = None

            if args.require_baseline_compile and not args.mock_llm:
                csproj_path = _compile_only.find_owning_csproj(repo_dir, row["file"])
                if csproj_path is None:
                    baseline_compile_ok = False
                    baseline_halt = "baseline_no_owning_csproj"
                else:
                    baseline_csproj = str(csproj_path.relative_to(repo_dir))
                    env = os.environ.copy()
                    env["DOTNET_NOLOGO"] = "1"
                    env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
                    env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
                    env["NUGET_PACKAGES"] = _compile_only.NUGET_CACHE
                    t_build = time.monotonic()
                    try:
                        br = subprocess.run(
                            [
                                _compile_only.DOTNET,
                                "build",
                                str(csproj_path),
                                "-c",
                                "Debug",
                                "-v",
                                "minimal",
                                "--nologo",
                                "/p:TreatWarningsAsErrors=false",
                                "/p:GenerateDocumentationFile=false",
                            ],
                            cwd=repo_dir,
                            capture_output=True,
                            text=True,
                            timeout=args.compile_timeout_s,
                            env=env,
                        )
                        baseline_build_ms = int((time.monotonic() - t_build) * 1000)
                        baseline_compile_ok = (br.returncode == 0)
                        if not baseline_compile_ok:
                            out_text = (br.stdout or "") + (br.stderr or "")
                            baseline_errors = _compile_only.first_compile_errors(out_text)
                            baseline_halt = "baseline_compile_failed"
                    except subprocess.TimeoutExpired:
                        baseline_build_ms = int((time.monotonic() - t_build) * 1000)
                        baseline_compile_ok = False
                        baseline_timeout = "build"
                        baseline_halt = "baseline_build_timeout"

                if baseline_compile_ok is False:
                    _record(
                        out,
                        target_id,
                        args,
                        sys_sha,
                        tmpl_sha,
                        repo=row["repo"],
                        test_path=None,
                        submitted=False,
                        halt_reason=baseline_halt,
                        turns_used=0,
                        tool_calls={
                            "read_file": 0,
                            "list_dir": 0,
                            "apply_refactor": 0,
                            "submit_test": 0,
                            "no_tool": 0,
                        },
                        reads_done=0,
                        submission_attempts_n=0,
                        final_compile_ok=False,
                        final_run_ok=False,
                        submission_iterations=[],
                        refactor_attempts_n=0,
                        refactor_attempts=[],
                        via_seam=None,
                        via_seam_checks=None,
                        seam=None,
                        files_restored=[],
                        baseline_compile_ok=baseline_compile_ok,
                        baseline_build_ms=baseline_build_ms,
                        baseline_csproj=baseline_csproj,
                        baseline_timeout=baseline_timeout,
                        baseline_first_compile_errors=baseline_errors[:5],
                        total_prompt_tokens=0,
                        total_completion_tokens=0,
                        total_latency_ms=0,
                        wall_ms=baseline_build_ms or 0,
                        model_snapshot=None,
                        rendered_user_prompt_sha256=None,
                        final_code_sha256=None,
                        turns_log=None,
                        refactors_log=None,
                        error=baseline_halt,
                    )
                    n_fail += 1
                    print(
                        f"{target_id:<30} {args.model:<22} "
                        f"submitted=False compile_ok=False run_ok=False "
                        f"halt={baseline_halt} baseline={baseline_csproj or 'n/a'} "
                        f"wall={baseline_build_ms or 0}ms"
                    )
                    continue

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

            # §4.3 anti-gaming: verify the submitted test exercises the seam.
            # Only meaningful when a seam-bearing refactor applied AND the cell
            # submitted a run-OK test. `via_seam` stays None otherwise (e.g.
            # make_virtual, which carries no seam, or a non-passing cell).
            effective_seam = None
            for ra in loop.refactor_attempts:
                if ra.get("applied") and ra.get("seam"):
                    effective_seam = ra["seam"]   # last applied seam wins
            via_seam = None
            via_seam_checks = None
            if effective_seam and loop.submitted and loop.final_run_ok and loop.final_code:
                via_seam, via_seam_checks = verify_via_seam(effective_seam, loop.final_code)
            # Persist the verdict + seam alongside the refactor log so the
            # attribution is auditable from saved artifacts alone (§4.4).
            if effective_seam is not None:
                with rpath.open("a") as rfh:
                    rfh.write(json.dumps({
                        "verification": True,
                        "via_seam": via_seam,
                        "checks": via_seam_checks,
                        "seam": effective_seam,
                    }) + "\n")

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
                via_seam=via_seam,
                via_seam_checks=via_seam_checks,
                seam=effective_seam,
                files_restored=restored,
                baseline_compile_ok=baseline_compile_ok,
                baseline_build_ms=baseline_build_ms,
                baseline_csproj=baseline_csproj,
                baseline_timeout=baseline_timeout,
                baseline_first_compile_errors=baseline_errors[:5],
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
