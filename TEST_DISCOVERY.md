# Test Discovery Diagnostic

Per-project counts of tests included by the CI `--filter` vs the
unfiltered universe of tests `dotnet test --list-tests` would
emit. Generated 2026-05-07.

- **tests_universe** — count from `dotnet test --no-build --list-tests` (no filter)
- **tests_in_filter** — count with the same FILTER the coverage workflow uses
- **tests_excluded** = universe − in_filter
- A project marked `<not-built>` could not be enumerated and is treated as 0/0/0.

## Per-repo summary

| Repo | Projects | Built | Universe | In filter | Excluded | Excl % |
|---|---:|---:|---:|---:|---:|---:|
| abp | 78 | 78 | 46 | 46 | 0 | 0.0% |
| aspnetcore | 117 | 100 | 1,152 | 1,152 | 0 | 0.0% |
| efcore | 17 | 17 | 0 | 0 | 0 | 0.0% |
| orleans | 15 | 15 | 108 | 32 | 76 | 70.4% |
| roslyn | 49 | 49 | 3 | 3 | 0 | 0.0% |
| semantic-kernel | 37 | 37 | 4,882 | 4,882 | 0 | 0.0% |

## Top 10 projects: highest filter-exclusion ratio

Projects where the FILTER drops the largest fraction of tests. Signal: filter is aggressive here — review whether legitimate unit tests are being excluded.

| Repo | Project | Universe | In filter | Excluded | Excl % |
|---|---|---:|---:|---:|---:|
| orleans | `test/Extensions/ServiceBus.Tests/ServiceBus.Tests.csproj` | 108 | 32 | 76 | 70.4% |
| abp | `test/Volo.Abp.Core.Tests/Volo.Abp.Core.Tests.csproj` | 46 | 46 | 0 | 0.0% |
| aspnetcore | `src/OpenApi/test/Microsoft.AspNetCore.OpenApi.Tests/Microsoft.AspNetCore.OpenApi.Tests.csproj` | 27 | 27 | 0 | 0.0% |
| aspnetcore | `src/Servers/Kestrel/Core/test/Microsoft.AspNetCore.Server.Kestrel.Core.Tests.csproj` | 574 | 574 | 0 | 0.0% |
| aspnetcore | `src/Shared/test/Shared.Tests/Microsoft.AspNetCore.Shared.Tests.csproj` | 551 | 551 | 0 | 0.0% |
| roslyn | `src/Workspaces/CoreTest/Microsoft.CodeAnalysis.Workspaces.UnitTests.csproj` | 3 | 3 | 0 | 0.0% |
| semantic-kernel | `./src/Agents/UnitTests/Agents.UnitTests.csproj` | 504 | 504 | 0 | 0.0% |
| semantic-kernel | `./src/Connectors/Connectors.AzureAIInference.UnitTests/Connectors.AzureAIInference.UnitTests.csproj` | 74 | 74 | 0 | 0.0% |
| semantic-kernel | `./src/Connectors/Connectors.AzureOpenAI.UnitTests/Connectors.AzureOpenAI.UnitTests.csproj` | 462 | 462 | 0 | 0.0% |
| semantic-kernel | `./src/Connectors/Connectors.Google.UnitTests/Connectors.Google.UnitTests.csproj` | 381 | 381 | 0 | 0.0% |

## Bottom 10 projects: lowest tests-in-filter count

Projects with the fewest tests actually executed by the CI filter (among built projects). Signal: the test inventory exists but isn't running — either narrowly scoped tests, or the filter excludes ~all of them.

| Repo | Project | Universe | In filter | Excluded |
|---|---|---:|---:|---:|
| semantic-kernel | `./test/VectorData/Pinecone.UnitTests/Pinecone.UnitTests.csproj` | 1 | 1 | 0 |
| roslyn | `src/Workspaces/CoreTest/Microsoft.CodeAnalysis.Workspaces.UnitTests.csproj` | 3 | 3 | 0 |
| semantic-kernel | `./test/VectorData/Redis.UnitTests/Redis.UnitTests.csproj` | 6 | 6 | 0 |
| semantic-kernel | `./test/VectorData/InMemory.UnitTests/InMemory.UnitTests.csproj` | 8 | 8 | 0 |
| semantic-kernel | `./src/Experimental/Process.Runtime.Dapr.UnitTests/Process.Runtime.Dapr.UnitTests.csproj` | 10 | 10 | 0 |
| semantic-kernel | `./src/Experimental/Orchestration.Flow.UnitTests/Experimental.Orchestration.Flow.UnitTests.csproj` | 12 | 12 | 0 |
| semantic-kernel | `./src/Experimental/Process.Utilities.UnitTests/Process.Utilities.UnitTests.csproj` | 12 | 12 | 0 |
| semantic-kernel | `./test/VectorData/Chroma.UnitTests/Chroma.UnitTests.csproj` | 12 | 12 | 0 |
| semantic-kernel | `./src/Extensions/PromptTemplates.Liquid.UnitTests/PromptTemplates.Liquid.UnitTests.csproj` | 16 | 16 | 0 |
| semantic-kernel | `./src/Plugins/Plugins.AI.UnitTests/Plugins.AI.UnitTests.csproj` | 16 | 16 | 0 |

## Projects skipped (not built / error)

| Repo | Project | Status |
|---|---|---|
| aspnetcore | `src/Grpc/JsonTranscoding/test/Microsoft.AspNetCore.Grpc.JsonTranscoding.Tests/Microsoft.AspNetCore.Grpc.JsonTranscoding.Tests.csproj` | <error> |
| aspnetcore | `src/Grpc/JsonTranscoding/test/Microsoft.AspNetCore.Grpc.Swagger.Tests/Microsoft.AspNetCore.Grpc.Swagger.Tests.csproj` | <error> |
| aspnetcore | `src/Identity/Specification.Tests/src/Microsoft.AspNetCore.Identity.Specification.Tests.csproj` | <error> |
| aspnetcore | `src/Logging.AzureAppServices/test/Microsoft.Extensions.Logging.AzureAppServices.Tests.csproj` | <error> |
| aspnetcore | `src/ProjectTemplates/test/Templates.Blazor.Tests/Templates.Blazor.Tests.csproj` | <error> |
| aspnetcore | `src/ProjectTemplates/test/Templates.Blazor.WebAssembly.Auth.Tests/Templates.Blazor.WebAssembly.Auth.Tests.csproj` | <error> |
| aspnetcore | `src/ProjectTemplates/test/Templates.Blazor.WebAssembly.Tests/Templates.Blazor.WebAssembly.Tests.csproj` | <error> |
| aspnetcore | `src/ProjectTemplates/test/Templates.Mvc.Tests/Templates.Mvc.Tests.csproj` | <error> |
| aspnetcore | `src/ProjectTemplates/test/Templates.Tests/Templates.Tests.csproj` | <error> |
| aspnetcore | `src/SignalR/server/Specification.Tests/src/Microsoft.AspNetCore.SignalR.Specification.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePack.AspNetCoreMvcFormatter.Tests/MessagePack.AspNetCoreMvcFormatter.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePack.Experimental.Tests/MessagePack.Experimental.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePack.GeneratedCode.Tests/MessagePack.GeneratedCode.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePack.Generator.Tests/MessagePack.Generator.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePack.Internal.Tests/MessagePack.Internal.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePack.Tests/MessagePack.Tests.csproj` | <error> |
| aspnetcore | `src/submodules/MessagePack-CSharp/tests/MessagePackAnalyzer.Tests/MessagePackAnalyzer.Tests.csproj` | <error> |
