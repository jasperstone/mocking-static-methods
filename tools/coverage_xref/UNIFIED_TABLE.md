# Unified per-repo table

Combines: test count (from job logs) · Mode #1 static-call sites by family · 
Mode #1 sites covered by tests · overall line coverage %.

Sources:
- Mode #1 sites: `Mode1Analyzer/results/mode1_sites.csv`
- Coverage XML: `/tmp/cov_phase2/coverage-xml-*/coverage.cobertura.xml`
- Test counts: scraped from job logs (sum of `Total: N` lines in `dotnet test` summaries; see _TEST_COUNTS in this script)

## Per-repo breakdown

| Repo | Tests | ILogger | HttpClient | IConfig | ISvcProv | Other | Mode #1 total | Mode #1 covered | Line cov % |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| abp | 1358 | 505 | 24 | 23 | 460 | 0 | 1012 | 192 | 42.04% |
| aspnetcore | 31603 | 277 | 32 | 53 | 561 | 0 | 923 | 314 | 64.36% |
| Avalonia | 6860 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 73.25% |
| duplicati | 1096 | 0 | 13 | 0 | 8 | 0 | 21 | 0 | 36.18% |
| efcore | 13724 | 0 | 0 | 0 | 13 | 0 | 13 | 11 | 27.37% |
| eShop | — | 77 | 8 | 3 | 6 | 0 | 94 | 4 | 13.95% |
| garnet | 3563 | 688 | 0 | 0 | 0 | 0 | 688 | 179 | 65.99% |
| jellyfin | 2740 | 1179 | 1 | 7 | 12 | 0 | 1199 | 153 | 55.93% |
| OpenRA | 473 | 0 | 13 | 0 | 0 | 0 | 13 | 0 | 5.84% |
| orleans | 11041 | 47 | 0 | 30 | 213 | 0 | 290 | 154 | 40.47% |
| roslyn | 155997 | 110 | 0 | 0 | 0 | 0 | 110 | 4 | 84.92% |
| runtime | 6012 | 0 | 26 | 0 | 7 | 0 | 33 | 1 | 14.83% |
| semantic-kernel | 6263 | 239 | 65 | 6 | 273 | 0 | 583 | 31 | 27.24% |
| server | 5118 | 76 | 0 | 1 | 95 | 0 | 172 | 44 | 3.44% |
| StockSharp | 4107 | 0 | 3 | 0 | 0 | 0 | 3 | 0 | 30.96% |
| **TOTAL** | **249,955** | **3,198** | **185** | **123** | **1,648** | **0** | **5,154** | **1,087** | **58.23%** |

## Notes

- **Tests** — sum of `Total: N` lines from each repo's job log (`dotnet test` summary). `—` means the test runner did not emit a parseable summary for this run (typically because tests crashed under coverlet.console — Avalonia/eShop — or dotnet-coverage wrapped the runner — runtime).
- **Family columns** — Mode #1 sites grouped by the receiver/extension family the analyzer detected.
- **Mode #1 covered** — call sites where the cobertura XML reports `hits > 0` for the source line.
- **Line cov %** — `lines-covered / lines-valid` summed across all cobertura XMLs for the repo. This is overall test-suite coverage, not Mode #1-specific.
