"""Agentic-loop strategy.

Single model, multi-turn, text-based tool protocol. The model can call:
  - read_file(path)         repo-relative file read
  - list_dir(path)          repo-relative directory listing
  - submit_test(LANG)       finalize: next fenced block is the test file

Bounded by max_turns and max_reads. Captures every turn in attempts.jsonl.
"""
from __future__ import annotations
import json
import os
import re
import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable

TOOL_RE = re.compile(
    # Accepts:
    #   <tool>read_file(path)</tool>
    #   <tool>read_file("path")</tool>
    #   <tool>read_file(path="src/x.cs")</tool>
    #   <tool>read_file path</tool>           (paren-less, some models do this)
    r"<tool>\s*(read_file|list_dir|submit_test)\s*[\(\s]\s*(?:path\s*=\s*)?[\"']?(.*?)[\"']?\s*\)?\s*</tool>",
    re.IGNORECASE | re.DOTALL,
)
CODE_BLOCK_RE = re.compile(r"```(?:csharp|cs|c#)?\s*\n(.*?)```", re.DOTALL | re.IGNORECASE)


def _pick_test_block(blocks: list[str]) -> str | None:
    """From a list of fenced code blocks, choose the one most likely to be the
    actual test file. Some models nest fences (` ```csharp ` containing
    `<tool>submit_test(csharp)</tool>` then another ` ```csharp ... ``` ` with
    real code), so the literal first/last block can be wrong. Prefer the
    longest block that looks like C# (contains `using` or `[Fact]` or `class`).
    Fall back to the longest block.
    """
    if not blocks:
        return None
    csharp_signals = ("using ", "[Fact]", "class ", "namespace ")
    candidates = [b for b in blocks if any(s in b for s in csharp_signals)]
    pool = candidates if candidates else blocks
    return max(pool, key=len)


@dataclass
class Turn:
    turn_index: int
    role: str                 # "assistant" or "tool"
    text: str
    tool_name: str | None = None
    tool_arg: str | None = None
    tool_ok: bool | None = None
    latency_ms: int = 0
    prompt_tokens: int = 0
    completion_tokens: int = 0
    model_snapshot: str | None = None
    finish_reason: str | None = None


@dataclass
class LoopResult:
    submitted: bool
    final_code: str | None
    turns: list[Turn] = field(default_factory=list)
    total_prompt_tokens: int = 0
    total_completion_tokens: int = 0
    total_latency_ms: int = 0
    halt_reason: str = ""
    reads_done: int = 0


def _safe_repo_path(repo_root: Path, raw: str) -> Path | None:
    raw = raw.strip().strip("'\"")
    if not raw:
        return None
    p = (repo_root / raw).resolve()
    try:
        p.relative_to(repo_root.resolve())
    except ValueError:
        return None
    return p


def _tool_read_file(repo_root: Path, raw: str, max_chars: int = 8000) -> str:
    p = _safe_repo_path(repo_root, raw)
    if p is None:
        return f"ERROR: path '{raw}' is outside the repo or empty."
    if not p.exists():
        return f"ERROR: file not found: {raw}"
    if not p.is_file():
        return f"ERROR: not a regular file: {raw}"
    try:
        text = p.read_text(encoding="utf-8", errors="replace")
    except OSError as e:
        return f"ERROR: read failed: {e}"
    if len(text) > max_chars:
        return text[:max_chars] + f"\n\n[... truncated, file is {len(text)} chars total ...]"
    return text


def _tool_list_dir(repo_root: Path, raw: str, max_entries: int = 200) -> str:
    p = _safe_repo_path(repo_root, raw or ".")
    if p is None:
        return f"ERROR: path '{raw}' is outside the repo."
    if not p.exists():
        return f"ERROR: directory not found: {raw}"
    if not p.is_dir():
        return f"ERROR: not a directory: {raw}"
    entries = []
    for child in sorted(p.iterdir()):
        suffix = "/" if child.is_dir() else ""
        entries.append(child.name + suffix)
        if len(entries) >= max_entries:
            entries.append(f"[... truncated at {max_entries} entries ...]")
            break
    return "\n".join(entries) if entries else "(empty)"


GenerateFn = Callable[..., "object"]


def run(
    *,
    generate: GenerateFn,
    model_id: str,
    system_prompt: str,
    user_prompt: str,
    repo_root: Path,
    max_turns: int = 6,
    max_reads: int = 5,
    max_output_tokens: int = 4096,
    temperature: float = 0.0,
    top_p: float = 1.0,
    seed: int = 42,
    timeout_s: int = 180,
) -> LoopResult:
    """Drive the agentic loop. `generate` is the adapter's generate(...) callable.

    `generate` MUST accept the same kwargs as foundry.generate (model_id,
    system_prompt, user_prompt, temperature, top_p, seed, max_output_tokens, timeout_s)
    and return an object with .text, .model_snapshot, .prompt_tokens,
    .completion_tokens, .latency_ms, .finish_reason.
    """
    result = LoopResult(submitted=False, final_code=None)

    # We re-send the entire conversation history each turn by concatenating into
    # the user prompt. (Adapter is single-message-pair; conversation memory is
    # the runner's responsibility.) Format below mirrors a chat transcript.
    conversation: list[str] = [user_prompt]

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
            result.turns.append(Turn(turn_index=turn_i, role="assistant", text=f"<adapter-error: {e}>", finish_reason="adapter_error"))
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
            # Nudge once: tell the model it must use a tool.
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
            if chosen:
                result.submitted = True
                result.final_code = chosen
                result.halt_reason = "submitted"
                assistant_turn.tool_ok = True
                return result
            assistant_turn.tool_ok = False
            conversation.append(
                f"<tool-result turn={turn_i}>"
                f"ERROR: submit_test was called but no fenced ```csharp block followed. "
                f"Re-emit your final answer as: <tool>submit_test(csharp)</tool> followed by ```csharp ... ```."
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
            conversation.append(f"<tool-result turn={turn_i} tool=read_file path={tool_arg!r}>\n{tool_out}\n</tool-result>")
            continue

        if tool_name == "list_dir":
            tool_out = _tool_list_dir(repo_root, tool_arg)
            assistant_turn.tool_ok = not tool_out.startswith("ERROR:")
            conversation.append(f"<tool-result turn={turn_i} tool=list_dir path={tool_arg!r}>\n{tool_out}\n</tool-result>")
            continue

    result.halt_reason = f"max_turns_exhausted after {max_turns}"
    return result
