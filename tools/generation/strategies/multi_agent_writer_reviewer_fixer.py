"""Multi-agent (writer / reviewer / fixer) strategy for phase 4.

Coordinates three LLM agents in a feedback loop:

  writer  → draft test
  build+test check
  reviewer → APPROVE | REQUEST_CHANGES + comment
  if APPROVE and run_ok: STOP
  if REQUEST_CHANGES (or run not OK): fixer revises → check → reviewer → ...

This module is intentionally thin. It delegates the inner agentic loop
(read_file / list_dir / submit_test) to `agentic_loop_feedback.run` for both
the writer and the fixer roles. The reviewer is one-shot: no tools, just a
structured verdict.

Halt reasons:
  - "approved_run_ok"          — reviewer APPROVE + last check was run_ok
  - "max_review_cycles"        — exhausted review cycles; latest draft submitted
  - "writer_failed_to_submit"  — writer never submitted any test
  - "fixer_failed_to_submit"   — fixer in cycle N could not submit
  - "adapter_error"            — downstream LLM adapter raised

This is scaffold-level code. It does NOT yet wire to a real Foundry adapter
in production (that wiring is the job of `multi_agent_runner.py` once the
Azure freeze ends). It DOES work end-to-end against the mock adapter for
smoke testing.
"""
from __future__ import annotations

import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable

from tools.generation.strategies.agentic_loop_feedback import (
    FeedbackLoopResult,
    SubmissionAttempt,
    run as run_agentic_loop,
)

VERDICT_RE = re.compile(r"<verdict>\s*(APPROVE|REQUEST_CHANGES)\s*</verdict>", re.IGNORECASE)
COMMENT_RE = re.compile(r"<comment>\s*(.*?)\s*</comment>", re.DOTALL | re.IGNORECASE)


@dataclass
class ReviewCycle:
    cycle_index: int                     # 1-based
    verdict: str                         # "APPROVE" | "REQUEST_CHANGES" | "MALFORMED"
    comment: str
    reviewer_prompt_tokens: int = 0
    reviewer_completion_tokens: int = 0
    reviewer_latency_ms: int = 0


@dataclass
class MultiAgentResult:
    submitted: bool
    final_code: str | None
    final_role: str                      # "writer" | "fixer"
    halt_reason: str
    # Per-agent loop results (preserve full trace for forensics).
    writer_loop: FeedbackLoopResult | None = None
    fixer_loops: list[FeedbackLoopResult] = field(default_factory=list)
    review_cycles: list[ReviewCycle] = field(default_factory=list)
    # Aggregated outcomes:
    attempts: list[SubmissionAttempt] = field(default_factory=list)
    final_compile_ok: bool = False
    final_run_ok: bool = False
    total_prompt_tokens: int = 0
    total_completion_tokens: int = 0
    total_latency_ms: int = 0


def _parse_reviewer(text: str) -> tuple[str, str]:
    """Extract (verdict, comment). Falls back to MALFORMED if not parseable."""
    v_match = VERDICT_RE.search(text or "")
    c_match = COMMENT_RE.search(text or "")
    verdict = v_match.group(1).upper() if v_match else "MALFORMED"
    comment = c_match.group(1).strip() if c_match else (text or "").strip()
    return verdict, comment


def _build_reviewer_user_prompt(
    *,
    user_task: str,
    draft_code: str,
    last_attempt: SubmissionAttempt | None,
) -> str:
    """Render the reviewer's one-shot user message."""
    parts: list[str] = []
    parts.append("ORIGINAL TASK:")
    parts.append(user_task.strip())
    parts.append("")
    parts.append("DRAFT TEST FILE (verbatim):")
    parts.append("```csharp")
    parts.append(draft_code.rstrip())
    parts.append("```")
    parts.append("")
    if last_attempt is None:
        parts.append("BUILD OUTCOME: no submission was made by the writer.")
    else:
        parts.append("BUILD + TEST OUTCOME:")
        if last_attempt.compile_ok and last_attempt.run_ok:
            parts.append(
                f"  compile_ok=true  run_ok=true  "
                f"tests_total={last_attempt.tests_total} "
                f"tests_passed={last_attempt.tests_passed}"
            )
        elif last_attempt.compile_ok:
            parts.append(
                f"  compile_ok=true  run_ok=false  "
                f"tests_total={last_attempt.tests_total} "
                f"tests_passed={last_attempt.tests_passed} "
                f"tests_failed={last_attempt.tests_failed}"
            )
            if last_attempt.tests_total == 0:
                parts.append("  (no [Fact] methods discovered)")
            for f in (last_attempt.test_failures or [])[:3]:
                name = f.get("test_name", "(unknown)")
                msg = (f.get("message") or "").strip().splitlines()[:1]
                parts.append(f"  FAILED: {name} — {msg[0] if msg else ''}")
        else:
            parts.append(f"  compile_ok=false  errors={len(last_attempt.errors)}")
            for e in (last_attempt.errors or [])[:3]:
                parts.append(
                    f"  {e.get('file','GeneratedTest.cs')}({e.get('line','?')}): "
                    f"error {e.get('code','?')}: {e.get('message','')}"
                )
    parts.append("")
    parts.append("Emit your verdict now.")
    return "\n".join(parts)


def _build_fixer_user_prompt(
    *,
    original_task: str,
    draft_code: str,
    reviewer_comment: str,
    last_attempt: SubmissionAttempt | None,
) -> str:
    parts: list[str] = []
    parts.append("ORIGINAL TASK:")
    parts.append(original_task.strip())
    parts.append("")
    parts.append("CURRENT DRAFT TEST FILE:")
    parts.append("```csharp")
    parts.append(draft_code.rstrip())
    parts.append("```")
    parts.append("")
    parts.append("REVIEWER COMMENT (address these defects):")
    parts.append(reviewer_comment.strip())
    parts.append("")
    if last_attempt is not None:
        if last_attempt.compile_ok and not last_attempt.run_ok:
            parts.append(
                f"Last run: compiled but {last_attempt.tests_failed} test(s) "
                f"failed out of {last_attempt.tests_total}."
            )
        elif not last_attempt.compile_ok:
            parts.append(
                f"Last run: did not compile ({len(last_attempt.errors)} errors)."
            )
    parts.append("")
    parts.append("Submit a revised, complete test file with submit_test.")
    return "\n".join(parts)


def run(
    *,
    writer_generate: Callable,
    reviewer_generate: Callable,
    fixer_generate: Callable,
    check_fn: Callable,
    model_id: str,
    writer_system_prompt: str,
    reviewer_system_prompt: str,
    fixer_system_prompt: str,
    user_prompt: str,
    repo_root: Path,
    writer_max_turns: int = 6,
    writer_max_reads: int = 4,
    reviewer_max_output_tokens: int = 1024,
    fixer_max_turns: int = 4,
    fixer_max_reads: int = 2,
    agent_max_attempts: int = 4,
    max_review_cycles: int = 3,
    max_output_tokens: int = 4096,
    temperature: float = 0.0,
    top_p: float = 1.0,
    seed: int = 42,
    timeout_s: int = 180,
) -> MultiAgentResult:
    """Drive writer → reviewer → fixer cycles.

    The shared `agent_max_attempts` budget is split across writer + fixer
    cycles. The writer gets one shot at the budget; each fixer cycle gets
    one too. We pass the remaining attempt count down to each inner loop.
    """
    result = MultiAgentResult(
        submitted=False,
        final_code=None,
        final_role="writer",
        halt_reason="",
    )

    # ---- WRITER ----
    writer_attempts_budget = max(1, agent_max_attempts - max_review_cycles)
    writer_loop = run_agentic_loop(
        generate=writer_generate,
        check_fn=check_fn,
        model_id=model_id,
        system_prompt=writer_system_prompt,
        user_prompt=user_prompt,
        repo_root=repo_root,
        max_turns=writer_max_turns,
        max_reads=writer_max_reads,
        max_attempts=writer_attempts_budget,
        max_output_tokens=max_output_tokens,
        temperature=temperature,
        top_p=top_p,
        seed=seed,
        timeout_s=timeout_s,
    )
    result.writer_loop = writer_loop
    result.total_prompt_tokens += writer_loop.total_prompt_tokens
    result.total_completion_tokens += writer_loop.total_completion_tokens
    result.total_latency_ms += writer_loop.total_latency_ms
    result.attempts.extend(writer_loop.attempts)

    if not writer_loop.submitted or writer_loop.final_code is None:
        result.halt_reason = "writer_failed_to_submit"
        result.submitted = False
        return result

    # Provisionally accept the writer's draft.
    result.submitted = True
    result.final_code = writer_loop.final_code
    result.final_role = "writer"
    result.final_compile_ok = writer_loop.final_compile_ok
    result.final_run_ok = writer_loop.final_run_ok

    # ---- REVIEW CYCLES ----
    attempts_used = len(writer_loop.attempts)

    for cycle_i in range(1, max_review_cycles + 1):
        last_attempt = result.attempts[-1] if result.attempts else None

        reviewer_user = _build_reviewer_user_prompt(
            user_task=user_prompt,
            draft_code=result.final_code or "",
            last_attempt=last_attempt,
        )
        try:
            rev = reviewer_generate(
                model_id=model_id,
                system_prompt=reviewer_system_prompt,
                user_prompt=reviewer_user,
                temperature=temperature,
                top_p=top_p,
                seed=seed,
                max_output_tokens=reviewer_max_output_tokens,
                timeout_s=timeout_s,
            )
        except Exception as e:
            result.review_cycles.append(ReviewCycle(
                cycle_index=cycle_i,
                verdict="ADAPTER_ERROR",
                comment=str(e),
            ))
            result.halt_reason = f"reviewer adapter error on cycle {cycle_i}: {e}"
            return result

        verdict, comment = _parse_reviewer(rev.text)
        result.review_cycles.append(ReviewCycle(
            cycle_index=cycle_i,
            verdict=verdict,
            comment=comment,
            reviewer_prompt_tokens=rev.prompt_tokens,
            reviewer_completion_tokens=rev.completion_tokens,
            reviewer_latency_ms=rev.latency_ms,
        ))
        result.total_prompt_tokens += rev.prompt_tokens
        result.total_completion_tokens += rev.completion_tokens
        result.total_latency_ms += rev.latency_ms

        if verdict == "APPROVE" and result.final_run_ok:
            result.halt_reason = "approved_run_ok"
            return result

        if attempts_used >= agent_max_attempts:
            result.halt_reason = "submission_budget_exhausted"
            return result

        # ---- FIXER ----
        fixer_user = _build_fixer_user_prompt(
            original_task=user_prompt,
            draft_code=result.final_code or "",
            reviewer_comment=comment,
            last_attempt=last_attempt,
        )
        fixer_loop = run_agentic_loop(
            generate=fixer_generate,
            check_fn=check_fn,
            model_id=model_id,
            system_prompt=fixer_system_prompt,
            user_prompt=fixer_user,
            repo_root=repo_root,
            max_turns=fixer_max_turns,
            max_reads=fixer_max_reads,
            max_attempts=1,
            max_output_tokens=max_output_tokens,
            temperature=temperature,
            top_p=top_p,
            seed=seed,
            timeout_s=timeout_s,
        )
        result.fixer_loops.append(fixer_loop)
        result.total_prompt_tokens += fixer_loop.total_prompt_tokens
        result.total_completion_tokens += fixer_loop.total_completion_tokens
        result.total_latency_ms += fixer_loop.total_latency_ms
        result.attempts.extend(fixer_loop.attempts)
        attempts_used += len(fixer_loop.attempts)

        if not fixer_loop.submitted or fixer_loop.final_code is None:
            # Keep the previous draft as the result.
            result.halt_reason = f"fixer_failed_to_submit_cycle_{cycle_i}"
            return result

        # Adopt the fixer's revised draft.
        result.final_code = fixer_loop.final_code
        result.final_role = "fixer"
        result.final_compile_ok = fixer_loop.final_compile_ok
        result.final_run_ok = fixer_loop.final_run_ok

    # Exhausted all cycles without an APPROVE.
    result.halt_reason = "max_review_cycles"
    return result
