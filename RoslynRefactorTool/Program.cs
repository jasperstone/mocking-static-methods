using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynRefactorTool;

// Phase-4 testability-seam rewriter (TRANSFORM_CONTRACT §0).
//
// PURE tool: reads the owning project source, proposes rewritten source text,
// and emits JSON on stdout. It NEVER writes the repo — Python owns all
// filesystem mutation, the snapshot/restore lifecycle, and the
// behavior-preservation build.
//
// stdout: JSON only ({ ok, applicable, reason, files{}, seam{} }).
// stderr: diagnostics.
// exit code: 0 normally (including applicable=false); nonzero only on internal
//            tool error (bad args / unhandled exception).
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"internal error: {ex}");
            EmitError($"internal tool error: {ex.GetType().Name}: {ex.Message}");
            return 2;
        }
    }

    private static int Run(string[] args)
    {
        var a = ParseArgs(args);

        var transform = Get(a, "transform");
        var owningDir = Get(a, "owning-dir");
        var file = Get(a, "file");
        var lineStr = Get(a, "line");
        var method = Get(a, "method");

        if (string.IsNullOrWhiteSpace(transform) || string.IsNullOrWhiteSpace(owningDir)
            || string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(method))
        {
            EmitError("missing required argument(s): --transform, --owning-dir, --file, --method");
            return 1;
        }
        if (transform is not ("wrapper_interface" or "parameterize_dependency"))
        {
            EmitError($"unsupported transform '{transform}'");
            return 1;
        }
        if (!int.TryParse(lineStr, out var line))
            line = 0;
        if (!Directory.Exists(owningDir))
        {
            EmitError($"owning directory does not exist: {owningDir}");
            return 1;
        }

        var refs = SeamCore.LoadReferences();
        Console.Error.WriteLine($"loaded {refs.Count} reference assemblies");

        var compilation = SeamCore.BuildCompilation(Path.GetFullPath(owningDir), refs);
        Console.Error.WriteLine($"compiled {compilation.SyntaxTrees.Count()} trees from {owningDir}");

        var ctx = SeamCore.Locate(
            compilation,
            Path.GetFullPath(file),
            line,
            method,
            transform,
            Get(a, "interface-name"),
            Get(a, "wrapper-name"),
            Get(a, "param-name"),
            out var locateReason);

        if (ctx is null)
        {
            EmitNotApplicable(locateReason);
            return 0;
        }

        RewriteResult result = transform == "wrapper_interface"
            ? WrapperInterfaceRewriter.Apply(ctx)
            : ParameterizeDependencyRewriter.Apply(ctx);

        if (!result.Applicable)
        {
            EmitNotApplicable(result.Reason);
            return 0;
        }

        Emit(new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["applicable"] = true,
            ["reason"] = result.Reason,
            ["files"] = result.Files,
            ["seam"] = result.Seam,
        });
        return 0;
    }

    // -- argv parsing ------------------------------------------------------

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) continue;
            var key = args[i][2..];
            string val = "";
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                val = args[++i];
            d[key] = val;
        }
        return d;
    }

    private static string Get(Dictionary<string, string> a, string k) =>
        a.TryGetValue(k, out var v) ? v : "";

    // -- JSON emit ---------------------------------------------------------

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static void Emit(Dictionary<string, object?> payload) =>
        Console.Out.Write(JsonSerializer.Serialize(payload, JsonOpts));

    private static void EmitNotApplicable(string reason) =>
        Emit(new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["applicable"] = false,
            ["reason"] = string.IsNullOrWhiteSpace(reason) ? "not_applicable" : reason,
            ["files"] = new Dictionary<string, string>(),
            ["seam"] = new Dictionary<string, object?>(),
        });

    private static void EmitError(string reason) =>
        Emit(new Dictionary<string, object?>
        {
            ["ok"] = false,
            ["applicable"] = false,
            ["reason"] = reason,
            ["files"] = new Dictionary<string, string>(),
            ["seam"] = new Dictionary<string, object?>(),
        });
}
