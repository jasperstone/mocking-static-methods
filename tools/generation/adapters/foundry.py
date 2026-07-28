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
import email.utils
import json
import os
import random
import re
import time
import urllib.error
import urllib.request
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
ENV_FILE = REPO_ROOT / ".env.foundry"

OPENAI_CHAT_API = "2024-10-21"
OPENAI_RESPONSES_API = "2025-04-01-preview"
INFERENCE_API = "2024-05-01-preview"
INFERENCE_FALLBACK_APIS = ("2024-05-01-preview", "2024-02-15-preview")


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
    for k in (
        "FOUNDRY_ENDPOINT",
        "FOUNDRY_API_KEY",
        "FOUNDRY_MODEL_ALIASES",
        "FOUNDRY_PANEL_OPENAI_CHAT",
        "FOUNDRY_PANEL_OPENAI_RESPONSES",
        "FOUNDRY_PANEL_INFERENCE",
        "FOUNDRY_RETRY_MAX_RETRIES",
        "FOUNDRY_RETRY_BUDGET_S",
        "FOUNDRY_RETRY_BASE_DELAY_S",
        "FOUNDRY_RETRY_MAX_DELAY_S",
        "FOUNDRY_RETRY_JITTER_RATIO",
    ):
        if os.environ.get(k):
            env[k] = os.environ[k]
    # Optional per-model/project credentials for split Foundry setups.
    # Examples: FOUNDRY_ENDPOINT_PHI + FOUNDRY_API_KEY_PHI.
    for key, value in os.environ.items():
        if value and (key.startswith("FOUNDRY_ENDPOINT_") or key.startswith("FOUNDRY_API_KEY_")):
            env[key] = value
    _ENV_CACHE = env
    return env


def _resolve_model_alias(env: dict[str, str], model_id: str) -> str:
    raw = env.get("FOUNDRY_MODEL_ALIASES", "").strip()
    if not raw:
        return model_id
    for item in raw.split(","):
        pair = item.strip()
        if not pair or ":" not in pair:
            continue
        src, dst = pair.split(":", 1)
        if model_id == src.strip() and dst.strip():
            return dst.strip()
    return model_id


def _is_project_endpoint(endpoint: str) -> bool:
    return "/api/projects/" in endpoint


def _is_full_chat_completions_endpoint(endpoint: str) -> bool:
    normalized = endpoint.strip().lower()
    return "/openai/v1/chat/completions" in normalized


def _is_full_chat_completions_endpoint(endpoint: str) -> bool:
    """True when endpoint is already a full /openai/v1/chat/completions URL.

    Accepts optional trailing slash and query string.
    """
    return re.search(r"/openai/v1/chat/completions/?(?:\?.*)?$", endpoint.rstrip("/"), re.IGNORECASE) is not None


def _normalize_endpoint(endpoint: str) -> str:
    endpoint = endpoint.strip()
    if not endpoint.endswith("/"):
        endpoint += "/"
    return endpoint


def _env_int(env: dict[str, str], key: str, default: int, min_value: int = 0) -> int:
    raw = env.get(key)
    if raw is None:
        return default
    try:
        val = int(raw)
    except ValueError:
        return default
    return max(min_value, val)


def _env_float(env: dict[str, str], key: str, default: float, min_value: float = 0.0) -> float:
    raw = env.get(key)
    if raw is None:
        return default
    try:
        val = float(raw)
    except ValueError:
        return default
    return max(min_value, val)


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


def _model_credential_suffixes(model_id: str) -> list[str]:
    """Return candidate suffixes for model-scoped credentials.

    Preference order is exact model-id first (normalized), then family aliases.
    Example: phi-4 -> PHI_4, PHI
    """
    normalized = re.sub(r"[^A-Za-z0-9]+", "_", model_id).strip("_").upper()
    out: list[str] = [normalized] if normalized else []
    if model_id.startswith("phi-"):
        out.append("PHI")
    elif model_id.startswith("grok-"):
        out.append("GROK")
    elif model_id.startswith("llama-"):
        out.append("LLAMA")
    elif model_id.startswith("codestral-"):
        out.append("CODESTRAL")

    # Keep order stable and remove duplicates.
    deduped: list[str] = []
    for suffix in out:
        if suffix and suffix not in deduped:
            deduped.append(suffix)
    return deduped


def _resolve_credentials(env: dict[str, str], model_id: str) -> tuple[str, str]:
    """Resolve endpoint/key with optional model-scoped override.

    If either endpoint or key exists for a model suffix, require both so we do
    not silently fall back to the wrong project in split deployments.
    """
    for suffix in _model_credential_suffixes(model_id):
        endpoint_key = f"FOUNDRY_ENDPOINT_{suffix}"
        api_key_key = f"FOUNDRY_API_KEY_{suffix}"
        endpoint = env.get(endpoint_key, "")
        api_key = env.get(api_key_key, "")
        if endpoint or api_key:
            if not endpoint or not api_key:
                raise FoundryError(
                    f"incomplete model credentials for '{model_id}': "
                    f"expected both {endpoint_key} and {api_key_key}"
                )
            return endpoint, api_key

    endpoint = env.get("FOUNDRY_ENDPOINT", "")
    api_key = env.get("FOUNDRY_API_KEY", "")
    if not endpoint or not api_key:
        raise FoundryError("FOUNDRY_ENDPOINT and FOUNDRY_API_KEY must be set (in .env.foundry or env vars)")
    return endpoint, api_key


def list_panel() -> list[str]:
    env = _load_env()
    out: list[str] = []
    for k in ("FOUNDRY_PANEL_OPENAI_CHAT", "FOUNDRY_PANEL_OPENAI_RESPONSES", "FOUNDRY_PANEL_INFERENCE"):
        out += [m for m in env.get(k, "").split(",") if m]
    return out


def _retry_after_seconds(headers) -> float | None:
    if not headers:
        return None
    value = headers.get("Retry-After")
    if not value:
        return None
    v = str(value).strip()
    if not v:
        return None
    if v.isdigit():
        return float(max(0, int(v)))
    try:
        dt = email.utils.parsedate_to_datetime(v)
    except (TypeError, ValueError):
        return None
    if dt is None:
        return None
    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)
    delta = (dt - datetime.now(timezone.utc)).total_seconds()
    return max(0.0, delta)


def _looks_rate_limited(body_txt: str) -> bool:
    text = body_txt.lower()
    needles = (
        "rate_limit_exceeded",
        "rate limit exceeded",
        "too_many_requests",
        "too many requests",
        "ratelimit",
    )
    return any(n in text for n in needles)


def _is_retriable_http(code: int, body_txt: str) -> bool:
    return code in (408, 429, 500, 502, 503, 504) or _looks_rate_limited(body_txt)


def _looks_api_version_unsupported(body_txt: str) -> bool:
    txt = body_txt.lower()
    needles = (
        "api version not supported",
        "unsupported api version",
        "unsupported api-version",
        "invalid api version",
        "api-version not supported",
    )
    return any(n in txt for n in needles)


def _compute_backoff_s(
    *,
    attempt: int,
    retry_base_delay_s: float,
    retry_max_delay_s: float,
    retry_jitter_ratio: float,
    retry_after_s: float | None = None,
) -> float:
    if retry_after_s is not None:
        # Honor provider-directed cooldown windows as-is. Clamping Retry-After
        # downward can create retry storms that keep triggering 429.
        return max(0.0, retry_after_s)
    exp_s = min(retry_max_delay_s, retry_base_delay_s * (2 ** attempt))
    jitter = random.uniform(0.0, exp_s * retry_jitter_ratio)
    return exp_s + jitter


def _request(
    url: str,
    body: dict,
    key: str,
    timeout_s: int,
    *,
    retry_max_retries: int,
    retry_budget_s: float,
    retry_base_delay_s: float,
    retry_max_delay_s: float,
    retry_jitter_ratio: float,
) -> tuple[dict, int]:
    req_data = json.dumps(body).encode("utf-8")
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
            if _is_retriable_http(e.code, body_txt) and attempt < retry_max_retries:
                retry_after_s = _retry_after_seconds(getattr(e, "headers", None))
                wait_s = _compute_backoff_s(
                    attempt=attempt,
                    retry_base_delay_s=retry_base_delay_s,
                    retry_max_delay_s=retry_max_delay_s,
                    retry_jitter_ratio=retry_jitter_ratio,
                    retry_after_s=retry_after_s,
                )
                elapsed = time.monotonic() - t0
                if elapsed + wait_s > retry_budget_s:
                    break
                time.sleep(wait_s)
                attempt += 1
                continue
            raise FoundryError(f"HTTP {e.code}: {body_txt}")
        except urllib.error.URLError as e:
            # Foundry endpoints intermittently emit transient connection/read
            # failures under load. Treat URLError as retriable within budget.
            if attempt < retry_max_retries:
                wait_s = _compute_backoff_s(
                    attempt=attempt,
                    retry_base_delay_s=retry_base_delay_s,
                    retry_max_delay_s=retry_max_delay_s,
                    retry_jitter_ratio=retry_jitter_ratio,
                )
                elapsed = time.monotonic() - t0
                if elapsed + wait_s <= retry_budget_s:
                    time.sleep(wait_s)
                    attempt += 1
                    continue
            raise FoundryError(f"network error: {e.reason}")
        except (TimeoutError, ConnectionError) as e:
            # Retry read/connect timeouts instead of hard-failing a cell on
            # first transient timeout.
            if attempt < retry_max_retries:
                wait_s = _compute_backoff_s(
                    attempt=attempt,
                    retry_base_delay_s=retry_base_delay_s,
                    retry_max_delay_s=retry_max_delay_s,
                    retry_jitter_ratio=retry_jitter_ratio,
                )
                elapsed = time.monotonic() - t0
                if elapsed + wait_s <= retry_budget_s:
                    time.sleep(wait_s)
                    attempt += 1
                    continue
            raise FoundryError(f"timeout/conn after {int((time.monotonic()-t0)*1000)}ms: {e}")
    raise FoundryError(
        f"retry budget exhausted after {attempt + 1} attempts over {int((time.monotonic() - t0) * 1000)}ms"
    )


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
    retry_max_retries: int | None = None,
    retry_budget_s: float | None = None,
    retry_base_delay_s: float | None = None,
    retry_max_delay_s: float | None = None,
    retry_jitter_ratio: float | None = None,
) -> GenerationResult:
    env = _load_env()
    endpoint, key = _resolve_credentials(env, model_id)
    endpoint_raw = endpoint.strip()
    endpoint = _normalize_endpoint(endpoint)

    if retry_max_retries is None:
        retry_max_retries = _env_int(env, "FOUNDRY_RETRY_MAX_RETRIES", default=8, min_value=0)
    if retry_budget_s is None:
        retry_budget_s = _env_float(env, "FOUNDRY_RETRY_BUDGET_S", default=180.0, min_value=0.0)
    if retry_base_delay_s is None:
        retry_base_delay_s = _env_float(env, "FOUNDRY_RETRY_BASE_DELAY_S", default=1.0, min_value=0.0)
    if retry_max_delay_s is None:
        retry_max_delay_s = _env_float(env, "FOUNDRY_RETRY_MAX_DELAY_S", default=30.0, min_value=0.1)
    if retry_jitter_ratio is None:
        retry_jitter_ratio = _env_float(env, "FOUNDRY_RETRY_JITTER_RATIO", default=0.25, min_value=0.0)

    surface = _surface(model_id)

    if surface == "openai_chat":
        resolved_model = _resolve_model_alias(env, model_id)
        if _is_project_endpoint(endpoint) or _is_full_chat_completions_endpoint(endpoint):
            # Project-scoped services.ai endpoints use the v1 path without
            # api-version query parameter. Also accept callers that provide a
            # full /openai/v1/chat/completions endpoint as the base.
            if _is_full_chat_completions_endpoint(endpoint):
                url = endpoint.rstrip("/")
            else:
                url = f"{endpoint}openai/v1/chat/completions"
            body = {
                "model": resolved_model,
                "messages": [
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": user_prompt},
                ],
                "temperature": temperature,
                "top_p": top_p,
                "max_tokens": max_output_tokens,
            }
        else:
            url = f"{endpoint}openai/deployments/{resolved_model}/chat/completions?api-version={OPENAI_CHAT_API}"
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
        payload, dt = _request(
            url,
            body,
            key,
            timeout_s,
            retry_max_retries=retry_max_retries,
            retry_budget_s=retry_budget_s,
            retry_base_delay_s=retry_base_delay_s,
            retry_max_delay_s=retry_max_delay_s,
            retry_jitter_ratio=retry_jitter_ratio,
        )
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
        payload, dt = _request(
            url,
            body,
            key,
            timeout_s,
            retry_max_retries=retry_max_retries,
            retry_budget_s=retry_budget_s,
            retry_base_delay_s=retry_base_delay_s,
            retry_max_delay_s=retry_max_delay_s,
            retry_jitter_ratio=retry_jitter_ratio,
        )
        return _parse_responses(payload, model_id, dt)

    # inference surface (model name in body)
    resolved_model = _resolve_model_alias(env, model_id)
    if _is_full_chat_completions_endpoint(endpoint_raw):
        # Model-scoped endpoint already points to chat completions.
        url = endpoint_raw
        body = {
            "model": resolved_model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
            "temperature": temperature,
            "top_p": top_p,
            "max_tokens": max_output_tokens,
        }
        payload, dt = _request(
            url,
            body,
            key,
            timeout_s,
            retry_max_retries=retry_max_retries,
            retry_budget_s=retry_budget_s,
            retry_base_delay_s=retry_base_delay_s,
            retry_max_delay_s=retry_max_delay_s,
            retry_jitter_ratio=retry_jitter_ratio,
        )
        return _parse_chat(payload, model_id, dt)

    if _is_project_endpoint(endpoint):
        # Project-scoped services.ai endpoints use the v1 path without
        # api-version query parameter.
        url = f"{endpoint}openai/v1/chat/completions"
        body = {
            "model": resolved_model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
            "temperature": temperature,
            "top_p": top_p,
            "max_tokens": max_output_tokens,
        }
        payload, dt = _request(
            url,
            body,
            key,
            timeout_s,
            retry_max_retries=retry_max_retries,
            retry_budget_s=retry_budget_s,
            retry_base_delay_s=retry_base_delay_s,
            retry_max_delay_s=retry_max_delay_s,
            retry_jitter_ratio=retry_jitter_ratio,
        )
        return _parse_chat(payload, model_id, dt)

    body = {
        "model": resolved_model,
        "messages": [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": user_prompt},
        ],
        "temperature": temperature,
        "top_p": top_p,
        "max_tokens": max_output_tokens,
    }
    last_err: FoundryError | None = None
    for idx, api_version in enumerate(INFERENCE_FALLBACK_APIS):
        url = f"{endpoint}models/chat/completions?api-version={api_version}"
        try:
            payload, dt = _request(
                url,
                body,
                key,
                timeout_s,
                retry_max_retries=retry_max_retries,
                retry_budget_s=retry_budget_s,
                retry_base_delay_s=retry_base_delay_s,
                retry_max_delay_s=retry_max_delay_s,
                retry_jitter_ratio=retry_jitter_ratio,
            )
            return _parse_chat(payload, model_id, dt)
        except FoundryError as e:
            msg = str(e)
            if idx + 1 < len(INFERENCE_FALLBACK_APIS) and _looks_api_version_unsupported(msg):
                last_err = e
                continue
            raise
    if last_err is not None:
        raise last_err
    raise FoundryError("inference API call failed without a recoverable fallback path")
