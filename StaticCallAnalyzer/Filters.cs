
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class Filters
{
    public static bool IsWrapperClass(ClassDeclarationSyntax classNode)
    {
        var name = classNode.Identifier.Text;
        return name.Contains("Provider") || name.Contains("Service") || name.Contains("Clock");
    }

    public static bool HasDateTimeProviderInjection(ClassDeclarationSyntax classNode)
    {
        var constructorParams = classNode.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .SelectMany(c => c.ParameterList.Parameters)
            .Select(p => p.Type?.ToString() ?? string.Empty);

        return constructorParams.Any(p => p.Contains("IDateTimeProvider") || p.Contains("IClock"));
    }
}
