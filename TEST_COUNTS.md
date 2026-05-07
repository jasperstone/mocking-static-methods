# Test Counts (from coverage workflow logs)

_Generated: 2026-05-07 from runs 25468601840, 25472048463_

Per-project test counts extracted from `dotnet test` summary lines in Coverage Orchestrator job logs. Bypasses the broken `--list-tests` path for xunit.v3 repos.

## Per-repo aggregate

| repo | projects | total | passed | failed | skipped |
|---|---:|---:|---:|---:|---:|
| abp | 74 | 1358 | 1351 | 0 | 7 |
| aspnetcore | 96 | 31603 | 31210 | 11 | 382 |
| efcore | 14 | 13724 | 13702 | 0 | 22 |
| orleans | 28 | 1692 | 759 | 115 | 818 |
| roslyn | 33 | 155993 | 144293 | 10378 | 1322 |
| semantic-kernel | 44 | 6263 | 5610 | 636 | 17 |

### abp

**Top 10 projects by test count**

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Volo.Abp.Core.Tests | net10.0 | 164 | 164 | 0 | 0 |
| Volo.Abp.AspNetCore.Mvc.Tests | net10.0 | 148 | 148 | 0 | 0 |
| Volo.Abp.EntityFrameworkCore.Tests | net10.0 | 127 | 124 | 0 | 3 |
| Volo.Abp.MongoDB.Tests | net10.0 | 105 | 104 | 0 | 1 |
| Volo.Abp.Auditing.Tests | net10.0 | 57 | 57 | 0 | 0 |
| Volo.Abp.MemoryDb.Tests | net10.0 | 57 | 56 | 0 | 1 |
| Volo.Abp.Caching.Tests | net10.0 | 45 | 45 | 0 | 0 |
| Volo.Abp.Http.Client.Tests | net10.0 | 40 | 40 | 0 | 0 |
| Volo.Abp.Json.Tests | net10.0 | 34 | 34 | 0 | 0 |
| Volo.Abp.Autofac.Tests | net10.0 | 29 | 29 | 0 | 0 |

**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Volo.Abp.Caching.StackExchangeRedis.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.ExceptionHandling.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.Http.Client.IdentityModel.Web.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.IdentityModel.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.ObjectMapping.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.RemoteServices.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.Sms.Aliyun.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.Sms.TencentCloud.Tests | net10.0 | 1 | 1 | 0 | 0 |
| Volo.Abp.AspNetCore.Authentication.OAuth.Tests | net10.0 | 2 | 2 | 0 | 0 |
| Volo.Abp.Ldap.Tests | net10.0 | 2 | 1 | 0 | 1 |

### aspnetcore

**Top 10 projects by test count**

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Microsoft.AspNetCore.Server.Kestrel.Core.Tests | net10.0 | 9808 | 9807 | 0 | 1 |
| Microsoft.AspNetCore.Routing.Tests | net10.0 | 3392 | 3392 | 0 | 0 |
| Microsoft.AspNetCore.Http.Extensions.Tests | net10.0 | 2098 | 2096 | 0 | 2 |
| Microsoft.Net.Http.Headers.Tests | net10.0 | 1796 | 1796 | 0 | 0 |
| Microsoft.AspNetCore.Shared.Tests | net10.0 | 1204 | 1204 | 0 | 0 |
| Microsoft.AspNetCore.Components.Tests | net10.0 | 1182 | 1174 | 0 | 8 |
| Microsoft.AspNetCore.Http.Results.Tests | net10.0 | 1052 | 1050 | 0 | 2 |
| Microsoft.AspNetCore.OpenApi.Tests | net10.0 | 747 | 742 | 0 | 5 |
| Microsoft.AspNetCore.SignalR.Common.Tests | net10.0 | 719 | 718 | 0 | 1 |
| Microsoft.AspNetCore.Http.Abstractions.Tests | net10.0 | 714 | 714 | 0 | 0 |

**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Microsoft.AspNetCore.StaticAssets.Tests | net10.0 | 2 | 2 | 0 | 0 |
| Microsoft.AspNetCore.AzureAppServicesIntegration.Tests | net10.0 | 3 | 3 | 0 | 0 |
| Microsoft.Extensions.Localization.RootNamespace.Tests | net10.0 | 3 | 3 | 0 | 0 |
| Microsoft.AspNetCore.MiddlewareAnalysis.Tests | net10.0 | 3 | 3 | 0 | 0 |
| Microsoft.AspNetCore.OpenApi.Build.Tests | net10.0 | 3 | 3 | 0 | 0 |
| Microsoft.AspNetCore.Hosting.WindowsServices.Tests | net10.0 | 4 | 2 | 0 | 2 |
| Microsoft.AspNetCore.DataProtection.StackExchangeRedis.Tests | net10.0 | 7 | 6 | 0 | 1 |
| Microsoft.Extensions.WebEncoders.Tests | net10.0 | 8 | 8 | 0 | 0 |
| Microsoft.AspNetCore.Components.WebAssembly.Server.Tests | net10.0 | 11 | 11 | 0 | 0 |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore.Tests | net10.0 | 11 | 11 | 0 | 0 |

### efcore

**Top 10 projects by test count**

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Microsoft.EntityFrameworkCore.Tests | net10.0 | 6622 | 6622 | 0 | 0 |
| Microsoft.EntityFrameworkCore.Relational.Tests | net10.0 | 1379 | 1378 | 0 | 1 |
| Microsoft.EntityFrameworkCore.SqlServer.Tests | net10.0 | 1304 | 1304 | 0 | 0 |
| Microsoft.EntityFrameworkCore.Design.Tests | net10.0 | 1189 | 1189 | 0 | 0 |
| Microsoft.EntityFrameworkCore.Sqlite.Tests | net10.0 | 838 | 838 | 0 | 0 |
| Microsoft.Data.Sqlite.e_sqlcipher.Tests | net10.0 | 687 | 680 | 0 | 7 |
| Microsoft.Data.Sqlite.e_sqlite3mc.Tests | net10.0 | 687 | 680 | 0 | 7 |
| Microsoft.Data.Sqlite.Tests | net10.0 | 686 | 679 | 0 | 7 |
| Microsoft.EntityFrameworkCore.Cosmos.Tests | net10.0 | 131 | 131 | 0 | 0 |
| Microsoft.EntityFrameworkCore.Proxies.Tests | net10.0 | 76 | 76 | 0 | 0 |

**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| dotnet-ef.Tests | net10.0 | 4 | 4 | 0 | 0 |
| ef.Tests | net10.0 | 6 | 6 | 0 | 0 |
| Microsoft.EntityFrameworkCore.InMemory.Tests | net10.0 | 39 | 39 | 0 | 0 |
| Microsoft.EntityFrameworkCore.Proxies.Tests | net10.0 | 76 | 76 | 0 | 0 |
| Microsoft.EntityFrameworkCore.SqlServer.HierarchyId.Tests | net10.0 | 76 | 76 | 0 | 0 |
| Microsoft.EntityFrameworkCore.Cosmos.Tests | net10.0 | 131 | 131 | 0 | 0 |
| Microsoft.Data.Sqlite.Tests | net10.0 | 686 | 679 | 0 | 7 |
| Microsoft.Data.Sqlite.e_sqlcipher.Tests | net10.0 | 687 | 680 | 0 | 7 |
| Microsoft.Data.Sqlite.e_sqlite3mc.Tests | net10.0 | 687 | 680 | 0 | 7 |
| Microsoft.EntityFrameworkCore.Sqlite.Tests | net10.0 | 838 | 838 | 0 | 0 |

### orleans

**Top 10 projects by test count**

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Orleans.Transactions.Azure.Tests | net10.0 | 268 | 0 | 25 | 243 |
| Orleans.Transactions.Azure.Tests | net8.0 | 268 | 0 | 25 | 243 |
| Orleans.Transactions.Tests | net10.0 | 162 | 162 | 0 | 0 |
| Orleans.Transactions.Tests | net8.0 | 162 | 162 | 0 | 0 |
| Tester.AdoNet | net8.0 | 122 | 7 | 0 | 115 |
| Tester.AdoNet | net10.0 | 122 | 7 | 0 | 115 |
| Orleans.Serialization.UnitTests | net8.0 | 116 | 116 | 0 | 0 |
| Orleans.Serialization.UnitTests | net10.0 | 116 | 116 | 0 | 0 |
| AWSUtils.Tests | net8.0 | 34 | 1 | 0 | 33 |
| AWSUtils.Tests | net10.0 | 34 | 1 | 0 | 33 |

**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| DefaultCluster.Tests | net8.0 | 3 | 3 | 0 | 0 |
| DefaultCluster.Tests | net10.0 | 3 | 3 | 0 | 0 |
| ServiceBus.Tests | net8.0 | 7 | 0 | 7 | 0 |
| ServiceBus.Tests | net10.0 | 7 | 0 | 7 | 0 |
| Orleans.Journaling.Tests | net8.0 | 8 | 8 | 0 | 0 |
| Orleans.Journaling.Tests | net10.0 | 8 | 8 | 0 | 0 |
| Tester | net10.0 | 12 | 12 | 0 | 0 |
| Tester | net8.0 | 12 | 11 | 1 | 0 |
| TesterInternal | net8.0 | 12 | 7 | 0 | 5 |
| TesterInternal | net10.0 | 12 | 7 | 0 | 5 |

### roslyn

**Top 10 projects by test count**

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Microsoft.CodeAnalysis.CSharp.EditorFeatures2.UnitTests | net9.0 | 27527 | 27464 | 15 | 48 |
| Microsoft.CodeAnalysis.CSharp.Emit3.UnitTests | net10.0 | 27053 | 26921 | 0 | 132 |
| Microsoft.CodeAnalysis.CSharp.Features.UnitTests | net8.0 | 20360 | 10391 | 9860 | 109 |
| Microsoft.CodeAnalysis.CSharp.Semantic.UnitTests | net9.0 | 19524 | 19421 | 0 | 103 |
| Microsoft.CodeAnalysis.UnitTests | net9.0 | 18218 | 18119 | 0 | 99 |
| Microsoft.CodeAnalysis.CSharp.Symbol.UnitTests | net9.0 | 13977 | 13847 | 0 | 130 |
| Microsoft.CodeAnalysis.CSharp.Syntax.UnitTests | net9.0 | 9761 | 9736 | 0 | 25 |
| Microsoft.CodeAnalysis.CSharp.Emit.UnitTests | net9.0 | 7109 | 6957 | 1 | 151 |
| Microsoft.CodeAnalysis.CSharp.IOperation.UnitTests | net9.0 | 2435 | 2428 | 0 | 7 |
| Microsoft.CodeAnalysis.CSharp.Emit2.UnitTests | net9.0 | 2185 | 2122 | 0 | 63 |

**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| Microsoft.CodeAnalysis.ExternalAccess.RazorCompiler.UnitTests | net9.0 | 1 | 1 | 0 | 0 |
| Microsoft.CodeAnalysis.ExternalAccess.HotReload.UnitTests | net9.0 | 6 | 6 | 0 | 0 |
| RulesetToEditorconfigConverter.UnitTests | net9.0 | 12 | 12 | 0 | 0 |
| Microsoft.CodeAnalysis.Remote.ServiceHub.UnitTests | net8.0 | 13 | 13 | 0 | 0 |
| SemanticSearch.BuildTask.UnitTests | net9.0 | 33 | 33 | 0 | 0 |
| Microsoft.CodeAnalysis.Scripting.UnitTests | net9.0 | 45 | 45 | 0 | 0 |
| InteractiveHost.UnitTests | net9.0 | 48 | 0 | 39 | 9 |
| Microsoft.CodeAnalysis.ResxSourceGenerator.UnitTests | net9.0 | 59 | 57 | 0 | 2 |
| Text.Analyzers.UnitTests | net9.0 | 88 | 88 | 0 | 0 |
| VBCSCompiler.UnitTests | net9.0 | 136 | 112 | 0 | 24 |

### semantic-kernel

**Top 10 projects by test count**

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| SemanticKernel.UnitTests | net10.0 | 1599 | 1599 | 0 | 0 |
| SemanticKernel.Agents.UnitTests | net10.0 | 504 | 503 | 0 | 1 |
| SemanticKernel.Connectors.AzureOpenAI.UnitTests | net10.0 | 490 | 490 | 0 | 0 |
| Concepts | net10.0 | 475 | 36 | 430 | 9 |
| SemanticKernel.Connectors.OpenAI.UnitTests | net10.0 | 475 | 474 | 0 | 1 |
| SemanticKernel.Functions.UnitTests | net10.0 | 425 | 425 | 0 | 0 |
| SemanticKernel.Connectors.GoogleVertexAI.UnitTests | net10.0 | 406 | 406 | 0 | 0 |
| SemanticKernel.Plugins.UnitTests | net10.0 | 265 | 265 | 0 | 0 |
| GettingStartedWithAgents | net10.0 | 135 | 0 | 134 | 1 |
| SemanticKernel.Connectors.Weaviate.UnitTests | net10.0 | 122 | 122 | 0 | 0 |

**Bottom 10 projects by test count** (⚠️ = 0 tests, suspect)

| project | framework | total | passed | failed | skipped |
|---|---|---:|---:|---:|---:|
| SemanticKernel.Connectors.Pinecone.UnitTests | net10.0 | 1 | 1 | 0 | 0 |
| GettingStartedWithVectorStores | net10.0 | 6 | 0 | 6 | 0 |
| SemanticKernel.Connectors.InMemory.UnitTests | net10.0 | 8 | 8 | 0 | 0 |
| LearnResources | net10.0 | 9 | 0 | 9 | 0 |
| SemanticKernel.Process.Runtime.Dapr.UnitTests | net10.0 | 10 | 10 | 0 | 0 |
| GettingStarted | net10.0 | 11 | 0 | 11 | 0 |
| SemanticKernel.Experimental.Orchestration.Flow.UnitTests | net10.0 | 12 | 12 | 0 | 0 |
| SemanticKernel.Process.Utilities.UnitTests | net10.0 | 12 | 12 | 0 | 0 |
| SemanticKernel.Connectors.Chroma.UnitTests | net10.0 | 12 | 12 | 0 | 0 |
| SemanticKernel.Extensions.PromptTemplates.Liquid.UnitTests | net10.0 | 16 | 16 | 0 | 0 |

## Repos missing data

- **runtime** — <coverlet — no per-project test counts available in this log shape>
