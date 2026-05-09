#!/bin/bash
# =============================================================================
# Run GitHub Actions Jobs Locally - Helper Script
# =============================================================================
# This script helps you run individual jobs from your GitHub Actions workflow
# locally using 'act' (https://github.com/nektos/act)
#
# Usage:
#   ./run-job-locally.sh <job-name> [options]
#
# Examples:
#   ./run-job-locally.sh abp
#   ./run-job-locally.sh aspnetcore --dry-run
#   ./run-job-locally.sh list  # list all jobs
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

WORKFLOW_FILE=".github/workflows/coverage-orchestrator.yml"

# Check if act is installed
if ! command -v act &> /dev/null; then
    echo -e "${RED}Error: 'act' is not installed.${NC}"
    echo ""
    echo "Install with:"
    echo "  curl -s https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash"
    echo ""
    echo "Or use the GitHub Actions Runner dev container:"
    echo "  Open VS Code Command Palette (Ctrl+Shift+P)"
    echo "  Select: Dev Containers: Rebuild and Reopen in Container"
    echo "  Choose the 'github-runner' configuration"
    exit 1
fi

# Function to list all jobs
list_jobs() {
    echo -e "${BLUE}Available jobs in workflow:${NC}"
    act -l --workflows "$WORKFLOW_FILE"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 <job-name> [options]"
    echo ""
    echo "Commands:"
    echo "  list              List all available jobs"
    echo "  <job-name>        Run a specific job (e.g., abp, aspnetcore, efcore)"
    echo ""
    echo "Options:"
    echo "  --dry-run         Show what would run without executing"
    echo "  --verbose         Show verbose output"
    echo "  --full-image      Use complete Ubuntu image (slower but more compatible)"
    echo ""
    echo "Examples:"
    echo "  $0 list"
    echo "  $0 abp"
    echo "  $0 aspnetcore --verbose"
    echo "  $0 efcore --dry-run"
}

# Parse arguments
if [ $# -eq 0 ]; then
    show_usage
    echo ""
    list_jobs
    exit 0
fi

JOB_NAME="$1"
shift

# Handle special commands
if [ "$JOB_NAME" = "list" ]; then
    list_jobs
    exit 0
fi

if [ "$JOB_NAME" = "help" ] || [ "$JOB_NAME" = "-h" ] || [ "$JOB_NAME" = "--help" ]; then
    show_usage
    exit 0
fi

# Build act command
ACT_CMD="act workflow_dispatch -j $JOB_NAME --workflows $WORKFLOW_FILE --input repo=$JOB_NAME"
DRY_RUN=false
VERBOSE=false
USE_FULL_IMAGE=false

# Parse options
while [ $# -gt 0 ]; do
    case "$1" in
        --dry-run)
            DRY_RUN=true
            ACT_CMD="$ACT_CMD -n"
            shift
            ;;
        --verbose)
            VERBOSE=true
            ACT_CMD="$ACT_CMD -v"
            shift
            ;;
        --full-image)
            USE_FULL_IMAGE=true
            ACT_CMD="$ACT_CMD -P ubuntu-latest=catthehacker/ubuntu:full-latest"
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            show_usage
            exit 1
            ;;
    esac
done

echo -e "${GREEN}Running job: ${YELLOW}$JOB_NAME${NC}"
echo -e "${BLUE}Command: $ACT_CMD${NC}"
echo ""

if [ "$DRY_RUN" = true ]; then
    echo -e "${YELLOW}(Dry run - not actually executing)${NC}"
fi

if [ "$USE_FULL_IMAGE" = true ]; then
    echo -e "${YELLOW}Note: First run with --full-image will take a while to download (>10GB)${NC}"
fi

echo ""
echo "Press Ctrl+C to cancel, or Enter to continue..."
read -r

# Execute act command
eval "$ACT_CMD"

echo ""
echo -e "${GREEN}✅ Job execution complete!${NC}"
