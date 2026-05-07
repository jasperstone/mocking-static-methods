### 2026-05-06: Composite actions for coverage orchestrator
**By:** Vogel (via Copilot, requested by jastone)
**What:** Factored 350+ lines of duplicated step bodies across 7 jobs into 5 composite actions under .github/actions/. Workflow shrunk from 1102 lines to 727.
**Why:** Maintainability — fixes to the validator/uploader/cache previously required 5–7 simultaneous edits. Composite actions centralize the canonical implementation.
**Scope:** No behavior changes. Roslyn and ASP.NET Core failures from run 25458463158 are unaddressed and will be tackled in follow-up commits.
