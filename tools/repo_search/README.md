# tools/repo_search

Find C# repos rich in **Mode #1** mockability-failure patterns —
extension methods on interfaces (e.g. `ILogger.LogWarning`) and non-virtual
instance methods on concrete classes (e.g. `HttpClient.GetAsync`). These compile
but throw `NotSupportedException` at test runtime when naively mocked with Moq.

## Quick start

```bash
export GITHUB_TOKEN=ghp_...   # any read-only PAT
python3 tools/repo_search/find_mode1_repos.py --top 25
```

Outputs:
- `tools/repo_search/mode1_candidates.csv` — full per-pattern data
- `tools/repo_search/MODE1_CANDIDATES.md` — top-N markdown table

## Common knobs

```bash
# Quick dry run — just list candidate repos, no pattern queries
python3 tools/repo_search/find_mode1_repos.py --dry-run

# Smaller pool for fast iteration
python3 tools/repo_search/find_mode1_repos.py --max-repos 50 --limit 50

# Larger pool, lower star threshold
python3 tools/repo_search/find_mode1_repos.py --min-stars 500 --max-repos 250
```

## Wall-clock cost

GitHub authenticated code search caps at 30 req/min. The script sleeps 2.5s
between queries (24 req/min, safe margin).

| Repos × Patterns | Queries | Wall clock |
|---:|---:|---:|
| 50 × 14 | 700 | ~29 min |
| 150 × 14 | 2,100 | ~88 min |
| 250 × 14 | 3,500 | ~146 min |

The 7 already-cloned repos are auto-seeded into the candidate list so the
existing baseline always appears alongside new candidates.

## Patterns measured

See `PATTERNS` in `find_mode1_repos.py`. Currently:
- ILogger extension methods: `LogInformation/Warning/Error/Debug/Critical/Trace`
- HttpClient instance methods: `GetAsync`, `PostAsync`, `SendAsync`
- IServiceProvider extensions: `GetRequiredService<T>`
- IConfiguration extensions: `GetValue<T>`, `GetConnectionString`,
  `Configuration.Bind`, `Configuration.GetSection`
