using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynRefactorTool;

// SHARED front-half for both phase-4 transforms (TRANSFORM_CONTRACT §7):
//   * reference loading (mirrors Mode1Analyzer.LoadReferences — fast path, no restore)
//   * compilation build over the owning .csproj directory
//   * locate + bind the target invocation by file + line
//   * reconstruct the instance-method signature from the bound IMethodSymbol
//     (drop `this`, carry generics/constraints/params/nullability/optional/ref-out)
//   * emit I{Wrapper}+{Wrapper} source, with deterministic name inference and
//     collision suffixing (§1.1)
//   * build the `seam` descriptor (§0.4 / §4)
//
// The two rewriters (WrapperInterfaceRewriter, ParameterizeDependencyRewriter)
// consume a fully-populated SeamContext and only differ in how they rewrite the
// containing type.

internal sealed class RewriteResult
{
    public bool Applicable;
    public string Reason = "";
    public Dictionary<string, string> Files = new();
    public Dictionary<string, object?> Seam = new();

    public static RewriteResult Reject(string reason) =>
        new() { Applicable = false, Reason = reason };

    public static RewriteResult Ok(string reason, Dictionary<string, string> files, Dictionary<string, object?> seam) =>
        new() { Applicable = true, Reason = reason, Files = files, Seam = seam };
}

internal sealed class SeamContext
{
    public required CSharpCompilation Compilation;
    public required SemanticModel Model;
    public required SyntaxTree Tree;
    public required string TargetFileAbs;
    public int Line;

    public required InvocationExpressionSyntax Invocation;
    public required IMethodSymbol Method;          // reduced (instance) form for extensions
    public required IMethodSymbol BoundMethod;      // raw bound symbol at the target site (for overload-precise matching)
    public required ITypeSymbol ReceiverType;       // type the seam member lives on
    public ExpressionSyntax? ReceiverExpr;          // the receiver expression at the call site (null = implicit this)
    public ISymbol? ReceiverSymbol;                 // bound symbol of the receiver (field/prop/local/param)

    public required TypeDeclarationSyntax ContainingType;
    public required INamedTypeSymbol ContainingTypeSymbol;
    public MethodDeclarationSyntax? EnclosingMethod;

    // Resolved names (§1.1).
    public string InterfaceName = "";
    public string WrapperName = "";
    public string ParamName = "";   // camelCase(wrapper) — ctor param / overload param base
    public string FieldName = "";   // _paramName
    public string Namespace = "";   // containing namespace ("" = global)

    public string Transform = "";

    public string ReceiverText => ReceiverExpr?.ToString() ?? "this";
    public string Fqn(string simple) => Namespace.Length == 0 ? simple : Namespace + "." + simple;
}

internal static class SeamCore
{
    // Generated-file type format: fully qualified WITH global:: so emitted source
    // never needs usings and never collides. Special types stay as keywords
    // (object/string/int/void) and nullable reference modifiers are preserved.
    public static readonly SymbolDisplayFormat GeneratedTypeFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    // Clean (no-namespace) format for the human-readable member_signature in the seam.
    public static readonly SymbolDisplayFormat CleanTypeFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    // -- references (mirror Mode1Analyzer.LoadReferences) ------------------
    //
    // Reference loading is DEDUP-BY-SIMPLE-NAME and tiered, so each later tier
    // only contributes assemblies whose file name is not already present. This
    // is what keeps the augmentation REGRESSION-SAFE: the authoritative tiers
    // (the NETCore.App runtime + the curated `refs/` set the transforms were
    // validated against) always win, so any target that already bound under the
    // old reference set binds IDENTICALLY. Later tiers can only ADD types that
    // were previously unresolvable (the `unbound_receiver` root cause), never
    // replace or shadow an existing reference identity.
    //
    //   1. NETCore.App runtime (typeof(object) dir) — System.* / BCL.
    //   2. tool-bundled `refs/` — the curated net9 abstractions set
    //      (DI / Logging / Configuration) the rewriters were validated against.
    //   3. Microsoft.AspNetCore.App shared framework (version-aligned) — adds
    //      the ASP.NET Core + extended Microsoft.Extensions.* surface
    //      (HttpContext / RequestDelegate / IEndpointRouteBuilder / Options /
    //      EF Core abstractions) that aspnetcore/eShop/server/jellyfin targets
    //      bind their receivers through but which is NOT in tiers 1-2. Only
    //      net-new names are added, so the curated DI/Logging refs stay
    //      authoritative (avoids the net9-vs-net10 duplicate-type ambiguity).
    public static List<MetadataReference> LoadReferences()
    {
        var byName = new Dictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);

        void AddDir(string? dir)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
            foreach (var dll in Directory.GetFiles(dir, "*.dll"))
            {
                var name = Path.GetFileName(dll);
                if (byName.ContainsKey(name)) continue;   // earlier tier wins
                try { byName[name] = MetadataReference.CreateFromFile(dll); } catch { }
            }
        }

        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        AddDir(runtimeDir);                                          // tier 1
        AddDir(Path.Combine(AppContext.BaseDirectory, "refs"));      // tier 2
        AddDir(FindAspNetCoreSharedFramework(runtimeDir));           // tier 3

        return byName.Values.ToList();
    }

    // Locate the Microsoft.AspNetCore.App shared-framework directory that sits
    // alongside the running NETCore.App runtime, preferring the version whose
    // major.minor matches the runtime, then the highest STABLE (non-preview)
    // version, then the highest available. Returns null if not installed.
    private static string? FindAspNetCoreSharedFramework(string netCoreRuntimeDir)
    {
        try
        {
            // runtimeDir = .../shared/Microsoft.NETCore.App/<ver>
            var sharedRoot = Path.GetDirectoryName(Path.GetDirectoryName(netCoreRuntimeDir));
            if (sharedRoot is null) return null;
            var aspNetRoot = Path.Combine(sharedRoot, "Microsoft.AspNetCore.App");
            if (!Directory.Exists(aspNetRoot)) return null;

            var runtimeVer = new DirectoryInfo(netCoreRuntimeDir).Name;
            var runtimeMajorMinor = string.Join('.', runtimeVer.Split('.').Take(2));

            var versions = Directory.GetDirectories(aspNetRoot)
                .Select(d => new DirectoryInfo(d).Name)
                .ToList();

            // 1. exact major.minor match (prefer stable over preview).
            string? Pick(IEnumerable<string> cands) => cands
                .OrderBy(v => v.Contains('-') ? 1 : 0)   // stable first
                .ThenByDescending(v => v, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            var matched = Pick(versions.Where(v => v.StartsWith(runtimeMajorMinor + ".", StringComparison.OrdinalIgnoreCase)));
            var chosen = matched ?? Pick(versions);
            return chosen is null ? null : Path.Combine(aspNetRoot, chosen);
        }
        catch { return null; }
    }

    // -- compilation over the owning .csproj directory ---------------------

    // The .NET SDK injects a generated `*.GlobalUsings.g.cs` for projects with
    // `<ImplicitUsings>enable</ImplicitUsings>` (the modern default). That file
    // lives under obj/ — which this fast-path compilation deliberately skips —
    // so without re-supplying these global usings, target files that rely on
    // implicit `using System;` (etc.) fail to bind common BCL types such as
    // `System.IServiceProvider`, yielding a spurious `unbound_receiver`
    // (TRANSFORM_CONTRACT §2.2 Case B). We re-add the default Microsoft.NET.Sdk
    // implicit-usings set so the analysis compilation matches how the owning
    // project actually compiles. Global usings are additive and lowest-priority,
    // so this never breaks files with explicit usings.
    private const string ImplicitUsingsSource =
        "global using global::System;\n" +
        "global using global::System.Collections.Generic;\n" +
        "global using global::System.IO;\n" +
        "global using global::System.Linq;\n" +
        "global using global::System.Net.Http;\n" +
        "global using global::System.Threading;\n" +
        "global using global::System.Threading.Tasks;\n" +
        "global using global::Microsoft.AspNetCore.Builder;\n" +
        "global using global::Microsoft.AspNetCore.Hosting;\n" +
        "global using global::Microsoft.AspNetCore.Http;\n" +
        "global using global::Microsoft.AspNetCore.Routing;\n" +
        "global using global::Microsoft.Extensions.Configuration;\n" +
        "global using global::Microsoft.Extensions.DependencyInjection;\n" +
        "global using global::Microsoft.Extensions.Hosting;\n" +
        "global using global::Microsoft.Extensions.Logging;\n";

    public static CSharpCompilation BuildCompilation(string owningDir, List<MetadataReference> refs)
    {
        // Augment the global reference set with the owning project's own build
        // output reference closure (bin/**/*.dll), dedup-by-simple-name against
        // what is already loaded. This is the REGRESSION-SAFE fix for receivers
        // whose type (or a base type they inherit a member from) is declared in
        // a SIBLING project of the SAME repo — e.g. OpenRA.Game's HttpClient-
        // Factory consumed from OpenRA.Mods.Common, bitwarden's base repository
        // in Core, or an abp/orleans cross-assembly context type. When the
        // project has been built (the build-verified gate builds the owning
        // project before locating), its bin closure contains exactly those
        // project-reference + package assemblies, so the receiver binds the
        // same way the real compiler binds it. Only net-new names are added, so
        // the runtime + curated refs stay authoritative (no duplicate-type
        // ambiguity, no change to already-binding targets).
        var allRefs = new List<MetadataReference>(refs);
        AddOwningProjectBinReferences(owningDir, allRefs);

        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var trees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(ImplicitUsingsSource, parseOptions,
                path: Path.Combine(owningDir, "__ImplicitGlobalUsings.g.cs")),
        };
        foreach (var f in Directory.EnumerateFiles(owningDir, "*.cs", SearchOption.AllDirectories))
        {
            var rel = f.Substring(owningDir.Length).Replace('\\', '/').ToLowerInvariant();
            if (rel.Contains("/obj/") || rel.Contains("/bin/")) continue;
            try
            {
                var src = File.ReadAllText(f);
                trees.Add(CSharpSyntaxTree.ParseText(src, parseOptions, path: f));
            }
            catch { /* tolerate unreadable/unparseable trees, as AnalyzeRepo does */ }
        }
        return CSharpCompilation.Create(
            "RefactorTarget",
            trees,
            allRefs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                nullableContextOptions: NullableContextOptions.Enable));
    }

    // Add the owning project's bin/ build-output reference closure, dedup by
    // simple file name against what is already loaded. Skips ref-only facade
    // copies (`/ref/`, `/refint/`) and prefers higher-sorting (newer-TFM /
    // Release) paths deterministically so a project built for multiple TFMs
    // resolves to a single stable copy per assembly name.
    private static void AddOwningProjectBinReferences(string owningDir, List<MetadataReference> refs)
    {
        var binDir = Path.Combine(owningDir, "bin");
        if (!Directory.Exists(binDir)) return;

        var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in refs)
            if (r.Display is { } d) loaded.Add(Path.GetFileName(d));

        IEnumerable<string> dlls;
        try
        {
            dlls = Directory.EnumerateFiles(binDir, "*.dll", SearchOption.AllDirectories)
                .OrderByDescending(p => p, StringComparer.OrdinalIgnoreCase);
        }
        catch { return; }

        foreach (var dll in dlls)
        {
            var lower = dll.Replace('\\', '/').ToLowerInvariant();
            if (lower.Contains("/ref/") || lower.Contains("/refint/")) continue;
            var name = Path.GetFileName(dll);
            if (!loaded.Add(name)) continue;   // dedup: earlier tier / first copy wins
            try { refs.Add(MetadataReference.CreateFromFile(dll)); } catch { }
        }
    }

    // -- diagnostics: explain WHY an invocation/receiver did not bind ------
    //
    // Appended (after a colon) to the `unbound_receiver` token so the reason
    // stays bucket-compatible (sweep takes the leading token) while carrying
    // the actual root cause: candidate-reason, candidate count, the receiver
    // expression, and the distinct CS error ids the compiler reports inside
    // the invocation span (CS0246 = missing using/assembly, CS1061 = no such
    // member, CS0234 = missing namespace member, etc.).
    private static string BindDiag(string where, SemanticModel model,
        InvocationExpressionSyntax hit, SymbolInfo symInfo)
    {
        var sb = new StringBuilder();
        sb.Append(where);
        sb.Append(" candReason=").Append(symInfo.CandidateReason);
        sb.Append(" cands=").Append(symInfo.CandidateSymbols.Length);
        string recvText = hit.Expression is MemberAccessExpressionSyntax ma
            ? ma.Expression.ToString()
            : hit.Expression.ToString();
        if (recvText.Length > 48) recvText = recvText.Substring(0, 48) + "…";
        sb.Append(" recv='").Append(recvText).Append('\'');

        // CS error ids reported within the invocation span.
        var span = hit.Span;
        var ids = model.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error && d.Location.SourceSpan.IntersectsWith(span))
            .Select(d => d.Id)
            .Distinct()
            .Take(6)
            .ToList();
        if (ids.Count > 0)
            sb.Append(" diags=[").Append(string.Join(",", ids)).Append(']');
        return sb.ToString();
    }

    // -- locate + bind the target invocation -------------------------------
    //
    // Returns a populated SeamContext, or null with `reason` set to a §5 token.
    public static SeamContext? Locate(
        CSharpCompilation compilation,
        string targetFileAbs,
        int line,
        string method,
        string transform,
        string receiverTypeHint,
        string containingTypeHint,
        string kindHint,
        string interfaceOverride,
        string wrapperOverride,
        string paramOverride,
        out string reason)
    {
        reason = "";
        var normTarget = Path.GetFullPath(targetFileAbs).Replace('\\', '/');
        SyntaxTree? tree = null;
        foreach (var t in compilation.SyntaxTrees)
        {
            if (Path.GetFullPath(t.FilePath).Replace('\\', '/') == normTarget)
            {
                tree = t;
                break;
            }
        }
        if (tree is null)
        {
            reason = "site_not_found: target file not part of the owning project compilation";
            return null;
        }

        var root = tree.GetRoot();
        var model = compilation.GetSemanticModel(tree);

        var byName = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(i => InvokedName(i) == method).ToList();
        if (byName.Count == 0)
        {
            reason = "site_not_found";
            return null;
        }

        var lineContext = BuildLineContext(root, tree, line);
        var hit = SelectBestInvocationCandidate(
            byName,
            model,
            compilation,
            line,
            method,
            receiverTypeHint,
            containingTypeHint,
            kindHint,
            lineContext);
        if (hit is null)
        {
            reason = "site_not_found";
            return null;
        }

        var symInfo = model.GetSymbolInfo(hit);
        var methodSym = symInfo.Symbol as IMethodSymbol
                        ?? PickBestCandidate(hit, symInfo.CandidateSymbols.OfType<IMethodSymbol>().ToImmutableArray(), model);
        methodSym ??= TryResolveUnboundExtensionMethod(
            hit,
            compilation,
            model,
            method,
            receiverTypeHint,
            containingTypeHint);
        if (methodSym is null)
        {
            reason = "unbound_receiver: " + BindDiag("method_unbound", model, hit, symInfo);
            return null;
        }
        // Reconstruct the seam member from the ORIGINAL (uninstantiated)
        // definition so generic type parameters stay as `T` rather than the
        // constructed argument. The call site keeps its explicit type args.
        var methodDef = methodSym.OriginalDefinition;

        // Receiver expression + type.
        ExpressionSyntax? recvExpr = null;
        if (hit.Expression is MemberAccessExpressionSyntax ma)
            recvExpr = ma.Expression;
        else if (hit.Expression is MemberBindingExpressionSyntax
                 && hit.Parent is ConditionalAccessExpressionSyntax ca
                 && ca.WhenNotNull == hit)
            recvExpr = ca.Expression; // preserve explicit receiver for `x?.M(...)`
        else if (hit.Expression is MemberBindingExpressionSyntax)
            recvExpr = null;
        // else: bare identifier (implicit this) → recvExpr stays null

        // Receiver type resolution (TRANSFORM_CONTRACT §2.2 Case B).
        //
        // For an extension invocation, take the receiver from the DECLARED
        // `this` parameter of the un-reduced static method (ReducedFrom). This
        // mirrors Mode1Analyzer's classification path, which binds these exact
        // IServiceProvider/IServiceScopeFactory framework extensions reliably.
        //
        // The reduced extension's own `ReceiverType` is inferred against the
        // call-site receiver during reduction; when the extension's `this`
        // parameter type and the call-site receiver originate from DIFFERENT
        // reference assemblies (e.g. the bundled net9.0 DI Abstractions ref vs
        // the net10 runtime `System.IServiceProvider`), that inference can yield
        // a null/error receiver — the false `unbound_receiver` Beck flagged. The
        // declared `this` parameter type is always a concrete, bound symbol and
        // sidesteps the reference-identity split.
        ITypeSymbol? recvType = null;
        if (methodDef.MethodKind == MethodKind.ReducedExtension
            && methodDef.ReducedFrom is { } unreducedDef
            && unreducedDef.Parameters.Length > 0)
        {
            var declaredRecv = unreducedDef.Parameters[0].Type;
            if (declaredRecv is not null && declaredRecv is not IErrorTypeSymbol)
                recvType = declaredRecv;
        }
        if ((recvType is null || recvType is IErrorTypeSymbol)
            && methodDef.IsExtensionMethod
            && methodDef.Parameters.Length > 0)
        {
            var declaredRecv = methodDef.Parameters[0].Type;
            if (declaredRecv is not null && declaredRecv is not IErrorTypeSymbol)
                recvType = declaredRecv;
        }
        recvType ??= methodDef.ReceiverType;
        if ((recvType is null || recvType is IErrorTypeSymbol) && recvExpr is not null)
            recvType = model.GetTypeInfo(recvExpr).Type;
        if (recvType is null || recvType is IErrorTypeSymbol)
        {
            // Genuine failure: the receiver truly cannot be bound (e.g. a
            // non-existent / typo'd receiver type). This guard is preserved.
            reason = "unbound_receiver: " + BindDiag("recv_type_error", model, hit, symInfo);
            return null;
        }

        var typeDecl = hit.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is null)
        {
            reason = "site_not_found: no containing type";
            return null;
        }
        var typeSym = model.GetDeclaredSymbol(typeDecl);
        if (typeSym is null)
        {
            reason = "site_not_found: containing type unbound";
            return null;
        }

        var ns = typeSym.ContainingNamespace.IsGlobalNamespace
            ? "" : typeSym.ContainingNamespace.ToDisplayString();

        var ctx = new SeamContext
        {
            Compilation = compilation,
            Model = model,
            Tree = tree,
            TargetFileAbs = Path.GetFullPath(targetFileAbs),
            Line = line,
            Invocation = hit,
            Method = methodDef,
            ReceiverType = recvType,
            BoundMethod = methodSym,            ReceiverExpr = recvExpr,
            ReceiverSymbol = recvExpr is null ? null : model.GetSymbolInfo(recvExpr).Symbol,
            ContainingType = typeDecl,
            ContainingTypeSymbol = typeSym,
            EnclosingMethod = hit.FirstAncestorOrSelf<MethodDeclarationSyntax>(),
            Namespace = ns,
            Transform = transform,
        };

        ResolveNames(ctx, interfaceOverride, wrapperOverride, paramOverride);
        return ctx;
    }

    private static (TypeDeclarationSyntax? type, MethodDeclarationSyntax? method) BuildLineContext(
        SyntaxNode root,
        SyntaxTree tree,
        int line)
    {
        if (line <= 0) return (null, null);
        var text = tree.GetText();
        var lineIndex = Math.Clamp(line - 1, 0, Math.Max(0, text.Lines.Count - 1));
        var pos = text.Lines[lineIndex].Start;
        var token = root.FindToken(pos);
        var node = token.Parent;
        return (
            node?.FirstAncestorOrSelf<TypeDeclarationSyntax>(),
            node?.FirstAncestorOrSelf<MethodDeclarationSyntax>());
    }

    private static InvocationExpressionSyntax? SelectBestInvocationCandidate(
        List<InvocationExpressionSyntax> byName,
        SemanticModel model,
        CSharpCompilation compilation,
        int targetLine,
        string method,
        string receiverTypeHint,
        string containingTypeHint,
        string kindHint,
        (TypeDeclarationSyntax? type, MethodDeclarationSyntax? method) lineContext)
    {
        var receiverHints = NormalizeReceiverHints(receiverTypeHint);
        var containingHint = NormalizeSimpleName(containingTypeHint);
        var expectedExtension = string.Equals(kindHint?.Trim(), "Extension", StringComparison.OrdinalIgnoreCase)
            ? true
            : string.Equals(kindHint?.Trim(), "NonVirtual", StringComparison.OrdinalIgnoreCase)
                ? false
                : (bool?)null;

        var scored = new List<(InvocationExpressionSyntax inv, int score)>();

        foreach (var inv in byName)
        {
            var symInfo = model.GetSymbolInfo(inv);
            var bound = symInfo.Symbol as IMethodSymbol
                ?? PickBestCandidate(inv, symInfo.CandidateSymbols.OfType<IMethodSymbol>().ToImmutableArray(), model);
            if (bound is null) continue;

            int score = 0;
            var nameLine = NameLine(inv);
            var startLine = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var dist = targetLine > 0
                ? Math.Min(Math.Abs(nameLine - targetLine), Math.Abs(startLine - targetLine))
                : int.MaxValue / 4;

            // Primary locator: line proximity with explicit bonus for exact line.
            if (targetLine > 0)
            {
                score += Math.Max(0, 200 - Math.Min(dist, 200));
                if (nameLine == targetLine || startLine == targetLine) score += 180;
            }

            var invType = inv.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            var invMethod = inv.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (lineContext.type is not null && invType is not null
                && string.Equals(invType.Identifier.Text, lineContext.type.Identifier.Text, StringComparison.Ordinal))
                score += 120;

            if (lineContext.method is not null && invMethod is not null
                && string.Equals(invMethod.Identifier.Text, lineContext.method.Identifier.Text, StringComparison.Ordinal)
                && invMethod.ParameterList.Parameters.Count == lineContext.method.ParameterList.Parameters.Count)
                score += 200;

            if (!string.IsNullOrEmpty(containingHint))
            {
                bool callsiteTypeMatch = invType is not null
                    && string.Equals(invType.Identifier.Text, containingHint, StringComparison.Ordinal);
                bool declaringTypeMatch = SymbolNameMatchesHint(
                    (bound.ReducedFrom ?? bound).ContainingType,
                    containingHint);
                if (!callsiteTypeMatch && !declaringTypeMatch)
                    continue;

                // `containing_type` in targets may name either the call-site
                // enclosing type OR the invoked member's declaring type
                // (common for extension rows like LoggerExtensions).
                score += callsiteTypeMatch ? 220 : 140;
                if (declaringTypeMatch)
                    score += 120;
            }

            if (expectedExtension.HasValue)
            {
                bool isExt = bound.MethodKind == MethodKind.ReducedExtension
                             || bound.IsExtensionMethod
                             || bound.ReducedFrom is not null;
                if (isExt != expectedExtension.Value) continue;
                score += 160;
            }

            var recvType = ResolveReceiverTypeForCandidate(inv, bound, model);
            if (receiverHints.Count > 0)
            {
                if (!ReceiverTypeMatchesHints(recvType, receiverHints, method))
                    continue;
                score += 180;
            }

            // Prefer signatures whose overload key is fully resolved and stable.
            score += OverloadKey(bound).ToDisplayString().Length % 17;

            scored.Add((inv, score));
        }

        if (scored.Count == 0)
        {
            // Fallback: if semantic filtering produced no candidate but there is
            // exactly one invocation on the target line, prefer that site.
            // This preserves line-anchored intent for noisy/broken semantic
            // contexts and avoids broad nearest-neighbor rewrites.
            var exactLine = byName
                .Where(i =>
                {
                    var start = i.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    var name = NameLine(i);
                    return start == targetLine || name == targetLine;
                })
                .ToList();

            if (targetLine > 0 && exactLine.Count == 1)
                return exactLine[0];

            return null;
        }
        scored.Sort((a, b) => b.score.CompareTo(a.score));
        if (scored.Count > 1 && scored[0].score == scored[1].score)
            return null; // ambiguity-safe: avoid rewriting the wrong duplicate site.
        return scored[0].inv;
    }

    private static HashSet<string> NormalizeReceiverHints(string raw)
    {
        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw)) return hs;

        foreach (var part in raw.Split('/'))
        {
            var p = part.Trim();
            if (p.Length == 0) continue;
            hs.Add(p);
            hs.Add(NormalizeSimpleName(p));
        }
        return hs;
    }

    private static string NormalizeSimpleName(string? t)
    {
        if (string.IsNullOrWhiteSpace(t)) return "";
        var s = t.Trim();
        var slash = s.LastIndexOf('/');
        if (slash >= 0) s = s.Substring(slash + 1);
        var dot = s.LastIndexOf('.');
        if (dot >= 0) s = s.Substring(dot + 1);
        var tick = s.IndexOf('`');
        if (tick >= 0) s = s.Substring(0, tick);
        var gen = s.IndexOf('<');
        if (gen >= 0) s = s.Substring(0, gen);
        return s.Trim();
    }

    private static bool SymbolNameMatchesHint(INamedTypeSymbol? symbol, string hint)
    {
        if (symbol is null || string.IsNullOrWhiteSpace(hint)) return false;

        var simple = NormalizeSimpleName(symbol.Name);
        if (string.Equals(simple, hint, StringComparison.OrdinalIgnoreCase))
            return true;

        var full = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "", StringComparison.Ordinal);
        var fullSimple = NormalizeSimpleName(full);
        return string.Equals(full, hint, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fullSimple, hint, StringComparison.OrdinalIgnoreCase);
    }

    private static ITypeSymbol? ResolveReceiverTypeForCandidate(
        InvocationExpressionSyntax inv,
        IMethodSymbol bound,
        SemanticModel model)
    {
        var def = bound.OriginalDefinition;
        ITypeSymbol? recvType = null;
        if (def.MethodKind == MethodKind.ReducedExtension
            && def.ReducedFrom is { } unreduced
            && unreduced.Parameters.Length > 0)
        {
            var declaredRecv = unreduced.Parameters[0].Type;
            if (declaredRecv is not null && declaredRecv is not IErrorTypeSymbol)
                recvType = declaredRecv;
        }
        recvType ??= def.ReceiverType;
        if (recvType is null || recvType is IErrorTypeSymbol)
        {
            if (inv.Expression is MemberAccessExpressionSyntax ma)
                recvType = model.GetTypeInfo(ma.Expression).Type;
        }
        return recvType;
    }

    private static IMethodSymbol? TryResolveUnboundExtensionMethod(
        InvocationExpressionSyntax hit,
        CSharpCompilation compilation,
        SemanticModel model,
        string method,
        string receiverTypeHint,
        string containingTypeHint)
    {
        if (string.IsNullOrWhiteSpace(method)) return null;

        var recvHints = NormalizeReceiverHints(receiverTypeHint);
        if (hit.Expression is MemberAccessExpressionSyntax ma)
        {
            var recvType = model.GetTypeInfo(ma.Expression).Type;
            if (recvType is not null)
            {
                var recvFull = recvType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", "", StringComparison.Ordinal);
                if (!string.IsNullOrWhiteSpace(recvFull))
                {
                    recvHints.Add(recvFull);
                    recvHints.Add(NormalizeSimpleName(recvFull));
                    recvHints.Add(NormalizeSimpleName(recvType.Name));
                }
            }
        }

        var containingHint = NormalizeSimpleName(containingTypeHint);
        var argCount = hit.ArgumentList?.Arguments.Count ?? 0;

        var candidates = compilation
            .GetSymbolsWithName(method, SymbolFilter.Member)
            .OfType<IMethodSymbol>()
            .Where(m => m.IsExtensionMethod && string.Equals(m.Name, method, StringComparison.Ordinal))
            .ToList();

        if (candidates.Count == 0) return null;

        var scored = new List<(IMethodSymbol m, int score)>();
        foreach (var m in candidates)
        {
            if (m.Parameters.Length == 0) continue;

            var recvParamType = m.Parameters[0].Type;
            if (recvHints.Count > 0 && !ReceiverTypeMatchesHints(recvParamType, recvHints, method))
                continue;

            var nonThis = m.Parameters.Skip(1).ToArray();
            var minArgs = nonThis.Count(p => !p.IsOptional && !p.IsParams);
            var hasParams = nonThis.LastOrDefault()?.IsParams == true;
            var maxArgs = hasParams ? int.MaxValue : nonThis.Length;
            if (argCount < minArgs || argCount > maxArgs)
                continue;

            int score = 0;
            if (!string.IsNullOrEmpty(containingHint)
                && SymbolNameMatchesHint(m.ContainingType, containingHint))
                score += 300;

            if (recvHints.Count > 0 && ReceiverTypeMatchesHints(recvParamType, recvHints, method))
                score += 220;

            if (!hasParams && argCount == nonThis.Length)
                score += 180;
            else if (hasParams)
                score += 120;

            // Prefer non-generic overloads in ambiguous telemetry-heavy APIs.
            if (m.TypeParameters.Length == 0)
                score += 15;

            scored.Add((m, score));
        }

        if (scored.Count == 0) return null;
        scored.Sort((a, b) => b.score.CompareTo(a.score));
        if (scored.Count > 1 && scored[0].score == scored[1].score)
            return null;

        return scored[0].m;
    }

    private static bool ReceiverTypeMatchesHints(ITypeSymbol? recvType, HashSet<string> hints, string method)
    {
        if (recvType is null || recvType is IErrorTypeSymbol) return false;

        var simple = NormalizeSimpleName(recvType.Name);
        var full = recvType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "", StringComparison.Ordinal);
        var fullSimple = NormalizeSimpleName(full);

        if (hints.Contains(simple) || hints.Contains(full) || hints.Contains(fullSimple))
            return true;

        // Compatibility fallback for framework aliases, e.g. HttpMessageInvoker rows
        // targeting HttpClient call sites.
        if (string.Equals(method, "GetAsync", StringComparison.Ordinal)
            || string.Equals(method, "PostAsync", StringComparison.Ordinal)
            || string.Equals(method, "SendAsync", StringComparison.Ordinal))
        {
            if ((hints.Contains("HttpMessageInvoker") && (simple == "HttpClient" || fullSimple == "HttpClient"))
                || (hints.Contains("HttpClient") && (simple == "HttpMessageInvoker" || fullSimple == "HttpMessageInvoker")))
                return true;
        }
        return false;
    }

    // -- name inference + collision suffixing (§1.1) -----------------------

    private static void ResolveNames(SeamContext ctx, string interfaceOverride, string wrapperOverride, string paramOverride)
    {
        var recvSimple = ctx.ReceiverType.Name;
        // Interface convention: strip a single leading I before an uppercase.
        var recv = Regex.IsMatch(recvSimple, "^I[A-Z]") ? recvSimple.Substring(1) : recvSimple;

        var ifaceBase = string.IsNullOrWhiteSpace(interfaceOverride) ? "I" + recv + "Wrapper" : interfaceOverride.Trim();
        var wrapBase = string.IsNullOrWhiteSpace(wrapperOverride) ? recv + "Wrapper" : wrapperOverride.Trim();

        var iface = ifaceBase;
        var wrap = wrapBase;
        if (TypeExists(ctx.Compilation, iface) || TypeExists(ctx.Compilation, wrap))
        {
            for (int n = 2; n < 1000; n++)
            {
                var i2 = ifaceBase + n;
                var w2 = wrapBase + n;
                if (!TypeExists(ctx.Compilation, i2) && !TypeExists(ctx.Compilation, w2))
                {
                    iface = i2; wrap = w2; break;
                }
            }
        }

        ctx.InterfaceName = iface;
        ctx.WrapperName = wrap;
        ctx.ParamName = string.IsNullOrWhiteSpace(paramOverride) ? CamelCase(wrap) : paramOverride.Trim();
        ctx.FieldName = "_" + ctx.ParamName;
    }

    private static bool TypeExists(Compilation comp, string name) =>
        comp.GetSymbolsWithName(name, SymbolFilter.Type).Any();

    public static string CamelCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

    // -- invocation name + overload-identity helpers -----------------------

    // Stable overload identity for symbol-precise call-site matching (BUG #2):
    // reduce extension methods to their static definition and strip generic
    // instantiation, so the SAME overload at different call sites compares equal
    // while DIFFERENT overloads of the same name compare unequal.
    public static IMethodSymbol OverloadKey(IMethodSymbol m) =>
        (m.ReducedFrom ?? m).OriginalDefinition;

    /// <summary>
    /// Extracts the simple method name from an invocation expression,
    /// handling member access (a.M), member binding (?. operator), generic names,
    /// and direct identifier references.
    /// </summary>
    /// <remarks>
    /// Returns empty string if the expression is not a recognized invocation pattern.
    /// </remarks>
    public static string InvokedName(InvocationExpressionSyntax inv) => inv.Expression switch
    {
        MemberAccessExpressionSyntax ma => SimpleName(ma.Name),
        MemberBindingExpressionSyntax mb => SimpleName(mb.Name),
        GenericNameSyntax gn => gn.Identifier.Text,
        IdentifierNameSyntax id => id.Identifier.Text,
        _ => "",
    };

    private static string SimpleName(SimpleNameSyntax n) => n.Identifier.Text;

    /// <summary>
    /// Gets the 1-based line number of the invoked member's simple-name token.
    /// For multi-line invocations, returns the line of the member name identifier,
    /// not the position of the opening paren or receiver.
    /// </summary>
    /// <remarks>
    /// This is used to match invocation sites to their corresponding rows in
    /// the targets CSV, which use the member-name line rather than the start-of-invocation line.
    /// </remarks>
    public static int NameLine(InvocationExpressionSyntax inv)
    {
        SyntaxToken tok = inv.Expression switch
        {
            MemberAccessExpressionSyntax ma => ma.Name.Identifier,
            MemberBindingExpressionSyntax mb => mb.Name.Identifier,
            GenericNameSyntax gn => gn.Identifier,
            IdentifierNameSyntax id => id.Identifier,
            _ => inv.GetFirstToken(),
        };
        return tok.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
    }

    // -- overload-candidate selection --------------------------------------
    //
    // When overload resolution does NOT fully bind (symInfo.Symbol is null —
    // e.g. the net9-Abstractions vs net10-runtime reference split prevents the
    // compiler from picking a single overload), CandidateSymbols holds every
    // same-named overload. Blindly taking FirstOrDefault() yields an ARBITRARY
    // overload whose signature may not match the actual call site, producing a
    // wrong seam member and a non-compiling rewrite (orleans:0116: the 4-param
    // LogDebug(EventId, Exception?, ...) was picked over the intended
    // LogDebug(string?, params object?[]) for `logger.LogDebug("msg", a, b)`).
    //
    // PickBestCandidate selects the candidate whose (extension-reduced)
    // parameter list is arity-compatible with the supplied arguments, and —
    // among the compatible ones — the candidate whose positional argument types
    // best match its parameter types. Exact fixed arity and fewer parameters
    // break remaining ties. Falls back to the first candidate (legacy behavior)
    // only when nothing is compatible, so previously-working cases never regress.
    public static IMethodSymbol? PickBestCandidate(
        InvocationExpressionSyntax hit,
        ImmutableArray<IMethodSymbol> candidates,
        SemanticModel model)
    {
        var methods = candidates.Where(c => c is not null).ToImmutableArray();
        if (methods.IsDefaultOrEmpty) return null;
        if (methods.Length == 1) return methods[0];

        var args = hit.ArgumentList?.Arguments ?? default;
        int argCount = args.Count;
        bool hasNamedArgs = args.Any(a => a.NameColon is not null);

        // Receiver context, used to reduce un-reduced extension candidates so
        // their parameter count excludes the leading `this`.
        var recvExpr = hit.Expression is MemberAccessExpressionSyntax ma ? ma.Expression : null;
        ITypeSymbol? recvType = recvExpr is null ? null : model.GetTypeInfo(recvExpr).Type;
        bool recvIsType = recvExpr is not null && model.GetSymbolInfo(recvExpr).Symbol is ITypeSymbol;
        bool instanceStyle = hit.Expression is MemberAccessExpressionSyntax or MemberBindingExpressionSyntax;

        // Explicit type arguments at the call site gate generic arity.
        int explicitTypeArgs = hit.Expression switch
        {
            MemberAccessExpressionSyntax { Name: GenericNameSyntax g } => g.TypeArgumentList.Arguments.Count,
            MemberBindingExpressionSyntax { Name: GenericNameSyntax gb } => gb.TypeArgumentList.Arguments.Count,
            GenericNameSyntax gn => gn.TypeArgumentList.Arguments.Count,
            _ => 0,
        };

        IMethodSymbol? best = null;
        (int typeScore, int exactness, int negParamCount) bestKey = default;

        foreach (var c in methods)
        {
            if (explicitTypeArgs > 0 && c.TypeParameters.Length != explicitTypeArgs) continue;

            var ps = EffectiveParameters(c, instanceStyle, recvIsType, recvType);
            int n = ps.Length;
            bool hasParams = n > 0 && ps[n - 1].IsParams;
            int requiredFixed = ps.Count(p => !p.IsOptional && !p.IsParams);
            int max = hasParams ? int.MaxValue : n;
            if (argCount < requiredFixed || argCount > max) continue; // not arity-compatible

            // Positional type-match score (skipped when named args break order).
            int typeScore = 0;
            if (!hasNamedArgs)
            {
                int fixedCount = n - (hasParams ? 1 : 0);
                for (int i = 0; i < argCount; i++)
                {
                    IParameterSymbol? target = i < fixedCount ? ps[i] : (hasParams ? ps[n - 1] : null);
                    if (target is null) continue;
                    var pt = (i >= fixedCount && target.Type is IArrayTypeSymbol arr) ? arr.ElementType : target.Type;
                    var at = model.GetTypeInfo(args[i].Expression).Type;
                    if (at is null || pt is null) continue;
                    var conv = model.Compilation.ClassifyConversion(at, pt);
                    if (conv.IsIdentity) typeScore += 2;
                    else if (conv.IsImplicit) typeScore += 1;
                }
            }

            int exactness = (argCount == n && !hasParams) ? 1 : 0; // exact fixed-arity match
            var key = (typeScore, exactness, -n);
            if (best is null || key.CompareTo(bestKey) > 0)
            {
                best = c;
                bestKey = key;
            }
        }

        // Preserve the legacy FirstOrDefault behavior when nothing is compatible.
        return best ?? methods[0];
    }

    // Parameter list of `c` as seen from the call site: for an un-reduced
    // extension method invoked instance-style, drop the leading `this`
    // parameter (via proper reduction when the receiver type binds, else by
    // dropping the first parameter). Reduced extensions and ordinary methods
    // are returned unchanged.
    private static ImmutableArray<IParameterSymbol> EffectiveParameters(
        IMethodSymbol c, bool instanceStyle, bool recvIsType, ITypeSymbol? recvType)
    {
        if (c.MethodKind == MethodKind.ReducedExtension)
            return c.Parameters;
        if (c.IsExtensionMethod && instanceStyle && !recvIsType)
        {
            if (recvType is not null && recvType is not IErrorTypeSymbol)
            {
                var reduced = c.ReduceExtensionMethod(recvType);
                if (reduced is not null) return reduced.Parameters;
            }
            if (c.Parameters.Length > 0) return c.Parameters.RemoveAt(0);
        }
        return c.Parameters;
    }

    // -- receiver-expression reachability (seam-injection-site scoping) ----
    //
    // Both transforms re-materialize the receiver expression at a point that is
    // NOT the original call site:
    //   * wrapper_interface emits `_field = param ?? new Wrapper(<recv>)` in the
    //     CONSTRUCTOR — so <recv> must reference no method-local and no parameter
    //     (only `this`/base, fields, properties, static members, or types).
    //   * parameterize_dependency emits the delegator at the TOP of the enclosing
    //     method — so <recv> may additionally reference a parameter of that
    //     enclosing method (and primary-constructor parameters, which are captured
    //     like fields), but never a local declared mid-body nor a parameter/local
    //     of a NESTED lambda or local function.
    // When the receiver root is out of scope at the injection site the emitted
    // code fails to compile (CS0103 "the name 'x' does not exist", or CS8820/
    // CS8821 inside a static anonymous function), which the build-preservation
    // guard then auto-reverts. Detecting it here lets the transform reject the
    // target cleanly instead of producing non-compiling code.

    // Identifier names in a receiver expression that denote VALUE roots. The
    // member-name side of a member/conditional access (`a.B` → `B`) denotes a
    // member lookup, not a binding root, and is skipped.
    private static IEnumerable<IdentifierNameSyntax> ReceiverRootIdentifiers(ExpressionSyntax expr)
    {
        foreach (var id in expr.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (id.Parent is MemberAccessExpressionSyntax ma && ma.Name == id) continue;
            if (id.Parent is MemberBindingExpressionSyntax mb && mb.Name == id) continue;
            yield return id;
        }
    }

    // wrapper_interface: receiver reachable from the constructor (no local / no
    // range variable). Constructor parameters (including primary-constructor
    // parameters) are allowed.
    public static bool ReceiverIsConstructorReachable(ExpressionSyntax expr, SemanticModel model)
    {
        foreach (var id in ReceiverRootIdentifiers(expr))
        {
            var sym = model.GetSymbolInfo(id).Symbol;
            if (sym is ILocalSymbol or IRangeVariableSymbol)
                return false;
            if (sym is IParameterSymbol p)
            {
                if (p.ContainingSymbol is IMethodSymbol pm && pm.MethodKind == MethodKind.Constructor)
                    continue;
                return false;
            }
        }
        return true;
    }

    // parameterize_dependency: receiver reachable from the TOP of `enclosing`.
    // Parameters of `enclosing` itself, and primary-constructor parameters of the
    // containing type, are reachable; locals and nested-function parameters are not.
    public static bool ReceiverReachableFromMethodTop(
        ExpressionSyntax expr, SemanticModel model, IMethodSymbol? enclosing)
    {
        foreach (var id in ReceiverRootIdentifiers(expr))
        {
            var sym = model.GetSymbolInfo(id).Symbol;
            if (sym is ILocalSymbol or IRangeVariableSymbol) return false;
            if (sym is IParameterSymbol p)
            {
                if (enclosing is not null
                    && SymbolEqualityComparer.Default.Equals(p.ContainingSymbol, enclosing))
                    continue;   // parameter of the method we overload
                if (p.ContainingSymbol is IMethodSymbol pm && pm.MethodKind == MethodKind.Constructor)
                    continue;   // primary-constructor parameter (captured like a field)
                return false;   // lambda / local-function parameter → out of scope
            }
        }
        return true;
    }

    // -- signature reconstruction from IMethodSymbol -----------------------

    public static string TypeParamList(IMethodSymbol m) =>
        m.TypeParameters.Length == 0
            ? ""
            : "<" + string.Join(", ", m.TypeParameters.Select(t => t.Name)) + ">";

    public static string Constraints(IMethodSymbol m)
    {
        var clauses = new List<string>();
        foreach (var tp in m.TypeParameters)
        {
            var cs = new List<string>();
            if (tp.HasUnmanagedTypeConstraint) cs.Add("unmanaged");
            else if (tp.HasValueTypeConstraint) cs.Add("struct");
            else if (tp.HasReferenceTypeConstraint) cs.Add("class");
            else if (tp.HasNotNullConstraint) cs.Add("notnull");
            foreach (var ct in tp.ConstraintTypes)
                cs.Add(ct.ToDisplayString(GeneratedTypeFormat));
            if (tp.HasConstructorConstraint) cs.Add("new()");
            if (cs.Count > 0) clauses.Add($"where {tp.Name} : {string.Join(", ", cs)}");
        }
        return clauses.Count == 0 ? "" : " " + string.Join(" ", clauses);
    }

    public static string ParamDecl(IParameterSymbol p, SymbolDisplayFormat fmt)
    {
        var sb = new StringBuilder();
        if (p.IsParams) sb.Append("params ");
        switch (p.RefKind)
        {
            case RefKind.Ref: sb.Append("ref "); break;
            case RefKind.Out: sb.Append("out "); break;
            case RefKind.In: sb.Append("in "); break;
        }
        sb.Append(p.Type.ToDisplayString(fmt)).Append(' ').Append(EscapeId(p.Name));
        if (p.HasExplicitDefaultValue)
            sb.Append(" = ").Append(FormatDefault(p));
        return sb.ToString();
    }

    public static string CallArg(IParameterSymbol p)
    {
        var pre = p.RefKind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            _ => "",
        };
        return pre + EscapeId(p.Name);
    }

    public static string ReturnTypeText(IMethodSymbol m, SymbolDisplayFormat fmt) =>
        m.ReturnsVoid ? "void" : m.ReturnType.ToDisplayString(fmt);

    private static string FormatDefault(IParameterSymbol p)
    {
        if (!p.HasExplicitDefaultValue) return "default";
        var v = p.ExplicitDefaultValue;
        if (v is null)
            return p.Type.IsReferenceType || p.Type.NullableAnnotation == NullableAnnotation.Annotated
                ? "null" : "default";
        return v switch
        {
            bool b => b ? "true" : "false",
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            char c => "'" + (c == '\'' ? "\\'" : c.ToString()) + "'",
            float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture) + "f",
            double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture) + "m",
            _ => Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? "default",
        };
    }

    public static string EscapeId(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
            || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;

    // -- wrapper interface + forwarder source (shared by both transforms) --

    public static string EmitWrapperSource(SeamContext ctx)
    {
        var fmt = GeneratedTypeFormat;
        var recvType = ctx.ReceiverType.ToDisplayString(fmt);
        var ret = ReturnTypeText(ctx.Method, fmt);
        var tp = TypeParamList(ctx.Method);
        var constraints = Constraints(ctx.Method);
        var paramDecls = string.Join(", ", ctx.Method.Parameters.Select(p => ParamDecl(p, fmt)));
        var callArgs = string.Join(", ", ctx.Method.Parameters.Select(CallArg));

        // Forwarder body. Extension methods are forwarded via their fully
        // qualified STATIC form so the generated file needs no `using` for the
        // extension's namespace (and never mis-resolves). Plain instance
        // members forward through the inner instance.
        string forwardExpr;
        if (ctx.Method.MethodKind == MethodKind.ReducedExtension && ctx.Method.ReducedFrom is { } unreduced)
        {
            var staticType = unreduced.ContainingType.ToDisplayString(fmt);
            var staticArgs = ctx.Method.Parameters.Length > 0 ? "_inner, " + callArgs : "_inner";
            forwardExpr = $"{staticType}.{ctx.Method.Name}{tp}({staticArgs})";
        }
        else
        {
            forwardExpr = $"_inner.{ctx.Method.Name}{tp}({callArgs})";
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by RoslynRefactorTool (phase-4 wrapper_interface seam).");
        sb.AppendLine("// Adapter over a statically-resolved / non-virtual member so it can be mocked.");
        sb.AppendLine("#nullable enable");
        // Generated seam type: suppress the "missing XML doc comment" warning so
        // repos that set <GenerateDocumentationFile> + warnings-as-errors (or an
        // .editorconfig that re-enables CS1591 for generated code) still build.
        sb.AppendLine("#pragma warning disable CS1591 // generated seam; XML docs intentionally omitted");
        if (ctx.Namespace.Length > 0)
        {
            sb.Append("namespace ").Append(ctx.Namespace).AppendLine(";");
            sb.AppendLine();
        }
        sb.Append("public interface ").AppendLine(ctx.InterfaceName);
        sb.AppendLine("{");
        sb.Append("    ").Append(ret).Append(' ').Append(ctx.Method.Name).Append(tp)
          .Append('(').Append(paramDecls).Append(')').Append(constraints).AppendLine(";");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.Append("public sealed class ").Append(ctx.WrapperName).Append(" : ").AppendLine(ctx.InterfaceName);
        sb.AppendLine("{");
        sb.Append("    private readonly ").Append(recvType).AppendLine(" _inner;");
        sb.Append("    public ").Append(ctx.WrapperName).Append('(').Append(recvType)
          .AppendLine(" inner) => _inner = inner;");
        sb.Append("    public ").Append(ret).Append(' ').Append(ctx.Method.Name).Append(tp)
          .Append('(').Append(paramDecls).Append(')').Append(constraints).AppendLine();
        sb.Append("        => ").Append(forwardExpr).AppendLine(";");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // -- seam descriptor (§0.4 / §4) ---------------------------------------

    public static string MemberSignature(SeamContext ctx)
    {
        var fmt = CleanTypeFormat;
        var ret = ReturnTypeText(ctx.Method, fmt);
        var tp = TypeParamList(ctx.Method);
        var paramTypes = string.Join(", ", ctx.Method.Parameters.Select(p =>
        {
            var pre = p.IsParams ? "params " : p.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => "",
            };
            return pre + p.Type.ToDisplayString(fmt);
        }));
        return $"{ret} {ctx.Method.Name}{tp}({paramTypes})";
    }

    public static Dictionary<string, object?> BuildSeam(
        SeamContext ctx, string injection, string injectionRef)
    {
        var siteLine = ctx.Invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        return new Dictionary<string, object?>
        {
            ["kind"] = ctx.Transform,
            ["interface"] = ctx.Fqn(ctx.InterfaceName),
            ["wrapper"] = ctx.Fqn(ctx.WrapperName),
            ["member"] = ctx.Method.Name,
            ["member_signature"] = MemberSignature(ctx),
            ["injection"] = injection,
            ["injection_ref"] = injectionRef,
            ["containing_type"] = ctx.ContainingTypeSymbol.ToDisplayString(),
            ["call_site"] = $"{ctx.TargetFileAbs}:{siteLine}",
        };
    }

    public static string GeneratedFilePath(SeamContext ctx)
    {
        var dir = Path.GetDirectoryName(ctx.TargetFileAbs)!;
        return Path.Combine(dir, ctx.InterfaceName + ".cs");
    }

    // -- analyzer-safe injection formatting (SA1137 / SA1505 / SA1028) -----
    //
    // Injected members/statements must EOL- and indent-match the surrounding
    // production file or StyleCop-strict repos (jellyfin/orleans/aspnetcore…)
    // reject the build with SA1137 ("elements should have the same indentation")
    // and SA1505 ("opening brace should not be followed by a blank line").
    // `NormalizeWhitespace` only formats relative to column 0, so a member
    // re-inserted at depth N is under-indented; we re-indent it to its real
    // nesting level here.

    // Dominant newline of the production file (\r\n vs \n).
    public static string DetectEol(string text)
    {
        int crlf = 0, lf = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                if (i > 0 && text[i - 1] == '\r') crlf++; else lf++;
            }
        }
        if (crlf > lf) return "\r\n";
        if (lf > 0) return "\n";
        return Environment.NewLine;
    }

    // Leading horizontal whitespace on the line a node begins on (its indent).
    public static string LineIndentOf(SyntaxNode node)
    {
        var lt = node.GetLeadingTrivia();
        for (int i = lt.Count - 1; i >= 0; i--)
        {
            if (lt[i].IsKind(SyntaxKind.WhitespaceTrivia)) return lt[i].ToString();
            if (lt[i].IsKind(SyntaxKind.EndOfLineTrivia)) return "";
        }
        return "";
    }

    // One indentation step used by this type's members (tab or N spaces),
    // derived from the gap between the type declaration and its first member.
    // Falls back to matching the type's own indent style (tab vs 4 spaces).
    public static string IndentUnitOf(TypeDeclarationSyntax type)
    {
        var typeIndent = LineIndentOf(type);
        foreach (var m in type.Members)
        {
            var mi = LineIndentOf(m);
            if (mi.Length > typeIndent.Length && mi.StartsWith(typeIndent, StringComparison.Ordinal))
                return mi.Substring(typeIndent.Length);
        }
        return typeIndent.StartsWith("\t", StringComparison.Ordinal) ? "\t" : "    ";
    }

    // Re-indent a node whose text was produced by NormalizeWhitespace (column-0
    // relative) so every line AFTER the first carries `baseIndent`. The first
    // line is positioned by the caller via leading trivia. Blank lines are left
    // truly empty (no trailing whitespace → avoids SA1028).
    public static string Reindent(string normalized, string baseIndent, string eol)
    {
        var parts = normalized.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();
        for (int i = 0; i < parts.Length; i++)
        {
            if (i > 0) sb.Append(eol);
            if (i == 0) { sb.Append(parts[i]); continue; }
            if (parts[i].Length == 0) continue;          // keep blank lines blank
            sb.Append(baseIndent).Append(parts[i]);
        }
        return sb.ToString();
    }

    // Format a freshly-built member declaration at `baseIndent` nesting using
    // the file's `eol` and `indentUnit`, returning a node whose ToFullString is
    // indentation-correct for re-insertion. `leading`/`trailing` trivia are
    // applied verbatim (caller supplies blank-line/doc framing).
    public static MemberDeclarationSyntax FormatMember(
        MemberDeclarationSyntax member, string baseIndent, string indentUnit, string eol,
        SyntaxTriviaList leading, SyntaxTriviaList trailing)
    {
        var normalized = member
            .WithLeadingTrivia()
            .WithTrailingTrivia()
            .NormalizeWhitespace(indentUnit, eol)
            .ToFullString();
        var reindented = Reindent(normalized, baseIndent, eol);
        return SyntaxFactory.ParseMemberDeclaration(reindented)!
            .WithLeadingTrivia(leading)
            .WithTrailingTrivia(trailing);
    }

    // -- containing-type kind classification (§5 shared guards) ------------
    //
    // Returns a §5 reason token if the containing type is structurally rejected
    // for the given transform, else null.
    public static string? ClassifyContainingType(SeamContext ctx)
    {
        var decl = ctx.ContainingType;

        if (decl is StructDeclarationSyntax)
            return "struct_type";
        if (decl is RecordDeclarationSyntax rec)
        {
            // record struct → struct semantics
            if (rec.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword))
                return "struct_type";
            // record class: wrapper rejects (positional/primary-ctor semantics),
            // parameterize handles ordinary methods.
            if (ctx.Transform == "wrapper_interface")
                return "record_type";
        }
        // Primary constructor is handled by converting to explicit constructor
        // in WrapperInterfaceRewriter, so no rejection here.

        return null;
    }

    /// <summary>
    /// Validates that a method can be made virtual for the subclass-and-override seam pattern.
    /// Returns a reason token (§5 format) if the method is rejected, null if applicable.
    /// </summary>
    /// <remarks>
    /// Rejects:
    /// - Methods in interface, struct, or record types (not inheritable)
    /// - Sealed classes (can't be subclassed)
    /// - Static methods (no instance to override)
    /// - Private methods (`private virtual` is illegal in C#)
    /// - Already virtual/abstract/sealed/override methods (already seamed)
    /// - Non-method syntax nodes (e.g. lambdas, local functions)
    /// </remarks>
    public static string? ValidateMethodForMakeVirtual(IMethodSymbol methodDef, TypeDeclarationSyntax? decl, INamedTypeSymbol? owner)
    {
        if (owner?.TypeKind == TypeKind.Interface)
            return "interface_member";
        if (owner?.TypeKind == TypeKind.Struct)
            return "struct_type";
        if (owner?.IsRecord == true)
            return "record_type";
        if (owner?.IsSealed == true)
            return "sealed_class";

        if (methodDef.IsStatic)
            return "static_method";
        if (methodDef.DeclaredAccessibility == Accessibility.Private)
            return "private_member";
        if (methodDef.IsAbstract)
            return "already_abstract";
        if (methodDef.IsVirtual)
            return "already_virtual";
        if (methodDef.IsSealed)
            return "already_sealed";
        if (methodDef.IsOverride)
            return "already_override";

        if (decl?.Modifiers.Any(SyntaxKind.PartialKeyword) == true)
            return "partial_method";
        if (decl?.Modifiers.Count == 0)
            return "private_member";

        return null;
    }

    /// <summary>
    /// Detects whether a type is from an external/framework assembly (no source available).
    /// Used to identify candidates for static utility wrapping.
    /// </summary>
    public static bool IsExternalType(ITypeSymbol type)
    {
        if (type == null) return false;
        var asmName = type.ContainingAssembly?.Name;
        if (asmName == null) return false;

        // Framework/system assembly patterns
        var frameworkPrefixes = new[]
        {
            "System.",
            "Microsoft.",
            "netstandard",
        };

        return frameworkPrefixes.Any(prefix => 
            asmName.Equals(prefix.TrimEnd('.'), StringComparison.OrdinalIgnoreCase) ||
            asmName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a call site can be handled via static utility wrapping pattern.
    /// This applies when the receiver is an external framework type that can't be
    /// injected directly, but we can create a wrapper that instantiates it.
    /// </summary>
    public static bool CanWrapAsStaticUtility(SeamContext ctx)
    {
        if (ctx?.ReceiverType == null) return false;
        if (!IsExternalType(ctx.ReceiverType)) return false;
        if (ctx.Method == null) return false;

        // Check for methods with problematic signatures
        if (ctx.Method.Parameters.Length > 10) return false;  // too many params to wrap
        
        // Generic methods with constraints are fine
        // Out/ref are fine, but could add restrictions if needed
        
        return true;
    }

    /// <summary>
    /// Generate wrapper source for a static utility (external framework type).
    /// The wrapper's constructor creates the inner instance rather than accepting it.
    /// </summary>
    public static string EmitStaticUtilityWrapperSource(SeamContext ctx)
    {
        var fmt = GeneratedTypeFormat;
        var recvType = ctx.ReceiverType.ToDisplayString(fmt);
        var ret = ReturnTypeText(ctx.Method, fmt);
        var tp = TypeParamList(ctx.Method);
        var constraints = Constraints(ctx.Method);
        var paramDecls = string.Join(", ", ctx.Method.Parameters.Select(p => ParamDecl(p, fmt)));
        var callArgs = string.Join(", ", ctx.Method.Parameters.Select(CallArg));

        // Determine instantiation strategy based on receiver type
        string innerCreation = DetermineInnerCreation(ctx.ReceiverType);

        // Forward to _inner
        var forwardExpr = $"_inner.{ctx.Method.Name}{tp}({callArgs})";
        
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Generated by RoslynRefactorTool (phase-4 static_utility_wrapper seam).");
        sb.AppendLine("// Wraps an external framework type so it can be mocked in tests.");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("#pragma warning disable CS1591 // generated seam; XML docs intentionally omitted");
        
        if (ctx.Namespace.Length > 0)
        {
            sb.Append("namespace ").Append(ctx.Namespace).AppendLine(";");
            sb.AppendLine();
        }

        // Interface definition
        sb.Append("public interface ").AppendLine(ctx.InterfaceName);
        sb.AppendLine("{");
        sb.Append("    ").Append(ret).Append(' ').Append(ctx.Method.Name).Append(tp)
          .Append('(').Append(paramDecls).Append(')').Append(constraints).AppendLine(";");
        sb.AppendLine("}");
        sb.AppendLine();

        // Wrapper class definition
        sb.Append("public sealed class ").Append(ctx.WrapperName).Append(" : ").AppendLine(ctx.InterfaceName);
        sb.AppendLine("{");
        sb.Append("    private readonly ").Append(recvType).AppendLine(" _inner;");
        
        // Constructor that creates _inner
        sb.Append("    public ").Append(ctx.WrapperName).Append("()").AppendLine();
        sb.Append("        => _inner = ").Append(innerCreation).AppendLine(";");
        
        // Forward method
        sb.Append("    public ").Append(ret).Append(' ').Append(ctx.Method.Name).Append(tp)
          .Append('(').Append(paramDecls).Append(')').Append(constraints).AppendLine();
        sb.Append("        => ").Append(forwardExpr).AppendLine(";");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    /// <summary>
    /// Determine how to instantiate the inner framework type.
    /// Returns the instantiation expression (e.g., "new System.Net.Http.HttpClient()").
    /// </summary>
    private static string DetermineInnerCreation(ITypeSymbol receiverType)
    {
        var typeName = receiverType.ToDisplayString(GeneratedTypeFormat);
        
        // For known framework types, use parameterless constructor
        // This handles: HttpClient, HttpClientHandler, etc.
        // Could be extended with special logic for types with no parameterless ctor
        
        return $"new {typeName}()";
    }
}

