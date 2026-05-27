#!/usr/bin/env python3
"""Bundle markdown + prompt files + small CSVs for dissertation context upload.

Outputs:
  dissertation_bundle/dissertation_context.md   — one concatenated markdown file
  dissertation_bundle/dissertation_csvs/        — small CSVs preserved as files
  dissertation_bundle/MANIFEST.txt              — what went in, sizes

Excludes large/generated artifacts, vendored code, and the orphan
phase2-singleshot scaffold.
"""
from __future__ import annotations

import os
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "dissertation_bundle"
OUT_MD = OUT / "dissertation_context.md"
OUT_CSV_DIR = OUT / "dissertation_csvs"
MANIFEST = OUT / "MANIFEST.txt"

# Directories to skip entirely (anywhere in the tree).
EXCLUDE_DIRS = {
    "cloned_repos",
    "baseline_artifacts",
    "discovery_artifacts",
    "test_logs",
    "bin",
    "obj",
    ".venv",
    ".git",
    "node_modules",
    "results",                  # per-cell JSONL/test outputs, not narrative
    "results_v1_oldprompt",
    "generated_tests",
    "coverage",
    "errors",
    "reports",                  # phase1-baseline auto-generated subreports
    "phase2-singleshot",        # orphan scaffold — real phase 2 is phase2-agentic
    "assets",                   # PNG figures — not useful to an LLM
}

# Filenames to skip even if they match the extension globs.
EXCLUDE_FILES = {
    "REPORT_PHASE1_LEGACY_7REPO.md",   # superseded by REPORT.md
    "COSTS_AUTOGEN.md",                # machine-regenerated; COSTS.md is the narrative
}

# Markdown + prompt-text extensions.
INCLUDE_EXT_MD = {".md"}
INCLUDE_EXT_TXT = {".txt"}     # only under */prompt/ or test_prompts/

# Small CSVs we want as standalone files in the bundle.
CSV_ALLOWLIST = [
    "baseline_coverage.csv",
    "test_counts.csv",
    "test_discovery_summary.csv",
    "targets/v1/targets.csv",
    "targets/v2/targets.csv",
    "targets/v1/covered_sites_analysis.csv",
]

# Hard cap per concatenated file (just in case something huge sneaks in).
MAX_FILE_BYTES = 500_000


def is_excluded(path: Path) -> bool:
    parts = set(path.relative_to(ROOT).parts)
    if parts & EXCLUDE_DIRS:
        return True
    if path.name in EXCLUDE_FILES:
        return True
    return False


def is_prompt_txt(path: Path) -> bool:
    rel = path.relative_to(ROOT).as_posix()
    return "/prompt/" in rel or rel.startswith("csharptune/test_prompts/")


def collect_markdown() -> list[Path]:
    out: list[Path] = []
    for dirpath, dirnames, filenames in os.walk(ROOT):
        # Prune excluded dirs in-place for speed.
        dirnames[:] = [d for d in dirnames if d not in EXCLUDE_DIRS and not d.startswith(".")]
        for name in filenames:
            p = Path(dirpath) / name
            if is_excluded(p):
                continue
            suf = p.suffix.lower()
            if suf in INCLUDE_EXT_MD:
                out.append(p)
            elif suf in INCLUDE_EXT_TXT and is_prompt_txt(p):
                out.append(p)
    # Stable order: by relative path.
    out.sort(key=lambda p: p.relative_to(ROOT).as_posix())
    return out


def write_bundle(files: list[Path]) -> tuple[int, int]:
    """Concatenate files into OUT_MD. Return (count, bytes)."""
    OUT.mkdir(parents=True, exist_ok=True)
    total_bytes = 0
    count = 0
    with OUT_MD.open("w", encoding="utf-8") as fh:
        fh.write("# Dissertation context bundle\n\n")
        fh.write(
            "This file is the concatenation of every narrative markdown and prompt "
            "template in the `mocking-static-methods` repository (excluding vendored "
            "code, generated artifacts, and figures). Each section is delimited by "
            "an `=== path/to/file ===` header so the LLM can cite individual sources.\n\n"
        )
        fh.write("---\n\n")
        for p in files:
            rel = p.relative_to(ROOT).as_posix()
            try:
                data = p.read_bytes()
            except OSError as e:
                print(f"skip {rel}: {e}")
                continue
            if len(data) > MAX_FILE_BYTES:
                print(f"skip {rel}: {len(data)} bytes > cap {MAX_FILE_BYTES}")
                continue
            try:
                text = data.decode("utf-8")
            except UnicodeDecodeError:
                print(f"skip {rel}: not utf-8")
                continue
            fh.write(f"\n\n=== {rel} ===\n\n")
            fh.write(text.rstrip())
            fh.write("\n")
            total_bytes += len(data)
            count += 1
    return count, total_bytes


def copy_csvs() -> list[tuple[str, int]]:
    OUT_CSV_DIR.mkdir(parents=True, exist_ok=True)
    rows: list[tuple[str, int]] = []
    for rel in CSV_ALLOWLIST:
        src = ROOT / rel
        if not src.exists():
            print(f"csv missing: {rel}")
            continue
        # Flatten path into filename so the bundle dir is flat.
        flat = rel.replace("/", "__")
        dst = OUT_CSV_DIR / flat
        dst.write_bytes(src.read_bytes())
        rows.append((rel, dst.stat().st_size))
    return rows


def write_manifest(md_count: int, md_bytes: int, csvs: list[tuple[str, int]]) -> None:
    with MANIFEST.open("w", encoding="utf-8") as fh:
        fh.write("Dissertation context bundle — manifest\n")
        fh.write("=" * 50 + "\n\n")
        fh.write(f"Concatenated markdown: {md_count} files, {md_bytes:,} bytes\n")
        fh.write(f"  → {OUT_MD.relative_to(ROOT)}\n\n")
        fh.write(f"CSVs ({len(csvs)} files):\n")
        for rel, size in csvs:
            fh.write(f"  {size:>10,}  {rel}\n")


def main() -> None:
    files = collect_markdown()
    md_count, md_bytes = write_bundle(files)
    csvs = copy_csvs()
    write_manifest(md_count, md_bytes, csvs)
    print(f"Wrote {OUT_MD.relative_to(ROOT)}: {md_count} files, {md_bytes:,} bytes")
    print(f"Wrote {len(csvs)} CSVs to {OUT_CSV_DIR.relative_to(ROOT)}/")
    print(f"Manifest: {MANIFEST.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
