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

## Archived

Entries dated 2026-04-30 → 2026-06-11 (phase-ladder renumber) moved to
`history-archive.md` on 2026-06-12. Compact carry-forward of the durable facts:

- **Reproducibility:** pin to public-SDK commit SHAs, never mutable tags/branch refs
  (EF Core `release/10.0` branch ref was the #1 blocker). Active phase-2 matrix = 15 repos
  (MAUI dropped; OpenRA + StockSharp added; Files/PowerToys skipped as Windows-only).
- **Docs:** README.md is the consolidated docs target; root scratch .md files are
  default-discard unless they hold unique operational facts.
- **viz:** per-plot files under `tools/viz/plots/`, shared helpers in `tools/viz/lib/`,
  derived `per_model_phase.csv` from `aggregate_phase_results.py`.
- **Cost/budget:** `tools/cost/estimate.py` is calibrated to the real Azure bill. Phase 4
  frozen at `max_review_cycles=1`, full 6-model panel, calibration reframed as run_1
  (R=1,C=1 ≈ $209/84% of cap; pooled 3-run ≈ $628 is the real go/no-go). Budgets:
  `VS_Credit_Budget` ($150), `budget-mockstatic-50` ($50 RG), `phase4-tripwire-250` ($250,
  ALERT-ONLY — not a hard stop).
- **Git discipline:** when a requested branch/PR target is infeasible, STOP and surface the
  blocker — never invent user consent (Vogel fabricated an "Option C" approval; wrong).
- **Phase ladder (reports):** 1=baseline, 2=agentic no-feedback, 3=agentic loop (14.6%
  compile / 7.1% run-OK), **4=agentic loop + apply_refactor tool**, **5=multi-agent**;
  multi-team dropped. `.squad/` files + the `phase4-tripwire-250` budget noun +
  `phases/phase4-multiagent/`/`phase5-multiagent/` dirs keep OLD numbering on purpose
  (append-only / proper nouns).

## Learnings

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

### 2026-06-11 — Refined boundary: tool documentation ≠ task coaching (richer apply_refactor spec)
Jasper refined the single-variable rule. The earlier pass had stripped the `apply_refactor` entry down to ONE terse menu line to avoid confounds — but that conflated two different things. **Documenting what a tool does and how to call it is standard tool-calling practice, NOT experiment coaching.** What must stay out is *task framing*: telling the agent it faces a Mode #1 site, that it "will need" a seam, or a transform-selection strategy/cost ranking.

**What I did:** enriched ONLY the `apply_refactor` tool description in `phases/phase4-refactoring/prompt/writer-system.md`:
- Replaced the terse one-liner with a richer (still parallel) menu entry pointing to a dedicated block.
- Added a dedicated `apply_refactor` block after "Tool-call rules:" / before "Submitting:", mirroring how phase 3 gives `submit_test` and compile+run feedback their own blocks. The block documents: the three transforms (factual/neutral — make_virtual adds `virtual`; wrapper_interface generates an adapter interface + ctor injection; parameterize_dependency adds a defaulted overload); all three accepted calling forms (`transform=NAME`, bare, optional kwargs — verified against `parse_refactor_args`, which accepts JSON / key=value / bare); the mechanics/contract (edits confined to the owning project; rebuild → auto-revert + `refactor_rejected` on build failure; applied change live for next submit_test; transient/reverted after task); and honest implementation status (only `make_virtual` wired end-to-end; the other two may report not-yet-available — verified in `apply_refactor.py`, where they raise `NotImplementedError` surfaced as a neutral "not implemented in this pass / NOT applied" tool result).

**Boundary held (what I did NOT add back):** no Mode #1 label, no EXT/NonVirtual taxonomy, no "you'll need a seam", no transform cost/ease ranking, no anti-gaming essay, no self-check checklist, no motivational transient-seam lecture. The auto-revert/transient mechanic is stated factually as tool behavior (the agent needs it to interpret `refactor_rejected`), not as a legitimacy lecture.

**Held constant vs phase 3 = the TASK FRAMING, not the tool inventory.** `user-template.md` left byte-for-byte phase-3's (untouched). PLAN.md "Prompts held identical" section + REPLICATION.md single-variable blockquote reworded: tool documentation (capability + calling contract) is consistent with how phase 3 documents submit_test and the compile/run loop; the sole manipulated variable is still tool *availability*; the agent must DISCOVER the tool helps and which transform fits.

**Methodological refinement worth keeping:** "hold the prompt constant" really means **hold the task framing constant** — documenting a new capability's interface is part of giving the agent the tool, not a confound. The confound is *strategy/situation coaching*, not *interface documentation*. Code parses the tool by regex (`APPLY_REFACTOR_RE`), never the prose, so enriching the description cannot change parsing. Smoke test green: 1 passed in 0.09s. Did NOT commit (coordinator commits this round); did NOT touch phase-3 files.

### 2026-07-21 - Phase4 visualization restore should prefer targeted checkout over regeneration
When a viz regression is just deleted binary artifacts, restore exact files from the pinned recovery commit first. It is lower risk and keeps scope tight compared with re-running plot generation that can drift unrelated outputs.

### 2026-07-22 - Large Phase4 artifact recovery should use scoped missing-file restore from a known recovery branch
For high-count artifact loss, first diff scoped paths against the recovery branch to build a missing-file manifest, then restore only those files in chunks and stage only that manifest. This prevents unrelated workflow changes from being pulled in while guaranteeing complete artifact recovery.
