#!/usr/bin/env bash
# list_tests.sh — shared helper for test-discovery.yml jobs.
#
# Provides one function: run_discovery <repo> <filter> <project>...
# For each project, runs `dotnet test --no-build --list-tests` twice
# (with FILTER and without), counts emitted test names via several
# heuristics (max wins), and prints one CSV row per project to stdout.
#
# CSV schema (unchanged):
#   repo,project,tests_universe,tests_in_filter,tests_excluded,build_status
#
# build_status values:
#   ok          — list-tests produced a recognisable test enumeration
#                 OR a recognisable empty-discovery summary
#   <not-built> — --no-build couldn't find a built test assembly
#   <error>     — list-tests failed for another reason
#
# 2026-05-07 — Vogel: rewritten after Run 25490696770 showed nearly every
# project reporting "universe=0 status=ok". The legacy counter only matched
# indented FQN lines under the VSTest header "The following Tests are
# available:". With the .NET 10 SDK / new dotnet test pipeline, several
# other shapes appear: Microsoft.Testing.Platform "Test Name:" prefixes,
# xunit.v3 enumeration, and projects whose adapter prints a `Total tests:`
# summary instead of a per-line listing. We now:
#   1. dump raw stdout/stderr per project under $LIST_TESTS_RAW_DIR
#      (defaults to ./_discovery_raw) so failures are post-mortem-able
#   2. run the dotnet command with `-v normal` so MSBuild's default
#      "minimal" logger doesn't strip enumerated tests
#   3. count tests via THREE heuristics and take the max:
#        (a) indented FQN lines under the VSTest header
#        (b) "Test Name:" prefixed lines (vstest direct / MTP)
#        (c) "Total tests: N" summary value
#   4. recognise extra empty-discovery markers ("No test is available",
#      "Found 0 tests") so we don't mis-classify them as <error>.

set -uo pipefail

LIST_TESTS_RAW_DIR="${LIST_TESTS_RAW_DIR:-./_discovery_raw}"

# --- counting heuristics -----------------------------------------------------

# (a) Indented FQN lines emitted under the VSTest header
#     "The following Tests are available:" — terminated by a blank line or
#     an MSBuild "Test run for ..." marker. Reads stdin, prints int.
_count_vstest_indented() {
    awk '
        /^The following Tests are available/ { in_list=1; next }
        in_list {
            if ($0 ~ /^[[:space:]]*$/) { in_list=0; next }
            if ($0 ~ /^Test run for/)  { in_list=0; next }
            if ($0 ~ /^[[:space:]]+\S/) { count++ }
        }
        END { print count + 0 }
    '
}

# (b) "Test Name:" prefixed lines (vstest --ListTests direct mode and some
#     Microsoft.Testing.Platform shapes). Reads stdin, prints int.
_count_test_name_prefixed() {
    grep -cE '^[[:space:]]*Test Name:' || true
}

# (c) Largest "Total tests: N" / "Total: N" summary value the run printed.
#     xunit.v3 / MTP frequently print this as part of discovery output.
#     Reads stdin, prints int. Uses grep+sed for mawk/gawk portability —
#     the SDK container is Ubuntu Noble with mawk, which lacks gawk's
#     3-argument match().
_count_total_summary() {
    local input
    input=$(cat)
    {
        printf '%s\n' "$input" | grep -oE 'Total tests:[[:space:]]*[0-9]+' \
            | sed -E 's/.*[[:space:]]([0-9]+)$/\1/'
        printf '%s\n' "$input" | grep -oE '^[[:space:]]*Total:[[:space:]]*[0-9]+' \
            | sed -E 's/.*[[:space:]]([0-9]+)$/\1/'
    } | awk 'BEGIN{best=0} { if ($1+0 > best) best=$1+0 } END { print best }'
}

# Combined counter — max of the three heuristics.
_count_listed_tests() {
    local input
    input=$(cat)
    local a b c best
    a=$(printf '%s\n' "$input" | _count_vstest_indented)
    b=$(printf '%s\n' "$input" | _count_test_name_prefixed)
    c=$(printf '%s\n' "$input" | _count_total_summary)
    best=$a
    [[ $b -gt $best ]] && best=$b
    [[ $c -gt $best ]] && best=$c
    printf '%s' "$best"
}

# Did the output show *any* recognisable discovery activity? Used to decide
# between status="ok" (discovery ran, possibly with 0 results) vs <error>.
_discovery_ran() {
    grep -qE \
        -e 'The following Tests are available' \
        -e '^[[:space:]]*Test Name:' \
        -e 'Total tests:[[:space:]]*[0-9]+' \
        -e '^[[:space:]]*Total:[[:space:]]*[0-9]+' \
        -e 'No test is available' \
        -e 'No test source files were specified' \
        -e 'Found [0-9]+ test'
}

# CSV-quote a field if it contains comma, quote, or newline.
_csv_quote() {
    local v="$1"
    if [[ "$v" == *,* || "$v" == *\"* || "$v" == *$'\n'* ]]; then
        v="${v//\"/\"\"}"
        printf '"%s"' "$v"
    else
        printf '%s' "$v"
    fi
}

# Sanitise a path into a flat filename for the raw-log dir.
_slug() {
    local s="$1"
    s="${s//\//__}"
    s="${s// /_}"
    printf '%s' "$s"
}

run_discovery() {
    local repo="$1"; shift
    local filter="$1"; shift
    local projects=("$@")

    mkdir -p "$LIST_TESTS_RAW_DIR"

    # CSV header
    echo "repo,project,tests_universe,tests_in_filter,tests_excluded,build_status"

    if [[ ${#projects[@]} -eq 0 ]]; then
        echo "::warning::run_discovery: no projects supplied for $repo" >&2
        return 0
    fi

    echo "::notice::Discovering tests for $repo across ${#projects[@]} projects" >&2
    echo "::notice::Raw per-project logs: $LIST_TESTS_RAW_DIR" >&2

    local i=0
    local proj base slug universe_log filter_log
    local universe_out filter_out universe_count filter_count status
    for proj in "${projects[@]}"; do
        i=$((i+1))
        base=$(basename "$proj")
        slug=$(_slug "$proj")
        universe_log="$LIST_TESTS_RAW_DIR/${slug}.universe.log"
        filter_log="$LIST_TESTS_RAW_DIR/${slug}.filter.log"
        echo "::group::[$i/${#projects[@]}] $base" >&2

        # `-v normal` keeps MSBuild from suppressing enumerated test lines on
        # the default "minimal" logger. Capture stdout+stderr to a per-project
        # file (preserved as workflow artifact) and into a variable for awk.
        dotnet test "$proj" --no-build --list-tests --nologo -v normal \
            > "$universe_log" 2>&1 || true
        universe_out=$(cat "$universe_log")

        dotnet test "$proj" --no-build --list-tests --filter "$filter" --nologo -v normal \
            > "$filter_log" 2>&1 || true
        filter_out=$(cat "$filter_log")

        local ran_universe=0 ran_filter=0
        _discovery_ran <<< "$universe_out" && ran_universe=1
        _discovery_ran <<< "$filter_out"   && ran_filter=1

        if [[ $ran_universe -eq 0 && $ran_filter -eq 0 ]]; then
            if grep -qiE "test assembly|could not be found|MSB[0-9]+: error|--no-build" \
                <<< "$universe_out$filter_out"; then
                status="<not-built>"
            else
                status="<error>"
            fi
            universe_count=0
            filter_count=0
        else
            status="ok"
            universe_count=$(_count_listed_tests <<< "$universe_out")
            filter_count=$(_count_listed_tests <<< "$filter_out")
        fi

        local excluded=$(( universe_count - filter_count ))
        printf '%s,%s,%s,%s,%s,%s\n' \
            "$(_csv_quote "$repo")" \
            "$(_csv_quote "$proj")" \
            "$universe_count" \
            "$filter_count" \
            "$excluded" \
            "$(_csv_quote "$status")"

        echo "  -> universe=$universe_count filter=$filter_count status=$status" >&2
        echo "::endgroup::" >&2
    done
}
