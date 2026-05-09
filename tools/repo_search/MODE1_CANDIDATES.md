# Mode #1 candidate repos

Repos ranked by raw count of Mode #1 mockability-failure patterns (`ILogger` extension methods, `HttpClient` non-virtual instance methods, `IServiceProvider.GetRequiredService<T>`).

Counts come from GitHub Code Search (`total_count` per pattern, restricted to `language:C#`).

_78 repos with at least one Mode #1 hit. Top 30 shown._

| Rank | Repo | Stars | Total | ILogger | HttpClient | ServiceProvider | IConfiguration | Already cloned |
|---:|---|---:|---:|---:|---:|---:|---:|:---:|
| 1 | [abpframework/abp](https://github.com/abpframework/abp) | 14246 | 1179 | 259 | 40 | 788 | 92 | ✅ |
| 2 | [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) | 37903 | 1171 | 274 | 354 | 440 | 103 | ✅ |
| 3 | [dotnet/roslyn](https://github.com/dotnet/roslyn) | 20397 | 811 | 234 | 5 | 550 | 22 | ✅ |
| 4 | [dotnet/runtime](https://github.com/dotnet/runtime) | 17872 | 765 | 370 | 183 | 68 | 144 | ✅ |
| 5 | [dotnet/AspNetCore.Docs](https://github.com/dotnet/AspNetCore.Docs) | 13087 | 728 | 308 | 56 | 163 | 201 |  |
| 6 | [microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) | 27853 | 645 | 155 | 51 | 228 | 211 | ✅ |
| 7 | [Unity-Technologies/UnityCsReference](https://github.com/Unity-Technologies/UnityCsReference) | 12810 | 607 | 604 | 0 | 0 | 3 |  |
| 8 | [microsoft/PowerToys](https://github.com/microsoft/PowerToys) | 132915 | 542 | 517 | 2 | 16 | 7 |  |
| 9 | [bitwarden/server](https://github.com/bitwarden/server) | 18552 | 463 | 305 | 59 | 61 | 38 | ✅ |
| 10 | [jellyfin/jellyfin](https://github.com/jellyfin/jellyfin) | 51341 | 447 | 399 | 34 | 8 | 6 |  |
| 11 | [dotnet/orleans](https://github.com/dotnet/orleans) | 10763 | 405 | 142 | 0 | 205 | 58 | ✅ |
| 12 | [babalae/better-genshin-impact](https://github.com/babalae/better-genshin-impact) | 13484 | 399 | 346 | 33 | 14 | 6 |  |
| 13 | [unoplatform/uno](https://github.com/unoplatform/uno) | 9930 | 368 | 324 | 18 | 11 | 15 |  |
| 14 | [files-community/Files](https://github.com/files-community/Files) | 43307 | 356 | 75 | 4 | 266 | 11 |  |
| 15 | [dotnet/maui](https://github.com/dotnet/maui) | 23239 | 301 | 155 | 6 | 132 | 8 |  |
| 16 | [StockSharp/StockSharp](https://github.com/StockSharp/StockSharp) | 9869 | 298 | 95 | 1 | 0 | 202 |  |
| 17 | [microsoft/garnet](https://github.com/microsoft/garnet) | 11818 | 287 | 282 | 0 | 0 | 5 |  |
| 18 | [Kareadita/Kavita](https://github.com/Kareadita/Kavita) | 10497 | 264 | 246 | 0 | 17 | 1 |  |
| 19 | [mono/mono](https://github.com/mono/mono) | 11437 | 250 | 19 | 0 | 7 | 224 | ✅ |
| 20 | [dotnet/efcore](https://github.com/dotnet/efcore) | 14644 | 167 | 4 | 1 | 140 | 22 | ✅ |
| 21 | [dodyg/practical-aspnetcore](https://github.com/dodyg/practical-aspnetcore) | 10372 | 140 | 70 | 11 | 51 | 8 |  |
| 22 | [OpenRA/OpenRA](https://github.com/OpenRA/OpenRA) | 16671 | 112 | 2 | 10 | 0 | 100 |  |
| 23 | [AvaloniaUI/Avalonia](https://github.com/AvaloniaUI/Avalonia) | 30701 | 97 | 6 | 8 | 68 | 15 |  |
| 24 | [SubtitleEdit/subtitleedit](https://github.com/SubtitleEdit/subtitleedit) | 12804 | 88 | 39 | 46 | 3 | 0 | ✅ |
| 25 | [duplicati/duplicati](https://github.com/duplicati/duplicati) | 14519 | 80 | 7 | 59 | 10 | 4 |  |
| 26 | [dotnet/eShop](https://github.com/dotnet/eShop) | 10426 | 79 | 58 | 9 | 5 | 7 |  |
| 27 | [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture) | 18162 | 67 | 45 | 1 | 12 | 9 |  |
| 28 | [Flow-Launcher/Flow.Launcher](https://github.com/Flow-Launcher/Flow.Launcher) | 14602 | 63 | 28 | 4 | 31 | 0 |  |
| 29 | [BeyondDimension/SteamTools](https://github.com/BeyondDimension/SteamTools) | 25364 | 55 | 26 | 13 | 12 | 4 |  |
| 30 | [LuckyPennySoftware/MediatR](https://github.com/LuckyPennySoftware/MediatR) | 11831 | 38 | 5 | 0 | 33 | 0 |  |

## Patterns measured

- **ILogger** — `LogInformation`
- **ILogger** — `LogWarning`
- **ILogger** — `LogError`
- **ILogger** — `LogDebug`
- **ILogger** — `LogCritical`
- **ILogger** — `LogTrace`
- **HttpClient** — `HttpClient.GetAsync`
- **HttpClient** — `HttpClient.PostAsync`
- **HttpClient** — `HttpClient.SendAsync`
- **ServiceProvider** — `GetRequiredService<T>`
- **IConfiguration** — `GetValue<T>`
- **IConfiguration** — `GetConnectionString`
- **IConfiguration** — `Configuration.Bind`
- **IConfiguration** — `Configuration.GetSection`
