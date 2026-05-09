# Phase _template_

Copy this directory when starting a new phase:

```bash
cp -r phases/_template phases/phase3-singleagent
```

Then:

1. Fill in `phase.lock.yaml` as the phase progresses; every field MUST be populated before sealing.
2. Drop generated test source files under `generated_tests/{repo}/`.
3. Drop compile/runtime error logs under `errors/{repo}/`.
4. Drop cobertura XML output under `coverage/{repo}/`.
5. Write the phase narrative in `REPORT.md`.
6. Append a new row to the root [`RESULTS.md`](../../RESULTS.md) table.
7. Tag the seal commit `phase-N-final` and never edit this directory again. Bug fixes go in the next phase.

## Subdirectories

| Path | Contents |
|---|---|
| `phase.lock.yaml` | All inputs needed to reproduce this phase |
| `REPORT.md` | Phase narrative — what was tried, what worked, what didn't |
| `coverage/{repo}/` | Cobertura XMLs from this phase's coverage run |
| `generated_tests/{repo}/` | Test files produced by the generation step (empty for baseline phases) |
| `errors/{repo}/` | Compile-fail and runtime-fail logs per attempted target (empty for baseline phases) |

## Why this layout

- **Tooling** (analyzers, orchestrator workflow, target builder) lives **outside** `phases/` in `tools/` and `.github/`. It evolves freely; phase reproducibility is anchored by the SHAs recorded in `phase.lock.yaml`, not by frozen copies of the tooling.
- **Inputs** (which Mode#1 sites we attempt) live in versioned `targets/v{N}/`. Phases pin to a specific version.
- **Outputs** (tests, errors, coverage) live in this phase directory.
- **The comparison table** lives once at the repo root in [`RESULTS.md`](../../RESULTS.md).
