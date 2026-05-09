#!/usr/bin/env python3
"""Build unified per-repo table:
  - test count (scraped from job logs; see _TEST_COUNTS)
  - Mode #1 static-call sites by family (from Mode1Analyzer/results/mode1_sites.csv)
  - Mode #1 sites covered (cobertura cross-reference)
  - line coverage % (from cobertura XML root attrs)

Outputs:
  - tools/coverage_xref/unified_table.csv
  - tools/coverage_xref/UNIFIED_TABLE.md
"""
from __future__ import annotations
import csv
import re
import xml.etree.ElementTree as ET
from collections import defaultdict, Counter
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
SITES_CSV = REPO_ROOT / "Mode1Analyzer" / "results" / "mode1_sites.csv"
COV_XML_BASE = Path("/tmp/cov_phase2")
COV_RAW_BASE = Path("/tmp/cov_raw")
OUT_CSV = REPO_ROOT / "tools" / "coverage_xref" / "unified_table.csv"
OUT_MD = REPO_ROOT / "tools" / "coverage_xref" / "UNIFIED_TABLE.md"

REPOS = [
    "abp", "aspnetcore", "Avalonia", "duplicati", "efcore", "eShop", "garnet",
    "jellyfin", "OpenRA", "orleans", "roslyn", "runtime", "semantic-kernel",
    "server", "StockSharp",
]

FAMILY_MAP = {
    "Microsoft.Extensions.Logging.LoggerExtensions": "ILogger",
    "Microsoft.Extensions.Configuration.ConfigurationBinder": "IConfiguration",
    "Microsoft.Extensions.Configuration.ConfigurationExtensions": "IConfiguration",
    "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions": "IServiceProvider",
    "System.Net.Http.HttpClient": "HttpClient",
    "System.Net.Http.HttpMessageInvoker": "HttpClient",
}

# Path segments that mark a site as non-production (test, benchmark, sample,
# playground). Sites under these paths are excluded from Mode #1 totals so
# the baseline reflects only production code call-sites worth covering.
NON_PROD_SEGMENT = re.compile(
    r"(^|/)("
    r"test|tests|UnitTest|UnitTests|IntegrationTests|FunctionalTests|"
    r"EndToEndTests|E2ETests|TestProjects|TestUtilities|TestHelpers|"
    r"Testing|TestingHost|TestInfrastructure|"
    r"Benchmarks|benchmarks|"
    r"Samples|samples|Examples|examples|Sample|sample|"
    r"playground|Sandbox|sandbox"
    r")/",
    re.IGNORECASE,
)


def is_production_site(file_path: str) -> bool:
    return NON_PROD_SEGMENT.search("/" + file_path) is None


def cobertura_files(repo: str) -> list[Path]:
    d = COV_XML_BASE / f"coverage-xml-{repo}"
    return sorted(d.rglob("coverage.cobertura.xml"))


def line_coverage(repo: str) -> tuple[int, int]:
    """Per-source-file line coverage aggregation.

    Each per-csproj cobertura file from coverlet contains an entry for *every*
    instrumented source file the test process loaded — not just files owned by
    the test project. When a repo runs N test projects (server: 12, jellyfin:
    16, semantic-kernel: 43), summing root `lines-valid` across all cobertura
    files double-counts each shared production source N times, while
    `lines-covered` only reflects whichever test actually exercised it. The
    result is a synthetically deflated `covered/valid` ratio (server: 1.7%).

    Correct aggregation: build a per-file map of (file, line) → max hits across
    all cobertura files, then sum unique lines-valid and lines-covered once.

    Note: cobertura puts the same <line> element under both
    <class>/<lines>/<line> and <class>/<methods>/<method>/<lines>/<line>.
    We iterate only the class-level <lines> child to avoid double counting.
    """
    # filename → {line_number: max_hits}
    file_lines: dict[str, dict[int, int]] = defaultdict(dict)
    for f in cobertura_files(repo):
        try:
            root = ET.parse(f).getroot()
        except ET.ParseError:
            continue
        for cls in root.iter("class"):
            fname = cls.get("filename", "")
            if not fname:
                continue
            key = fname.replace("\\", "/").lstrip("/").lower()
            line_map = file_lines[key]
            # Only direct class-level <lines>/<line>, NOT method-level lines.
            class_lines = cls.find("lines")
            if class_lines is None:
                continue
            for line in class_lines.findall("line"):
                try:
                    num = int(line.get("number", "0"))
                    hits = int(line.get("hits", "0"))
                except ValueError:
                    continue
                if num <= 0:
                    continue
                # Always register the line as instrumented; track max hits.
                cur = line_map.get(num, -1)
                if hits > cur:
                    line_map[num] = hits
    valid = covered = 0
    for line_map in file_lines.values():
        valid += len(line_map)
        covered += sum(1 for h in line_map.values() if h > 0)
    return valid, covered


def load_coverage_map(repo: str) -> dict[str, dict[int, int]]:
    coverage: dict[str, dict[int, int]] = defaultdict(dict)
    for f in cobertura_files(repo):
        try:
            root = ET.parse(f).getroot()
        except ET.ParseError:
            continue
        for cls in root.iter("class"):
            fname = cls.get("filename", "")
            if not fname:
                continue
            key = fname.replace("\\", "/").lstrip("/").lower()
            for line in cls.iter("line"):
                num = int(line.get("number", "0"))
                hits = int(line.get("hits", "0"))
                if num <= 0:
                    continue
                cur = coverage[key].get(num, 0)
                coverage[key][num] = max(cur, hits)
    return coverage


def find_site(coverage: dict[str, dict[int, int]], site_file: str, line: int) -> str:
    site_norm = site_file.replace("\\", "/").lstrip("/").lower()
    if site_norm in coverage:
        line_map = coverage[site_norm]
        if line in line_map:
            return "covered" if line_map[line] > 0 else "uncovered"
        return "unknown_line"
    parts = site_norm.split("/")
    for n in (5, 4, 3, 2):
        if len(parts) < n:
            continue
        suffix = "/".join(parts[-n:])
        for cov_path, line_map in coverage.items():
            if cov_path.endswith(suffix):
                if line in line_map:
                    return "covered" if line_map[line] > 0 else "uncovered"
                return "unknown_line"
    return "unknown_file"


_TEST_COUNTS: dict[str, int | None] = {
    # Scraped from job logs. Two patterns:
    #   - `Total: N` (uppercase) — emitted by classic `dotnet test` summary and
    #     by the StockSharp Microsoft.Testing.Platform exe `Passed!` line.
    #   - `total: N` (lowercase) — emitted inside per-assembly summary blocks
    #     when the dotnet-coverage wrapper runs the MTP exe directly
    #     (Avalonia per-csproj loop, .NET runtime targeted XPlat step).
    # eShop's runs crashed under coverlet.console with 0% per-project coverage
    # so no parseable summary exists; left as None.
    "abp": 1358,
    "aspnetcore": 31603,
    "Avalonia": 6860,        # sum of `total: N` across 5 UnitTest assemblies (job 75121694042)
    "duplicati": 1096,
    "efcore": 13724,
    "eShop": None,
    "garnet": 3563,
    "jellyfin": 2740,
    "OpenRA": 473,
    "orleans": 11041,
    "roslyn": 155997,
    "runtime": 6012,         # sum of `Total: N` across 12 libs.tests assemblies (job 75113219551)
    "semantic-kernel": 6263,
    "server": 5118,
    "StockSharp": 4107,      # MTP `Passed!` line for StockSharp.Tests.dll (job 75121921818)
}


def count_tests(repo: str) -> int | None:
    return _TEST_COUNTS.get(repo)


def main():
    # Load sites and group. Skip sites under non-production paths so the
    # baseline reflects only call-sites in shipping code.
    family_counts: dict[str, Counter] = defaultdict(Counter)
    mode1_total: Counter = Counter()
    excluded_non_prod: Counter = Counter()
    sites_by_repo: dict[str, list[dict]] = defaultdict(list)
    with SITES_CSV.open() as fh:
        for s in csv.DictReader(fh):
            repo = s["repo"]
            if not is_production_site(s["file"]):
                excluded_non_prod[repo] += 1
                continue
            ct = s.get("containing_type", "")
            fam = FAMILY_MAP.get(ct, "Other")
            family_counts[repo][fam] += 1
            mode1_total[repo] += 1
            sites_by_repo[repo].append(s)

    rows = []
    for repo in REPOS:
        valid, covered = line_coverage(repo)
        line_pct = (100.0 * covered / valid) if valid else 0.0

        # Mode #1 covered
        cov_map = load_coverage_map(repo)
        mode1_covered = mode1_uncovered = mode1_unknown_line = mode1_unknown_file = 0
        for s in sites_by_repo.get(repo, []):
            status = find_site(cov_map, s["file"], int(s["line"]))
            if status == "covered":
                mode1_covered += 1
            elif status == "uncovered":
                mode1_uncovered += 1
            elif status == "unknown_line":
                mode1_unknown_line += 1
            else:
                mode1_unknown_file += 1

        tests = count_tests(repo)
        fc = family_counts[repo]
        rows.append({
            "repo": repo,
            "tests": tests if tests is not None else "",
            "ilogger": fc["ILogger"],
            "httpclient": fc["HttpClient"],
            "iconfiguration": fc["IConfiguration"],
            "iserviceprovider": fc["IServiceProvider"],
            "other": fc["Other"],
            "mode1_total": mode1_total[repo],
            "mode1_covered": mode1_covered,
            "mode1_uncovered": mode1_uncovered,
            "mode1_unknown_line": mode1_unknown_line,
            "mode1_unknown_file": mode1_unknown_file,
            "lines_valid": valid,
            "lines_covered": covered,
            "line_coverage_pct": f"{line_pct:.2f}",
        })

    # CSV
    with OUT_CSV.open("w", newline="") as fh:
        w = csv.DictWriter(fh, fieldnames=list(rows[0].keys()))
        w.writeheader()
        w.writerows(rows)
    print(f"Wrote {OUT_CSV}")

    # Markdown
    md = ["# Unified per-repo table",
          "",
          "Combines: test count (from job logs) · Mode #1 static-call sites by family · ",
          "Mode #1 sites covered by tests · overall line coverage %.",
          "",
          "Sources:",
          f"- Mode #1 sites: `Mode1Analyzer/results/mode1_sites.csv`",
          f"- Coverage XML: `/tmp/cov_phase2/coverage-xml-*/coverage.cobertura.xml`",
          "- Test counts: scraped from job logs (sum of `Total: N` lines in `dotnet test` summaries; see _TEST_COUNTS in this script)",
          "",
          "## Per-repo breakdown",
          "",
          "| Repo | Tests | ILogger | HttpClient | IConfig | ISvcProv | Other | Mode #1 total | Mode #1 covered | Line cov % |",
          "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|"]
    for r in rows:
        tests = r["tests"] if r["tests"] != "" else "—"
        md.append(
            f"| {r['repo']} | {tests} | {r['ilogger']} | {r['httpclient']} | "
            f"{r['iconfiguration']} | {r['iserviceprovider']} | {r['other']} | "
            f"{r['mode1_total']} | {r['mode1_covered']} | {r['line_coverage_pct']}% |"
        )
    # Totals row
    tot_tests = sum(r["tests"] for r in rows if isinstance(r["tests"], int))
    tot = {k: sum(r[k] for r in rows if isinstance(r[k], int))
           for k in ("ilogger", "httpclient", "iconfiguration",
                     "iserviceprovider", "other", "mode1_total", "mode1_covered",
                     "lines_valid", "lines_covered")}
    overall_pct = (100.0 * tot["lines_covered"] / tot["lines_valid"]) if tot["lines_valid"] else 0.0
    md.append(
        f"| **TOTAL** | **{tot_tests:,}** | **{tot['ilogger']:,}** | **{tot['httpclient']:,}** | "
        f"**{tot['iconfiguration']:,}** | **{tot['iserviceprovider']:,}** | **{tot['other']:,}** | "
        f"**{tot['mode1_total']:,}** | **{tot['mode1_covered']:,}** | **{overall_pct:.2f}%** |"
    )
    md.append("")
    md.append("## Notes")
    md.append("")
    md.append("- **Tests** — sum of test-runner totals from each repo's job log. Three patterns are scraped:  `Total: N` (uppercase) from classic `dotnet test` summaries; `total: N` (lowercase) from per-assembly summary blocks emitted by the dotnet-coverage MTP wrapper (Avalonia per-csproj loop, runtime targeted XPlat step); and the `Passed!` line from the StockSharp Microsoft.Testing.Platform exe. `—` means no parseable summary survives (eShop's coverlet.console crashed both unit suites with 0% per-project coverage in the captured run).")
    md.append("- **Family columns** — Mode #1 sites grouped by the receiver/extension family the analyzer detected.")
    md.append("- **Mode #1 covered** — call sites where the cobertura XML reports `hits > 0` for the source line.")
    md.append("- **Line cov %** — unique `(file, line)` instrumented across all cobertura XMLs for the repo, with max hits taken across files. This dedupes the per-csproj cobertura inflation: each test project's coverlet output enumerates *every* assembly the test process loaded, so naively summing root `lines-valid` would multiply shared production lines by N test projects (16 for jellyfin, 12 for server, 43 for semantic-kernel) while `lines-covered` reflects only one runner. See `line_coverage()` docstring.")
    OUT_MD.write_text("\n".join(md) + "\n")
    print(f"Wrote {OUT_MD}")


if __name__ == "__main__":
    main()
