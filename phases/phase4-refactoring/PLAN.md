# Phase 4 — Agentic loop + testability refactoring tool: PLAN

> **Status: scaffolding only. No Azure dispatch before run_1 go/no-go.**
>
> This document is the design-of-record for phase 4 before any tokens are
> spent. The runner, prompts, and workflow exist on disk so the next session
> can do production runs once the smoke test is green and the run_1 budget is
> cleared. The `phase4-refactoring.yml` workflow (being stubbed by Vogel) is
> `workflow_dispatch`-only with defaults that point at the mock adapter, so an
> accidental dispatch from the GitHub UI cannot incur token spend.

## Hypothesis

Phase 3 gave a single agent compile + run feedback and a read-only view of the
repository. On the frozen v2 300-cell set it landed **14.6% compile / 7.1%
run-OK**. The ceiling is structural: a large share of the targets are **Mode #1
static call sites** that *cannot be mocked at all* with Moq or NSubstitute,
because the receiver shape gives the test no seam to substitute. No amount of
prompt cleverness fixes a call site that is fundamentally unmockable — the agent
can only assert on whatever the un-isolated path happens to produce.

Phase 4 tests a single, specific contribution: **give the proven phase-3 agent a
refactoring capability.** The agent may introduce a *testability seam* into
production code — a small, behavior-preserving transform that converts an
unmockable Mode #1 site into a mockable one — and then write a test that exploits
the seam. The contribution under examination is **capability/tooling
augmentation, NOT prompt engineering**: the writer's prompt stays generic; the
only new degree of freedom is the `apply_refactor` tool.

### What "Mode #1" means

A Mode #1 site is a static-shaped call that is not directly mockable:

1. **Extension methods on interface receivers** — `EXT` kind. The call looks like
   `receiver.DoThing()` where `DoThing` is a static extension method. The
   interface can be mocked, but the extension method is resolved statically and
   cannot be intercepted.
2. **Non-virtual instance methods on non-sealed concrete classes** — `NonVirtual`
   kind. `concrete.DoThing()` where `DoThing` is a non-virtual instance method.
   The class is subclassable (non-sealed) but the method can't be overridden, so
   a fake can't change its behavior.

Both shapes deny the test a substitution point. The refactor introduces one.

## The `apply_refactor` tool — a constrained transform menu

`apply_refactor` is a **local** tool (no LLM behind it) that edits production
source. It is deliberately **NOT free-form production editing** — it exposes a
fixed menu of three behavior-preserving transforms. **The constraint is the
anti-gaming mechanism**: a bounded menu of named, audited transforms makes it
impossible for the agent to "win" by deleting the call site, stubbing it out, or
rewriting the method to do nothing.

| Transform | Applies to | What it does |
|---|---|---|
| `make_virtual` | `NonVirtual` kind | Marks the target instance method `virtual` so a test can subclass-and-override it (the classic *extract-and-override* seam). The smallest possible change: one keyword. |
| `wrapper_interface` (extract-and-adapter) | `EXT` and `NonVirtual` | Generates an adapter interface plus a thin wrapper class around the receiver type. The consumer is changed to depend on the **interface** (constructor-injected), so a test can substitute a mock for the interface. |
| `parameterize_dependency` | `EXT` and `NonVirtual` | Introduces the dependency as an injected constructor/method parameter — **always via a new defaulted overload that preserves the existing public API** (no breaking change). The test calls the new overload with a fake. |

The tool reports back which transform it applied, the files it touched, and
whether the behavior-preservation guard passed. The agent then writes a test
against the new seam and submits it through the same `submit_test` protocol as
phases 2–3.

## Anti-gaming rules

The whole experiment is only meaningful if a "pass" reflects a genuinely tested
behavior reached through a legitimate seam. The following rules are enforced
mechanically by the tool and the harness, not left to the agent's good faith:

- **No deletion / disabling / no-op.** The refactor must NOT delete, disable, or
  no-op the target call site, and must not change observable behavior. The target
  method must still be invoked on the same logical path.
- **Behavior-preservation guard.** After a refactor is applied, the owning
  production project MUST still build; and if the owning project has an associated
  existing test project, those existing tests MUST still pass. **If the guard
  fails, the refactor is auto-reverted and the cell is recorded as
  `refactor_rejected`.** A rejected refactor cannot be submitted against.
- **API-preserving parameterization.** `parameterize_dependency` must keep a
  default-preserving overload — the existing public signature continues to exist
  and behave identically. No breaking public API.
- **Blast-radius confinement.** All edits are confined to the **owning `.csproj`
  subtree**. The tool refuses edits outside the project that owns the target file.

### What counts as a *legitimate pass*

A cell counts as a legitimate run-OK only if:

1. The submitted test **exercises the target method via the seam** introduced by
   the refactor (i.e. it goes through the override / mocked interface / injected
   fake), and
2. It **asserts on real behavior** the target method observably produces (return
   value, mutation, side effect, or thrown exception) —
3. It does NOT assert trivially (`Assert.True(true)`), and does NOT bypass the
   target site.

A test that compiles and "passes" without touching the target through the seam is
not a legitimate pass and is excluded from the refactor-attributable metric.

## Per-cell lifecycle (snapshot / restore)

Phase 4 is the first phase where the agent **writes to production source**, so
cell isolation is critical — one cell's edits must never contaminate another.

```
for each cell:
  1. snapshot-on-write   capture pristine bytes of any prod file the FIRST
                         time it is about to be edited (lazy snapshot)
  2. agent loop          read_file / list_dir / apply_refactor / submit_test
                           - apply_refactor edits prod source in place
                           - the next compile+run rebuilds prod FROM SOURCE
                             for free (single owning csproj), so the seam is
                             real in the build the test runs against
  3. restore             revert ALL touched files to their snapshotted
                         pristine state, so the repo is byte-identical to
                         its pinned SHA before the next cell starts
  4. log                 record applied refactors + guard outcomes for the cell
```

Because the harness rebuilds **only the single owning csproj from source**, the
seam costs nothing extra to materialize — the existing `compile_only` /
compile-and-run path picks up the edited source automatically. Restore is
unconditional and runs even if the cell errored, so a crashed cell cannot leak a
half-applied refactor into the next one.

## Evaluation design — A/B vs phase 3

The headline comparison is **run-OK% A/B against phase 3 on the identical cells**:
same frozen v2 300-cell target set, same 6-model panel, same compile/run harness.
The only difference between the phase-3 arm and the phase-4 arm is the presence of
`apply_refactor`. Any lift in run-OK% is therefore attributable to the refactoring
*capability*, not to a prompt change or a model swap.

### Metrics

1. **Run-OK%** on the same cells, phase 4 vs phase 3's 7.1%. This is the headline.
2. **Refactor-attributable breakdown** — the subset of cells that pass **ONLY when
   a refactor was applied** (i.e. they were run-fail in phase 3 and are run-OK in
   phase 4 *and* the legitimate-pass test goes through a seam). This isolates the
   causal contribution of the tool.
3. **`refactor_rejected` rate** — how often the behavior-preservation guard
   auto-reverts. A high rate signals the transforms are too aggressive for the
   target population.
4. **Transform-type success** — which of `make_virtual` / `wrapper_interface` /
   `parameterize_dependency` succeeds most often, by Mode #1 kind.

### Predicted bucket conversion

Phase 3's run failures are dominated by structural problems an isolation seam
addresses directly. The buckets a refactor should convert: the cells that failed
because the target was unmockable (no seam to assert through) should move from
run-fail to run-OK once `make_virtual` or `wrapper_interface` gives the test a
substitution point. Buckets that are *not* about mockability (real assertion
failures, environmental setup) are not expected to move. See
[REPORT.md](REPORT.md) for the per-bucket prediction table.

## Success / legitimacy definition

Phase 4 "succeeds" as an experiment (independent of the magnitude of the lift) if:

- The refactor-attributable metric is **non-trivial and clean** — i.e. there is a
  measurable set of cells that pass only because of a legitimate seam, and the
  legitimacy filter is doing real work (some compiling "passes" are correctly
  excluded for bypassing the site).
- The `refactor_rejected` guard fires on genuinely behavior-changing edits and
  the restore step keeps the repo pristine across all cells.

A null or small result is still a publishable finding: it would say that a
constrained automated refactoring capability does *not* move the needle on this
input set, which is itself informative.

## Threats to validity

- **Gaming.** The biggest threat is the agent "passing" by neutering the target.
  Mitigated by (a) the bounded transform menu — no free-form prod edits; (b) the
  no-deletion / no-op rule; (c) the behavior-preservation guard with auto-revert;
  (d) the legitimate-pass filter that requires the test to go through the seam and
  assert on real behavior.
- **Build heterogeneity.** Different repos build differently, and editing prod
  source could in principle break a large dependency graph. **Contained:** the
  harness rebuilds only the **single owning csproj from source**, not the whole
  repo, so the blast radius of any edit is one project. Edits outside the owning
  `.csproj` subtree are refused by the tool.
- **Cell contamination.** Mitigated by lazy snapshot-on-write + unconditional
  restore after every cell (see lifecycle above).
- **Over-attribution.** A cell that would have passed in phase 3 anyway should not
  be credited to the refactor. The refactor-attributable metric explicitly
  requires the cell to have been run-fail in phase 3 *and* the pass to go through
  the seam.

## Cost note

Phase 4 is **one writer LLM + one local `apply_refactor` tool** — there is no
reviewer or fixer LLM, so unlike phase 5 there is no second/third model role
multiplying token spend. The single-LLM-role design is the reason phase 4 is far
cheaper than the multi-agent phase 5.

Reproduce the projection with:

```bash
python3 tools/cost/estimate.py --project-phase4 --cap 250
```

- **Phase 4 run_1 (runs=1) ≈ $214 combined → ~85% of the $250 cap, UNDER cap.**
  Clean go. (~$64 to card, inside the $150 monthly credit.)
- Modeling: a flat `P4R_TOKEN_INFLATION = 1.5` on the phase-3 writer token base
  (refactoring makes the writer take more turns per cell) plus
  `P4R_REFACTOR_CALLS_PER_CELL = 1.2` apply_refactor calls/cell billed at the
  existing `TOOLS_SURCHARGE_PER_CALL` ($0.03375/invocation), exactly like
  read_file / list_dir. The tool is local (zero-token) but the agent-runtime/tool
  surface still bills.
- For context, a full 3-run sweep (runs=3) projects ~$641 (over cap) — but that is
  roughly **half** of phase 5's $1,197, because phase 4 has only one LLM role.
  run_1 is the honest default dispatch and the go/no-go checkpoint.

See decision `2026-06-11: Phase-4 (agentic loop + refactoring tool) cost model
added to estimate.py` for the full derivation.

## Tooling (being built in parallel by Watney)

The phase-4 runner and the refactoring tool are authored to **this same spec**:

- `tools/generation/apply_refactor.py` — the local transform tool implementing the
  three-item menu, the behavior-preservation guard, and snapshot/restore.
- `tools/generation/strategies/agentic_loop_refactor.py` — the writer strategy
  that adds `apply_refactor` to the phase-3 tool set.
- `tools/generation/agentic_refactor_runner.py` — the per-cell runner (mock +
  foundry adapters) that drives the lifecycle above.

## Pre-flight checklist (run BEFORE run_1 dispatch)

- [ ] Mock-adapter smoke test green: the full read → apply_refactor → submit_test
      loop runs end-to-end against fixture responses, with snapshot/restore
      verified (repo byte-identical after the cell).
- [ ] `apply_refactor` unit tests green for all three transforms + the guard's
      auto-revert path.
- [ ] `phase4-tripwire-250` Azure budget live (already created — alert-only).
- [ ] At least one **paid** single-cell smoke test (real foundry adapter) lands a
      well-formed JSONL row in `phases/phase4-refactoring/results/`.
- [ ] Open a pre-flight PR with the dispatch plan (target count, model list, run
      count = 1) and get explicit go/no-go review.

Once green, the production dispatch goes through `phase4-refactoring.yml` with
`mock_llm: "false"` and the full six-model panel.
