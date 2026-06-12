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
import os
import shutil
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


# ======================================================================
# §9.2 integration smoke + §4.3 via_seam verifier discrimination.
# ======================================================================
#
# The make_virtual smoke above carries an EMPTY seam, so via_seam stays None and
# nothing exercises the §4.3 verifier. The cases below drive the two Roslyn
# transforms (wrapper_interface on an ILogger extension call; parameterize_
# dependency on an HttpClient async call) end-to-end through the runner against a
# throwaway temp repo, then assert:
#   - the refactor APPLIED and produced a non-empty seam,
#   - files_changed includes the generated wrapper-interface file,
#   - the runner's verifier set via_seam correctly: TRUE for a test that injects
#     the mock through the seam, FALSE for a "gamed" test that builds a mock but
#     never injects it (the real forwarder still runs).
#
# These require the prebuilt RoslynRefactorTool.dll + a net10 dotnet runtime
# (the runner shells out to the tool even in --mock-llm mode; only the post-write
# `dotnet build` is skipped). They skip cleanly when that toolchain is absent.
# The pure-Python verifier unit tests at the bottom always run.

import pytest  # noqa: E402

sys.path.insert(0, str(REPO_ROOT))
from tools.evaluation.compile_only import DOTNET  # noqa: E402
from tools.generation.apply_refactor import _resolve_roslyn_tool_dll  # noqa: E402
from tools.generation.agentic_refactor_runner import verify_via_seam  # noqa: E402

_DLL = _resolve_roslyn_tool_dll()
_DOTNET = DOTNET if Path(DOTNET).exists() else shutil.which("dotnet")

_needs_tool = pytest.mark.skipif(
    _DLL is None or _DOTNET is None,
    reason="RoslynRefactorTool.dll not built or dotnet runtime unavailable "
           "(build: dotnet build RoslynRefactorTool/RoslynRefactorTool.csproj -c Release).",
)

# A minimal csproj just so apply_refactor's find_owning_csproj resolves an owning
# project; the Roslyn tool compiles the *.cs in-memory against its OWN bundled
# refs and never builds this project.
_LIB_CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"""

# ILogger extension-call site (line 16 = the LogInformation call).
_ILOGGER_SITE = """using Microsoft.Extensions.Logging;

namespace Acme;

public sealed class Worker
{
    private readonly ILogger _logger;

    public Worker(ILogger logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);
    }
}
"""

# HttpClient async-call site (line 17 = the GetAsync call).
_HTTPCLIENT_SITE = """using System.Net.Http;
using System.Threading.Tasks;

namespace Acme;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> FetchAsync(string url)
    {
        HttpResponseMessage resp = await _http.GetAsync(url);
        return await resp.Content.ReadAsStringAsync();
    }
}
"""


def _run_integration(tmp_path: Path, *, repo: str, site_file: str, site_src: str,
                     cell: dict, fixtures_dir: Path):
    """Stand up a temp repo + cell.json, run the runner in mock mode against the
    given fixtures dir, and return (returncode, stdout, stderr, out_dir, tid)."""
    cloned_root = tmp_path / "cloned_repos"
    out_dir = tmp_path / "out"
    repo_dir = cloned_root / repo
    repo_dir.mkdir(parents=True, exist_ok=True)
    (repo_dir / "Lib.csproj").write_text(_LIB_CSPROJ, encoding="utf-8")
    (repo_dir / site_file).write_text(site_src, encoding="utf-8")

    cell_json = tmp_path / "cell.json"
    cell_json.write_text(json.dumps(cell), encoding="utf-8")

    cmd = [
        sys.executable, str(RUNNER),
        "--phase", "phase4-refactoring",
        "--model", "mock-llm",
        "--run-index", "0",
        "--target-set", "v2",
        "--target-ids", cell["target_id"],
        "--mock-llm",
        "--mock-fixtures-dir", str(fixtures_dir),
        "--mock-cell-json", str(cell_json),
        "--out-dir", str(out_dir),
        "--cloned-repos", str(cloned_root),
    ]
    proc = subprocess.run(cmd, capture_output=True, text=True, cwd=str(REPO_ROOT))
    return proc.returncode, proc.stdout, proc.stderr, out_dir, cell["target_id"]


def _load_attempt(out_dir: Path, tid: str) -> dict:
    rows = (out_dir / "attempts.jsonl").read_text().strip().splitlines()
    assert len(rows) == 1, f"expected 1 attempts row, got {len(rows)}"
    rec = json.loads(rows[0])
    assert rec["target_id"] == tid
    return rec


def _load_refactor_log(out_dir: Path, repo: str, tid: str) -> list[dict]:
    p = out_dir / "refactors" / repo / f"{tid.replace(':', '_')}.jsonl"
    assert p.is_file(), f"missing refactor log {p}"
    return [json.loads(ln) for ln in p.read_text().strip().splitlines()]


@_needs_tool
def test_wrapper_interface_via_seam_legit():
    """wrapper_interface applies; a test that INJECTS the mock through the
    constructor and verifies on it -> via_seam=True."""
    with tempfile.TemporaryDirectory() as tmp:
        rc, out, err, out_dir, tid = _run_integration(
            Path(tmp),
            repo="ilogger-repo",
            site_file="Worker.cs",
            site_src=_ILOGGER_SITE,
            cell={
                "target_id": "mock:wrap-legit",
                "repo": "ilogger-repo",
                "file": "Worker.cs",
                "line": 16,
                "method": "LogInformation",
                "receiver_type": "ILogger",
                "kind": "Extension",
                "containing_type": "Worker",
            },
            fixtures_dir=REPO_ROOT / "tools" / "generation" / "tests"
            / "fixtures" / "refactor_wrapper_legit",
        )
        assert rc == 0, f"runner failed\nSTDOUT:\n{out}\nSTDERR:\n{err}"

        rec = _load_attempt(out_dir, tid)
        assert rec["submitted"] is True
        applied = [r for r in rec["refactor_attempts"]
                   if r.get("transform") == "wrapper_interface" and r.get("applied")]
        assert applied, f"wrapper_interface did not apply: {rec['refactor_attempts']}"
        seam = applied[-1]["seam"]
        assert seam, "applied refactor carried an empty seam"
        assert seam["interface"].split(".")[-1] == "ILoggerWrapper"
        assert seam["injection"] == "ctor"
        files_changed = applied[-1]["files_changed"]
        assert any(f.endswith("ILoggerWrapper.cs") for f in files_changed), files_changed

        assert rec["via_seam"] is True, rec.get("via_seam_checks")
        assert all(rec["via_seam_checks"].values()), rec["via_seam_checks"]

        # The verdict is also persisted on the refactor log for audit.
        log = _load_refactor_log(out_dir, "ilogger-repo", tid)
        verdicts = [r for r in log if r.get("verification")]
        assert verdicts and verdicts[-1]["via_seam"] is True


@_needs_tool
def test_wrapper_interface_via_seam_gamed():
    """Same refactor, but the test builds a mock and NEVER injects it -> the real
    forwarder runs, so the verifier must report via_seam=False."""
    with tempfile.TemporaryDirectory() as tmp:
        rc, out, err, out_dir, tid = _run_integration(
            Path(tmp),
            repo="ilogger-repo",
            site_file="Worker.cs",
            site_src=_ILOGGER_SITE,
            cell={
                "target_id": "mock:wrap-gamed",
                "repo": "ilogger-repo",
                "file": "Worker.cs",
                "line": 16,
                "method": "LogInformation",
                "receiver_type": "ILogger",
                "kind": "Extension",
                "containing_type": "Worker",
            },
            fixtures_dir=REPO_ROOT / "tools" / "generation" / "tests"
            / "fixtures" / "refactor_wrapper_gamed",
        )
        assert rc == 0, f"runner failed\nSTDOUT:\n{out}\nSTDERR:\n{err}"

        rec = _load_attempt(out_dir, tid)
        applied = [r for r in rec["refactor_attempts"]
                   if r.get("transform") == "wrapper_interface" and r.get("applied")]
        assert applied, f"wrapper_interface did not apply: {rec['refactor_attempts']}"

        assert rec["via_seam"] is False, rec.get("via_seam_checks")
        # The seam type IS referenced (mock constructed) but it is NOT injected,
        # which is exactly the gaming pattern the verifier exists to catch.
        checks = rec["via_seam_checks"]
        assert checks["seam_type_referenced"] is True
        assert checks["injected_at_injection_point"] is False


@_needs_tool
def test_parameterize_dependency_via_seam_legit():
    """parameterize_dependency adds an overload; a test that calls the overload
    with the mock and verifies on it -> via_seam=True."""
    with tempfile.TemporaryDirectory() as tmp:
        rc, out, err, out_dir, tid = _run_integration(
            Path(tmp),
            repo="httpclient-repo",
            site_file="ApiClient.cs",
            site_src=_HTTPCLIENT_SITE,
            cell={
                "target_id": "mock:param-legit",
                "repo": "httpclient-repo",
                "file": "ApiClient.cs",
                "line": 17,
                "method": "GetAsync",
                "receiver_type": "HttpClient",
                "kind": "NonVirtual",
                "containing_type": "ApiClient",
            },
            fixtures_dir=REPO_ROOT / "tools" / "generation" / "tests"
            / "fixtures" / "refactor_parameterize_legit",
        )
        assert rc == 0, f"runner failed\nSTDOUT:\n{out}\nSTDERR:\n{err}"

        rec = _load_attempt(out_dir, tid)
        applied = [r for r in rec["refactor_attempts"]
                   if r.get("transform") == "parameterize_dependency" and r.get("applied")]
        assert applied, f"parameterize_dependency did not apply: {rec['refactor_attempts']}"
        seam = applied[-1]["seam"]
        assert seam, "applied refactor carried an empty seam"
        assert seam["interface"].split(".")[-1] == "IHttpClientWrapper"
        assert seam["injection"] == "overload"
        files_changed = applied[-1]["files_changed"]
        assert any(f.endswith("IHttpClientWrapper.cs") for f in files_changed), files_changed

        assert rec["via_seam"] is True, rec.get("via_seam_checks")
        assert all(rec["via_seam_checks"].values()), rec["via_seam_checks"]


# ----------------------------------------------------------------------
# Pure-Python verifier unit tests (no dotnet) — exhaustive discrimination.
# ----------------------------------------------------------------------

_CTOR_SEAM = {
    "interface": "Acme.ILoggerWrapper",
    "containing_type": "Acme.Worker",
    "injection": "ctor",
    "injection_ref": "loggerWrapper",
    "member": "LogInformation",
}

_OVERLOAD_SEAM = {
    "interface": "Acme.IHttpClientWrapper",
    "containing_type": "Acme.ApiClient",
    "injection": "overload",
    "injection_ref": "FetchAsync(string, IHttpClientWrapper)",
    "member": "GetAsync",
}


def test_verify_via_seam_ctor_legit_moq():
    src = """using Moq;
var wrapper = new Mock<ILoggerWrapper>();
var worker = new Worker(logger, wrapper.Object);
worker.Run("j");
wrapper.Verify(w => w.LogInformation("starting {Job}", "j"), Times.Once);
"""
    ok, checks = verify_via_seam(_CTOR_SEAM, src)
    assert ok is True, checks
    assert all(checks.values())


def test_verify_via_seam_ctor_legit_named_arg_nsubstitute():
    src = """using NSubstitute;
var sub = Substitute.For<ILoggerWrapper>();
var worker = new Worker(logger, loggerWrapper: sub);
worker.Run("j");
sub.Received(1).LogInformation("starting {Job}", "j");
"""
    ok, checks = verify_via_seam(_CTOR_SEAM, src)
    assert ok is True, checks


def test_verify_via_seam_ctor_gamed_not_injected():
    src = """using Moq;
var wrapper = new Mock<ILoggerWrapper>();
var worker = new Worker(logger);
worker.Run("j");
Assert.NotNull(worker);
"""
    ok, checks = verify_via_seam(_CTOR_SEAM, src)
    assert ok is False
    assert checks["seam_type_referenced"] is True
    assert checks["injected_at_injection_point"] is False


def test_verify_via_seam_ctor_gamed_trivial_assert():
    # Injected correctly, but the only assertion is trivial -> rejected.
    src = """using Moq;
var wrapper = new Mock<ILoggerWrapper>();
var worker = new Worker(logger, wrapper.Object);
worker.Run("j");
Assert.True(true);
"""
    ok, checks = verify_via_seam(_CTOR_SEAM, src)
    assert ok is False
    assert checks["injected_at_injection_point"] is True
    assert checks["non_trivial_assertion"] is False


def test_verify_via_seam_overload_legit():
    src = """using Moq;
var client = new ApiClient(new HttpClient());
var wrapper = new Mock<IHttpClientWrapper>();
var body = await client.FetchAsync("http://x", wrapper.Object);
wrapper.Verify(w => w.GetAsync("http://x"), Times.Once);
"""
    ok, checks = verify_via_seam(_OVERLOAD_SEAM, src)
    assert ok is True, checks


def test_verify_via_seam_overload_gamed_calls_original():
    # Calls the ORIGINAL single-arg signature, mock never reaches the seam.
    src = """using Moq;
var client = new ApiClient(new HttpClient());
var wrapper = new Mock<IHttpClientWrapper>();
var body = await client.FetchAsync("http://x");
Assert.NotNull(body);
"""
    ok, checks = verify_via_seam(_OVERLOAD_SEAM, src)
    assert ok is False
    assert checks["seam_type_referenced"] is True
    assert checks["injected_at_injection_point"] is False


def test_verify_via_seam_empty_seam_is_falsey():
    # make_virtual carries no seam; an empty dict yields all-false checks.
    ok, checks = verify_via_seam({}, "Assert.Equal(1, svc.DoSomething());")
    assert ok is False


if __name__ == "__main__":
    test_refactor_smoke()
    print("OK: phase-4 refactor smoke test passed")
