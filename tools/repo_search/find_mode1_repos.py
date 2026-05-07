#!/usr/bin/env python3
"""
Find C# repos that heavily use Mode #1 mockability-failure patterns.

Mode #1 = "compiles but fails at runtime when you try to mock it":
  - Extension methods on interfaces (ILogger.LogWarning, etc.)
  - Non-virtual instance methods on concrete classes (HttpClient.GetAsync, etc.)

Strategy:
  1. Use GitHub repository search to get a candidate pool of popular C# repos.
  2. For each candidate, query code search for each Mode #1 pattern, restricted
     to that repo. Sum the counts.
  3. Rank by total Mode #1 hits and emit a CSV + markdown table.

Output:
  - tools/repo_search/mode1_candidates.csv
  - tools/repo_search/MODE1_CANDIDATES.md

Usage:
  GITHUB_TOKEN=... python3 tools/repo_search/find_mode1_repos.py \\
      [--min-stars 1000] [--max-repos 200] [--top 25] [--dry-run]

Notes:
  - GitHub authenticated code search is rate-limited to 30 req/min.
  - This tool sleeps between queries and backs off on 403.
  - Already-cloned repos in cloned_repos/ are seeded into the candidate set
    automatically so the existing 7 always appear in the ranking.
"""
from __future__ import annotations

import argparse
import csv
import json
import os
import sys
import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable
from urllib.parse import quote

import requests

REPO_ROOT = Path(__file__).resolve().parents[2]
OUT_CSV = Path(__file__).parent / "mode1_candidates.csv"
OUT_MD = Path(__file__).parent / "MODE1_CANDIDATES.md"
CLONED_DIR = REPO_ROOT / "cloned_repos"

# Mode #1 patterns. Each entry is (search_term, friendly_label, family).
# Patterns are quoted so GitHub treats them as literal strings.
PATTERNS: list[tuple[str, str, str]] = [
    # ILogger extension methods (Microsoft.Extensions.Logging)
    ('"LogInformation("', "LogInformation", "ILogger"),
    ('"LogWarning("', "LogWarning", "ILogger"),
    ('"LogError("', "LogError", "ILogger"),
    ('"LogDebug("', "LogDebug", "ILogger"),
    ('"LogCritical("', "LogCritical", "ILogger"),
    ('"LogTrace("', "LogTrace", "ILogger"),
    # HttpClient non-virtual instance methods
    ('"HttpClient" "GetAsync("', "HttpClient.GetAsync", "HttpClient"),
    ('"HttpClient" "PostAsync("', "HttpClient.PostAsync", "HttpClient"),
    ('"HttpClient" "SendAsync("', "HttpClient.SendAsync", "HttpClient"),
    # IServiceProvider extension methods
    ('"GetRequiredService<"', "GetRequiredService<T>", "ServiceProvider"),
]

API_BASE = "https://api.github.com"
SLEEP_BETWEEN_QUERIES = 2.5  # seconds — stay well under 30 req/min
MAX_BACKOFF = 90  # seconds


@dataclass
class RepoStats:
    full_name: str
    stars: int = 0
    is_fork: bool = False
    has_test_dir: bool = False
    pattern_counts: dict[str, int] = field(default_factory=dict)
    family_counts: dict[str, int] = field(default_factory=dict)
    total_hits: int = 0
    seeded: bool = False  # already in cloned_repos/

    def add_count(self, pattern_label: str, family: str, count: int) -> None:
        self.pattern_counts[pattern_label] = count
        self.family_counts[family] = self.family_counts.get(family, 0) + count
        self.total_hits = sum(self.pattern_counts.values())


def gh_get(session: requests.Session, url: str) -> dict | None:
    """GET with backoff on 403 (rate limit) and 422 (query too complex)."""
    backoff = 5
    while True:
        resp = session.get(url, timeout=30)
        if resp.status_code == 200:
            return resp.json()
        if resp.status_code == 403:
            # Rate limited — back off and retry
            print(f"   [rate-limit] waiting {backoff}s...", file=sys.stderr)
            time.sleep(backoff)
            backoff = min(backoff * 2, MAX_BACKOFF)
            continue
        if resp.status_code == 422:
            # Query too complex / unprocessable — treat as zero
            return None
        if resp.status_code in (404, 451):
            return None
        # Anything else is a hard fail
        print(
            f"   [error] {resp.status_code} on {url}: "
            f"{resp.text[:200]}",
            file=sys.stderr,
        )
        return None


def fetch_candidate_repos(
    session: requests.Session,
    min_stars: int,
    max_repos: int,
) -> list[RepoStats]:
    """Pull popular C# repos via the repository search API."""
    candidates: list[RepoStats] = []
    seen: set[str] = set()
    per_page = 100
    page = 1
    print(f"🔍 Fetching popular C# repos (stars > {min_stars})...")
    while len(candidates) < max_repos:
        query = f"language:C# stars:>{min_stars} archived:false"
        url = (
            f"{API_BASE}/search/repositories"
            f"?q={quote(query)}&sort=stars&order=desc"
            f"&per_page={per_page}&page={page}"
        )
        data = gh_get(session, url)
        if not data or not data.get("items"):
            break
        for item in data["items"]:
            full = item["full_name"]
            if full in seen:
                continue
            seen.add(full)
            candidates.append(
                RepoStats(
                    full_name=full,
                    stars=item.get("stargazers_count", 0),
                    is_fork=item.get("fork", False),
                )
            )
            if len(candidates) >= max_repos:
                break
        page += 1
        if page > 10:  # search API caps at 1000 results
            break
        time.sleep(SLEEP_BETWEEN_QUERIES)
    print(f"   found {len(candidates)} candidates")
    return candidates


def seed_from_cloned_repos(candidates: list[RepoStats]) -> list[RepoStats]:
    """Ensure repos in cloned_repos/ appear in the ranking even if they
    didn't make the popular-repos cut."""
    if not CLONED_DIR.exists():
        return candidates
    existing = {c.full_name.split("/", 1)[1].lower() for c in candidates}
    for child in sorted(CLONED_DIR.iterdir()):
        if not child.is_dir() or child.name.startswith("."):
            continue
        name_lower = child.name.lower()
        if name_lower in existing:
            for c in candidates:
                if c.full_name.split("/", 1)[1].lower() == name_lower:
                    c.seeded = True
                    break
            continue
        # Not in candidate set — add as seeded with unknown owner
        candidates.append(
            RepoStats(full_name=f"local/{child.name}", seeded=True)
        )
    return candidates


def query_pattern_count(
    session: requests.Session,
    repo: str,
    pattern: str,
) -> int:
    """Return total_count for `pattern` restricted to `repo`."""
    if "/" not in repo:
        # local-only seeds can't be queried via API
        return 0
    query = f"{pattern} repo:{repo} language:C#"
    url = f"{API_BASE}/search/code?q={quote(query)}&per_page=1"
    data = gh_get(session, url)
    return data.get("total_count", 0) if data else 0


def measure_repo(
    session: requests.Session,
    repo: RepoStats,
) -> RepoStats:
    """Query each Mode #1 pattern for one repo."""
    for pattern, label, family in PATTERNS:
        count = query_pattern_count(session, repo.full_name, pattern)
        repo.add_count(label, family, count)
        time.sleep(SLEEP_BETWEEN_QUERIES)
    return repo


def write_outputs(repos: list[RepoStats], top_n: int) -> None:
    ranked = sorted(
        [r for r in repos if r.total_hits > 0 or r.seeded],
        key=lambda r: (r.total_hits, r.stars),
        reverse=True,
    )

    # CSV — full data
    pattern_labels = [label for _, label, _ in PATTERNS]
    OUT_CSV.parent.mkdir(parents=True, exist_ok=True)
    with OUT_CSV.open("w", newline="") as fh:
        w = csv.writer(fh)
        w.writerow(
            ["repo", "stars", "seeded", "total_hits", "ilogger_total",
             "httpclient_total", "serviceprovider_total"]
            + pattern_labels
        )
        for r in ranked:
            w.writerow(
                [
                    r.full_name,
                    r.stars,
                    "yes" if r.seeded else "no",
                    r.total_hits,
                    r.family_counts.get("ILogger", 0),
                    r.family_counts.get("HttpClient", 0),
                    r.family_counts.get("ServiceProvider", 0),
                ]
                + [r.pattern_counts.get(label, 0) for label in pattern_labels]
            )

    # Markdown — top N summary
    lines: list[str] = []
    lines.append("# Mode #1 candidate repos")
    lines.append("")
    lines.append(
        "Repos ranked by raw count of Mode #1 mockability-failure patterns "
        "(`ILogger` extension methods, `HttpClient` non-virtual instance "
        "methods, `IServiceProvider.GetRequiredService<T>`)."
    )
    lines.append("")
    lines.append(
        "Counts come from GitHub Code Search "
        "(`total_count` per pattern, restricted to `language:C#`)."
    )
    lines.append("")
    lines.append(f"_{len(ranked)} repos with at least one Mode #1 hit. "
                 f"Top {min(top_n, len(ranked))} shown._")
    lines.append("")
    lines.append("| Rank | Repo | Stars | Total | ILogger | HttpClient | ServiceProvider | Already cloned |")
    lines.append("|---:|---|---:|---:|---:|---:|---:|:---:|")
    for i, r in enumerate(ranked[:top_n], 1):
        seeded_marker = "✅" if r.seeded else ""
        lines.append(
            f"| {i} | [{r.full_name}](https://github.com/{r.full_name}) "
            f"| {r.stars} | {r.total_hits} "
            f"| {r.family_counts.get('ILogger', 0)} "
            f"| {r.family_counts.get('HttpClient', 0)} "
            f"| {r.family_counts.get('ServiceProvider', 0)} "
            f"| {seeded_marker} |"
        )
    lines.append("")
    lines.append("## Patterns measured")
    lines.append("")
    for _, label, family in PATTERNS:
        lines.append(f"- **{family}** — `{label}`")
    OUT_MD.write_text("\n".join(lines) + "\n")
    print(f"\n✅ Wrote {OUT_CSV.relative_to(REPO_ROOT)}")
    print(f"✅ Wrote {OUT_MD.relative_to(REPO_ROOT)}")
    print(f"\nTop {min(top_n, len(ranked))} repos by Mode #1 hits:")
    for i, r in enumerate(ranked[:top_n], 1):
        marker = " (seeded)" if r.seeded else ""
        print(
            f"  {i:2d}. {r.full_name:<45s} "
            f"hits={r.total_hits:>6d}  stars={r.stars:>6d}{marker}"
        )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--min-stars", type=int, default=1000)
    parser.add_argument("--max-repos", type=int, default=150)
    parser.add_argument("--top", type=int, default=25)
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Fetch candidate list only; skip the per-repo pattern queries.",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Measure only the first N candidates (0 = all). For testing.",
    )
    args = parser.parse_args()

    token = os.getenv("GITHUB_TOKEN")
    if not token:
        print("error: GITHUB_TOKEN env var required", file=sys.stderr)
        return 2

    session = requests.Session()
    session.headers.update(
        {
            "Authorization": f"Bearer {token}",
            "Accept": "application/vnd.github+json",
            "X-GitHub-Api-Version": "2022-11-28",
        }
    )

    candidates = fetch_candidate_repos(session, args.min_stars, args.max_repos)
    candidates = seed_from_cloned_repos(candidates)
    print(f"📋 {len(candidates)} total candidates "
          f"(including {sum(1 for c in candidates if c.seeded)} seeded)")

    if args.dry_run:
        for c in candidates:
            print(f"  - {c.full_name} (★{c.stars})")
        return 0

    to_measure = candidates if args.limit == 0 else candidates[: args.limit]
    n_queries = len(to_measure) * len(PATTERNS)
    est_minutes = n_queries * SLEEP_BETWEEN_QUERIES / 60
    print(
        f"🔬 Measuring {len(to_measure)} repos × {len(PATTERNS)} patterns "
        f"= {n_queries} queries (~{est_minutes:.0f} min wall clock)"
    )

    for i, repo in enumerate(to_measure, 1):
        print(f"[{i}/{len(to_measure)}] {repo.full_name} (★{repo.stars})")
        measure_repo(session, repo)
        if repo.total_hits > 0:
            fams = ", ".join(
                f"{k}:{v}" for k, v in repo.family_counts.items() if v
            )
            print(f"   → {repo.total_hits} hits ({fams})")

    write_outputs(to_measure, args.top)
    return 0


if __name__ == "__main__":
    sys.exit(main())
