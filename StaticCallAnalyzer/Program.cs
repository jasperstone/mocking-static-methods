
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: StaticCallAnalyzer <path-to-repo>");
            return;
        }

        var repoPath = args[0];
        var csFiles = Directory.GetFiles(repoPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("Tests") && !f.Contains("Samples") && !f.Contains("Demo"));

        var results = new List<object>();

        foreach (var file in csFiles)
        {
            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            var staticCalls = root.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Where(m => StaticCallConfig.Patterns.Any(p =>
                    m.Expression.ToString() == p.ClassName && m.Name.ToString() == p.MethodName))
                .ToList();

            if (staticCalls.Count < 2) continue; // Require multiple calls

            var classNode = staticCalls.First().Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classNode == null) continue;

            // Exclude wrappers or injected abstractions
            if (Filters.IsWrapperClass(classNode) || Filters.HasDateTimeProviderInjection(classNode)) continue;

            foreach (var method in classNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var complexity = ComplexityCalculator.Compute(method);
                if (complexity > 5)
                {
                    foreach (var call in staticCalls)
                    {
                        var pattern = StaticCallConfig.Patterns.First(p =>
                            p.ClassName == call.Expression.ToString() && p.MethodName == call.Name.ToString());

                        results.Add(new
                        {
                            File = file,
                            Class = classNode.Identifier.Text,
                            Method = method.Identifier.Text,
                            Pattern = pattern.ClassName + "." + pattern.MethodName,
                            Complexity = complexity,
                            StaticCallCount = staticCalls.Count
                        });
                    }
                }
            }
        }

        var jsonOutput = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(jsonOutput);
    }
}
