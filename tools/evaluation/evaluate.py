#!/usr/bin/env python3
"""Compile + test evaluator for agentic-loop generated tests.

For each generated test.cs in `phases/<phase>/results/<model>/run_<i>/generated_tests/`,
this builds a sandbox xUnit test project that ProjectReferences the production
project owning the target file, drops the generated test in, and runs:

    1. dotnet build      -> compile_ok + first 5 errors
    2. dotnet test       -> run_ok + tests_passed/failed/skipped
    3. (optional) coverage on the target file via coverlet

Writes one JSON line per evaluated test to:
    phases/<phase>/results/<model>/run_<i>/evaluation.jsonl

Usage:
    python3 tools/evaluation/evaluate.py \
        --phase phase2-agentic --model gpt-4.1-mini --run-index 1 \
        --target-set v1
"""
from __future__ import annotations
import argparse
import csv
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
DOTNET = os.environ.get("DOTNET", str(Path.home() / ".dotnet" / "dotnet"))
if not Path(DOTNET).exists():
    DOTNET = "dotnet"

# Test project boilerplate. Targets net10.0 + xunit + Moq + NSubstitute +
# common .NET package references the generated tests are likely to need.
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

NUGET_CACHE = str(REPO_ROOT / ".nuget-cache")


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


def parse_trx(trx_path: Path) -> dict:
    """Parse a .trx file (xunit/MSTest VSTest output) → counts."""
    if not trx_path.exists():
        return {"tests_total": 0, "tests_passed": 0, "tests_failed": 0, "tests_skipped": 0}
    try:
        ns = {"vs": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
        root = ET.parse(trx_path).getroot()
        counters = root.find(".//vs:Counters", ns)
        if counters is None:
            return {"tests_total": 0, "tests_passed": 0, "tests_failed": 0, "tests_skipped": 0}
        return {
            "tests_total": int(counters.attrib.get("total", 0)),
            "tests_passed": int(counters.attrib.get("passed", 0)),
            "tests_failed": int(counters.attrib.get("failed", 0)),
            "tests_skipped": int(counters.attrib.get("notExecuted", 0)),
        }
    except (ET.ParseError, KeyError, ValueError) as e:
        return {"tests_total": 0, "tests_passed": 0, "tests_failed": 0, "tests_skipped": 0, "trx_error": str(e)}


def parse_cobertura_for_file(coverage_xml: Path, target_repo_relpath: str) -> dict | None:
    """Pull line-rate for the target source file out of cobertura.xml."""
    if not coverage_xml.exists():
        return None
    try:
        root = ET.parse(coverage_xml).getroot()
        # Match by suffix — coverage paths are absolute in cobertura
        suffix = target_repo_relpath.replace("\\", "/").lower()
        for cls in root.iter("class"):
            fn = (cls.attrib.get("filename") or "").replace("\\", "/").lower()
            if fn.endswith(suffix):
                lines = cls.findall(".//line")
                covered = sum(1 for ln in lines if int(ln.attrib.get("hits", 0)) > 0)
                total = len(lines)
                return {
                    "lines_covered": covered,
                    "lines_total": total,
                    "line_rate": (covered / total) if total else 0.0,
                }
    except (ET.ParseError, KeyError, ValueError):
        return None
    return None


COMPILE_ERR_RE = re.compile(r"^(.*?)\((\d+),(\d+)\):\s+error\s+(\w+):\s+(.*?)$", re.MULTILINE)


def first_compile_errors(stdout: str, n: int = 5) -> list[dict]:
    errs = []
    for m in COMPILE_ERR_RE.finditer(stdout):
        errs.append({"code": m.group(4), "message": m.group(5)[:200]})
        if len(errs) >= n:
            break
    return errs


def evaluate_one(
    test_cs: Path,
    repo_dir: Path,
    target_file: str,
    timeout_build: int,
    timeout_test: int,
) -> dict:
    """Build + run + (try) coverage for one generated test file."""
    rec: dict = {
        "test_file": str(test_cs),
        "compile_ok": False,
        "run_attempted": False,
        "run_ok": False,
    }

    csproj_path = find_owning_csproj(repo_dir, target_file)
    if csproj_path is None:
        rec["error"] = f"no owning csproj found under {repo_dir}/{target_file}"
        return rec
    rec["prod_csproj"] = str(csproj_path.relative_to(repo_dir))

    # Place the test project as a SIBLING of the production project, inside the
    # repo, so all parent Directory.Build.props/targets and nuget.config apply
    # naturally. Without this, package source mapping and transitive feeds break.
    sandbox_root = repo_dir / ".squad-eval"
    sandbox_root.mkdir(exist_ok=True)
    work = Path(tempfile.mkdtemp(prefix="eval_", dir=sandbox_root))
    try:
        # Test project
        (work / "TestProj.csproj").write_text(
            CSPROJ_TEMPLATE.format(
                nuget_cache=NUGET_CACHE,
                prod_csproj=str(csproj_path),
            )
        )
        # Drop the generated test, renamed for safety
        shutil.copy(test_cs, work / "GeneratedTest.cs")

        env = os.environ.copy()
        env["DOTNET_NOLOGO"] = "1"
        env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
        env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        env["NUGET_PACKAGES"] = NUGET_CACHE

        # 1. Build
        t0 = time.monotonic()
        try:
            br = subprocess.run(
                [DOTNET, "build", "-c", "Debug", "-v", "minimal", "--nologo",
                 "/p:TreatWarningsAsErrors=false", "/p:GenerateDocumentationFile=false"],
                cwd=work, capture_output=True, text=True,
                timeout=timeout_build, env=env,
            )
        except subprocess.TimeoutExpired:
            rec["build_timeout"] = True
            rec["build_ms"] = int((time.monotonic() - t0) * 1000)
            return rec
        rec["build_ms"] = int((time.monotonic() - t0) * 1000)
        rec["compile_ok"] = (br.returncode == 0)
        out_text = (br.stdout or "") + (br.stderr or "")
        if not rec["compile_ok"]:
            rec["compile_errors"] = first_compile_errors(out_text)
            rec["compile_stdout_tail"] = out_text[-800:]
            return rec

        # 2. Test
        rec["run_attempted"] = True
        results_dir = work / "TestResults"
        t0 = time.monotonic()
        try:
            tr = subprocess.run(
                [DOTNET, "test", "--no-build", "-v", "minimal", "--nologo",
                 "--logger", "trx;LogFileName=results.trx",
                 "--collect", "XPlat Code Coverage",
                 "--results-directory", str(results_dir)],
                cwd=work, capture_output=True, text=True,
                timeout=timeout_test, env=env,
            )
        except subprocess.TimeoutExpired:
            rec["test_timeout"] = True
            rec["test_ms"] = int((time.monotonic() - t0) * 1000)
            return rec
        rec["test_ms"] = int((time.monotonic() - t0) * 1000)
        rec["run_ok"] = (tr.returncode == 0)
        test_out = (tr.stdout or "") + (tr.stderr or "")
        rec["test_stdout_tail"] = test_out[-800:]
        # Parse trx
        trx_files = sorted(results_dir.rglob("results.trx"))
        if trx_files:
            rec.update(parse_trx(trx_files[0]))
        # Parse cobertura
        cov_files = sorted(results_dir.rglob("coverage.cobertura.xml"))
        if cov_files:
            cov = parse_cobertura_for_file(cov_files[0], target_file)
            if cov:
                rec["coverage_target_file"] = cov

        return rec
    finally:
        shutil.rmtree(work, ignore_errors=True)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--phase", required=True)
    ap.add_argument("--model", required=True)
    ap.add_argument("--run-index", type=int, required=True)
    ap.add_argument("--target-set", required=True)
    ap.add_argument("--cloned-repos", default=str(REPO_ROOT / "cloned_repos"))
    ap.add_argument("--build-timeout", type=int, default=180)
    ap.add_argument("--test-timeout", type=int, default=180)
    args = ap.parse_args()

    phase_dir = REPO_ROOT / "phases" / args.phase
    run_dir = phase_dir / "results" / args.model.replace("/", "__") / f"run_{args.run_index}"
    if not run_dir.is_dir():
        print(f"error: run dir not found: {run_dir}", file=sys.stderr)
        return 2

    targets = {}
    with (REPO_ROOT / "targets" / args.target_set / "targets.csv").open() as fh:
        for row in csv.DictReader(fh):
            targets[row["target_id"]] = row

    eval_path = run_dir / "evaluation.jsonl"
    cloned_root = Path(args.cloned_repos)

    n_compile = n_run = n_total = 0
    with eval_path.open("w") as out:
        # Iterate every test.cs under generated_tests/<repo>/<target_id>/
        for test_cs in sorted((run_dir / "generated_tests").rglob("test.cs")):
            target_id_safe = test_cs.parent.name        # repo_NNNN
            repo_name = test_cs.parent.parent.name
            target_id = target_id_safe.replace("_", ":", 1)
            row = targets.get(target_id)
            if row is None:
                out.write(json.dumps({"target_id": target_id, "error": "target not in targets.csv"}) + "\n")
                continue
            n_total += 1
            print(f"  evaluating {target_id} ({repo_name}) ...", flush=True)
            rec = evaluate_one(
                test_cs=test_cs,
                repo_dir=cloned_root / row["repo"],
                target_file=row["file"],
                timeout_build=args.build_timeout,
                timeout_test=args.test_timeout,
            )
            rec["target_id"] = target_id
            rec["model_id"] = args.model
            rec["run_index"] = args.run_index
            rec["phase"] = args.phase
            out.write(json.dumps(rec) + "\n")
            out.flush()
            if rec.get("compile_ok"):
                n_compile += 1
            if rec.get("run_ok"):
                n_run += 1
            tail = (
                f"compile={rec.get('compile_ok')} "
                f"run={rec.get('run_ok')} "
                f"tests={rec.get('tests_total','-')}/{rec.get('tests_passed','-')}p/{rec.get('tests_failed','-')}f "
                f"build_ms={rec.get('build_ms','-')} test_ms={rec.get('test_ms','-')}"
            )
            if rec.get("compile_errors"):
                tail += f" first_err={rec['compile_errors'][0]['code']}"
            print("    " + tail)

    print(f"\n{args.model} run_{args.run_index}: {n_compile}/{n_total} compile, {n_run}/{n_total} run OK")
    print(f"evaluation written to {eval_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
