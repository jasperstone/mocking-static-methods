
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class ComplexityCalculator
{
    public static int Compute(MethodDeclarationSyntax method)
    {
        int complexity = 1; // Base complexity
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
        complexity += method.DescendantNodes().OfType<BinaryExpressionSyntax>()
            .Count(b => b.OperatorToken.Text == "&&" || b.OperatorToken.Text == "||");
        return complexity;
    }
}
