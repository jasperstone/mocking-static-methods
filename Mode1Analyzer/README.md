# Mode1Analyzer

Roslyn semantic-model analyzer that finds **Mode #1 mockability failure
sites** in C# source: invocations that *look* mockable but aren't.

## What counts as Mode #1

A call site `expr.Method(args)` where the bound `IMethodSymbol` is either:

1. **Extension method on an interface receiver** —
   `IMethodSymbol.IsExtensionMethod && ReducedFrom.Parameters[0].Type.TypeKind == Interface`.
   You can't `Mock<IFoo>().Setup(x => x.Bar())` because `Bar` lives on a static class, not on `IFoo`. Moq throws `NotSupportedException` at Setup time.
2. **Non-virtual instance method on a non-sealed concrete class** —
   `!IsStatic && !IsVirtual && !IsAbstract && !IsOverride`. There's no virtual slot for Moq to override. Same `NotSupportedException`.

Both compile cleanly. Both fail at test runtime. That's what makes Mode #1
the strongest experimental story.

## Scope filter

We only flag calls whose static container is in the research scope:

- `Microsoft.Extensions.Logging.LoggerExtensions` (LogInformation, LogWarning, ...)
- `Microsoft.Extensions.Configuration.ConfigurationBinder` (GetValue<T>, Bind)
- `Microsoft.Extensions.Configuration.ConfigurationExtensions` (GetConnectionString)
- `Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions` (GetRequiredService<T>, GetService<T>, CreateScope)
- `System.Net.Http.HttpClient` instance methods (GetAsync, PostAsync, SendAsync, ...)

Anything else (LINQ, Newtonsoft, AutoMapper, custom extensions) is technically
Mode #1 but outside this study, so the analyzer suppresses it.

## Run it

Container-based — no local SDK install required:

```bash
docker run --rm -v "$PWD:/work" -w /work mcr.microsoft.com/dotnet/sdk:10.0-noble \
  bash -c "cd Mode1Analyzer && dotnet build -c Release"

docker run --rm -v "$PWD:/work" -w /work mcr.microsoft.com/dotnet/sdk:10.0-noble \
  bash -c "cd Mode1Analyzer && dotnet bin/Release/net10.0/Mode1Analyzer.dll \
    /work/cloned_repos/eShop \
    --out /work/Mode1Analyzer/results/eshop.csv"
```

Multi-repo:

```bash
docker run --rm -v "$PWD:/work" -w /work mcr.microsoft.com/dotnet/sdk:10.0-noble \
  bash -c "cd Mode1Analyzer && dotnet bin/Release/net10.0/Mode1Analyzer.dll \
    /work/cloned_repos/eShop /work/cloned_repos/jellyfin /work/cloned_repos/orleans \
    --out /work/Mode1Analyzer/results/mode1_sites.csv"
```

## Output

CSV with one row per Mode #1 site:

```
repo,file,line,receiver_type,method,kind,containing_type
eShop,src/Basket.API/Repositories/RedisBasketRepository.cs,41,Microsoft.Extensions.Logging.ILogger,LogInformation,Extension,Microsoft.Extensions.Logging.LoggerExtensions
```

`kind` is `Extension` or `NonVirtual`.

## How it works (fast path)

We don't run `dotnet restore` per project. Instead we build a single
`CSharpCompilation` per repo with:

- All `*.cs` files under the repo (excluding `obj/`, `bin/`, `test*/`, `samples/`, `benchmark*/`, `examples/`)
- The .NET runtime ref pack (from the SDK image — `Path.GetDirectoryName(typeof(object).Assembly.Location)`)
- The Microsoft.Extensions.* assemblies copied alongside our binary at build time

Symbols that bind cleanly are classified. Symbols that fail to bind (because
the project uses third-party references we don't have) silently skip. For
the standard-library Mode #1 patterns this is accurate enough.
