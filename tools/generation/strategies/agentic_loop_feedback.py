"""Agentic-loop strategy WITH compile feedback (phase 3).

Same text-based tool protocol as agentic_loop.py:
  - read_file(path)
  - list_dir(path)
  - submit_test(LANG)

But on `submit_test`, instead of returning immediately, we run a compile-only
check on the candidate and — if it fails — feed the compile errors back as a
synthetic tool result and let the model continue the conversation. The model
can then read more files, then call submit_test again with a revised version.

Budgets:
  - max_turns:               total assistant turns across initial + fix-up phase
  - max_reads:               total read_file calls
  - max_compile_attempts:    how many submit_test cycles we run compile_check on
                             (1 = same as phase 2; >1 = phase 3 fix-up loop)

Halt reasons (in addition to phase-2 set):
  - "submitted_compile_ok"           — submit_test succeeded AND compile passed
  - "submitted_compile_failed"       — final submission still didn't compile,
                                       fix-up budget exhausted
  - "max_turns_exhausted"            — ran out of conversation turns
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
class CompileAttempt:
    """One submit_test → compile cycle."""
    attempt_index: int           # 1-based
    turn_index: int              # the assistant turn that produced this submission
    compile_ok: bool
    build_ms: int
    errors: list[dict] = field(default_factory=list)
    code_sha256: str | None = None


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
    # Phase 3 additions:
    compile_attempts: list[CompileAttempt] = field(default_factory=list)
    final_compile_ok: bool = False


CompileFn = Callable[[str], "object"]  # text -> CompileResult-like (has .ok, .errors, .build_ms)


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


def run(
    *,
    generate,
    compile_fn: CompileFn,
    model_id: str,
    system_prompt: str,
    user_prompt: str,
    repo_root: Path,
    max_turns: int = 12,
    max_reads: int = 8,
    max_compile_attempts: int = 4,
    max_output_tokens: int = 4096,
    temperature: float = 0.0,
    top_p: float = 1.0,
    seed: int = 42,
    timeout_s: int = 180,
) -> FeedbackLoopResult:
    """Drive the agentic loop with compile feedback.

    `compile_fn(candidate_text) -> CompileResult` is injected: it should
    compile the candidate against the correct production project and return
    an object with `.ok: bool`, `.errors: list[dict]`, `.build_ms: int`.
    """
    import hashlib

    def _sha(t: str) -> str:
        return hashlib.sha256(t.encode("utf-8")).hexdigest()

    result = FeedbackLoopResult(submitted=False, final_code=None)
    conversation: list[str] = [user_prompt]
    compile_attempts_used = 0

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

            # We have a candidate. Run compile.
            compile_attempts_used += 1
            cres = compile_fn(chosen)
            errors_list = list(getattr(cres, "errors", []) or [])
            attempt = CompileAttempt(
                attempt_index=compile_attempts_used,
                turn_index=turn_i,
                compile_ok=bool(getattr(cres, "ok", False)),
                build_ms=int(getattr(cres, "build_ms", 0) or 0),
                errors=errors_list,
                code_sha256=_sha(chosen),
            )
            result.compile_attempts.append(attempt)
            # Always keep latest candidate as final_code so the runner can persist it.
            result.final_code = chosen
            result.submitted = True

            if attempt.compile_ok:
                assistant_turn.tool_ok = True
                result.final_compile_ok = True
                result.halt_reason = "submitted_compile_ok"
                return result

            # Compile failed. Decide whether we can give them another shot.
            attempts_left = max_compile_attempts - compile_attempts_used
            if attempts_left <= 0:
                assistant_turn.tool_ok = False
                result.final_compile_ok = False
                result.halt_reason = "submitted_compile_failed"
                return result

            # Feed errors back and let them revise.
            assistant_turn.tool_ok = False
            err_block = _format_errors_block(errors_list)
            extra = (getattr(cres, "error", None) or "").strip()
            extra_line = f"\n{extra}\n" if extra else ""
            conversation.append(
                f"<tool-result turn={turn_i} tool=submit_test compile_ok=false>"
                f"\nYour test did not compile. First errors:\n{err_block}{extra_line}\n"
                f"You have {attempts_left} more submission attempt(s). "
                f"You may call read_file() to inspect related code, "
                f"or call submit_test again with a revised file."
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

    # Out of turns. If we had a successful submission but never compiled OK, we
    # still record final_code; halt_reason describes what stopped us.
    if result.submitted and not result.final_compile_ok:
        result.halt_reason = "submitted_compile_failed"
    else:
        result.halt_reason = f"max_turns_exhausted after {max_turns}"
    return result
