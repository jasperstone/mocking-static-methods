#!/usr/bin/env bash
# test_orchestrator_example.sh
# Example script showing various ways to run and monitor the test orchestrator

echo "╔════════════════════════════════════════════════════════════════╗"
echo "║     Test Orchestrator - Usage Examples                         ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

# Example 1: Basic execution
echo "Example 1: Basic Execution"
echo "═══════════════════════════════════════════════════════════════"
echo "Command:"
echo "  python3 test_orchestrator.py"
echo ""
echo "What it does:"
echo "  • Processes all projects in cloned_repos/"
echo "  • Generates test_metrics.csv with results"
echo "  • Creates logs in test_logs/ directory"
echo "  • Runs until completion or interrupted (Ctrl+C)"
echo ""

# Example 2: With output redirection
echo "Example 2: Running with Output Redirection"
echo "═══════════════════════════════════════════════════════════════"
echo "Command:"
echo "  python3 test_orchestrator.py > orchestrator_output.log 2>&1"
echo ""
echo "What it does:"
echo "  • Redirects all output to orchestrator_output.log"
echo "  • Runs in foreground"
echo "  • Check progress with: tail -f orchestrator_output.log"
echo ""

# Example 3: Background execution with nohup
echo "Example 3: Background Execution (with nohup)"
echo "═══════════════════════════════════════════════════════════════"
echo "Command:"
echo "  nohup python3 test_orchestrator.py > orchestrator_output.log 2>&1 &"
echo ""
echo "What it does:"
echo "  • Runs in background even if terminal closes"
echo "  • Process ID (PID) shown on screen"
echo "  • Check status: tail -f orchestrator_output.log"
echo "  • Kill if needed: kill <PID>"
echo ""

# Example 4: Using screen for session management
echo "Example 4: Using GNU Screen"
echo "═══════════════════════════════════════════════════════════════"
echo "Commands:"
echo "  screen -S orchestrator                   # Create new session"
echo "  python3 test_orchestrator.py             # Run orchestrator"
echo "  Ctrl+A, then D                           # Detach from session"
echo "  screen -r orchestrator                   # Reattach to session"
echo "  screen -ls                               # List sessions"
echo ""
echo "What it does:"
echo "  • Creates persistent terminal session"
echo "  • Can disconnect and reconnect anytime"
echo "  • Useful for long-running processes"
echo ""

# Example 5: Using tmux for session management
echo "Example 5: Using tmux"
echo "═══════════════════════════════════════════════════════════════"
echo "Commands:"
echo "  tmux new-session -d -s orchestrator      # Create new session"
echo "  tmux send-keys -t orchestrator \"python3 test_orchestrator.py\" Enter"
echo "  tmux attach -t orchestrator              # Attach to session"
echo "  tmux ls                                  # List sessions"
echo ""
echo "What it does:"
echo "  • Similar to screen, alternative option"
echo "  • More modern and feature-rich"
echo "  • Popular among developers"
echo ""

# Example 6: Monitoring progress in real-time
echo "Example 6: Real-Time Progress Monitoring"
echo "═══════════════════════════════════════════════════════════════"
echo "Terminal 1 - Start orchestrator:"
echo "  python3 test_orchestrator.py"
echo ""
echo "Terminal 2 - Monitor main log:"
echo "  tail -f test_logs/orchestrator_*.log"
echo ""
echo "Terminal 3 - Watch CSV updates:"
echo "  watch -n 5 'head -5 test_metrics.csv && echo \"---\" && tail -1 test_metrics.csv'"
echo ""

# Example 7: Analyzing results after completion
echo "Example 7: Analyzing Results After Completion"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "View CSV with formatting:"
echo "  column -t -s ',' test_metrics.csv"
echo ""
echo "Count total projects processed:"
echo "  wc -l test_metrics.csv"
echo ""
echo "Find project with most static calls:"
echo "  awk -F ',' 'NR>1 {print \$3, \$1}' test_metrics.csv | sort -rn | head -1"
echo ""
echo "Calculate average coverage improvement:"
echo "  awk -F ',' 'NR>1 {sum+=\$6; count++} END {print \"Average: \" sum/count \"%\"}' test_metrics.csv"
echo ""
echo "List projects with build failures:"
echo "  awk -F ',' '\$8 > 0 {print \$1, \$8 \" failures\"}' test_metrics.csv"
echo ""

# Example 8: Project-specific error inspection
echo "Example 8: Inspecting Project-Specific Errors"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "View build errors for project 'abp':"
echo "  cat test_logs/abp_build_errors.log"
echo ""
echo "View test failures for project 'aspnetcore':"
echo "  cat test_logs/aspnetcore_test_failures.log"
echo ""
echo "Count error lines in a build log:"
echo "  grep -i 'error' test_logs/abp_build_errors.log | wc -l"
echo ""
echo "Find specific error pattern:"
echo "  grep -i 'CS0001' test_logs/*/build_errors.log"
echo ""

# Example 9: Re-running after fixes
echo "Example 9: Re-Running After Implementing Fixes"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "After making code changes:"
echo "  1. Make fixes in cloned_repos/ projects"
echo "  2. Delete old results (optional):"
echo "     rm test_metrics.csv"
echo "     rm -rf test_logs/*"
echo "  3. Run orchestrator again:"
echo "     python3 test_orchestrator.py"
echo "  4. Compare new results with old metrics"
echo ""

# Example 10: Automated CI/CD integration
echo "Example 10: CI/CD Pipeline Integration"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "GitHub Actions example:"
cat << 'EOF'
  - name: Run Test Orchestrator
    run: |
      python3 test_orchestrator.py
    timeout-minutes: 120

  - name: Upload Metrics
    if: always()
    uses: actions/upload-artifact@v2
    with:
      name: test-metrics
      path: |
        test_metrics.csv
        test_logs/
EOF
echo ""

# Example 11: Checking prerequisites before running
echo "Example 11: Pre-flight Checks"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "Verify prerequisites:"
cat << 'EOF'
  # Check Python version
  python3 --version

  # Check .NET SDK
  dotnet --version

  # Check required Python packages
  python3 -c "import dotenv; import requests; print('OK')" || \
    echo "Install: pip install -r requirements.txt"

  # Verify cloned_repos exists
  ls -d cloned_repos/ && echo "Projects found: $(ls cloned_repos/ | wc -l)"
EOF
echo ""

# Example 12: Conditional execution based on project status
echo "Example 12: Selective Project Processing"
echo "═══════════════════════════════════════════════════════════════"
echo ""
echo "Python code to run orchestrator on specific projects:"
cat << 'EOF'
  from test_orchestrator import TestOrchestrator

  orchestrator = TestOrchestrator()
  
  # Get all projects
  projects = orchestrator.get_projects()
  
  # Process only projects with 'core' in name
  for project in projects:
      if 'core' in project.lower():
          print(f"Processing: {project}")
          metrics = orchestrator.process_project(project)
          orchestrator.metrics.append(metrics)
  
  orchestrator.save_metrics_to_csv(orchestrator.metrics)
EOF
echo ""

echo "═══════════════════════════════════════════════════════════════"
echo "For more examples and detailed information, see:"
echo "  • QUICK_START.md"
echo "  • TEST_ORCHESTRATOR_README.md"
echo "  • TEST_ORCHESTRATOR_OVERVIEW.md"
echo "═══════════════════════════════════════════════════════════════"
