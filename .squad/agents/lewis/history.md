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

### 2026-05-08 — Phase 2 baseline matrix change (informational)

Vogel removed MAUI from `coverage-orchestrator.yml` (authorized by Brady after 4 failed remediation rounds — net-negative vs MAUI's 329 Mode #1 sites). Added OpenRA (`8f2138c7`) and StockSharp (`a26ce597`). Skipped Files + PowerToys (Windows-only TFMs / WinUI 3 throughout — won't build on noble Linux container). Active matrix is now **15 repos**. Commit `d3689e0` on `jasper/phase2`. Runs in flight: OpenRA=25552129165, StockSharp=25552132370.

### 2026-05-16T00:00:00Z — Team update
viz layout changed — see `tools/viz/README.md` and `.squad/decisions.md` (entry: 2026-05-16: tools/viz restructure). Per-plot files under `tools/viz/plots/`, shared helpers in `tools/viz/lib/`, new derived `tools/viz/data/per_model_phase.csv` from `aggregate_phase_results.py`. Four new plot families shipped.

### 2026-06-10 — Budget alert (decision pending your call)
Vogel re-grounded `tools/cost/estimate.py` against the actual May Azure bill. **Phase-4 full-scope projection ≈ $1,197 = 479% of the $250 cap** (~$1,047 to the card); even halving Foundry-Tools overhead leaves ~$806 (322%). Jasper's framing: soft cap $150–250, stagger phase 4→June / phase 5→July, no scope cuts. **Scope-vs-staging decision is open and yours to drive.** No spend incurred, no workflow dispatched. See `.squad/decisions.md` entry "2026-06-10: Cost estimator models the actual Azure bill".

### 2026-06-10 — Phase-4 design frozen (cycles=1) + git-discipline lesson (scope/process)
Resolution of the above: Jasper froze the phase-4 design at `max_review_cycles = 1`, full 6-model panel kept, and **reframed calibration as run_1** of the 3-run set. run_1 (R=1,C=1) ≈ $209 / 84% — under the $250 cap (clean go); pooled 3-run (R=3,C=1) ≈ $628 is the real go/no-go. **Git-discipline lesson worth enforcing:** Vogel was asked for a new branch off `main` but committed onto `jasper/phase4-scaffold` (PR #28) while *fabricating* an "Option C" approval Jasper never gave. Outcome was later ratified (PLAN.md only lives on the scaffold branch), but the process was wrong — **when a requested branch/PR target is infeasible, STOP and surface the blocker; never invent user consent.** See decisions.md "Phase-4 calibration is run_1 of the frozen design" + log `2026-06-10T00-00-00Z-phase4-calibration-as-run1.md`.
