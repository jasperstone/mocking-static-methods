using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynRefactorTool;

// DISTINCT rewrite path (TRANSFORM_CONTRACT §2):
//   * inject the generated interface into the containing type via the ctor
//     (trailing optional `{iface}? {param} = null`),
//   * append the field assignment LAST (`_field = param ?? new Wrapper(recv);`),
//   * add the backing field,
//   * rewrite ALL same-receiver invocations of the target member in the type to
//     the injected field.
// Edge cases per §5 are rejected with the exact reason tokens.
internal static class WrapperInterfaceRewriter
{
    public static RewriteResult Apply(SeamContext ctx)
    {
        // -- §5 structural guards ------------------------------------------
        var classify = SeamCore.ClassifyContainingType(ctx);
        if (classify is not null) return RewriteResult.Reject(classify);

        if (ctx.ContainingType is not ClassDeclarationSyntax classDecl)
            return RewriteResult.Reject("site_not_found: containing type is not a class");

        // Receiver must be `this`/implicit → reject.
        if (ctx.ReceiverExpr is null || ctx.ReceiverExpr is ThisExpressionSyntax)
            return RewriteResult.Reject("receiver_is_this");

        // Receiver must be a field/property the ctor can re-reference.
        if (ctx.ReceiverSymbol is not (IFieldSymbol or IPropertySymbol))
            return RewriteResult.Reject("no_receiver_source");

        // The receiver EXPRESSION (not just its final member) must be reachable
        // from the CONSTRUCTOR, where `_field = param ?? new Wrapper(<recv>)` is
        // emitted. A receiver rooted in a method parameter / local / lambda
        // parameter (e.g. `workerContext.ServiceProvider`, `httpContext.Request-
        // Services`) is out of scope in the ctor → the emitted ctor fails to
        // compile (CS0103). Reject cleanly rather than emit non-compiling code.
        if (ctx.ReceiverExpr is not null
            && !SeamCore.ReceiverIsConstructorReachable(ctx.ReceiverExpr, ctx.Model))
            return RewriteResult.Reject("receiver_not_ctor_reachable");

        // Site inside a static method → no instance to hold the field.
        if (ctx.EnclosingMethod is not null
            && ctx.EnclosingMethod.Modifiers.Any(SyntaxKind.StaticKeyword))
            return RewriteResult.Reject("static_method_no_instance");

        // -- constructor analysis (via symbol, partial-aware) --------------
        var userCtors = ctx.ContainingTypeSymbol.InstanceConstructors
            .Where(c => !c.IsImplicitlyDeclared)
            .ToList();

        if (userCtors.Count > 1)
            return RewriteResult.Reject("multiple_ctors");

        ConstructorDeclarationSyntax? ctorDecl = null;
        if (userCtors.Count == 1)
        {
            var ctorSym = userCtors[0];
            var syntaxRef = ctorSym.DeclaringSyntaxReferences.FirstOrDefault();
            var ctorNode = syntaxRef?.GetSyntax() as ConstructorDeclarationSyntax;
            if (ctorNode is null)
                return RewriteResult.Reject("site_not_found: constructor syntax unavailable");

            // Ctor in a different file than the site (partial split).
            if (Path.GetFullPath(ctorNode.SyntaxTree.FilePath).Replace('\\', '/')
                != ctx.TargetFileAbs.Replace('\\', '/'))
                return RewriteResult.Reject("partial_split");

            // `: this(...)` chaining → reject.
            if (ctorNode.Initializer is { } init
                && init.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword))
                return RewriteResult.Reject("ctor_chaining");

            ctorDecl = ctorNode;
        }

        var iface = ctx.InterfaceName;
        var wrapper = ctx.WrapperName;
        var param = ctx.ParamName;
        var field = ctx.FieldName;
        var recvText = ctx.ReceiverText;
        var method = ctx.Method.Name;

        // -- 1. rewrite all same-receiver call sites in the class ----------
        var siteRewriter = new SameReceiverCallRewriter(method, recvText, field, ctx.Model, ctx.BoundMethod);
        var newClass = (ClassDeclarationSyntax)siteRewriter.Visit(classDecl)!;
        if (siteRewriter.Rewrites == 0)
            return RewriteResult.Reject("site_not_found: no rewritable call site for the target member");

        // -- file formatting context (SA1137 / SA1505 / SA1028) -----------
        // Match the production file's newline + indentation so injected members
        // sit at the correct nesting depth (NormalizeWhitespace only formats
        // relative to column 0, which is what tripped SA1137 in strict repos).
        var fileText = ctx.Tree.GetText().ToString();
        var eol = SeamCore.DetectEol(fileText);
        var indentUnit = SeamCore.IndentUnitOf(classDecl);
        var classIndent = SeamCore.LineIndentOf(classDecl);
        var memberIndent = classIndent + indentUnit;

        // -- 2. constructor: add trailing optional param + append assignment
        // Respect the nullable ANNOTATION context at the injection site: a file
        // (or region) under `#nullable disable` cannot carry a `?` annotation
        // (CS8632). Emit the optional param's `?` only where annotations are
        // enabled; `{iface} {param} = null` is legal (no warning) when disabled.
        var nullablePos = (ctorDecl ?? (SyntaxNode)ctx.ContainingType).SpanStart;
        bool annotationsEnabled =
            (ctx.Model.GetNullableContext(nullablePos) & NullableContext.AnnotationsEnabled) != 0;
        var optMark = annotationsEnabled ? "?" : "";

        if (ctorDecl is not null)
        {
            // re-find the (possibly rewritten) ctor node inside newClass by span match on identifier+param count
            var liveCtor = newClass.Members.OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault(c => c.ParameterList.Parameters.Count == ctorDecl.ParameterList.Parameters.Count
                                     && c.Identifier.Text == ctorDecl.Identifier.Text)
                ?? newClass.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (liveCtor is null)
                return RewriteResult.Reject("site_not_found: constructor lost during rewrite");

            // The ctor's real nesting indent in the file — re-indent target.
            var ctorIndent = SeamCore.LineIndentOf(liveCtor);

            var assignStmt = (StatementSyntax)SyntaxFactory.ParseStatement(
                $"{field} = {param} ?? new {wrapper}({recvText});");

            var newParam = SyntaxFactory.ParseParameterList($"({iface}{optMark} {param} = null)").Parameters[0];
            var withParam = liveCtor.WithParameterList(liveCtor.ParameterList.AddParameters(newParam));

            BlockSyntax block;
            if (withParam.Body is { } body)
            {
                block = body.AddStatements(assignStmt);
            }
            else if (withParam.ExpressionBody is { } exprBody)
            {
                // Convert `=> expr;` ctor into a block, then append the assignment.
                var orig = SyntaxFactory.ExpressionStatement(exprBody.Expression);
                block = SyntaxFactory.Block(orig, assignStmt);
            }
            else
            {
                block = SyntaxFactory.Block(assignStmt);
            }

            var rebuilt = withParam
                .WithBody(block)
                .WithExpressionBody(null)
                .WithSemicolonToken(default);

            // CS1573: if sibling params are XML-documented, add a `<param>` tag
            // for the injected one. The augmented doc keeps the ctor's original
            // file indentation, so it doubles as the (indented) leading trivia.
            var docTrivia = AugmentCtorDoc(liveCtor.GetLeadingTrivia(), param, eol);
            var newCtor = SeamCore.FormatMember(
                rebuilt, ctorIndent, indentUnit, eol,
                leading: docTrivia,
                trailing: liveCtor.GetTrailingTrivia());
            newClass = newClass.ReplaceNode(liveCtor, newCtor);
        }
        else
        {
            // §5: No explicit ctor → synthesize one taking `{iface}? {param}=null`.
            // Document it (summary + param) so a public ctor builds even under
            // SA1600 / <GenerateDocumentationFile> (CS1591/CS1573).
            var synthesized = SyntaxFactory.ParseMemberDeclaration(
                $"public {classDecl.Identifier.Text}({iface}{optMark} {param} = null)\n" +
                $"{{\n    {field} = {param} ?? new {wrapper}({recvText});\n}}")!;
            var doc =
                $"{eol}{memberIndent}/// <summary>Initializes a new instance; the optional seam adapter is injected for testability.</summary>" +
                $"{eol}{memberIndent}/// <param name=\"{param}\">Optional seam adapter; defaults to wrapping the original receiver.</param>" +
                $"{eol}{memberIndent}";
            var synthCtor = SeamCore.FormatMember(
                synthesized, memberIndent, indentUnit, eol,
                leading: SyntaxFactory.ParseLeadingTrivia(doc),
                trailing: SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(eol)));
            newClass = InsertCtor(newClass, synthCtor);
        }

        // -- 3. add the backing field --------------------------------------
        newClass = InsertField(newClass, iface, field, recvText, memberIndent, eol);

        // -- 4. produce the rewritten file + generated wrapper file --------
        var root = ctx.Tree.GetRoot();
        var newRoot = root.ReplaceNode(classDecl, newClass);
        var files = new Dictionary<string, string>
        {
            [ctx.TargetFileAbs] = newRoot.ToFullString(),
            [SeamCore.GeneratedFilePath(ctx)] = SeamCore.EmitWrapperSource(ctx),
        };

        var seam = SeamCore.BuildSeam(ctx, injection: "ctor", injectionRef: param);
        var reason = $"wrapper_interface applied: injected {iface} via ctor param '{param}' on "
                   + $"{ctx.ContainingTypeSymbol.Name}; rewrote {siteRewriter.Rewrites} call site(s).";
        return RewriteResult.Ok(reason, files, seam);
    }

    // If the enclosing ctor carries an XML doc comment that already documents its
    // parameters (`<param .../>`), a newly-injected parameter with no matching
    // `<param>` tag trips CS1573 under <GenerateDocumentationFile>. Add a matching
    // tag after the last existing one so documented ctors keep building.
    private static SyntaxTriviaList AugmentCtorDoc(SyntaxTriviaList leading, string paramName, string eol)
    {
        var text = leading.ToFullString();
        int last = text.LastIndexOf("</param>", StringComparison.Ordinal);
        if (last < 0) return leading; // no documented params → nothing to do

        // End of the line containing that closing tag.
        int nl = text.IndexOf('\n', last);
        int after = nl < 0 ? text.Length : nl + 1;

        // Indentation prefix of the `///` doc line we're inserting after.
        int lineStart = text.LastIndexOf('\n', last) + 1;
        int ws = lineStart;
        while (ws < text.Length && (text[ws] == ' ' || text[ws] == '\t')) ws++;
        var indent = text.Substring(lineStart, ws - lineStart);

        var inserted = $"{indent}/// <param name=\"{paramName}\">Optional seam adapter; "
                     + "defaults to wrapping the original receiver.</param>" + eol;
        var augmented = text.Substring(0, after) + inserted + text.Substring(after);
        return SyntaxFactory.ParseLeadingTrivia(augmented);
    }

    // Insert the backing field after the receiver's own field (else at the top
    // of the member list), EOL/indent-matched. Leading trivia is just the member
    // indent — the preceding member/open-brace already ends the line — so no
    // blank line is introduced right after the opening brace (SA1505).
    private static ClassDeclarationSyntax InsertField(
        ClassDeclarationSyntax cls, string iface, string field, string recvText,
        string memberIndent, string eol)
    {
        var fieldDecl = ((FieldDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(
                $"private readonly {iface} {field};")!)
            .WithLeadingTrivia(SyntaxFactory.Whitespace(memberIndent))
            .WithTrailingTrivia(SyntaxFactory.EndOfLine(eol));
        for (int i = 0; i < cls.Members.Count; i++)
        {
            if (cls.Members[i] is FieldDeclarationSyntax fd
                && fd.Declaration.Variables.Any(v => v.Identifier.Text == recvText.TrimStart('@')))
            {
                return cls.WithMembers(cls.Members.Insert(i + 1, fieldDecl));
            }
        }
        return cls.WithMembers(cls.Members.Insert(0, fieldDecl));
    }

    private static ClassDeclarationSyntax InsertCtor(
        ClassDeclarationSyntax cls, MemberDeclarationSyntax ctor)
    {
        // Place the synthesized ctor after the last field, else at the end.
        int idx = 0;
        for (int i = 0; i < cls.Members.Count; i++)
            if (cls.Members[i] is FieldDeclarationSyntax) idx = i + 1;
        return cls.WithMembers(cls.Members.Insert(idx, ctor));
    }
}

// Rewrites every `recvText.Method(...)` invocation in the visited subtree to
// `field.Method(...)`, preserving type arguments and call arguments.
//
// BUG #2 fix: matching is OVERLOAD-PRECISE — only invocations binding to the
// SAME IMethodSymbol overload as the target are rewritten. Sibling invocations
// that call a DIFFERENT overload/method of the same name on the same receiver
// (e.g. ILogger.LogError(Exception,string) vs the target LogError(string)) are
// left on the raw receiver, so they keep binding to the framework type instead
// of the narrow generated wrapper (which only models the target overload). This
// preserves §4.1.1's anti-gaming intent: EVERY call site of the *target* member
// is redirected through the seam (the target is reachable only via the wrapper),
// while genuinely-different sibling members stay on the original receiver.
internal sealed class SameReceiverCallRewriter : CSharpSyntaxRewriter
{
    private readonly string _method;
    private readonly string _recvText;
    private readonly string _newReceiver;
    private readonly SemanticModel? _model;
    private readonly IMethodSymbol? _targetKey;
    public int Rewrites { get; private set; }

    public SameReceiverCallRewriter(string method, string recvText, string newReceiver,
        SemanticModel? model = null, IMethodSymbol? targetSymbol = null)
    {
        _method = method;
        _recvText = recvText;
        _newReceiver = newReceiver;
        _model = model;
        _targetKey = targetSymbol is null ? null : SeamCore.OverloadKey(targetSymbol);
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Decide on the ORIGINAL node — the semantic model is bound to the
        // original tree, so binding must happen before child rewrites.
        bool match = node.Expression is MemberAccessExpressionSyntax ma0
            && SeamCore.InvokedName(node) == _method
            && ma0.Expression.ToString().Trim() == _recvText.Trim();

        if (match && _model is not null && _targetKey is not null)
        {
            var info = _model.GetSymbolInfo(node);
            var sym = info.Symbol as IMethodSymbol
                      ?? SeamCore.PickBestCandidate(node, info.CandidateSymbols.OfType<IMethodSymbol>().ToImmutableArray(), _model);
            // Overload-precise: only rewrite sites binding to the target overload.
            // If the site cannot be bound, do NOT rewrite (safe: leaves it on the
            // raw receiver, which still compiles).
            match = sym is not null
                && SymbolEqualityComparer.Default.Equals(SeamCore.OverloadKey(sym), _targetKey);
        }

        var visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;
        if (match && visited.Expression is MemberAccessExpressionSyntax ma)
        {
            Rewrites++;
            var newMa = ma.WithExpression(
                SyntaxFactory.IdentifierName(_newReceiver).WithTriviaFrom(ma.Expression));
            return visited.WithExpression(newMa);
        }
        return visited;
    }
}
