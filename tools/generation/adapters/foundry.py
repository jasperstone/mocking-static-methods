"""Azure AI Foundry adapter.

Single Foundry account hosts three API surfaces, accessed by model id:

  - Azure OpenAI Chat Completions  (gpt-4.1-mini, gpt-4.1-nano)
      {endpoint}openai/deployments/{name}/chat/completions?api-version=2024-10-21
  - Azure OpenAI Responses API     (gpt-5-codex)
      {endpoint}openai/responses?api-version=2025-04-01-preview
  - Foundry Models Inference       (everything else: phi-4, codestral, llama, grok)
      {endpoint}models/chat/completions?api-version=2024-05-01-preview

Auth: shared `api-key` header. Endpoint + key + panel split read from .env.foundry.

Determinism: temperature/top_p forwarded. Foundry does not honor seed for most
non-OpenAI models. Run distributions across runs_per_model are reported, not
single-run numbers.
"""
from __future__ import annotations
import json
import os
import time
import urllib.error
import urllib.request
from dataclasses import dataclass
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
ENV_FILE = REPO_ROOT / ".env.foundry"

OPENAI_CHAT_API = "2024-10-21"
OPENAI_RESPONSES_API = "2025-04-01-preview"
INFERENCE_API = "2024-05-01-preview"


@dataclass
class GenerationResult:
    text: str
    model_snapshot: str
    prompt_tokens: int
    completion_tokens: int
    latency_ms: int
    finish_reason: str


class FoundryError(RuntimeError):
    pass


_ENV_CACHE: dict[str, str] | None = None


def _load_env() -> dict[str, str]:
    global _ENV_CACHE
    if _ENV_CACHE is not None:
        return _ENV_CACHE
    env: dict[str, str] = {}
    if ENV_FILE.exists():
        for line in ENV_FILE.read_text().splitlines():
            line = line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            k, v = line.split("=", 1)
            env[k.strip()] = v.strip()
    # Allow process env to override file
    for k in ("FOUNDRY_ENDPOINT", "FOUNDRY_API_KEY",
              "FOUNDRY_PANEL_OPENAI_CHAT", "FOUNDRY_PANEL_OPENAI_RESPONSES",
              "FOUNDRY_PANEL_INFERENCE"):
        if os.environ.get(k):
            env[k] = os.environ[k]
    _ENV_CACHE = env
    return env


def _surface(model_id: str) -> str:
    env = _load_env()
    chat = set(env.get("FOUNDRY_PANEL_OPENAI_CHAT", "").split(",")) - {""}
    resp = set(env.get("FOUNDRY_PANEL_OPENAI_RESPONSES", "").split(",")) - {""}
    inf = set(env.get("FOUNDRY_PANEL_INFERENCE", "").split(",")) - {""}
    if model_id in chat:
        return "openai_chat"
    if model_id in resp:
        return "openai_responses"
    if model_id in inf:
        return "inference"
    raise FoundryError(
        f"model '{model_id}' is not in any FOUNDRY_PANEL_* list "
        f"(chat={sorted(chat)} resp={sorted(resp)} inf={sorted(inf)})"
    )


def list_panel() -> list[str]:
    env = _load_env()
    out: list[str] = []
    for k in ("FOUNDRY_PANEL_OPENAI_CHAT", "FOUNDRY_PANEL_OPENAI_RESPONSES", "FOUNDRY_PANEL_INFERENCE"):
        out += [m for m in env.get(k, "").split(",") if m]
    return out


def _request(url: str, body: dict, key: str, timeout_s: int) -> tuple[dict, int]:
    req_data = json.dumps(body).encode("utf-8")
    backoffs = [2, 5, 12, 25]  # 4 retries on 429/503; total wait <= 44s
    attempt = 0
    t0 = time.monotonic()
    while True:
        req = urllib.request.Request(
            url,
            data=req_data,
            headers={"api-key": key, "Content-Type": "application/json", "Accept": "application/json"},
            method="POST",
        )
        try:
            with urllib.request.urlopen(req, timeout=timeout_s) as resp:
                payload = json.loads(resp.read().decode("utf-8"))
            return payload, int((time.monotonic() - t0) * 1000)
        except urllib.error.HTTPError as e:
            body_txt = e.read().decode("utf-8", "replace")[:512]
            if e.code in (429, 503) and attempt < len(backoffs):
                # Honor Retry-After if present, else use backoff schedule.
                ra = e.headers.get("Retry-After") if hasattr(e, "headers") else None
                wait = int(ra) if ra and ra.isdigit() else backoffs[attempt]
                time.sleep(wait)
                attempt += 1
                continue
            raise FoundryError(f"HTTP {e.code}: {body_txt}")
        except urllib.error.URLError as e:
            raise FoundryError(f"network error: {e.reason}")
        except (TimeoutError, ConnectionError) as e:
            raise FoundryError(f"timeout/conn after {int((time.monotonic()-t0)*1000)}ms: {e}")


def _parse_chat(payload: dict, default_model: str, latency_ms: int) -> GenerationResult:
    try:
        choice = payload["choices"][0]
        text = choice["message"]["content"] or ""
        finish_reason = choice.get("finish_reason", "")
        usage = payload.get("usage", {})
        return GenerationResult(
            text=text,
            model_snapshot=payload.get("model", default_model),
            prompt_tokens=int(usage.get("prompt_tokens", 0)),
            completion_tokens=int(usage.get("completion_tokens", 0)),
            latency_ms=latency_ms,
            finish_reason=finish_reason,
        )
    except (KeyError, IndexError, TypeError) as e:
        raise FoundryError(f"unexpected chat response shape ({e}): {json.dumps(payload)[:512]}")


def _parse_responses(payload: dict, default_model: str, latency_ms: int) -> GenerationResult:
    """Parse Azure OpenAI /responses payload (used by gpt-5-codex).

    Output items can be type=message (assistant text), type=reasoning (chain of
    thought, sometimes contains the only visible text), or type=refusal. We
    take message text first; fall back to reasoning summaries if message is
    empty (gpt-5 reasoning models occasionally emit reasoning-only turns).
    """
    try:
        msg_parts: list[str] = []
        reasoning_parts: list[str] = []
        refusal_parts: list[str] = []
        for item in payload.get("output", []):
            itype = item.get("type")
            content = item.get("content") if isinstance(item.get("content"), list) else []
            if itype in ("message", "output_text", "text"):
                for part in content:
                    if isinstance(part, dict):
                        t = part.get("text") or part.get("refusal")
                        if t:
                            (refusal_parts if part.get("refusal") else msg_parts).append(t)
            elif itype == "reasoning":
                # reasoning items carry text in `summary` (list of {type,text}) on Azure
                for s in item.get("summary", []) or []:
                    t = s.get("text") if isinstance(s, dict) else None
                    if t:
                        reasoning_parts.append(t)
                for part in content:
                    if isinstance(part, dict) and part.get("text"):
                        reasoning_parts.append(part["text"])
        text = (
            "".join(msg_parts)
            or payload.get("output_text", "")
            or "".join(refusal_parts)
            or ("[reasoning-only output]\n" + "\n".join(reasoning_parts) if reasoning_parts else "")
        )
        usage = payload.get("usage", {})
        return GenerationResult(
            text=text,
            model_snapshot=payload.get("model", default_model),
            prompt_tokens=int(usage.get("input_tokens", 0)),
            completion_tokens=int(usage.get("output_tokens", 0)),
            latency_ms=latency_ms,
            finish_reason=payload.get("status", ""),
        )
    except (KeyError, TypeError) as e:
        raise FoundryError(f"unexpected responses payload ({e}): {json.dumps(payload)[:512]}")


def generate(
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
    env = _load_env()
    endpoint = env.get("FOUNDRY_ENDPOINT")
    key = env.get("FOUNDRY_API_KEY")
    if not endpoint or not key:
        raise FoundryError("FOUNDRY_ENDPOINT and FOUNDRY_API_KEY must be set (in .env.foundry or env vars)")

    surface = _surface(model_id)

    if surface == "openai_chat":
        url = f"{endpoint}openai/deployments/{model_id}/chat/completions?api-version={OPENAI_CHAT_API}"
        body = {
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
            "temperature": temperature,
            "top_p": top_p,
            "max_tokens": max_output_tokens,
            "seed": seed,
        }
        payload, dt = _request(url, body, key, timeout_s)
        return _parse_chat(payload, model_id, dt)

    if surface == "openai_responses":
        url = f"{endpoint}openai/responses?api-version={OPENAI_RESPONSES_API}"
        # gpt-5* family on /responses rejects temperature, top_p, seed.
        # Reasoning models pin sampling to defaults — omit them.
        # They also burn the output budget on internal reasoning items before
        # emitting a visible message, so give them a generous headroom.
        budget = max(max_output_tokens, 16384)
        body = {
            "model": model_id,
            "input": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
            "max_output_tokens": budget,
        }
        payload, dt = _request(url, body, key, timeout_s)
        return _parse_responses(payload, model_id, dt)

    # inference surface (model name in body)
    url = f"{endpoint}models/chat/completions?api-version={INFERENCE_API}"
    body = {
        "model": model_id,
        "messages": [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        "temperature": temperature,
        "top_p": top_p,
        "max_tokens": max_output_tokens,
    }
    payload, dt = _request(url, body, key, timeout_s)
    return _parse_chat(payload, model_id, dt)
