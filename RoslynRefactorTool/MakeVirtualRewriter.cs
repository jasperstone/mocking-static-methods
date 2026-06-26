using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynRefactorTool;

// Phase-4 `make_virtual` transform (TRANSFORM_CONTRACT §1) — Roslyn-based.
//
// Intent: add the `virtual` modifier to a NON-virtual instance method that is
// DECLARED in the owning production project, so a test can subclass-and-override
// it. The test seam is subclass-and-override; there is NO wrapper/parameter
// seam, so the emitted `seam` is intentionally `{}` and via_seam attribution is
// None (do NOT invent a seam descriptor).
//
// Unlike wrapper_interface / parameterize_dependency, the target is a method
// DECLARATION, not a call-site rewrite. We locate the declaring
// MethodDeclarationSyntax SEMANTICALLY (not regex): bind the invocation at the
// target file+line and follow it to its declaration (handles cross-file decls);
// if the site has no bindable invocation (e.g. the target line is the
// declaration itself) we fall back to a unique declaration-by-name lookup in the
// owning project. Applicability is checked against the semantic model, then
// `virtual` is added with `WithModifiers`, preserving leading doc comments,
// attributes, indentation, and EOL trivia.
//
// PURE: returns proposed source text only; Python owns all writes + the
// behavior-preservation build + auto-revert.
internal static class MakeVirtualRewriter
{
    public static RewriteResult Apply(
        CSharpCompilation compilation,
        string targetFileAbs,
        int line,
        string method)
    {
        // -- 1. locate the call-site tree --------------------------------------
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
            return RewriteResult.Reject(
                "site_not_found: target file not part of the owning project compilation");

        var root = tree.GetRoot();
        var model = compilation.GetSemanticModel(tree);

        // -- 2. resolve the declaring method + its symbol ----------------------
        MethodDeclarationSyntax? methodDecl;
        IMethodSymbol? methodDef;

        var methodSym = BindInvocation(root, model, line, method);
        if (methodSym is not null)
        {
            // Follow the (possibly constructed) call-site symbol to its ORIGINAL
            // (uninstantiated) declaration.
            methodDef = methodSym.OriginalDefinition;

            var declRefs = methodDef.DeclaringSyntaxReferences;
            if (declRefs.Length == 0)
                return RewriteResult.Reject(
                    $"not_in_owning_project: '{method}' is declared in a framework/external "
                    + "type (no in-repo declaration); use wrapper_interface to introduce a "
                    + "mockable seam.");
            if (declRefs.Length > 1)
                return RewriteResult.Reject("partial_method");
            if (declRefs[0].GetSyntax() is not MethodDeclarationSyntax decl)
                return RewriteResult.Reject(
                    "not_a_method_declaration: target is not an ordinary method declaration");
            methodDecl = decl;
        }
        else
        {
            // No bindable invocation at the site → declaration-by-name fallback
            // (target line is the declaration itself, or the call could not bind).
            (methodDecl, var declModel) = LocateDeclarationByName(compilation, tree, method);
            if (methodDecl is null || declModel is null)
                return RewriteResult.Reject("site_not_found");
            methodDef = declModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
            if (methodDef is null)
                return RewriteResult.Reject("unbound_method");
        }

        if (methodDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return RewriteResult.Reject("partial_method");

        // -- 3. applicability via the semantic model (TRANSFORM_CONTRACT §1) ---
        var owner = methodDef.ContainingType;

        // Containing-type kind: subclass-and-override needs an inheritable class.
        if (owner.TypeKind == TypeKind.Interface)
            return RewriteResult.Reject("interface_member");
        if (owner.TypeKind == TypeKind.Struct)
            return RewriteResult.Reject("struct_type");
        if (owner.IsRecord)
            return RewriteResult.Reject("record_type");

        // Static method → no instance to override.
        if (methodDef.IsStatic)
            return RewriteResult.Reject("static_method");

        // `private virtual` is illegal (CS0621); reject before the build catches it.
        if (methodDef.DeclaredAccessibility == Accessibility.Private)
            return RewriteResult.Reject("private_member");

        // Already overridable / non-virtualizable members.
        if (methodDef.IsAbstract)
            return RewriteResult.Reject("already_abstract");
        if (methodDef.IsVirtual)
            return RewriteResult.Reject("already_virtual");
        if (methodDef.IsSealed)
            return RewriteResult.Reject("already_sealed");
        if (methodDef.IsOverride)
            return RewriteResult.Reject("already_override");

        // Sealed class cannot be subclassed → nothing to override.
        if (owner.IsSealed)
            return RewriteResult.Reject("sealed_class");

        // A non-private method always carries an access modifier we can sit
        // after; a modifier-less declaration is private (rejected above).
        if (methodDecl.Modifiers.Count == 0)
            return RewriteResult.Reject("private_member");

        // -- 4. add `virtual`, preserving all trivia ---------------------------
        // Append `virtual` after the existing modifiers (e.g. `public virtual`).
        // The first modifier keeps its leading trivia (indent + doc comments +
        // attributes), so only the modifier list — one token — changes.
        var virtualToken = SyntaxFactory.Token(SyntaxKind.VirtualKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);
        var newMethodDecl = methodDecl.WithModifiers(methodDecl.Modifiers.Add(virtualToken));

        var declTree = methodDecl.SyntaxTree;
        var newRoot = declTree.GetRoot().ReplaceNode(methodDecl, newMethodDecl);
        var declFileAbs = Path.GetFullPath(declTree.FilePath);
        var declLine = methodDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        var files = new Dictionary<string, string>
        {
            [declFileAbs] = newRoot.ToFullString(),
        };
        var reason = $"make_virtual applied: added `virtual` to {owner.Name}.{method} "
                   + $"at {Path.GetFileName(declFileAbs)}:{declLine}.";

        // make_virtual's seam is subclass-and-override — NO wrapper/param seam
        // descriptor (TRANSFORM_CONTRACT: empty {} for make_virtual).
        return RewriteResult.Ok(reason, files, new Dictionary<string, object?>());
    }

    // -- locate helpers ----------------------------------------------------

    // Bind the invocation of `method` near `line` to its IMethodSymbol. Returns
    // null if no same-named invocation is found at/near the line (and the name
    // is not unique in the file) or the symbol cannot be resolved.
    private static IMethodSymbol? BindInvocation(
        SyntaxNode root, SemanticModel model, int line, string method)
    {
        InvocationExpressionSyntax? hit = null;
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (SeamCore.InvokedName(inv) != method) continue;
            var startLine = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            if (startLine == line || SeamCore.NameLine(inv) == line) { hit = inv; break; }
        }
        if (hit is null)
        {
            // Line drift tolerance: a single same-named invocation in the file.
            var byName = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(i => SeamCore.InvokedName(i) == method).ToList();
            if (byName.Count == 1) hit = byName[0];
        }
        if (hit is null) return null;

        var symInfo = model.GetSymbolInfo(hit);
        return symInfo.Symbol as IMethodSymbol
               ?? SeamCore.PickBestCandidate(
                   hit, symInfo.CandidateSymbols.OfType<IMethodSymbol>().ToImmutableArray(), model);
    }

    // Fallback locate: a UNIQUE method declaration named `method`, preferring the
    // target file, then the whole owning-project compilation. Returns (null,null)
    // if absent or ambiguous (an overload set cannot be disambiguated without a
    // call site).
    private static (MethodDeclarationSyntax?, SemanticModel?) LocateDeclarationByName(
        CSharpCompilation comp, SyntaxTree preferredTree, string method)
    {
        var inPreferred = preferredTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text == method).ToList();
        if (inPreferred.Count == 1)
            return (inPreferred[0], comp.GetSemanticModel(preferredTree));
        if (inPreferred.Count > 1)
            return (null, null); // ambiguous in-file overloads → need a call site

        MethodDeclarationSyntax? found = null;
        SyntaxTree? foundTree = null;
        int count = 0;
        foreach (var t in comp.SyntaxTrees)
        {
            foreach (var m in t.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (m.Identifier.Text != method) continue;
                count++;
                if (found is null) { found = m; foundTree = t; }
            }
        }
        return count == 1 ? (found, comp.GetSemanticModel(foundTree!)) : (null, null);
    }


}
