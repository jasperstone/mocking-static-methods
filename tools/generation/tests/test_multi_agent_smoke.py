"""Smoke test for the multi-agent runner using the fixture-driven mock LLM.

Validates:
  - The writer / reviewer / fixer loop wires up end-to-end.
  - The reviewer's APPROVE verdict short-circuits the fixer.
  - `attempts.jsonl` is well-formed and contains the expected fields.
  - No real Azure / Foundry calls are made (the mock adapter has no
    network code at all).
"""
from __future__ import annotations

import json
import subprocess
import sys
import tempfile
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
RUNNER = REPO_ROOT / "tools" / "generation" / "multi_agent_runner.py"
FIXTURES = REPO_ROOT / "tools" / "generation" / "tests" / "fixtures" / "multi_agent"


def test_multi_agent_smoke():
    with tempfile.TemporaryDirectory() as tmp:
        out_dir = Path(tmp) / "smoke-out"
        cmd = [
            sys.executable,
            str(RUNNER),
            "--phase", "phase4-multiagent",
            "--model", "mock-llm",
            "--run-index", "0",
            "--target-set", "v2",
            "--mock-llm",
            "--mock-fixtures-dir", str(FIXTURES),
            "--out-dir", str(out_dir),
            "--max-review-cycles", "1",
        ]
        proc = subprocess.run(cmd, capture_output=True, text=True, cwd=str(REPO_ROOT))
        assert proc.returncode == 0, (
            f"runner exited {proc.returncode}\nSTDOUT:\n{proc.stdout}\nSTDERR:\n{proc.stderr}"
        )

        attempts_path = out_dir / "attempts.jsonl"
        assert attempts_path.is_file(), f"missing {attempts_path}"

        lines = attempts_path.read_text().strip().splitlines()
        assert len(lines) == 1, f"expected 1 row, got {len(lines)}"

        rec = json.loads(lines[0])
        # Required scaffolding fields
        for key in (
            "target_id", "phase", "model_id", "strategy",
            "writer_max_turns", "fixer_max_turns", "max_review_cycles",
            "mock_llm", "submitted", "halt_reason", "final_role",
            "multi_agent_cycles", "review_summary",
            "final_compile_ok", "final_run_ok",
            "submission_attempts_n", "submission_iterations",
            "prompt_writer_system_sha256",
            "prompt_reviewer_system_sha256",
            "prompt_fixer_system_sha256",
            "prompt_user_template_sha256",
        ):
            assert key in rec, f"missing field: {key}"

        assert rec["mock_llm"] is True
        assert rec["strategy"] == "multi_agent_writer_reviewer_fixer"
        assert rec["submitted"] is True
        assert rec["final_compile_ok"] is True
        assert rec["final_run_ok"] is True
        # Reviewer APPROVE'd on cycle 1 → no fixer invocation.
        assert rec["final_role"] == "writer"
        assert rec["halt_reason"] == "approved_run_ok"
        assert rec["multi_agent_cycles"] == 1
        assert rec["review_summary"][0]["verdict"] == "APPROVE"

        # Generated test file should exist.
        rel = rec["test_path"]
        assert rel is not None
        test_file = out_dir / rel
        assert test_file.is_file()
        body = test_file.read_text()
        assert "[Fact]" in body
        assert "DoSomething" in body


if __name__ == "__main__":
    test_multi_agent_smoke()
    print("OK: multi-agent smoke test passed")
