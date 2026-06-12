# Phase 4 — `apply_refactor` Transform Contract (`wrapper_interface` + `parameterize_dependency`)

> **Authority:** This is a prescriptive build contract authored by Lewis for the
> builder (Watney). It is not a suggestion. Watney implements against it
> verbatim; deviations require a decision drop and Lewis sign-off.
>
> **Scope:** the two currently-STUBBED phase-4 transforms,
> [`_wrapper_interface`](../../tools/generation/apply_refactor.py) and
> [`_parameterize_dependency`](../../tools/generation/apply_refactor.py).
> `make_virtual` is **unchanged** — it stays in the menu (`TRANSFORMS`), keeps
> its pure-Python regex implementation, and remains covered by
> [test_refactor_smoke.py](../../tools/generation/tests/test_refactor_smoke.py).
> This contract adds no obligations to `make_virtual`.
>
> **Decision of record (Jasper, 2026-06-11):** both stubbed transforms are
> implemented as a **fully general Roslyn rewriter** — NOT scoped to the known
> receiver families (`ILogger`/`IServiceProvider`/`IConfiguration`/`HttpClient`).
> The known families are the *validation set*, not the *capability bound*.

---

## 0. Architecture (binding)

A new C# tool **`RoslynRefactorTool`** performs the AST rewrite; Python
([apply_refactor.py](../../tools/generation/apply_refactor.py)) owns all
filesystem mutation, the snapshot/restore lifecycle, and the
behavior-preservation build. The split is non-negotiable: **the C# tool is pure
(reads source, returns proposed source text); it never writes the repo.**

```
RefactorEngine.apply("wrapper_interface", **args)        [Python]
  └─ build subprocess argv (target row + resolved defaults)
       └─ dotnet RoslynRefactorTool.dll  --transform … --json-out  [C#, PURE]
            └─ binds invocation via semantic model, rewrites AST,
               returns JSON { ok, applicable, reason, files{}, seam{} }
  └─ if !applicable  → RefactorResult(applied=False, reason=…)   (no write)
  └─ else            → engine._write(path, text) for each files{}  (snapshotted)
       └─ _build_owning_project()  → auto-revert on failure (existing path)
```

### 0.1 `RoslynRefactorTool` project (mirror `Mode1Analyzer`)

Create `RoslynRefactorTool/RoslynRefactorTool.csproj` mirroring
[Mode1Analyzer.csproj](../../Mode1Analyzer/Mode1Analyzer.csproj) **exactly** for
the build/reference pattern:

- `net10.0`, `OutputType=Exe`, `ImplicitUsings=enable`, `Nullable=enable`.
- `Microsoft.CodeAnalysis.CSharp` **4.14.0**.
- The same `PrivateAssets="all" GeneratePathProperty="true"` ref-pack
  references (`Microsoft.Extensions.Logging[.Abstractions]`,
  `Configuration[.Abstractions/.Binder]`, `DependencyInjection.Abstractions`,
  all `9.0.0`) and the same `CopyRefAssemblies` `AfterTargets="Build"` target
  copying `lib/net9.0/*.dll` into `$(OutDir)refs/`.
- Reference loading mirrors `Program.LoadReferences()`: runtime ref pack from
  `typeof(object).Assembly.Location` dir **plus** `AppContext.BaseDirectory/refs`.
  This is the **fast path** — no per-project `dotnet restore`. Symbols that need
  third-party references bind as `ErrorType`; for the target families the
  standard refs are sufficient (identical assumption to Mode1Analyzer).

### 0.2 Compilation unit

To bind the target invocation's `IMethodSymbol` (required for generic-method and
extension-method signature reconstruction), the tool compiles the owning project
the same way Mode1Analyzer compiles a repo: parse every non-`obj`/`bin` `*.cs`
under the **owning `.csproj` directory** into one `CSharpCompilation` with the
loaded references, `LanguageVersion.Latest`. Bind only the single target
invocation (located by `file` + `line`). Parse errors elsewhere are tolerated
(skip the tree) exactly as in `AnalyzeRepo`.

### 0.3 Subprocess argv (Python → tool)

```
dotnet <RoslynRefactorTool.dll>
  --transform        <wrapper_interface|parameterize_dependency>
  --owning-dir       <abs path to owning .csproj directory>
  --file             <abs path to target file>
  --line             <1-based target line from CSV>
  --method           <CSV method>
  --receiver-type    <CSV receiver_type>
  --containing-type  <CSV containing_type>
  --kind             <Extension|NonVirtual>
  --interface-name   <resolved default or model override>
  --wrapper-name     <resolved default or model override>
  --param-name       <resolved default or model override>
  --json-out         -            # emit JSON to stdout
```

The tool MUST emit **only** JSON on stdout (diagnostics to stderr, like
Mode1Analyzer) so Python can `json.loads(stdout)` unconditionally.

### 0.4 Tool JSON response schema (tool → Python)

```jsonc
{
  "ok": true,                 // false only on internal tool error (not "not applicable")
  "applicable": true,         // false ⇒ Python returns applied=False, NO write, NO build
  "reason": "string",         // machine-ish + human; ALWAYS populated
  "files": {                  // post-state full source text, repo-relative paths
    "src/Foo/Foo.cs":        "…rewritten…",
    "src/Foo/ILoggerWrapper.cs": "…generated…"
  },
  "seam": {                   // the seam descriptor — see §4
    "kind":            "wrapper_interface",
    "interface":       "Foo.ILoggerWrapper",      // FQN of generated mockable type
    "wrapper":         "Foo.LoggerWrapper",       // FQN of concrete forwarder
    "member":          "LogInformation",          // interface member exercised at the site
    "member_signature":"void LogInformation(string)",
    "injection":       "ctor",                    // ctor | overload
    "injection_ref":   "loggerWrapper",           // ctor param name OR overload signature
    "containing_type": "Foo.Worker",
    "call_site":       "src/Foo/Foo.cs:42"        // file:line of the rewritten invocation
  }
}
```

`files` is **post-state** (full text, not a diff), matching how Python's
`_write` + snapshot/restore already operate. Python writes every entry through
`_write` (which snapshots originals and creates parents), runs the existing
`_build_owning_project()`, and auto-reverts via `restore_all()` on build
failure — **no changes to that lifecycle are permitted.**

---

## 1. Input arguments (JSON schema + defaults)

Both transforms are reachable through the existing
[`parse_refactor_args`](../../tools/generation/strategies/agentic_loop_refactor.py)
(`{"transform": …, …}` / `transform=…, k=v` / bare). **Every argument has a
default derived from the target row**, so the model MAY call with the transform
name alone:

```
<tool>apply_refactor(wrapper_interface)</tool>
<tool>apply_refactor(parameterize_dependency)</tool>
```

### 1.1 Shared arguments

| Arg | Type | Default (inferred) | Meaning |
|---|---|---|---|
| `transform` | str | — (required) | `wrapper_interface` \| `parameterize_dependency` |
| `interface_name` | str | `I{Recv}Wrapper` | Name of the generated mockable interface |
| `wrapper_name` | str | `{Recv}Wrapper` | Name of the concrete forwarder class |
| `param_name` | str | camelCase(`wrapper_name`) | Injected ctor param / field base name |
| `method` | str | CSV `method` | Target method to wrap (the seam member) |
| `file` | str | CSV `file` | Declaring file of the call site |

**`{Recv}` derivation (deterministic, tool-side):** take the simple name of
`receiver_type`; if it matches `^I[A-Z]` (interface convention) strip the single
leading `I`. Examples — `ILogger → Logger`, `IServiceProvider → ServiceProvider`,
`IConfiguration → Configuration`, `HttpClient → HttpClient`. Then:

- `interface_name = "I" + {Recv} + "Wrapper"` → `ILoggerWrapper`, `IHttpClientWrapper`.
- `wrapper_name   = {Recv} + "Wrapper"`       → `LoggerWrapper`, `HttpClientWrapper`.
- `param_name     = camelCase(wrapper_name)`  → `loggerWrapper`, `httpClientWrapper`;
  field is `_" + param_name` → `_loggerWrapper`.

If the inferred name collides with an existing type in the owning project, the
tool appends a numeric suffix (`ILoggerWrapper2`) rather than failing.

### 1.2 `parameterize_dependency` extra arguments

| Arg | Type | Default (inferred) | Meaning |
|---|---|---|---|
| `param_type` | str | `interface_name` | Type of the new method parameter (the mockable seam type) |
| `param_name` | str | camelCase(`param_type`) | Name of the new method parameter |

> Note: for **all** phase-4 target families the parameterized dependency type is
> the **generated wrapper interface**, not the raw receiver — see §7. The raw
> receiver is not directly mockable (extension method statically resolved; or a
> non-virtual framework method), so passing it as a parameter would not create a
> seam. `param_type` therefore defaults to `interface_name`.

### 1.3 Argument validation

The model cannot supply a write path outside the owning subtree: generated-file
paths are computed tool-side under the owning dir, and Python re-checks every
returned `files` key through the existing
[`_safe_prod_path`](../../tools/generation/apply_refactor.py) guard. Any returned
path that escapes the owning subtree ⇒ Python rejects the **entire** result
(applied=False, reason cites the prod-write guard), writing nothing.

---

## 2. Output transformation — `wrapper_interface`

**Intent:** generate an adapter interface (the mockable type) + a thin concrete
forwarder, then inject the interface into the **containing type** via the
constructor (defaulted to the real forwarder so existing call sites and runtime
behavior are preserved), and rewrite the call site to go through the field.

### 2.1 Case A — Extension on interface receiver: `ILogger.LogInformation`

CSV row shape: `kind=Extension`, `receiver_type=ILogger`,
`containing_type=Worker`, `method=LogInformation`.

**BEFORE**
```csharp
namespace Acme.Workers;

public sealed class Worker
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    public void Run(string job)
    {
        _logger.LogInformation("starting {Job}", job);   // ← target site, line N
    }
}
```

**AFTER** — generated file `ILoggerWrapper.cs` (new, in owning subtree):
```csharp
namespace Acme.Workers;

// Generated by RoslynRefactorTool (phase-4 wrapper_interface seam). Adapter over
// the statically-resolved extension method so it can be mocked.
public interface ILoggerWrapper
{
    void LogInformation(string message, params object?[] args);
}

public sealed class LoggerWrapper : ILoggerWrapper
{
    private readonly ILogger _inner;
    public LoggerWrapper(ILogger inner) => _inner = inner;
    public void LogInformation(string message, params object?[] args)
        => _inner.LogInformation(message, args);   // forwards to the real extension
}
```

**AFTER** — rewritten `Worker`:
```csharp
public sealed class Worker
{
    private readonly ILogger<Worker> _logger;
    private readonly ILoggerWrapper _loggerWrapper;          // ← field added

    public Worker(ILogger<Worker> logger, ILoggerWrapper? loggerWrapper = null)  // ← param added
    {
        _logger = logger;
        _loggerWrapper = loggerWrapper ?? new LoggerWrapper(_logger);  // ← appended LAST
    }

    public void Run(string job)
    {
        _loggerWrapper.LogInformation("starting {Job}", job);  // ← call site rewritten
    }
}
```

Key rules embodied here, all mandatory:
- The wrapper interface declares the **instance** form of the extension method:
  the `this` parameter is dropped, the remaining parameters/return are
  reconstructed from the bound `IMethodSymbol` (so `params`, optional args,
  ref/out, nullable annotations are preserved — line-level text matching is
  insufficient, which is why the semantic model is required).
- The constructor gains a **trailing optional** parameter
  `{interface_name}? {param_name} = null`. Trailing + optional ⇒ every existing
  caller of `new Worker(logger)` still compiles and binds.
- The field assignment is **appended as the last statement** of the constructor
  body, after all existing assignments, so the real receiver (`_logger`) is
  already set when `new LoggerWrapper(_logger)` runs. Default path constructs the
  real forwarder ⇒ runtime behavior identical.
- **Every** invocation of the target member on the same receiver field within the
  containing type is rewritten to the injected field (not only the line-N site),
  so production is consistent and the seam is the sole path.

### 2.2 Case B — Extension, generic: `IServiceProvider.GetRequiredService<T>()`

CSV: `kind=Extension`, `receiver_type=IServiceProvider`, `method=GetRequiredService`.

**BEFORE**
```csharp
public sealed class Handler
{
    private readonly IServiceProvider _sp;
    public Handler(IServiceProvider sp) => _sp = sp;

    public void Dispatch()
    {
        var svc = _sp.GetRequiredService<IMessageBus>();  // ← target site
        svc.Publish();
    }
}
```

**AFTER** — generated `IServiceProviderWrapper.cs`:
```csharp
public interface IServiceProviderWrapper
{
    T GetRequiredService<T>() where T : notnull;   // ← generic member; constraints from symbol
}

public sealed class ServiceProviderWrapper : IServiceProviderWrapper
{
    private readonly IServiceProvider _inner;
    public ServiceProviderWrapper(IServiceProvider inner) => _inner = inner;
    public T GetRequiredService<T>() where T : notnull
        => _inner.GetRequiredService<T>();
}
```

**AFTER** — rewritten `Handler` ctor + site (same pattern as §2.1):
```csharp
    public Handler(IServiceProvider sp, IServiceProviderWrapper? serviceProviderWrapper = null)
    {
        _sp = sp;
        _serviceProviderWrapper = serviceProviderWrapper ?? new ServiceProviderWrapper(_sp);
    }
    // …
        var svc = _serviceProviderWrapper.GetRequiredService<IMessageBus>();  // ← type args preserved
```

Generic rule: the wrapper interface method carries the **type parameters and
constraints** of the bound symbol; the call site preserves the explicit type
arguments (`<IMessageBus>`). Moq/NSubstitute can mock generic interface methods,
so the seam is real.

### 2.3 Case C — NonVirtual on framework receiver: `HttpClient.GetAsync`

CSV: `kind=NonVirtual`, `receiver_type=HttpClient`, `method=GetAsync`. `HttpClient`
is a framework type → `make_virtual` is impossible (not declared in-repo); the
wrapper is the correct seam.

**BEFORE**
```csharp
public sealed class ApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    public async Task<string> FetchAsync(string url)
    {
        HttpResponseMessage resp = await _http.GetAsync(url);  // ← target site
        return await resp.Content.ReadAsStringAsync();
    }
}
```

**AFTER** — generated `IHttpClientWrapper.cs`:
```csharp
public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string? requestUri);
}

public sealed class HttpClientWrapper : IHttpClientWrapper
{
    private readonly HttpClient _inner;
    public HttpClientWrapper(HttpClient inner) => _inner = inner;
    public Task<HttpResponseMessage> GetAsync(string? requestUri)
        => _inner.GetAsync(requestUri);
}
```

**AFTER** — rewritten `ApiClient` (note: the wrapper method is **not** declared
`async`; it forwards the `Task` directly, which is behavior-identical and avoids a
redundant state machine):
```csharp
    public ApiClient(HttpClient http, IHttpClientWrapper? httpClientWrapper = null)
    {
        _http = http;
        _httpClientWrapper = httpClientWrapper ?? new HttpClientWrapper(_http);
    }
    // …
        HttpResponseMessage resp = await _httpClientWrapper.GetAsync(url);  // ← awaits the seam
```

Async rule: reconstruct the **return type** (`Task<HttpResponseMessage>`) from the
symbol; the forwarder is a non-`async` expression-bodied method returning the
inner `Task`. The original `await` at the call site is preserved.

---

## 3. Output transformation — `parameterize_dependency`

**Intent:** inject the same generated wrapper interface, but **per-call at the
method level** rather than as a constructor field. The public API is preserved by
the **two-method overload-delegation** pattern (NOT an optional parameter — C#
forbids a `new Wrapper(...)` default value, which is a non-constant). The original
signature is kept byte-for-byte and delegates to a new overload carrying the
dependency.

### 3.1 Case A — `ILogger.LogInformation` (same BEFORE as §2.1)

**AFTER** — generated `ILoggerWrapper.cs` is **identical to §2.1** (shared
wrapper-emission utility — see §7). Rewritten `Worker.Run`:
```csharp
    // Public API preserved: original signature unchanged, delegates to the overload.
    public void Run(string job)
        => Run(job, new LoggerWrapper(_logger));

    // New overload carries the mockable dependency; contains the real body.
    public void Run(string job, ILoggerWrapper loggerWrapper)
    {
        loggerWrapper.LogInformation("starting {Job}", job);  // ← site uses the parameter
    }
```

Rules:
- The **original method keeps its exact signature** (name, params, modifiers,
  return) and becomes a one-line delegator passing `new {wrapper_name}(<receiver>)`.
  `<receiver>` is the original call-site receiver expression (here `_logger`).
- The **new overload** appends a trailing `{param_type} {param_name}` parameter
  and contains the *original body* with the call-site receiver replaced by
  `{param_name}`.
- Default runtime path: callers hit the original signature → real forwarder
  constructed → behavior identical.
- The test calls the **new overload** directly with a mock of `{param_type}`.

### 3.2 Case C — `HttpClient.GetAsync` (async, return value)

**AFTER** — wrapper identical to §2.3; rewritten `ApiClient.FetchAsync`:
```csharp
    public Task<string> FetchAsync(string url)
        => FetchAsync(url, new HttpClientWrapper(_http));   // original signature preserved

    public async Task<string> FetchAsync(string url, IHttpClientWrapper httpClientWrapper)
    {
        HttpResponseMessage resp = await httpClientWrapper.GetAsync(url);
        return await resp.Content.ReadAsStringAsync();
    }
```

Async/return rule: the **overload** keeps `async` and the real body; the
**original** is a non-async expression-bodied delegator returning the `Task`
(behavior-identical, no extra await frame). For `void` methods the original is a
statement-bodied delegator (`{ M(args, new Wrapper(recv)); }`); for value-returning
methods it `return`s the overload result.

### 3.3 Receiver-availability requirement (hard constraint)

`parameterize_dependency` constructs `new {wrapper_name}(<receiver>)` **in the
original method's scope**. The receiver expression MUST be resolvable at the top
of the original method — i.e. a field/property of the containing type, or a
parameter of the method. If the receiver is a **local computed mid-body**, the
delegator cannot construct the wrapper ⇒ **reject cleanly** (`applicable=false`,
reason `receiver_not_in_method_scope`). See §5.

---

## 4. Anti-gaming / validity requirement (MOST IMPORTANT)

A phase-4 "pass" is only meaningful if the generated test exercises the target
method **through the new seam** — i.e. the mock the test injects is what is
actually invoked at the (rewritten) production call site. This section defines
"legitimately applied" vs "gamed" and the exact post-hoc verification.

### 4.1 What makes a transform legitimately applied (tool-time invariants)

The tool MUST guarantee, and encode in the `seam` descriptor, that after the
rewrite the **only** path to the target behavior at the call site is through the
seam type:

1. **Call-site exclusivity.** The original receiver-based invocation
   (`_logger.LogInformation(...)`) no longer exists at the rewritten site; it is
   replaced by an invocation on the injected interface (`_loggerWrapper.…` or the
   overload parameter). The tool must not leave the original invocation reachable
   on the same path.
2. **No behavior change on the default path.** Verified mechanically by the
   existing owning-project build (compile) — see §5 — and structurally by the
   default-construction of the real forwarder (`?? new Wrapper(recv)` / the
   delegator passing `new Wrapper(recv)`).
3. **No deletion / no-op.** The transform may only *redirect* the call through
   the seam; it may never delete the call, stub it to a constant, or empty the
   method. (The bounded menu already forbids free-form edits; this is the
   semantic restatement.)

### 4.2 What "gamed" looks like (must be excluded downstream)

- A test that compiles and passes **without** constructing a mock of `seam.interface`.
- A test that constructs the mock but **never injects** it at `seam.injection_ref`
  (so production still uses the real forwarder).
- A test that asserts trivially (`Assert.True(true)`) or never invokes the
  containing method/overload.
- A "pass" on a cell whose seam was `applicable=false`/reverted.

### 4.3 Post-hoc verification (exact mechanism)

`via_seam` is **not** knowable at apply time (the test does not exist yet), so it
is computed **after `submit_test` succeeds**, by a new verifier step in
[agentic_refactor_runner.py](../../tools/generation/agentic_refactor_runner.py)
that cross-references the **seam descriptor** against the **final submitted test
source**. The verifier sets `via_seam ∈ {true,false}` per cell using these
checks (ALL required for `true`):

1. **Seam-type referenced.** The test source mentions `seam.interface` (simple
   name) in a mock/fake construction context — e.g. `Mock<ILoggerWrapper>`,
   `Substitute.For<ILoggerWrapper>()`, or a hand-rolled `class … : ILoggerWrapper`.
2. **Injected at the injection point.** For `injection=ctor`: the test passes the
   mock object into the constructor of `seam.containing_type` (positional or named
   `param_name:`). For `injection=overload`: the test calls the overload whose
   signature is `seam.injection_ref`, passing the mock as the trailing argument.
3. **Target method driven.** The test invokes `seam.containing_type`'s method that
   contains the rewritten site (the method named in `call_site`), so the seam
   member is reached.
4. **Non-trivial assertion present.** At least one assertion that is not
   `Assert.True(true)` / `Assert.Equal(1, 1)` and that references the mock
   (`.Verify`/`.Received`) or a value flowing from the seam.

Checks 1–3 are **static** (regex/Roslyn over the test source — a small reuse of
the same `Microsoft.CodeAnalysis` infra is acceptable but a regex pass is
sufficient for v1). Because §4.1.1 guarantees the production site invokes **only**
the injected interface, checks 1–3 together are *sufficient* evidence that the
mock is what runs at the call site: if the mock is injected and the method is
driven, the production path cannot reach the real forwarder. Check 4 guards
against trivial assertions.

`via_seam` gates the **refactor-attributable metric** in
[PLAN.md](PLAN.md#metrics): a cell counts toward "passes only because of a
legitimate seam" iff it was run-fail in phase 3 **and** run-OK in phase 4 **and**
`via_seam=true`. Cells that pass with `via_seam=false` are reported separately
(possible gaming / incidental pass) and **excluded** from the headline
attribution.

### 4.4 `via_seam` field recommendation (binding)

- **Add `seam: dict` to `RefactorResult`** (apply-time, populated by the
  transform from the tool's `seam` JSON; empty `{}` for `make_virtual`). Surface
  it in `RefactorResult.to_dict()`.
- **Add `via_seam: bool | None` to the per-cell attempts row** (NOT to
  `RefactorResult`). It is `None` until verification runs, `True`/`False` after.
  Populated by the new verifier step in the runner once `submit_test` returns
  run-OK. Persist `seam` alongside it so the attribution is auditable from
  artifacts alone (`attempts.jsonl` row carries both `refactor_attempts[].seam`
  and the cell-level `via_seam`).

This keeps `RefactorResult` honest (it only knows what it did to production) while
making the legitimacy decision reproducible from saved artifacts.

---

## 5. Edge cases — handle or reject cleanly (NEVER corrupt)

The general rewriter MUST either produce a correct rewrite **or** return
`applicable=false` with a specific `reason`. It must never emit partially-edited
or non-compiling source on purpose. (The owning-project build is the backstop:
any rewrite that slips through and breaks compilation is auto-reverted by the
existing Python guard and recorded as `refactor_rejected`.)

| Case | `wrapper_interface` | `parameterize_dependency` |
|---|---|---|
| **Multi-line method/ctor signature** | **Handle** — operate on AST nodes, not lines | **Handle** |
| **Enclosing method already ends in an optional / `params` parameter** | n/a (method-level) | **Handle** — the injected dependency is a REQUIRED parameter inserted in the new overload BEFORE the trailing optional group and BEFORE any `params` array (required→dep→optional→params is always legal), keeping required-before-optional-before-params ordering valid. The delegator passes the dependency argument at the same position. **Reject** (`trailing_params_conflict`) only if no legal overload shape exists (e.g. a `__arglist` parameter, which must stay last and cannot be forwarded positionally). || **async / `Task`/`Task<T>` returns** | **Handle** — forwarder returns the `Task` non-async; site keeps `await` | **Handle** — overload keeps `async`, original delegates |
| **Generic target method** (`GetRequiredService<T>`) | **Handle** — carry type params + constraints; preserve type args at site | **Handle** |
| **Single existing ctor, incl. `: base(...)`** | **Handle** — append trailing optional param; append assignment last; preserve `: base(...)` | n/a (method-level) |
| **Ctor with `: this(...)` chaining** | **Reject** (`reason=ctor_chaining`) — threading the param through a chain risks behavior change | n/a |
| **Multiple constructors** | **Reject** (`reason=multiple_ctors`) — ambiguous injection point | n/a |
| **No explicit ctor** | **Handle** — synthesize a ctor taking `{interface}? {param}=null`; receiver must be a field initializer/property (else reject `no_receiver_source`) | n/a |
| **Primary constructor** (class/record) | **Reject** (`reason=primary_ctor`) in v1 | **Reject** if the method is the primary-ctor body region |
| **`record` type** | **Reject** (`reason=record_type`) v1 — positional/primary-ctor semantics | **Handle** if it's an ordinary method on the record and receiver is in scope |
| **`struct` type** | **Reject** (`reason=struct_type`) — injected-field/identity semantics unsafe | **Reject** (`struct_type`) |
| **Static containing method** | **Reject** (`reason=static_method_no_instance`) — no instance to hold an injected field | **Handle** — add a static overload with the extra param; original static method delegates |
| **`partial` class, edits in one part** | **Handle** — edit the part holding the ctor + site | **Handle** |
| **`partial` class, ctor & site in different files** | **Reject** (`reason=partial_split`) v1 | **Handle** if the method + receiver are co-located |
| **Receiver is a local computed mid-body** | **Handle** — wrapper wraps the field/property; if the receiver is a pure local with no field source, **reject** (`no_receiver_source`) | **Reject** (`receiver_not_in_method_scope`) — delegator can't reconstruct it |
| **Receiver is `this` / implicit-this extension** | **Reject** (`reason=receiver_is_this`) v1 | **Reject** (`receiver_is_this`) |
| **Name collision with existing type** | **Handle** — numeric suffix (`ILoggerWrapper2`) | **Handle** |
| **Target invocation not found at `file:line`** | **Reject** (`reason=site_not_found`) | **Reject** (`site_not_found`) |
| **Receiver type binds to `ErrorType`** (missing ref) | **Reject** (`reason=unbound_receiver`) — cannot reconstruct the signature safely | **Reject** (`unbound_receiver`) |

`reason` strings are part of the contract (the verifier and the smoke tests assert
on them); use the exact tokens above.

---

## 6. `RefactorResult` fields each transform must populate

Existing fields (unchanged semantics) — both transforms set:

| Field | `wrapper_interface` / `parameterize_dependency` value |
|---|---|
| `transform` | the requested name |
| `applied` | `True` iff the tool returned `applicable=true` **and** Python wrote the files |
| `reverted` | `True` iff the post-write owning-project build failed and `restore_all()` ran |
| `reason` | tool's `reason` (success summary or the §5 rejection token + human text) |
| `files_changed` | repo-relative paths from the tool's `files{}` (post-state), incl. the generated interface file |
| `build_ok` | result of `_build_owning_project()` (`None` if `verify_build=False`, e.g. mock mode) |
| `errors` | first build errors when `build_ok` is `False` |

New field (added by this contract):

| Field | Value |
|---|---|
| `seam` | the tool's `seam{}` dict (see §0.4 / §4.4). `{}` for `make_virtual`. Included in `to_dict()`. |

Not-applicable returns: `applied=False`, `reverted=False`, `build_ok=None`,
`files_changed=[]`, `seam={}`, `reason=<§5 token>` — Python writes nothing and
does not build (mirrors the existing `make_virtual` not-found return).

---

## 7. Shared vs distinct Roslyn code paths

**Decision: distinct rewrite paths over a shared core library.**

```
RoslynRefactorTool/
  Program.cs                  // argv parse, compile owning project, dispatch on --transform, emit JSON
  SeamCore.cs                 // SHARED: reference loading (mirror Mode1Analyzer.LoadReferences),
                              //         compilation build, locate+bind target invocation,
                              //         reconstruct instance-method signature from IMethodSymbol
                              //         (drop `this`, carry generics/constraints/params/nullability),
                              //         emit I{Wrapper}+{Wrapper} source, name/collision/defaults,
                              //         build the `seam` descriptor
  WrapperInterfaceRewriter.cs // DISTINCT: ctor param+field injection, append assignment last,
                              //           rewrite ALL same-receiver sites in the type
  ParameterizeDependencyRewriter.cs // DISTINCT: two-method overload-delegation, receiver-in-scope
                              //           check, static-method overload handling
```

**Rationale:**
- The **front half is identical** — both must load refs the Mode1Analyzer way,
  build the compilation, bind the invocation, reconstruct the seam member
  signature, and emit the wrapper. Duplicating it would let the two transforms
  drift (e.g. one handles generics, the other doesn't). → share in `SeamCore`.
- The **rewrite half is structurally different** — type-level constructor
  injection that rewrites *all* same-receiver call sites vs. method-level
  overload-delegation that rewrites *one* site and synthesizes a delegator. They
  have different applicability rules (§5 rows diverge: static methods, ctor
  chaining), different seam `injection` kinds, and different failure modes. Fusing
  them into one parameterized rewriter would be harder to test and reason about. →
  keep distinct `CSharpSyntaxRewriter`s.

This mirrors the project's existing taste: Mode1Analyzer keeps binding/reference
logic centralized and the per-pattern classification separate.

---

## 8. What is explicitly out of scope (v1)

- Rewriting receivers that are locals/`this`, primary constructors, records
  (wrapper case), structs, multi-ctor types — all **reject cleanly** (§5).
- Cross-project edits — the existing `_safe_prod_path` guard already confines
  writes to the owning `.csproj` subtree; the tool computes generated paths there.
- Touching `make_virtual` — unchanged, still pure-Python, still smoke-tested.

## 9. Test obligations for Watney (must land before run_1)

1. **Unit (C#, hermetic):** for each of §2/§3 Cases A/B/C, a fixture source in →
   asserts the rewritten output compiles and the `seam` JSON matches the
   descriptor. Plus one fixture per §5 reject row asserting `applicable=false` +
   exact `reason` token.
2. **Python integration (extend
   [test_apply_refactor.py](../../tools/generation/tests/) per REPLICATION §3):**
   `wrapper_interface` and `parameterize_dependency` each produce a compiling seam
   on a fixture project; a behavior-changing edit auto-reverts (`refactor_rejected`);
   fixture project byte-identical after each test (snapshot/restore).
3. **Smoke (extend
   [test_refactor_smoke.py](../../tools/generation/tests/test_refactor_smoke.py)):**
   add a mock fixture driving `apply_refactor(wrapper_interface)` end-to-end and
   assert the refactor log records it applied with a populated `seam`, and that the
   verifier sets `via_seam=true` for a seam-using mock test.
