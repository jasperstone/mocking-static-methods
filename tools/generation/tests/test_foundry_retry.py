from __future__ import annotations

import io
import json
import urllib.error

from tools.generation.adapters import foundry


class _FakeResponse:
    def __init__(self, payload: dict):
        self._payload = payload

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc, tb):
        return False

    def read(self) -> bytes:
        return json.dumps(self._payload).encode("utf-8")


def _http_error(code: int, body: str, retry_after: str | None = None) -> urllib.error.HTTPError:
    headers = {}
    if retry_after is not None:
        headers["Retry-After"] = retry_after
    return urllib.error.HTTPError(
        url="https://example.invalid",
        code=code,
        msg="error",
        hdrs=headers,
        fp=io.BytesIO(body.encode("utf-8")),
    )


def test_request_retries_429_until_success(monkeypatch):
    sleep_calls: list[float] = []
    monkeypatch.setattr(foundry.time, "sleep", lambda s: sleep_calls.append(float(s)))
    monkeypatch.setattr(foundry.random, "uniform", lambda _a, _b: 0.0)

    calls = {"n": 0}

    def fake_urlopen(_req, timeout):
        calls["n"] += 1
        if calls["n"] <= 2:
            raise _http_error(429, '{"error":{"code":"rate_limit_exceeded"}}', retry_after="1")
        return _FakeResponse({"ok": True})

    monkeypatch.setattr(foundry.urllib.request, "urlopen", fake_urlopen)

    payload, latency_ms = foundry._request(
        "https://example.invalid",
        {"hello": "world"},
        "secret",
        timeout_s=5,
        retry_max_retries=5,
        retry_budget_s=30,
        retry_base_delay_s=1,
        retry_max_delay_s=10,
        retry_jitter_ratio=0.25,
    )

    assert payload == {"ok": True}
    assert latency_ms >= 0
    assert calls["n"] == 3
    assert sleep_calls == [1.0, 1.0]


def test_request_honors_retry_after_without_capping(monkeypatch):
    sleep_calls: list[float] = []
    monkeypatch.setattr(foundry.time, "sleep", lambda s: sleep_calls.append(float(s)))
    monkeypatch.setattr(foundry.random, "uniform", lambda _a, _b: 0.0)

    calls = {"n": 0}

    def fake_urlopen(_req, timeout):
        calls["n"] += 1
        if calls["n"] == 1:
            raise _http_error(429, '{"error":{"code":"rate_limit_exceeded"}}', retry_after="120")
        return _FakeResponse({"ok": True})

    monkeypatch.setattr(foundry.urllib.request, "urlopen", fake_urlopen)

    payload, _ = foundry._request(
        "https://example.invalid",
        {"hello": "world"},
        "secret",
        timeout_s=5,
        retry_max_retries=3,
        retry_budget_s=300,
        retry_base_delay_s=1,
        retry_max_delay_s=10,
        retry_jitter_ratio=0.25,
    )

    assert payload == {"ok": True}
    assert calls["n"] == 2
    assert sleep_calls == [120.0]


def test_request_retries_rate_limit_marker_even_with_non_429(monkeypatch):
    monkeypatch.setattr(foundry.time, "sleep", lambda _s: None)
    monkeypatch.setattr(foundry.random, "uniform", lambda _a, _b: 0.0)

    calls = {"n": 0}

    def fake_urlopen(_req, timeout):
        calls["n"] += 1
        if calls["n"] == 1:
            raise _http_error(400, '{"error":{"code":"too_many_requests"}}')
        return _FakeResponse({"ok": True})

    monkeypatch.setattr(foundry.urllib.request, "urlopen", fake_urlopen)

    payload, _ = foundry._request(
        "https://example.invalid",
        {"hello": "world"},
        "secret",
        timeout_s=5,
        retry_max_retries=3,
        retry_budget_s=30,
        retry_base_delay_s=1,
        retry_max_delay_s=10,
        retry_jitter_ratio=0.25,
    )

    assert payload == {"ok": True}
    assert calls["n"] == 2


def test_generate_inference_falls_back_on_api_version_not_supported(monkeypatch):
    monkeypatch.setattr(
        foundry,
        "_load_env",
        lambda: {
            "FOUNDRY_ENDPOINT": "https://example.inference/",
            "FOUNDRY_API_KEY": "secret",
            "FOUNDRY_PANEL_OPENAI_CHAT": "",
            "FOUNDRY_PANEL_OPENAI_RESPONSES": "",
            "FOUNDRY_PANEL_INFERENCE": "phi-4",
        },
    )

    calls = {"urls": []}

    def fake_request(url, body, key, timeout_s, **kwargs):
        calls["urls"].append(url)
        if "api-version=2024-05-01-preview" in url:
            raise foundry.FoundryError('HTTP 400: {"error":{"code":"BadRequest","message":"API version not supported"}}')
        return (
            {
                "choices": [{"message": {"content": "ok"}, "finish_reason": "stop"}],
                "usage": {"prompt_tokens": 1, "completion_tokens": 2},
                "model": "phi-4",
            },
            12,
        )

    monkeypatch.setattr(foundry, "_request", fake_request)

    out = foundry.generate(
        model_id="phi-4",
        system_prompt="sys",
        user_prompt="usr",
        timeout_s=5,
    )

    assert out.text == "ok"
    assert calls["urls"] == [
        "https://example.inference/models/chat/completions?api-version=2024-05-01-preview",
        "https://example.inference/models/chat/completions?api-version=2024-02-15-preview",
    ]


def test_generate_inference_falls_back_on_unsupported_api_version_hyphenated(monkeypatch):
    monkeypatch.setattr(
        foundry,
        "_load_env",
        lambda: {
            "FOUNDRY_ENDPOINT": "https://example.inference/",
            "FOUNDRY_API_KEY": "secret",
            "FOUNDRY_PANEL_OPENAI_CHAT": "",
            "FOUNDRY_PANEL_OPENAI_RESPONSES": "",
            "FOUNDRY_PANEL_INFERENCE": "phi-4",
        },
    )

    calls = {"urls": []}

    def fake_request(url, body, key, timeout_s, **kwargs):
        calls["urls"].append(url)
        if "api-version=2024-05-01-preview" in url:
            raise foundry.FoundryError('HTTP 400: {"error":{"code":"BadRequest","message":"Unsupported API-Version"}}')
        return (
            {
                "choices": [{"message": {"content": "ok"}, "finish_reason": "stop"}],
                "usage": {"prompt_tokens": 1, "completion_tokens": 2},
                "model": "phi-4",
            },
            12,
        )

    monkeypatch.setattr(foundry, "_request", fake_request)

    out = foundry.generate(
        model_id="phi-4",
        system_prompt="sys",
        user_prompt="usr",
        timeout_s=5,
    )

    assert out.text == "ok"
    assert calls["urls"] == [
        "https://example.inference/models/chat/completions?api-version=2024-05-01-preview",
        "https://example.inference/models/chat/completions?api-version=2024-02-15-preview",
    ]


def test_generate_project_endpoint_uses_openai_v1_without_fallback(monkeypatch):
    monkeypatch.setattr(
        foundry,
        "_load_env",
        lambda: {
            "FOUNDRY_ENDPOINT": "https://example.services.ai/api/projects/p123/",
            "FOUNDRY_API_KEY": "secret",
            "FOUNDRY_PANEL_OPENAI_CHAT": "",
            "FOUNDRY_PANEL_OPENAI_RESPONSES": "",
            "FOUNDRY_PANEL_INFERENCE": "phi-4",
        },
    )

    calls = {"urls": []}

    def fake_request(url, body, key, timeout_s, **kwargs):
        calls["urls"].append(url)
        return (
            {
                "choices": [{"message": {"content": "ok"}, "finish_reason": "stop"}],
                "usage": {"prompt_tokens": 1, "completion_tokens": 1},
                "model": "phi-4",
            },
            8,
        )

    monkeypatch.setattr(foundry, "_request", fake_request)

    out = foundry.generate(
        model_id="phi-4",
        system_prompt="sys",
        user_prompt="usr",
        timeout_s=5,
    )

    assert out.text == "ok"
    assert calls["urls"] == [
        "https://example.services.ai/api/projects/p123/openai/v1/chat/completions"
    ]


def test_generate_openai_chat_project_endpoint_uses_v1(monkeypatch):
    monkeypatch.setattr(
        foundry,
        "_load_env",
        lambda: {
            "FOUNDRY_ENDPOINT": "https://example.services.ai/api/projects/p123/",
            "FOUNDRY_API_KEY": "secret",
            "FOUNDRY_PANEL_OPENAI_CHAT": "gpt-4.1-mini,gpt-4.1-nano",
            "FOUNDRY_PANEL_OPENAI_RESPONSES": "",
            "FOUNDRY_PANEL_INFERENCE": "phi-4",
        },
    )

    calls = {"urls": [], "bodies": []}

    def fake_request(url, body, key, timeout_s, **kwargs):
        calls["urls"].append(url)
        calls["bodies"].append(body)
        return (
            {
                "choices": [{"message": {"content": "ok"}, "finish_reason": "stop"}],
                "usage": {"prompt_tokens": 1, "completion_tokens": 1},
                "model": "gpt-4.1-mini",
            },
            8,
        )

    monkeypatch.setattr(foundry, "_request", fake_request)

    out = foundry.generate(
        model_id="gpt-4.1-mini",
        system_prompt="sys",
        user_prompt="usr",
        timeout_s=5,
    )

    assert out.text == "ok"
    assert calls["urls"] == [
        "https://example.services.ai/api/projects/p123/openai/v1/chat/completions"
    ]
    assert calls["bodies"][0]["model"] == "gpt-4.1-mini"
    assert "seed" not in calls["bodies"][0]


def test_generate_openai_chat_non_project_keeps_deployments_api_version(monkeypatch):
    monkeypatch.setattr(
        foundry,
        "_load_env",
        lambda: {
            "FOUNDRY_ENDPOINT": "https://example.openai/",
            "FOUNDRY_API_KEY": "secret",
            "FOUNDRY_PANEL_OPENAI_CHAT": "gpt-4.1-mini,gpt-4.1-nano",
            "FOUNDRY_PANEL_OPENAI_RESPONSES": "",
            "FOUNDRY_PANEL_INFERENCE": "phi-4",
        },
    )

    calls = {"urls": [], "bodies": []}

    def fake_request(url, body, key, timeout_s, **kwargs):
        calls["urls"].append(url)
        calls["bodies"].append(body)
        return (
            {
                "choices": [{"message": {"content": "ok"}, "finish_reason": "stop"}],
                "usage": {"prompt_tokens": 1, "completion_tokens": 1},
                "model": "gpt-4.1-mini",
            },
            8,
        )

    monkeypatch.setattr(foundry, "_request", fake_request)

    out = foundry.generate(
        model_id="gpt-4.1-mini",
        system_prompt="sys",
        user_prompt="usr",
        timeout_s=5,
    )

    assert out.text == "ok"
    assert calls["urls"] == [
        "https://example.openai/openai/deployments/gpt-4.1-mini/chat/completions?api-version=2024-10-21"
    ]
    assert calls["bodies"][0]["seed"] == 42
    assert "model" not in calls["bodies"][0]
