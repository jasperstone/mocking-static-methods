#!/usr/bin/env python3
"""Candidate-test sandbox checks for the phase-3 agentic loop.

Two public entry points:

    compile_check(test_cs, repo_dir, target_file, timeout_s=240)
        -> CompileResult                          # build only

    compile_and_run_check(test_cs, repo_dir, target_file,
                          build_timeout_s=240, run_timeout_s=60)
        -> CompileRunResult                       # build + dotnet test

Both build a throwaway xUnit project under {repo_dir}/.squad-eval/ that
ProjectReferences the production .csproj nearest to `target_file`, drop the
candidate in as GeneratedTest.cs, run dotnet, and clean up. The full
compile + run + coverage evaluator lives in evaluate.py and runs offline on
the final submission; what's here is the in-loop fast path the model sees.
"""
from __future__ import annotations

import os
import re
import shutil
import subprocess
import tempfile
import time
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
DOTNET = os.environ.get("DOTNET", str(Path.home() / ".dotnet" / "dotnet"))
if not Path(DOTNET).exists():
    DOTNET = "dotnet"

NUGET_CACHE = str(REPO_ROOT / ".nuget-cache")

# Keep this in lockstep with tools/evaluation/evaluate.py CSPROJ_TEMPLATE.
CSPROJ_TEMPLATE = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <NoWarn>CS1591;CS8600;CS8602;CS8604;CS8618;CS8625;NU1605;NU1701;SKEXP0001;SKEXP0010;SKEXP0020;SKEXP0040;SKEXP0050;SKEXP0060;SKEXP0070;SKEXP0080;SKEXP0100;SKEXP0101;SKEXP0110;SKEXP0120;SKEXP0130</NoWarn>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <RestoreLockedMode>false</RestoreLockedMode>
    <RestorePackagesPath>{nuget_cache}</RestorePackagesPath>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="{prod_csproj}" />
  </ItemGroup>
</Project>
"""

COMPILE_ERR_RE = re.compile(
    r"^(.*?)\((\d+),(\d+)\):\s+error\s+(\w+):\s+(.*?)$", re.MULTILINE
)


def find_owning_csproj(repo_dir: Path, target_file: str) -> Path | None:
    """Find the .csproj nearest to (and an ancestor of) the target file."""
    p = (repo_dir / target_file).resolve().parent
    repo_root = repo_dir.resolve()
    while p == repo_root or repo_root in p.parents or p == repo_root:
        csprojs = sorted(p.glob("*.csproj"))
        if csprojs:
            return csprojs[0]
        if p == repo_root:
            return None
        p = p.parent
    return None


def first_compile_errors(stdout: str, n: int = 8) -> list[dict]:
    errs: list[dict] = []
    seen: set[tuple[str, str]] = set()
    for m in COMPILE_ERR_RE.finditer(stdout):
        code = m.group(4)
        msg = m.group(5).strip()
        # Trim repeated entries (msbuild prints per-target file).
        key = (code, msg[:120])
        if key in seen:
            continue
        seen.add(key)
        # Strip absolute sandbox path noise from the file ref.
        file_ref = m.group(1).strip()
        file_ref = re.sub(r"^.*[\\/]\.squad-eval[\\/][^\\/]+[\\/]", "", file_ref)
        errs.append({
            "file": file_ref,
            "line": int(m.group(2)),
            "col": int(m.group(3)),
            "code": code,
            "message": msg[:300],
        })
        if len(errs) >= n:
            break
    return errs


@dataclass
class CompileResult:
    ok: bool
    errors: list[dict] = field(default_factory=list)
    build_ms: int = 0
    stdout_tail: str = ""
    error: str | None = None
    prod_csproj: str | None = None


def compile_check(
    test_cs_text: str,
    repo_dir: Path,
    target_file: str,
    timeout_s: int = 240,
) -> CompileResult:
    """Compile `test_cs_text` against the owning project of `target_file`.

    Builds a throwaway xUnit test project under {repo_dir}/.squad-eval/eval_*
    that ProjectReferences the production .csproj nearest to `target_file`,
    drops the test code in as GeneratedTest.cs, runs `dotnet build`, returns
    structured pass/fail + first errors. Always cleans up its sandbox.
    """
    rec = CompileResult(ok=False)

    repo_dir = repo_dir.resolve()
    csproj_path = find_owning_csproj(repo_dir, target_file)
    if csproj_path is None:
        rec.error = f"no owning csproj found under {repo_dir}/{target_file}"
        return rec
    rec.prod_csproj = str(csproj_path.relative_to(repo_dir))

    sandbox_root = repo_dir / ".squad-eval"
    sandbox_root.mkdir(exist_ok=True)
    work = Path(tempfile.mkdtemp(prefix="compile_", dir=sandbox_root))
    try:
        (work / "TestProj.csproj").write_text(
            CSPROJ_TEMPLATE.format(
                nuget_cache=NUGET_CACHE,
                prod_csproj=str(csproj_path),
            )
        )
        (work / "GeneratedTest.cs").write_text(test_cs_text, encoding="utf-8")

        env = os.environ.copy()
        env["DOTNET_NOLOGO"] = "1"
        env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
        env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        env["NUGET_PACKAGES"] = NUGET_CACHE

        t0 = time.monotonic()
        try:
            br = subprocess.run(
                [DOTNET, "build", "-c", "Debug", "-v", "minimal", "--nologo",
                 "/p:TreatWarningsAsErrors=false",
                 "/p:GenerateDocumentationFile=false"],
                cwd=work, capture_output=True, text=True,
                timeout=timeout_s, env=env,
            )
        except subprocess.TimeoutExpired:
            rec.build_ms = int((time.monotonic() - t0) * 1000)
            rec.error = f"build timeout after {timeout_s}s"
            return rec

        rec.build_ms = int((time.monotonic() - t0) * 1000)
        rec.ok = (br.returncode == 0)
        out_text = (br.stdout or "") + (br.stderr or "")
        if not rec.ok:
            rec.errors = first_compile_errors(out_text)
            # Keep a short tail for forensics; never the whole log.
            rec.stdout_tail = out_text[-2000:]
        return rec
    finally:
        shutil.rmtree(work, ignore_errors=True)


def format_errors_for_model(errors: list[dict], max_errors: int = 6) -> str:
    """Render compile errors as a compact human-readable block for the agent."""
    if not errors:
        return "(no structured errors parsed)"
    lines = []
    for e in errors[:max_errors]:
        loc = f"{e.get('file','GeneratedTest.cs')}({e.get('line','?')},{e.get('col','?')})"
        lines.append(f"{loc}: error {e['code']}: {e['message']}")
    if len(errors) > max_errors:
        lines.append(f"... and {len(errors) - max_errors} more errors")
    return "\n".join(lines)


# ============================================================================
# Compile + run check (phase 3 option B): build, then `dotnet test`, parse TRX.
# ============================================================================

_TRX_NS = {"vs": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}


def _parse_trx_failures(trx_path: Path, max_failures: int = 5) -> tuple[dict, list[dict]]:
    """Return (counters, failure_details) from a .trx file.

    counters: {tests_total, tests_passed, tests_failed, tests_skipped}
    failure_details: list of {test_name, message, stack_tail} for the first
    `max_failures` failing test results, with messages trimmed to keep the
    feedback block small enough for the model.
    """
    counters = {"tests_total": 0, "tests_passed": 0, "tests_failed": 0, "tests_skipped": 0}
    failures: list[dict] = []
    if not trx_path.exists():
        return counters, failures
    try:
        root = ET.parse(trx_path).getroot()
    except ET.ParseError as e:
        counters["trx_error"] = str(e)
        return counters, failures

    c = root.find(".//vs:Counters", _TRX_NS)
    if c is not None:
        counters["tests_total"] = int(c.attrib.get("total", 0))
        counters["tests_passed"] = int(c.attrib.get("passed", 0))
        counters["tests_failed"] = int(c.attrib.get("failed", 0))
        counters["tests_skipped"] = int(c.attrib.get("notExecuted", 0))

    for ut in root.findall(".//vs:UnitTestResult", _TRX_NS):
        outcome = (ut.attrib.get("outcome") or "").lower()
        if outcome in ("passed", "notexecuted", "skipped"):
            continue
        test_name = ut.attrib.get("testName") or "(unknown)"
        msg_el = ut.find(".//vs:ErrorInfo/vs:Message", _TRX_NS)
        stack_el = ut.find(".//vs:ErrorInfo/vs:StackTrace", _TRX_NS)
        message = (msg_el.text or "").strip() if msg_el is not None else ""
        stack = (stack_el.text or "").strip() if stack_el is not None else ""
        # Keep only the first 2-3 stack lines (cheap repro for the model).
        stack_lines = [ln for ln in stack.splitlines() if ln.strip()][:3]
        failures.append({
            "test_name": test_name[:200],
            "message": message[:500],
            "stack_tail": "\n".join(stack_lines)[:600],
        })
        if len(failures) >= max_failures:
            break
    return counters, failures


@dataclass
class CompileRunResult:
    compile_ok: bool = False
    run_attempted: bool = False
    run_ok: bool = False               # True iff compiled AND dotnet test exit==0 AND tests_total > 0
    errors: list[dict] = field(default_factory=list)            # compile errors
    test_failures: list[dict] = field(default_factory=list)     # runtime/assertion failures
    tests_total: int = 0
    tests_passed: int = 0
    tests_failed: int = 0
    tests_skipped: int = 0
    build_ms: int = 0
    run_ms: int = 0
    stdout_tail: str = ""
    error: str | None = None
    prod_csproj: str | None = None
    timeout: str | None = None         # "build" | "test" | None


def compile_and_run_check(
    test_cs_text: str,
    repo_dir: Path,
    target_file: str,
    build_timeout_s: int = 240,
    run_timeout_s: int = 60,
) -> CompileRunResult:
    """Compile, then run, the candidate test against the owning csproj.

    Returns a CompileRunResult with both phases populated. If build fails the
    test phase is skipped. If build succeeds and `dotnet test` runs, we parse
    the TRX so the caller can feed failure messages back to the model. No
    coverage collection here — that stays in evaluate.py for offline scoring.
    """
    rec = CompileRunResult()
    repo_dir = repo_dir.resolve()
    csproj_path = find_owning_csproj(repo_dir, target_file)
    if csproj_path is None:
        rec.error = f"no owning csproj found under {repo_dir}/{target_file}"
        return rec
    rec.prod_csproj = str(csproj_path.relative_to(repo_dir))

    sandbox_root = repo_dir / ".squad-eval"
    sandbox_root.mkdir(exist_ok=True)
    work = Path(tempfile.mkdtemp(prefix="compile_run_", dir=sandbox_root))
    try:
        (work / "TestProj.csproj").write_text(
            CSPROJ_TEMPLATE.format(
                nuget_cache=NUGET_CACHE,
                prod_csproj=str(csproj_path),
            )
        )
        (work / "GeneratedTest.cs").write_text(test_cs_text, encoding="utf-8")

        env = os.environ.copy()
        env["DOTNET_NOLOGO"] = "1"
        env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
        env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        env["NUGET_PACKAGES"] = NUGET_CACHE

        # --- Build ---
        t0 = time.monotonic()
        try:
            br = subprocess.run(
                [DOTNET, "build", "-c", "Debug", "-v", "minimal", "--nologo",
                 "/p:TreatWarningsAsErrors=false",
                 "/p:GenerateDocumentationFile=false"],
                cwd=work, capture_output=True, text=True,
                timeout=build_timeout_s, env=env,
            )
        except subprocess.TimeoutExpired:
            rec.build_ms = int((time.monotonic() - t0) * 1000)
            rec.timeout = "build"
            rec.error = f"build timeout after {build_timeout_s}s"
            return rec
        rec.build_ms = int((time.monotonic() - t0) * 1000)
        rec.compile_ok = (br.returncode == 0)
        build_out = (br.stdout or "") + (br.stderr or "")
        if not rec.compile_ok:
            rec.errors = first_compile_errors(build_out)
            rec.stdout_tail = build_out[-2000:]
            return rec

        # --- Test ---
        rec.run_attempted = True
        results_dir = work / "TestResults"
        t0 = time.monotonic()
        try:
            tr = subprocess.run(
                [DOTNET, "test", "--no-build", "-v", "minimal", "--nologo",
                 "--logger", "trx;LogFileName=results.trx",
                 "--results-directory", str(results_dir)],
                cwd=work, capture_output=True, text=True,
                timeout=run_timeout_s, env=env,
            )
        except subprocess.TimeoutExpired:
            rec.run_ms = int((time.monotonic() - t0) * 1000)
            rec.timeout = "test"
            rec.error = f"test run timeout after {run_timeout_s}s"
            rec.stdout_tail = "<test run timed out>"
            return rec
        rec.run_ms = int((time.monotonic() - t0) * 1000)
        test_out = (tr.stdout or "") + (tr.stderr or "")
        rec.stdout_tail = test_out[-2000:]

        trx_files = sorted(results_dir.rglob("results.trx"))
        if trx_files:
            counters, failures = _parse_trx_failures(trx_files[0])
            rec.tests_total = counters.get("tests_total", 0)
            rec.tests_passed = counters.get("tests_passed", 0)
            rec.tests_failed = counters.get("tests_failed", 0)
            rec.tests_skipped = counters.get("tests_skipped", 0)
            rec.test_failures = failures

        # run_ok = compiled, dotnet test exit 0, and at least one test executed.
        rec.run_ok = (
            tr.returncode == 0
            and rec.tests_total > 0
            and rec.tests_failed == 0
        )
        return rec
    finally:
        shutil.rmtree(work, ignore_errors=True)


def format_test_failures_for_model(
    failures: list[dict],
    counters: dict | None = None,
    max_failures: int = 3,
) -> str:
    """Render test failures + counters as a compact block for the agent."""
    parts: list[str] = []
    if counters is not None:
        parts.append(
            f"Test counters: total={counters.get('tests_total', 0)} "
            f"passed={counters.get('tests_passed', 0)} "
            f"failed={counters.get('tests_failed', 0)} "
            f"skipped={counters.get('tests_skipped', 0)}"
        )
    if not failures:
        if counters and counters.get("tests_total", 0) == 0:
            parts.append("No [Fact] methods executed. Make sure your test class has at least one [Fact] method.")
        return "\n".join(parts) if parts else "(no test failures parsed)"

    for f in failures[:max_failures]:
        block = [f"FAILED: {f.get('test_name', '(unknown)')}"]
        msg = f.get("message", "").strip()
        if msg:
            # Trim multi-line messages to first 3 lines.
            msg_lines = [ln for ln in msg.splitlines() if ln.strip()][:3]
            block.append("  Message: " + " | ".join(msg_lines))
        stack = f.get("stack_tail", "").strip()
        if stack:
            for ln in stack.splitlines()[:3]:
                block.append("  at " + ln.strip())
        parts.append("\n".join(block))
    if len(failures) > max_failures:
        parts.append(f"... and {len(failures) - max_failures} more failed tests")
    return "\n".join(parts)
