#!/usr/bin/env python3
"""Compile-only check for a candidate generated test file.

Factored from tools/evaluation/evaluate.py for use inside the agentic loop:
phase 3 needs to verify whether a model's submission compiles before deciding
whether to ask for a fix-up. We deliberately stop after `dotnet build` — we
do NOT run tests here. The full compile + run + coverage evaluator stays in
evaluate.py and runs in its own workflow stage against the FINAL submission.

Public entry point:

    compile_check(test_cs, repo_dir, target_file, timeout_s=240) -> CompileResult

Returns a small dataclass with .ok, .errors[], .build_ms, .stdout_tail.
Re-entrant and side-effect-clean: every call creates+removes its own sandbox
under {repo_dir}/.squad-eval/ (same convention as evaluate.py so parent
Directory.Build.props / nuget.config apply).
"""
from __future__ import annotations

import os
import re
import shutil
import subprocess
import tempfile
import time
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
