#!/usr/bin/env python3
"""Phase-4 `apply_refactor` tool: constrained testability-seam transforms.

Phase 4 = the phase-3 single-agent compile+run feedback loop PLUS an
`apply_refactor` tool that edits PRODUCTION source to introduce a testability
seam, so a Mode #1 static call site (an extension method on an interface
receiver, OR a non-virtual instance method on a non-sealed class) becomes
mockable. The existing `compile_and_run_check` rebuilds the owning .csproj
from source, so seam edits are picked up for free on the next submit_test.

THE CONSTRAINT IS THE ANTI-GAMING MECHANISM. The agent cannot rewrite the
production code freely — it can only pick from a fixed transform menu:

  1. make_virtual          — add `virtual` to a non-virtual instance method so
                             a test can subclass-and-override. IMPLEMENTED.
  2. wrapper_interface     — generate an adapter interface + thin wrapper for
                             constructor injection. STUB (contract + TODO).
  3. parameterize_dependency — inject the dependency via a NEW defaulted
                             overload preserving the public API. STUB.

Safety rails (all implemented):
  * `_safe_prod_path` — writes are allowed ONLY inside the owning .csproj
    subtree. Escapes and out-of-subtree writes are rejected.
  * snapshot-on-write + `restore_all()` — original bytes are captured before
    the first edit to any file; `restore_all()` returns every touched file to
    pristine (and deletes files the engine created). The runner calls this
    after every cell so cells never contaminate each other and the git working
    tree stays clean.
  * behaviour-preservation build check — after a transform, the owning
    production project is rebuilt (`dotnet build`). If it no longer builds the
    transform is AUTO-REVERTED and a `refactor_rejected` RefactorResult is
    returned with the build errors.

The edits here are pure-Python, text/line-anchored (target line + method name).
A Roslyn-based transform (reusing the Mode1Analyzer/ infra with
Microsoft.CodeAnalysis.CSharp 4.14.0) is the robust future path for the harder
transforms — see the TODO stubs — but we deliberately do not block on standing
up a C# project here.
"""
from __future__ import annotations

import json
import os
import re
import subprocess
import time
from dataclasses import dataclass, field
from pathlib import Path

# Reuse the build toolchain knobs from the evaluation harness so the
# behaviour-preservation build matches the in-loop compile sandbox exactly.
from tools.evaluation.compile_only import (
    DOTNET,
    NUGET_CACHE,
    find_owning_csproj,
    first_compile_errors,
)

# Transform names the agent may request. The menu IS the contract.
TRANSFORMS = ("make_virtual", "wrapper_interface", "parameterize_dependency")

# The pure C# Roslyn rewriter that performs `wrapper_interface` and
# `parameterize_dependency` (TRANSFORM_CONTRACT §0). Built via
# `dotnet build RoslynRefactorTool/RoslynRefactorTool.csproj -c Release`.
_REPO_ROOT = Path(__file__).resolve().parents[2]
_ROSLYN_TOOL_DIR = _REPO_ROOT / "RoslynRefactorTool"


def _resolve_roslyn_tool_dll() -> Path | None:
    """Locate the built RoslynRefactorTool.dll, preferring Release over Debug."""
    for cfg in ("Release", "Debug"):
        cand = _ROSLYN_TOOL_DIR / "bin" / cfg / "net10.0" / "RoslynRefactorTool.dll"
        if cand.exists():
            return cand
    return None


ROSLYN_REFACTOR_TOOL_DLL = _resolve_roslyn_tool_dll()

_ACCESS_MODIFIERS = ("public", "protected", "internal", "private")
# Modifiers that mean a method is NOT a plain overridable instance method.
_NON_OVERRIDABLE = ("static", "virtual", "abstract", "override", "const", "sealed")


@dataclass
class RefactorResult:
    """Outcome of a single apply_refactor call.

    transform     — requested transform name
    applied       — True iff the seam edit is currently in place on disk
    reverted      — True iff an applied edit was rolled back (e.g. build broke)
    reason        — short machine-ish reason code / human explanation
    files_changed — repo-relative paths the transform wrote (post-state)
    build_ok      — result of the behaviour-preservation build (None if skipped)
    errors        — first build errors when build_ok is False
    seam          — seam descriptor from the Roslyn tool (TRANSFORM_CONTRACT §4);
                    {} for make_virtual and for not-applicable returns
    """
    transform: str
    applied: bool = False
    reverted: bool = False
    reason: str = ""
    files_changed: list[str] = field(default_factory=list)
    build_ok: bool | None = None
    errors: list[dict] = field(default_factory=list)
    seam: dict = field(default_factory=dict)

    def to_dict(self) -> dict:
        return {
            "transform": self.transform,
            "applied": self.applied,
            "reverted": self.reverted,
            "reason": self.reason,
            "files_changed": self.files_changed,
            "build_ok": self.build_ok,
            "errors": self.errors[:5],
            "seam": self.seam,
        }


def _safe_prod_path(repo_root: Path, owning_csproj_dir: Path, raw: str) -> Path | None:
    """Resolve `raw` (repo-relative or absolute) and allow it ONLY if it lives
    inside `owning_csproj_dir`. Returns the resolved Path, or None on any escape
    / empty / out-of-subtree path. This is the production-write guard.
    """
    if raw is None:
        return None
    raw = raw.strip().strip("'\"")
    if not raw:
        return None
    candidate = Path(raw)
    p = candidate.resolve() if candidate.is_absolute() else (repo_root / raw).resolve()
    base = owning_csproj_dir.resolve()
    try:
        p.relative_to(base)
    except ValueError:
        return None
    return p


def _inject_virtual(text: str, method: str) -> tuple[str | None, int | None]:
    """Add the `virtual` modifier to the declaration of `method` in `text`.

    Returns (new_text, 1-based_line) on success, (None, None) if no suitable
    non-virtual instance method declaration was found. Line-anchored and
    conservative: only touches a line that starts with an access modifier,
    names the method immediately before a `(`, and is not already
    static/virtual/abstract/override/sealed/const.
    """
    lines = text.splitlines(keepends=True)
    pat = re.compile(rf"\b{re.escape(method)}\s*\(")
    for i, line in enumerate(lines):
        stripped = line.strip()
        if not pat.search(stripped):
            continue
        tokens = stripped.split()
        if not tokens or tokens[0] not in _ACCESS_MODIFIERS:
            continue
        if any(kw in tokens for kw in _NON_OVERRIDABLE):
            continue
        # Must look like a declaration: at least <access> <returntype> <method>(
        # i.e. there is a return-type token between the modifier and the method.
        decl_head = stripped.split("(", 1)[0].split()
        if len(decl_head) < 3:
            continue
        if decl_head[-1].split("<")[0] != method and not decl_head[-1].startswith(method):
            # method name must be the last token before '('
            continue
        access = tokens[0]
        insert_at = line.find(access) + len(access)
        new_line = line[:insert_at] + " virtual" + line[insert_at:]
        lines[i] = new_line
        return "".join(lines), i + 1
    return None, None


class RefactorEngine:
    """Applies constrained testability-seam transforms to production source.

    One instance per cell. Construct with the repo root and the target-row
    metadata; the engine locates the owning .csproj (the production project
    that `compile_and_run_check` rebuilds) and confines every write to that
    project's subtree.
    """

    def __init__(
        self,
        repo_root: Path | str,
        target: dict,
        owning_csproj_dir: Path | str | None = None,
        *,
        verify_build: bool = True,
        build_timeout_s: int = 240,
    ):
        self.repo_root = Path(repo_root).resolve()
        self.target = dict(target)
        self.target_file = target.get("file", "")
        self.method = target.get("method", "")
        self.receiver_type = target.get("receiver_type", "")
        self.containing_type = target.get("containing_type", "")
        self.kind = target.get("kind", "")
        try:
            self.target_line = int(target.get("line", 0) or 0)
        except (TypeError, ValueError):
            self.target_line = 0
        self.verify_build = verify_build
        self.build_timeout_s = build_timeout_s

        # Locate the owning .csproj + its directory (the write-allowed subtree).
        if owning_csproj_dir is not None:
            self.owning_csproj_dir = Path(owning_csproj_dir).resolve()
            csprojs = sorted(self.owning_csproj_dir.glob("*.csproj"))
            self.owning_csproj = csprojs[0] if csprojs else None
        else:
            self.owning_csproj = (
                find_owning_csproj(self.repo_root, self.target_file)
                if self.target_file else None
            )
            self.owning_csproj_dir = (
                self.owning_csproj.parent if self.owning_csproj else self.repo_root
            )

        # path -> original bytes (None means "file did not exist before").
        self._snapshots: dict[Path, bytes | None] = {}

    # -- snapshot / restore ------------------------------------------------

    def _snapshot(self, p: Path) -> None:
        if p not in self._snapshots:
            self._snapshots[p] = p.read_bytes() if p.exists() else None

    def _write(self, p: Path, text: str) -> None:
        self._snapshot(p)
        p.parent.mkdir(parents=True, exist_ok=True)
        p.write_text(text, encoding="utf-8")

    def restore_all(self) -> list[str]:
        """Return every touched file to its pre-edit state. Files that the
        engine created (no prior bytes) are deleted. Returns the list of
        repo-relative paths restored. Safe to call multiple times.
        """
        restored: list[str] = []
        for p, original in self._snapshots.items():
            try:
                if original is None:
                    if p.exists():
                        p.unlink()
                else:
                    p.write_bytes(original)
                restored.append(self._rel(p))
            except OSError:
                pass
        self._snapshots.clear()
        return restored

    def _rel(self, p: Path) -> str:
        try:
            return str(p.relative_to(self.repo_root))
        except ValueError:
            return str(p)

    # -- behaviour-preservation build -------------------------------------

    def _build_owning_project(self) -> tuple[bool, list[dict], str]:
        """`dotnet build` the owning csproj. Returns (ok, errors, stdout_tail)."""
        if self.owning_csproj is None:
            return False, [{"code": "NOCSPROJ", "message": "no owning csproj located"}], ""
        env = os.environ.copy()
        env["DOTNET_NOLOGO"] = "1"
        env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
        env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        env["NUGET_PACKAGES"] = NUGET_CACHE
        try:
            br = subprocess.run(
                [DOTNET, "build", str(self.owning_csproj),
                 "-c", "Debug", "-v", "minimal", "--nologo",
                 "/p:TreatWarningsAsErrors=false",
                 "/p:GenerateDocumentationFile=false"],
                cwd=str(self.owning_csproj_dir), capture_output=True, text=True,
                timeout=self.build_timeout_s, env=env,
            )
        except subprocess.TimeoutExpired:
            return False, [{"code": "TIMEOUT", "message": f"build timeout after {self.build_timeout_s}s"}], ""
        except OSError as e:
            return False, [{"code": "EXEC", "message": f"dotnet invocation failed: {e}"}], ""
        out = (br.stdout or "") + (br.stderr or "")
        ok = br.returncode == 0
        errors = [] if ok else first_compile_errors(out)
        return ok, errors, out[-2000:]

    # -- dispatch ----------------------------------------------------------

    def apply(self, transform_name: str, **args) -> RefactorResult:
        """Dispatch a transform from the fixed menu. Unknown transforms are
        rejected. On success, runs the behaviour-preservation build (unless
        `verify_build=False`) and auto-reverts if the project no longer builds.
        """
        name = (transform_name or "").strip().lower()
        if name not in TRANSFORMS:
            return RefactorResult(
                transform=name or "(none)",
                applied=False,
                reason=f"unknown transform '{transform_name}'. "
                       f"Choose one of: {', '.join(TRANSFORMS)}.",
            )
        if self.owning_csproj is None:
            return RefactorResult(
                transform=name, applied=False,
                reason=f"no owning .csproj found for target file '{self.target_file}'.",
            )

        if name == "make_virtual":
            res = self._make_virtual(**args)
        elif name == "wrapper_interface":
            res = self._wrapper_interface(**args)
        else:  # parameterize_dependency
            res = self._parameterize_dependency(**args)

        # Behaviour-preservation: if we changed code, the owning project must
        # still build. Otherwise auto-revert and report rejection.
        if res.applied and self.verify_build:
            ok, errors, _tail = self._build_owning_project()
            res.build_ok = ok
            if not ok:
                self.restore_all()
                res.applied = False
                res.reverted = True
                res.errors = errors
                res.reason = "refactor_rejected: owning project no longer builds after the edit"
        return res

    # -- transform 1: make_virtual (IMPLEMENTED) --------------------------

    def _make_virtual(self, **args) -> RefactorResult:
        """Add `virtual` to the target instance method's declaration so a test
        can subclass-and-override it. Applies to NonVirtual-kind sites whose
        method is declared in the owning production project.

        Optional args:
          file   — repo-relative path of the declaring file (default: target_file)
          method — method name to make virtual (default: the target method)
        """
        method = (args.get("method") or self.method or "").strip()
        if not method:
            return RefactorResult("make_virtual", reason="no method name available to make virtual.")

        # Candidate files: the explicit/declaring file first, then the target
        # file, then a bounded scan of the owning subtree for the declaration.
        candidates: list[Path] = []
        raw_file = args.get("file") or self.target_file
        primary = _safe_prod_path(self.repo_root, self.owning_csproj_dir, raw_file)
        if primary is not None and primary.is_file():
            candidates.append(primary)
        elif args.get("file"):
            return RefactorResult(
                "make_virtual",
                reason=f"write target '{args.get('file')}' is outside the owning project "
                       f"subtree ({self._rel(self.owning_csproj_dir)}); rejected by prod-write guard.",
            )

        if not candidates:
            # Scan the owning subtree (bounded) for the declaration.
            for cs in sorted(self.owning_csproj_dir.rglob("*.cs")):
                if "/obj/" in str(cs) or "/bin/" in str(cs):
                    continue
                candidates.append(cs)
                if len(candidates) >= 500:
                    break

        for path in candidates:
            try:
                text = path.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            new_text, line_no = _inject_virtual(text, method)
            if new_text is not None:
                self._write(path, new_text)
                return RefactorResult(
                    "make_virtual",
                    applied=True,
                    reason=f"added `virtual` to {method}() at {self._rel(path)}:{line_no}.",
                    files_changed=[self._rel(path)],
                )

        return RefactorResult(
            "make_virtual",
            applied=False,
            reason=f"could not find a non-virtual instance declaration of '{method}' in the "
                   f"owning project. (If it lives in a framework type it cannot be made virtual; "
                   f"consider wrapper_interface instead.)",
        )

    # -- Roslyn subprocess bridge (wrapper_interface / parameterize_dependency)

    def _invoke_roslyn_tool(self, transform: str, args: dict) -> RefactorResult:
        """Drive the pure C# `RoslynRefactorTool` for an AST-level seam transform
        (TRANSFORM_CONTRACT §0). The tool reads the owning project source and
        returns proposed post-state source text + a seam descriptor as JSON; this
        method owns every filesystem write (snapshotted) and the prod-write guard.

        On `applicable=false` → applied=False, reason=tool reason, seam={} (no
        write, no build). On internal tool error (ok=false / nonzero / bad json)
        → applied=False with a diagnostic reason. Otherwise every returned path is
        re-checked through `_safe_prod_path`; if any escapes the owning subtree the
        WHOLE result is rejected and nothing is written. The `apply()` wrapper then
        runs `_build_owning_project()` + auto-revert (unchanged lifecycle).
        """
        dll = ROSLYN_REFACTOR_TOOL_DLL or _resolve_roslyn_tool_dll()
        if dll is None or not Path(dll).exists():
            return RefactorResult(
                transform,
                reason="roslyn_tool_missing: RoslynRefactorTool.dll not built; run "
                       "`dotnet build RoslynRefactorTool/RoslynRefactorTool.csproj -c Release`.",
            )

        method = (args.get("method") or self.method or "").strip()
        raw_file = (args.get("file") or self.target_file or "").strip()
        cand = Path(raw_file)
        target_abs = cand.resolve() if cand.is_absolute() else (self.repo_root / raw_file).resolve()

        argv = [
            DOTNET, str(dll),
            "--transform", transform,
            "--owning-dir", str(self.owning_csproj_dir),
            "--file", str(target_abs),
            "--line", str(self.target_line),
            "--method", method,
            "--receiver-type", self.receiver_type,
            "--containing-type", self.containing_type,
            "--kind", self.kind,
            "--interface-name", (args.get("interface_name") or "").strip(),
            "--wrapper-name", (args.get("wrapper_name") or "").strip(),
            "--param-name", (args.get("param_name") or "").strip(),
            "--json-out", "-",
        ]

        env = os.environ.copy()
        env["DOTNET_NOLOGO"] = "1"
        env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
        env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
        env["NUGET_PACKAGES"] = NUGET_CACHE
        try:
            proc = subprocess.run(
                argv, cwd=str(self.owning_csproj_dir), capture_output=True, text=True,
                timeout=self.build_timeout_s, env=env,
            )
        except subprocess.TimeoutExpired:
            return RefactorResult(transform, reason=f"roslyn_tool_timeout after {self.build_timeout_s}s")
        except OSError as e:
            return RefactorResult(transform, reason=f"roslyn_tool_exec_failed: {e}")

        try:
            payload = json.loads(proc.stdout)
        except ValueError:
            tail = (proc.stdout or proc.stderr or "")[-300:]
            return RefactorResult(
                transform,
                reason=f"roslyn_tool_bad_json (rc={proc.returncode}): {tail}",
            )

        if not payload.get("ok", False):
            return RefactorResult(
                transform,
                reason=payload.get("reason") or f"roslyn_tool_error (rc={proc.returncode})",
            )

        if not payload.get("applicable", False):
            # Clean §5 rejection: no write, no build.
            return RefactorResult(
                transform, applied=False,
                reason=payload.get("reason") or "not_applicable", seam={},
            )

        files = payload.get("files") or {}
        if not files:
            return RefactorResult(transform, reason="roslyn_tool_no_files: applicable but empty files{}")

        # Re-check EVERY returned path through the prod-write guard. Any escape
        # rejects the entire result, writing nothing.
        resolved: list[tuple[Path, str]] = []
        for raw, text in files.items():
            p = _safe_prod_path(self.repo_root, self.owning_csproj_dir, raw)
            if p is None:
                return RefactorResult(
                    transform, applied=False,
                    reason=f"prod-write guard rejected '{raw}' (outside owning subtree "
                           f"{self._rel(self.owning_csproj_dir)}); entire refactor rejected.",
                )
            resolved.append((p, text))

        for p, text in resolved:
            self._write(p, text)

        return RefactorResult(
            transform, applied=True,
            reason=payload.get("reason") or f"{transform} applied",
            files_changed=[self._rel(p) for p, _ in resolved],
            seam=payload.get("seam") or {},
        )

    # -- transform 2: wrapper_interface (Roslyn) --------------------------

    def _wrapper_interface(self, **args) -> RefactorResult:
        """Generate an adapter interface + thin forwarder for the seam member and
        inject the interface into the containing type via the constructor
        (defaulted to the real forwarder so behaviour is preserved), rewriting all
        same-receiver call sites. Implemented by `RoslynRefactorTool` over the
        semantic model — see TRANSFORM_CONTRACT §2. Args: interface_name,
        wrapper_name, param_name, method, file (all default-inferred tool-side).
        """
        return self._invoke_roslyn_tool("wrapper_interface", args)

    # -- transform 3: parameterize_dependency (Roslyn) --------------------

    def _parameterize_dependency(self, **args) -> RefactorResult:
        """Two-method overload-delegation: the original signature is preserved and
        delegates to a new overload that carries the mockable seam type as a
        trailing parameter (TRANSFORM_CONTRACT §3). Implemented by
        `RoslynRefactorTool`. Args: param_type defaults to the generated interface;
        param_name / interface_name / wrapper_name default-inferred tool-side.
        """
        return self._invoke_roslyn_tool("parameterize_dependency", args)
