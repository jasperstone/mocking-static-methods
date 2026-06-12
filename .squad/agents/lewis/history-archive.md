# Lewis — History Archive

Older `history.md` entries moved here by Scribe on 2026-06-12 to keep the live history
under the summarization threshold. Append-only; preserved verbatim for reference.

---

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

### 2026-06-10 — Budget live: `phase4-tripwire-250` ($250 combined soft cap, ALERT-ONLY)
Vogel created the Azure budget `phase4-tripwire-250` (subscription scope, $250 Monthly, Actual 50/80/100% + Forecasted 100%) ahead of the ~Jun 11 credit reset — it tracks combined marketplace+credit spend = the cap metric. **Caveat for dispatch gating:** Azure budgets ALERT only, they do NOT hard-stop; the real kill switch is still the subscription spending-limit toggle (currently OFF for the soft-cap strategy). A true at-cap auto-cancel (alert → action group → webhook) is an unbuilt follow-up. See decisions.md "phase4-tripwire-250 Azure budget created".

### 2026-06-11 — Budget cleanup: now 3 budgets (informational)
Vogel deleted the redundant `phase3-tripwire-250` (exact twin of phase4 after phase 3 sealed). `phase4-tripwire-250` held at $250 (combined soft cap = $150 credit + $100 card); no "$150 card-begins" threshold added (marketplace models always card-bill, never draw the credit). Net state: `VS_Credit_Budget` ($150), `budget-mockstatic-50` ($50 RG), `phase4-tripwire-250` ($250). See decisions.md "2026-06-11: Budget cleanup".

### 2026-06-11 — Phase ladder RENUMBERED in the reports (forward-looking labels only)
Jasper renumbered the canonical phase ladder; I brought the stale forward-looking roadmap text in the **reports** into line (RESULTS numbers untouched — only labels/projections changed). New ladder:
- Phase 1 = baseline coverage; Phase 2 = agentic no-feedback; Phase 3 = agentic loop (shipped, 14.6% compile / 7.1% run-OK).
- **Phase 4 = agentic loop + testability refactoring tool** (`apply_refactor` introduces a seam — extract-and-override / wrapper-interface-adapter / dependency-parameterization — into prod code before testing; prompts stay generic; isolates effect of a refactoring *capability* on the fixed input set). **MOVED IN** as the immediate next phase.
- **Phase 5 = multi-agent (writer + reviewer + fixer)** — MOVED here from old phase 4.
- **multi-team DROPPED entirely.**

Files that carry forward-looking roadmap text (the ones I edited): `phases/phase2-agentic/REPORT.md` (## Next tiers), `phases/phase2-agentic/COSTS.md` (item 3 "Budget headroom" parenthetical + the phases-3-5 projection table — phase-4 refactoring kept modest at 2-3× ~$33-50, phase-5 multi-agent expensive at 4-6× ~$67-100, remaining total ~$133-200), `phases/phase3-agentic-loop/REPORT.md` (no-[Fact] now "phase 5", runner-csproj-parity generalized to "a later phase", "## Next" section + variance open-question phase labels). **Note the naming mismatch left in place on purpose:** `.squad/` decisions/history and the Azure budget proper noun `phase4-tripwire-250` + `phases/phase4-multiagent/` + `phases/phase5-multiagent/` dirs still use the OLD numbering internally (phase4=multi-agent); those are append-only / proper nouns / out-of-scope and were NOT touched. `phases/README.md` and the `phase5-multiagent/` docs already happen to read multi-agent=phase 5, consistent with the new ladder. Cost specifics: precise phase-5 multi-agent cost model lives in `tools/cost/estimate.py` (`--project-phase5` / historically `--project-phase4`); the COSTS.md table is the older ROUGH projection, kept rough but relabeled.
