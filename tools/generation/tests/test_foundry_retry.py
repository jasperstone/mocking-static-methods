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
