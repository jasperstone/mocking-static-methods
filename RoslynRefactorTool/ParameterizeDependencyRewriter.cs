using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynRefactorTool;

// DISTINCT rewrite path (TRANSFORM_CONTRACT §3):
//   two-method overload-delegation. The original method keeps its exact
//   signature and becomes a one-line delegator passing `new Wrapper(recv)`; a
//   NEW overload appends a trailing `{param_type} {param_name}` parameter and
//   holds the real body with the call-site receiver swapped for the parameter.
// Edge cases per §5 are rejected with the exact reason tokens.
internal static class ParameterizeDependencyRewriter
{
    public static RewriteResult Apply(SeamContext ctx)
    {
        // struct/record-struct → reject (record class is allowed for ordinary methods)
        if (ctx.ContainingType is StructDeclarationSyntax)
            return RewriteResult.Reject("struct_type");
        if (ctx.ContainingType is RecordDeclarationSyntax rec
            && rec.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword))
            return RewriteResult.Reject("struct_type");

        // Receiver must not be `this`/implicit.
        if (ctx.ReceiverExpr is null || ctx.ReceiverExpr is ThisExpressionSyntax)
            return RewriteResult.Reject("receiver_is_this");

        // Receiver must be resolvable at the top of the original method:
        // a field/property of the type, or a parameter of the method. A local
        // computed mid-body cannot be reconstructed by the delegator.
        if (ctx.ReceiverSymbol is not (IFieldSymbol or IPropertySymbol or IParameterSymbol))
            return RewriteResult.Reject("receiver_not_in_method_scope");

        // A *parameter* receiver must be reachable from the top of the enclosing
        // method. Parameters of the enclosing method and PRIMARY-CONSTRUCTOR
        // parameters of the containing type (captured, accessible in every
        // instance member — like fields) are fine. But lambda / local-function
        // parameters (e.g. `sp` in `services.TryAddScoped(sp => sp.GetService<T>())`)
        // exist only inside the nested function; the delegator, which runs at the
        // top of the enclosing method, cannot reference them → reject cleanly.
        if (ctx.ReceiverSymbol is IParameterSymbol prm
            && prm.ContainingSymbol is IMethodSymbol owner
            && owner.MethodKind is MethodKind.LambdaMethod
                                or MethodKind.AnonymousFunction
                                or MethodKind.LocalFunction)
            return RewriteResult.Reject("receiver_not_in_method_scope");

        var method = ctx.EnclosingMethod;
        if (method is null)
            return RewriteResult.Reject("site_not_found: no enclosing method to overload");

        // The receiver EXPRESSION must be reachable from the TOP of the method
        // we overload, because the delegator emits `M(args, new Wrapper(<recv>))`
        // there. A receiver rooted in a LOCAL declared mid-body (e.g.
        // `appHost.Services` where `appHost` is a local) or in a NESTED lambda /
        // local-function parameter (e.g. `httpContext.RequestServices` inside a
        // `static httpContext => …`) is out of scope at the method top → the
        // delegator fails to compile (CS0103, or CS8820/CS8821 in a static
        // anonymous function). Reject cleanly instead of emitting broken code.
        if (ctx.ReceiverExpr is not null)
        {
            var enclosingSym = ctx.Model.GetDeclaredSymbol(method) as IMethodSymbol;
            if (!SeamCore.ReceiverReachableFromMethodTop(ctx.ReceiverExpr, ctx.Model, enclosingSym))
                return RewriteResult.Reject("receiver_not_in_method_scope");
        }

        // A `__arglist` parameter must remain the last parameter and cannot be
        // forwarded positionally by the delegator → no legal overload shape (§5).
        if (method.ParameterList.ToString().Contains("__arglist"))
            return RewriteResult.Reject("trailing_params_conflict");

        // Primary-constructor body region is not a method; ordinary methods are fine.
        var wrapper = ctx.WrapperName;
        var paramType = ctx.InterfaceName;   // §1.2 — the mockable seam type
        var paramName = ctx.ParamName;
        var recvText = ctx.ReceiverText;
        var seamMember = ctx.Method.Name;          // call site member (e.g. GetAsync)
        var enclosingName = method.Identifier.Text; // method we overload (e.g. FetchAsync)

        // -- new overload: original + trailing param; body uses the parameter
        var bodyRewriter = new SameReceiverCallRewriter(seamMember, recvText, paramName, ctx.Model, ctx.BoundMethod);
        MethodDeclarationSyntax overload = method;

        if (method.Body is { } block)
        {
            var newBlock = (BlockSyntax)bodyRewriter.Visit(block)!;
            overload = method.WithBody(newBlock).WithExpressionBody(null).WithSemicolonToken(default);
        }
        else if (method.ExpressionBody is { } exprBody)
        {
            var newExpr = (ExpressionSyntax)bodyRewriter.Visit(exprBody.Expression)!;
            overload = method.WithExpressionBody(exprBody.WithExpression(newExpr));
        }
        else
        {
            return RewriteResult.Reject("site_not_found: target method has no body");
        }
        if (bodyRewriter.Rewrites == 0)
            return RewriteResult.Reject("site_not_found: no rewritable call site in the method body");

        var newParam = SyntaxFactory.ParseParameterList($"({paramType} {paramName})").Parameters[0];

        // BUG #1 fix (CS1737): the injected dependency parameter is REQUIRED, so
        // it must be inserted after the last required parameter — i.e. BEFORE the
        // trailing optional group and BEFORE any `params` array — otherwise
        // appending it last produces an illegal signature when the original
        // method already ends in an optional/`params` parameter. The result
        // (required…, dep, optional…, params) is always legal C#.
        var origParams = method.ParameterList.Parameters;
        int insertIdx = origParams.Count;
        for (int i = 0; i < origParams.Count; i++)
        {
            bool isOptional = origParams[i].Default is not null;
            bool isParams = origParams[i].Modifiers.Any(SyntaxKind.ParamsKeyword);
            if (isOptional || isParams) { insertIdx = i; break; }
        }
        overload = overload
            .WithParameterList(overload.ParameterList.WithParameters(
                overload.ParameterList.Parameters.Insert(insertIdx, newParam)))
            .WithAttributeLists(method.AttributeLists);

        // The new overload is a BRAND-NEW method, not part of the type's
        // inheritance chain: it overrides/hides nothing. Carrying `override`
        // (or `virtual`/`abstract`/`sealed`/`new`) from the original method
        // makes it illegal — there is no base member with the augmented
        // signature to override (CS0115) — so strip those inheritance modifiers.
        // Access/async/static/etc. are preserved. (The delegator keeps the
        // original signature, so it legitimately retains `override`.)
        var overloadMods = SyntaxFactory.TokenList(
            overload.Modifiers.Where(m =>
                !m.IsKind(SyntaxKind.OverrideKeyword)
                && !m.IsKind(SyntaxKind.VirtualKeyword)
                && !m.IsKind(SyntaxKind.AbstractKeyword)
                && !m.IsKind(SyntaxKind.SealedKeyword)
                && !m.IsKind(SyntaxKind.NewKeyword)));
        overload = overload.WithModifiers(overloadMods);

        // -- delegator: original signature byte-for-byte → calls the overload
        var callArgs = method.ParameterList.Parameters
            .Select(p =>
            {
                var pre = p.Modifiers.Any(SyntaxKind.RefKeyword) ? "ref "
                        : p.Modifiers.Any(SyntaxKind.OutKeyword) ? "out "
                        : p.Modifiers.Any(SyntaxKind.InKeyword) ? "in " : "";
                return pre + SeamCore.EscapeId(p.Identifier.Text);
            })
            .ToList();
        // Insert the dependency argument at the SAME position the parameter was
        // inserted into the overload, so the positional call binds correctly.
        callArgs.Insert(insertIdx, $"new {wrapper}({recvText})");

        // If the method we overload is GENERIC, the delegator must pass its own
        // type parameters explicitly: the injected dependency argument carries no
        // information about the method type parameters, so overload resolution
        // cannot infer them from arguments alone (CS0411). Forwarding
        // `M<T1,…>(args, dep)` binds the generic overload unambiguously.
        var typeArgs = method.TypeParameterList is { Parameters.Count: > 0 } tpl
            ? "<" + string.Join(", ", tpl.Parameters.Select(p => p.Identifier.Text)) + ">"
            : "";
        var delegateCall = $"{enclosingName}{typeArgs}({string.Join(", ", callArgs)})";

        // Delegator is never async; it just forwards/returns the overload result.
        var delegatorMods = SyntaxFactory.TokenList(
            method.Modifiers.Where(m => !m.IsKind(SyntaxKind.AsyncKeyword)));

        MethodDeclarationSyntax delegator;
        if (ctx.Method.ReturnsVoid && !method.Modifiers.Any(SyntaxKind.AsyncKeyword)
            && method.ReturnType is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            // void → statement-bodied delegator: `{ M(args, new Wrapper(recv)); }`
            var stmt = SyntaxFactory.ParseStatement($"{delegateCall};\n");
            delegator = method
                .WithModifiers(delegatorMods)
                .WithBody(SyntaxFactory.Block(stmt))
                .WithExpressionBody(null)
                .WithSemicolonToken(default);
        }
        else
        {
            // value-returning / Task-returning → expression-bodied delegator.
            var arrow = SyntaxFactory.ArrowExpressionClause(
                SyntaxFactory.ParseExpression(delegateCall));
            delegator = method
                .WithModifiers(delegatorMods)
                .WithBody(null)
                .WithExpressionBody(arrow)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        // -- replace the original method with [delegator, overload] --------
        // EOL/indent-match the production file so the two emitted methods sit at
        // the method's real nesting depth. NormalizeWhitespace alone formats
        // relative to column 0, under-indenting the bodies → SA1137 in strict
        // repos; SeamCore.FormatMember re-indents to the method's own indent.
        var root = ctx.Tree.GetRoot();
        var fileText = ctx.Tree.GetText().ToString();
        var eol = SeamCore.DetectEol(fileText);
        var indentUnit = SeamCore.IndentUnitOf(ctx.ContainingType);
        var methodIndent = SeamCore.LineIndentOf(method);
        var leadingTrivia = method.GetLeadingTrivia();   // preserves doc + indent on both

        var delegatorNode = SeamCore.FormatMember(
            delegator, methodIndent, indentUnit, eol,
            leading: leadingTrivia,
            trailing: SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(eol)));
        var overloadNode = SeamCore.FormatMember(
            overload, methodIndent, indentUnit, eol,
            leading: leadingTrivia,
            trailing: method.GetTrailingTrivia());
        var newRoot = root.ReplaceNode(method, new SyntaxNode[] { delegatorNode, overloadNode });

        var files = new Dictionary<string, string>
        {
            [ctx.TargetFileAbs] = newRoot.ToFullString(),
            [SeamCore.GeneratedFilePath(ctx)] = SeamCore.EmitWrapperSource(ctx),
        };

        // injection_ref = the overload signature the test must call.
        var overloadParamTypes = method.ParameterList.Parameters
            .Select(p => p.Type?.ToString() ?? "object").ToList();
        overloadParamTypes.Insert(insertIdx, paramType);
        var injectionRef = $"{enclosingName}({string.Join(", ", overloadParamTypes)})";

        var seam = SeamCore.BuildSeam(ctx, injection: "overload", injectionRef: injectionRef);
        var reason = $"parameterize_dependency applied: added overload "
                   + $"{injectionRef} delegating from the original signature on "
                   + $"{ctx.ContainingTypeSymbol.Name}.";
        return RewriteResult.Ok(reason, files, seam);
    }
}
