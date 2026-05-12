#!/usr/bin/env python3
"""Smoke-ping every deployed model. Sends one trivial chat completion.
Records: model, surface, http_status, latency_ms, prompt_tokens, completion_tokens, response_snippet.
"""
from __future__ import annotations
import json
import os
import sys
import time
from pathlib import Path
from urllib import request, error

REPO_ROOT = Path(__file__).resolve().parents[2]
ENV_FILE = REPO_ROOT / ".env.foundry"


def load_env() -> dict[str, str]:
    env: dict[str, str] = {}
    for line in ENV_FILE.read_text().splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        k, v = line.split("=", 1)
        env[k.strip()] = v.strip()
    return env


def call_openai_surface(endpoint: str, key: str, deployment: str) -> dict:
    url = f"{endpoint}openai/deployments/{deployment}/chat/completions?api-version=2024-10-21"
    body = json.dumps({
        "messages": [{"role": "user", "content": "Reply with exactly the word PONG and nothing else."}],
        "max_tokens": 10,
        "temperature": 0,
    }).encode()
    req = request.Request(url, data=body, method="POST")
    req.add_header("api-key", key)
    req.add_header("Content-Type", "application/json")
    return _do(req)


def call_inference_surface(endpoint: str, key: str, model: str) -> dict:
    url = f"{endpoint}models/chat/completions?api-version=2024-05-01-preview"
    body = json.dumps({
        "model": model,
        "messages": [{"role": "user", "content": "Reply with exactly the word PONG and nothing else."}],
        "max_tokens": 10,
        "temperature": 0,
    }).encode()
    req = request.Request(url, data=body, method="POST")
    req.add_header("api-key", key)
    req.add_header("Content-Type", "application/json")
    return _do(req)


def _do(req) -> dict:
    t0 = time.time()
    try:
        with request.urlopen(req, timeout=120) as resp:
            data = json.loads(resp.read().decode())
            dt = int((time.time() - t0) * 1000)
            usage = data.get("usage", {})
            choices = data.get("choices", [])
            content = choices[0]["message"]["content"] if choices else ""
            return {
                "ok": True,
                "status": resp.status,
                "latency_ms": dt,
                "prompt_tokens": usage.get("prompt_tokens", 0),
                "completion_tokens": usage.get("completion_tokens", 0),
                "response": (content or "")[:80],
            }
    except error.HTTPError as e:
        body = e.read().decode(errors="replace")
        return {"ok": False, "status": e.code, "latency_ms": int((time.time() - t0) * 1000), "error": body[:400]}
    except Exception as e:
        return {"ok": False, "status": 0, "latency_ms": int((time.time() - t0) * 1000), "error": str(e)[:400]}


def main() -> int:
    env = load_env()
    endpoint = env["FOUNDRY_ENDPOINT"]
    key = env["FOUNDRY_API_KEY"]
    openai_panel = [m for m in env["FOUNDRY_PANEL_OPENAI"].split(",") if m]
    inference_panel = [m for m in env["FOUNDRY_PANEL_INFERENCE"].split(",") if m]

    print(f"{'model':<28} {'surface':<10} {'status':<7} {'latency':<9} {'p_tok':<6} {'c_tok':<6} response")
    print("-" * 100)
    results = []
    for m in openai_panel:
        r = call_openai_surface(endpoint, key, m)
        results.append({"model": m, "surface": "openai", **r})
        status = r["status"]
        lat = f"{r['latency_ms']}ms"
        if r["ok"]:
            print(f"{m:<28} {'openai':<10} {status:<7} {lat:<9} {r['prompt_tokens']:<6} {r['completion_tokens']:<6} {r['response']!r}")
        else:
            print(f"{m:<28} {'openai':<10} {status:<7} {lat:<9} {'-':<6} {'-':<6} ERROR: {r['error'][:200]}")
    for m in inference_panel:
        r = call_inference_surface(endpoint, key, m)
        results.append({"model": m, "surface": "inference", **r})
        status = r["status"]
        lat = f"{r['latency_ms']}ms"
        if r["ok"]:
            print(f"{m:<28} {'inference':<10} {status:<7} {lat:<9} {r['prompt_tokens']:<6} {r['completion_tokens']:<6} {r['response']!r}")
        else:
            print(f"{m:<28} {'inference':<10} {status:<7} {lat:<9} {'-':<6} {'-':<6} ERROR: {r['error'][:200]}")
    out = REPO_ROOT / "tools" / "generation" / "ping_results.json"
    out.write_text(json.dumps(results, indent=2))
    print(f"\nSaved {out}")
    return 0 if all(r["ok"] for r in results) else 1


if __name__ == "__main__":
    sys.exit(main())
