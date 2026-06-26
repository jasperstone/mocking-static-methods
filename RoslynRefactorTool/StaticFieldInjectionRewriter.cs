using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynRefactorTool;

// Phase-4 `static_field_injection` transform — Roslyn-based.
//
// Intent: For a static method in an INSTANCE class, inject a mockable interface
// via a static field + setter pattern. This addresses the "new is glue" principle:
// replace newed-up dependencies with injected static fields that tests can replace.
//
// Pattern:
//   1. Emit a static field: `private static IWrapper _wrapper = new Default...();`
//   2. Emit a setter: `public static void Set{WrapperName}ForTesting(IWrapper wrapper)`
//   3. Rewrite call sites to use the field instead of direct receiver reference
//
// Applicability (TRANSFORM_CONTRACT extension):
//   - Containing type MUST be an instance class (not static, not struct, not interface)
//   - Enclosing method MUST be static
//   - Receiver must be a field/property/parameter or external type
//   - All same-receiver call sites in the static method are rewritten
//
// PURE: returns proposed source text only; Python owns all writes + the
// behavior-preservation build + auto-revert.
internal static class StaticFieldInjectionRewriter
{
    public static RewriteResult Apply(SeamContext ctx)
    {
        // -- §5 structural guards ------------------------------------------

        if (ctx.ContainingType is not ClassDeclarationSyntax classDecl)
            return RewriteResult.Reject("site_not_found: containing type is not a class");

        // Containing class must NOT be static
        if (classDecl.Modifiers.Any(SyntaxKind.StaticKeyword))
            return RewriteResult.Reject("static_class: cannot inject static field into static class");

        var method = ctx.EnclosingMethod;
        if (method is null)
            return RewriteResult.Reject("site_not_found: no enclosing method");

        // Enclosing method MUST be static
        if (!method.Modifiers.Any(SyntaxKind.StaticKeyword))
            return RewriteResult.Reject("not_static_method: method must be static for field injection");

        // Receiver must be resolvable (field, property, parameter, or external type)
        if (ctx.ReceiverSymbol is null && !SeamCore.IsExternalType(ctx.ReceiverType))
            return RewriteResult.Reject("receiver_not_resolvable");

        // -- formatting context -----------------------------------------------
        var fileText = ctx.Tree.GetText().ToString();
        var eol = SeamCore.DetectEol(fileText);
        var indentUnit = SeamCore.IndentUnitOf(classDecl);
        var classIndent = SeamCore.LineIndentOf(classDecl);
        var memberIndent = classIndent + indentUnit;

        var iface = ctx.InterfaceName;          // e.g., "IHttpClientWrapper"
        var wrapper = ctx.WrapperName;          // e.g., "HttpClientWrapper"
        var fieldName = "_" + char.ToLower(wrapper[0]) + wrapper.Substring(1);  // e.g., "_httpClientWrapper"
        var setterName = $"Set{wrapper}ForTesting";  // e.g., "SetHttpClientWrapperForTesting"
        var seamMember = ctx.Method.Name;       // e.g., "GetAsync"

        // -- 1. Rewrite all same-receiver call sites in the static method ------
        // Use the existing SameReceiverCallRewriter to replace all invocations of
        // the target member on the same receiver, changing from receiver to field reference
        var recvText = ctx.ReceiverText;
        var siteRewriter = new SameReceiverCallRewriter(seamMember, recvText, fieldName, ctx.Model, ctx.BoundMethod);
        
        // Apply the rewriter to the containing class (not just the method)
        var newClass = (ClassDeclarationSyntax)siteRewriter.Visit(classDecl)!;

        if (siteRewriter.Rewrites == 0)
            return RewriteResult.Reject("site_not_found: no rewritable call site in the static method");

        // -- 2. Determine default instantiation for the field ---------------
        // For now, always initialize with new Wrapper() 
        // (wrapper's constructor will handle receiver initialization)
        string defaultInstantiation = $"new {wrapper}()";

        // -- 3. Emit static field -------------------------------------------
        var fieldCode = $"private static {iface} {fieldName} = {defaultInstantiation};";
        var fieldDecl = (FieldDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(fieldCode)!;
        var formattedField = SeamCore.FormatMember(
            fieldDecl, memberIndent, indentUnit, eol,
            leading: SyntaxFactory.TriviaList(),
            trailing: SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(eol)));

        // -- 4. Emit setter method ------------------------------------------
        var setterCode =
            $"public static void {setterName}({iface} value)\n" +
            $"{{\n{memberIndent}{memberIndent}{fieldName} = value ?? throw new ArgumentNullException(nameof(value));\n{memberIndent}}}";
        var setterDecl = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(setterCode)!;
        var formattedSetter = SeamCore.FormatMember(
            setterDecl, memberIndent, indentUnit, eol,
            leading: SyntaxFactory.TriviaList(),
            trailing: SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(eol)));

        // -- 5. Add field and setter to the class ----------------------------
        // Insert field at the beginning of the member list (after any existing fields)
        // Insert setter after the field
        var fieldsCount = newClass.Members.OfType<FieldDeclarationSyntax>().Count();
        newClass = newClass.WithMembers(newClass.Members.Insert(fieldsCount, formattedField));
        newClass = newClass.WithMembers(newClass.Members.Insert(fieldsCount + 1, formattedSetter));

        // -- 6. Build result ------------------------------------------------
        var root = ctx.Tree.GetRoot();
        var newRoot = root.ReplaceNode(classDecl, newClass);
        
        var files = new Dictionary<string, string>
        {
            [ctx.TargetFileAbs] = newRoot.ToFullString(),
            [SeamCore.GeneratedFilePath(ctx)] = SeamCore.EmitWrapperSource(ctx),
        };

        var seam = SeamCore.BuildSeam(ctx, injection: "static_field", injectionRef: fieldName);
        var reason = $"static_field_injection applied: injected {iface} via static field '{fieldName}' "
                   + $"with setter '{setterName}' on {ctx.ContainingTypeSymbol.Name}.{method.Identifier.Text}; "
                   + $"rewrote {siteRewriter.Rewrites} call site(s).";
        return RewriteResult.Ok(reason, files, seam);
    }
}
