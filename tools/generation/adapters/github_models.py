"""GitHub Models adapter.

Endpoint: https://models.github.ai/inference (OpenAI-compatible Chat Completions).
Auth: Bearer token with `models:read` scope. Anyone with a GitHub account
can mint one, which is the reproducibility selling point of this experiment.

Response capture: every successful call records the `model` field returned
by the gateway (the actual snapshot) into attempts.jsonl. This is the only
way to know which snapshot served any given request, since the gateway
routes generic ids like `openai/gpt-5` to whatever's current.

Determinism: temperature/top_p/seed are forwarded but no closed-weight
provider currently guarantees deterministic decoding even at T=0. We
report distributions across runs_per_model runs, not single-run numbers.
"""
from __future__ import annotations
import json
import os
import time
import urllib.error
import urllib.request
from dataclasses import dataclass

ENDPOINT = "https://models.github.ai/inference/chat/completions"


@dataclass
class GenerationResult:
    text: str                # raw assistant message content
    model_snapshot: str      # the `model` field returned by the gateway
    prompt_tokens: int
    completion_tokens: int
    latency_ms: int
    finish_reason: str


class GitHubModelsError(RuntimeError):
    pass


def generate(
    *,
    model_id: str,
    system_prompt: str,
    user_prompt: str,
    temperature: float = 0.0,
    top_p: float = 1.0,
    seed: int = 42,
    max_output_tokens: int = 4096,
    timeout_s: int = 120,
    token: str | None = None,
) -> GenerationResult:
    """Single completion. Raises GitHubModelsError on non-200 or schema mismatch."""
    token = token or os.environ.get("GITHUB_MODELS_TOKEN") or os.environ.get("GITHUB_TOKEN")
    if not token:
        raise GitHubModelsError("GITHUB_MODELS_TOKEN (or GITHUB_TOKEN) is not set")

    body = {
        "model": model_id,
        "messages": [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        "temperature": temperature,
        "top_p": top_p,
        "max_tokens": max_output_tokens,
        # `seed` is forwarded but not all providers honor it. Kept for
        # documentation in attempts.jsonl alongside the actual response.
        "seed": seed,
    }

    req = urllib.request.Request(
        ENDPOINT,
        data=json.dumps(body).encode("utf-8"),
        headers={
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
            "Accept": "application/json",
        },
        method="POST",
    )

    t0 = time.monotonic()
    try:
        with urllib.request.urlopen(req, timeout=timeout_s) as resp:
            payload = json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as e:
        raise GitHubModelsError(f"HTTP {e.code}: {e.read().decode('utf-8', 'replace')[:512]}")
    except urllib.error.URLError as e:
        raise GitHubModelsError(f"network error: {e.reason}")
    latency_ms = int((time.monotonic() - t0) * 1000)

    try:
        choice = payload["choices"][0]
        text = choice["message"]["content"]
        finish_reason = choice.get("finish_reason", "")
        usage = payload.get("usage", {})
        return GenerationResult(
            text=text,
            model_snapshot=payload.get("model", model_id),
            prompt_tokens=int(usage.get("prompt_tokens", 0)),
            completion_tokens=int(usage.get("completion_tokens", 0)),
            latency_ms=latency_ms,
            finish_reason=finish_reason,
        )
    except (KeyError, IndexError, TypeError) as e:
        raise GitHubModelsError(f"unexpected response shape ({e}): {json.dumps(payload)[:512]}")
