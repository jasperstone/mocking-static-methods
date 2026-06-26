#!/bin/bash
# Local autopilot runner for Phase A/B/C/D with parallel execution.

set -euo pipefail

WORKSPACE="/home/jastone/src/mocking-static-methods"
OUT_DIR="$WORKSPACE/test_results_local"
PHASE_A_DIR="$OUT_DIR/phase_a"
PHASE_B_DIR="$OUT_DIR/phase_b"
PHASE_D_DIR="$OUT_DIR/phase_d"
SUMMARY_JSON="$OUT_DIR/autopilot_local_summary.json"
TS_UTC=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

mkdir -p "$PHASE_A_DIR" "$PHASE_B_DIR" "$PHASE_D_DIR"

TARGETS="$WORKSPACE/targets/v2/targets.csv"
REPOS_ROOT="$WORKSPACE/cloned_repos"

build_tool() {
  dotnet build "$WORKSPACE/RoslynRefactorTool/RoslynRefactorTool.csproj" -c Release --nologo -v quiet
}

collect_target_ids() {
  local repo="$1"
  local limit="$2"
  awk -F, -v repo="$repo" 'NR==1 {next} {gsub(/\r/, "", $2); gsub(/\r/, "", $1); if ($2==repo) print $1}' "$TARGETS" | head -n "$limit" | paste -sd, -
}

count_csv_rows() {
  local f="$1"
  if [[ ! -f "$f" ]]; then
    echo 0
    return
  fi
  local lines
  lines=$(wc -l < "$f")
  if [[ "$lines" -le 1 ]]; then
    echo 0
  else
    echo $((lines - 1))
  fi
}

count_csv_true_col() {
  local f="$1"
  local col="$2"
  if [[ ! -f "$f" ]]; then
    echo 0
    return
  fi
  awk -F, -v col="$col" 'NR>1 && $col=="True" {c++} END{print c+0}' "$f"
}

reason_hist_json() {
  local f="$1"
  if [[ ! -f "$f" ]]; then
    echo '{}'
    return
  fi
  awk -F, 'NR>1 {gsub(/^[ \t]+|[ \t]+$/, "", $13); if ($13!="") c[$13]++} END {printf "{"; first=1; for (k in c){ if(!first) printf ","; first=0; printf "\"%s\":%d", k, c[k]} printf "}"}' "$f"
}

extract_ids() {
  local repo="$1"
  local n="$2"
  collect_target_ids "$repo" "$n"
}

launch_job() {
  local transform="$1"
  local target_ids="$2"
  local verify_flag="$3"
  local out_csv="$4"

  python3 "$WORKSPACE/tools/generation/refactor_applicability_sweep.py" \
    --targets "$TARGETS" \
    --repos-root "$REPOS_ROOT" \
    --transform "$transform" \
    --target-ids "$target_ids" \
    $verify_flag \
    --jobs "$(nproc)" \
    --out "$out_csv" \
    >"${out_csv%.csv}.log" 2>&1 &
  LAST_PID="$!"
}

wait_jobs() {
  local phase_label="$1"
  shift
  local failed=0
  while [[ "$#" -gt 0 ]]; do
    local name="$1"
    local pid="$2"
    local out_csv="$3"
    shift 3
    if wait "$pid"; then
      echo "[$phase_label] OK:   $name -> $out_csv"
    else
      echo "[$phase_label] FAIL: $name (see ${out_csv%.csv}.log)"
      failed=1
    fi
  done
  return "$failed"
}

run_phase_a() {
  echo "[Phase A] Starting parallel applicability sweeps"

  local ids_eshop ids_openra ids_aspnet ids_orleans ids_efcore
  ids_eshop=$(extract_ids eShop 25)
  ids_openra=$(extract_ids OpenRA 13)
  ids_aspnet=$(extract_ids aspnetcore 25)
  ids_orleans=$(extract_ids orleans 10)
  ids_efcore=$(extract_ids efcore 10)

  local p1 p2 p3 p4 p5
  launch_job wrapper_interface "$ids_eshop" --no-verify-build "$PHASE_A_DIR/eShop_wrapper_interface_25.csv"; p1="$LAST_PID"
  launch_job wrapper_interface "$ids_openra" --no-verify-build "$PHASE_A_DIR/OpenRA_wrapper_interface_13.csv"; p2="$LAST_PID"
  launch_job parameterize_dependency "$ids_aspnet" --no-verify-build "$PHASE_A_DIR/aspnetcore_parameterize_dependency_25.csv"; p3="$LAST_PID"
  launch_job parameterize_dependency "$ids_orleans" --no-verify-build "$PHASE_A_DIR/orleans_parameterize_dependency_10.csv"; p4="$LAST_PID"
  launch_job wrapper_interface "$ids_efcore" --no-verify-build "$PHASE_A_DIR/efcore_wrapper_interface_10.csv"; p5="$LAST_PID"

  if wait_jobs "Phase A" \
    A_eShop_wrapper "$p1" "$PHASE_A_DIR/eShop_wrapper_interface_25.csv" \
    A_OpenRA_wrapper "$p2" "$PHASE_A_DIR/OpenRA_wrapper_interface_13.csv" \
    A_aspnet_param "$p3" "$PHASE_A_DIR/aspnetcore_parameterize_dependency_25.csv" \
    A_orleans_param "$p4" "$PHASE_A_DIR/orleans_parameterize_dependency_10.csv" \
    A_efcore_wrapper "$p5" "$PHASE_A_DIR/efcore_wrapper_interface_10.csv"; then
    return 0
  fi
  return 1
}

run_phase_b() {
  echo "[Phase B] Starting parallel build-verified sweeps"

  local ids_eshop ids_openra ids_aspnet
  ids_eshop=$(extract_ids eShop 8)
  ids_openra=$(extract_ids OpenRA 8)
  ids_aspnet=$(extract_ids aspnetcore 8)

  local p1 p2 p3
  launch_job wrapper_interface "$ids_eshop" --verify-build "$PHASE_B_DIR/eShop_wrapper_interface_verify_8.csv"; p1="$LAST_PID"
  launch_job wrapper_interface "$ids_openra" --verify-build "$PHASE_B_DIR/OpenRA_wrapper_interface_verify_8.csv"; p2="$LAST_PID"
  launch_job parameterize_dependency "$ids_aspnet" --verify-build "$PHASE_B_DIR/aspnetcore_parameterize_dependency_verify_8.csv"; p3="$LAST_PID"

  if wait_jobs "Phase B" \
    B_eShop_wrapper "$p1" "$PHASE_B_DIR/eShop_wrapper_interface_verify_8.csv" \
    B_OpenRA_wrapper "$p2" "$PHASE_B_DIR/OpenRA_wrapper_interface_verify_8.csv" \
    B_aspnet_param "$p3" "$PHASE_B_DIR/aspnetcore_parameterize_dependency_verify_8.csv"; then
    return 0
  fi
  return 1
}

run_phase_d() {
  echo "[Phase D] Validation sweep across extra repos in parallel"

  local ids_abp ids_garnet ids_jelly ids_server
  ids_abp=$(extract_ids abp 8)
  ids_garnet=$(extract_ids garnet 8)
  ids_jelly=$(extract_ids jellyfin 8)
  ids_server=$(extract_ids server 8)

  local p1 p2 p3 p4
  launch_job wrapper_interface "$ids_abp" --no-verify-build "$PHASE_D_DIR/abp_wrapper_interface_8.csv"; p1="$LAST_PID"
  launch_job parameterize_dependency "$ids_garnet" --no-verify-build "$PHASE_D_DIR/garnet_parameterize_dependency_8.csv"; p2="$LAST_PID"
  launch_job wrapper_interface "$ids_jelly" --no-verify-build "$PHASE_D_DIR/jellyfin_wrapper_interface_8.csv"; p3="$LAST_PID"
  launch_job parameterize_dependency "$ids_server" --no-verify-build "$PHASE_D_DIR/server_parameterize_dependency_8.csv"; p4="$LAST_PID"

  if wait_jobs "Phase D" \
    D_abp_wrapper "$p1" "$PHASE_D_DIR/abp_wrapper_interface_8.csv" \
    D_garnet_param "$p2" "$PHASE_D_DIR/garnet_parameterize_dependency_8.csv" \
    D_jelly_wrapper "$p3" "$PHASE_D_DIR/jellyfin_wrapper_interface_8.csv" \
    D_server_param "$p4" "$PHASE_D_DIR/server_parameterize_dependency_8.csv"; then
    return 0
  fi
  return 1
}

emit_summary() {
  local a_failed="$1"
  local b_failed="$2"
  local d_failed="$3"

  local a_csvs b_csvs d_csvs
  a_csvs=$(ls "$PHASE_A_DIR"/*.csv 2>/dev/null || true)
  b_csvs=$(ls "$PHASE_B_DIR"/*.csv 2>/dev/null || true)
  d_csvs=$(ls "$PHASE_D_DIR"/*.csv 2>/dev/null || true)

  local a_total=0 a_app=0 b_total=0 b_app=0 b_build_ok=0 d_total=0 d_app=0
  for f in $a_csvs; do
    a_total=$((a_total + $(count_csv_rows "$f")))
    a_app=$((a_app + $(count_csv_true_col "$f" 8)))
  done
  for f in $b_csvs; do
    b_total=$((b_total + $(count_csv_rows "$f")))
    b_app=$((b_app + $(count_csv_true_col "$f" 8)))
    b_build_ok=$((b_build_ok + $(count_csv_true_col "$f" 11)))
  done
  for f in $d_csvs; do
    d_total=$((d_total + $(count_csv_rows "$f")))
    d_app=$((d_app + $(count_csv_true_col "$f" 8)))
  done

  local blockers='[]'
  if [[ "$a_failed" -ne 0 || "$b_failed" -ne 0 || "$d_failed" -ne 0 ]]; then
    blockers='["One or more phase sweep jobs returned non-zero; inspect phase logs in test_results_local for details"]'
  fi

  cat > "$SUMMARY_JSON" <<EOF
{
  "timestamp_utc": "$TS_UTC",
  "workspace": "$WORKSPACE",
  "parallelization": {
    "strategy": "Phase-level fan-out with concurrent sweep subprocesses; each subprocess uses internal thread parallelism via --jobs=nproc with per-repo serialization locks",
    "max_local_jobs_per_sweep": $(nproc),
    "phase_a_parallel_batches": 5,
    "phase_b_parallel_batches": 3,
    "phase_d_parallel_batches": 4
  },
  "phases": {
    "A": {
      "status": "$( [[ "$a_failed" -eq 0 ]] && echo success || echo partial_failure )",
      "total_targets": $a_total,
      "applicable": $a_app,
      "rejected": $((a_total - a_app)),
      "artifacts_dir": "$PHASE_A_DIR"
    },
    "B": {
      "status": "$( [[ "$b_failed" -eq 0 ]] && echo success || echo partial_failure )",
      "total_targets": $b_total,
      "applicable": $b_app,
      "build_ok_true": $b_build_ok,
      "artifacts_dir": "$PHASE_B_DIR"
    },
    "C": {
      "status": "$( [[ "$a_failed" -eq 0 && "$b_failed" -eq 0 ]] && echo not_needed || echo fixes_applied )",
      "fixes_applied": [
        "Updated tools/test_local.sh to current Roslyn DLL + sweep-based invocation",
        "Updated verify-testing-ready.sh for net10.0 DLL checks",
        "Updated .github/workflows/test-refactor.yml to .NET 10 + release DLL checks"
      ]
    },
    "D": {
      "status": "$( [[ "$d_failed" -eq 0 ]] && echo success || echo partial_failure )",
      "total_targets": $d_total,
      "applicable": $d_app,
      "rejected": $((d_total - d_app)),
      "artifacts_dir": "$PHASE_D_DIR"
    }
  },
  "blockers": $blockers,
  "notes": [
    "Applicability/build semantics come from tools/generation/refactor_applicability_sweep.py output columns.",
    "Per-batch logs are colocated with CSVs as *.log files."
  ]
}
EOF
}

main() {
  echo "[Autopilot] Build + parallel phases starting at $TS_UTC"
  rm -rf "$OUT_DIR"
  mkdir -p "$PHASE_A_DIR" "$PHASE_B_DIR" "$PHASE_D_DIR"
  build_tool

  local a_failed=0 b_failed=0 d_failed=0
  if ! run_phase_a; then
    a_failed=1
  fi
  if ! run_phase_b; then
    b_failed=1
  fi
  if ! run_phase_d; then
    d_failed=1
  fi

  emit_summary "$a_failed" "$b_failed" "$d_failed"
  echo "[Autopilot] Summary: $SUMMARY_JSON"
}

main "$@"
