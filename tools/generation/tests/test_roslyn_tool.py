"""Hermetic C# checks for `RoslynRefactorTool` (TRANSFORM_CONTRACT §9.1).

Turns Watney's manual "20/20 tool checks + 5 compiles" into a committed,
repeatable artifact that runs under `pytest tools/generation/tests/`.

For each §2/§3 case (ILogger extension / IServiceProvider generic / HttpClient
async) and BOTH transforms, this:
  - invokes the prebuilt `RoslynRefactorTool.dll` (PURE: JSON on stdout) against
    a committed `RoslynRefactorTool/tests/cases/<case>/Site.cs` owning project,
  - asserts `applicable=true` and that the emitted `seam` descriptor matches the
    contract (interface name, injection kind, injection_ref, member), and
  - compiles the rewritten `files{}` against the tool's OWN bundled ref
    assemblies (`bin/Release/net10.0/refs/`) — no NuGet restore, fully offline —
    to prove the output builds.

Plus one reject case per reproducible §5 row asserting `applicable=false` with
the EXACT reason token.

The Case-B (IServiceProvider generic) rows are now full positive cases. They
previously returned `unbound_receiver` for the framework generic extension
`GetRequiredService<T>` because the fast-path compilation omitted the SDK's
implicit global usings (so `System.IServiceProvider` did not bind). Watney fixed
this by re-supplying the default implicit usings in the analysis compilation —
see the `watney-isp-binding-fix` decision drop.

NO Azure / NO Foundry. The only subprocess is the local dotnet runtime running
the prebuilt tool dll and (for compile checks) `dotnet build` of a HintPath-only
project.
"""
from __future__ import annotations

import json
import os
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

import pytest

REPO_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO_ROOT))

from tools.evaluation.compile_only import DOTNET  # noqa: E402  (~/.dotnet/dotnet)
from tools.generation.apply_refactor import _resolve_roslyn_tool_dll  # noqa: E402

CASES = REPO_ROOT / "RoslynRefactorTool" / "tests" / "cases"
REFS = REPO_ROOT / "RoslynRefactorTool" / "bin" / "Release" / "net10.0" / "refs"
DLL = _resolve_roslyn_tool_dll()

SKIP_COMPILE = os.environ.get("BECK_SKIP_DOTNET_COMPILE") == "1"


def _dotnet_path() -> str | None:
    if Path(DOTNET).exists():
        return DOTNET
    return shutil.which("dotnet")


_DOTNET = _dotnet_path()

pytestmark = pytest.mark.skipif(
    DLL is None or _DOTNET is None,
    reason="RoslynRefactorTool.dll not built or dotnet runtime unavailable "
           "(build: dotnet build RoslynRefactorTool/RoslynRefactorTool.csproj -c Release).",
)


def _tool_env() -> dict:
    env = os.environ.copy()
    env["DOTNET_NOLOGO"] = "1"
    env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
    env["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1"
    return env


def run_tool(case: str, transform: str, line: int, method: str,
             receiver: str, containing: str, kind: str) -> dict:
    """Invoke the prebuilt tool on cases/<case>/Site.cs; return parsed JSON."""
    case_dir = CASES / case
    site = case_dir / "Site.cs"
    argv = [
        _DOTNET, str(DLL),
        "--transform", transform,
        "--owning-dir", str(case_dir),
        "--file", str(site.resolve()),
        "--line", str(line),
        "--method", method,
        "--receiver-type", receiver,
        "--containing-type", containing,
        "--kind", kind,
        "--json-out", "-",
    ]
    proc = subprocess.run(argv, capture_output=True, text=True, env=_tool_env())
    assert proc.stdout.strip(), (
        f"tool produced no stdout (rc={proc.returncode}); stderr:\n{proc.stderr}"
    )
    return json.loads(proc.stdout)


def compile_rewritten(files: dict[str, str]) -> tuple[bool, str]:
    """Compile the tool's post-state files{} against the bundled refs.

    Writes each entry by basename into a temp classlib referencing ONLY the
    tool's own `refs/*.dll` via HintPath (offline, no restore of those refs),
    then `dotnet build`. Returns (ok, build_output_tail).
    """
    refs = sorted(REFS.glob("*.dll"))
    with tempfile.TemporaryDirectory() as tmp:
        build = Path(tmp)
        for raw_path, text in files.items():
            (build / Path(raw_path).name).write_text(text, encoding="utf-8")
        ref_items = "\n".join(
            f'    <Reference Include="{r.stem}"><HintPath>{r}</HintPath></Reference>'
            for r in refs
        )
        (build / "Case.csproj").write_text(
            "<Project Sdk=\"Microsoft.NET.Sdk\">\n"
            "  <PropertyGroup>\n"
            "    <TargetFramework>net10.0</TargetFramework>\n"
            "    <OutputType>Library</OutputType>\n"
            "    <Nullable>enable</Nullable>\n"
            "    <ImplicitUsings>enable</ImplicitUsings>\n"
            "    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>\n"
            "    <GenerateDocumentationFile>false</GenerateDocumentationFile>\n"
            "  </PropertyGroup>\n"
            f"  <ItemGroup>\n{ref_items}\n  </ItemGroup>\n"
            "</Project>\n",
            encoding="utf-8",
        )
        proc = subprocess.run(
            [_DOTNET, "build", str(build / "Case.csproj"),
             "-v", "quiet", "--nologo", "-c", "Debug"],
            cwd=str(build), capture_output=True, text=True, env=_tool_env(),
        )
        out = (proc.stdout or "") + (proc.stderr or "")
        return proc.returncode == 0, out[-2000:]


# (case, transform, line, method, receiver, containing, kind,
#  expected interface simple name, expected injection, expected injection_ref)
POSITIVE_CASES = [
    ("ilogger", "wrapper_interface", 16, "LogInformation", "ILogger", "Worker",
     "Extension", "ILoggerWrapper", "ctor", "loggerWrapper"),
    ("ilogger", "parameterize_dependency", 16, "LogInformation", "ILogger", "Worker",
     "Extension", "ILoggerWrapper", "overload", "Run(string, ILoggerWrapper)"),
    ("httpclient", "wrapper_interface", 17, "GetAsync", "HttpClient", "ApiClient",
     "NonVirtual", "IHttpClientWrapper", "ctor", "httpClientWrapper"),
    ("httpclient", "parameterize_dependency", 17, "GetAsync", "HttpClient", "ApiClient",
     "NonVirtual", "IHttpClientWrapper", "overload", "FetchAsync(string, IHttpClientWrapper)"),
    # Case B (TRANSFORM_CONTRACT §2.2): framework generic extension
    # IServiceProvider.GetRequiredService<T>(). Previously returned
    # unbound_receiver (implicit-usings gap; fixed by Watney — see
    # watney-isp-binding-fix decision drop).
    ("isp", "wrapper_interface", 18, "GetRequiredService", "IServiceProvider", "Handler",
     "Extension", "IServiceProviderWrapper", "ctor", "serviceProviderWrapper"),
    ("isp", "parameterize_dependency", 18, "GetRequiredService", "IServiceProvider", "Handler",
     "Extension", "IServiceProviderWrapper", "overload", "Dispatch(IServiceProviderWrapper)"),
    # BUG #1 fix (CS1737): parameterize_dependency on an enclosing method whose
    # signature already ENDS in an optional / `params` parameter. The injected
    # (required) dependency must be inserted BEFORE the trailing optional/params
    # group so the overload signature stays legal C#.
    ("param_trailing_optional", "parameterize_dependency", 16, "LogInformation",
     "ILogger", "TrailingWorker", "Extension", "ILoggerWrapper", "overload",
     "Run(string, ILoggerWrapper, int)"),
    ("param_trailing_params", "parameterize_dependency", 16, "LogInformation",
     "ILogger", "ParamsWorker", "Extension", "ILoggerWrapper", "overload",
     "Emit(string, ILoggerWrapper, object[])"),
    # BUG #2 fix (CS1503): wrapper_interface where the containing type invokes
    # TWO distinct ILogger extension overloads on the same receiver — the target
    # LogError(string, …) and the sibling LogError(Exception, string, …). Only
    # the target overload's sites are redirected through the seam; the sibling
    # stays on the raw receiver, so the rewrite still compiles.
    ("wrapper_heterogeneous", "wrapper_interface", 17, "LogError", "ILogger",
     "MultiLogWorker", "Extension", "ILoggerWrapper", "ctor", "loggerWrapper"),
    # wrapper_interface into a file under `#nullable disable`: the injected
    # optional ctor param must NOT carry a `?` annotation (CS8632) — the real-repo
    # shape behind jellyfin:0006 (ApplicationHost.cs begins with #nullable disable).
    ("wrapper_nullable_disable", "wrapper_interface", 17, "LogInformation",
     "ILogger", "NdWorker", "Extension", "ILoggerWrapper", "ctor", "loggerWrapper"),
    # wrapper_interface into a class whose ctor already carries an XML doc comment
    # with `<param>` tags: the injected param must get its OWN `<param>` tag so a
    # <GenerateDocumentationFile> repo does not trip CS1573 (analyzer hardening).
    ("wrapper_doc_ctor", "wrapper_interface", 18, "LogInformation",
     "ILogger", "DocWorker", "Extension", "ILoggerWrapper", "ctor", "loggerWrapper"),
]

@pytest.mark.parametrize(
    "case,transform,line,method,receiver,containing,kind,iface,injection,injref",
    POSITIVE_CASES,
    ids=[f"{c}-{t}" for c, t, *_ in POSITIVE_CASES],
)
def test_positive_case_applies_with_correct_seam(
    case, transform, line, method, receiver, containing, kind,
    iface, injection, injref,
):
    payload = run_tool(case, transform, line, method, receiver, containing, kind)
    assert payload["ok"] is True, payload
    assert payload["applicable"] is True, payload.get("reason")
    seam = payload["seam"]
    assert seam["kind"] == transform
    assert seam["interface"].split(".")[-1] == iface
    assert seam["member"] == method
    assert seam["injection"] == injection
    assert seam["injection_ref"] == injref
    assert seam["containing_type"].split(".")[-1] == containing
    assert ":" in seam["call_site"]

    files = payload["files"]
    # The generated mockable interface file must be present.
    assert any(Path(p).name == f"{iface}.cs" for p in files), list(files)
    # The original site file must be rewritten too.
    assert any(Path(p).name == "Site.cs" for p in files), list(files)

    if SKIP_COMPILE:
        pytest.skip("BECK_SKIP_DOTNET_COMPILE=1")
    ok, out = compile_rewritten(files)
    assert ok, f"rewritten output did not compile:\n{out}"


def test_heterogeneous_receiver_redirects_only_target_overload():
    """BUG #2: with two distinct ILogger extension overloads on one receiver,
    the target overload's site is redirected to the wrapper field while the
    sibling overload stays on the raw receiver (so the rewrite still binds)."""
    payload = run_tool("wrapper_heterogeneous", "wrapper_interface", 17, "LogError",
                        "ILogger", "MultiLogWorker", "Extension")
    assert payload["applicable"] is True, payload.get("reason")
    site = next(t for p, t in payload["files"].items() if Path(p).name == "Site.cs")
    # Target overload LogError(string, …) → redirected through the seam field.
    assert "_loggerWrapper.LogError(\"starting {Job}\", job)" in site, site
    # Sibling overload LogError(Exception, string, …) → left on the raw receiver.
    assert "_logger.LogError(ex, \"failed {Job}\", job)" in site, site


def _wrapper_files(case, line, method, receiver, containing):
    payload = run_tool(case, "wrapper_interface", line, method, receiver,
                       containing, "Extension")
    assert payload["applicable"] is True, payload.get("reason")
    files = payload["files"]
    site = next(t for p, t in files.items() if Path(p).name == "Site.cs")
    iface_name = payload["seam"]["interface"].split(".")[-1]
    iface = next(t for p, t in files.items() if Path(p).name == f"{iface_name}.cs")
    return site, iface


def test_generated_wrapper_file_has_autogen_header():
    """The generated wrapper/interface FILE must open with `// <auto-generated/>`
    so file-scoped analyzers (StyleCop SA*, IDE0xxx) skip it in strict repos."""
    _site, iface = _wrapper_files("ilogger", 16, "LogInformation", "ILogger", "Worker")
    assert iface.lstrip().startswith("// <auto-generated/>"), iface[:200]
    # The nullable context is pinned in the generated file so it builds the same
    # regardless of the consuming project's <Nullable> default.
    assert "#nullable enable" in iface, iface[:200]


def test_nullable_disable_site_injects_param_without_annotation():
    """Under `#nullable disable`, the injected optional ctor param must NOT carry
    a `?` annotation (else CS8632 in a nullable-disabled context)."""
    site, _iface = _wrapper_files("wrapper_nullable_disable", 17, "LogInformation",
                                  "ILogger", "NdWorker")
    assert "ILoggerWrapper loggerWrapper = null" in site, site
    assert "ILoggerWrapper? loggerWrapper" not in site, site


def test_doc_commented_ctor_gets_param_tag_for_injected_param():
    """When the enclosing ctor already documents its params, the injected param
    must receive its OWN `<param>` tag so a <GenerateDocumentationFile> repo does
    not trip CS1573 (missing XML comment for parameter)."""
    site, _iface = _wrapper_files("wrapper_doc_ctor", 18, "LogInformation",
                                  "ILogger", "DocWorker")
    # original param doc preserved …
    assert "<param name=\"logger\">" in site, site
    # … and the injected seam param now documented too.
    assert "<param name=\"loggerWrapper\">" in site, site


def test_candidate_fallback_picks_matching_arity_overload():
    """REGRESSION (orleans:0116): when overload resolution does NOT fully bind
    (symInfo.Symbol is null — e.g. the net9-Abstractions vs net10-runtime
    reference split prevents the compiler from picking one overload), the tool
    falls back to CandidateSymbols. It must select the candidate whose arity +
    argument types match the actual call site, NOT an arbitrary first candidate
    (the old blind `FirstOrDefault()`).

    The `overload_candidate_arity` fixture declares the LONG
    `Note(int, Exception, string, params object[])` overload FIRST and the SHORT
    `Note(string, params object[])` second, then makes a string-first call with a
    deliberately unbound trailing argument to force the candidate-fallback path.
    The seam must reconstruct the SHORT overload."""
    payload = run_tool("overload_candidate_arity", "wrapper_interface", 36, "Note",
                       "Diag", "OverloadWorker", "NonVirtual")
    assert payload["applicable"] is True, payload.get("reason")
    seam = payload["seam"]
    assert seam["member"] == "Note"
    sig = seam["member_signature"]
    # SHORT overload picked by arity + argument type …
    assert sig == "void Note(string, params object[])", sig
    # … NOT the first-declared LONG `Note(int, Exception, string, …)` overload.
    assert "Exception" not in sig and "int" not in sig, sig


@pytest.mark.parametrize(
    "transform,expected_injection_ref",
    [
        ("wrapper_interface", None),
        ("parameterize_dependency", "Run(string, ILoggerWrapper)"),
    ],
)
def test_locator_picks_correct_duplicate_invocation_under_line_drift(
    transform, expected_injection_ref,
):
    """Regression: when two same-name invocations exist in one type and the
    recorded target line drifts to the enclosing method signature, locator must
    still resolve the intended invocation deterministically (Run, not Audit)."""
    payload = run_tool(
        "locator_line_drift_duplicate",
        transform,
        14,  # drifted to `Run` signature line (actual call is line 17)
        "LogInformation",
        "ILogger",
        "DriftWorker",
        "Extension",
    )
    assert payload["ok"] is True, payload
    assert payload["applicable"] is True, payload.get("reason")

    seam = payload["seam"]
    assert seam["member"] == "LogInformation"
    assert seam["call_site"].endswith("Site.cs:17"), seam["call_site"]

    if expected_injection_ref is not None:
        assert seam["injection_ref"] == expected_injection_ref, seam["injection_ref"]


# ======================================================================
# unbound_receiver reference-coverage fixes (2026-06-15) — regression guards.
# ======================================================================
#
# Two distinct binding-coverage sub-causes were behind the bulk of the
# `unbound_receiver` rejects on real targets, both fixed by extending the
# analysis compilation's reference set (NOT by relaxing applicability):
#   A. the receiver's type is reached through a Microsoft.AspNetCore.App
#      shared-framework type (e.g. HttpContext) absent from the bundled refs;
#   B. the receiver's type is declared in a SIBLING project of the same repo,
#      resolved via the owning project's built bin/ closure.

# (transform, expected injection) for the AspNetCore.App framework-receiver case.
ASPNETCORE_FRAMEWORK_CASES = [
    ("wrapper_interface", "ctor"),
    ("parameterize_dependency", "overload"),
]


@pytest.mark.parametrize(
    "transform,injection", ASPNETCORE_FRAMEWORK_CASES,
    ids=[c[0] for c in ASPNETCORE_FRAMEWORK_CASES],
)
def test_aspnetcore_framework_receiver_binds(transform, injection):
    """REGRESSION (sub-cause A): the seam receiver `_ctx.RequestServices` is
    reached through `HttpContext`, a type in the Microsoft.AspNetCore.App shared
    framework — NOT in the NETCore.App runtime nor the bundled `refs/`. Before the
    AspNetCore.App reference tier was added, HttpContext bound to an ErrorType and
    GetRequiredService<T> could not bind (spurious `unbound_receiver`). With the
    tier loaded the receiver binds and the seam applies. Mirrors aspnetcore:0020.

    Compile is intentionally NOT asserted here: the rewritten Site.cs references
    HttpContext, which needs the AspNetCore.App refs the hermetic
    `compile_rewritten` classlib (bundled `refs/` only) does not carry. Binding +
    seam correctness is the regression surface; real-repo compilation is covered
    by the build-verified sweep (unbound_recheck_*.csv)."""
    payload = run_tool("aspnetcore_framework_receiver", transform, 29,
                       "GetRequiredService", "IServiceProvider",
                       "FrameworkReceiverWorker", "Extension")
    assert payload["ok"] is True, payload
    assert payload["applicable"] is True, payload.get("reason")
    seam = payload["seam"]
    assert seam["kind"] == transform
    assert seam["interface"].split(".")[-1] == "IServiceProviderWrapper"
    assert seam["member"] == "GetRequiredService"
    assert seam["injection"] == injection


def test_bin_closure_resolves_cross_assembly_receiver(tmp_path):
    """REGRESSION (sub-cause B): the seam receiver's type is declared in a SIBLING
    assembly of the same repo (e.g. OpenRA.Game's HttpClientFactory consumed from
    OpenRA.Mods.Common, bitwarden's base repository in Core). The owning-dir source
    compilation cannot see it, so the receiver bound to an ErrorType →
    `unbound_receiver`. BuildCompilation now augments the reference set with the
    owning project's built `bin/**/*.dll` closure, so once the project is built the
    sibling type resolves and the seam applies (the build-verified gate builds the
    owning project before locating).

    This builds a one-type sibling assembly, drops it in the owning project's bin/,
    and asserts the tool binds a receiver typed ONLY from that assembly."""
    if SKIP_COMPILE:
        pytest.skip("BECK_SKIP_DOTNET_COMPILE=1 (needs dotnet to build the sibling assembly)")

    # 1. Build the sibling assembly that declares the receiver's type.
    lib = tmp_path / "sibling"
    lib.mkdir()
    (lib / "Sibling.cs").write_text(
        "namespace Sibling;\n"
        "public sealed class SiblingClient\n"
        "{\n"
        "    public string Fetch(string url) => url;\n"
        "}\n",
        encoding="utf-8",
    )
    (lib / "Sibling.csproj").write_text(
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n"
        "  <PropertyGroup><TargetFramework>net10.0</TargetFramework>"
        "<OutputType>Library</OutputType></PropertyGroup>\n"
        "</Project>\n",
        encoding="utf-8",
    )
    proc = subprocess.run(
        [_DOTNET, "build", str(lib / "Sibling.csproj"), "-v", "quiet", "--nologo",
         "-c", "Debug", "-o", str(lib / "out")],
        capture_output=True, text=True, env=_tool_env(),
    )
    assert proc.returncode == 0, (proc.stdout + proc.stderr)[-2000:]

    # 2. Owning project whose Site.cs consumes the sibling type; drop the sibling
    #    DLL into the owning project's bin/ closure.
    owning = tmp_path / "owning"
    bin_dir = owning / "bin" / "Debug" / "net10.0"
    bin_dir.mkdir(parents=True)
    shutil.copy(lib / "out" / "Sibling.dll", bin_dir / "Sibling.dll")
    site = owning / "Site.cs"
    site.write_text(
        "namespace Demo;\n"
        "public sealed class Consumer\n"
        "{\n"
        "    private readonly Sibling.SiblingClient _client;\n"
        "    public Consumer(Sibling.SiblingClient client) { _client = client; }\n"
        "    public string Go() => _client.Fetch(\"x\");\n"
        "}\n",
        encoding="utf-8",
    )

    # 3. The receiver `_client` is typed SiblingClient, reachable ONLY via the
    #    bin closure. Without it the call is unbound_receiver.
    argv = [
        _DOTNET, str(DLL),
        "--transform", "wrapper_interface",
        "--owning-dir", str(owning),
        "--file", str(site.resolve()),
        "--line", "6",
        "--method", "Fetch",
        "--receiver-type", "SiblingClient",
        "--containing-type", "Consumer",
        "--kind", "NonVirtual",
    ]
    proc = subprocess.run(argv, capture_output=True, text=True, env=_tool_env())
    assert proc.stdout.strip(), f"tool produced no stdout; stderr:\n{proc.stderr}"
    payload = json.loads(proc.stdout)
    assert payload["applicable"] is True, payload.get("reason")
    assert payload["seam"]["member"] == "Fetch"
    assert payload["seam"]["interface"].split(".")[-1] == "ISiblingClientWrapper"


# (case, transform, line, method, receiver, containing, kind, expected reason)
REJECT_CASES = [
    ("reject_struct", "wrapper_interface", 16, "LogInformation", "ILogger",
     "SWorker", "Extension", "struct_type"),
    ("reject_record", "wrapper_interface", 16, "LogInformation", "ILogger",
     "RWorker", "Extension", "record_type"),
    ("reject_multi_ctor", "wrapper_interface", 22, "LogInformation", "ILogger",
     "MWorker", "Extension", "multiple_ctors"),
    ("reject_primary_ctor", "wrapper_interface", 9, "LogInformation", "ILogger",
     "PWorker", "Extension", "primary_ctor"),
    ("reject_static", "wrapper_interface", 9, "LogInformation", "ILogger",
     "SUtil", "Extension", "no_receiver_source"),
    ("reject_local_receiver", "parameterize_dependency", 9, "LogInformation",
     "ILogger", "LWorker", "Extension", "receiver_not_in_method_scope"),
    # parameterize: the receiver is a LAMBDA parameter (sp => sp.GetRequiredService<T>()),
    # not resolvable in the delegator's scope → reject cleanly (the real-repo
    # shape behind efcore:0007 / semantic-kernel:0034).
    ("reject_lambda_receiver", "parameterize_dependency", 13, "GetRequiredService",
     "IServiceProvider", "Registrar", "Extension", "receiver_not_in_method_scope"),
    # site not found: a method name that does not appear at the site.
    ("ilogger", "wrapper_interface", 16, "NoSuchMethod", "ILogger", "Worker",
     "Extension", "site_not_found"),
]


@pytest.mark.parametrize(
    "case,transform,line,method,receiver,containing,kind,reason",
    REJECT_CASES,
    ids=[r[-1] for r in REJECT_CASES],
)
def test_reject_case_emits_exact_reason_token(
    case, transform, line, method, receiver, containing, kind, reason,
):
    payload = run_tool(case, transform, line, method, receiver, containing, kind)
    assert payload["ok"] is True, payload
    assert payload["applicable"] is False, (
        f"expected rejection for {case}, got applicable=true: {payload.get('reason')}"
    )
    assert payload["reason"] == reason, (
        f"expected reason token {reason!r}, got {payload['reason']!r}"
    )
    assert payload["files"] == {}, "rejected case must write no files"


# ======================================================================
# make_virtual (TRANSFORM_CONTRACT §1) — Roslyn-based, subclass-and-override.
# ======================================================================
#
# make_virtual targets a method DECLARATION (not a call-site rewrite). The seam
# is subclass-and-override, so the emitted `seam` is intentionally {} (no
# wrapper/param descriptor) and via_seam attribution stays None.

def test_make_virtual_applies_and_preserves_trivia():
    """Positive: a non-virtual instance method gains `virtual`, with its leading
    doc-comment + `<param>` tags and 4-space indentation preserved, and the
    rewritten file still compiles. seam stays {}."""
    payload = run_tool("make_virtual_ok", "make_virtual", 13, "Add",
                       "Calculator", "Worker", "NonVirtual")
    assert payload["ok"] is True, payload
    assert payload["applicable"] is True, payload.get("reason")
    # make_virtual carries NO seam descriptor (subclass-and-override seam).
    assert payload["seam"] == {}, payload["seam"]

    files = payload["files"]
    site = next((t for p, t in files.items() if Path(p).name == "Site.cs"), None)
    assert site is not None, list(files)
    # `virtual` added on the declaration …
    assert "public virtual int Add(int a, int b) => a + b;" in site, site
    # … leading doc-comment trivia preserved …
    assert "/// <summary>Adds two numbers together.</summary>" in site, site
    assert '<param name="a">First addend.</param>' in site, site
    # … and the 4-space indentation intact (only the modifier list changed).
    assert "\n    public virtual int Add" in site, repr(site[:400])

    if SKIP_COMPILE:
        pytest.skip("BECK_SKIP_DOTNET_COMPILE=1")
    ok, out = compile_rewritten(files)
    assert ok, f"rewritten make_virtual output did not compile:\n{out}"


# (case, line, method, expected reason token)
MAKE_VIRTUAL_REJECT_CASES = [
    ("make_virtual_already_virtual", 10, "Ping", "already_virtual"),
    ("make_virtual_static", 10, "Log", "static_method"),
    ("make_virtual_sealed_class", 10, "Ping", "sealed_class"),
    ("make_virtual_abstract", 10, "Ping", "already_abstract"),
    ("make_virtual_struct", 10, "Ping", "struct_type"),
    ("make_virtual_record", 10, "Ping", "record_type"),
    # framework-declared (no in-repo declaration) → suggest wrapper_interface.
    ("make_virtual_framework", 7, "Append", "not_in_owning_project"),
    # `private virtual` is illegal C# (CS0621) → reject cleanly.
    ("make_virtual_private", 7, "Secret", "private_member"),
]


@pytest.mark.parametrize(
    "case,line,method,reason",
    MAKE_VIRTUAL_REJECT_CASES,
    ids=[r[-1] for r in MAKE_VIRTUAL_REJECT_CASES],
)
def test_make_virtual_reject_emits_reason_token(case, line, method, reason):
    payload = run_tool(case, "make_virtual", line, method,
                       "Recv", "Worker", "NonVirtual")
    assert payload["ok"] is True, payload
    assert payload["applicable"] is False, (
        f"expected rejection for {case}, got applicable=true: {payload.get('reason')}"
    )
    # Reason may carry a human suffix after the token (e.g. framework hint).
    assert payload["reason"].split(":", 1)[0] == reason, (
        f"expected reason token {reason!r}, got {payload['reason']!r}"
    )
    assert payload["files"] == {}, "rejected case must write no files"


if __name__ == "__main__":
    raise SystemExit(pytest.main([__file__, "-q"]))
