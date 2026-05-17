"""Phase 3 failure-mode taxonomy.

Scans phase 3 attempts.jsonl files and buckets every failed submission by
compile-error code (CS####) or runtime-failure family. Useful for spotting
prompt-side fix candidates (e.g. the "no [Fact]" bucket) and dominant
compiler errors (e.g. CS0246 type-not-found).

Usage::

    python3 tools/analysis/phase3_taxonomy.py
    python3 tools/analysis/phase3_taxonomy.py --phase-dir phases/phase3-agentic-loop
    python3 tools/analysis/phase3_taxonomy.py --json   # machine-readable

The script is read-only — it never writes back into ``results/``.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Iterable, Iterator

REPO_ROOT = Path(__file__).resolve().parents[2]
DEFAULT_PHASE_DIR = REPO_ROOT / "phases" / "phase3-agentic-loop"

# CS0123 / CS9876 — five-digit forms tolerated.
CS_CODE_RE = re.compile(r"CS\d{3,5}")

# Runtime-failure family signatures, evaluated against test-failure messages
# in priority order; first match wins so more specific rules go first.
RUNTIME_FAMILIES: list[tuple[str, re.Pattern[str]]] = [
    ("moq_unmockable_static", re.compile(r"Unsupported expression.*static", re.I)),
    ("type_or_method_load", re.compile(r"TypeLoad|MethodLoad|FileLoad|FileNotFoundException", re.I)),
    ("arg_null", re.compile(r"ArgumentNullException", re.I)),
    ("null_ref", re.compile(r"NullReferenceException", re.I)),
    ("invalid_op_runtime", re.compile(r"InvalidOperationException", re.I)),
    ("assertion_failed", re.compile(r"Assert\.|Xunit\.Sdk|FailException", re.I)),
]


def iter_attempts(phase_dir: Path) -> Iterator[tuple[str, dict]]:
    """Yield (model_id, attempt) tuples for every attempts.jsonl row."""
    for path in sorted(phase_dir.glob("results/*/run_*/attempts.jsonl")):
        model_id = path.parent.parent.name
        with path.open() as fh:
            for line in fh:
                line = line.strip()
                if not line:
                    continue
                try:
                    yield model_id, json.loads(line)
                except json.JSONDecodeError:
                    continue


def classify_compile_iteration(it: dict) -> Iterable[str]:
    """Yield CS codes from a non-compiling iteration. Falls back to '(no_data)'
    when stdout was empty (typical build-timeout signature)."""
    errors = it.get("first_compile_errors") or []
    if not errors:
        yield "(no_data)"
        return
    seen: set[str] = set()
    for err in errors:
        code = (err.get("code") or "").upper()
        if not code:
            msg = err.get("message") or ""
            match = CS_CODE_RE.search(msg)
            code = match.group(0) if match else "(unknown)"
        if code not in seen:
            seen.add(code)
            yield code


def classify_runtime_iteration(it: dict) -> Iterable[str]:
    """Yield runtime-failure family labels from a compiled-but-failed
    iteration. ``no_fact_methods`` is emitted when the test class compiled
    but xUnit found no tests to execute."""
    if it.get("tests_total", 0) == 0:
        yield "no_fact_methods"
        return
    failures = it.get("first_test_failures") or []
    if not failures:
        yield "other_exception"
        return
    seen: set[str] = set()
    for failure in failures:
        msg = " ".join(filter(None, [failure.get("message"), failure.get("stack_tail")]))
        label = "other_exception"
        for name, pattern in RUNTIME_FAMILIES:
            if pattern.search(msg):
                label = name
                break
        if label not in seen:
            seen.add(label)
            yield label


def analyse(phase_dir: Path) -> dict:
    compile_buckets: Counter[str] = Counter()
    runtime_buckets: Counter[str] = Counter()
    per_model_compile: dict[str, Counter[str]] = defaultdict(Counter)
    per_model_runtime: dict[str, Counter[str]] = defaultdict(Counter)
    iterations_seen = 0
    attempts_seen = 0

    for model_id, attempt in iter_attempts(phase_dir):
        attempts_seen += 1
        for it in attempt.get("submission_iterations") or []:
            iterations_seen += 1
            if not it.get("compile_ok"):
                for code in classify_compile_iteration(it):
                    compile_buckets[code] += 1
                    per_model_compile[model_id][code] += 1
                continue
            if not it.get("run_ok"):
                for label in classify_runtime_iteration(it):
                    runtime_buckets[label] += 1
                    per_model_runtime[model_id][label] += 1

    return {
        "phase_dir": str(phase_dir.relative_to(REPO_ROOT)),
        "attempts_seen": attempts_seen,
        "iterations_seen": iterations_seen,
        "compile_failures": dict(compile_buckets.most_common()),
        "runtime_failures": dict(runtime_buckets.most_common()),
        "per_model_compile": {m: dict(c.most_common()) for m, c in per_model_compile.items()},
        "per_model_runtime": {m: dict(c.most_common()) for m, c in per_model_runtime.items()},
    }


def render_text(report: dict) -> str:
    lines: list[str] = []
    lines.append(f"# Phase 3 failure taxonomy — {report['phase_dir']}")
    lines.append("")
    lines.append(f"Attempts scanned: {report['attempts_seen']}")
    lines.append(f"Submission iterations scanned: {report['iterations_seen']}")
    lines.append("")
    lines.append("## Compile-error families")
    for code, n in report["compile_failures"].items():
        lines.append(f"  {code:<12} {n}")
    lines.append("")
    lines.append("## Runtime-failure families")
    for label, n in report["runtime_failures"].items():
        lines.append(f"  {label:<26} {n}")
    return "\n".join(lines)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--phase-dir", default=str(DEFAULT_PHASE_DIR),
                        help="Path to the phase directory (default: phases/phase3-agentic-loop)")
    parser.add_argument("--json", action="store_true", help="Emit machine-readable JSON")
    args = parser.parse_args(argv)

    phase_dir = Path(args.phase_dir).resolve()
    if not phase_dir.exists():
        print(f"phase dir not found: {phase_dir}", file=sys.stderr)
        return 2

    report = analyse(phase_dir)
    if args.json:
        json.dump(report, sys.stdout, indent=2)
        sys.stdout.write("\n")
    else:
        print(render_text(report))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
