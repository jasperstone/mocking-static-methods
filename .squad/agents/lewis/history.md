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
- **Default-discard policy for stale scratch markdown (2026-04-30):** When `.squad/decisions.md` is canonical, untracked root-level scratch .md files (status reports, before/after summaries, ad-hoc analyses) should be deleted by default. Extract any reusable methodology into a decisions inbox entry first, then delete. Do not preserve scratch files "just in case" — they bitrot and confuse future audits. Tracked .md files require separate review before deletion.

## Recent Updates

- 2026-04-30 — Workflow audit: EF Core `release/10.0` is the #1 reproducibility blocker (branch ref). Roslyn workflow correctly SHA-pinned but README still says `release/dev18.3` — docs lag. Tags (abp 10.0.2, orleans v10.0.0, runtime v10.0.2, sk dotnet-1.70.0) are mutable; SHA-pin where feasible. `dotnet-version` wildcards (`10.0.x`/`9.0.x`) and ReportGenerator unversioned install also drift.

## Learnings

### 2026-04-30 — Documentation strategy
- README.md is the consolidated docs target for this repo. Auxiliary root-level .md files accumulated during PoC iterations are disposable.
- Comprehensive documentation refresh is a post-CI-stable task; do not block on it.
- Default-discard policy applies to scratch/legacy .md files unless they hold unique operational facts.
