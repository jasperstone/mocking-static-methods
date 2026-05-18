"""One-shot consolidator: take per-(model, repo, run) eval artifacts from
`/tmp/p3_runs23/eval/` and merge into the canonical phase 3 layout at
`phases/phase3-agentic-loop/results/<model>/run_{N}/`.

Eval artifacts are a superset of gen artifacts (they contain the attempts.jsonl
that gen produced PLUS the evaluation.jsonl). So eval is the source of truth.

Re-runnable: clears destination run_N/ dirs before writing so partial state
never leaks in.
"""
from __future__ import annotations

import re
import shutil
import sys
from pathlib import Path

SRC = Path("/tmp/p3_runs23/eval")
DST_ROOT = Path("phases/phase3-agentic-loop/results")
MODELS = [
    "codestral-2501", "gpt-4.1-mini", "gpt-4.1-nano",
    "grok-4-1-fast", "llama-3.3-70b-instruct", "phi-4",
]
RUNS = [2, 3]

# Artifact name: eval-{model}-{repo}-run{N}
# Models contain hyphens, so anchor on the trailing -run{N} and the leading eval-.
ARTIFACT_RE = re.compile(r"^eval-(?P<model>.+)-(?P<repo>[^-]+)-run(?P<run>\d+)$")


def consolidate() -> int:
    if not SRC.exists():
        print(f"SRC not found: {SRC}", file=sys.stderr)
        return 1

    # Index source artifacts by (model, run).
    by_key: dict[tuple[str, int], list[Path]] = {}
    for art in sorted(SRC.iterdir()):
        if not art.is_dir():
            continue
        # Greedy match: try each model so we don't mis-split hyphenated names.
        match = None
        for m in MODELS:
            for r in RUNS:
                stem = f"eval-{m}-"
                tail = f"-run{r}"
                if art.name.startswith(stem) and art.name.endswith(tail):
                    repo = art.name[len(stem):-len(tail)]
                    match = (m, r, repo)
                    break
            if match:
                break
        if not match:
            print(f"  SKIP (unparseable): {art.name}", file=sys.stderr)
            continue
        model, run, _repo = match
        by_key.setdefault((model, run), []).append(art)

    # Clear and rebuild each destination run dir.
    summary: list[tuple[str, int, int, int, int]] = []
    for (model, run), arts in sorted(by_key.items()):
        dst = DST_ROOT / model / f"run_{run}"
        if dst.exists():
            shutil.rmtree(dst)
        (dst / "generated_tests").mkdir(parents=True, exist_ok=True)

        attempts_path = dst / "attempts.jsonl"
        eval_path = dst / "evaluation.jsonl"
        n_attempts = n_evals = n_tests = 0

        with attempts_path.open("w") as a_out, eval_path.open("w") as e_out:
            for art in sorted(arts):
                # Source files live under {art}/{model}/run_{run}/.
                base = art / model / f"run_{run}"
                if not base.exists():
                    print(f"  WARN: expected base {base} missing", file=sys.stderr)
                    continue
                a_src = base / "attempts.jsonl"
                if a_src.exists():
                    text = a_src.read_text()
                    if text and not text.endswith("\n"):
                        text += "\n"
                    a_out.write(text)
                    n_attempts += sum(1 for _ in text.splitlines())
                e_src = base / "evaluation.jsonl"
                if e_src.exists():
                    text = e_src.read_text()
                    if text and not text.endswith("\n"):
                        text += "\n"
                    e_out.write(text)
                    n_evals += sum(1 for _ in text.splitlines())
                tests_src = base / "generated_tests"
                if tests_src.exists():
                    for cs in tests_src.rglob("*.cs"):
                        rel = cs.relative_to(tests_src)
                        target = dst / "generated_tests" / rel
                        target.parent.mkdir(parents=True, exist_ok=True)
                        shutil.copy2(cs, target)
                        n_tests += 1

        summary.append((model, run, n_attempts, n_evals, n_tests))
        print(f"  {model:<25} run_{run}  attempts={n_attempts:>3}  eval={n_evals:>3}  tests={n_tests:>3}  (from {len(arts)} artifacts)")

    print("\nTotals:")
    total_a = sum(s[2] for s in summary)
    total_e = sum(s[3] for s in summary)
    total_t = sum(s[4] for s in summary)
    print(f"  attempts={total_a}  eval={total_e}  tests={total_t}")
    return 0


if __name__ == "__main__":
    raise SystemExit(consolidate())
