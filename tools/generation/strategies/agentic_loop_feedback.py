"""Agentic-loop strategy WITH compile + run feedback (phase 3, option B).

Same text-based tool protocol as agentic_loop.py:
  - read_file(path)
  - list_dir(path)
  - submit_test(LANG)

On `submit_test`, instead of returning immediately, we hand the candidate to
an injected `check_fn`. The function compiles the candidate AND runs it
(`dotnet test`) and returns a result with both compile and run information.
On failure (either phase) the runner feeds a synthetic tool-result back into
the conversation and lets the model revise. The model can read more files,
then call submit_test again with a corrected version.

Budgets:
  - max_turns:               total assistant turns across initial + fix-up phase
  - max_reads:               total read_file calls
  - max_attempts:            how many submit_test cycles we check
                             (1 = phase 2 behavior; >1 = phase 3 fix-up loop)

Halt reasons (in addition to phase-2 set):
  - "submitted_run_ok"        — submit_test compiled AND all tests passed
  - "submitted_run_failed"    — compiled but tests failed/threw; budget exhausted
  - "submitted_compile_failed"— never compiled; budget exhausted
  - "max_turns_exhausted"     — ran out of conversation turns
"""
from __future__ import annotations

import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable

from tools.generation.strategies.agentic_loop import (
    TOOL_RE,
    CODE_BLOCK_RE,
    Turn,
    _pick_test_block,
    _tool_read_file,
    _tool_list_dir,
)


@dataclass
class SubmissionAttempt:
    """One submit_test → compile (+ optionally run) cycle."""
    attempt_index: int           # 1-based
    turn_index: int              # the assistant turn that produced this submission
    compile_ok: bool
    run_attempted: bool
    run_ok: bool
    build_ms: int
    run_ms: int
    errors: list[dict] = field(default_factory=list)             # compile errors
    test_failures: list[dict] = field(default_factory=list)      # test failures
    tests_total: int = 0
    tests_passed: int = 0
    tests_failed: int = 0
    tests_skipped: int = 0
    code_sha256: str | None = None
    timeout: str | None = None


@dataclass
class FeedbackLoopResult:
    submitted: bool
    final_code: str | None
    turns: list[Turn] = field(default_factory=list)
    total_prompt_tokens: int = 0
    total_completion_tokens: int = 0
    total_latency_ms: int = 0
    halt_reason: str = ""
    reads_done: int = 0
    # Phase 3 additions (compile + run):
    attempts: list[SubmissionAttempt] = field(default_factory=list)
    final_compile_ok: bool = False
    final_run_ok: bool = False


CheckFn = Callable[[str], "object"]
"""text -> CompileRunResult-like. Attributes used:
  .compile_ok, .run_attempted, .run_ok, .build_ms, .run_ms,
  .errors[], .test_failures[],
  .tests_total, .tests_passed, .tests_failed, .tests_skipped,
  .error, .timeout
"""


def _format_errors_block(errors: list[dict], max_errors: int = 6) -> str:
    if not errors:
        return "(no structured errors parsed; check the test syntax)"
    lines = []
    for e in errors[:max_errors]:
        loc = f"{e.get('file', 'GeneratedTest.cs')}({e.get('line', '?')},{e.get('col', '?')})"
        lines.append(f"  {loc}: error {e['code']}: {e['message']}")
    if len(errors) > max_errors:
        lines.append(f"  ... and {len(errors) - max_errors} more")
    return "\n".join(lines)


def _format_test_failures_block(
    failures: list[dict], counters: dict, max_failures: int = 3
) -> str:
    parts: list[str] = []
    parts.append(
        f"  Test counters: total={counters.get('tests_total', 0)} "
        f"passed={counters.get('tests_passed', 0)} "
        f"failed={counters.get('tests_failed', 0)} "
        f"skipped={counters.get('tests_skipped', 0)}"
    )
    if not failures:
        if counters.get("tests_total", 0) == 0:
            parts.append(
                "  No [Fact] methods executed. Your test class must have at "
                "least one [Fact]-attributed public method."
            )
        return "\n".join(parts)
    for f in failures[:max_failures]:
        parts.append(f"  FAILED: {f.get('test_name', '(unknown)')}")
        msg = (f.get("message") or "").strip()
        if msg:
            msg_lines = [ln for ln in msg.splitlines() if ln.strip()][:3]
            parts.append("    Message: " + " | ".join(msg_lines))
        stack = (f.get("stack_tail") or "").strip()
        for ln in stack.splitlines()[:3]:
            parts.append("    at " + ln.strip())
    if len(failures) > max_failures:
        parts.append(f"  ... and {len(failures) - max_failures} more failed tests")
    return "\n".join(parts)


def run(
    *,
    generate,
    check_fn: CheckFn,
    model_id: str,
    system_prompt: str,
    user_prompt: str,
    repo_root: Path,
    max_turns: int = 12,
    max_reads: int = 8,
    max_attempts: int = 4,
    max_output_tokens: int = 4096,
    temperature: float = 0.0,
    top_p: float = 1.0,
    seed: int = 42,
    timeout_s: int = 180,
) -> FeedbackLoopResult:
    """Drive the agentic loop with compile + run feedback.

    `check_fn(candidate_text) -> CompileRunResult` is injected: it should
    compile the candidate against the correct production project, run
    `dotnet test`, and return a result with both phases populated.
    """
    import hashlib

    def _sha(t: str) -> str:
        return hashlib.sha256(t.encode("utf-8")).hexdigest()

    result = FeedbackLoopResult(submitted=False, final_code=None)
    conversation: list[str] = [user_prompt]
    attempts_used = 0

    for turn_i in range(1, max_turns + 1):
        composed_user = "\n\n".join(conversation)
        try:
            r = generate(
                model_id=model_id,
                system_prompt=system_prompt,
                user_prompt=composed_user,
                temperature=temperature,
                top_p=top_p,
                seed=seed,
                max_output_tokens=max_output_tokens,
                timeout_s=timeout_s,
            )
        except Exception as e:
            result.turns.append(Turn(
                turn_index=turn_i, role="assistant",
                text=f"<adapter-error: {e}>", finish_reason="adapter_error",
            ))
            result.halt_reason = f"adapter error on turn {turn_i}: {e}"
            return result

        assistant_turn = Turn(
            turn_index=turn_i,
            role="assistant",
            text=r.text,
            latency_ms=r.latency_ms,
            prompt_tokens=r.prompt_tokens,
            completion_tokens=r.completion_tokens,
            model_snapshot=r.model_snapshot,
            finish_reason=r.finish_reason,
        )
        result.total_prompt_tokens += r.prompt_tokens
        result.total_completion_tokens += r.completion_tokens
        result.total_latency_ms += r.latency_ms

        m = TOOL_RE.search(r.text)
        if not m:
            assistant_turn.tool_ok = False
            result.turns.append(assistant_turn)
            conversation.append(f"<assistant-turn-{turn_i}>\n{r.text}\n</assistant-turn-{turn_i}>")
            conversation.append(
                f"<tool-result turn={turn_i}>"
                f"ERROR: no <tool>...</tool> call detected in your response. "
                f"You must call exactly one tool per turn. "
                f"Reply with one of: read_file(path), list_dir(path), or submit_test(csharp)."
                f"</tool-result>"
            )
            continue

        tool_name = m.group(1).lower()
        tool_arg = m.group(2)
        assistant_turn.tool_name = tool_name
        assistant_turn.tool_arg = tool_arg
        result.turns.append(assistant_turn)
        conversation.append(f"<assistant-turn-{turn_i}>\n{r.text}\n</assistant-turn-{turn_i}>")

        if tool_name == "submit_test":
            blocks = CODE_BLOCK_RE.findall(r.text)
            chosen = _pick_test_block(blocks)
            if not chosen:
                assistant_turn.tool_ok = False
                conversation.append(
                    f"<tool-result turn={turn_i}>"
                    f"ERROR: submit_test was called but no fenced ```csharp block followed. "
                    f"Re-emit your final answer as: <tool>submit_test(csharp)</tool> followed by ```csharp ... ```."
                    f"</tool-result>"
                )
                continue

            # We have a candidate. Run compile + (if compile_ok) tests.
            attempts_used += 1
            cres = check_fn(chosen)
            compile_ok = bool(getattr(cres, "compile_ok", False))
            run_attempted = bool(getattr(cres, "run_attempted", False))
            run_ok = bool(getattr(cres, "run_ok", False))
            errors_list = list(getattr(cres, "errors", []) or [])
            failures_list = list(getattr(cres, "test_failures", []) or [])
            counters = {
                "tests_total": int(getattr(cres, "tests_total", 0) or 0),
                "tests_passed": int(getattr(cres, "tests_passed", 0) or 0),
                "tests_failed": int(getattr(cres, "tests_failed", 0) or 0),
                "tests_skipped": int(getattr(cres, "tests_skipped", 0) or 0),
            }
            attempt = SubmissionAttempt(
                attempt_index=attempts_used,
                turn_index=turn_i,
                compile_ok=compile_ok,
                run_attempted=run_attempted,
                run_ok=run_ok,
                build_ms=int(getattr(cres, "build_ms", 0) or 0),
                run_ms=int(getattr(cres, "run_ms", 0) or 0),
                errors=errors_list,
                test_failures=failures_list,
                tests_total=counters["tests_total"],
                tests_passed=counters["tests_passed"],
                tests_failed=counters["tests_failed"],
                tests_skipped=counters["tests_skipped"],
                code_sha256=_sha(chosen),
                timeout=getattr(cres, "timeout", None),
            )
            result.attempts.append(attempt)
            # Always keep latest candidate as final_code so the runner can persist it.
            result.final_code = chosen
            result.submitted = True

            if run_ok:
                assistant_turn.tool_ok = True
                result.final_compile_ok = True
                result.final_run_ok = True
                result.halt_reason = "submitted_run_ok"
                return result

            # Either compile failed or tests failed. Decide whether we can give them another shot.
            attempts_left = max_attempts - attempts_used
            if attempts_left <= 0:
                assistant_turn.tool_ok = False
                result.final_compile_ok = compile_ok
                result.final_run_ok = False
                result.halt_reason = (
                    "submitted_compile_failed" if not compile_ok else "submitted_run_failed"
                )
                return result

            # Feed the right kind of error back and let them revise.
            assistant_turn.tool_ok = False
            extra = (getattr(cres, "error", None) or "").strip()
            if not compile_ok:
                err_block = _format_errors_block(errors_list)
                extra_line = f"\n  {extra}\n" if extra else ""
                conversation.append(
                    f"<tool-result turn={turn_i} tool=submit_test compile_ok=false run_ok=false>"
                    f"\nYour test did not compile. First errors:\n{err_block}{extra_line}\n"
                    f"You have {attempts_left} more submission attempt(s). "
                    f"You may call read_file() to inspect related code, "
                    f"or call submit_test again with a revised file."
                    f"</tool-result>"
                )
            else:
                fail_block = _format_test_failures_block(failures_list, counters)
                timeout_line = ""
                if attempt.timeout == "test":
                    timeout_line = (
                        f"\n  Test run timed out — your test likely hangs (waits on async/IO without timeout)."
                    )
                extra_line = f"\n  {extra}" if extra else ""
                conversation.append(
                    f"<tool-result turn={turn_i} tool=submit_test compile_ok=true run_ok=false>"
                    f"\nYour test compiled but did not pass when run:\n{fail_block}{timeout_line}{extra_line}\n"
                    f"You have {attempts_left} more submission attempt(s). "
                    f"Revise your test (fix the assertion, add the missing setup/mocks, "
                    f"or change the input data) and resubmit. You may also read_file() "
                    f"to look at the production code more carefully."
                    f"</tool-result>"
                )
            continue

        if tool_name == "read_file":
            if result.reads_done >= max_reads:
                tool_out = f"ERROR: read budget exhausted ({max_reads} reads). Submit your final test now."
                assistant_turn.tool_ok = False
            else:
                tool_out = _tool_read_file(repo_root, tool_arg)
                result.reads_done += 1
                assistant_turn.tool_ok = not tool_out.startswith("ERROR:")
            conversation.append(
                f"<tool-result turn={turn_i} tool=read_file path={tool_arg!r}>\n{tool_out}\n</tool-result>"
            )
            continue

        if tool_name == "list_dir":
            tool_out = _tool_list_dir(repo_root, tool_arg)
            assistant_turn.tool_ok = not tool_out.startswith("ERROR:")
            conversation.append(
                f"<tool-result turn={turn_i} tool=list_dir path={tool_arg!r}>\n{tool_out}\n</tool-result>"
            )
            continue

    # Out of turns. If we had a successful submission but never ran clean,
    # record final_code; halt_reason describes what stopped us.
    if result.submitted and not result.final_run_ok:
        last = result.attempts[-1] if result.attempts else None
        if last and last.compile_ok:
            result.halt_reason = "submitted_run_failed"
        else:
            result.halt_reason = "submitted_compile_failed"
    else:
        result.halt_reason = f"max_turns_exhausted after {max_turns}"
    return result
