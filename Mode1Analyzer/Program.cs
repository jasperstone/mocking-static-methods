using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mode1Analyzer;

// Mode #1 mockability failure detector.
//
// A "Mode #1 site" is an InvocationExpression whose bound IMethodSymbol is
// either:
//   (a) an extension method on an interface receiver (Moq cannot Setup it
//       because the method does not actually live on the interface), OR
//   (b) a non-virtual instance method on a non-sealed class (Moq cannot
//       intercept the call without a virtual slot).
//
// We bind via Roslyn's semantic model. Each repo is compiled as a single
// CSharpCompilation: every *.cs file we find under the repo root, with
// references to the .NET runtime ref pack + the BCL extension assemblies
// we copy alongside this binary at build time (lib/net9.0/*.dll).
//
// This is the FAST PATH. We do NOT run `dotnet restore` per project. That
// means symbols that require third-party references will bind as ErrorType
// and we silently skip them. For the Mode #1 patterns we care about
// (ILogger, HttpClient, IConfiguration, IServiceProvider) the standard
// references are sufficient.

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("usage: Mode1Analyzer <repo-path> [<repo-path> ...] [--out <csv>]");
            return 1;
        }

        string outCsv = "mode1_sites.csv";
        var repos = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--out" && i + 1 < args.Length) { outCsv = args[++i]; continue; }
            repos.Add(args[i]);
        }

        var refs = LoadReferences();
        Console.Error.WriteLine($"loaded {refs.Count} reference assemblies");

        using var writer = new StreamWriter(outCsv);
        writer.WriteLine("repo,file,line,receiver_type,method,kind,containing_type");

        int totalSites = 0;
        foreach (var repo in repos)
        {
            var name = Path.GetFileName(repo.TrimEnd('/'));
            Console.Error.WriteLine($"\n=== {name} ===");
            var sites = AnalyzeRepo(name, repo, refs);
            foreach (var s in sites)
                writer.WriteLine($"{Csv(s.Repo)},{Csv(s.File)},{s.Line},{Csv(s.ReceiverType)},{Csv(s.Method)},{s.Kind},{Csv(s.ContainingType)}");
            writer.Flush();
            totalSites += sites.Count;
            Console.Error.WriteLine($"  → {sites.Count} Mode #1 sites");
        }

        Console.Error.WriteLine($"\nTOTAL: {totalSites} sites across {repos.Count} repos → {outCsv}");
        return 0;
    }

    private static List<MetadataReference> LoadReferences()
    {
        var refs = new List<MetadataReference>();
        // Runtime ref pack — the dotnet SDK in the container has these on disk.
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        foreach (var dll in Directory.GetFiles(runtimeDir, "*.dll"))
        {
            try { refs.Add(MetadataReference.CreateFromFile(dll)); } catch { }
        }
        // Our copied BCL extension refs (lib/net9.0/*.dll).
        var ourRefs = Path.Combine(AppContext.BaseDirectory, "refs");
        if (Directory.Exists(ourRefs))
        {
            foreach (var dll in Directory.GetFiles(ourRefs, "*.dll"))
            {
                try { refs.Add(MetadataReference.CreateFromFile(dll)); } catch { }
            }
        }
        return refs;
    }

    private static List<Site> AnalyzeRepo(string repoName, string repoPath, List<MetadataReference> refs)
    {
        // Filter out test/sample/benchmark trees so we measure production code only.
        // Also skip obj/ and bin/ which contain generated source.
        var files = Directory.EnumerateFiles(repoPath, "*.cs", SearchOption.AllDirectories)
            .Where(f =>
            {
                var rel = f[(repoPath.Length + 1)..].Replace('\\', '/').ToLowerInvariant();
                if (rel.Contains("/obj/") || rel.Contains("/bin/")) return false;
                if (rel.Contains("/test/") || rel.Contains("/tests/")) return false;
                if (rel.Contains(".test.") || rel.Contains(".tests.")) return false;
                if (rel.Contains("/sample") || rel.Contains("/samples/")) return false;
                if (rel.Contains("/benchmark") || rel.Contains("/perf/")) return false;
                if (rel.Contains("/example") || rel.Contains("/examples/")) return false;
                return true;
            })
            .ToList();

        Console.Error.WriteLine($"  {files.Count:N0} .cs files (post-filter)");

        // Parse all in parallel. Tolerate parse errors; just skip the tree.
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var trees = new ConcurrentBag<SyntaxTree>();
        Parallel.ForEach(files, f =>
        {
            try
            {
                var src = File.ReadAllText(f);
                var tree = CSharpSyntaxTree.ParseText(src, parseOptions, path: f);
                trees.Add(tree);
            }
            catch { /* unreadable / encoding issues — skip */ }
        });

        var compilation = CSharpCompilation.Create(
            assemblyName: repoName,
            syntaxTrees: trees,
            references: refs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                nullableContextOptions: NullableContextOptions.Disable));

        var sites = new ConcurrentBag<Site>();
        Parallel.ForEach(compilation.SyntaxTrees, tree =>
        {
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                Site? site = Classify(inv, model, repoName);
                if (site != null) sites.Add(site);
            }
        });

        return sites.OrderBy(s => s.File).ThenBy(s => s.Line).ToList();
    }

    private static Site? Classify(InvocationExpressionSyntax inv, SemanticModel model, string repoName)
    {
        var symInfo = model.GetSymbolInfo(inv);
        if (symInfo.Symbol is not IMethodSymbol m) return null;

        // Static method calls — those are Mode #3 (the original analyzer's
        // territory). Skip; we only score Mode #1 here.
        if (m.IsStatic && m.ReducedFrom == null) return null;

        // Find receiver type — the thing on the left of the dot.
        ITypeSymbol? receiverType = null;
        string? methodName = m.Name;
        if (inv.Expression is MemberAccessExpressionSyntax ma)
        {
            var rec = model.GetTypeInfo(ma.Expression).Type;
            if (rec != null) receiverType = rec;
        }

        // Mode #1 (a): extension method whose declared receiver is an interface
        // AND whose containing static class is in our research scope.
        if (m.IsExtensionMethod && m.ReducedFrom != null)
        {
            var reduced = m.ReducedFrom;
            if (reduced.Parameters.Length == 0) return null;
            var declaredRecv = reduced.Parameters[0].Type;
            if (declaredRecv.TypeKind != TypeKind.Interface) return null;

            var containingFqn = reduced.ContainingType?.ToDisplayString() ?? "";
            if (!IsResearchExtensionClass(containingFqn)) return null;

            var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            return new Site(
                Repo: repoName,
                File: TrimRepoPath(inv.SyntaxTree.FilePath, repoName),
                Line: line,
                ReceiverType: declaredRecv.ToDisplayString(),
                Method: methodName,
                Kind: "Extension",
                ContainingType: containingFqn
            );
        }

        // Mode #1 (b): non-virtual instance method on a non-sealed class.
        // (Sealed instance methods are uninteresting for our experiment because
        // there's no inheritance to override anyway — a different failure mode.)
        if (!m.IsStatic && !m.IsVirtual && !m.IsAbstract && !m.IsOverride)
        {
            var declaringType = m.ContainingType;
            if (declaringType == null) return null;
            if (declaringType.TypeKind != TypeKind.Class) return null;
            // We only flag types where the user *might reasonably try* to
            // mock the instance: well-known concrete service types.
            // Filter to the classes from our research scope — anything else
            // is noise (millions of getters, ToString, etc.).
            var fqn = declaringType.ToDisplayString();
            if (!IsInterestingNonVirtualReceiver(fqn)) return null;

            var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            return new Site(
                Repo: repoName,
                File: TrimRepoPath(inv.SyntaxTree.FilePath, repoName),
                Line: line,
                ReceiverType: receiverType?.ToDisplayString() ?? fqn,
                Method: methodName,
                Kind: "NonVirtual",
                ContainingType: fqn
            );
        }

        return null;
    }

    // Whitelist of classes whose non-virtual instance methods are part of
    // the Mode #1 study. Keeping this narrow keeps the CSV signal-rich.
    private static readonly HashSet<string> InterestingClasses = new(StringComparer.Ordinal)
    {
        "System.Net.Http.HttpClient",
        "System.Net.Http.HttpMessageInvoker",
    };

    private static bool IsInterestingNonVirtualReceiver(string fqn) => InterestingClasses.Contains(fqn);

    // Static classes whose extension methods are the headline "Mode #1"
    // patterns from the experiment. Anything outside this set (LINQ,
    // Newtonsoft, AutoMapper, etc.) is technically Mode #1 by definition
    // but not part of this study, so we filter it out to keep the signal
    // pure.
    private static readonly HashSet<string> ResearchExtensionClasses = new(StringComparer.Ordinal)
    {
        // ILogger
        "Microsoft.Extensions.Logging.LoggerExtensions",
        // IConfiguration
        "Microsoft.Extensions.Configuration.ConfigurationBinder",
        "Microsoft.Extensions.Configuration.ConfigurationExtensions",
        // IServiceProvider
        "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions",
    };

    private static bool IsResearchExtensionClass(string fqn) => ResearchExtensionClasses.Contains(fqn);

    private static string TrimRepoPath(string path, string repoName)
    {
        var idx = path.IndexOf("/cloned_repos/" + repoName + "/", StringComparison.Ordinal);
        if (idx >= 0) return path[(idx + ("/cloned_repos/" + repoName + "/").Length)..];
        return path;
    }

    private static string Csv(string s)
    {
        if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    private record Site(string Repo, string File, int Line, string ReceiverType, string Method, string Kind, string ContainingType);
}
