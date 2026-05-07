#!/usr/bin/env bash
# list_tests.sh — shared helper for test-discovery.yml jobs.
#
# Provides one function: run_discovery <repo> <filter> <project>...
# For each project, runs `dotnet test --no-build --list-tests` twice
# (with FILTER and without), counts emitted test names, and prints one
# CSV row per project to stdout.
#
# CSV schema:
#   repo,project,tests_universe,tests_in_filter,tests_excluded,build_status
#
# build_status values:
#   ok          — both list-tests calls returned a "tests are available" header
#   <not-built> — --no-build couldn't find a built test assembly
#   <error>     — list-tests failed for another reason (exit non-zero, no header)
#
# Test counting: `dotnet test --list-tests` emits a header
#   "The following Tests are available:"
# followed by one indented test FQN per line, until a blank line / EOF / a
# new "Test run for..." marker. We count lines that begin with whitespace
# AFTER the header. Robust against repos that emit MSBuild prefix noise.

set -uo pipefail

# Count tests in `dotnet test --list-tests` output. Reads stdin, prints int.
_count_listed_tests() {
    awk '
        /^The following Tests are available/ { in_list=1; next }
        in_list {
            if ($0 ~ /^[[:space:]]*$/) { in_list=0; next }
            if ($0 ~ /^[[:space:]]+\S/) { count++ }
        }
        END { print count + 0 }
    '
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

run_discovery() {
    local repo="$1"; shift
    local filter="$1"; shift
    local projects=("$@")

    # CSV header
    echo "repo,project,tests_universe,tests_in_filter,tests_excluded,build_status"

    if [[ ${#projects[@]} -eq 0 ]]; then
        echo "::warning::run_discovery: no projects supplied for $repo" >&2
        return 0
    fi

    echo "::notice::Discovering tests for $repo across ${#projects[@]} projects" >&2

    local i=0
    local proj base universe_out filter_out universe_count filter_count status header_universe header_filter
    for proj in "${projects[@]}"; do
        i=$((i+1))
        base=$(basename "$proj")
        echo "::group::[$i/${#projects[@]}] $base" >&2

        universe_out=$(dotnet test "$proj" --no-build --list-tests --nologo 2>&1 || true)
        filter_out=$(dotnet test "$proj" --no-build --list-tests --filter "$filter" --nologo 2>&1 || true)

        header_universe=0
        header_filter=0
        if grep -q "The following Tests are available" <<< "$universe_out"; then header_universe=1; fi
        if grep -q "The following Tests are available" <<< "$filter_out";   then header_filter=1; fi

        if [[ $header_universe -eq 0 && $header_filter -eq 0 ]]; then
            # Distinguish "not built" from "other error" by sniffing common phrases.
            if grep -qiE "test assembly|could not be found|MSB[0-9]+: error|--no-build" <<< "$universe_out$filter_out"; then
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
