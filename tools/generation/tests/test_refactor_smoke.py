"""Smoke test for the phase-4 agentic-refactor runner using the mock LLM.

Validates (hermetically — NO dotnet / NO Azure / NO Foundry):
  - The phase-4 single-agent loop (read_file -> apply_refactor -> submit_test)
    wires up end-to-end through `agentic_refactor_runner.py`.
  - The `apply_refactor(transform=make_virtual)` tool actually APPLIES against a
    real in-repo declaration (we stand up a tiny throwaway `mock-repo` with a
    `.csproj` + a non-virtual `DoSomething` so the engine finds an owning
    project; mock mode sets `verify_build=False`, so no `dotnet build` runs).
  - All four expected output artifacts are produced:
        attempts.jsonl
        generated_tests/{repo}/{tid}/test.cs
        turns/{repo}/{tid}.jsonl
        refactors/{repo}/{tid}.jsonl
  - The refactors log records a `make_virtual` attempt (and it applied).

Why `--target-ids mock:0001`: the phase-4 runner's mock mode synthesizes a
single placeholder cell (target_id `mock:0001`, repo `mock-repo`, file
`mock.cs`, method `DoSomething`, kind `NonVirtual`) instead of reading
targets.csv. The `--target-ids` filter runs against that synthesized row, so we
pass the synthesized id to keep the cell. (Passing a real targets.csv id such as
`OpenRA:0003` would filter the synthesized row to empty.) The engine still
operates on a genuine NonVirtual instance method, so make_virtual exercises its
real `_inject_virtual` path.
"""
from __future__ import annotations

import json
import subprocess
import sys
import tempfile
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
RUNNER = REPO_ROOT / "tools" / "generation" / "agentic_refactor_runner.py"
FIXTURES = REPO_ROOT / "tools" / "generation" / "tests" / "fixtures" / "refactor"

# Matches the synthesized mock cell hardcoded by the runner in --mock-llm mode.
MOCK_TARGET_ID = "mock:0001"
MOCK_REPO = "mock-repo"
MOCK_FILE = "mock.cs"

# A non-virtual instance method on a non-sealed class, declared in-repo, so that
# apply_refactor(make_virtual) finds the declaration and applies the seam.
MOCK_SOURCE = """namespace MockSmoke;

public class MockService
{
    public string DoSomething()
    {
        return "ok";
    }
}
"""

MOCK_CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"""


def _build_mock_repo(cloned_root: Path) -> None:
    """Create cloned_root/mock-repo/{MockLib.csproj, mock.cs}."""
    repo = cloned_root / MOCK_REPO
    repo.mkdir(parents=True, exist_ok=True)
    (repo / "MockLib.csproj").write_text(MOCK_CSPROJ, encoding="utf-8")
    (repo / MOCK_FILE).write_text(MOCK_SOURCE, encoding="utf-8")


def test_refactor_smoke():
    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        cloned_root = tmp_path / "cloned_repos"
        out_dir = tmp_path / "smoke-out"
        _build_mock_repo(cloned_root)

        cmd = [
            sys.executable,
            str(RUNNER),
            "--phase", "phase4-refactoring",
            "--model", "mock-llm",
            "--run-index", "0",
            "--target-set", "v2",
            "--target-ids", MOCK_TARGET_ID,
            "--mock-llm",
            "--mock-fixtures-dir", str(FIXTURES),
            "--out-dir", str(out_dir),
            "--cloned-repos", str(cloned_root),
        ]
        proc = subprocess.run(cmd, capture_output=True, text=True, cwd=str(REPO_ROOT))
        assert proc.returncode == 0, (
            f"runner exited {proc.returncode}\nSTDOUT:\n{proc.stdout}\nSTDERR:\n{proc.stderr}"
        )

        tid_safe = MOCK_TARGET_ID.replace(":", "_")

        # 1. attempts.jsonl ------------------------------------------------
        attempts_path = out_dir / "attempts.jsonl"
        assert attempts_path.is_file(), f"missing {attempts_path}"
        lines = attempts_path.read_text().strip().splitlines()
        assert len(lines) == 1, f"expected 1 attempts row, got {len(lines)}"
        rec = json.loads(lines[0])
        assert rec["target_id"] == MOCK_TARGET_ID
        assert rec["phase"] == "phase4-refactoring"
        assert rec["model_id"] == "mock-llm"
        assert rec["strategy"] == "agentic_loop_refactor"
        assert rec["submitted"] is True
        assert rec["final_compile_ok"] is True
        assert rec["final_run_ok"] is True
        assert rec["halt_reason"] == "submitted_run_ok"
        assert rec["refactor_attempts_n"] >= 1

        # 2. generated test file ------------------------------------------
        test_file = out_dir / "generated_tests" / MOCK_REPO / tid_safe / "test.cs"
        assert test_file.is_file(), f"missing {test_file}"
        body = test_file.read_text()
        assert "[Fact]" in body
        assert "DoSomething" in body

        # 3. per-turn forensics -------------------------------------------
        turns_path = out_dir / "turns" / MOCK_REPO / f"{tid_safe}.jsonl"
        assert turns_path.is_file(), f"missing {turns_path}"
        turns = [json.loads(ln) for ln in turns_path.read_text().strip().splitlines()]
        tool_names = [t.get("tool_name") for t in turns]
        assert "apply_refactor" in tool_names
        assert "submit_test" in tool_names

        # 4. per-cell refactor log ----------------------------------------
        refactors_path = out_dir / "refactors" / MOCK_REPO / f"{tid_safe}.jsonl"
        assert refactors_path.is_file(), f"missing {refactors_path}"
        refactors = [
            json.loads(ln) for ln in refactors_path.read_text().strip().splitlines()
        ]
        assert len(refactors) >= 1, "refactor log is empty"
        mv = [r for r in refactors if r.get("transform") == "make_virtual"]
        assert mv, f"no make_virtual attempt recorded; got {refactors}"
        # The seam should APPLY against the in-repo declaration (verify_build is
        # off in mock mode, so no dotnet build gates this).
        assert mv[0]["applied"] is True, (
            f"make_virtual did not apply: {mv[0].get('reason')!r}"
        )
        assert mv[0]["reverted"] is False
        assert mv[0]["files_changed"], "applied transform changed no files"

        # The runner reverts production edits after the cell: the embedded
        # attempts-row copy of the refactor attempt agrees with the log.
        assert rec["refactor_attempts"][0]["transform"] == "make_virtual"
        assert rec["refactor_attempts"][0]["applied"] is True


if __name__ == "__main__":
    test_refactor_smoke()
    print("OK: phase-4 refactor smoke test passed")
