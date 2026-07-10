"""Agentic-loop strategy WITH compile+run feedback AND an `apply_refactor` tool
(phase 4: agentic loop + testability refactoring).

This is the phase-3 `agentic_loop_feedback` loop plus one extra tool. The model
can now edit PRODUCTION source — but ONLY through a constrained transform menu
served by `tools.generation.apply_refactor.RefactorEngine`:

  - read_file(path)                       repo-relative read           (unchanged)
  - list_dir(path)                        repo-relative listing        (unchanged)
  - apply_refactor(transform=NAME, ...)   introduce a testability seam (NEW)
  - submit_test(LANG)                     finalize + compile/run check (unchanged)

apply_refactor tool-call syntax (documented for the prompts + smoke test):

  primary:  <tool>apply_refactor(transform=make_virtual)</tool>
  bare:     <tool>apply_refactor(make_virtual)</tool>
  extra kw: <tool>apply_refactor(transform=make_virtual, method=GetAsync)</tool>
  json:     <tool>apply_refactor({"transform": "wrapper_interface",
                                   "interface_name": "IHttpWrapper"})</tool>

On apply_refactor the engine applies the seam, rebuilds the owning production
project to confirm behaviour is preserved (auto-reverting if it no longer
builds), and the loop appends a `<tool-result>` describing success/rejection so
the model can react (e.g. read more code, pick a different transform, or write
the test against the new seam). Because `check_fn` rebuilds the owning csproj
from source, a successful seam is visible to the very next submit_test.

The production-write guard lives in the engine (`_safe_prod_path`): writes are
confined to the owning .csproj subtree. The runner snapshots originals and calls
`engine.restore_all()` after every cell, so cells never contaminate each other.

Result type extends FeedbackLoopResult with `refactor_attempts` (a list of
RefactorResult dicts in call order).
"""
from __future__ import annotations

import hashlib
import json
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Callable

from tools.generation.strategies.agentic_loop import (
    TOOL_RE,
    CODE_BLOCK_RE,
    Turn,
    _pick_test_block,
    _tool_read_file,
    _tool_list_dir,
)
from tools.generation.strategies.agentic_loop_feedback import (
    FeedbackLoopResult,
    SubmissionAttempt,
    _format_errors_block,
    _format_test_failures_block,
)
from tools.generation.apply_refactor import RefactorEngine, RefactorResult

# apply_refactor has its own regex because its argument is a small arg-list /
# JSON object rather than the single path the other three tools take. We detect
# it separately and prefer it when it appears first in the response.
APPLY_REFACTOR_RE = re.compile(
    r"<tool>\s*apply_refactor\s*\((.*?)\)\s*</tool>",
    re.IGNORECASE | re.DOTALL,
)


@dataclass
class RefactorLoopResult(FeedbackLoopResult):
    """FeedbackLoopResult + the refactor transforms applied/rejected this cell."""
    refactor_attempts: list[dict] = field(default_factory=list)


def parse_refactor_args(raw: str) -> tuple[str, dict]:
    """Parse the inside of apply_refactor(...) into (transform_name, kwargs).

    Accepts:
      - JSON object:        {"transform": "make_virtual", "method": "GetAsync"}
      - key=value list:     transform=make_virtual, method=GetAsync
      - bare transform:     make_virtual
    Returns ("", {}) if nothing usable is found.
    """
    s = (raw or "").strip()
    if not s:
        return "", {}

    if s.startswith("{"):
        try:
            obj = json.loads(s)
        except json.JSONDecodeError:
            return "", {}
        if not isinstance(obj, dict):
            return "", {}
        transform = str(obj.pop("transform", "") or "").strip()
        kwargs = {str(k): obj[k] for k in obj}
        return transform, kwargs

    # key=value comma list, or a bare transform token.
    kwargs: dict = {}
    transform = ""
    parts = [p.strip() for p in s.split(",") if p.strip()]
    for i, part in enumerate(parts):
        if "=" in part:
            k, v = part.split("=", 1)
            k, v = k.strip(), v.strip().strip("'\"")
            if k.lower() == "transform":
                transform = v
            else:
                kwargs[k] = v
        elif i == 0:
            # First bare token is the transform name.
            transform = part.strip().strip("'\"")
    return transform, kwargs


def _format_refactor_result(res: RefactorResult) -> str:
    """Render a RefactorResult as a compact <tool-result> body for the model."""
    if res.applied:
        return (
            f"apply_refactor({res.transform}) APPLIED.\n"
            f"  {res.reason}\n"
            f"  files changed: {', '.join(res.files_changed) or '(none)'}\n"
            f"  owning project still builds: {res.build_ok}\n"
            f"The seam is now in the production source. Write your test against it "
            f"(e.g. subclass-and-override the now-virtual method, or inject the new "
            f"dependency), then call submit_test(csharp)."
        )
    if res.reverted:
        err = _format_errors_block(res.errors)
        return (
            f"apply_refactor({res.transform}) REJECTED and auto-reverted.\n"
            f"  {res.reason}\n"
            f"  build errors:\n{err}\n"
            f"The production source is back to its original state. Pick a different "
            f"transform or test the code as-is."
        )
    return (
        f"apply_refactor({res.transform}) was NOT applied.\n"
        f"  {res.reason}\n"
        f"Pick a different transform/args, read_file() to inspect the declaration, "
        f"or write the test without a seam."
    )


def run(
    *,
    generate,
    check_fn: Callable[[str], "object"],
    engine: RefactorEngine,
    model_id: str,
    system_prompt: str,
    user_prompt: str,
    repo_root: Path,
    max_turns: int = 14,
    max_reads: int = 8,
    max_attempts: int = 4,
    max_refactors: int = 3,
    max_output_tokens: int = 4096,
    temperature: float = 0.0,
    top_p: float = 1.0,
    seed: int = 42,
    timeout_s: int = 180,
    progress_cb: Callable[..., None] | None = None,
) -> RefactorLoopResult:
    """Drive the phase-4 loop: compile+run feedback plus the apply_refactor tool.

    `engine` is a per-cell RefactorEngine confined to the owning csproj subtree.
    `check_fn(candidate_text) -> CompileRunResult` compiles+runs the candidate
    against the owning project (which already reflects any applied seam).
    """

    def _sha(t: str) -> str:
        return hashlib.sha256(t.encode("utf-8")).hexdigest()

    result = RefactorLoopResult(submitted=False, final_code=None)
    conversation: list[str] = [user_prompt]
    attempts_used = 0
    refactors_used = 0

    def _emit(stage: str, **fields: Any) -> None:
        if progress_cb is None:
            return
        try:
            progress_cb(stage=stage, **fields)
        except Exception:
            # Telemetry callbacks must never change loop behavior.
            pass

    for turn_i in range(1, max_turns + 1):
        _emit(
            "turn_start",
            turn_index=turn_i,
            reads_done=result.reads_done,
            attempts_used=attempts_used,
            refactors_used=refactors_used,
        )
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
            _emit("halt", reason=result.halt_reason, turn_index=turn_i)
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

        # Detect apply_refactor and the standard tools; prefer whichever the
        # model emitted first.
        ar = APPLY_REFACTOR_RE.search(r.text)
        m = TOOL_RE.search(r.text)

        if not ar and not m:
            assistant_turn.tool_ok = False
            result.turns.append(assistant_turn)
            conversation.append(f"<assistant-turn-{turn_i}>\n{r.text}\n</assistant-turn-{turn_i}>")
            conversation.append(
                f"<tool-result turn={turn_i}>"
                f"ERROR: no <tool>...</tool> call detected. You must call exactly one "
                f"tool per turn. Reply with one of: read_file(path), list_dir(path), "
                f"apply_refactor(transform=NAME), or submit_test(csharp)."
                f"</tool-result>"
            )
            continue

        use_refactor = ar is not None and (m is None or ar.start() <= m.start())

        if use_refactor:
            assistant_turn.tool_name = "apply_refactor"
            assistant_turn.tool_arg = ar.group(1)
            result.turns.append(assistant_turn)
            conversation.append(f"<assistant-turn-{turn_i}>\n{r.text}\n</assistant-turn-{turn_i}>")

            if refactors_used >= max_refactors:
                assistant_turn.tool_ok = False
                conversation.append(
                    f"<tool-result turn={turn_i} tool=apply_refactor>"
                    f"ERROR: refactor budget exhausted ({max_refactors}). Write your test "
                    f"against the current source and call submit_test(csharp)."
                    f"</tool-result>"
                )
                continue

            transform, kwargs = parse_refactor_args(ar.group(1))
            refactors_used += 1
            try:
                res = engine.apply(transform, **kwargs)
            except NotImplementedError as e:
                res = RefactorResult(
                    transform=transform or "(none)",
                    applied=False,
                    reason=f"transform not implemented in this pass: {e}",
                )
            except TypeError as e:
                # Bad kwargs from the model — report, don't crash.
                res = RefactorResult(
                    transform=transform or "(none)",
                    applied=False,
                    reason=f"invalid arguments for apply_refactor: {e}",
                )
            result.refactor_attempts.append(res.to_dict())
            assistant_turn.tool_ok = bool(res.applied)
            _emit(
                "refactor_result",
                turn_index=turn_i,
                transform=res.transform,
                applied=bool(res.applied),
                refactors_used=refactors_used,
            )
            conversation.append(
                f"<tool-result turn={turn_i} tool=apply_refactor "
                f"transform={res.transform} applied={str(res.applied).lower()}>"
                f"\n{_format_refactor_result(res)}\n</tool-result>"
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
            result.final_code = chosen
            result.submitted = True

            if run_ok:
                assistant_turn.tool_ok = True
                result.final_compile_ok = True
                result.final_run_ok = True
                result.halt_reason = "submitted_run_ok"
                return result

            attempts_left = max_attempts - attempts_used
            _emit(
                "submit_result",
                turn_index=turn_i,
                attempt_index=attempts_used,
                compile_ok=compile_ok,
                run_ok=run_ok,
                attempts_left=attempts_left,
            )
            if attempts_left <= 0:
                assistant_turn.tool_ok = False
                result.final_compile_ok = compile_ok
                result.final_run_ok = False
                result.halt_reason = (
                    "submitted_compile_failed" if not compile_ok else "submitted_run_failed"
                )
                return result

            assistant_turn.tool_ok = False
            extra = (getattr(cres, "error", None) or "").strip()
            if not compile_ok:
                err_block = _format_errors_block(errors_list)
                extra_line = f"\n  {extra}\n" if extra else ""
                conversation.append(
                    f"<tool-result turn={turn_i} tool=submit_test compile_ok=false run_ok=false>"
                    f"\nYour test did not compile. First errors:\n{err_block}{extra_line}\n"
                    f"You have {attempts_left} more submission attempt(s). You may call "
                    f"read_file()/list_dir() to inspect related code, apply_refactor() to add "
                    f"a testability seam, or call submit_test again with a revised file."
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
                    f"You have {attempts_left} more submission attempt(s). Revise your test "
                    f"(fix the assertion, add the missing setup/mocks, or change the input data) "
                    f"and resubmit. You may also read_file() or apply_refactor() to introduce a seam."
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
            _emit(
                "tool_result",
                turn_index=turn_i,
                tool_name="read_file",
                tool_ok=bool(assistant_turn.tool_ok),
                reads_done=result.reads_done,
            )
            conversation.append(
                f"<tool-result turn={turn_i} tool=read_file path={tool_arg!r}>\n{tool_out}\n</tool-result>"
            )
            continue

        if tool_name == "list_dir":
            tool_out = _tool_list_dir(repo_root, tool_arg)
            assistant_turn.tool_ok = not tool_out.startswith("ERROR:")
            _emit(
                "tool_result",
                turn_index=turn_i,
                tool_name="list_dir",
                tool_ok=bool(assistant_turn.tool_ok),
                reads_done=result.reads_done,
            )
            conversation.append(
                f"<tool-result turn={turn_i} tool=list_dir path={tool_arg!r}>\n{tool_out}\n</tool-result>"
            )
            continue

    if result.submitted and not result.final_run_ok:
        last = result.attempts[-1] if result.attempts else None
        if last and last.compile_ok:
            result.halt_reason = "submitted_run_failed"
        else:
            result.halt_reason = "submitted_compile_failed"
    else:
        result.halt_reason = f"max_turns_exhausted after {max_turns}"
    _emit("halt", reason=result.halt_reason)
    return result
