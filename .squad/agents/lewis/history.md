# Project Context

- **Project:** mocking-static-methods
- **User:** Jasper
- **Created:** 2026-04-30
- **Goal:** Reproducibly build + test + collect coverage across abp, aspnetcore, efcore, orleans, roslyn, runtime, semantic-kernel. Skipped: mono, IdentityServer4, subtitleedit.

## Core Context

Lead for build/CI/coverage reproducibility work.

## Learnings

- Tags like `v10.0.0`, `v10.0.2` for aspnetcore reference internal Microsoft RC/servicing builds — don't use them. Pin to a public-SDK commit instead (e.g., `ecb199c2` on `release/10.0` uses SDK 10.0.101).
- EF Core uses `activate.sh` to pin local SDK 10.0.102.
- Branches drift; commit SHAs don't.

## Recent Updates

- 2026-04-30 — Workflow audit: EF Core `release/10.0` is the #1 reproducibility blocker (branch ref). Roslyn workflow correctly SHA-pinned but README still says `release/dev18.3` — docs lag. Tags (abp 10.0.2, orleans v10.0.0, runtime v10.0.2, sk dotnet-1.70.0) are mutable; SHA-pin where feasible. `dotnet-version` wildcards (`10.0.x`/`9.0.x`) and ReportGenerator unversioned install also drift.
