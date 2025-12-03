
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

            if (staticCalls.Count == 0) continue;

            // Changed from requiring 2+ calls to just requiring 1+ calls
            if (staticCalls.Count < 1) continue;

            var classNode = staticCalls.First().Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classNode == null) continue;

            // Exclude wrappers or injected abstractions
            if (Filters.IsWrapperClass(classNode) || Filters.HasDateTimeProviderInjection(classNode)) continue;
            if (Filters.IsWrapperClass(classNode) || Filters.HasDateTimeProviderInjection(classNode)) continue;

            foreach (var method in classNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var complexity = ComplexityCalculator.Compute(method);
                
                // Lowered complexity threshold to capture more results
                if (complexity > 2)
                {
                    // Find static calls within this specific method, not the entire class
                    var methodStaticCalls = method.DescendantNodes()
                        .OfType<MemberAccessExpressionSyntax>()
                        .Where(m => StaticCallConfig.Patterns.Any(p =>
                            m.Expression.ToString() == p.ClassName && m.Name.ToString() == p.MethodName))
                        .ToList();

                    if (methodStaticCalls.Any())
                    {
                        // Group by pattern to avoid duplicates for the same pattern in the same method
                        var patternGroups = methodStaticCalls
                            .GroupBy(call => new { 
                                ClassName = call.Expression.ToString(), 
                                MethodName = call.Name.ToString() 
                            })
                            .ToList();

                        foreach (var group in patternGroups)
                        {
                            var pattern = StaticCallConfig.Patterns.First(p =>
                                p.ClassName == group.Key.ClassName && p.MethodName == group.Key.MethodName);

                            results.Add(new
                            {
                                File = file,
                                Class = classNode.Identifier.Text,
                                Method = method.Identifier.Text,
                                Pattern = pattern.ClassName + "." + pattern.MethodName,
                                PatternCount = group.Count(), // How many times this pattern appears in this method
                                Complexity = complexity,
                                StaticCallCount = methodStaticCalls.Count // Total static calls in this method
                            });
                        }
                    }
                }
            }
        }

        var jsonOutput = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        
        // Write to analysis_results.json file
        var outputPath = "analysis_results.json";
        if (results.Any())
        {
            // Read existing results if file exists
            var existingResults = new List<object>();
            if (File.Exists(outputPath))
            {
                try
                {
                    var existingContent = File.ReadAllText(outputPath);
                    if (!string.IsNullOrWhiteSpace(existingContent))
                    {
                        existingResults = JsonSerializer.Deserialize<List<object>>(existingContent) ?? new List<object>();
                    }
                }
                catch
                {
                    // If file is corrupted, start fresh
                    existingResults = new List<object>();
                }
            }
            
            // Append new results
            existingResults.AddRange(results);
            
            // Write combined results back to file
            var combinedJson = JsonSerializer.Serialize(existingResults, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputPath, combinedJson);
        }
        
        Console.WriteLine(jsonOutput);
    }
}
