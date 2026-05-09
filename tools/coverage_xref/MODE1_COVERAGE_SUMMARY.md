# Mode #1 sites × coverage cross-reference

Source: `Mode1Analyzer/results/mode1_sites.csv` (6879 sites)
Coverage: CI run 25495265941 (cobertura XML)

## Per-repo breakdown

| Repo | Sites | Covered | Uncovered | Unknown line | Unknown file | No coverage |
|---|---:|---:|---:|---:|---:|---:|
| Avalonia | 9 | 0 | 0 | 0 | 0 | 9 |
| Files | 163 | 0 | 0 | 0 | 0 | 163 |
| OpenRA | 13 | 0 | 0 | 0 | 0 | 13 |
| PowerToys | 66 | 0 | 0 | 0 | 0 | 66 |
| StockSharp | 3 | 0 | 0 | 0 | 0 | 3 |
| abp | 1017 | 194 | 445 | 5 | 373 | 0 |
| aspnetcore | 936 | 330 | 400 | 3 | 203 | 0 |
| duplicati | 34 | 0 | 0 | 0 | 0 | 34 |
| eShop | 94 | 0 | 0 | 0 | 0 | 94 |
| efcore | 13 | 11 | 1 | 0 | 1 | 0 |
| garnet | 745 | 0 | 0 | 0 | 0 | 745 |
| jellyfin | 1206 | 0 | 0 | 0 | 0 | 1206 |
| maui | 329 | 0 | 0 | 0 | 0 | 329 |
| orleans | 1181 | 261 | 717 | 0 | 203 | 0 |
| roslyn | 114 | 4 | 6 | 0 | 104 | 0 |
| runtime | 33 | 0 | 0 | 0 | 33 | 0 |
| semantic-kernel | 742 | 21 | 471 | 12 | 238 | 0 |
| server | 181 | 0 | 0 | 0 | 0 | 181 |

**Status meanings:**
- `covered` — Mode #1 call site executed by tests at least once
- `uncovered` — site exists, line is in coverage map, hits = 0
- `unknown_line` — file in coverage map but the specific line isn't (likely whitespace/comment offset)
- `unknown_file` — file not in cobertura output (likely production code excluded by test filter, or path mismatch)
- `no_coverage_data` — repo not yet wired into coverage CI

## Headline

Of 6879 Mode #1 sites with coverage data attempted:
- **821** covered (11.9%)
- **2,040** uncovered (29.7%)
- **20** unknown_line (0.3%)
- **1,155** unknown_file (16.8%)
- **2,843** no_coverage_data (41.3%)
