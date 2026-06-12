"""Mock LLM adapter for phase 4 smoke tests.

Returns canned responses keyed by (role, call_index). Reads fixtures from a
directory of JSON files. Mirrors the `generate(...)` signature used by
adapters/foundry.py so the multi-agent runner can swap them transparently.

Fixture file shape: JSON list of {"role": "writer"|"reviewer"|"fixer",
"text": "..."}. The adapter returns them in order per role.

This adapter exists EXCLUSIVELY for testing the multi-agent runner without
incurring Azure spend during the 2026-05-18 → ~2026-06-08 freeze.
"""
from __future__ import annotations

import json
import threading
from dataclasses import dataclass
from pathlib import Path


@dataclass
class GenerationResult:
    text: str
    model_snapshot: str
    prompt_tokens: int
    completion_tokens: int
    latency_ms: int
    finish_reason: str


class MockExhaustedError(RuntimeError):
    """Raised when a role's fixture queue is empty."""


class MockAdapter:
    """Fixture-driven LLM adapter. One instance per cell (per task)."""

    def __init__(self, fixtures_path: Path, role: str):
        self.fixtures_path = Path(fixtures_path)
        self.role = role
        if not self.fixtures_path.exists():
            raise FileNotFoundError(f"Mock fixtures not found: {fixtures_path}")
        all_turns = json.loads(self.fixtures_path.read_text())
        self._queue = [t["text"] for t in all_turns if t.get("role") == role]
        self._call_index = 0
        self._lock = threading.Lock()

    def generate(
        self,
        *,
        model_id: str,
        system_prompt: str,
        user_prompt: str,
        temperature: float = 0.0,
        top_p: float = 1.0,
        seed: int = 42,
        max_output_tokens: int = 4096,
        timeout_s: int = 180,
    ) -> GenerationResult:
        with self._lock:
            if self._call_index >= len(self._queue):
                raise MockExhaustedError(
                    f"Mock adapter for role={self.role!r} exhausted after "
                    f"{self._call_index} calls (fixture has "
                    f"{len(self._queue)} entries)."
                )
            text = self._queue[self._call_index]
            self._call_index += 1

        # Coarse token estimates: ~4 chars per token.
        prompt_tokens = max(1, (len(system_prompt) + len(user_prompt)) // 4)
        completion_tokens = max(1, len(text) // 4)
        return GenerationResult(
            text=text,
            model_snapshot=f"mock:{model_id}",
            prompt_tokens=prompt_tokens,
            completion_tokens=completion_tokens,
            latency_ms=1,
            finish_reason="stop",
        )


def make_role_generate(fixtures_path: Path, role: str):
    """Return a callable with the same signature as foundry.generate."""
    adapter = MockAdapter(fixtures_path, role)
    return adapter.generate
