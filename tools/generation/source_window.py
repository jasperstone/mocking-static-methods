"""Pull a window of source lines around a target line, plus repo-level
test conventions (test framework, target framework, sample test class).

The window size is fixed at 30 lines before/after the target. The marker
'>>>' is prepended to the target line so the model can locate it.
"""
from __future__ import annotations
from dataclasses import dataclass
from pathlib import Path

WINDOW_BEFORE = 30
WINDOW_AFTER = 30


@dataclass
class SourceWindow:
    text: str
    start_line: int
    end_line: int


def read_window(repo_root: Path, file_rel: str, target_line: int) -> SourceWindow:
    p = repo_root / file_rel
    lines = p.read_text(encoding="utf-8", errors="replace").splitlines()
    start = max(1, target_line - WINDOW_BEFORE)
    end = min(len(lines), target_line + WINDOW_AFTER)
    out: list[str] = []
    for i in range(start, end + 1):
        prefix = ">>> " if i == target_line else "    "
        out.append(f"{prefix}{lines[i - 1]}")
    return SourceWindow(text="\n".join(out), start_line=start, end_line=end)
