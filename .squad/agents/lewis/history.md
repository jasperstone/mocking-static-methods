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

### 2026-06-11 — Phase-4 (refactoring tool) experiment directory scaffolded + design-of-record authored
Created the NEW `phases/phase4-refactoring/` experiment dir on branch `jasper/phase4-refactoring`, mirroring the phase-5 layout. This is the design-of-record; Watney built the tool to the same spec in parallel.

**The design (authoritative spec):**
- **Goal:** measure the effect of giving the proven phase-3 single agent a *refactoring capability*. Headline = run-OK% A/B vs phase-3's 7.1% on the IDENTICAL frozen v2 300-cell set, same 6-model panel, same compile/run harness. Contribution is capability/tooling augmentation, NOT prompt engineering (writer prompt stays generic).
- **Mode #1 sites** = (1) extension methods on interface receivers (EXT); (2) non-virtual instance methods on non-sealed concrete classes (NonVirtual). Both unmockable as-is → the refactor adds a seam.
- **`apply_refactor` tool** = local (no LLM), a CONSTRAINED 3-item menu (the constraint is the anti-gaming mechanism, NOT free-form prod editing): `make_virtual` (extract-and-override, NonVirtual only), `wrapper_interface` (extract-and-adapter interface + ctor injection), `parameterize_dependency` (injected param via NEW defaulted overload, no breaking API).
- **Anti-gaming:** no delete/disable/no-op of the target site; no observable behavior change; parameterize keeps a default overload; edits confined to the owning .csproj subtree. **Behavior-preservation guard:** after a refactor, the owning prod project must still build AND (if it has an associated test project) its existing tests must still pass — else auto-revert + record `refactor_rejected`. **Legitimate pass** = test exercises the target THROUGH the seam and asserts on real behavior (no trivial assert, no bypass).
- **Per-cell lifecycle:** snapshot-on-write (lazy) of any prod file before first edit → agent loop (read_file/list_dir/apply_refactor/submit_test; compile+run rebuilds the single owning csproj from source for free) → UNCONDITIONAL restore of all touched files to pristine after the cell (cells never contaminate each other) → log applied refactors + guard outcomes.
- **Metrics:** run-OK% vs phase 3; **refactor-attributable** breakdown (cells passing ONLY when a legitimate refactor was applied); `refactor_rejected` rate; transform-type success by kind.
- **Cost:** ONE writer LLM + a local tool (no reviewer/fixer) → run_1 (runs=1) ≈ $214 / ~85% of the $250 cap, clean go (`tools/cost/estimate.py --project-phase4`). Full 3-run ≈ $641, ~half of phase-5's $1,197.

**File locations created:** `phases/phase4-refactoring/{PLAN.md, REPLICATION.md, REPORT.md, phase.lock.yaml, prompt/writer-system.md, prompt/user-template.md, results/.gitkeep}`. Lock copies phase-5's exact `targets_sha256` (4db523f9…) + count 300, infra, repo SHA pins, and 6-model panel; adds a `refactoring:` section (menu + guard + lifecycle) and `agent_topology: single_writer_refactor`; `runs_per_model: 1` (run_1). REPLICATION reuses the EXISTING `phase4-tripwire-250` budget noun (do NOT invent a new one) and references the `phase4-refactoring.yml` workflow Vogel is stubbing. Single-agent → NO reviewer/fixer prompts (unlike phase 5). REPORT is explicitly predictions-only. Tool paths Watney owns: `tools/generation/apply_refactor.py`, `tools/generation/strategies/agentic_loop_refactor.py`, `tools/generation/agentic_refactor_runner.py`. No Azure spend; no workflow dispatched.

### 2026-06-11 — Phase-4 prompts held IDENTICAL to phase 3 (single-variable control; fixes a prompt-engineering confound)
Jasper caught this reviewing PR #30: the first phase-4 prompts introduced MULTIPLE confounds vs phase 3 (Mode #1 explanation, seam concept, coached transform menu — "make_virtual is the cheapest", an anti-gaming essay, a transient-seam paragraph, a self-check checklist, a 12-turn budget phase 3 lacks, and a "you'll likely need a seam" line in the user template). That makes any run-OK delta un-attributable: it could be the tool OR the prompt engineering.

**Design decision (now of-record):** phase-4 prompts are held **identical to phase 3 (the control)**. `writer-system.md` = phase-3 `system.md` **verbatim** with **exactly ONE addition** — a single factual tool-menu line for `apply_refactor` (same terse style as read_file/list_dir, no coaching). `user-template.md` = phase-3 `user-template.md` **byte-for-byte** (dropped the Mode #1/seam sentence and the {{TEST_FRAMEWORK}}/{{TARGET_TFM}} vars phase 3 doesn't have). **The SOLE manipulated independent variable is the availability of `apply_refactor`.** Anti-gaming + behavior-preservation live in the HARNESS and are surfaced to the agent ONLY via tool feedback (`refactor_rejected`), never pre-coached — exactly like compile/run errors in phase 3. The agent must DISCOVER the tool helps. Net effect: any run-OK% delta vs phase 3 is attributable to the tool, not prompt wording.

**Methodological rule worth keeping:** when a new phase adds a capability (a tool, a role, feedback), hold EVERYTHING else (prompts, panel, harness, params) constant against the prior phase. Push enforcement/coaching into the harness + tool feedback, never the prompt, or the manipulation is confounded. **One change confound check:** before dispatch, diff the new phase's prompt against the control — it must differ by only the one declared variable.

**One subtlety:** phase-3's intro said "read-only tool access"; since `apply_refactor` writes prod source, I changed that one phrase to "tool access" (matches the old phase-4 wording). Leaving "read-only" would have biased the agent AGAINST using the treatment tool — a confound in the opposite direction. Not coaching; a factual correction. All enumerated keep-byte-for-byte sections (tool-call rules, 8-read budget, submit format, compile+run feedback) are unchanged. Removed the 12-turn budget (phase 3 has none).

**Verified:** runner (`agentic_refactor_runner.py`) and strategy (`agentic_loop_refactor.py`) parse `<tool>apply_refactor(...)</tool>` by REGEX (`APPLY_REFACTOR_RE`), not prompt prose — confirmed by grep, so the prose removal is safe. Smoke test green: `python -m pytest tools/generation/tests/test_refactor_smoke.py -q` → 1 passed in 0.10s. Did NOT commit (coordinator commits this round); did NOT touch phase-3 files. PLAN.md + REPLICATION.md now state the single-variable design explicitly; REPORT.md already said "writer prompt stays generic" (consistent, untouched).
