# Squad Decisions

## Active Decisions

### 2026-04-30: Coverage workflow architecture

**By:** Lewis (approved), proposed by Vogel + Watney + Beck

**Decision:**
- Each repo's coverage job runs inside `mcr.microsoft.com/dotnet/sdk:10.0-bookworm-slim` container.
- All target-repo refs are pinned to commit SHAs (resolved 2026-04-30 from local clones).
- All GitHub Actions and the runner image (`ubuntu-24.04`) are SHA-pinned.
- Coverage collection is dual-mode: native `--collect:"XPlat Code Coverage"` for repos that already include coverlet.collector (aspnetcore, efcore, orleans, semantic-kernel), and external `dotnet-coverage` tool for repos that don't (abp, roslyn, runtime). Zero modifications to cloned repos.
- Runtime job installs native deps (cmake, clang, llvm, lld, libicu-dev, etc.) inside the container.
- Test filters skip integration / E2E / quarantined only — no .csproj changes.
- `continue-on-error: true` on test steps so reportgenerator and artifact upload always run.
- Both HTML coverage reports and raw cobertura XML are uploaded as artifacts (90 days retention for XML, 30 for HTML).
- `push:` trigger removed; workflow_dispatch only (avoids surprise CI bills during the experiment).
- `prepare-disk` job NOT used: each container job runs on its own VM, so a host-level cleanup job can't free disk for downstream jobs.

**Pinned target SHAs:**
- abp: `ea4bbb8b517869a9fb735ea5bc05c819c209d0b5` (tag 10.0.2)
- aspnetcore: `ecb199c29cbefb6fcb6aa789436de36e44427a78`
- efcore: `45e3af0273b71919189367bc152a335b69f443c6`
- orleans: `8024faf860549cb960b4b573c1571b379e283daa` (tag v10.0.0)
- roslyn: `02d301627ed5016a4c18acd1a35e5bbc20ff03f0` (release/dev18.3 tip; replaces stale `3f2819f9...`)
- runtime: `9ffface2f3fa6fbbb427793c3230b1626a1fdd84` (tag v10.0.2)
- semantic-kernel: `0c898161a355b0a845aea48de79cb43e2e9435d2` (tag dotnet-1.70.0)

**Pinned action SHAs:**
- actions/checkout: `11bd71901bbe5b1630ceea73d27597364c9af683` (v4.2.2)
- actions/upload-artifact: `b4b15b8c7c6ac21ea08fcf65892d2ee8f75cf882` (v4.4.3)
- actions/cache: `1bd1e32a3bdc45362d1e726936510720a7c30a57` (v4.2.0)

### 2026-04-30: Methodology — finding buildable SDK commits

**By:** Lewis

When a target repo's tags reference internal RC/servicing SDKs that aren't publicly available, run `git log -p --all -- global.json | grep -E "(^commit|version.*10\.0\.10[1-9])"` to locate commits where `global.json` updates to a publicly released SDK (e.g., 10.0.101) rather than an internal `-rc.X` or `-servicing.X` version. This is how the aspnetcore pin `ecb199c29cbefb6fcb6aa789436de36e44427a78` was discovered. Reusable for any dotnet repo whose tags lag public SDK availability.

**Source:** Preserved from pre-Squad scratch note `aspnetcore_build_results.md` (deleted 2026-04-30 with Jasper's authorization).

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

### 2026-04-30: Tracked root-level .md purge

**By:** Lewis (Lead) — authorized by Jasper

**What:** Deleted 13 tracked root-level .md files via `git rm`. All described the superseded `test_orchestrator.py` / ABP-only PoC architecture and predate the current `coverage-orchestrator.yml` + `StaticCallAnalyzer/` design. None contained unique operational facts worth preserving — patterns, commands, and workflow are now captured in README.md, the StaticCallAnalyzer source, and the coverage-orchestrator workflow.

**Deleted:** 00_START_HERE.md, ABP_WORKFLOW.md, AGENT_TOOLS_EXAMPLES.md, ANALYSIS_REPORT.md, DELIVERY_SUMMARY.md, DOCUMENTATION_INDEX.md, DOCUMENTATION_MANIFEST.md, QUICK_REFERENCE.md, QUICK_START.md, TEST_ORCHESTRATOR_INDEX.md, TEST_ORCHESTRATOR_OVERVIEW.md, TEST_ORCHESTRATOR_README.md, TEST_ORCHESTRATOR_REFINEMENT.md.

**Kept:** README.md, LICENSE, csharptune/README.md (unique component-level doc).

**Why:** Default-discard policy. Comprehensive documentation pass deferred until CI is stable.
